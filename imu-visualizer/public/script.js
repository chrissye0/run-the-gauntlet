// points on graphs
const MAX_POINTS = 150;

// to contain graph data
const data = {
    accelX: [],
    accelY: [],
    accelZ: [],
    gyroX: [],
    gyroY: [],
    gyroZ: []
};

let newData = false;

// GRAPH CONFIG
const graphConfig = {
    accelX: { id: "accelX", min: -2, max: 2 },
    accelY: { id: "accelY", min: -2, max: 2 },
    accelZ: { id: "accelZ", min: -2, max: 2 },
    gyroX: { id: "gyroX", min: -500, max: 500 },
    gyroY: { id: "gyroY", min: -500, max: 500 },
    gyroZ: { id: "gyroZ", min: -500, max: 500 }
};

const graphs = {};

// make graphs
for (const key in graphConfig) {
    const config = graphConfig[key];
    const canvas = document.getElementById(config.id);
    const ctx = canvas.getContext("2d");
    graphs[key] = {
        canvas,
        ctx,
        min: config.min,
        max: config.max,
        width: 0,
        height: 0
    };
}

// resize
function resizeGraph(graph) {
    const rect = graph.canvas.getBoundingClientRect();
    const dpr = Math.min(window.devicePixelRatio || 1, 2);

    graph.canvas.width = Math.floor(rect.width * dpr);
    graph.canvas.height = Math.floor(rect.height * dpr);
    graph.canvas.style.width = `${rect.width}px`;
    graph.canvas.style.height = `${rect.height}px`;

    graph.ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

    graph.width = rect.width;
    graph.height = rect.height;
}

function resizeAll() {
    Object.values(graphs).forEach(resizeGraph);
}

resizeAll();

window.addEventListener("resize", resizeAll);

// add data sample to graph
function addSample(sensor) {
    data.accelX.push(sensor.accelX);
    data.accelY.push(sensor.accelY);
    data.accelZ.push(sensor.accelZ);
    data.gyroX.push(sensor.gyroX);
    data.gyroY.push(sensor.gyroY);
    data.gyroZ.push(sensor.gyroZ);

    // remove oldest sample when buffer is full
    if (data.accelX.length > MAX_POINTS) {
        data.accelX.shift();
    }
    if (data.accelY.length > MAX_POINTS) {
        data.accelY.shift();
    }
    if (data.accelZ.length > MAX_POINTS) {
        data.accelZ.shift();
    }
    if (data.gyroX.length > MAX_POINTS) {
        data.gyroX.shift();
    }
    if (data.gyroY.length > MAX_POINTS) {
        data.gyroY.shift();
    }
    if (data.gyroZ.length > MAX_POINTS) {
        data.gyroZ.shift();
    }
    newData = true;
}

// y values and normalizing
function valueToY(value, min, max, height) {
    const normalized = (value - min) / (max - min);
    return (height - normalized * height);
}

// draw graph
function drawGraph(graph, values) {
    const ctx = graph.ctx;
    const width = graph.width;
    const height = graph.height;

    ctx.fillStyle = "#1c1c1c";
    ctx.fillRect(0, 0, width, height);

    // grid
    ctx.strokeStyle = "#333";
    ctx.lineWidth = 1;
    ctx.beginPath();
    for (let i = 1; i < 4; i++) {
        const y = height / 4 * i;
        ctx.moveTo(0, y);
        ctx.lineTo(width, y);
    }
    for (let i = 0; i < values.length; i++) {
        const x = i / (MAX_POINTS - 1) * width;
        const y = valueToY(values[i], graph.min, graph.max, height);
        if (i === 0) {
            ctx.moveTo(x, y);
        } else {
            ctx.lineTo(x, y);
        }
    }
    ctx.stroke();

    // zero line
    const zeroY = valueToY(0, graph.min, graph.max, height);
    ctx.strokeStyle = "#555";
    ctx.beginPath();
    ctx.moveTo(0, zeroY);
    ctx.lineTo(width, zeroY);
    ctx.stroke();

    // data
    if (values.length < 2) {
        return;
    }
    ctx.strokeStyle = "#fff";
    ctx.lineWidth = 2;
    ctx.beginPath();
    for (let i = 0; i < values.length; i++) {
        const x = i / (MAX_POINTS - 1) * width;
        const y = valueToY(values[i], graph.min, graph.max, height);
        if (i === 0) {
            ctx.moveTo(x, y);
        } else {
            ctx.lineTo(x, y);
        }
    }
    ctx.stroke();

    // value labels
    ctx.fillStyle = "#aaa";
    ctx.font = "12px Arial";
    ctx.fillText(graph.max, 5, 14);
    ctx.fillText("0", 5, zeroY - 5);
    ctx.fillText(graph.min, 5, height - 5);
}

// rendering graph data
function render() {
    drawGraph(graphs.accelX, data.accelX);
    drawGraph(graphs.accelY, data.accelY);
    drawGraph(graphs.accelZ, data.accelZ);
    drawGraph(graphs.gyroX, data.gyroX);
    drawGraph(graphs.gyroY, data.gyroY);
    drawGraph(graphs.gyroZ, data.gyroZ);
}

// rendering loop
function animationLoop() {
    if (newData) {
        render();
        newData = false;
    }
    requestAnimationFrame(animationLoop);
}

requestAnimationFrame(animationLoop);

// websockets
const socket = new WebSocket(`ws://${window.location.host}`);
socket.addEventListener("open", () => {
    console.log("WebSocket connected");
});

socket.addEventListener("message", event => {
    try {
        const sensor = JSON.parse(event.data);
        if (Number.isFinite(sensor.accelX)) {
            addSample(sensor);
        }
    } catch (error) {
        console.error("Invalid sensor data:", error);
    }
});

socket.addEventListener("error", error => {
    console.error("WebSocket error:", error);
});

socket.addEventListener("close", () => {
    console.log("WebSocket disconnected");
});