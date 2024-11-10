using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LineRendererTimestampKeeper : MonoBehaviour
{
    private float[] m_timestamps = new float[4];
    private int index=0;



    public void setTimestamp(float timestamp)
    {
        if (m_timestamps == null) {
            Debug.Log("WTF Happened here?");
            m_timestamps = new float[4];
            index = 0;
        }
        if (index < m_timestamps.Length)
        {
            m_timestamps[index] = timestamp;
            index++;
        }
        else
        {
            Array.Resize<float>(ref m_timestamps, index + 10);
            m_timestamps[index] = timestamp;
            index++;
        }
    }

    public void setTimestamp(int position, float timestamp) {
        if (position < m_timestamps.Length)
        {
            m_timestamps[position] = timestamp;
            index++;
        }
        else {
            Array.Resize<float>(ref m_timestamps, position+10);
            m_timestamps[position] = timestamp;
            index++;
        }
    }

    public void setTimestamps(float[] timestamps) {
            Array.Resize<float>(ref m_timestamps, timestamps.Length);
        Array.Copy(timestamps, m_timestamps, timestamps.Length);
    }

    public float[] getTimestamps() {
        return m_timestamps;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
