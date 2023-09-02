using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerCollisionHandler : MonoBehaviour
{
    //variable declaration
    [Header("References")]
    public GameObject placementDeadZoneRef;
    public GameObject scaleDeadZoneRef;
    public GameObject InstantiateObject;
    //This only matter when you want to use the SpawnObject method to test
    private GameObject pokeSphere;
    [Header("Hand Settings")]
    public Handedness desiredHand;
    public TrackedHandJoint desiredJoint;
    public TrackedHandJoint testJoint;
    [Header("Collider Settings")]
    public bool isColliderVisible = false;
    public float colliderSize = .03f;
    [Header("Placement Settings")]
    public bool isDragEnabled = false;
    public bool isSnappingEnabled;
    public float snappingScaleNum = .1f;
    public ScaleDirection scaleDirection;
    public float timeoutPeriod = 1f;
    private float nextTimeoutPeriodEnd;
    public GameObject currentSelectedPoint = null;
    public float placementDeadZoneScale = 1.1f;
    public float scaleDeadeZoneScale = 1.1f;
    [Header("Placement Settings------")]
    private float scaleTimeoutPeriod = .1f;
    private float nextScaleTimeoutPeriodEnd;
    public bool isInsidePoint = false;
    public bool isInsideInteractionDeadZone = true;
    public bool isInteractionStarted = false;
    public float maxInteractionDistance;
    private Vector3 currentObjectStartingScale;
    public enum DeadZoneTypes
    {
        Placement = 0,
        Scale = 1
    }
    public enum ScaleDirection
    {
        pull_increase = 0,
        pull_decrease = 1
    }
    private bool IsInteractionStarted
    {
        get { return isInteractionStarted; }
        set { isInteractionStarted = value; }
    }
    //need to make this one variable
    void Start()
    {
        pokeSphere = this.gameObject;
        this.GetComponent<MeshRenderer>().enabled = isColliderVisible;
        this.transform.localScale = new Vector3(colliderSize, colliderSize, colliderSize);
        placementDeadZoneRef.GetComponent<MeshRenderer>().enabled = isColliderVisible;
        placementDeadZoneRef.transform.localScale = new Vector3(placementDeadZoneScale, placementDeadZoneScale, placementDeadZoneScale);
        scaleDeadZoneRef.transform.localScale = new Vector3(scaleDeadeZoneScale, scaleDeadeZoneScale, scaleDeadeZoneScale);
    }
    void Update()
    {
        UpdateHand();
        //GetHandBodyDistance();
        GetScaleStartingPosition();
    }
    #region Update hand and Distances
    private void UpdateHand()
    {
        if (HandJointUtils.TryGetJointPose(desiredJoint, desiredHand, out MixedRealityPose pose))
        {
            if (pose != null)
            {
                pokeSphere.transform.position = pose.Position;
                placementDeadZoneRef.transform.position = pose.Position;
                this.GetComponent<SphereCollider>().enabled = true;
                this.GetComponent<MeshRenderer>().enabled = isColliderVisible;
                placementDeadZoneRef.GetComponent<SphereCollider>().enabled = true;
                scaleDeadZoneRef.GetComponent<SphereCollider>().enabled = true;
                placementDeadZoneRef.GetComponent<MeshRenderer>().enabled = isColliderVisible;
                scaleDeadZoneRef.GetComponent<MeshRenderer>().enabled = isColliderVisible;
                //Debug.Log("Hello");
            }
        }
        else
        {
        }
    }//handles tracking the orbs to the current hands
    public void GetScaleStartingPosition()
    {
        if (HandJointUtils.TryGetJointPose(testJoint, desiredHand, out MixedRealityPose pose))
        {
            if (pose != null && currentSelectedPoint != null)
            {
                //Debug.Log($"Finger to current obj {Vector3.Distance(pose.Position, currentSelectedPoint.transform.position)}");
                if (Vector3.Distance(pose.Position, currentSelectedPoint.transform.position) > 0)
                {
                    //GetHandBodyDistance();
                    //Debug.Log("past the scale dead zone");
                    var tempmeasure = GetHandBodyDistance();
                    if (tempmeasure > maxInteractionDistance)
                    {
                        maxInteractionDistance = tempmeasure;
                    }
                }
            }
        }
    }
    public float GetScaleValue(float distancevalue)
    {
        var temp = Mathf.InverseLerp(0, maxInteractionDistance - scaleDeadeZoneScale, distancevalue);
        return temp;
    }
    public void SetCurrentObject(GameObject currentobject)
    {
        //currentobject.GetComponent<SphereHandler>().ChangeMaterial(true);
        currentSelectedPoint = currentobject;
        maxInteractionDistance = 0;
        currentObjectStartingScale = currentobject.transform.localScale;

    }
    public void ScalePlacedObject(float scalevalue)
    {
        if (scaleDirection == ScaleDirection.pull_increase && !(nextScaleTimeoutPeriodEnd > Time.time))
        {
            if (!isSnappingEnabled)
            {
                var temp = 1 - scalevalue;
                temp = (temp + 1.1f) * currentObjectStartingScale.x;
                currentSelectedPoint.transform.localScale = new Vector3(temp, temp, temp);//+currentscale;
            }
            else
            {
                var temp = 1 - scalevalue;
                var snapincrements = maxInteractionDistance / 5;
                var currentsnapnum = Mathf.RoundToInt(temp / snapincrements);
                float c = currentObjectStartingScale.x + (0.0015875f * currentsnapnum);
                //float c = currentObjectStartingScale.x + (0.03f * currentsnapnum);
                //Debug.Log($"The current snap is {currentsnapnum} and the new scale is {c}");
                currentSelectedPoint.transform.localScale = new Vector3(c, c, c);
            }
        }
        else if (scaleDirection == ScaleDirection.pull_decrease)
        {

        }
    }
    public float GetHandBodyDistance()
    {
        if (HandJointUtils.TryGetJointPose(testJoint, desiredHand, out MixedRealityPose pose))
        {
            if (pose != null)
            {
                //Debug.Log($"Current scale value {GetScaleValue(Vector3.Distance(Camera.main.transform.position, pose.Position))}");
                //Debug.Log($"The interaction is {isInteractionStarted}");
                if (isInteractionStarted)
                {
                    ScalePlacedObject(GetScaleValue(Vector3.Distance(Camera.main.transform.position, pose.Position)));
                }


                return Vector3.Distance(Camera.main.transform.position, pose.Position);
            }
            return 0f;
        }
        else
        {
            return 0f;
            //Debug.Log($"Differece between is null");
        }
    }
    public void SphereEventReciever(bool eventstate)
    {
        isInsidePoint = eventstate;
        Debug.Log($"IsInsidePoint-S: {eventstate}");
    }
    public void FingerDeadEventReciever(DeadZoneTypes type, bool eventstate)
    {
        if (type == DeadZoneTypes.Placement)
        {
            isInsidePoint = eventstate;
            Debug.Log($"isInsidePoint-S: {eventstate}");
        }
        else if (type == DeadZoneTypes.Scale)
        {
            isInsideInteractionDeadZone = eventstate;
        }

    }
    public void SetInteractionMode(bool interactionstate)
    {
        isInteractionStarted = interactionstate;
        Debug.Log($"The interaction mode is{interactionstate}");
        nextScaleTimeoutPeriodEnd = Time.time + scaleTimeoutPeriod;

        if (interactionstate)
        {
            maxInteractionDistance = 0;
            if (currentSelectedPoint != null)
            {
                //currentSelectedPoint.GetComponent<SphereHandler>().ChangeMaterial(true);
            }
            //GetScaleStartingPosition();  //need to try this again
        }
        else
        {
            //currentSelectedPoint.GetComponent<SphereHandler>().ChangeMaterial(false);
        }
    }
    #endregion
    //This method is only for testing as you would only spawn new point from Kyle's script
    public void SpawnObject(Vector3 pokeposition)
    {
        var temp = Instantiate(InstantiateObject);
        temp.transform.position = pokeposition;
        SetCurrentObject(temp);
        //currentSelectedPoint = temp; //might need to change this
        //maxInteractionDistance = 0;
    }
    //The three collider events
    #region Colliders
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"This is the inside sphere {isInteractionStarted} {isInsidePoint}");
        if (other.name.Contains("Spatial") && !(nextTimeoutPeriodEnd > Time.time) && isInteractionStarted == false && isInsidePoint == false)
        {
            Debug.Log($"Inside for loop isInsidePoint: {isInsidePoint}");
            nextTimeoutPeriodEnd = Time.time + timeoutPeriod;
            SpawnObject(this.transform.position);
        }
        else
        {
            //Debug.Log($"Collider Hit {other.name}");
        }

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.name.Contains("Spatial") && isDragEnabled)
        {
            //SpawnObject(this.transform.position);
        }
    }
    private void OnTriggerExit(Collider other)
    {
    }
    #endregion
}
