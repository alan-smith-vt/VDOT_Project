using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using System;

//Script per variazione scala punto
public class LAV_InteractionScale_old : MonoBehaviour, IMixedRealityPointerHandler
{
    private LAV_FaultMeasurement faultMeasurement;
    private LAV_InteractionManager InteractionManager;
    private LAV_MenuSystemHandler LAV_MenuSystemHandler;
    private TextMeshPro FingerDistancerx, FingerDistancesx, SphereDiameter;
    private Transform indexrx, indexsx, thumbrx, thumbsx;
    public float constrain_noScaleChange, ScaleSpeed = 1;
    //public Material ScaleManipulatedColor;
    public float timeoutPeriod = 2f;
    public bool Snap;
    public float SnapGap;

    public GameObject[] mesh;

    [HideInInspector] public float existingPoint_interactions = 0;
    void Start()
    {
        LAV_MenuSystemHandler = GameObject.Find("Menu_Crack").GetComponent<LAV_MenuSystemHandler>();
        InteractionManager = GameObject.Find("FaultInteraction").GetComponent<LAV_InteractionManager>();
        FingerDistancerx = GameObject.Find("FaultCollection/FingerDistance").GetComponent<TextMeshPro>();
        FingerDistancesx = GameObject.Find("FaultCollection/FingerDistancesx").GetComponent<TextMeshPro>();
        FingerDistancerx.SetText("0.010");
        FingerDistancesx.SetText("0.010");
        CoreServices.InputSystem.RegisterHandler<IMixedRealityPointerHandler>(this);
    }
    void Update()
    {
        var handJointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>();

        if (handJointService == null) //non fa niente se non si vede almeno un giunto
            return;

        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Far_Pointer_Right || InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Right)
        {
            indexrx = handJointService.RequestJointTransform(TrackedHandJoint.IndexTip, Handedness.Right);
            thumbrx = handJointService.RequestJointTransform(TrackedHandJoint.ThumbTip, Handedness.Right);
            FingerDistancerx.transform.position = (indexrx.position + thumbrx.position) / 2f - new Vector3(0.15f, 0, 0); //solo per la scritta
        }
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Far_Pointer_Left || InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Left)
        {
            indexsx = handJointService.RequestJointTransform(TrackedHandJoint.IndexTip, Handedness.Left);
            thumbsx = handJointService.RequestJointTransform(TrackedHandJoint.ThumbTip, Handedness.Left);
            FingerDistancesx.transform.position = (indexsx.position + thumbsx.position) / 2f;
        }
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Both)
        {
            indexrx = handJointService.RequestJointTransform(TrackedHandJoint.IndexTip, Handedness.Right);
            thumbrx = handJointService.RequestJointTransform(TrackedHandJoint.ThumbTip, Handedness.Right); 
            indexsx = handJointService.RequestJointTransform(TrackedHandJoint.IndexTip, Handedness.Left);
            thumbsx = handJointService.RequestJointTransform(TrackedHandJoint.ThumbTip, Handedness.Left);
            FingerDistancerx.transform.position = (indexrx.position + thumbrx.position) / 2f - new Vector3(0.15f, 0, 0); //solo per la scritta
            FingerDistancesx.transform.position = (indexsx.position + thumbsx.position) / 2f;
        }
    }
        public void OnPointerDown(MixedRealityPointerEventData eventData)
    { }
    public void OnPointerDragged(MixedRealityPointerEventData eventData)  //gesto mantenuto
    {
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Far_Pointer_Right || InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Right)
        {
            if (eventData.Handedness != Handedness.Right)
                return;
        }
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Far_Pointer_Left || InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Left)
        {
            if (eventData.Handedness != Handedness.Left)
                return;
        }

        var result = eventData.Pointer.Result;
        GameObject interactingObject= result.Details.Object;
        Transform interactingObject_Start = interactingObject.transform;
        //axisconstrain = interactingObject.GetComponent<MoveAxisConstraint>();
        if (interactingObject.name.Contains("FaultPoint") && eventData.Handedness == Handedness.Right)
        {
            faultMeasurement = interactingObject_Start.parent.GetComponent<LAV_FaultMeasurement>(); //accede allo script del padre

            Transform PointDisplay = interactingObject.transform;
            float indexPointDist = Math.Abs(indexrx.position.magnitude - interactingObject_Start.position.magnitude) / 100;
            float newScale = (indexPointDist + 0.01f / ScaleSpeed - constrain_noScaleChange) * ScaleSpeed;
            float newScaleAprox = Mathf.Round(newScale * 10000.0f) * 0.0001f;   //arrotonda il numero alla 4a cifra decimale
            //string newScaleString = newScale.ToString("F4");

            if (!Snap)
            {
                if (indexPointDist > constrain_noScaleChange)
            {
                Vector3 PointScale = PointDisplay.localScale;
                PointScale = new Vector3(newScale, newScale, newScale);
                PointDisplay.localScale = PointScale;
                //interactingObject.GetComponent<Renderer>().material = ScaleManipulatedColor;
            }
            }
            else
            {
                //Debug.Log("newScaleAprox: " + newScaleAprox);
                //Debug.Log("newScale: " + newScale);
                //Debug.Log(newScaleString.Substring(newScaleString.Length - 1));  //prende l'ultimo carattere di una stringa
                if (indexPointDist > constrain_noScaleChange && Math.Abs(newScaleAprox % SnapGap) < 0.0005)  //% serve per dire che un numero + multiplo di un altro, non si mette 0 preciso perchè è float
                {
                    Vector3 PointScale = PointDisplay.localScale;
                    PointScale = new Vector3(newScale, newScale, newScale);
                    PointDisplay.localScale = PointScale;
                }
            }


            FingerDistancerx.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 2f); //orienta la scritta in base a dove guarda la camera, il moltiplicatore allontana l'oggetto che si osserva per una rotazione più lenta
            FingerDistancerx.SetText(PointDisplay.transform.localScale.x.ToString("F3") + "m"); //fino a terza cifra decimale

            SphereDiameter = interactingObject.transform.Find("SphereDiameter").GetComponent<TextMeshPro>();
            SphereDiameter.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 2f);
            SphereDiameter.SetText(PointDisplay.transform.localScale.x.ToString("F3") + "m");

            int idx = int.Parse( interactingObject.name.Split('_')[1] ); //prende il secondo termine di un array dividendo il nome da un carattere e lo converte da string a int per richiamare uno specifico punto della lista
            faultMeasurement.faultPoints[idx].diameter = PointDisplay.transform.localScale.x; //associa il diametro del punto preso a quello del punto corrente della lista

            if (interactingObject.transform.position.x != faultMeasurement.faultPoints[idx].position.x)
             {
                faultMeasurement.faultPoints[idx].position = interactingObject.transform.position;
                faultMeasurement.CalculateMinimumBoundingBox();
              }
        }
        if (interactingObject.name.Contains("FaultPoint") && eventData.Handedness == Handedness.Left)
        {
            faultMeasurement = interactingObject_Start.parent.GetComponent<LAV_FaultMeasurement>(); //accede allo script del padre

            Transform PointDisplay = interactingObject.transform;
            float indexPointDist = Math.Abs(indexsx.position.magnitude - interactingObject_Start.position.magnitude) / 100;

            if (indexPointDist > constrain_noScaleChange)
            {
                Vector3 PointScale = PointDisplay.localScale;
                PointScale = new Vector3((indexPointDist + 0.01f / ScaleSpeed - constrain_noScaleChange) * ScaleSpeed, (indexPointDist + 0.01f / ScaleSpeed - constrain_noScaleChange) * ScaleSpeed, (indexPointDist + 0.01f / ScaleSpeed - constrain_noScaleChange) * ScaleSpeed);
                PointDisplay.localScale = PointScale;
            }

            FingerDistancesx.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 2f); //orienta la scritta in base a dove guarda la camera, il moltiplicatore allontana l'oggetto che si osserva per una rotazione più lenta
            FingerDistancesx.SetText(PointDisplay.transform.localScale.x.ToString("F3") + "m"); //fino a terza cifra decimale

            SphereDiameter = interactingObject.transform.Find("SphereDiameter").GetComponent<TextMeshPro>();
            SphereDiameter.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 2f);
            SphereDiameter.SetText(PointDisplay.transform.localScale.x.ToString("F3") + "m");

            int idx = int.Parse(interactingObject.name.Split('_')[1]); //prende il secondo termine di un array dividendo il nome da un carattere e lo converte da string a int per richiamare uno specifico punto della lista
            faultMeasurement.faultPoints[idx].diameter = PointDisplay.transform.localScale.x; //associa il diametro del punto preso a quello del punto corrente della lista

            if (interactingObject.transform.position.x != faultMeasurement.faultPoints[idx].position.x)
            {
                faultMeasurement.faultPoints[idx].position = interactingObject.transform.position;
                faultMeasurement.CalculateMinimumBoundingBox();
            }
        }
        for (int i = 0; i <= mesh.Length; i++)
        {
            mesh[i].SetActive(false); 
        }
    }
    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {  return; }
    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    { 
      var result = eventData.Pointer.Result;
      GameObject interactingObject= result.Details.Object;
      if (interactingObject.layer != 5 && LAV_MenuSystemHandler.Manual_Test_Menu.activeSelf || LAV_MenuSystemHandler.CV_Test_Menu.activeSelf)
      {
        existingPoint_interactions++;
        Debug.Log("existingPoint_interactions: " + existingPoint_interactions);
      }

      StartCoroutine(Wait()); 
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(timeoutPeriod);
        for (int i=0; i<= mesh.Length; i++)
        {
            mesh[i].SetActive(true);
        }
    }
    public void Snap_ON()
    {
        Snap = true;
    }
    public void Snap_OFF()
    {
        Snap = false;
    }
}
