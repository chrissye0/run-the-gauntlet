using System;
using System.IO.Ports;
using UnityEngine;

public class ArduinoConnector : MonoBehaviour
{
    private SerialPort leftSerial = new SerialPort("COM5", 115200);
    private SerialPort rightSerial = new SerialPort("COM6", 115200);

    public enum PunchType
    {
        Jab, Cross, Hook, Uppercut
    }

    public enum Hand
    {
        Left, Right
    }

    public event Action<Hand, PunchType> PunchDetected;

    private enum PunchState
    {
        Waiting,
        Windup,
        Punching,
        Cooldown
    }

    private PunchState leftState = PunchState.Waiting;
    private PunchState rightState = PunchState.Waiting;


    // LEFT HAND THRESHOLDS
    [Header("Left Hand Thresholds")]
    // Jab
    public float leftJabAccelX = -5f;
    // Cross
    public float leftCrossAccelX = -4f;
    public float leftCrossGyroX = 800f;
    // Hook
    public float leftHookAccelY = 2f;
    public float leftHookAccelZ = -2f;
    // Uppercut
    public float leftUppercutAccelY = 2f;
    public float leftUppercutAccelZ = -8f;

    // RIGHT HAND THRESHOLDS
    [Header("Right Hand Thresholds")]
    // Jab
    public float rightJabAccelX = -5f;
    // Cross
    public float rightCrossAccelX = -4f;
    public float rightCrossGyroX = 800f;
    // Hook
    public float rightHookAccelY = 2f;
    public float rightHookAccelZ = -5f;
    // Uppercut
    public float rightUppercutAccelY = 2f;
    public float rightUppercutAccelZ = -8f;

    // TIMING
    [Header("Timing")]
    // time for collecting sensor data
    public float maxPunchDuration = 0.20f;
    // punch cooldown
    public float cooldownDuration = 0.70f;
    private float leftCooldownTimer = 0f;
    private float rightCooldownTimer = 0f;

    // left hand detection data
    private float leftPeakAccelY;
    private float leftMinAccelX;
    private float leftMinAccelZ;
    private float leftPeakGyroX;
    private float leftDetectionStartTime;

    // right hand detection data
    private float rightPeakAccelY;
    private float rightMinAccelX;
    private float rightMinAccelZ;
    private float rightPeakGyroX;
    private float rightDetectionStartTime;

    // BLOCKING THRESHOLDS (ADJUST AS NEEDED)
    [Header("Blocking")]
    // threshold to start blocking
    public float blockAccelX = 3f;
    // threshold to stop blocking (if blocking)
    public float unblockAccelX = 3f;
    // latest accelX received from each sensor
    private float latestLeftAccelX = 0f;
    private float latestRightAccelX = 0f;
    private PlayerCombat playerCombat;

    private void Start()
    {
        leftSerial.Open();
        rightSerial.Open();

        // fixes serial connection problem
        leftSerial.DtrEnable = true;
        rightSerial.DtrEnable = true;

        leftSerial.ReadTimeout = 50;
        rightSerial.ReadTimeout = 50;

        playerCombat = GetComponent<PlayerCombat>();

        Debug.Log("Left serial opened: " + leftSerial.IsOpen);
        Debug.Log("Right serial opened: " + leftSerial.IsOpen);
    }

    private void Update()
    {
        if (leftSerial == null || !leftSerial.IsOpen || rightSerial == null || !rightSerial.IsOpen) return;
        while (leftSerial.BytesToRead > 0)
        {
            try
            {
                ProcessSensorData(leftSerial.ReadLine());
            }
            catch (TimeoutException)
            {
                break;
            }
        }
        while (rightSerial.BytesToRead > 0)
        {
            try
            {
                ProcessSensorData(rightSerial.ReadLine());
            }
            catch (TimeoutException)
            {
                break;
            }
        }
        // check cooldown
        if (leftState == PunchState.Cooldown)
        {
            leftCooldownTimer -= Time.deltaTime;
            if (leftCooldownTimer <= 0f)
            {
                leftCooldownTimer = 0f;
                leftState = PunchState.Waiting;
            }
        }
        if (rightState == PunchState.Cooldown)
        {
            rightCooldownTimer -= Time.deltaTime;
            if (rightCooldownTimer <= 0f)
            {
                rightCooldownTimer = 0f;
                rightState = PunchState.Waiting;
            }
        }
    }

