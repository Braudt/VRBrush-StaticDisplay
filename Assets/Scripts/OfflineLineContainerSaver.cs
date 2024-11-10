using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class OfflineLineContainerSaver : MonoBehaviour
{
    public GameObject lineRendererPrefab;
    [Serializable]
    struct LineContainerParameters {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public OfflineLineRendererSaver.LineParameters[] lineParameters;
    }


    public void SaveArtwork()
    {
        string filename = "artwork-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".data";
        string saveFile = Application.persistentDataPath + "/" + filename; // artwork-" + DateTime.Now.ToString("yyyyMMddHHmmss")+".data";

        LineContainerParameters lcp = new LineContainerParameters();
        lcp.position = transform.localPosition;
        lcp.rotation = transform.localRotation;
        lcp.scale = transform.localScale;
        LineRenderer[] children = transform.GetComponentsInChildren<LineRenderer>();
        lcp.lineParameters = new OfflineLineRendererSaver.LineParameters[children.Length];

        for(int i=0; i<lcp.lineParameters.Length; i++)
        {
            lcp.lineParameters[i] = new OfflineLineRendererSaver.LineParameters();
            GameObject childObject= children[i].gameObject;
            OfflineLineRendererSaver saver = childObject.GetComponent<OfflineLineRendererSaver>();
            lcp.lineParameters[i] = saver.SaveLineParameters();
            string test = JsonUtility.ToJson(lcp.lineParameters[i], true);
            Debug.Log(test);

        }
        Debug.Log(lcp.lineParameters.ToString());
        string json = JsonUtility.ToJson(lcp);
        Debug.Log(json);
        File.WriteAllText(saveFile, json);
        File.Copy(saveFile, Application.persistentDataPath + "/artwork-latest.data", true);
    }

    public void ClearExistingArtwork() {
        LineRenderer[] children = transform.GetComponentsInChildren<LineRenderer>();
        foreach (LineRenderer child in children)
        {
            Destroy(child.gameObject);
        }
    }

    public void LoadArtwork(string saveFile, bool replay) {
        Debug.Log("Load " + saveFile);

        ClearExistingArtwork();
        string json = File.ReadAllText(saveFile); ;
        LoadArtworkFromString(json, replay);  

    }

    public void LoadArtworkFromString(string json, bool replay) {
        LineContainerParameters lcp = JsonUtility.FromJson<LineContainerParameters>(json);
        transform.localPosition  = lcp.position;
        transform.localRotation = lcp.rotation;
        transform.localScale = lcp.scale;
        OfflineLineRendererSaver.LineParameters[] set = lcp.lineParameters;
        foreach (OfflineLineRendererSaver.LineParameters lineParameters in set)
        {
            GameObject renderer = Instantiate(lineRendererPrefab);
            Debug.Log(set.Length + " + " + renderer.name);
            //renderer.GetComponent<NetworkObject>().Spawn(
            //renderer.GetComponent<NetworkObject>().enabled = false;
            //if (DrawSettings.Singleton.gameMode == DrawSettings.Mode.VR)
            //{ 
            if (transform != null)
            {
                Debug.Log("not Null");
            }
            renderer.transform.parent = transform;
            //}
            if (replay)
            {
                renderer.GetComponent<OfflineLineRendererSaver>().ReplayLineParameters(lineParameters);
            }
            else
            {
                renderer.GetComponent<OfflineLineRendererSaver>().RestoreLineParameters(lineParameters);
            }
        }
    }


    public void LoadLatestArtwork() {
       
        LoadArtwork(Application.persistentDataPath + "/artwork-latest.data", false);
    }

    public void ReplayLatestArtwork()
    {

        LoadArtwork(Application.persistentDataPath + "/artwork-latest.data", true);
    }

    public void StopReplay() {
        LineRenderer[] children = transform.GetComponentsInChildren<LineRenderer>();
        foreach (LineRenderer child in children)
        {
            child.gameObject.GetComponent<OfflineLineRendererSaver>().StopAllCoroutines();
            Destroy(child.gameObject);
        }
    }

}
