using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//Script per indicare larghezza cricca
public class LAV_FaultManager : MonoBehaviour
{
    public GameObject defaultFault;
    private LAV_InteractionManager InteractionManager;
    private LAV_FaultMeasurement currentFault;
    private TextMeshPro fingerWidth;
    private Transform index, thumb;

    void Start() 
    {
      InteractionManager = GameObject.Find("FaultCollection").GetComponent<LAV_InteractionManager>();
      fingerWidth = GameObject.Find("FingerWidth").GetComponent<TextMeshPro>();
      defaultFault = transform.Find("Fault").gameObject;
      defaultFault.SetActive(false);
      currentFault = null;
    }
    void Update() 
    {
      var handJointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>();

      if (handJointService == null) //non fa niente se non si vede almeno un giunto
          return;

        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Far_Pointer_Right || InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Right)
        {
            index = handJointService.RequestJointTransform(TrackedHandJoint.IndexTip, Handedness.Left);
            thumb = handJointService.RequestJointTransform(TrackedHandJoint.ThumbTip, Handedness.Left);
            fingerWidth.transform.position = (index.position + thumb.position) / 2f; //solo per la scritta
        }
        if (InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Far_Pointer_Left || InteractionManager.InteractionTypeMethod == LAV_InteractionManager.InteractionType.Proximity_Pointer_Left)
        {
            index = handJointService.RequestJointTransform(TrackedHandJoint.IndexTip, Handedness.Right);
            thumb = handJointService.RequestJointTransform(TrackedHandJoint.ThumbTip, Handedness.Right);
            fingerWidth.transform.position = (index.position + thumb.position) / 2f - new Vector3(0.15f, 0, 0);
        }

      if (index.position == Vector3.zero || thumb.position == Vector3.zero)
          return;

      Transform widthDisplay = currentFault.selectedPoint.transform.Find("PointWidth");
      Vector3 indexThumbDiff = index.position - thumb.position;
      Vector3 dirVec = indexThumbDiff.normalized;
      widthDisplay.up = dirVec; //asse y del cilindro uguale al vettore 1 distanza dita 
      Vector3 widthScale = widthDisplay.localScale;
      widthScale.y = indexThumbDiff.magnitude / (currentFault.selectedPoint.transform.localScale.y * 2f);
      widthDisplay.localScale = widthScale;

      fingerWidth.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 2f); //orienta la scritta in base a dove guarda la camera, il moltiplicatore allontana l'oggetto che si osserva per una rotazione più lenta
      fingerWidth.SetText(indexThumbDiff.magnitude.ToString("F2") + "m"); //fino a seconda cifra decimale
    }
    public void CreateNewFault() 
    {
      currentFault = Instantiate(defaultFault, transform).GetComponent<LAV_FaultMeasurement>();
      currentFault.gameObject.SetActive(true);
    }
    public void AddFaultPoint(Vector3 pos, Quaternion rot, Vector3 norm) 
    {
      if (currentFault == null)
          CreateNewFault();

      Debug.Log(string.Format("Adding point {0} {1}", pos.ToString("F5"), norm.ToString("F5")));
        currentFault.AddFaultPoint(pos, rot, norm);
    }
}
