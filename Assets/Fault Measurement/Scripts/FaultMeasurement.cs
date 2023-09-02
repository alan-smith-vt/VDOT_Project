using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class FaultMeasurement : MonoBehaviour
{
    public bool showBounds;
    [HideInInspector] public GameObject selectedPoint;


    private float length, width;
    private Vector3 boxZSum, boxZMean;
    private List<FaultPoint> faultPoints;
    private Bounds faultBounds;
    private Transform boundsDisplay, faultPoint;
    private TextMeshPro heightText, widthText, depthText;
    
    struct FaultPoint
    {
        public FaultPoint(Vector3 p, Vector3 n) {
            position = p;
            norm = n;
        }

        public Vector3 position;
        public Vector3 norm;
    }

    void Awake() {
        length = 0f;
        width = 0f;
        boxZSum = Vector3.zero;
        boxZMean = Vector3.zero;
        faultPoints = new List<FaultPoint>();
        faultPoint = transform.Find("FaultPoint");
        Debug.Log("FM start: " + faultPoint.name);
        boundsDisplay = transform.Find("BoundsDisplay");
        boundsDisplay.gameObject.SetActive(showBounds);

        heightText = transform.Find("TextObjects/HeightText").GetComponent<TextMeshPro>();
        widthText = transform.Find("TextObjects/WidthText").GetComponent<TextMeshPro>();
        depthText = transform.Find("TextObjects/DepthText").GetComponent<TextMeshPro>();
        heightText.gameObject.SetActive(showBounds);
        widthText.gameObject.SetActive(showBounds);
        depthText.gameObject.SetActive(showBounds);
    }

    public void AddFaultPoint(Vector3 pos, Vector3 norm) {
        Transform newPt = Instantiate(faultPoint, pos, Quaternion.identity, transform);
        newPt.GetComponent<MeshRenderer>().enabled = true;
        selectedPoint = newPt.gameObject;
        faultPoints.Add(new FaultPoint(pos, norm));

        boxZSum += norm;
        boxZMean = boxZSum / faultPoints.Count;

        CalculateMinimumBoundingBox();
    }

    public void RemoveFaultPoint(Vector3 pos) {
        faultPoints.RemoveAll(x => x.position == pos);
    }

    
    private void CalculateMinimumBoundingBox() {
        faultBounds = GeometryUtility.CalculateBounds(faultPoints.Select(x => x.position).ToArray(), Matrix4x4.identity);
        Matrix4x4 tr = Matrix4x4.TRS(faultBounds.center, Quaternion.LookRotation(boxZMean), Vector3.one);
        faultBounds = GeometryUtility.CalculateBounds(faultPoints.Select(x => x.position).ToArray(), Matrix4x4.identity);
        Quaternion lookAtNorm = Quaternion.LookRotation(boxZMean);
        Quaternion normalizeRot = Quaternion.Inverse(lookAtNorm);
        Vector3[] points = faultPoints.Select(x => normalizeRot * x.position).ToArray();
        float minX = points.Min(v => v.x);
        float minY = points.Min(v => v.y);
        float minZ = points.Min(v => v.z);
        float maxX = points.Max(v => v.x);
        float maxY = points.Max(v => v.y);
        float maxZ = points.Max(v => v.z);

        faultBounds.size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);

        if (showBounds) {
            Debug.Log("Updating bounds");
            boundsDisplay.localPosition = faultBounds.center;
            Vector3 scale = faultBounds.size;
            //scale.z = 0.01f;
            boundsDisplay.localScale = scale;
            boundsDisplay.forward = boxZMean;

            heightText.transform.parent.localPosition = faultBounds.center;
            heightText.transform.parent.forward = boxZMean;

            widthText.transform.localPosition = new Vector3(0f, faultBounds.extents.y + 0.05f, 0f);
            heightText.transform.localPosition = new Vector3(faultBounds.extents.x + 0.05f, 0f, 0f);

            heightText.SetText(faultBounds.size.y.ToString("F2") + "m");
            widthText.SetText(faultBounds.size.x.ToString("F2") + "m");

            if (faultBounds.size.z > 0.01) {
                depthText.gameObject.SetActive(true);
                depthText.transform.localPosition = new Vector3(0f, 0f, 0.1f);
                depthText.SetText(faultBounds.size.z.ToString("F2") + "m");
            } else {
                depthText.gameObject.SetActive(false);
            }

        }
    }
    

    /*
    private void CalculateMinimumBoundingBox() {
        Quaternion lookAtNorm = Quaternion.LookRotation(boxZMean);
        Quaternion normalizeRot = Quaternion.Inverse(lookAtNorm);
        Vector3[] points = faultPoints.Select(x => normalizeRot * x.position).ToArray();
        float minX = points.Min(v => v.x);
        float minY = points.Min(v => v.y);
        float minZ = points.Min(v => v.z);
        float maxX = points.Max(v => v.x);
        float maxY = points.Max(v => v.y);
        float maxZ = points.Max(v => v.z);

        faultBounds.size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
        faultBounds.center = lookAtNorm * new Vector3((maxX - minX) / 2f, (maxY - minY) / 2f, (maxZ - minZ) / 2f);

        if (showBounds) {
            Debug.Log("Updating bounds");
            boundsDisplay.localPosition = faultBounds.center;
            boundsDisplay.localScale = faultBounds.size;
            boundsDisplay.forward = boxZMean;
        }
    }
    */
}

