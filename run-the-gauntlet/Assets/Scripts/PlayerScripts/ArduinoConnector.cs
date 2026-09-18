using System;
using System.IO.Ports;
using UnityEngine;

public class ArduinoConnector : MonoBehaviour
{
    // arguments are port number and baud rate (ADJUST AS NEEDED)
    private SerialPort leftSerial = new SerialPort("COM5", 115200);
    private SerialPort rightSerial = new SerialPort("COM6", 115200);

    //picking up on punch events (connect to PlayerCombat)
    public enum PunchType
    {
        Jab,
        Cross,
        Hook,
        Uppercut
    }
    public enum Hand
    {
        Left,
        Right
    }
    public event Action<Hand, PunchType> PunchDetected;

    // time in between punches
    public float punchCooldown = 0.8f;

    // TRACKING PUNCH STATES
    private enum PunchState
    {
        Waiting,
        DetectingPunch,
        Cooldown
    }

    private class PunchTracker
    {
        // initialize punch state to waiting
        public PunchState state = PunchState.Waiting;
        // to track time that has passed
        public float cooldownTimer = 0f;
        // Accel X
        public float accelXMin = 0f;
        public float accelXMax = 0f;
        // Accel Y
        public float accelYMin = 0f;
        public float accelYMax = 0f;
        // Accel Z
        public float accelZMin = 0f;
        public float accelZMax = 0f;
        // Gyro X
        public float gyroXMin = 0f;
        public float gyroXMax = 0f;
        // Gyro Y
        public float gyroYMin = 0f;
        public float gyroYMax = 0f;
    }

    // create independent trackers for each glove
    private PunchTracker leftTracker = new PunchTracker();
    private PunchTracker rightTracker = new PunchTracker();

    // threshold for when accelX peaks (the forward jabbing motion)
    public float accelXPunchThreshold = 1f;

    // threshold for when accelY peaks
    public float accelYPunchThreshold = 2f;

    // threshold for when accelZ peaks
    public float accelZPunchThreshold = 0.0f;

    // threshold for gyroX range (for detecting arm rotation in crosses)
    public float gyroXPunchThreshold = 400f;

    // threshold for gyroY range
    public float gyroYPunchThreshold = 300f;

    // threshold for gyroZ range
    //public float gyroZPunchThreshold = 400f;

    void Start()
    {
        leftSerial.Open();
        rightSerial.Open();
        // fixes serial connection problem
        leftSerial.DtrEnable = true;
        rightSerial.DtrEnable = true;
        // time in ms that the serial will wait to read the command
        leftSerial.ReadTimeout = 50;
        rightSerial.ReadTimeout = 50;
        Debug.Log("Left serial opened: " + leftSerial.IsOpen);
        Debug.Log("Right serial opened: " + leftSerial.IsOpen);
    }

    // Update is called once per frame
    void Update()
    {
        // PARSING ARDUINO DATA
        string leftData;
        string rightData;
        try
        {
            leftData = leftSerial.ReadLine();
            rightData = rightSerial.ReadLine();
        }
        catch (TimeoutException)
        {
            return;
        }
        string[] leftValues = leftData.Split(",");
        string[] rightValues = rightData.Split(",");
        // return out if not right amount
        if (leftValues.Length != 8) return;
        if (rightValues.Length != 8) return;
        // try to parse values, return out if it fails
        //if (!long.TryParse(leftValues[1], out long leftTime)) return;
        if (!float.TryParse(leftValues[2], out float leftAccelX)) return;
        if (!float.TryParse(leftValues[3], out float leftAccelY)) return;
        if (!float.TryParse(leftValues[4], out float leftAccelZ)) return;
        if (!float.TryParse(leftValues[5], out float leftGyroX)) return;
        if (!float.TryParse(leftValues[6], out float leftGyroY)) return;
        if (!float.TryParse(leftValues[7], out float leftGyroZ)) return;
        //if (!long.TryParse(rightValues[1], out long rightTime)) return;
        if (!float.TryParse(rightValues[2], out float rightAccelX)) return;
        if (!float.TryParse(rightValues[3], out float rightAccelY)) return;
        if (!float.TryParse(rightValues[4], out float rightAccelZ)) return;
        if (!float.TryParse(rightValues[5], out float rightGyroX)) return;
        if (!float.TryParse(rightValues[6], out float rightGyroY)) return;
        if (!float.TryParse(rightValues[7], out float rightGyroZ)) return;
        // use these values + hand type + tracker to detect punches
        DetectPunch(Hand.Left, leftAccelX, leftAccelY, leftAccelZ, leftGyroX, leftGyroY, leftTracker);
        DetectPunch(Hand.Right, rightAccelX, rightAccelY, rightAccelZ, rightGyroX, rightGyroY, rightTracker);
    }

