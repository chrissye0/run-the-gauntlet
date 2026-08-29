const http = require("http");
const fs = require("fs");
const path = require("path");
const WebSocket = require("ws");

const connect = require("./serial");

const PORT = 3000;

// http server
const server = http.createServer((req, res) => {
    let filePath;
    if (req.url === "/") {
        filePath = path.join(
            __dirname,
            "public",
            "index.html"
        );
    } else {
        filePath = path.join(
            __dirname,
            "public",
            req.url
        );
    }

    const extension = path.extname(filePath);

    const contentTypes = {
        ".html": "text/html",
        ".js": "text/javascript",
        ".css": "text/css"
    };

    const contentType = contentTypes[extension] || "text/plain";

    fs.readFile(filePath, (err, data) => {
        if (err) {
            res.writeHead(404);
            res.end("Not found");
            return;
        }
        res.writeHead(200, {
            "Content-Type": contentType
        });
        res.end(data);
    }
    );

});

// websocket
const wss = new WebSocket.Server({ server });

wss.on("connection", (socket) => {
    console.log("Browser connected");
}
);

// latest sensor data
let latestData = null;

// serial data handler
const handleSerialData =
    (line, device) => {
        // ignore header
        if (line.startsWith("time")) {
            return;
        }
        // split CSV
        const values = line.split(",");
        if (values.length !== 7) {
            return;
        }
        const [time, accelX, accelY, accelZ, gyroX, gyroY, gyroZ] = values.map(Number);

        // validate
        if (!Number.isFinite(time) || !Number.isFinite(accelX) || !Number.isFinite(accelY) || !Number.isFinite(accelZ) || !Number.isFinite(gyroX) || !Number.isFinite(gyroY) || !Number.isFinite(gyroZ)) {
            return;
        }

        // Store ONLY the newest sample
        latestData = { device, time, accelX, accelY, accelZ, gyroX, gyroY, gyroZ };
    };

// connect to xiao
connect(handleSerialData);

// SEND TO BROWSER
// xiao samples at 100 hz, node just doesn't send every sample to the viz
setInterval(() => {
    if (!latestData) {
        return;
    }
    const message = JSON.stringify(latestData);
    wss.clients.forEach(
        (client) => {
            if (client.readyState === WebSocket.OPEN) {
                client.send(message);
            }
        }
    );
},
    33 // samples (roughly 30fps)
);

// start server
server.listen(
    PORT, () => {
        console.log(`Server running at http://localhost:${PORT}`);
    }
);