using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CameraPoseManager : MonoBehaviour
{
    public Camera camera;
    public Canvas controlCanvas;

    public Canvas webcamCanvas;

    private RawImage webcamImage;
    private TMPro.TMP_Dropdown dropdown;


    private bool activeOverlay;


    // Saving Camera Pose
    [Serializable]
    struct CameraSettings
    {
        public Vector3 position;
        public Quaternion rotation;
        public float FoV;
    }
    CameraSettings cameraSettings;
    private string saveFile;


    // Camera Motion
    public float moveSpeed = 5.0f;
    public float mouseSensitivity = 2.0f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private int movementMode = 0;


    //Webcam

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









    // Start is called before the first frame update
    void Start()
    {
        cameraSettings = new CameraSettings();
        saveFile = Application.persistentDataPath + "/cameraSettings.data";
        activeOverlay = true;
        controlCanvas.gameObject.SetActive(activeOverlay);
        dropdown = controlCanvas.gameObject.GetComponentInChildren<TMPro.TMP_Dropdown>();
        webcamImage = webcamCanvas.gameObject.GetComponentInChildren<RawImage>();
        Debug.Log("Canvas: "+webcamImage.gameObject.name);


        
        devices = WebCamTexture.devices;

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

        }
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
        webcamImage.texture = rt; // webcamTexture;
        webcamTexture.Play();

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

    public void toggleCanvas() { 
        activeOverlay=!activeOverlay;
        controlCanvas.gameObject.SetActive(activeOverlay);

    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            toggleCanvas();
        }


        string axisX;
        string axisY;
        axisX = "Mouse X";
        axisY = "Mouse Y";
        bool rotated = false;

        if (!activeOverlay)
        {

            if (movementMode == 0)
            {


                rotationX += Input.GetAxis(axisX) * mouseSensitivity;
                rotationY += Input.GetAxis(axisY) * mouseSensitivity;


                if (rotated)
                {


                    //rotationX = Mathf.Clamp(rotationX, -90.0f, 90.0f);
                    camera.transform.localRotation = Quaternion.AngleAxis(rotationY, -Vector3.up);
                    camera.transform.localRotation *= Quaternion.AngleAxis(rotationX, Vector3.left);
                    camera.transform.localRotation *= Quaternion.Euler(0, 0, 90);


                    float forwardMovement = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
                    float strafeMovement = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
                    camera.transform.position += camera.transform.forward * forwardMovement;
                    camera.transform.position += camera.transform.right * strafeMovement;
                }
                else
                {
                    rotationY = Mathf.Clamp(rotationY, -90.0f, 90.0f);
                    camera.transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
                    camera.transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);



                    float forwardMovement = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
                    float strafeMovement = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
                    camera.transform.position += camera.transform.forward * forwardMovement;
                    camera.transform.position += camera.transform.right * strafeMovement;
                }

            }
            else
            {
                if (rotated)
                {
                    float mouseX = Input.GetAxis(axisX) * mouseSensitivity / 2;
                    float mouseY = Input.GetAxis(axisY) * mouseSensitivity / 2;

                    float forwardMovement = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
                    float strafeMovement = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
                    camera.transform.position += camera.transform.up * mouseY;
                    camera.transform.position += camera.transform.right * mouseX;
                    camera.transform.position += camera.transform.forward * forwardMovement;
                    camera.transform.position += camera.transform.right * strafeMovement;
                }
                else
                {
                    float mouseX = Input.GetAxis(axisX) * mouseSensitivity / 2;
                    float mouseY = Input.GetAxis(axisY) * mouseSensitivity / 2;

                    float forwardMovement = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
                    float strafeMovement = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
                    camera.transform.position += Vector3.up * mouseY;
                    camera.transform.position += camera.transform.right * mouseX;
                    camera.transform.position += camera.transform.forward * forwardMovement;
                    camera.transform.position += camera.transform.right * strafeMovement;
                }
            }
            if (Input.GetKeyUp(KeyCode.P))
            {
                camera.fieldOfView += 1;
            }
            else if (Input.GetKeyUp(KeyCode.O))
            {
                camera.fieldOfView -= 1;
            }

            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                movementMode = (movementMode + 1) % 2;
                Debug.Log("Movement");
            }
            if ((Mathf.Abs(Input.mouseScrollDelta.y) > 0.1f))
            {
                setScale(new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height), (int)Input.mouseScrollDelta.y);
                SetOffsetBoundaries();
            }
        }
        Graphics.Blit(webcamTexture, rt, scale, offset);

    }

    public void SavePose() { 
        cameraSettings.position = camera.transform.position;
        cameraSettings.rotation = camera.transform.rotation;
        cameraSettings.FoV = camera.fieldOfView;
        string json = JsonUtility.ToJson(cameraSettings);
        File.WriteAllText(saveFile, json);
    }

    public void LoadPose()
    {
        string json = File.ReadAllText(saveFile); ;
        cameraSettings = JsonUtility.FromJson<CameraSettings>(json);
        camera.transform.position = cameraSettings.position;
        camera.transform.rotation = cameraSettings.rotation;
        camera.fieldOfView = cameraSettings.FoV;
        rotationX = camera.transform.rotation.eulerAngles.y; 
        rotationY = camera.transform.rotation.eulerAngles.x > 180 ? -(camera.transform.rotation.eulerAngles.x - 360) : -camera.transform.rotation.eulerAngles.x;

        //rotationY = -camera.transform.rotation.eulerAngles.x;
    }

}
