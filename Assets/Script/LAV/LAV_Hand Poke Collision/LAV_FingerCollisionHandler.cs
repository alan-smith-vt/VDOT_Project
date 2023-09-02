using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script per creazione punto al tocco
public class LAV_FingerCollisionHandler : MonoBehaviour
{
    private LAV_InteractionManager InteractionManager;
    private LAV_FaultManager faultManager;
    private Vector3 selectionPt, selectionNorm;
    private Quaternion rotPt;

    public float colliderSize = .03f;
    public GameObject InstantiateObject; //This only matter when you want to use the SpawnObject method to test
    private GameObject pokeSphere;
    public Handedness desiredHand;
    public TrackedHandJoint desiredJoint;
    public float timeoutPeriod = 1f;
    public bool isColliderVisible = false;
    public bool isDragEnabled = false;
    private float nextTimeoutPeriodEnd;

    void Start()
    {
        InteractionManager = GameObject.Find("FaultCollection").GetComponent<LAV_InteractionManager>();
        faultManager = GameObject.Find("FaultCollection").GetComponent<LAV_FaultManager>();
        selectionPt = selectionNorm = Vector3.zero;
        rotPt = Quaternion.identity;

        pokeSphere = this.gameObject;
        this.GetComponent<MeshRenderer>().enabled = isColliderVisible;
        this.transform.localScale = new Vector3(colliderSize, colliderSize, colliderSize);
    }

    void Update()
    {
        UpdateHand();
    }

    private void UpdateHand()
    {
        if (HandJointUtils.TryGetJointPose(desiredJoint, desiredHand, out MixedRealityPose pose)) //pose è la posizione e rotazione
        {
            if (pose != null)
            {
                pokeSphere.transform.position = pose.Position;
                this.GetComponent<SphereCollider>().enabled = true;
                this.GetComponent<MeshRenderer>().enabled = isColliderVisible;
            }
        }
        else
        {
            this.GetComponent<SphereCollider>().enabled = false;
            this.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    //This method is only for testing as you would only spawn new point from Kyle's script
    //public void SpawnObject(Vector3 pokeposition)
    //{
    //    var temp = Instantiate(InstantiateObject);
    //    temp.transform.position = pokeposition;
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Left)
        {
            desiredHand = Handedness.Left;
        }
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Right)
        {
            desiredHand = Handedness.Right;
        }

        //if(other.name.Contains("Spatial") && !(nextTimeoutPeriodEnd>Time.time)) 
        if (other.gameObject.layer == 31 && !(nextTimeoutPeriodEnd > Time.time) && InteractionManager.InteractionTypeMethod != LAV_InteractionManager.InteractionType.Far_Pointer_Right && InteractionManager.InteractionTypeMethod != LAV_InteractionManager.InteractionType.Far_Pointer_Left)
        {
            nextTimeoutPeriodEnd = Time.time + timeoutPeriod;
            //SpawnObject(this.transform.position);

            var result = pokeSphere.transform.position;
            selectionPt = result - result.normalized * 0.01f;
            selectionNorm = - result.normalized;

            if (selectionPt == Vector3.zero)
                return;

            faultManager.AddFaultPoint(selectionPt, rotPt, selectionNorm);
            selectionPt = Vector3.zero;
            selectionNorm = Vector3.zero;
        }

    }

    private void OnTriggerStay(Collider other)
    {
    //    if (other.name.Contains("Spatial") && isDragEnabled)
    //    {
    //        //SpawnObject(this.transform.position);
    //    }
    }

    private void OnTriggerExit(Collider other)
    {
    }
}
