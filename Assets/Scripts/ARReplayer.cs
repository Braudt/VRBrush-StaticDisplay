using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARReplayer : MonoBehaviour
{
    public TextAsset artwork;
    public bool startHidden;

    public void Start()
    {
        /*
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
            "127.0.0.1",
            (ushort)12345,
            "127.0.0.1");
        NetworkManager.Singleton.StartServer();
        */
        GameObject lc = transform.Find("Line Container").gameObject;//Find("Line Container");
        Debug.Log("Loading Artwork");
        lc.GetComponent<OfflineLineContainerSaver>().ClearExistingArtwork();
        lc.GetComponent<OfflineLineContainerSaver>().LoadArtworkFromString(artwork.text, false);
        lc.layer = 8;
        foreach (Transform child in lc.transform)
        {
            child.gameObject.layer = 8;
        }
        if (startHidden)
        {
            lc.SetActive(false);
        }

    }

    public void LocalisationSuccess() {
        GameObject lc = transform.Find("Line Container").gameObject;//Find("Line Container");
        lc.SetActive(true);
    }

    public void TrackingLost() {
        GameObject lc = transform.Find("Line Container").gameObject;//Find("Line Container");
        lc.SetActive(false);
    }
}
