
#include <Arduino.h>
#include <bluefruit.h>
#include <Wire.h>
#include "LSM6DS3.h"

// TO CHANGE THE BOARD, REFERENCE LINES 42 AND 43

LSM6DS3 myIMU(I2C_MODE, 0x6A);

// send data every 20 ms
const unsigned long SAMPLE_INTERVAL_MS = 20;
unsigned long lastSampleTime = 0;

// Custom BLE service and characteristic UUIDs
BLEService imuService(0xFFF0);
BLECharacteristic imuCharacteristic(0xFFF1);

// packet contains six int16 values (12 bytes)
const uint16_t PACKET_SIZE = 12;

void setup()
{
    Serial.begin(115200);
    delay(500);
    // Initialize the IMU
    Serial.println("Initializing IMU");
    if (myIMU.begin() != 0) {
        Serial.println("IMU initialization failed");
        while (true) {
            delay(1000);
        }
    }

    Serial.println("IMU initialized successfully.");
    // START BLUETOOTH
    Bluefruit.begin();
    Bluefruit.setTxPower(4);

    // Uncomment XIAO_LEFT or XIAO_RIGHT, connect that XIAO to your computer, then upload the code
    // repeat for 2nd hand
    // Bluefruit.setName("XIAO_LEFT");
    Bluefruit.setName("XIAO_RIGHT");

    // start the service before starting its characteristic
    imuService.begin();

    // configure the characteristic
    imuCharacteristic.setProperties(CHR_PROPS_READ | CHR_PROPS_NOTIFY);
    imuCharacteristic.setPermission(SECMODE_OPEN, SECMODE_NO_ACCESS);
    imuCharacteristic.setFixedLen(PACKET_SIZE);
    imuCharacteristic.begin();

    // initialize the characteristic with a 12-byte value
    uint8_t initialPacket[PACKET_SIZE] = {0};
    imuCharacteristic.write(initialPacket, PACKET_SIZE);

    // configure BLE advertising
    Bluefruit.Advertising.addFlags(BLE_GAP_ADV_FLAGS_LE_ONLY_GENERAL_DISC_MODE);

    Bluefruit.Advertising.addTxPower();
    Bluefruit.Advertising.addService(imuService);
    Bluefruit.Advertising.addName();

    // resume advertising after disconnect
    Bluefruit.Advertising.restartOnDisconnect(true);

    // advertising interval: 20 ms to 152.5 ms
    Bluefruit.Advertising.setInterval(32, 244);
    Bluefruit.Advertising.start(0);

    Serial.println("Service UUID: FFF0");
    Serial.println("Characteristic UUID: FFF1");
    Serial.println("Packet size: 12 bytes");
    Serial.println("Waiting for BLE connection...");
}

void loop()
{
    unsigned long currentTime = millis();
    if (currentTime - lastSampleTime < SAMPLE_INTERVAL_MS) {
        return;
    }

    lastSampleTime = currentTime;

    // read accelerometer values
    float ax = myIMU.readFloatAccelX();
    float ay = myIMU.readFloatAccelY();
    float az = myIMU.readFloatAccelZ();

    // read gyroscope values
    float gx = myIMU.readFloatGyroX();
    float gy = myIMU.readFloatGyroY();
    float gz = myIMU.readFloatGyroZ();

    // convert readings to compact signed 16-bit integers
    // acceleration precision of 0.001
    // gyroscope precision of 0.1
    int16_t values[6] =
    {
        (int16_t)(ax * 1000.0f),
        (int16_t)(ay * 1000.0f),
        (int16_t)(az * 1000.0f),
        (int16_t)(gx * 10.0f),
        (int16_t)(gy * 10.0f),
        (int16_t)(gz * 10.0f)
    };

    // send one 12-byte BLE notification
    imuCharacteristic.notify((uint8_t*)values, sizeof(values));

    // print status message over USB serial
    static unsigned long lastStatusTime = 0;
    if (currentTime - lastStatusTime >= 2000) {
        lastStatusTime = currentTime;
        Serial.print("IMU: ");
        Serial.print(ax, 3);
        Serial.print(", ");
        Serial.print(ay, 3);
        Serial.print(", ");
        Serial.print(az, 3);
        Serial.print(", ");
        Serial.print(gx, 1);
        Serial.print(", ");
        Serial.print(gy, 1);
        Serial.print(", ");
        Serial.print(gz, 1);
        Serial.print("\n");
    }
}