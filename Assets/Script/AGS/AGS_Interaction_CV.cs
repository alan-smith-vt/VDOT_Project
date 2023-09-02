using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;



public class AGS_Interaction_CV : MonoBehaviour
{
    public byte[] HololensBytesJPG;
    private Matrix4x4 cameraToWorldMatrix;
    private Matrix4x4 projectionMatrix;
    private GameObject m_Canvas;
    public List<decimal[]> cvData;
    private cameraParameters camParams;
    private UIManager uiManager;
    private LAV_FaultMeasurement currentFault;
    private LAV_InteractionManager InteractionManager;

    private LineRenderer[] lineRendererList;
    private bool existingPicture = false;

    public int totalPoints = 0;
    public int currentPoints = 0;
    public string pointData = "";

    public GameObject UILoading;

    // Start is called before the first frame update
    void Start()
    {
        InteractionManager = GameObject.Find("FaultInteraction").GetComponent<LAV_InteractionManager>();
    }

    public void CV_Only(byte[] HololensBytesJPG, Matrix4x4 cameraToWorldMatrix, Matrix4x4 projectionMatrix, GameObject m_Canvas, List<decimal[]> cvData, UIManager uiManager)
    {
        Debug.Log("Inside interaction_CV caller");
        this.HololensBytesJPG = HololensBytesJPG;
        this.cameraToWorldMatrix = cameraToWorldMatrix;
        this.projectionMatrix = projectionMatrix;
        this.m_Canvas = m_Canvas;
        this.cvData = cvData;
        this.uiManager = uiManager;

        this.currentFault = GameObject.Find("Fault").GetComponent<LAV_FaultMeasurement>();

        this.camParams = calculateCameraParameters(cameraToWorldMatrix);

        currentPoints = 0;

        if (existingPicture)
        {
            pointData = "";
            //delete all fault objects
            foreach (Transform childTransform in currentFault.transform)
            {
                // Check if the child transform's name starts with "FaultPoint_"
                if (childTransform.name.StartsWith("FaultPoint_"))
                {
                    //Delete faultPoint and remove from data list
                    GameObject childObject = childTransform.gameObject;
                    currentFault.RemoveFaultPoint(childTransform.position);
                    Destroy(childObject);
                }
            }

            //delete splines
            Transform splineParent = currentFault.transform.Find("Spline");
            if (splineParent != null)
            {
                DestroyImmediate(splineParent.gameObject);
            }
            //hide bounding box
            currentFault.updateBoundsVisibility(false);
        }
        existingPicture = true;
        for (int j = 0; j < cvData.Count - 1; j++)
        {
            float x = (float)cvData[j + 1][0];
            float y = (float)cvData[j + 1][1];
            float t = (float)cvData[j + 1][2];

            if (InteractionManager.InteractionMethodChosen != LAV_InteractionManager.InteractionMethod.Human_then_CV)
            {
                addOrb(x, y, t);
                totalPoints++;
                currentPoints++;
            }

        }
        uiManager.requestSplineWrapper(this);
    }

    public void addOrb(float x = 0, float y = 0, float t = 0)
    {
        if (x == 0 && y == 0)
        {
            x = 3904f / 2;
            y = 2196f / 2;
        }

        Vector3 targetPoint = ConvertUV2KyleXYZ(new Vector2(x, y));
        Vector3 p0 = RaycastPoint(targetPoint);

        Debug.Log(string.Format("Adding point {0} {1}", p0.ToString("F5"), new Vector3(-0.01f, 0, 0.1f).ToString("F5")));
        currentFault.AddFaultPoint(p0, Quaternion.identity, new Vector3(-0.01f, 0, 0.1f));

        // Loop through all child transforms of the parent object
        foreach (Transform childTransform in currentFault.transform)
        {
            // Check if the child transform's name starts with "FaultPoint_"
            if (childTransform.name.StartsWith("FaultPoint_"))
            {
                // Do something with the child object, for example:
                GameObject childObject = childTransform.gameObject;
                if (childObject.transform.position == p0)
                {
                    pointData += $"(x = {x} y = {y} t = {t} X = {p0.x} Y = {p0.y} Z = {p0.z}  T = ";
                    parseThickness(childObject, t);
                }
            }
        }
    }

