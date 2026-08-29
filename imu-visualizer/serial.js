const { SerialPort } = require("serialport");

const { ReadlineParser } = require("@serialport/parser-readline");

const connect = (handler) => {
    // serial ports
    // UPDATE AS NEEDED FOR DEVICE

    let port = new SerialPort({
        path: "COM6",
        baudRate: 115200,
    });

    // uncomment port2 once 2nd xiao is implemented

    // let port2 = new SerialPort({
    //     path: "COM6",
    //     baudRate: 9600,
    // });

    const parser = port.pipe(new ReadlineParser({ delimiter: "\r\n" }));
    // const parser2 = port2.pipe(new ReadlineParser({ delimiter: "\r\n" }));

    port.on("open", () => {
        console.log("Serial port opened. Listening...\n");
    });

    // port2.on("open", () => {
    //     console.log("Serial port2 opened. Listening...");
    // });

    parser.on("data", (line) => {
        handler(line, "arduino1");
    });

    // parser2.on("data", (line) => {
    //     console.log("Received from Arduino 2: ", line);
    //     handler(line, "arduino2");
    // });

    port.on("error", (err) => {
        console.log("Could not connect to Arduino 1");
        console.log(err);
    });

    // port2.on("error", (err) => {
    //     console.log("Could not connect to Arduino 2");
    //     console.log(err);
    // });

    return { port, /*port2 */ };
};

module.exports = connect;