    /**
    * CODE FOR PUNCH DETECTION
    * take in all needed values for jab, cross, hook, and uppercut
    * use a switch statement and PunchState states to differentiate
    */
    private void DetectPunch(Hand hand, float accelX, float accelY, float accelZ, float gyroX, float gyroY, PunchTracker tracker)
    {
        switch (tracker.state)
        {
            // initializing values once accelX reaches a threshold
            case PunchState.Waiting:
                if (accelX > accelXPunchThreshold /* || Mathf.Abs(accelY) > accelYPunchThreshold */)
                {
                    // initialize tracker values
                    tracker.accelXMin = accelX;
                    tracker.accelXMax = accelX;
                    tracker.accelYMin = accelY;
                    tracker.accelYMax = accelY;
                    tracker.accelZMin = accelZ;
                    tracker.accelZMax = accelZ;
                    tracker.gyroXMin = gyroX;
                    tracker.gyroXMax = gyroX;
                    tracker.gyroYMin = gyroY;
                    tracker.gyroYMax = gyroY;
                    // go into detecting punch state
                    tracker.state = PunchState.DetectingPunch;
                }
                break;
            // detect what punch is thrown
            case PunchState.DetectingPunch:
                // update min and max values
                if (accelX < tracker.accelXMin)
                {
                    tracker.accelXMin = accelX;
                }
                if (accelX > tracker.accelXMax)
                {
                    tracker.accelXMax = accelX;
                }
                if (accelY < tracker.accelYMin)
                {
                    tracker.accelYMin = accelY;
                }
                if (accelY > tracker.accelYMax)
                {
                    tracker.accelYMax = accelY;
                }
                if (accelZ < tracker.accelZMin)
                {
                    tracker.accelZMin = accelZ;
                }
                if (accelZ > tracker.accelZMax)
                {
                    tracker.accelZMax = accelZ;
                }
                if (gyroX < tracker.gyroXMin)
                {
                    tracker.gyroXMin = gyroX;
                }
                if (gyroX > tracker.gyroXMax)
                {
                    tracker.gyroXMax = gyroX;
                }
                if (gyroY < tracker.gyroYMin)
                {
                    tracker.gyroYMin = gyroY;
                }
                if (gyroY > tracker.gyroYMax)
                {
                    tracker.gyroYMax = gyroY;
                }
                // get ranges of everything
                float accelXRange = tracker.accelXMax - tracker.accelXMin;
                float accelYRange = tracker.accelYMax - tracker.accelYMin;
                float accelZRange = tracker.accelZMax - tracker.accelZMin;
                float gyroXRange = tracker.gyroXMax - tracker.gyroXMin;
                float gyroYRange = tracker.gyroYMax - tracker.gyroYMin;
                // if accelX passes threshold (this would be a forward motion)
                if (accelXRange > accelXPunchThreshold)
                {
                    // check gyroX for crosses
                    if (gyroXRange > gyroXPunchThreshold)
                    {
                        if (hand == Hand.Left)
                        {
                            Debug.Log("LEFT CROSS DETECTED!");
                            PunchDetected?.Invoke(Hand.Left, PunchType.Cross);
                        }
                        else if (hand == Hand.Right)
                        {
                            Debug.Log("RIGHT CROSS DETECTED!");
                            PunchDetected?.Invoke(Hand.Right, PunchType.Cross);
                        }
                        tracker.state = PunchState.Cooldown;
                        tracker.cooldownTimer = Time.time + punchCooldown;
                        break;
                    }
                    else if (gyroYRange > gyroYPunchThreshold && accelZRange > accelZPunchThreshold)
                    {
                        if (hand == Hand.Left)
                        {
                            Debug.Log("LEFT HOOK DETECTED!");
                            PunchDetected?.Invoke(Hand.Left, PunchType.Hook);
                        }
                        else if (hand == Hand.Right)
                        {
                            Debug.Log("RIGHT HOOK DETECTED!");
                            PunchDetected?.Invoke(Hand.Right, PunchType.Hook);
                        }
                        tracker.state = PunchState.Cooldown;
                        tracker.cooldownTimer = Time.time + punchCooldown;
                        break;
                    }
                    else
                    {
                        // if no gyroX fluctuation
                        if (hand == Hand.Left)
                        {
                            Debug.Log("LEFT JAB DETECTED!");
                            PunchDetected?.Invoke(Hand.Left, PunchType.Jab);
                        }
                        else if (hand == Hand.Right)
                        {
                            Debug.Log("RIGHT JAB DETECTED!");
                            PunchDetected?.Invoke(Hand.Right, PunchType.Jab);
                        }
                        tracker.state = PunchState.Cooldown;
                        tracker.cooldownTimer = Time.time + punchCooldown;
                        break;
                    }
                }
                break;
            case PunchState.Cooldown:
                // go into cooldown - wait until cooldown timer passes and accelX goes back to normal
                if (Time.time >= tracker.cooldownTimer && Mathf.Abs(accelX) < accelXPunchThreshold)
                {
                    tracker.state = PunchState.Waiting;
                }
                break;
        }
    }

    // close serial when app is closed
    private void OnApplicationQuit()
    {
        leftSerial.Close();
        rightSerial.Close();
    }
}