    public void drawSpline(List<List<decimal[]>> data)
    {
        GameObject spline = new GameObject("Spline");
        spline.transform.SetParent(currentFault.transform);

        lineRendererList = new LineRenderer[data.Count];
        for (int i = 0; i < data.Count; i++)
        {
            Vector3[] contour = new Vector3[data[i].Count];
            Vector2 temp = new Vector2();
            for (int j = 0; j < data[i].Count; j++)
            {
                temp = new Vector2((float)data[i][j][0], (float)data[i][j][1]);
                contour[j] = RaycastPoint(ConvertUV2KyleXYZ(temp));
                float thresh = 0.1f;
                if (j > 0)
                {
                    if (Vector3.Distance(contour[j], contour[j-1]) > thresh)
                    {
                        contour[j] = contour[j - 1];
                    }
                }
            }

            GameObject line = new GameObject("Spline Component");
            line.transform.SetParent(spline.transform);
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            Color c = Color.red;
            c.a = 0.5f;
            lineRenderer.material.SetColor("_Color", c);
            lineRenderer.positionCount = contour.Length;
            lineRenderer.SetPositions(contour);

            lineRenderer.startWidth = 0.001f;
            lineRenderer.endWidth = 0.001f;
            lineRendererList[i] = lineRenderer;
        }
        spline.SetActive(UIManager.Singleton.toggleSplineFlag);
        UILoading.transform.Find("Canvas").gameObject.SetActive(false);
    }

    public void toggleSpline()
    {
        GameObject spline = GameObject.Find("Spline");
        spline.SetActive(!spline.activeSelf);
    }

