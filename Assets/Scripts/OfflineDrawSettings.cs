using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class OfflineDrawSettings : MonoBehaviour
{

    public static OfflineDrawSettings Singleton { get; private set; }



    public enum BrushTypes
    {
        Discrete,
        Continuous,
        Origin
    }

    [HideInInspector]
    public float firstTimeStamp;




    // Serialize content
    public Material[] drawingMaterialList;
    [HideInInspector]
    public int drawingMaterialIndex; //= new NetworkVariable<int>();
    public GameObject[] drawingPrefabList;


    public Material getDrawingMaterial() { 
        return drawingMaterialList[drawingMaterialIndex];
    }

    public void setDrawingMaterial(Material mat)
    {
        drawingMaterialIndex = getDrawingMaterialIndex(mat);
    }

    public Material getDrawingMaterialFromIndex(int index)
    {
        return drawingMaterialList[index];
    }

    public int getDrawingMaterialIndex(Material mat)
    {
        for (int i = 0; i < drawingMaterialList.Length; i++)
        {
            if (drawingMaterialList[i].name == mat.name)
            {
                return i;
            }
        }
        return 0;
    }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Singleton != null && Singleton != this)
        {
            Destroy(this);
        }
        else
        {
            Singleton = this;
        }
    }

    private void Start()
    {
        firstTimeStamp = 0;

         drawingMaterialIndex = 0;
        


    }


    


}

