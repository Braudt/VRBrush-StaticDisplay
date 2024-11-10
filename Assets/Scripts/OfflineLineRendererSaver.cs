using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class OfflineLineRendererSaver : MonoBehaviour
{
    [Serializable]
    public struct LineParameters {
        public Vector3 position;
        public Quaternion rotation;
        public float lineWidth;
        public Color color;
        public int materialIndex;
        public int numCornerVertices;
        public int numCapVertices;
        public LineAlignment alignment;
        public int positionCount;
        public Vector3[] positions;
        public float[] timestamps;
    }

    private LineRenderer lr;
    private LineRendererTimestampKeeper lrtk;



    // Start is called before the first frame update
    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lrtk = GetComponent<LineRendererTimestampKeeper>();
    }

    public LineParameters SaveLineParameters() {
        LineParameters parameters = new LineParameters(); ;
        parameters.position = transform.localPosition; parameters.rotation = transform.localRotation;
        parameters.lineWidth = lr.startWidth; 
        parameters.color = lr.startColor;
        parameters.materialIndex = OfflineDrawSettings.Singleton.getDrawingMaterialIndex(lr.sharedMaterial);
        parameters.numCornerVertices = lr.numCornerVertices;
        parameters.numCapVertices = lr.numCapVertices;
        parameters.alignment = lr.alignment;
        parameters.positionCount = lr.positionCount;
        parameters.positions = new Vector3[parameters.positionCount];
        parameters.timestamps = new float[parameters.positionCount];

        Debug.Log(lrtk.getTimestamps().Length);
        Debug.Log(parameters.positionCount);
        Array.Copy(lrtk.getTimestamps(), parameters.timestamps, parameters.positionCount);

        //TODO: Check on the network version that we do it that way
        lr.GetPositions(parameters.positions);
        return parameters;
    }

    public void RestoreLineParameters(LineParameters parameters) {
        transform.localPosition = parameters.position;
        transform.localRotation = parameters.rotation;
        lr.startWidth = parameters.lineWidth;
        lr.endWidth = parameters.lineWidth;
        lr.startColor = parameters.color;
        lr.endColor = parameters.color;
        lr.sharedMaterial = OfflineDrawSettings.Singleton.getDrawingMaterialFromIndex(parameters.materialIndex);
        lr.numCornerVertices = parameters.numCornerVertices;
        lr.numCapVertices= parameters.numCapVertices;
        lr.positionCount = parameters.positionCount;
        lr.SetPositions(parameters.positions);

        lrtk.setTimestamps(parameters.timestamps);

    }

    private IEnumerator setPositionAtTime(LineRenderer lr, int positionCount, Vector3 position, float timestamp) {
        yield return new WaitForSeconds(timestamp);
        lr.positionCount = positionCount;
        lr.SetPosition(positionCount-1, position);
    }

    public void ReplayLineParameters(LineParameters parameters)
    {
        transform.localPosition = parameters.position;
        transform.localRotation = parameters.rotation;
        lr.startWidth = parameters.lineWidth;
        lr.endWidth = parameters.lineWidth;
        lr.startColor = parameters.color;
        lr.endColor = parameters.color;
        lr.sharedMaterial = OfflineDrawSettings.Singleton.getDrawingMaterialFromIndex(parameters.materialIndex);
        lr.numCornerVertices = parameters.numCornerVertices;
        lr.numCapVertices = parameters.numCapVertices;
        //lr.positionCount = parameters.positionCount;

        for (int i = 0; i < parameters.positionCount; i++) {
            IEnumerator coroutine = setPositionAtTime(lr, i+1, parameters.positions[i], parameters.timestamps[i]);
            StartCoroutine(coroutine);
        }

    }

    public void StopReplay() { 
        StopAllCoroutines();
    }

}