    #region helper functions
    private Vector3 RaycastPoint(Vector3 p2)
    {
        Vector3 v = p2 - camParams.focalPoint;
        float dist = 10f;

        RaycastHit[] hits = Physics.RaycastAll(camParams.focalPoint, v, dist);
        RaycastHit hit = new RaycastHit();
        bool foundHit = false;

        //Take the closer raycast hit if there are multiple
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.layer == 31)
            {
                if (foundHit)
                {
                    float distHit_i = Vector3.Distance(hits[i].point, camParams.focalPoint);
                    float distHit = Vector3.Distance(hit.point, camParams.focalPoint);
                    if (distHit_i < distHit)
                    {
                        hit = hits[i];
                    }
                }
                else
                {
                    hit = hits[i];
                    foundHit = true;
                }  
            }
        }

        if (!foundHit)
        {
            hit.point = camParams.focalPoint - camParams.vz * 0.5f;
            hit.normal = camParams.vz;
        }
        else
        {
            hit.point = hit.point;// + hit.normal * 0.001f;
        }

        return hit.point;
    }

    //Returns the x,y image coordinates transformed into X,Y,Z coordinates on the projected canvas
    private Vector3 ConvertUV2KyleXYZ(Vector2 imgCoords)
    {
        float reducedWidth = 920f + 40f;//The last 40f is emperical bandaid
        float imageWidth = 3904f - reducedWidth;
        float imageHeight = 2196f;
        float U = (imgCoords.x - reducedWidth / 2) / imageWidth - 0.5f + 0.007f;//The last 0.007f is emperical bandaid
        float V = (imgCoords.y / imageHeight) * (0.35f + 0.39f) - 0.39f + 0.040f;//The last 0.040f is emperical bandaid
        Vector2 UV = new Vector2(U, V);
        Vector3 targetPoint = m_Canvas.transform.TransformPoint(new Vector3(UV.x, -UV.y, 0)); //Flip the y axis
        return targetPoint;
    }

    //In theory this should return the X,Y,Z coordinates transforemd into x,y image coordinates
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

    private cameraParameters calculateCameraParameters(Matrix4x4 c2w)
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

    private class cameraParameters
    {
        public Vector3 vx;
        public Vector3 vy;
        public Vector3 vz;
        public Vector3 focalPoint;
        public float imageW;
        public float imageH;
    }

    //Find the closest point on any of the spline component lines
    //Use binary search to increase efficiency assuming these
    //splines have >100 points each
    public Vector3 GetClosestPointOnLine(Vector3 point, LineRenderer[] lines)
    {
        int closestLineIndex = -1;
        int closestPointIndex = -1;
        float closestDistance = Mathf.Infinity;

        foreach (var line in lines)
        {
            int start = 0;
            int end = line.positionCount - 1;

            while (start <= end)
            {
                int mid = (start + end) / 2;

                Vector3 lineStart = line.GetPosition(mid);
                Vector3 lineEnd = line.GetPosition(mid + 1);

                Vector3 projectedPoint = Vector3.Project(point - lineStart, lineEnd - lineStart) + lineStart;

                float distance = Vector3.Distance(projectedPoint, point);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestLineIndex = Array.IndexOf(lines, line);
                    closestPointIndex = mid;
                }

                if (Vector3.Dot(point - lineStart, lineEnd - lineStart) > 0f)
                {
                    start = mid + 1;
                }
                else
                {
                    end = mid - 1;
                }
            }
        }

        if (closestLineIndex >= 0 && closestPointIndex >= 0)
        {
            Vector3 closestLineStart = lines[closestLineIndex].GetPosition(closestPointIndex);

            return closestLineStart;
        }
        else
        {
            Debug.LogError("No valid closest point found");
            return point; // or some other default value
        }

        /*
        Vector3 closestLineNext = lines[closestLineIndex].GetPosition(closestPointIndex + 1);
        Vector3 closestLinePrev = lines[closestLineIndex].GetPosition(closestPointIndex - 1);

        Debug.Log($"Closestlinestart = {closestLineStart.x},{closestLineStart.y},{closestLineStart.z}");
        Debug.Log($"closestLineNext = {closestLineNext.x},{closestLineNext.y},{closestLineNext.z}");
        Debug.Log($"closestLinePrev = {closestLinePrev.x},{closestLinePrev.y},{closestLinePrev.z}");

        Vector3 closestPointNext = Vector3.Project(point - closestLineStart, closestLineNext - closestLineStart) + closestLineStart;
        Vector3 closestPointPrev = Vector3.Project(point - closestLineStart, closestLinePrev - closestLineStart) + closestLineStart;

        Debug.Log($"closestPointNext = {closestPointNext.x},{closestPointNext.y},{closestPointNext.z}");
        Debug.Log($"closestPointPrev = {closestPointPrev.x},{closestPointPrev.y},{closestPointPrev.z}");
        */

        //Right now the projection is not giving the behavior I want.
        //However, the closest point is correct.
        //(Outside of z axis issues)

        //TODO: Add an interpolation check on the adjacent points and 
        //itterate until some threshold is reached to find the 
        //closest point on the line
    }

    private Vector3 intersection(Vector3 l0, Vector3 lv, Vector3 p0, Vector3 n)
    {
        Vector3 intersectionPoint = l0 + (Vector3.Dot((p0 - l0), n) / Vector3.Dot(lv, n)) * lv;
        return intersectionPoint;
    }

    public void updateThickness(GameObject interactingObject)
    {
        Vector3 pos = interactingObject.transform.position;

        Vector3 canvasPosition = m_Canvas.transform.position;
        Vector3 canvasNormal = -m_Canvas.transform.forward;
        Vector3 xyzCanvas = intersection(pos, pos - camParams.focalPoint, canvasPosition, canvasNormal);
        Vector2 xy = ReverseKyleXYZ2UV(xyzCanvas);
        uiManager.updateThicknessWrapper(interactingObject, xy);
    }

    public Vector2 get_xy_from_XYZ(Vector3 XYZ)
    {
        Vector3 canvasPosition = m_Canvas.transform.position;
        Vector3 canvasNormal = -m_Canvas.transform.forward;
        Vector3 xyzCanvas = intersection(XYZ, XYZ - camParams.focalPoint, canvasPosition, canvasNormal);
        Vector2 xy = ReverseKyleXYZ2UV(xyzCanvas);
        return xy;
    }

    public void parseThickness(GameObject interactingObject, float thicknessPixels)
    {
        float x = interactingObject.transform.position.x;
        float y = interactingObject.transform.position.y;

        Vector3 p1 = RaycastPoint(ConvertUV2KyleXYZ(new Vector2(x - thicknessPixels / 2, y)));
        Vector3 p2 = RaycastPoint(ConvertUV2KyleXYZ(new Vector2(x + thicknessPixels / 2, y)));
        float tWorld = Vector3.Distance(p1, p2);
        if ((InteractionManager.InteractionMethodChosen == LAV_InteractionManager.InteractionMethod.CV_Only) || (InteractionManager.InteractionMethodChosen == LAV_InteractionManager.InteractionMethod.CV_then_Human))
        {
            pointData += $"{tWorld})  ";
        }

        if (tWorld < 0.0015875f)
        {
            tWorld = 0.0015875f;
        }

        TextMeshPro SphereDiameter;
        Transform PointDisplay = interactingObject.transform;

        //Update the sphere's scale
        Vector3 PointScale = PointDisplay.localScale;
        PointScale = new Vector3(tWorld, tWorld, tWorld);
        PointDisplay.localScale = PointScale;

        //Update the label on the sphere
        SphereDiameter = interactingObject.transform.Find("SphereDiameter").GetComponent<TextMeshPro>();
        SphereDiameter.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 2f);
        //SphereDiameter.transform.position = SphereDiameter.transform.position - InteractionManager.zAxisOverride * 0.01f;
        //SphereDiameter.SetText(PointDisplay.transform.localScale.x.ToString("F3") + "m");
        Debug.Log($"Calculated thickess = {(PointDisplay.transform.localScale.x * 1000).ToString("F3")} mm");

        SphereDiameter = interactingObject.transform.Find("SphereDiameter").GetComponent<TextMeshPro>();
        SphereDiameter.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 2f);
        SphereDiameter.SetText((PointDisplay.transform.localScale.x * 39.3701f).ToString("F3") + "in");
        SphereDiameter.transform.localScale = new Vector3(100f, 100f, 100f) / (PointDisplay.localScale.x / 0.0015875f);
        SphereDiameter.transform.localPosition = new Vector3(-4.6100049f, 3.51000047f, 16.651638f) / (PointDisplay.localScale.x / 0.0015875f);

        GameObject circleObject = interactingObject.transform.Find("Circle").gameObject;
        circleObject.transform.localScale = new Vector3(10, 10, 10) / (PointDisplay.localScale.x / 0.0015875f); ;
    }
    #endregion
}
