using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.Mathf;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;

    private static string[] logMessages = new string[10];
    private static string fullMessage = "";
    private static byte[] textureBytes;
    private static int textureWidth;
    private static int textureHeight;
    private static bool[] texturePackageManifest;
    private static int packageSizeLocal;

    public static byte[] HololensBytesJPG;
    public static byte[] DistortedHololensBytesJPG;
    public static float[] NodeList;
    public static int extents = 0;

    public static List<List<decimal[]>> nodeList;

    public static int maxUpdates = 200;
    public static int updateCount = 0;

    public static GameObject meshObject;
    public static GameObject verticalOrb;
    public static GameObject horizontalOrb;
    public static GameObject centerOrb;

    public static int UI_gameState = 0; //0 = no image, 1 = image with physical orbs, 2 = image with image orbs

    public static Matrix4x4 cameraToWorldMatrix;
    public static Matrix4x4 projectionMatrix;
    public static GameObject m_Canvas;
    public static cameraParameters camParams;

    public bool defectCreated = false;

    public static bool planeOverrideFlag = false;
    Vector3[] orbCoordinates = new Vector3[3];

    private AGS_Interaction_CV interaction_CV;
    private LAV_InteractionManager interactionManager;
    private GameObject faultInteractionObject;
    private GameObject faultPrefab;
    private Transform buttonParent;
    private Transform titleBar;


    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    [Header("Connect")]
    [SerializeField] private Text logWindow;
    [SerializeField] public RawImage displayImage;
    [SerializeField] public GameObject grippyOrbPrefab;
    [SerializeField] public GameObject endNodePrefab;
    [SerializeField] public Material material;
    [SerializeField] public GameObject meshPrefab;
    [SerializeField] public GameObject toggleButtonPrefab;
    [SerializeField] public GameObject buttonPrefab;


    public bool toggleSplineFlag = true;
    public GameObject addOrbButton;
    public bool startedFlag = false;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        interaction_CV = GameObject.Find("FaultInteraction").GetComponent<AGS_Interaction_CV>();
        faultInteractionObject = GameObject.Find("FaultInteraction");//For disabling finger collider
        faultPrefab = GameObject.Find("FaultCollection").transform.Find("Fault").Find("FaultPoint").gameObject;//For disabling orb colliders
        interactionManager = GameObject.Find("FaultInteraction").GetComponent<LAV_InteractionManager>();

        //buttonParent = GameObject.Find("UI").transform.Find("TitleBar").Find("Buttons");
        //titleBar = GameObject.Find("UI").transform.Find("TitleBar");
    }

    #region Basic UI controls

    //Server coms check
    public void Sendtest()
    {
        StartCoroutine(NetworkManager.Singleton.ComCheck());
    }

    public void ViewImage()//Local
    {
        AddToLog("Render local image.");
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(HololensBytesJPG); //..this will auto-resize the texture dimensions.
        tex.Apply();
        displayImage.texture = tex;
    }

    private void startButtons()
    {
        /*
        if (titleBar != null)
        {
            titleBar.Find("BackPlate (2)").gameObject.SetActive(false);
            titleBar.Find("BackPlate (1)").gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("Can't find titlebar.");
        }
        
        if (buttonParent != null)
        {
            buttonParent.Find("B. Start").gameObject.SetActive(true);
            buttonParent.Find("B. Manual").gameObject.SetActive(false);
            buttonParent.Find("B. CV_only").gameObject.SetActive(false);
            buttonParent.Find("B. CV-then-human").gameObject.SetActive(false);
            buttonParent.Find("B. Human-then-CV").gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Can't find button parrent.");
        }
        */
    }

    public void Manual_Interface()
    {
        Debug.Log("Manual interface in ui manager called");
        faultInteractionObject.GetComponent<SphereCollider>().isTrigger = true; //Enable finger collider
        interactionManager.InteractionMethodChosen = LAV_InteractionManager.InteractionMethod.Manual_Only;
    }

    public void CV_Only_Interface()
    {
        Debug.Log("CV interface in ui manager called");
        faultInteractionObject.GetComponent<SphereCollider>().isTrigger = false; //Disable finger collider
        faultPrefab.GetComponent<SphereCollider>().enabled = false; //Disable point collider (no moving them)
        interactionManager.InteractionMethodChosen = LAV_InteractionManager.InteractionMethod.CV_Only;
    }

    public void CV_Then_Human_Interface()
    {
        Debug.Log("CV-Human interface in ui manager called");
        faultInteractionObject.GetComponent<SphereCollider>().isTrigger = true; //Enable finger collider
        interactionManager.InteractionMethodChosen = LAV_InteractionManager.InteractionMethod.CV_then_Human;
    }

    public void Human_Then_CV_Interface()
    {
        Debug.Log("Human-CV interface in ui manager called");
        faultInteractionObject.GetComponent<SphereCollider>().isTrigger = true; //Enable finger collider
        interactionManager.InteractionMethodChosen = LAV_InteractionManager.InteractionMethod.Human_then_CV;
    }

    public void Tutorial_Interface()
    {
        Debug.Log("Tutorial interface in ui manager called");
        faultInteractionObject.GetComponent<SphereCollider>().isTrigger = true; //Enable finger collider
        interactionManager.InteractionMethodChosen = LAV_InteractionManager.InteractionMethod.Tutorial;
    }
    #endregion

    #region Send Images to Server
    public void CrackML_ImageToServer()
    {
        AddToLog("Sending image of size " + (int)HololensBytesJPG.Length / 1028 + " kb to server.");
        StartCoroutine(NetworkManager.Singleton.SendCrackedImage(HololensBytesJPG, false));
    }

    public void CrackML_ImageToServerInteractable()
    {
        AddToLog("Sending image of size " + (int)HololensBytesJPG.Length / 1028 + " kb to server.");
        StartCoroutine(NetworkManager.Singleton.SendCrackedImage(HololensBytesJPG, true));
    }

    public void CorrosionML_ImageToServer()
    {
        AddToLog("Sending image of size " + (int)HololensBytesJPG.Length / 1028 + " kb to server.");
        StartCoroutine(NetworkManager.Singleton.SendCorrodedImage(HololensBytesJPG));
    }

    public void MaterialML_ImageToServer()
    {
        AddToLog("Sending image of size " + (int)HololensBytesJPG.Length / 1028 + " kb to server.");
        StartCoroutine(NetworkManager.Singleton.SendMaterialImage(HololensBytesJPG));
    }

    public void CV_only_caller()
    {
        AddToLog("Sending image of size " + (int)HololensBytesJPG.Length / 1028 + " kb to server.");
        StartCoroutine(NetworkManager.Singleton.SendImage_CV_only(HololensBytesJPG));
    }
    #endregion

    #region Debuging Visualization Tools
    public void RaycastOrb(Vector3 pos, Vector3 norm, Color c)
    {
        float step = 0.01f;
        GameObject[] newOrb = new GameObject[100];
        for (int i = 0; i < 100; i++)
        {
            newOrb[i] = Instantiate(endNodePrefab);
            newOrb[i].transform.position = pos + norm * step * i;
            newOrb[i].GetComponent<MeshRenderer>().material.SetColor("_Color", c);
        }

        Debug.Log($"Spawning node at position {pos}");
    }
    public void RaycastLine(Vector3 p1, Vector3 p2, Color c)
    {
        //Debug.DrawLine(p1, p2, c, 999f);
        Vector3 v = p2 - p1;
        float dist = v.magnitude;
        GameObject[] newOrb = new GameObject[100];
        for (int i = 0; i < 100; i++)
        {
            newOrb[i] = Instantiate(endNodePrefab);
            newOrb[i].transform.position = p1 + (v * (i/100f+1f));
            newOrb[i].GetComponent<MeshRenderer>().material.SetColor("_Color", c);
        }
    }

    //Raycasts a line from the focal point through p2 and spawns an orb wherever this ray intersects the mesh
    public Vector3 RaycastPoint(Vector3 p2, Color c, bool spawnOrb)
    {
        Vector3 v = p2 - camParams.focalPoint;
        float dist = v.magnitude;

        RaycastHit hit;
        if (Physics.Raycast(camParams.focalPoint, v, out hit, 10))
        {
            hit.point = hit.point + hit.normal * 0.001f;
        }
        else
        {
            hit.point = camParams.focalPoint - camParams.vz * 0.5f;
            hit.normal = camParams.vz;
        }

        if (spawnOrb)
        {
            GameObject newOrb = Instantiate(grippyOrbPrefab);
            newOrb.transform.position = hit.point;
            newOrb.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            newOrb.GetComponent<MeshRenderer>().material.SetColor("_Color", c);
        }

        return hit.point;
    }

    public Vector3 RaycastPoint2Plane(Vector3 p2, Color c, float sz)
    {
        Vector3 v = p2 - camParams.focalPoint;

        Vector3 normal = new Vector3();
        Vector3 point = new Vector3();
        int count = 0;
        RaycastHit hit;
        float dispersion = 50;//±4 cm spread
        for (float i = -2; i <= 2; i++)
        {
            for (float j = -2; j <= 2; j++)
            { 
                if (Physics.Raycast(camParams.focalPoint, -camParams.vz + (i/dispersion) * camParams.vx + (j / dispersion) * camParams.vy, out hit, 10))
                {
                    hit.point = hit.point + hit.normal * 0.001f;
                }
                else
                {
                    hit.point = camParams.focalPoint - camParams.vz * 0.5f;
                    hit.normal = camParams.vz;
                }
                if (i==0 && j == 0)
                {
                    point = hit.point;
                }
                normal += hit.normal;
                count++;
            }
        }
        normal = normal / count;//Average normal vector in the ±4 cm dispersion around the center

        GameObject newOrb = Instantiate(endNodePrefab);
        Vector3 projectedPoint = intersection(camParams.focalPoint, v, point, normal);
        newOrb.transform.position = projectedPoint;
        newOrb.transform.localScale = new Vector3(sz, sz, sz);
        newOrb.GetComponent<MeshRenderer>().material.SetColor("_Color", c);
        return projectedPoint;
    }

    public void SpawnOrb(Vector3 pos, Color c)
    {
        GameObject newOrb = Instantiate(endNodePrefab);
        newOrb.transform.position = pos;
        //newOrb.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        newOrb.GetComponent<MeshRenderer>().material.SetColor("_Color", c);
    }

    public void SpawnOrbSize(Vector3 pos, Color c, float sz)
    {
        GameObject newOrb = Instantiate(endNodePrefab);
        newOrb.transform.position = pos;
        newOrb.transform.localScale = new Vector3(sz, sz, sz);
        newOrb.GetComponent<MeshRenderer>().material.SetColor("_Color", c);
    }

    #endregion

    #region DefectContainer stuff
    public void SaveCameraMatrix(Matrix4x4 c2w, Matrix4x4 proj, GameObject camera_m_Canvas)
    {
        UIManager.Singleton.AddToLog("SaveCameraMatrix called");
        cameraToWorldMatrix = c2w;
        projectionMatrix = proj;
        planeOverrideFlag = false;//default to mesh, not used anymore
        m_Canvas = camera_m_Canvas;
        camParams = calculateCameraParameters(c2w);
        CV_only_caller();
    }

    //create the CV spline and points with the data passed from the image routines
    public void drawGUI(List<decimal[]> data)
    {
        //GameObject handMenu = GameObject.Find("UI");
        //handMenu.SetActive(false);
        if (defectCreated)
        {
            Debug.Log("Attempting to delete defect");
            //interaction_CV.deleteSelf();
        }

        Debug.Log("Data structure from server:");
        foreach (decimal[] decimalArray in data)
        {
            string res = "";
            foreach (decimal value in decimalArray)
            {
                res += $"{value}, ";
            }
            Debug.Log(res);
        }
        if (data.Count > 1)
        {
            Debug.Log("Before interaction_CV caller");
            interaction_CV.CV_Only(HololensBytesJPG, cameraToWorldMatrix, projectionMatrix, m_Canvas, data, _singleton);
        }
        else
        {
            Debug.Log("Failed to find any cracks in image.");
            AddToLog("Failed to find any cracks in image.");
        }
        //handMenu.SetActive(true);
    }


    //Since the defect container can only access classes it has a singleton reference to,
    //we have to access the network manager through the UIManager
    public void updateThicknessWrapper(GameObject interactingObject, Vector2 coords)
    {
        StartCoroutine(NetworkManager.Singleton.updateThickness(interactingObject, coords));
    }

    public void requestSplineWrapper(AGS_Interaction_CV CV)
    {
        StartCoroutine(NetworkManager.Singleton.splineContour(CV));
    }

    public void sendBasicImageWrapper(byte[] image)
    {
        StartCoroutine(NetworkManager.Singleton.SendBasicImage(image));
    }

    #endregion

    #region Kyle projection code
    //Returns the x,y image coordinates transformed into X,Y,Z coordinates on the projected canvas
    public Vector3 ConvertUV2KyleXYZ(Vector2 imgCoords)
    {
        float reducedWidth = 920f + 40f;//The last 40f is emperical bandaid
        float imageWidth = 3904f-reducedWidth;
        float imageHeight = 2196f;
        float U = (imgCoords.x - reducedWidth / 2) / imageWidth - 0.5f + 0.007f;//The last 0.007f is emperical bandaid
        float V = (imgCoords.y / imageHeight) * (0.35f + 0.39f) - 0.39f + 0.040f;//The last 0.040f is emperical bandaid
        Vector2 UV = new Vector2(U, V);
        Vector3 targetPoint = m_Canvas.transform.TransformPoint(new Vector3(UV.x, -UV.y, 0)); //Flip the y axis

        if (U < -0.5f || V < -0.5f || U > 0.5f || V > 0.5f)
        {
            AddToLog($"U,V out of bounds: {U},{V}");
        }
        return targetPoint;
    }

    //This returns the X,Y,Z coordinates transforemd into x,y image coordinates
    private Vector2 ReverseKyleXYZ2UV(Vector3 targetPoint)
    {
        float reducedWidth = 920f + 40f;
        float imageWidth = 3904f - reducedWidth;
        float imageHeight = 2196f;
        Vector2 canvasCoords = m_Canvas.transform.InverseTransformPoint(targetPoint);
        float U = (canvasCoords.x + 0.5f - 0.007f) * imageWidth + reducedWidth / 2;
        float V = (-canvasCoords.y + 0.39f - 0.040f) / (0.35f + 0.39f) * imageHeight;//Flip the y axis
        Vector2 imgCoords = new Vector2(U, V);
        return imgCoords;
    }
    #endregion 

    #region physics based image projection
    public void DrawPlanarMesh(Vector3[] vertices, Vector2[] uvs)
    {
        uvs = scaleAndFlipLocalCoords(uvs);

        for (var i = 0; i < uvs.Length; i++)
        {
            //Debug.Log($"Scaled UVs: {uvs[i][0]},{uvs[i][1]}");
        }

        int[] triangles =
        {
            //WHY IS UNITY USING LEFT HAND RULE F0OR NORMALS
            //THIS IS STUPID

            0,1,2, //First triangle (top left)
            2,3,0 //Second triangle (bottom right)
        };

        if (meshObject != null)
        {
            Destroy(meshObject);
        }
        meshObject = Instantiate(meshPrefab);
        meshObject.GetComponent<MeshFilter>().mesh.Clear();
        meshObject.GetComponent<MeshFilter>().mesh.vertices = vertices;
        meshObject.GetComponent<MeshFilter>().mesh.triangles = triangles;
        meshObject.GetComponent<MeshFilter>().mesh.uv = uvs;
        meshObject.GetComponent<MeshFilter>().mesh.RecalculateBounds();
        meshObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        meshObject.GetComponent<MeshRenderer>().material = material;
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(DistortedHololensBytesJPG); //..this will auto-resize the texture dimensions.
        tex.Apply();
        meshObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", tex);
        ViewImage();
        meshObject.SetActive(true);
    }

    /*
    public void callSendBasicImage()
    {
        RaycastHit hit;
        if (Physics.Raycast(camParams.focalPoint, -camParams.vz, out hit, 10))
        {
            hit.point = hit.point + hit.normal * 0.001f;
        }
        else
        {
            hit.point = camParams.focalPoint - camParams.vz * 0.5f;
            hit.normal = camParams.vz;
        }

        Vector3[] vertices = new Vector3[4];
        Vector3[] canvasCoords =
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
            new Vector3(0.5f, -0.5f, 0)
        };

        for (int i = 0; i < 4; i++)
        {
            Vector3 orbOnCanvas = m_Canvas.transform.TransformPoint(canvasCoords[i]);
            Vector3 rayDir = orbOnCanvas - camParams.focalPoint;
            vertices[i] = intersection(camParams.focalPoint, rayDir, hit.point, hit.normal);
        };

        Vector2[] localCoords = findLocalImageCoordinates(vertices);
        //Debug.Log("Sending image of size " + (int)UIManager.HololensBytesJPG.Length / 1028 + " kb to server.");
        StartCoroutine(NetworkManager.Singleton.SendBasicImage(HololensBytesJPG, vertices, localCoords));
    }
    
    public void drawPlanePhysics()
    {
        AddToLog("drawPlanePhysics Called");
        float focalLength = 4.87f / 1000;
        float ar = camParams.imageH / camParams.imageW;
        float sensorWidth = 0.006168085150420666f;//Assume sensor width is known (calculated from nominal hfov = 64.69 deg and focal length = 4.87mm)
        float sensorHeight = sensorWidth * ar;

        RaycastHit hit;
        if (Physics.Raycast(camParams.focalPoint, -camParams.vz, out hit, 10))
        {
            hit.point = hit.point + hit.normal * 0.001f;
        }
        else
        {
            hit.point = camParams.focalPoint - camParams.vz * 0.5f;
            hit.normal = camParams.vz;
        }

        Vector3 topRightSensor = camParams.focalPoint + camParams.vz * focalLength + camParams.vx * sensorWidth / 2 + camParams.vy * sensorHeight / 2;
        Vector3 bottomRightSensor = camParams.focalPoint + camParams.vz * focalLength + camParams.vx * sensorWidth / 2 - camParams.vy * sensorHeight / 2;
        Vector3 topLeftSensor = camParams.focalPoint + camParams.vz * focalLength - camParams.vx * sensorWidth / 2 + camParams.vy * sensorHeight / 2;
        Vector3 bottomLeftSensor = camParams.focalPoint + camParams.vz * focalLength - camParams.vx * sensorWidth / 2 - camParams.vy * sensorHeight / 2;

        Vector3 topRightVsensor = camParams.focalPoint - bottomLeftSensor;
        Vector3 bottomRightVSensor = camParams.focalPoint - topLeftSensor;
        Vector3 topLeftVSensor = camParams.focalPoint - bottomRightSensor;
        Vector3 bottomLeftVSensor = camParams.focalPoint - topRightSensor;

        Vector3 topRightV = topRightVsensor / topRightVsensor.magnitude;
        Vector3 bottomRightV = bottomRightVSensor / bottomRightVSensor.magnitude;
        Vector3 topLeftV = topLeftVSensor / topLeftVSensor.magnitude;
        Vector3 bottomLeftV = bottomLeftVSensor / bottomLeftVSensor.magnitude;

        //Assume for now that the plane extends far enough to capture whole image
        Vector3 topLeftIntersect = intersection(camParams.focalPoint, topLeftV, hit.point, hit.normal);
        Vector3 bottomLeftIntersect = intersection(camParams.focalPoint, bottomLeftV, hit.point, hit.normal);
        Vector3 bottomRightIntersect = intersection(camParams.focalPoint, bottomRightV, hit.point, hit.normal);
        Vector3 topRightIntersect = intersection(camParams.focalPoint, topRightV, hit.point, hit.normal);

        Vector3 cgPlane = (topLeftIntersect + topRightIntersect + bottomLeftIntersect + bottomRightIntersect) / 4;
        Vector3 midLeftIntersect = (topLeftIntersect + bottomLeftIntersect) / 2;
        Vector3 midTopIntersect = (topRightIntersect + topLeftIntersect) / 2;
        float sxPlane = (midLeftIntersect - cgPlane).magnitude * 2;
        float syPlane = (midTopIntersect - cgPlane).magnitude * 2;

        Vector3[] vertices =
        {
            bottomLeftIntersect,
            topLeftIntersect,
            topRightIntersect,
            bottomRightIntersect
        };

        Vector2[] localCoords = findLocalImageCoordinates(vertices);
        //Debug.Log("Sending image of size " + (int)UIManager.HololensBytesJPG.Length / 1028 + " kb to server.");
        StartCoroutine(NetworkManager.Singleton.SendBasicImage(HololensBytesJPG, vertices, localCoords));
    }
    */
    #endregion

    #region Supporting Functions
    private Vector3 rotateAboutK(float inputAngle, Vector3 rotationAxisK, Vector3 inputV)
    {
        inputAngle = inputAngle * Mathf.PI / 180;
        //Rodrigues' rotation formula
        Vector3 vrot = inputV * Cos(inputAngle) + Vector3.Cross(rotationAxisK, inputV) * Sin(inputAngle) + rotationAxisK * Vector3.Dot(rotationAxisK, inputV) * (1 - Cos(inputAngle));
        return vrot;
    }

    //intersection of a point and plane
    private Vector3 intersection(Vector3 l0, Vector3 lv, Vector3 p0, Vector3 n)
    {
        Vector3 intersectionPoint = l0 + (Vector3.Dot((p0 - l0), n) / Vector3.Dot(lv, n)) * lv;
        return intersectionPoint;
    }

    //NOTE THAT PLOTTING THE IMAGE REQUIRES UV VALUES SCALED TO A MAX OF 1 IN X AND Y
    //HOWEVER, THE HOMOGRAPHY REQUIRES UNSCALED VALUES OR ELSE YOU WILL SQUARE THE IMAGE
    private Vector2[] zeroLocalCoords(Vector2[] inputCoords)
    {
        //Find min x & min y
        float minX = Infinity;
        float minY = Infinity;
        for (int i = 0; i < inputCoords.Length; i++)
        {
            if (inputCoords[i][0] < minX)
            {
                minX = inputCoords[i][0];
            }
            if (inputCoords[i][1] < minY)
            {
                minY = inputCoords[i][1];
            }
        }
        //Subtract min x & min y from all
        for (int i = 0; i < inputCoords.Length; i++)
        {
            inputCoords[i][0] = inputCoords[i][0] - minX;
            inputCoords[i][1] = inputCoords[i][1] - minY;
        }
        return inputCoords;
    }

    //LOCAL COORDINTES ARE ZEROED TO THE TOP LEFT CORNER,
    // ZERO THEM SUCH THAT THERE ARE NO NEGATIVES EITHER HERE OR IN PYTHON
    public Vector2[] findLocalImageCoordinates(Vector3[] globalImageCoordinates)
    {
        Vector3 bottomLeft = globalImageCoordinates[0];
        Vector3 topLeft = globalImageCoordinates[1];
        Vector3 topRight = globalImageCoordinates[2];
        Vector3 bottomRight = globalImageCoordinates[3];

        Vector3 Xaxis = topRight - topLeft;
        Vector3 Raxis = bottomLeft - topLeft;
        Vector3 Zaxis = Vector3.Cross(Xaxis, Raxis);
        Vector3 Yaxis = -Vector3.Cross(Xaxis, Zaxis);//Positive down

        Vector2[] localCoordinates = new Vector2[4];
        localCoordinates[0] = new Vector2(0, 0);

        int counter = 0;
        for (int i = 0; i < globalImageCoordinates.Length; i++)
        {
            if (i == 1)
            {
                counter++;
                continue;
            }
            Vector3 globalPos = globalImageCoordinates[i];
            Vector3 tempSanti = globalPos - topLeft;
            float distX_santi = Vector3.Dot(tempSanti, Xaxis) / Xaxis.magnitude;


            Vector3 yIntersection = intersection(topLeft, Yaxis, globalPos, Yaxis);
            float distY_santi = Vector3.Dot(tempSanti, Yaxis) / Yaxis.magnitude;
            localCoordinates[counter] = new Vector2(distX_santi, distY_santi);
            counter++;
        }
        localCoordinates = zeroLocalCoords(localCoordinates);
        return localCoordinates;
    }

    private Vector2[] scaleAndFlipLocalCoords(Vector2[] inputCoords)
    {
        //Find max x & max y
        float maxX = -Infinity;
        float maxY = -Infinity;
        for (int i = 0; i < inputCoords.Length; i++)
        {
            if (inputCoords[i][0] > maxX)
            {
                maxX = inputCoords[i][0];
            }
            if (inputCoords[i][1] > maxY)
            {
                maxY = inputCoords[i][1];
            }
        }
        //Divide max x & max y from all
        for (int i = 0; i < inputCoords.Length; i++)
        {
            inputCoords[i][0] = inputCoords[i][0] / maxX;
            inputCoords[i][1] = inputCoords[i][1] / maxY;
        }
        //Flip y coordinates
        for (int i = 0; i < inputCoords.Length; i++)
        {
            inputCoords[i][1] = 1 - inputCoords[i][1];
        }
        return inputCoords;
    }
    #endregion

    #region LogWindow
    public void AddToLog(string content)
    {
        for (int i = 0; i < logMessages.Length - 1; i++)
        {
            logMessages[i] = logMessages[i + 1];
        }
        logMessages[logMessages.Length - 1] = DateTime.Now.ToString("[HH:mm:ss]") + ">" + content;
        UpdateLogWindow();
    }

    public void OverwriteLog(string content)
    {
        logMessages[logMessages.Length - 1] = content;
        UpdateLogWindow();
    }

    private void UpdateLogWindow()
    {
        fullMessage = "";
        foreach (string value in logMessages)
        {
            fullMessage += (string.IsNullOrEmpty(value) ? "" : "");
            fullMessage += value;
            fullMessage += "\n";
        }
        logWindow.text = fullMessage;
    }
    #endregion

    //DONT FORGET TO QUESTION WHETHER WE SHOULD NORMALIZE THE VECTORS
    public cameraParameters calculateCameraParameters(Matrix4x4 c2w)
    {
        cameraParameters outParams = new cameraParameters();
        outParams.focalPoint = cameraToWorldMatrix.GetColumn(3);
        outParams.vx = cameraToWorldMatrix.GetColumn(0);
        outParams.vy = cameraToWorldMatrix.GetColumn(1);
        outParams.vz = cameraToWorldMatrix.GetColumn(2);

        outParams.imageW = 3904;
        outParams.imageH = 2196;
        //outParams.imageW = 640; //Webcam
        //outParams.imageH = 480;
        return outParams;
    }

    public class cameraParameters
    {
        public Vector3 vx;
        public Vector3 vy;
        public Vector3 vz;
        public Vector3 focalPoint;
        public float imageW;
        public float imageH;
    }
}