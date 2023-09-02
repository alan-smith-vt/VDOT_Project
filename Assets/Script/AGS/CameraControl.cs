using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using static UnityEngine.Mathf;
using Vuforia;

public class CameraControl : MonoBehaviour
{
    GameObject m_Canvas = null;
    Renderer m_CanvasRenderer = null;
    PhotoCapture m_PhotoCaptureObj;
    CameraParameters m_CameraParameters;
    bool m_CapturingPhoto = false;
    Texture2D m_Texture = null;
    LAV_InteractionManager interactionManager;

    [SerializeField] GameObject mainCamera;
    [SerializeField] Shader holographicShader;

    public int numberPictures = 0;

    private void Awake() 
    {
        UIManager.Singleton.AddToLog("CameraControl is awake.");
    }

    private void Start() {
        UIManager.Singleton.AddToLog("CameraControl 'Start' Function triggered.");
        interactionManager = GameObject.Find("FaultInteraction").GetComponent<LAV_InteractionManager>();
    }

    public void ReleaseCamera()
    {
        m_PhotoCaptureObj.StopPhotoModeAsync(OnStopPhotoMode);
    }

    void OnStopPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        Debug.Log("Released the camera.");
        m_PhotoCaptureObj.Dispose();
        m_PhotoCaptureObj = null;
        if (VuforiaBehaviour.Instance == null)
        {
            UIManager.Singleton.AddToLog("Vuforia Instance is null, cannot re-enable.");
        }
        else
        {
            UIManager.Singleton.AddToLog("Vuforia instance is not null. Re-enabling (or trying to)");
            //PERMA DISABLING VURFORIA AFTER LINE ALIGNMENT
            //VuforiaBehaviour.Instance.enabled = true;
        }
    }

    public void Initialize() {
        UIManager.Singleton.AddToLog("Initializing...");

        if (VuforiaBehaviour.Instance != null)
        {
            UIManager.Singleton.AddToLog("Disabling Vuforia instance.");
            VuforiaBehaviour.Instance.enabled = false;
        }
        
        List<Resolution> resolutions = new List<Resolution>(PhotoCapture.SupportedResolutions);
        Resolution selectedResolution = resolutions[0];

        m_CameraParameters = new CameraParameters(WebCamMode.PhotoMode);
        m_CameraParameters.cameraResolutionWidth = selectedResolution.width;
        m_CameraParameters.cameraResolutionHeight = selectedResolution.height;
        UIManager.Singleton.AddToLog($"Selected resolution: {selectedResolution.width}, {selectedResolution.height}");
        m_CameraParameters.hologramOpacity = 0.0f;
        m_CameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

        m_Texture = new Texture2D(selectedResolution.width, selectedResolution.height, TextureFormat.BGRA32, false);

        PhotoCapture.CreateAsync(false, OnCreatedPhotoCaptureObject);
    }

    void OnCreatedPhotoCaptureObject(PhotoCapture captureObject) {
        UIManager.Singleton.AddToLog("onPhotocaptureObject called");
        m_PhotoCaptureObj = captureObject;
        m_PhotoCaptureObj.StartPhotoModeAsync(m_CameraParameters, OnStartPhotoMode);
    }

    void OnStartPhotoMode(PhotoCapture.PhotoCaptureResult result) {

        UIManager.Singleton.AddToLog("Photo Mode Ready!");
    }

    void OnPhotoCaptured(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        numberPictures++;
        if (m_Canvas == null)
        {
            m_Canvas = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_Canvas.name = "PhotoCaptureCanvas";
            m_CanvasRenderer = m_Canvas.GetComponent<Renderer>() as Renderer;
            m_CanvasRenderer.material = new Material(holographicShader);
        }


        Matrix4x4 cameraToWorldMatrix;
        photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);
        Debug.Log($"world matrix: {cameraToWorldMatrix}");


        Matrix4x4 worldToCameraMatrix = cameraToWorldMatrix.inverse;

        Matrix4x4 projectionMatrix;
        photoCaptureFrame.TryGetProjectionMatrix(out projectionMatrix);

        if (Application.isEditor)
        {
            cameraToWorldMatrix = Camera.main.cameraToWorldMatrix;
            projectionMatrix = Camera.main.projectionMatrix;
        }



        photoCaptureFrame.UploadImageDataToTexture(m_Texture);
        m_Texture.wrapMode = TextureWrapMode.Clamp;

        m_CanvasRenderer.sharedMaterial.SetTexture("_MainTex", m_Texture);
        m_CanvasRenderer.sharedMaterial.SetMatrix("_WorldToCameraMatrix", worldToCameraMatrix);
        m_CanvasRenderer.sharedMaterial.SetMatrix("_CameraProjectionMatrix", projectionMatrix);
        m_CanvasRenderer.sharedMaterial.SetFloat("_VignetteScale", 0.0f);


        // Position the canvas object slightly in front
        // of the real world web camera.
        Vector3 position = cameraToWorldMatrix.GetColumn(3) - cameraToWorldMatrix.GetColumn(2);

        // Rotate the canvas object so that it faces the user.
        Quaternion rotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));

        m_Canvas.transform.position = position;
        m_Canvas.transform.rotation = rotation;

        m_Canvas.SetActive(false);

        byte[] bytes = m_Texture.EncodeToJPG(90);
        UIManager.HololensBytesJPG = bytes;

        UIManager.Singleton.SaveCameraMatrix(cameraToWorldMatrix, projectionMatrix, m_Canvas);

        #region Debugging for canvas position
        /*
        float numOrbs = 5;
        int imageWidth = 3904;
        int imageHeight = 2196;
        Vector3 cameraPos = cameraToWorldMatrix.GetColumn(3);
        
        UIManager.Singleton.SpawnOrbSize(cameraPos, Color.white, 0.01f);

        for (float i = -0.5f; i <= 0.5f; i = i + 1 / numOrbs)
        {
            for (float j = -0.5f; j <= 0.5f; j = j + 1 / numOrbs)
            {
                Vector3 res = m_Canvas.transform.TransformPoint(new Vector3(i, j, 0));
                //UIManager.Singleton.RaycastLine(cameraPos, res, Color.cyan);
                UIManager.Singleton.SpawnOrbSize(res, Color.cyan, 0.01f);
            }
        }


        float redDecimal = 0.5f * 2196f / 3904f;
        Vector2[] cornersRed = new Vector2[]
        {
            new Vector2(-0.5f,-redDecimal),
            new Vector2(-0.5f,redDecimal),
            new Vector2(0.5f,-redDecimal),
            new Vector2(0.5f,redDecimal)
        };

        float greenDecimal = 0.5f - 0.1f;
        Vector2[] cornersGreen = new Vector2[]
        {
            new Vector2(-0.5f,-greenDecimal),
            new Vector2(-0.5f,greenDecimal),
            new Vector2(0.5f,-greenDecimal),
            new Vector2(0.5f,greenDecimal)
        };

        float magentaDecimalTop = greenDecimal - 0.05f;
        float magentaDecimalBot = greenDecimal - 0.01f;
        Vector2[] cornersMagenta = new Vector2[]
        {
            new Vector2(-0.5f,-magentaDecimalBot),
            new Vector2(-0.5f,magentaDecimalTop),
            new Vector2(0.5f,-magentaDecimalBot),
            new Vector2(0.5f,magentaDecimalTop)
        };

        for (int i = 0; i < cornersRed.Length; i++)
        {
            Vector3 targetPoint = m_Canvas.transform.TransformPoint(new Vector3(cornersRed[i][0], cornersRed[i][1], 0));
            targetPoint = m_Canvas.transform.TransformPoint(new Vector3(cornersGreen[i][0], cornersGreen[i][1], 0));
            targetPoint = m_Canvas.transform.TransformPoint(new Vector3(cornersMagenta[i][0], cornersMagenta[i][1], 0));
            UIManager.Singleton.SpawnOrbSize(targetPoint, Color.magenta, 0.01f);
        }
        Debug.Log($"Render width: {(float)m_CanvasRenderer.material.mainTexture.width}");
        Debug.Log($"Render height: {(float)m_CanvasRenderer.material.mainTexture.height}");
        */
        #endregion
        m_CapturingPhoto = false;
    }
    public void TakePhoto()
    {
        bool res = (m_CapturingPhoto || interactionManager.InteractionMethodChosen == LAV_InteractionManager.InteractionMethod.Manual_Only);
        Debug.Log($"Taking photo maybe {res}");

        //Don't take photo if manualOnly
        if (m_CapturingPhoto || interactionManager.InteractionMethodChosen == LAV_InteractionManager.InteractionMethod.Manual_Only)
        {
            return;
        }

        //TODO add delay and photo taking animation stuff
        m_CapturingPhoto = true;
        UIManager.Singleton.AddToLog("Taking picture...");
        m_PhotoCaptureObj.TakePhotoAsync(OnPhotoCaptured);
        Debug.Log("Taking photo (actually though).");
    }

    public void TakeDelayedPhoto()
    {
        StartCoroutine(delayPhoto());
    }

    IEnumerator delayPhoto()
    {
        yield return new WaitForSeconds(3f);
        if (m_CapturingPhoto)
        {
            yield break;
        }

        m_CapturingPhoto = true;
        UIManager.Singleton.AddToLog("Taking picture...");
        m_PhotoCaptureObj.TakePhotoAsync(OnPhotoCaptured);
    }
}