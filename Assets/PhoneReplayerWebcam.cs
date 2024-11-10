using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneReplayerWebcam : MonoBehaviour
{
    [SerializeField]
    public RawImage displayImage;
    [SerializeField]
    public TMPro.TMP_Dropdown dropdown;
    [SerializeField]
    public Canvas UICanvas;
    
    RenderTexture rt;
    WebCamTexture webcamTexture;
    WebCamDevice[] devices;



    float zoom;
    Vector2 scale;
    Vector2 baseScale;
    Vector2 offset;
    Vector2 baseOffset;

    public int webcamWidth;
    public int webcamHeight;

    private string pictureFolder;
    private string companyName = "RainbowCatXR";

    private int lastWidth;
    private int lastHeight;
    private float resizeCheck;
    private float lastTime;

    // Start is called before the first frame update
    void Start()
    {
        devices = WebCamTexture.devices;

        lastWidth = Screen.width;
        lastHeight = Screen.height;
        resizeCheck = 1.0f;
        lastTime = Time.time;

        zoom = 0;
        baseScale = Vector2.one;

        scale = Vector2.one;


        offset = Vector2.zero;
        baseOffset = Vector2.zero;

        if (devices.Length > 0)
        {
            foreach (WebCamDevice device in devices)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = device.name;
                dropdown.options.Add(optionData);
            }

            dropdown.onValueChanged.AddListener(delegate { DropDownValueChanged(dropdown); });
            dropdown.value = 0;
            DropDownValueChanged(dropdown);
            dropdown.RefreshShownValue();


            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                string pictureLibrary = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                if (System.IO.Directory.Exists(pictureLibrary))
                {
                    pictureFolder = Path.Combine(pictureLibrary, "RainbowCatXR");
                }
                else
                {
                    pictureFolder = "RainbowCatXR";
                }
                System.IO.Directory.CreateDirectory(pictureFolder);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                pictureFolder = "RainbowCatXR";
            }


        }
        // TODO: Placeholder if no device detected


    }

    void DropDownValueChanged(TMP_Dropdown change)
    {
        Debug.Log(change.value);
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }

        Destroy(webcamTexture);
        //webcamTexture = new WebCamTexture();
        webcamTexture = new WebCamTexture(webcamWidth, webcamHeight, 30);
        webcamTexture.deviceName = devices[change.value].name;
        Debug.Log(devices[change.value].kind);
        rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        displayImage.texture = rt; // webcamTexture;
        webcamTexture.Play();
        //float screenRatio = Screen.width/ Screen.height;
        //float webcamRatio = webcamTexture.width / webcamTexture.height;

        Resized();

        Debug.Log(webcamTexture.width.ToString());
    }

    public void Resized()
    {
        float widthRatio = (float)Screen.width / (float)webcamTexture.width;
        float heightRatio = (float)Screen.height / (float)webcamTexture.height;

        if (widthRatio > heightRatio)
        {
            Debug.Log("Case 1");
            // Scale with height 
            baseScale.x = 1;
            baseScale.y = heightRatio / widthRatio;
            baseOffset.x = 0;//(1 - baseScale.x)/2;
            baseOffset.y = (1 - baseScale.y) / 2;
            scale = baseScale;
            offset = baseOffset;
            Debug.Log(baseScale);
            Debug.Log(offset);

        }
        else
        {
            Debug.Log("Case 2");
            // Scale with height 
            baseScale.x = widthRatio / heightRatio;
            baseScale.y = 1;
            baseOffset.x = (1 - baseScale.x) / 2;
            baseOffset.y = 0;// (1 - baseScale.y) / 2;
            scale = baseScale;
            offset = baseOffset;
            Debug.Log(baseScale);
            Debug.Log(offset);
        }
    }

    /*
    public Texture2D toTexture2D(Texture rTex)
    {
        RenderTexture photoTexture = new RenderTexture(webcamWidth, webcamHeight, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(rTex, photoTexture);
        Texture2D dest = new Texture2D(photoTexture.width, photoTexture.height, TextureFormat.RGBA32, false);
        dest.Apply(false);
        Graphics.CopyTexture(rt, dest);
        dest.Apply(true);
        //Texture2D dest = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        //dest.Apply(false);
        //Graphics.CopyTexture(rTex, dest);
        return dest;
    }

    public Texture2D toTexture2D(Texture rTex, Rect uvRect)
    {
        Texture2D dest = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        dest.Apply(false);
        Graphics.CopyTexture(rTex, dest);
        Texture2D dest_cropped = new Texture2D((int)(rTex.width*uvRect.width), (int)uvRect.height, TextureFormat.RGB24, false);
        
        return dest;
    }*/



    public void setScale(Vector2 centre, int zoomincr)
    {
        zoom += zoomincr;
        if (zoom >= 0)
        {

            scale.x = baseScale.x * (1f - zoom / 10.0f);
            scale.y = baseScale.y * (1f - zoom / 10.0f);
            offset.x += (1 - baseOffset.x * 2) * (float)zoomincr / 20.0f;
            offset.y += (1 - baseOffset.y * 2) * (float)zoomincr / 20.0f;
            Debug.Log(offset);
        }
        else
        {
            zoom = 0;
        }
    }

    public void SetOffsetBoundaries()
    {
        if (offset.x < 0)
            offset.x = 0;
        else if (offset.x > baseOffset.x * 2 + (float)zoom / 10f)
            offset.x = baseOffset.x * 2 + (float)zoom / 10f;

        if (offset.y < 0)
            offset.y = 0;
        else if (offset.y > baseOffset.y * 2 + (float)zoom / 10f)
            offset.y = baseOffset.y * 2 + (float)zoom / 10f;
    }

    public void ZoomIn()
    {
        Vector2 centre = new Vector2(0.5f, 0.5f);
        setScale(centre, 1);
    }
    public void ZoomOut()
    {
        Vector2 centre = new Vector2(0.5f, 0.5f);
        setScale(centre, -1);
    }

    bool dragging = false;
    Vector2 mousePos = Vector2.zero;

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastTime > resizeCheck)
        {
            Debug.Log("ResizeCheck!");
            lastTime = Time.deltaTime;
            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                Resized();
                lastWidth = Screen.width;
                lastHeight = Screen.height;
            }

        }
        /*
        if (Input.GetMouseButtonDown(0) && dragging == false)
        {
            dragging = true;
            mousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0) && dragging == true)
        {
            dragging = false;
        }
        if (dragging == true)
        {
            float displacementx = (Input.mousePosition.x - mousePos.x) / (Screen.width) * (1.0f - zoom / 20f * 2);
            offset.x = offset.x - (1 - baseOffset.x * 2) * displacementx;

            float displacementy = (Input.mousePosition.y - mousePos.y) / (Screen.height) * (1.0f - zoom / 20f * 2);
            offset.y = offset.y - (1 - baseOffset.y * 2) * displacementy;

            SetOffsetBoundaries();

            mousePos = Input.mousePosition;
        }
        else
        {
        */
            if ((Mathf.Abs(Input.mouseScrollDelta.y) > 0.1f))
            {
                setScale(new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height), (int)Input.mouseScrollDelta.y);
                SetOffsetBoundaries();
            }

        //}

        if(Input.GetKeyUp(KeyCode.Escape))
        {
            UICanvas.gameObject.SetActive(!UICanvas.gameObject.activeSelf);
        }



        // TODO: Need to handle absurd screen formats
        Graphics.Blit(webcamTexture, rt, scale, offset);
    }

    private void OnDisable()
    {
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }

        Destroy(webcamTexture);
    }

    private void OnEnable()
    {
        dropdown.value = 0;
        DropDownValueChanged(dropdown);
        dropdown.RefreshShownValue();
    }
}