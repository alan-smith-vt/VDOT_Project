using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LAV_InteractionManager : MonoBehaviour
{
    public enum InteractionType
    {
        Far_Pointer_Right,
        Far_Pointer_Left,
        Proximity_Pointer_Right,
        Proximity_Pointer_Left,
        Proximity_Pointer_Both,
        Proximity_Pointer_None,
    }

    public enum InteractionMethod
    {
        Manual_Only,
        CV_Only,
        CV_then_Human,
        Human_then_CV,
        Tutorial,
    }

    public InteractionType InteractionTypeMethod = InteractionType.Proximity_Pointer_Right;
    public InteractionMethod InteractionMethodChosen = InteractionMethod.CV_Only;

    GameObject planeObject;
    public Vector3 zAxisOverride;

    void Start()
    {
        Debug.Log($"Interaction method chosen = {InteractionMethodChosen}");
        Debug.Log($"Does interaction method chosen = CV? {InteractionMethodChosen == InteractionMethod.CV_Only}");
        Debug.Log($"Does interaction method chosen = manual? {InteractionMethodChosen == InteractionMethod.Manual_Only}");
        if (Application.isEditor)
        {
            planeObject = GameObject.Find("Plane");
        }
        else
        {
            planeObject = GameObject.Find("Crack Meshes/Plane");
        }
        
        zAxisOverride = planeObject.transform.up;

        GameObject faultBoundsDisplay = GameObject.Find("FaultCollection").transform.Find("Fault").Find("BoundsDisplay").gameObject;
        faultBoundsDisplay.transform.forward = -zAxisOverride;
        GameObject faultPoint = GameObject.Find("FaultCollection").transform.Find("Fault").Find("FaultPoint").gameObject;
        faultPoint.transform.forward = -zAxisOverride;
        GameObject faultText = GameObject.Find("FaultCollection").transform.Find("Fault").Find("TextObjects").gameObject;
        faultText.transform.forward = -zAxisOverride;

        //Debug.Log($"Plane transform.forward = {planeObject.transform.forward}, transform.up = {planeObject.transform.up}, rotation = {planeObject.transform.rotation}, up * rotation = {planeObject.transform.rotation * Vector3.up}");
        //Debug.Log($"Current zAxisOverride = {zAxisOverride}");
    }

    void Update()
    {
        zAxisOverride = planeObject.transform.up;

        GameObject faultBoundsDisplay = GameObject.Find("FaultCollection").transform.Find("Fault").Find("BoundsDisplay").gameObject;
        faultBoundsDisplay.transform.forward = -zAxisOverride;
        GameObject faultPoint = GameObject.Find("FaultCollection").transform.Find("Fault").Find("FaultPoint").gameObject;
        faultPoint.transform.forward = -zAxisOverride;
        GameObject faultText = GameObject.Find("FaultCollection").transform.Find("Fault").Find("TextObjects").gameObject;
        faultText.transform.forward = -zAxisOverride;
    }
}

