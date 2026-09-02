using UnityEngine;
using System.IO.Ports;

public class ArduinoConnector : MonoBehaviour
{
    // arguments are port number and baud rate (ADJUST AS NEEDED)
    private SerialPort serial = new SerialPort("COM6", 115200);

    // time in between punches
    public float punchCooldown = 0.3f;
    // to track time that has passed
    private float punchCooldownTimer = 0f;

    // TRACKING PUNCH STATES
    private enum PunchState
    {
        Waiting,
        DetectingPunch,
        Cooldown
    }

    // initial state is waiting
    private PunchState punchState = PunchState.Waiting;

    // accelX detection (FOR ALL PUNCHES)
    // recording accelX peaks (updated in punchDetection)
    private float accelXPeak = 0f;
    // threshold for when accelX peaks (the jabbing motion)
    public float accelXPunchThreshold = 1.1f;
    // threshold for when accelX goes down (bringing arm back)
    public float accelXReturnThreshold = -0.9f;
    // threshold for default accelX (rest state)
    public float accelXRestThreshold = 0.3f;

    // accelZ detection (FOR UPPERCUTS)
    // recording accelZ peaks (updated in punchDetection)
    private float accelZPeak = 0f;
    // threshold for when accelZ peaks (the uppercut motion)
    public float accelZPunchThreshold = 0.8f;
    // threshold for when accelZ goes down (bringing arm down)
    public float accelZReturnThreshold = -0.1f;
    // threshold for default accelZ (rest state)
    public float accelZRestThreshold = 0.3f;

    // gyroX detection (FOR CROSSES)
    // threshold for gyroX range (for detecting arm rotation in crosses)
    public float gyroXPunchThreshold = 1000f;
    // minimum gyroX value (updated in punchDetection)
    private float gyroXMin = 0f;
    // maximum gyroX value (updated in punchDetection)
    private float gyroXMax = 0f;

    void Start()
    {
        serial.Open();
        // fixes serial connection problem
        serial.DtrEnable = true;
        // time in ms that the serial will wait to read the command
        serial.ReadTimeout = 50;
        Debug.Log("Serial opened: " + serial.IsOpen);
    }

    // Update is called once per frame
    void Update()
    {
        // PARSING ARDUINO DATA
        string data = serial.ReadLine();
        string[] values = data.Split(",");

        // return out if not right amount
        if (values.Length != 7) return;

        // try to parse values, return out if it fails
        if (!long.TryParse(values[0], out long time)) return;
        if (!float.TryParse(values[1], out float accelX)) return;
        if (!float.TryParse(values[2], out float accelY)) return;
        if (!float.TryParse(values[3], out float accelZ)) return;
        if (!float.TryParse(values[4], out float gyroX)) return;
        if (!float.TryParse(values[5], out float gyroY)) return;
        if (!float.TryParse(values[6], out float gyroZ)) return;

        DetectPunch(accelX, accelZ, accelZ, gyroX, gyroZ);
    }

    /**
     * CODE FOR PUNCH DETECTION
     * take in all needed values for jab, cross, hook, and uppercut
     * use a switch statement and PunchState states to differentiate
     */
    private void DetectPunch(float accelX, float accelY, float accelZ, float gyroX, float gyroZ)
    {
        switch (punchState)
        {
            // initializing values once accelX reaches a threshold
            case PunchState.Waiting:
                if (accelX > accelXPunchThreshold)
                {
                    // Initialize values
                    accelXPeak = accelX;
                    // if uppercut motion (hand goes up)
                    if (accelZ > accelZPunchThreshold)
                    {
                        accelZPeak = accelZ;
                    }
                    gyroXMin = gyroX;
                    gyroXMax = gyroX;
                    // go into detecting punch state
                    punchState = PunchState.DetectingPunch;
                }
                break;
            // detect what punch is thrown
            case PunchState.DetectingPunch:
                // track accelX peak
                if (accelX > accelXPeak)
                {
                    accelXPeak = accelX;
                }
                // track accelZ peak
                if (accelZ > accelZPeak)
                {
                    accelZPeak = accelZ;
                }
                // track gyroX range
                if (gyroX < gyroXMin)
                {
                    gyroXMin = gyroX;
                }
                if (gyroX > gyroXMax)
                {
                    gyroXMax = gyroX;
                }
                // punch ends when accelX goes negative
                if (accelX < accelXReturnThreshold)
                {
                    // get gyroX range for cross detection (because it's a range, it'll work for both left and right crosses)
                    float gyroXRange = gyroXMax - gyroXMin;
                    // determine punch type
                    if (gyroXRange > gyroXPunchThreshold)
                    {
                        // if gyroX fluctuation (cross)
                        Debug.Log("CROSS DETECTED!");
                    }
                    else
                    {
                        // if no gyroX fluctuation
                        if (accelZ < accelZReturnThreshold)
                        {
                            // if accelZ fluctuation (uppercut)
                            Debug.Log("UPPERCUT DETECTED!");
                        }
                        else
                        {
                            // if no accelZ fluctuation (jab)
                            Debug.Log("JAB DETECTED!");
                        }
                    }
                    // go into cooldown
                    punchState = PunchState.Cooldown;
                    punchCooldownTimer = Time.time + punchCooldown;
                }
                break;
            // go into cooldown after a punch is thrown
            case PunchState.Cooldown:
                // wait for cooldown and for acceleration to settle before going back to waiting state
                if (Time.time >= punchCooldownTimer && Mathf.Abs(accelX) < accelXRestThreshold && Mathf.Abs(accelZ) < accelZRestThreshold)
                {
                    punchState = PunchState.Waiting;
                }
                break;
        }
    }

    // close serial when app is closed
    private void OnApplicationQuit()
    {
        serial.Close();
    }
}
