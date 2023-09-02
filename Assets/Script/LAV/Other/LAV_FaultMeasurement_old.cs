using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;


//public class FaultPoint //raccolta dati numerici posizione punti - la classe è più flessibile di una struct perchè puoi inserire variabili anche al di fuori delle () e può essere chiamata in altri script direttamente
//{
//    public FaultPoint(Vector3 p, Quaternion rot, Vector3 n)
//    {
//        position = p;
//        rotation = rot;
//        norm = n;
//        diameter = 0.010f;
//    }
//    public Vector3 position;
//    public Quaternion rotation;
//    public Vector3 norm;
//    public float diameter;
//}

////Script per aggiungere punti, mostrare bounding box e scritte misure
//public class LAV_FaultMeasurement_old : MonoBehaviour
//{
//    public FaultPoint currentPoint;
//    public bool showBounds;
//    [HideInInspector] public GameObject selectedPoint;

//    private float length, width;
//    private Vector3 boxZSum, boxZMean;
//    public List<FaultPoint> faultPoints; //insieme di punti creati
//    private Bounds faultBounds;
//    private Transform boundsDisplay, faultPoint;
//    private TextMeshPro heightText, widthText, depthText, SphereDiameter;
//    public Quaternion Correction_angles;

//    void Awake() {
//        length = 0f;
//        width = 0f;
//        boxZSum = Vector3.zero;
//        boxZMean = Vector3.zero;
//        faultPoints = new List<FaultPoint>();
//        faultPoint = transform.Find("FaultPoint");
//        Debug.Log("FM start: " + faultPoint.name);
//        boundsDisplay = transform.Find("BoundsDisplay");
//        boundsDisplay.gameObject.SetActive(showBounds); //si attiva se è spuntato 

//        heightText = transform.Find("TextObjects/HeightText").GetComponent<TextMeshPro>();
//        widthText = transform.Find("TextObjects/WidthText").GetComponent<TextMeshPro>();
//        depthText = transform.Find("TextObjects/DepthText").GetComponent<TextMeshPro>();
//        heightText.gameObject.SetActive(showBounds);
//        widthText.gameObject.SetActive(showBounds);
//        depthText.gameObject.SetActive(showBounds);
//    }
//    void Update()
//    {
//    }
//    public void AddFaultPoint(Vector3 pos, Quaternion rot, Vector3 norm) 
//    {
//        //Transform newPt = Instantiate(faultPoint, pos, Quaternion.identity, transform);
//        Transform newPt = Instantiate(faultPoint, pos, rot, transform);
//        newPt.name = "FaultPoint_" + faultPoints.Count.ToString();
//        newPt.GetComponent<MeshRenderer>().enabled = true;
//        selectedPoint = newPt.gameObject;  //serve per richiamare il punto nello script FaultManager
//        SphereDiameter = newPt.transform.Find("SphereDiameter").GetComponent<TextMeshPro>();
//        SphereDiameter.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 2f);
//        currentPoint = new FaultPoint(pos, rot, norm);
//        faultPoints.Add(currentPoint); //aggiunge punto alla lista
        
//        boxZSum += norm;
//        boxZMean = boxZSum / faultPoints.Count;
//        CalculateMinimumBoundingBox(); 
//    }
//    public void RemoveFaultPoint(Vector3 pos) {
//        faultPoints.RemoveAll(x => x.position == pos);
//    }
//    public void CalculateMinimumBoundingBox() 
//    {
//        faultBounds = GeometryUtility.CalculateBounds(faultPoints.Select(x => x.position).ToArray(), Matrix4x4.identity);
//        //Matrix4x4 tr = Matrix4x4.TRS(faultBounds.center, Quaternion.LookRotation(boxZMean), Vector3.one); non serve
//        //faultBounds = GeometryUtility.CalculateBounds(faultPoints.Select(x => x.position).ToArray(), Matrix4x4.identity); ripetizione
//        Quaternion lookAtNorm = Quaternion.LookRotation(boxZMean);
//        Quaternion normalizeRot = Quaternion.Inverse(lookAtNorm);
//        Vector3[] points = faultPoints.Select(x => normalizeRot * x.position).ToArray();
//        float minX = points.Min(v => v.x);
//        float minY = points.Min(v => v.y);
//        float minZ = points.Min(v => v.z);
//        float maxX = points.Max(v => v.x);
//        float maxY = points.Max(v => v.y);
//        float maxZ = points.Max(v => v.z);

//        faultBounds.size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);

//        if (showBounds) 
//        {
//            //Debug.Log("Updating bounds");
//            boundsDisplay.localPosition = faultBounds.center;
//            Vector3 scale = faultBounds.size;
//            boundsDisplay.localScale = scale;
//            boundsDisplay.forward = boxZMean;
//            //boundsDisplay.transform.rotation = Correction_angles;

//            heightText.transform.parent.localPosition = faultBounds.center;
//            heightText.transform.parent.forward = boxZMean;
//            //heightText.transform.parent.rotation = Correction_angles;

//            widthText.transform.localPosition = new Vector3(0f, faultBounds.extents.y + 0.05f, 0f);
//            heightText.transform.localPosition = new Vector3(faultBounds.extents.x + 0.05f, 0f, 0f);

//            heightText.SetText(faultBounds.size.y.ToString("F2") + "m");
//            widthText.SetText(faultBounds.size.x.ToString("F2") + "m");

//            if (faultBounds.size.z > 0.01) 
//            {
//                depthText.gameObject.SetActive(true);
//                depthText.transform.localPosition = new Vector3(0f, 0f, 0.1f);
//                depthText.SetText(faultBounds.size.z.ToString("F2") + "m");
//            } 
//            else 
//            {
//                depthText.SetText("0.00 m");
//                depthText.gameObject.SetActive(false);
//            }
//        }
//    }
//}

