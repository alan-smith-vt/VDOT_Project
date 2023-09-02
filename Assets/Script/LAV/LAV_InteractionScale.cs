using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//Script per variazione scala punto
public class LAV_InteractionScale : MonoBehaviour, IMixedRealityPointerHandler
{
    private LAV_FaultMeasurement faultMeasurement;
    private LAV_InteractionManager InteractionManager;
    private LAV_MenuSystemHandler LAV_MenuSystemHandler;
    private LAV_InteractionConstrain LAV_InteractionConstrain;
    private AGS_Interaction_CV interaction_CV;
    private TextMeshPro FingerDistancerx, FingerDistancesx, SphereDiameter;
    private Transform indexrx, indexsx, thumbrx, thumbsx;
    public float constrain_noScaleChange, ScaleSpeed = 1;
    public float timeoutPeriod = 2f;
    public bool Snap;
    public float SnapGap;

    private float timer;
    private MoveAxisConstraint axisconstrain;

    private Vector3 updatedPosition;

    private LineRenderer[] lines;

    [HideInInspector] public float existingPoint_interactions = 0;

    private bool isScaling;
    private 
    void Start()
    {
        LAV_MenuSystemHandler = GameObject.Find("Menu_Crack").GetComponent<LAV_MenuSystemHandler>();
        LAV_InteractionConstrain = GameObject.Find("FaultCollection/Fault/FaultPoint").GetComponent<LAV_InteractionConstrain>();
        interaction_CV = GameObject.Find("FaultInteraction").GetComponent<AGS_Interaction_CV>();
        InteractionManager = GameObject.Find("FaultInteraction").GetComponent<LAV_InteractionManager>();
        FingerDistancerx = GameObject.Find("FaultCollection/FingerDistance").GetComponent<TextMeshPro>();
        FingerDistancesx = GameObject.Find("FaultCollection/FingerDistancesx").GetComponent<TextMeshPro>();
        FingerDistancerx.SetText("0.010");
        FingerDistancesx.SetText("0.010");   
    }
    private void OnEnable()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
    }
    private void OnDisable()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
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

        //GameObject[] faultPoints = GameObject.FindGameObjectsWithTag("FaultPoint");
        //Vector3[] faultPointPositions = new Vector3[faultPoints.Length];
        ////var filteredFaultPoints = new List<GameObject>();
        ////foreach (var fp in faultPoints)
        ////{
        ////    if (fp.name.StartsWith("FaultPoint_"))
        ////    {
        ////        filteredFaultPoints.Add(fp);
        ////    }
        ////}
        //if (isScaling == false)
        //{
        //    for (int i = 0; i < faultPoints.Length; i++)
        //    {
        //        faultPointPositions[i] = faultPoints[i].transform.position;
        //    }
        //}
        //else 
        //{
        //    for (int i = 0; i < faultPoints.Length; i++)
        //    {
        //        faultPoints[i].transform.position = faultPointPositions[i];
        //    }
        //}
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
        //CV_only and Human-then-CV can't scale the points
        if (InteractionManager.InteractionMethodChosen == LAV_InteractionManager.InteractionMethod.CV_Only)
        {
            return;
        }

        var result = eventData.Pointer.Result;
        GameObject interactingObject = result.Details.Object;
        Transform interactingObject_Start = interactingObject.transform;
        if (interactingObject.name.Contains("FaultPoint") && eventData.Handedness == Handedness.Right)
        {
            updatedPosition = interactingObject.transform.position;
            faultMeasurement = interactingObject_Start.parent.GetComponent<LAV_FaultMeasurement>(); //accede allo script del padre
            int idx = int.Parse(interactingObject.name.Split('_')[1]); //prende il secondo termine di un array dividendo il nome da un carattere e lo converte da string a int per richiamare uno specifico punto della lista

            axisconstrain = interactingObject.gameObject.GetComponent<MoveAxisConstraint>();

            if (InteractionManager.InteractionMethodChosen != LAV_InteractionManager.InteractionMethod.Human_then_CV)
            {
                Transform PointDisplay = interactingObject.transform;
                //float indexPointDist = Math.Abs(indexrx.position.magnitude - interactingObject_Start.position.magnitude) / 100;
                float indexPointDist = Math.Abs((indexrx.position - interactingObject_Start.position).magnitude) / 100;
                float newScale = (indexPointDist + 0.0015875f / ScaleSpeed - constrain_noScaleChange) * ScaleSpeed;
                float newScaleAprox = Mathf.Round(newScale * 10000.0f) * 0.0001f;   //arrotonda il numero alla 4a cifra decimale
                //Debug.Log($"Position hand: {indexrx.position}, Position point: {interactingObject_Start.position}, dif = {(indexrx.position - interactingObject_Start.position)}, " +
                //    $"dif mag = {Math.Abs((indexrx.position - interactingObject_Start.position).magnitude)}, indexpointdist = {indexPointDist}, newScale = {newScale}");
                //string newScaleString = newScale.ToString("F4");

                if (!Snap)
                {
                    if (indexPointDist > constrain_noScaleChange)
                    {
                        Vector3 PointScale = PointDisplay.localScale;
                        PointScale = new Vector3(newScale, newScale, newScale);
                        PointDisplay.localScale = PointScale;
                        timer = 0;

                        //interactingObject.transform.position = updatedPosition;

                        axisconstrain.enabled = true;
                        //axisconstrain.ConstraintOnMovement = true;
                        MoveAxisConstraint[] myScripts = interactingObject.GetComponents<MoveAxisConstraint>();

                        // Call the DoSomething() method on the second MyScript component
                        if (myScripts.Length >= 2)
                        {
                            MoveAxisConstraint secondScript = myScripts[1];
                            secondScript.enabled = true;
                        }
                    }
                    else
                    {
                        //faultPointPosition = interactingObject.transform.position;

                        timer += Time.deltaTime;
                        if (timer > LAV_InteractionConstrain.time_scale_constrain)
                        { axisconstrain.enabled = false; interactingObject.gameObject.GetComponent<Renderer>().material = LAV_InteractionConstrain.ChangeZColor; }
                        MoveAxisConstraint[] myScripts = interactingObject.GetComponents<MoveAxisConstraint>();

                        // Call the DoSomething() method on the second MyScript component
                        if (myScripts.Length >= 2)
                        {
                            MoveAxisConstraint secondScript = myScripts[1];
                            secondScript.enabled = false;
                        }
                        isScaling = false;
                    }
                    //else
                    //{ timer = 0; axisconstrain.enabled = true; }
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
                SphereDiameter.SetText((PointDisplay.transform.localScale.x * 39.3701f).ToString("F3") + "in");
                SphereDiameter.transform.localScale = new Vector3(100f,100f,100f) / (PointDisplay.localScale.x / 0.0015875f);
                //SphereDiameter.transform.localPosition = new Vector3(-4.6100049f, 3.51000047f, 16.651638f) / (PointDisplay.localScale.x / 0.0015875f);
                SphereDiameter.transform.localPosition = new Vector3(-30f, -3f, 16.651638f) / (PointDisplay.localScale.x / 0.0015875f);

                GameObject circleObject = interactingObject.transform.Find("Circle").gameObject;
                circleObject.transform.localScale = new Vector3(10, 10, 10) / (PointDisplay.localScale.x / 0.0015875f);

                if (PointDisplay.transform.localScale.x > LAV_InteractionConstrain.scale_max)
                {
                    FingerDistancerx.SetText("0.100" + "m");
                    SphereDiameter.SetText("0.100" + "m");
                }
                faultMeasurement.faultPoints[idx].diameter = PointDisplay.transform.localScale.x; //associa il diametro del punto preso a quello del punto corrente della lista
            }

            if (interactingObject.transform.position.x != faultMeasurement.faultPoints[idx].position.x)
             {
                faultMeasurement.faultPoints[idx].position = interactingObject.transform.position;
                faultMeasurement.CalculateMinimumBoundingBox();
             }
        }
        if (interactingObject.name.Contains("FaultPoint") && eventData.Handedness == Handedness.Left)
        {
            faultMeasurement = interactingObject_Start.parent.GetComponent<LAV_FaultMeasurement>(); //accede allo script del padre
            int idx = int.Parse(interactingObject.name.Split('_')[1]); //prende il secondo termine di un array dividendo il nome da un carattere e lo converte da string a int per richiamare uno specifico punto della lista
            
            axisconstrain = interactingObject.gameObject.GetComponent<MoveAxisConstraint>();

            if (InteractionManager.InteractionMethodChosen != LAV_InteractionManager.InteractionMethod.Human_then_CV)
            {
                Transform PointDisplay = interactingObject.transform;
                //float indexPointDist = Math.Abs(indexrx.position.magnitude - interactingObject_Start.position.magnitude) / 100;
                float indexPointDist = Math.Abs((indexsx.position - interactingObject_Start.position).magnitude) / 100;
                float newScale = (indexPointDist + 0.0015875f / ScaleSpeed - constrain_noScaleChange) * ScaleSpeed;
                float newScaleAprox = Mathf.Round(newScale * 10000.0f) * 0.0001f;   //arrotonda il numero alla 4a cifra decimale
                //Debug.Log($"Position hand: {indexrx.position}, Position point: {interactingObject_Start.position}, dif = {(indexrx.position - interactingObject_Start.position)}, " +
                //    $"dif mag = {Math.Abs((indexrx.position - interactingObject_Start.position).magnitude)}, indexpointdist = {indexPointDist}, newScale = {newScale}");
                //string newScaleString = newScale.ToString("F4");

                if (!Snap)
                {
                    if (indexPointDist > constrain_noScaleChange)
                    {
                        Vector3 PointScale = PointDisplay.localScale;
                        PointScale = new Vector3(newScale, newScale, newScale);
                        PointDisplay.localScale = PointScale;
                        timer = 0;

                        //interactingObject.transform.position = updatedPosition;

                        axisconstrain.enabled = true;
                        //axisconstrain.ConstraintOnMovement = true;
                        MoveAxisConstraint[] myScripts = interactingObject.GetComponents<MoveAxisConstraint>();

                        // Call the DoSomething() method on the second MyScript component
                        if (myScripts.Length >= 2)
                        {
                            MoveAxisConstraint secondScript = myScripts[1];
                            secondScript.enabled = true;
                        }
                    }
                    else
                    {
                        //faultPointPosition = interactingObject.transform.position;

                        timer += Time.deltaTime;
                        if (timer > LAV_InteractionConstrain.time_scale_constrain)
                        { axisconstrain.enabled = false; interactingObject.gameObject.GetComponent<Renderer>().material = LAV_InteractionConstrain.ChangeZColor; }
                        MoveAxisConstraint[] myScripts = interactingObject.GetComponents<MoveAxisConstraint>();

                        // Call the DoSomething() method on the second MyScript component
                        if (myScripts.Length >= 2)
                        {
                            MoveAxisConstraint secondScript = myScripts[1];
                            secondScript.enabled = false;
                        }
                        isScaling = false;
                    }
                    //else
                    //{ timer = 0; axisconstrain.enabled = true; }
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
                SphereDiameter.SetText((PointDisplay.transform.localScale.x * 39.3701f).ToString("F3") + "in");
                SphereDiameter.transform.localScale = new Vector3(100f, 100f, 100f) / (PointDisplay.localScale.x / 0.0015875f);
                //SphereDiameter.transform.localPosition = new Vector3(-4.6100049f, 3.51000047f, 16.651638f) / (PointDisplay.localScale.x / 0.0015875f);
                SphereDiameter.transform.localPosition = new Vector3(-30f, -3f, 16.651638f) / (PointDisplay.localScale.x / 0.0015875f);

                GameObject circleObject = interactingObject.transform.Find("Circle").gameObject;
                circleObject.transform.localScale = new Vector3(10, 10, 10) / (PointDisplay.localScale.x / 0.0015875f);

                if (PointDisplay.transform.localScale.x > LAV_InteractionConstrain.scale_max)
                {
                    FingerDistancerx.SetText("0.100" + "m");
                    SphereDiameter.SetText("0.100" + "m");
                }
                faultMeasurement.faultPoints[idx].diameter = PointDisplay.transform.localScale.x; //associa il diametro del punto preso a quello del punto corrente della lista
            }      

            if (interactingObject.transform.position.x != faultMeasurement.faultPoints[idx].position.x)
            {
                faultMeasurement.faultPoints[idx].position = interactingObject.transform.position;
                faultMeasurement.CalculateMinimumBoundingBox();
            }
        }
    }
    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        
        //CV_only can't drag anything
        if (InteractionManager.InteractionMethodChosen == LAV_InteractionManager.InteractionMethod.CV_Only)
        {
            return;
        }
        float thresh = 0.1f;
        var result = eventData.Pointer.Result;
        GameObject interactingObject = result.Details.Object;
        if (interactingObject.name.Contains("FaultPoint"))
        {

            //interactingObject.transform.position = updatedPosition;

            lines = GameObject.Find("Spline").GetComponentsInChildren<LineRenderer>();
            Transform interactingObject_Finish = interactingObject.transform;
            Vector3 closestSpline = interaction_CV.GetClosestPointOnLine(interactingObject_Finish.position, lines);
            float dist = Vector3.Distance(closestSpline, interactingObject_Finish.position);
            Debug.Log($"Released {interactingObject.name}. Distance to spline = {dist}");

            SphereDiameter = interactingObject.transform.Find("SphereDiameter").GetComponent<TextMeshPro>();
            SphereDiameter.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 2f);

            if (dist < thresh && InteractionManager.InteractionMethodChosen != LAV_InteractionManager.InteractionMethod.CV_then_Human)
            {
                interactingObject.transform.position = closestSpline;
                SphereDiameter.SetText("...");
                interaction_CV.updateThickness(interactingObject);
            }

            int idx = int.Parse(interactingObject.name.Split('_')[1]);
            faultMeasurement = interactingObject.transform.parent.GetComponent<LAV_FaultMeasurement>();
            if (interactingObject.transform.position.x != faultMeasurement.faultPoints[idx].position.x)
            {
                faultMeasurement.faultPoints[idx].position = interactingObject.transform.position;
                faultMeasurement.CalculateMinimumBoundingBox();
            }
        }

        MoveAxisConstraint[] myScripts = interactingObject.GetComponents<MoveAxisConstraint>();

        // Call the DoSomething() method on the second MyScript component
        if (myScripts.Length >= 2)
        {
            MoveAxisConstraint secondScript = myScripts[1];
            secondScript.enabled = false;
        }
    }
    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        var result = eventData.Pointer.Result;
        GameObject interactingObject = result.Details.Object;
        if (interactingObject.layer != 5)// && LAV_MenuSystemHandler.Manual_Test_Menu.activeSelf || LAV_MenuSystemHandler.CV_Test_Menu.activeSelf || LAV_MenuSystemHandler.CV_Test_Menu.activeSelf || LAV_MenuSystemHandler.CV_Test_Menu.activeSelf)
        {
            existingPoint_interactions++;
            Debug.Log("existingPoint_interactions: " + existingPoint_interactions);
        }
        if (interactingObject.name.Contains("FaultPoint"))
        {
            axisconstrain = interactingObject.gameObject.GetComponent<MoveAxisConstraint>();
            timer = 0; axisconstrain.enabled = true;
        }
    }
    public void Snap_ON()
    { Snap = true; }
    public void Snap_OFF()
    { Snap = false; }
}