    private void ProcessSensorData(string line)
    {
        string[] values = line.Trim().Split(',');
        // if not the right number of values
        if (values.Length < 8) return;
        // parse into floats
        if (!float.TryParse(values[2], out float accelX)) return;
        if (!float.TryParse(values[3], out float accelY)) return;
        if (!float.TryParse(values[4], out float accelZ)) return;
        if (!float.TryParse(values[5], out float gyroX)) return;
        if (!float.TryParse(values[6], out float gyroY)) return;
        if (!float.TryParse(values[7], out float gyroZ)) return;

        if (values[0].ToString() == "LEFT") ProcessLeftHand(accelX, accelY, accelZ, gyroX);
        else if (values[0].ToString() == "RIGHT") ProcessRightHand(accelX, accelY, accelZ, gyroX);
    }

    // process data from left hand
    private void ProcessLeftHand(float accelX, float accelY, float accelZ, float gyroX)
    {
        // return out if in cooldown
        if (leftState == PunchState.Cooldown) return;

        // update the latest blocking sensor value and update blocking state
        latestLeftAccelX = accelX;
        UpdateBlockingState();

        // return out if currently blocking
        if (playerCombat.blocking) return;

        // if waiting for input
        if (leftState == PunchState.Waiting)
        {
            // if the start of a movement has been detected
            if (accelX < leftCrossAccelX || accelZ < leftHookAccelZ || (accelY > leftUppercutAccelY && accelZ < leftUppercutAccelZ))
            {
                leftState = PunchState.Windup;
                leftDetectionStartTime = Time.time;
                // initialize values
                leftPeakAccelY = accelY;
                leftMinAccelX = accelX;
                leftMinAccelZ = accelZ;
                leftPeakGyroX = Mathf.Abs(gyroX);
            }
            return;
        }
        // if in windup state
        if (leftState == PunchState.Windup)
        {
            // strongest positive accelY
            leftPeakAccelY = Mathf.Max(leftPeakAccelY, accelY);
            // strongest negative accelX
            leftMinAccelX = Mathf.Min(leftMinAccelX, accelX);
            // strongest negative accelZ
            leftMinAccelZ = Mathf.Min(leftMinAccelZ, accelZ);
            // biggest gyroX magnitude
            leftPeakGyroX = Mathf.Max(leftPeakGyroX, Mathf.Abs(gyroX));
            // check if enough time has passed
            if (Time.time - leftDetectionStartTime >= maxPunchDuration)
            {
                ClassifyLeftPunch();
            }
        }
    }

    // classify left punch
    private void ClassifyLeftPunch()
    {
        float jabScore = 0f;
        float crossScore = 0f;
        float hookScore = 0f;
        float uppercutScore = 0f;

        // UPPERCUT
        if (leftMinAccelZ < leftUppercutAccelZ) uppercutScore += 4f;
        if (leftPeakAccelY > leftUppercutAccelY) uppercutScore += 2f;

        // JAB
        // check for big accelX
        if (leftMinAccelX < leftJabAccelX) jabScore += 5f;

        // CROSS
        // check for medium accelX and big gyroX
        if (leftMinAccelX < leftCrossAccelX) crossScore += 3f;
        if (leftPeakGyroX > leftCrossGyroX) crossScore += 3f;

        // HOOK
        if (leftMinAccelZ < leftHookAccelZ) hookScore += 3f;
        if (leftPeakAccelY > leftHookAccelY) hookScore += 2f;

        // calculate the highest score
        float highestScore = Mathf.Max(jabScore, crossScore, hookScore, uppercutScore);
        // return out if nothing classified
        if (highestScore < 3f)
        {
            leftState = PunchState.Waiting;
            return;
        }
        if (uppercutScore == highestScore) ExecutePunch(Hand.Left, PunchType.Uppercut);
        else if (hookScore == highestScore) ExecutePunch(Hand.Left, PunchType.Hook);
        else if (crossScore == highestScore) ExecutePunch(Hand.Left, PunchType.Cross);
        else if (jabScore == highestScore) ExecutePunch(Hand.Left, PunchType.Jab);
    }

