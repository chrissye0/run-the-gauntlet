using System;
using System.IO.Ports;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class ArduinoConnector : MonoBehaviour
{
    // WINDOW PORTS
    //private SerialPort leftSerial = new SerialPort("COM5", 115200);
    //private SerialPort rightSerial = new SerialPort("COM6", 115200);

    // MAC PORTS
    //private SerialPort leftSerial = new SerialPort("/dev/tty.usbmodem14301", 115200);
    //private SerialPort rightSerial = new SerialPort("/dev/cu.usbmodem14201", 115200);

    [Header("UDP Connection")]
    [SerializeField] private int udpPort = 5005;
    private UdpClient udpClient;

    public enum PunchType
    {
        Jab, Cross, Hook, Uppercut
    }

    public enum Hand
    {
        Left, Right
    }

    public event Action<Hand, PunchType, string> PunchDetected;
    public event Action BlockDetected;
    public event Action UnblockDetected;

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
    [SerializeField] private float leftJabAccelX = -5f;
    // Cross
    [SerializeField] private float leftCrossAccelX = -4f;
    [SerializeField] private float leftCrossGyroX = 800f;
    // Hook
    [SerializeField] private float leftHookAccelY = 2f;
    [SerializeField] private float leftHookAccelZ = -2f;
    // Uppercut
    [SerializeField] private float leftUppercutAccelY = 2f;
    [SerializeField] private float leftUppercutAccelZ = -8f;

    // RIGHT HAND THRESHOLDS
    [Header("Right Hand Thresholds")]
    // Jab
    public float rightJabAccelX = -5f;
    // Cross
    [SerializeField] private float rightCrossAccelX = -4f;
    [SerializeField] private float rightCrossGyroX = 800f;
    // Hook
    [SerializeField] private float rightHookAccelY = 2f;
    [SerializeField] private float rightHookAccelZ = -5f;
    // Uppercut
    [SerializeField] private float rightUppercutAccelY = 2f;
    [SerializeField] private float rightUppercutAccelZ = -8f;

    // TIMING
    [Header("Timing")]
    // time for collecting sensor data
    public float maxPunchDuration = 0.20f;
    // punch cooldown
    [SerializeField] private float cooldownDuration = 0.70f;
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
    [SerializeField] private float unblockAccelX = -2f;
    // minimum time a block can last
    [SerializeField] private float minimumBlockDuration = 1f;
    // latest accelX received from each sensor
    private float latestLeftAccelX = 0f;
    private float latestRightAccelX = 0f;
    [SerializeField] private float blockCooldownDuration = 2f;
    private float blockCooldownTimer = 0f;
    private float blockStartTime = 0f;
    private PlayerCombat playerCombat;

    private void Start()
    {
        playerCombat = GetComponent<PlayerCombat>();
        try
        {
            // listen for Python BLE bridge on UDP port 5005
            udpClient = new UdpClient(udpPort);
            // don't block Unity's main thread waiting for sensor data
            udpClient.Client.Blocking = false;
            Debug.Log($"IMU UDP receiver listening on port {udpPort}");
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Could not open UDP port {udpPort}: {ex.Message}");
            enabled = false;
        }
    }

    private void Update()
    {
        ReceiveImuPackets();
        // check cooldowns
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
        if (blockCooldownTimer > 0f)
        {
            blockCooldownTimer -= Time.deltaTime;
            if (blockCooldownTimer < 0f)
            {
                blockCooldownTimer = 0f;
            }
        }
    }

    private void ReceiveImuPackets()
    {
        if (udpClient == null) return;
        //dDrain every packet currently waiting without blocking Unity
        while (udpClient.Available > 0)
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] packet = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(packet);
                ProcessSensorData(message);
            }
            catch (SocketException ex)
            {
                // if no packet is ready
                if (ex.ErrorCode != 10035) Debug.LogWarning($"UDP receive error: {ex.Message}");
                break;
            }
        }
    }

    private void ProcessSensorData(string line)
    {
        string[] values = line.Trim().Split(',');
        // return out if not the right length
        if (values.Length != 7) return;
        // parse with invariant culture because of decimal points
        if (!float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float accelX) ||
            !float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float accelY) ||
            !float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float accelZ) ||
            !float.TryParse(values[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float gyroX) ||
            !float.TryParse(values[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float gyroY) ||
            !float.TryParse(values[6], NumberStyles.Float, CultureInfo.InvariantCulture, out float gyroZ)) return;
        switch (values[0].Trim())
        {
            case "L":
                ProcessLeftHand(accelX, accelY, accelZ, gyroX);
                break;
            case "R":
                ProcessRightHand(accelX, accelY, accelZ, gyroX);
                break;
            default:
                Debug.LogWarning("unknown label");
                break;
        }
    }

    // process data from left hand
    private void ProcessLeftHand(float accelX, float accelY, float accelZ, float gyroX)
    {
        // return out if in cooldown
        if (leftState == PunchState.Cooldown || blockCooldownTimer - 1f > 0f) return;

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
        if (uppercutScore == highestScore)
        {
            ExecutePunch(Hand.Left, PunchType.Uppercut, "UppercutL");
            Debug.Log("UPPERCUT SCORE: " + highestScore);
        }
        else if (hookScore == highestScore)
        {
            ExecutePunch(Hand.Left, PunchType.Hook, "HookL");
            Debug.Log("HOOK SCORE: " + highestScore);
        }
        else if (crossScore == highestScore)
        {
            ExecutePunch(Hand.Left, PunchType.Cross, "CrossL");
            Debug.Log("CROSS SCORE: " + highestScore);
        }
        else if (jabScore == highestScore)
        {
            ExecutePunch(Hand.Left, PunchType.Jab, "JabL");
            Debug.Log("JAB SCORE: " + highestScore);
        }
    }

    // RIGHT HAND
    // process data from right hand
    private void ProcessRightHand(float accelX, float accelY, float accelZ, float gyroX)
    {
        // return out if in cooldown
        if (rightState == PunchState.Cooldown || blockCooldownTimer > 0f) return;

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
        if (uppercutScore == highestScore)
        {
            ExecutePunch(Hand.Right, PunchType.Uppercut, "UppercutR");
            Debug.Log("UPPERCUT SCORE: " + highestScore);
        }
        else if (hookScore == highestScore)
        {
            ExecutePunch(Hand.Right, PunchType.Hook, "HookR");
            Debug.Log("HOOK SCORE: " + highestScore);
        }
        else if (crossScore == highestScore)
        {
            ExecutePunch(Hand.Right, PunchType.Cross, "CrossR");
            Debug.Log("CROSS SCORE: " + highestScore);
        }
        else if (jabScore == highestScore)
        {
            ExecutePunch(Hand.Right, PunchType.Jab, "JabR");
            Debug.Log("JAB SCORE: " + highestScore);
        }
    }

    // execute punch and go into cooldown
    private void ExecutePunch(Hand hand, PunchType punch, string animation)
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
        PunchDetected?.Invoke(hand, punch, animation);
    }

    private void UpdateBlockingState()
    {
        // return out if on cooldown
        if (!playerCombat.blocking && blockCooldownTimer > 0f) return;
        // if not currently blocking and both arms pass thresholds
        if (!playerCombat.blocking && latestLeftAccelX > blockAccelX && latestRightAccelX > blockAccelX)
        {
            Debug.Log("Blocking");
            playerCombat.blocking = true;
            // record when the block starts
            blockStartTime = Time.time;
            // reset punch states
            leftState = PunchState.Waiting;
            rightState = PunchState.Waiting;
            BlockDetected.Invoke();
        }
        // if unblocking
        else if (playerCombat.blocking && latestLeftAccelX < unblockAccelX && latestRightAccelX < unblockAccelX)
        {
            // return out if not enough time passed
            if (Time.time - blockStartTime < minimumBlockDuration) return;
            Debug.Log("Unblocking");
            playerCombat.blocking = false;
            // start cooldown
            blockCooldownTimer = blockCooldownDuration;
            // reset punch states
            leftState = PunchState.Waiting;
            rightState = PunchState.Waiting;
            UnblockDetected.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }
    }
}