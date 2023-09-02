using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingInputHandler : MonoBehaviour, IMixedRealityPointerHandler
{
    //private LineManager lineManager;
    private FaultManager faultManager;
    private InteractionManager im;
    private Vector3 selectionPt, selectionNorm;
    private GameObject handMenu;

    void Awake() {
        handMenu = GameObject.Find("HandMenu/MenuContent");
    }

    void Start() {
        //lineManager = GameObject.Find("LineCollection").GetComponent<LineManager>();
        faultManager = GameObject.Find("FaultCollection").GetComponent<FaultManager>();
        selectionPt = selectionNorm = Vector3.zero;
        im = GetComponent<InteractionManager>();
        CoreServices.InputSystem.RegisterHandler<IMixedRealityPointerHandler>(this);
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData) {
        if (eventData.Handedness != Handedness.Right)
            return;

        var result = eventData.Pointer.Result;
        int interactingLayer = result.Details.Object.layer;
        if (interactingLayer == 31) {
            selectionPt = result.Details.Point + result.Details.Normal * 0.01f;
            selectionNorm = result.Details.Normal;
        }
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData) {
        if (selectionPt == Vector3.zero)
            return;

        var result = eventData.Pointer.Result;
        int interactingLayer = result.Details.Object.layer;
        if (interactingLayer == 31) {
            selectionPt = result.Details.Point + result.Details.Normal * 0.01f;
            selectionNorm = result.Details.Normal;
        } else {
            selectionPt = selectionNorm = Vector3.zero;
        }
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData) {
        if (selectionPt == Vector3.zero)
            return;

        //lineManager.AddLinePoint(selectionPt, selectionNorm);
        faultManager.AddFaultPoint(selectionPt, selectionNorm);
        selectionPt = Vector3.zero;
        selectionNorm = Vector3.zero;
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) {
        return;
    }
}
