
using System.IO;
using System;
using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;

/**
 * This script is optional and can be used to show Ping rate for accessing Network Lag
 * It will also present the Average Frame rate per second. 
 * Within the Unity Editor, this will runs as fast as it can given the processingm power.
 * Within an executable, the frame rate is tied to the screen refresh rate, 60hz will be 60 fps.
 * It will also show the current build date and time, but only after a build.
 
 */
public class SF2X_Stats : MonoBehaviour
{
    private int AverageFPS { get; set; }
    private int[] fpsBuffer;
    private int fpsBufferIndex;
    private int frameRange = 120;
    public TMP_Text frameRate;
    public TMP_Text pingRate;
    public TMP_Text version;

    private void Start()
    {
        if (Resources.Load<TextAsset>("BuildDate"))
        {
            string dt = Resources.Load<TextAsset>("BuildDate").text;

            const string regex = "";

            string sanitised = Regex.Replace(dt, regex, string.Empty);

            version.SetText("Build: " + sanitised);
        }


    }

    void Update()
    {
        if (fpsBuffer == null || fpsBuffer.Length != frameRange)
        {
            InitializeBuffer();
        }
        UpdateBuffer();
        CalculateFPS();
        pingRate.SetText(SF2X_GameManager.Instance.clientServerLag.ToString() + " ms Network Lag");

    }

    void InitializeBuffer()
    {
        if (frameRange <= 0)
        {
            frameRange = 1;
        }
        fpsBuffer = new int[frameRange];
        fpsBufferIndex = 0;
    }

    void UpdateBuffer()
    {
        fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
        if (fpsBufferIndex >= frameRange)
        {
            fpsBufferIndex = 0;
        }
    }
    void CalculateFPS()
    {
        int sum = 0;
        for (int i = 0; i < frameRange; i++)
        {
            sum += fpsBuffer[i];
        }
        AverageFPS = sum / frameRange;
        frameRate.SetText(AverageFPS.ToString() + " fps for Device");
    }
  

}
