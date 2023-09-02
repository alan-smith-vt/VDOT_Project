using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script per creazione punto con raycasting
public class LAV_DrawingInputHandler : MonoBehaviour, IMixedRealityPointerHandler
{
    private LAV_InteractionManager InteractionManager;
    private LAV_FaultManager faultManager;
    private Vector3 selectionPt, selectionNorm;
    private Quaternion rotPt;

    void Start() 
    {
      InteractionManager = GameObject.Find("FaultCollection").GetComponent<LAV_InteractionManager>();
      faultManager = GameObject.Find("FaultCollection").GetComponent<LAV_FaultManager>();
      selectionPt = selectionNorm = Vector3.zero;
      rotPt = Quaternion.identity;
      CoreServices.InputSystem.RegisterHandler<IMixedRealityPointerHandler>(this);
    }
    public void OnPointerDown(MixedRealityPointerEventData eventData) 
    {
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Far_Pointer_Right)
        {
          if (eventData.Handedness != Handedness.Right)
          return;
        }
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Far_Pointer_Left)
        {
          if (eventData.Handedness != Handedness.Left)
          return;
        }
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Left || InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Right)
        {
            if (eventData.Handedness != Handedness.Left || eventData.Handedness != Handedness.Right)
                return;
        }
        Debug.Log("ok");
      var result = eventData.Pointer.Result;
      int interactingLayer = result.Details.Object.layer;
      if (interactingLayer == 31) 
         {
          selectionPt = result.Details.Point + result.Details.Normal * 0.01f;
          selectionNorm = result.Details.Normal;
         }
    }
    public void OnPointerDragged(MixedRealityPointerEventData eventData)  //gesto mantenuto
    {
      if (selectionPt == Vector3.zero)
          return;

      var result = eventData.Pointer.Result;
      int interactingLayer = result.Details.Object.layer;
      if (interactingLayer == 31) 
         {
          selectionPt = result.Details.Point + result.Details.Normal * 0.01f; //FocusDetails Struct - Point è il punto del raycast, normal la normale
          selectionNorm = result.Details.Normal;
         } 
      else 
         {
          selectionPt = selectionNorm = Vector3.zero;
         }
    }
    public void OnPointerUp(MixedRealityPointerEventData eventData) 
    {
      if (selectionPt == Vector3.zero)
          return;

      faultManager.AddFaultPoint(selectionPt, rotPt, selectionNorm);
      selectionPt = Vector3.zero;
      selectionNorm = Vector3.zero;
    }
    public void OnPointerClicked(MixedRealityPointerEventData eventData) 
    {
      return;
    }
}
