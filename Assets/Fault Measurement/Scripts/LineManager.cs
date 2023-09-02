using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{
    private Transform lineObject, linePoint;
    private List<Transform> linePoints;

    public LineRenderer currentLine;

    void Awake() {
        currentLine = null;
        lineObject = transform.Find("Line");
        linePoint = lineObject.transform.Find("LinePoint");
        linePoints = new List<Transform>();
    }

    public void CreateNewLine(bool resetCurrent) {
        if (resetCurrent) {
            for (int i = 0; i < currentLine.transform.childCount; i++) {
                RemoveLinePoint(currentLine.transform.GetChild(i));
                Destroy(currentLine.transform.GetChild(i).gameObject);
            }
            currentLine.positionCount = 0;

        } else {
            currentLine = Instantiate(lineObject, lineObject.parent).GetComponent<LineRenderer>();
            currentLine.alignment = LineAlignment.TransformZ;
            linePoints.Clear();
            currentLine.positionCount = 0;
        }
    }

    public void AddLinePoint(Vector3 pos, Vector3 norm) {
        if (currentLine == null) {
            CreateNewLine(false);
        }

        if (linePoints.Count > 0) {
            currentLine.enabled = true;
        } else {
            currentLine.transform.forward = -norm;
        }
        Transform newPt = Instantiate(linePoint, pos, currentLine.transform.rotation, currentLine.transform);
        newPt.GetComponent<MeshRenderer>().enabled = true;
        linePoints.Add(newPt);
        Debug.Log(string.Format("Currently {0} line points", linePoints.Count));
        currentLine.SetPositions(LinePointsToPositionArray());
    }

    public void RemoveLinePoint(Transform tr) {
        linePoints.Remove(tr);
        if (linePoints.Count > 1) {
            currentLine.SetPositions(LinePointsToPositionArray());
        } else {
            currentLine.enabled = false;
        }
    }

    private Vector3[] LinePointsToPositionArray() {
        Vector3[] positions = new Vector3[linePoints.Count];
        for (int i = 0; i < linePoints.Count; i++)
            positions[i] = linePoints[i].position;
        currentLine.positionCount = positions.Length;

        return positions;
    }
}
