#include <Arduino.h>
#include "LSM6DS3.h"
#include "Wire.h"

LSM6DS3 myIMU(I2C_MODE, 0x6A);

void setup() {
    Serial.begin(115200);
    while (!Serial);

    if (myIMU.begin() != 0) {
        Serial.println("ERROR");
        while (1);
    }

    Serial.println("time,accelX,accelY,accelZ,gyroX,gyroY,gyroZ");
}

void loop() {
    float ax = myIMU.readFloatAccelX();
    float ay = myIMU.readFloatAccelY();
    float az = myIMU.readFloatAccelZ();

    float gx = myIMU.readFloatGyroX();
    float gy = myIMU.readFloatGyroY();
    float gz = myIMU.readFloatGyroZ();

    Serial.print(millis());
    Serial.print(",");

    Serial.print(ax, 4);
    Serial.print(",");

    Serial.print(ay, 4);
    Serial.print(",");

    Serial.print(az, 4);
    Serial.print(",");

    Serial.print(gx, 4);
    Serial.print(",");

    Serial.print(gy, 4);
    Serial.print(",");

    Serial.println(gz, 4);

    delay(10);
}