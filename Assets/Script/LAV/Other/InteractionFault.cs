using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Script per creazione punto al tocco
public class InteractionFault : MonoBehaviour
{
    private LAV_InteractionManager InteractionManager;
    private Vector3 selectionPt, selectionNorm;
    private Quaternion rotationPt;
    private LAV_FaultMeasurement currentFault;

    private float colliderSize = .03f;
    private GameObject pokeSphere;
    public Handedness desiredHand;
    public TrackedHandJoint desiredJoint;
    public TrackedHandJoint testJoint;
    public float timeoutPeriod = 1f;
    public bool isColliderVisible = false;
    private float nextTimeoutPeriodEnd;

    public GameObject currentSelectedPoint = null;
    public float maxInteractionDistance;
    public ScaleDirection scaleDirection;
    private float nextScaleTimeoutPeriodEnd;
    public bool isSnappingEnabled;
    private Vector3 currentObjectStartingScale;
    public bool isInteractionStarted = false;
    public float scaleDeadeZoneScale = 1.1f;
    private float scaleTimeoutPeriod = .1f;

    private TextMeshPro SphereDiameter;
    public enum ScaleDirection
    {
        pull_increase = 0,
        pull_decrease = 1
    }

    private bool UI = false;
    void Start()
    {
        InteractionManager = GameObject.Find("FaultInteraction").GetComponent<LAV_InteractionManager>();
        currentFault = GameObject.Find("Fault").GetComponent<LAV_FaultMeasurement>();
        selectionPt = selectionNorm = Vector3.zero;
        rotationPt = Quaternion.identity;

        pokeSphere = this.gameObject;
        this.GetComponent<MeshRenderer>().enabled = isColliderVisible;
        this.transform.localScale = new Vector3(colliderSize, colliderSize, colliderSize);
    }
    void Update()
    {
        UpdateHand();
        GetScaleStartingPosition();

        int idx = int.Parse(currentSelectedPoint.name.Split('_')[1]); //prende il secondo termine di un array dividendo il nome da un carattere e lo converte da string a int per richiamare uno specifico punto della lista
        currentFault.faultPoints[idx].diameter = currentSelectedPoint.transform.localScale.x;
        if (currentSelectedPoint.transform.position.x != currentFault.faultPoints[idx].position.x)
        {
            currentFault.faultPoints[idx].position = currentSelectedPoint.transform.position;
            currentFault.CalculateMinimumBoundingBox();
        }
        SphereDiameter = currentSelectedPoint.transform.Find("SphereDiameter").GetComponent<TextMeshPro>();
        SphereDiameter.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 2f);
        SphereDiameter.SetText(currentSelectedPoint.transform.localScale.x.ToString("F3") + "m");

        if (UI == true && !(nextTimeoutPeriodEnd > Time.time))
        {
            nextTimeoutPeriodEnd = Time.time + timeoutPeriod;
            UI = false;
        }
    }
    private void UpdateHand()
    {
        if (HandJointUtils.TryGetJointPose(desiredJoint, desiredHand, out MixedRealityPose pose) /*&& UI == false*/) //pose è la posizione e rotazione
        {
            if (pose != null)
            {
                pokeSphere.transform.position = pose.Position;
                pokeSphere.transform.rotation = pose.Rotation;
                this.GetComponent<SphereCollider>().enabled = true;
                this.GetComponent<MeshRenderer>().enabled = isColliderVisible;
            }
        }
        else
        {
            this.GetComponent<SphereCollider>().enabled = false;
            this.GetComponent<MeshRenderer>().enabled = false;
        }
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Left)
        {
            desiredHand = Handedness.Left;
        }
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Right)
        {
            desiredHand = Handedness.Right;
        }
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Both)
        {
            desiredHand = Handedness.Both;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 31 && !(nextTimeoutPeriodEnd > Time.time) && InteractionManager.InteractionTypeMethod != LAV_InteractionManager.InteractionType.Far_Pointer_Right && InteractionManager.InteractionTypeMethod != LAV_InteractionManager.InteractionType.Far_Pointer_Left)
        {
            nextTimeoutPeriodEnd = Time.time + timeoutPeriod;

            var result = pokeSphere.transform.position;
            selectionPt = result - result.normalized * 0.01f;
            rotationPt = pokeSphere.transform.rotation;
            //selectionNorm = -result.normalized;
            selectionNorm = new Vector3(-0.01f, 0, 0.1f);

            if (selectionPt == Vector3.zero)
                return;

            Debug.Log(string.Format("Adding point {0} {1}", selectionPt.ToString("F5"), selectionNorm.ToString("F5")));
            currentFault.AddFaultPoint(selectionPt, rotationPt, selectionNorm);
            selectionPt = Vector3.zero;
            rotationPt = Quaternion.identity;
            selectionNorm = Vector3.zero;
        }
        if (other.gameObject.layer == 5)
        {
            UI = true;
            this.GetComponent<SphereCollider>().enabled = false;
            this.GetComponent<MeshRenderer>().enabled = false;
        }
    }
    private void OnTriggerStay(Collider other)
    { }
    private void OnTriggerExit(Collider other)
    { }
    public void Collider_ON()
    {
        isColliderVisible = true;
    }
    public void Collider_OFF()
    {
        isColliderVisible = false;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------

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
                    //Debug.Log(tempmeasure);
                    if (tempmeasure > maxInteractionDistance)
                    {
                        //Debug.Log(maxInteractionDistance);
                        maxInteractionDistance = tempmeasure;
                    }
                }
            }
        }
    }
    public void ScalePlacedObject(float scalevalue)
    {
        //Debug.Log(scalevalue);
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
                float c = currentObjectStartingScale.x + (0.005f * currentsnapnum);
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
    public float GetScaleValue(float distancevalue)
    {
        var temp = Mathf.InverseLerp(0, maxInteractionDistance, distancevalue);
        return temp;
    }
    public void SetCurrentObject(GameObject currentobject)
    {
        //currentobject.GetComponent<SphereHandler>().ChangeMaterial(true);
        currentSelectedPoint = currentobject;
        maxInteractionDistance = 0;
        currentObjectStartingScale = currentobject.transform.localScale;
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
}
