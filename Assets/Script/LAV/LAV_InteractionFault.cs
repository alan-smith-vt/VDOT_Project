using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Script per creazione punto al tocco
public class LAV_InteractionFault : MonoBehaviour
{
    private LAV_InteractionManager InteractionManager;
    private LAV_MenuSystemHandler LAV_MenuSystemHandler;
    private Vector3 selectionPt, selectionNorm;
    private Quaternion rotationPt;
    private LAV_FaultMeasurement currentFault;

    private float colliderSize = .03f;
    private GameObject pokeSphere;
    public Handedness desiredHand;
    public TrackedHandJoint desiredJoint;
    public float timeoutPeriod = 1f;
    public bool isColliderVisible = false;
    private float nextTimeoutPeriodEnd;

    [HideInInspector] public float created_points_trial = 0;

    private bool UI = false;
    void Start()
    {
        LAV_MenuSystemHandler = GameObject.Find("Menu_Crack").GetComponent<LAV_MenuSystemHandler>();
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
        {  desiredHand = Handedness.Left;  }
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Right)
        { desiredHand = Handedness.Right; }
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Both)
        { desiredHand = Handedness.Both; }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 31 && other.gameObject.layer != 3 && !(nextTimeoutPeriodEnd > Time.time) && InteractionManager.InteractionTypeMethod != LAV_InteractionManager.InteractionType.Far_Pointer_Right && InteractionManager.InteractionTypeMethod != LAV_InteractionManager.InteractionType.Far_Pointer_Left)
        {
            nextTimeoutPeriodEnd = Time.time + timeoutPeriod;

            var result = pokeSphere.transform.position;
            selectionPt = result - result.normalized * 0.01f;
            rotationPt = pokeSphere.transform.rotation;

            if (selectionPt == Vector3.zero)
                return;

            Debug.Log(string.Format("Trying to add point {0} {1}", selectionPt.ToString("F5"), selectionNorm.ToString("F5")));
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
        if (other.gameObject.layer == 31 && LAV_MenuSystemHandler.Manual_Test_Menu.activeSelf || LAV_MenuSystemHandler.CV_Test_Menu.activeSelf)
        {
            created_points_trial++;
            Debug.Log("created_points_trial: " + created_points_trial);
        }
    }
    private void OnTriggerStay(Collider other)
    {}
    private void OnTriggerExit(Collider other)
    {}
    public void Collider_ON()
    {  isColliderVisible = true; }
    public void Collider_OFF()
    { isColliderVisible = false; }
}