    // RIGHT HAND
    // process data from right hand
    private void ProcessRightHand(float accelX, float accelY, float accelZ, float gyroX)
    {
        // return out if in cooldown
        if (rightState == PunchState.Cooldown) return;

        // update the latest blocking sensor value and update blocking state
        latestRightAccelX = accelX;
        UpdateBlockingState();

        // return out if currently blocking
        if (playerCombat.blocking) return;

        // if waiting for input
        if (rightState == PunchState.Waiting)
        {
            // if the start of a movement has been detected
            if (accelX < rightCrossAccelX || accelZ < rightHookAccelZ || (accelY > rightUppercutAccelY && accelZ < rightUppercutAccelZ))
            {
                rightState = PunchState.Windup;
                rightDetectionStartTime = Time.time;
                // initialize values
                rightPeakAccelY = accelY;
                rightMinAccelX = accelX;
                rightMinAccelZ = accelZ;
                rightPeakGyroX = Mathf.Abs(gyroX);
            }
            return;
        }
        // if in windup state
        if (rightState == PunchState.Windup)
        {
            // strongest positive accelY
            rightPeakAccelY = Mathf.Max(rightPeakAccelY, accelY);
            // strongest negative accelX
            rightMinAccelX = Mathf.Min(rightMinAccelX, accelX);
            // strongest negative accelZ
            rightMinAccelZ = Mathf.Min(rightMinAccelZ, accelZ);
            // biggest gyroX magnitude
            rightPeakGyroX = Mathf.Max(rightPeakGyroX, Mathf.Abs(gyroX));
            // check if enough time has passed
            if (Time.time - rightDetectionStartTime >= maxPunchDuration)
            {
                ClassifyRightPunch();
            }
        }
    }

    // classify right punch
    private void ClassifyRightPunch()
    {
        float jabScore = 0f;
        float crossScore = 0f;
        float hookScore = 0f;
        float uppercutScore = 0f;

        // UPPERCUT
        if (rightMinAccelZ < rightUppercutAccelZ) uppercutScore += 4f;
        if (rightPeakAccelY > rightUppercutAccelY) uppercutScore += 2f;

        // JAB
        // check for big accelX
        if (rightMinAccelX < rightJabAccelX) jabScore += 5f;

        // CROSS
        // check for medium accelX and big gyroX
        if (rightMinAccelX < rightCrossAccelX) crossScore += 3f;
        if (rightPeakGyroX > rightCrossGyroX) crossScore += 3f;

        // HOOK
        if (rightMinAccelZ < rightHookAccelZ) hookScore += 3f;
        if (rightPeakAccelY > rightHookAccelY) hookScore += 2f;

        // calculate the highest score
        float highestScore = Mathf.Max(jabScore, crossScore, hookScore, uppercutScore);
        // return out if nothing classified
        if (highestScore < 3f)
        {
            rightState = PunchState.Waiting;
            return;
        }
        if (uppercutScore == highestScore) ExecutePunch(Hand.Right, PunchType.Uppercut);
        else if (hookScore == highestScore) ExecutePunch(Hand.Right, PunchType.Hook);
        else if (crossScore == highestScore) ExecutePunch(Hand.Right, PunchType.Cross);
        else if (jabScore == highestScore) ExecutePunch(Hand.Right, PunchType.Jab);
    }

    // execute punch and go into cooldown
    private void ExecutePunch(Hand hand, PunchType punch)
    {
        Debug.Log(hand + " " + punch + " executed");
        if (hand == Hand.Left)
        {
            leftState = PunchState.Cooldown;
            leftCooldownTimer = cooldownDuration;
        }
        else
        {
            rightState = PunchState.Cooldown;
            rightCooldownTimer = cooldownDuration;
        }
        PunchDetected?.Invoke(hand, punch);
    }

    private void UpdateBlockingState()
    {
        // if not currently blocking and both arms pass thresholds
        if (!playerCombat.blocking && latestLeftAccelX > blockAccelX && latestRightAccelX > blockAccelX)
        {
            Debug.Log("blocking");
            Debug.Log("left accelX: " + latestLeftAccelX);
            Debug.Log("right accelX: " + latestRightAccelX);
            playerCombat.blocking = true;
            // reset punch states
            leftState = PunchState.Waiting;
            rightState = PunchState.Waiting;
        }
        // if unblocking
        else if (playerCombat.blocking && latestLeftAccelX > unblockAccelX && latestRightAccelX > unblockAccelX)
        {
            Debug.Log("unblocking");
            playerCombat.blocking = false;
            Debug.Log("left accelX: " + latestLeftAccelX);
            Debug.Log("right accelX: " + latestRightAccelX);
            // reset punch states
            leftState = PunchState.Waiting;
            rightState = PunchState.Waiting;
        }
    }

    private void OnDestroy()
    {
        if (leftSerial != null && leftSerial.IsOpen) leftSerial.Close();
        if (rightSerial != null && rightSerial.IsOpen) rightSerial.Close();
    }
}