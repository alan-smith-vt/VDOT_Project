using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    private AGS_Interaction_CV interaction_CV;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    //TODO: add additional ip fields for offline desktop, offline laptop, and eduroam desktop
        //Set a default # which determines which we connect to
        //If connection fails, connect to the next # in line
        //Wrap around until all connections are checked
        //Then tell the user the connection failed
    [SerializeField] public string ip_eduroamDesktop;
    [SerializeField] public string ip_LAN_Desktop;
    [SerializeField] public string ip_LAN_Laptop;
    [SerializeField] public string ip_HomeWifi_Laptop;
    [SerializeField] public string port;

    public string ip;//
    private string[] ipList = new string[4];
    
    public string serverEndpoint;

    private void Awake()
    {
        Singleton = this;
        ipList[0] = ip_eduroamDesktop;
        ipList[1] = ip_LAN_Desktop;
        ipList[2] = ip_LAN_Laptop;
        ipList[3] = ip_HomeWifi_Laptop;
    }

    private void Start()
    {
        StartCoroutine(Connect(0));
        StartCoroutine(Connect(1));
        StartCoroutine(Connect(2));
        StartCoroutine(Connect(3));
        interaction_CV = GameObject.Find("FaultInteraction").GetComponent<AGS_Interaction_CV>();
    }

    //Currently, attempts to connect to the given IP address, then if failure,
    //it checks the next ip address in the ipList until all have been checked

    //Future plans include checking if in editor and if so starting with the Eduroam option.
    public IEnumerator Connect(int ip_attempts)
    {
        string ip_connect = ipList[ip_attempts];
        Debug.Log($"Trying to connect to {ip_connect}");
        string serverEndpoint_connect = "http://" + ip_connect + ":" + port;

        WWWForm webform = new WWWForm();
        using (UnityWebRequest uwr = UnityWebRequest.Get(serverEndpoint_connect))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received: " + uwr.downloadHandler.text);
                Debug.Log($"Connected to server. IP = {ip_connect}");
                UIManager.Singleton.AddToLog($"Connected to server. IP = {ip_connect}");
                ip = ip_connect;
                serverEndpoint = serverEndpoint_connect;
            }
        }
    }

    public IEnumerator ComCheck()
    {
        serverEndpoint = "http://" + ip + ":" + port;

        WWWForm webform = new WWWForm();
        using (UnityWebRequest uwr = UnityWebRequest.Post(serverEndpoint, webform))
        {
            uwr.SetRequestHeader("Request-Type", "ComCheck");

            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received: " + uwr.downloadHandler.text);
            }
        }
    }

    #region Send Images and Request ML
    public IEnumerator SendCrackedImage(byte[] imageBytes, bool interactable)
    {
        serverEndpoint = "http://" + ip + ":" + port;

        WWWForm webform = new WWWForm();
        using (UnityWebRequest uwr = UnityWebRequest.Post(serverEndpoint, webform))
        {
            uwr.SetRequestHeader("Content-Type", "application/octet-stream");

            //I think the request-type might have to match the funtion name including caps
            uwr.SetRequestHeader("Request-Type", "SendCrackedImage");

            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.uploadHandler = new UploadHandlerRaw(imageBytes);
            uwr.uploadHandler.contentType = "application/octet-stream";
            Debug.Log("Sending image to server.");
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received " + uwr.downloadHandler.text.Length + "bytes of data. Reading as image.");
                UIManager.HololensBytesJPG = uwr.downloadHandler.data;
                UIManager.Singleton.ViewImage();
                if (interactable)
                {
                    Debug.Log("Calling SendNodes function!");
                    StartCoroutine(SendNodes());
                }
            }
        }
    }

    public IEnumerator SendCorrodedImage(byte[] imageBytes)
    {
        serverEndpoint = "http://" + ip + ":" + port;


        WWWForm webform = new WWWForm();
        using (UnityWebRequest uwr = UnityWebRequest.Post(serverEndpoint, webform))
        {
            uwr.SetRequestHeader("Content-Type", "application/octet-stream");

            //I think the request-type might have to match the funtion name including caps
            uwr.SetRequestHeader("Request-Type", "SendCorrodedImage");

            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.uploadHandler = new UploadHandlerRaw(imageBytes);
            uwr.uploadHandler.contentType = "application/octet-stream";
            Debug.Log("Sending image to server.");
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received " + uwr.downloadHandler.text.Length + "bytes of data. Reading as image.");
                UIManager.HololensBytesJPG = uwr.downloadHandler.data;
                UIManager.Singleton.ViewImage();
            }
        }
    }

    public IEnumerator SendMaterialImage(byte[] imageBytes)
    {
        serverEndpoint = "http://" + ip + ":" + port;

        WWWForm webform = new WWWForm();
        using (UnityWebRequest uwr = UnityWebRequest.Post(serverEndpoint, webform))
        {
            uwr.SetRequestHeader("Content-Type", "application/octet-stream");

            //I think the request-type might have to match the funtion name including caps
            uwr.SetRequestHeader("Request-Type", "SendMaterialImage");

            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.uploadHandler = new UploadHandlerRaw(imageBytes);
            uwr.uploadHandler.contentType = "application/octet-stream";
            Debug.Log("Sending image to server.");
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received " + uwr.downloadHandler.text.Length + "bytes of data. Reading as image.");
                UIManager.HololensBytesJPG = uwr.downloadHandler.data;
                UIManager.Singleton.ViewImage();
            }
        }
    }
    #endregion

    //This used to be used for projecting blobs on the distorted projected image
    //Now just used to send the image to the server
    public IEnumerator SendBasicImage(byte[] imageBytes)//, Vector3[] vertices, Vector2[] localCoords
    {
        UIManager.Singleton.AddToLog("SendBasicImage Called");
        serverEndpoint = "http://" + ip + ":" + port;

        WWWForm webform = new WWWForm();
        using (UnityWebRequest uwr = UnityWebRequest.Post(serverEndpoint, webform))
        {
            
            uwr.SetRequestHeader("Content-Type", "application/octet-stream");

            //I think the request-type might have to match the funtion name including caps
            uwr.SetRequestHeader("Request-Type", "SendBasicImage");

            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.uploadHandler = new UploadHandlerRaw(imageBytes);
            uwr.uploadHandler.contentType = "application/octet-stream";
            Debug.Log("Sending image to server.");
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received:" + uwr.downloadHandler.text);
                //StartCoroutine(RequestDistortedImage(vertices, localCoords));
                //StartCoroutine(projectBlobs());
            }
        }
    }

    public IEnumerator SendImage_CV_only(byte[] imageBytes)
    {
        UIManager.Singleton.AddToLog("Send CV_only Called");
        serverEndpoint = "http://" + ip + ":" + port;

        WWWForm webform = new WWWForm();
        using (UnityWebRequest uwr = UnityWebRequest.Post(serverEndpoint, webform))
        {

            uwr.SetRequestHeader("Content-Type", "application/octet-stream");

            //I think the request-type might have to match the funtion name including caps
            uwr.SetRequestHeader("Request-Type", "CV_only");
            uwr.timeout = 10;

            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.uploadHandler = new UploadHandlerRaw(imageBytes);
            uwr.uploadHandler.contentType = "application/octet-stream";
            Debug.Log("Sending image to server.");
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
                //UIManager.newDefect.setActive(false);
                
                //What is this doing? Why would we do a simple send image if the CV image send fails?
                //StartCoroutine(SendBasicImage(UIManager.Singleton.newDefect.HololensBytesJPG));
            }
            else
            {
                Debug.Log("Received:" + uwr.downloadHandler.text);
                UIManager.Singleton.AddToLog($"Recieved response from server.");
                List<decimal[]> data = unpackMatrix(uwr.downloadHandler.text);
                Debug.Log("Data unpacked: " + data.ToString());
                UIManager.Singleton.drawGUI(data);
            }
        }
    }

    //Unpack a matrix of arbitrary rows and 13 columns
    //TODO: update to take column # as argument
    List<decimal[]> unpackMatrix(string data_text)
    {
        int level = 0;
        string tempString = "";
        List<decimal[]> res = new List<decimal[]>();
        foreach (char c in data_text)
        {
            if (c == '[')
            {
                level++;
            }
            else if (c == ']')
            {
                level--;
                if (level == 1)//if after going down a level we end up at level 2, we need to reset the temp string and parse it's contents
                {
                    tempString = tempString.Substring(1);//remove the first [
                    string[] contents = tempString.Split(',');
                    decimal[] arr = new decimal[3];
                    for (int i = 0; i < contents.Length; i++)
                    {
                        arr[i] = decimal.Parse(contents[i]);
                    }
                    //Add this result to the running list
                    res.Add(arr);
                    tempString = "";
                }
            }
            if (level == 2)
            {
                tempString += c;
            }
        }
        return res;
    }
    
    public IEnumerator updateThickness(GameObject interactingObject, Vector2 coords)
    {
        serverEndpoint = "http://" + ip + ":" + port;

        thicknessCoordinate contents = new thicknessCoordinate();
        contents.thicknessPixel = coords;

        string jsonContents = JsonUtility.ToJson(contents);
        WWWForm webform = new WWWForm();

        using (UnityWebRequest uwr = UnityWebRequest.Post(serverEndpoint, webform))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonContents);
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("Request-Type", "updateThickness");

            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received " + uwr.downloadHandler.text.Length + "bytes of data. Reading as float.");
                float newThickness = (float)decimal.Parse(uwr.downloadHandler.text);
                Debug.Log($"Parsed result: {newThickness}");

                interaction_CV.parseThickness(interactingObject, newThickness);
            }
        }
    }

    public IEnumerator splineContour(AGS_Interaction_CV CV)
    {
        serverEndpoint = "http://" + ip + ":" + port;
        Debug.Log("splineContour function started!");
        WWWForm webform = new WWWForm();
        using (UnityWebRequest uwr = UnityWebRequest.Post(serverEndpoint, webform))
        {

            uwr.SetRequestHeader("Request-Type", "splineContour");

            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                string data_text = uwr.downloadHandler.text;
                Debug.Log("Received: " + data_text);
                var res = unpackSpline(data_text);
                CV.drawSpline(res);
            }
        }
    }

    #region CV_Testing_wo_ML
    public IEnumerator projectBlobs()
    {

        serverEndpoint = "http://" + ip + ":" + port;

        WWWForm webform = new WWWForm();
        using (UnityWebRequest uwr = UnityWebRequest.Post(serverEndpoint, webform))
        {
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("Request-Type", "projectBlobs");

            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received " + uwr.downloadHandler.text.Length + "bytes of data.");
                Debug.Log($"bytes in text: {uwr.downloadHandler.text}");
                Vector2[] res = extractCoords(uwr.downloadHandler.text);
                Vector2[] imageCorners = new Vector2[]
                {
                    new Vector2(0, 2196),
                    new Vector2(0,0),
                    new Vector2(3904,0),
                    new Vector2(3904, 2196)
                };
                //res = fakeRes;
                for (int i = 0; i < res.Length; i++)
                {
                    Debug.Log($"Res[{i}] = {res[i][0]},{res[i][1]}");
                    //public Vector3 ConvertUV2XYZ(Vector2 imgCoords, offsetParameter xOffset)
                    Vector3 res3 = UIManager.Singleton.ConvertUV2KyleXYZ(res[i]);
                    //UIManager.Singleton.SpawnOrbSize(res3, Color.green, 0.01f);
                    UIManager.Singleton.RaycastPoint(res3, Color.red, true);
                }
                Vector3[] projectedCorners = new Vector3[4];
                for (int i = 0; i < imageCorners.Length; i++)
                {
                    Vector3 corner = UIManager.Singleton.ConvertUV2KyleXYZ(imageCorners[i]);
                    UIManager.Singleton.SpawnOrbSize(corner, Color.blue, 0.01f);
                    projectedCorners[i] = UIManager.Singleton.RaycastPoint2Plane(corner, Color.green, 0.01f);
                }
                StartCoroutine(RequestDistortedImage(projectedCorners, UIManager.Singleton.findLocalImageCoordinates(projectedCorners)));
            }
        }
    }

    Vector2[] extractCoords(string data_text)
    {
        int level = 0;
        string tempString = "";
        Vector2[] resTemp = new Vector2[100];//Temporarily assign 100 slots
        int contourIndex = 0;
        foreach (char c in data_text)
        {
            if (c == '[')
            {
                level++;
            }
            else if (c == ']')
            {
                level--;
                if (level == 1)//if after going down a level we end up at level 2, we need to reset the temp string and parse it's contents
                {
                    tempString = tempString.Substring(1);//remove the first [
                    string[] contents = tempString.Split(',');
                    contents[1] = contents[1].Substring(1);//remove the whitespace after the comma
                    float x = float.Parse(contents[0]);
                    float y = float.Parse(contents[1]);
                    Vector2 xy = new Vector2(x, y);
                    //Add this result to the running list
                    resTemp[contourIndex] = xy;
                    tempString = "";
                    contourIndex++;
                }
            }
            if (level == 2)
            {
                tempString += c;
            }
        }
        Vector2[] res = new Vector2[contourIndex];
        for (int i = 0; i < contourIndex; i++)
        {
            res[i] = resTemp[i];
        }
        return res;
    }

    //Image homography code for projecting onto planar mesh
    public IEnumerator RequestDistortedImage(Vector3[] vertices, Vector2[] localCoords)
    {
        UIManager.Singleton.AddToLog("RequestDistortedImage Called");
        serverEndpoint = "http://" + ip + ":" + port;

        coordContent contents = new coordContent();
        contents.coordinate_0 = localCoords[0];
        contents.coordinate_1 = localCoords[1];
        contents.coordinate_2 = localCoords[2];
        contents.coordinate_3 = localCoords[3];


        string jsonContents = JsonUtility.ToJson(contents);
        WWWForm webform = new WWWForm();
        using (UnityWebRequest uwr = UnityWebRequest.Post(serverEndpoint, webform))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonContents);
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("Request-Type", "RequestDistortedImage");

            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received " + uwr.downloadHandler.text.Length + "bytes of data. Reading as image (distorted).");
                UIManager.DistortedHololensBytesJPG = uwr.downloadHandler.data;
                UIManager.Singleton.DrawPlanarMesh(vertices, localCoords);
                //StartCoroutine(projectBlobs());
                //UIManager.Singleton.castUV_kyleCode();
            }
        }
    }
    public class coordContent
    {
        public Vector2 coordinate_0;
        public Vector2 coordinate_1;
        public Vector2 coordinate_2;
        public Vector2 coordinate_3;
    }

    public class blobCoords
    {
        public Vector2[] coords;
    }

    #endregion

    #region Spline_code
    public IEnumerator UpdateSpline(int contour, int node, float[] coords)
    {
        serverEndpoint = "http://" + ip + ":" + port;

        nodeUpdate contents = new nodeUpdate();
        contents.contourIndex = contour;
        contents.nodeIndex = node;
        contents.coordinates = coords;

        string jsonContents = JsonUtility.ToJson(contents);
        WWWForm webform = new WWWForm();
        using (UnityWebRequest uwr = UnityWebRequest.Post(serverEndpoint, webform))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonContents);
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("Request-Type", "UpdateSpline");

            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                Debug.Log("Received " + uwr.downloadHandler.text.Length + "bytes of data. Reading as image.");
                UIManager.HololensBytesJPG = uwr.downloadHandler.data;
                UIManager.Singleton.ViewImage();
            }
        }
    }

    //Send nodes sends nothing but an empty request to the server
        //Then the server sends the nodes back
    public IEnumerator SendNodes()
    {
        serverEndpoint = "http://" + ip + ":" + port;
        Debug.Log("SendNodes function started!");
        WWWForm webform = new WWWForm();
        using (UnityWebRequest uwr = UnityWebRequest.Post(serverEndpoint, webform))
        {

            uwr.SetRequestHeader("Request-Type", "SendNodes");

            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                string data_text = uwr.downloadHandler.text;
                Debug.Log("Received: " + data_text);
                var res = unpackSpline(data_text);
                UIManager.nodeList = res;
                //UIManager.Singleton.SpawnOrbs(); //UNCOMMENT ME LATER, IM BREAKING THINGS
            }
        }
    }

    List<List<decimal[]>> unpackSpline(string data_text)
    {
        int level = 0;
        string tempString = "";
        List<List<decimal[]>> res  = new List<List<decimal[]>>
        {
            new List<decimal[]>()
        };
        int contourIndex = 0;
        foreach (char c in data_text)
        {
            if (c == '[')
            {
                level++;
            }
            else if (c== ']')
            {
                level--;
                if (level == 2)//if after going down a level we end up at level 2, we need to reset the temp string and parse it's contents
                {
                    tempString = tempString.Substring(1);//remove the first [
                    string[] contents = tempString.Split(',');
                    contents[1] = contents[1].Substring(1);//remove the whitespace after the comma
                    decimal x = decimal.Parse(contents[0]);
                    decimal y = decimal.Parse(contents[1]);
                    decimal[] xy = new decimal[] { x, y };
                    //Add this result to the running list
                    res[contourIndex].Add(xy);
                    tempString = "";
                }
                if (level == 1)//if after going down a level we end up at level 1, we need to add another contourIndex
                {
                    res.Add(new List<decimal[]>());
                    contourIndex++;
                }
                if (level == 0)//we've reached the end! Delete that last unused contour index
                {
                    res.RemoveAt(res.Count - 1);
                }
            }
            if (level == 3)
            {
                tempString += c;
            }
        }
        return res;
    }

    public class nodeUpdate
    {
        public int contourIndex;
        public int nodeIndex;
        public float[] coordinates;
    }

    public class thicknessCoordinate
    {
        public Vector2 thicknessPixel;
    }

    public class thicknessResult
    {
        public float thickness;
    }
    #endregion
}
