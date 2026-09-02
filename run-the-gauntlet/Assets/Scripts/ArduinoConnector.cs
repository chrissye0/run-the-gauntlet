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

    // JAB VARIABLES - OPTIMIZE THIS LATER
    private bool detectingJab = false;
    private float jabPeak = 0f;
    public float jabPunchThreshold = 1.1f;
    public float jabReturnThreshold = -0.9f;

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

        if (Time.time > punchCooldownTimer)
        {
            DetectJab(accelX);
        }
    }

    /**
     * CODE FOR JAB DETECTION
     * Parameter is accelX value
     * Jabs show a positive spike followed by a negative one
     */
    private void DetectJab(float accelX)
    {
        if (!detectingJab)
        {
            // punch starts when accelX goes above threshold
            if (accelX > jabPunchThreshold)
            {
                // set detectingJab to true and update jabPeak to current accelX
                detectingJab = true;
                jabPeak = accelX;
            }
        }
        else
        {
            // keep track of the highest accelX by updating jabPeak
            if (accelX > jabPeak)
            {
                jabPeak = accelX;
            }

            // when accelX reaches the negative threshold and cooldown time has passed
            if(accelX < jabReturnThreshold)
            {
                // update cooldown timer
                punchCooldownTimer = Time.time + punchCooldown;
                Debug.Log("JAB DETECTED!");
                detectingJab = false;
            }
        }
    }

    // close serial when app is closed
    private void OnApplicationQuit()
    {
        serial.Close();
    }
}
