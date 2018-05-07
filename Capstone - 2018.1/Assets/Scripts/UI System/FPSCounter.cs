using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour {

    public int AverageFPS { get; private set; }
    public int HighestFPS { get; private set; }
    public int LowestFPS { get; private set; }

    public int frameRange = 60;

    int[] fpsBuffer;
    int fpsBufferIndex;

    //Initalizes FPS buffer for counting frames.
    void InitializeBuffer()
    {
        if (frameRange <= 0)
        {
            frameRange = 1;
        }
        fpsBuffer = new int[frameRange];
        fpsBufferIndex = 0;
    }

    private void Update()
    {   
        if (fpsBuffer == null || fpsBuffer.Length != frameRange)
        {
            InitializeBuffer();
        }
        UpdateBuffer();
        CalculateFPS();
    }

    //Updates frame buffer count and wraps for your convenience.
    void UpdateBuffer ()
    {
        fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);

        if (fpsBufferIndex >= frameRange)
            fpsBufferIndex = 0;
    }

    //Calculates the current average framerate, based on FrameRange. Also tracks highest and lowest fps.
    void CalculateFPS()
    {
        int sum = 0;
        int highest = 0;
        int lowest = int.MaxValue;

        for (int i = 0; i < frameRange; i++)
        {
            int fps = fpsBuffer[i];

            sum += fps;

            if (fps > highest)
            { highest = fps; }

            if (fps < lowest)
            { lowest = fps; }
        }

        AverageFPS = sum / frameRange;
        HighestFPS = highest;
        LowestFPS = lowest;
    }
}
