using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FaultManager : MonoBehaviour
{
    public GameObject defaultFault;
    [HideInInspector] public FaultMeasurement currentFault;
    private TextMeshPro fingerWidth;

    private void Start() {
        fingerWidth = GameObject.Find("FingerWidth").GetComponent<TextMeshPro>();
        defaultFault = transform.Find("Fault").gameObject;
        defaultFault.SetActive(false);
        currentFault = null;
    }

    void Update() {
        var handJointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>();

        if (handJointService == null)
            return;
        
        Transform index = handJointService.RequestJointTransform(TrackedHandJoint.IndexTip, Handedness.Left);
        Transform thumb = handJointService.RequestJointTransform(TrackedHandJoint.ThumbTip, Handedness.Left);

        if (index.position == Vector3.zero || thumb.position == Vector3.zero)
            return;

        Debug.Log("Updating point width");
        Transform widthDisplay = currentFault.selectedPoint.transform.Find("PointWidth");
        Vector3 indexThumbDiff = index.position - thumb.position;
        Vector3 dirVec = indexThumbDiff.normalized;
        widthDisplay.up = dirVec;
        Vector3 widthScale = widthDisplay.localScale;
        widthScale.y = indexThumbDiff.magnitude / (currentFault.selectedPoint.transform.localScale.y * 2f);
        widthDisplay.localScale = widthScale;

        fingerWidth.transform.position = (index.position + thumb.position) / 2f;
        fingerWidth.transform.right = dirVec;
        fingerWidth.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 2f);
        fingerWidth.SetText(indexThumbDiff.magnitude.ToString("F2") + "m");
    }

    public void CreateNewFault() {
        currentFault = Instantiate(defaultFault, transform).GetComponent<FaultMeasurement>();
        currentFault.gameObject.SetActive(true);
    }

    public void AddFaultPoint(Vector3 pos, Vector3 norm) {
        if (currentFault == null)
            CreateNewFault();

        Debug.Log(string.Format("Adding point {0} {1}", pos.ToString("F5"), norm.ToString("F5")));
        currentFault.AddFaultPoint(pos, norm);
    }

    /*
    public void OnHandJointsUpdated(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData) {
        Debug.Log("Hand joints update");

        if (currentFault == null)
            return;
        if (currentFault.selectedPoint == null)
            return;

        if (eventData.Handedness == Handedness.Left &&
            eventData.InputData.TryGetValue(TrackedHandJoint.IndexTip, out MixedRealityPose indexPose) &&
            eventData.InputData.TryGetValue(TrackedHandJoint.ThumbTip, out MixedRealityPose thumbPose)) {

            Debug.Log("Updating point width");
            Transform widthDisplay = currentFault.selectedPoint.transform.Find("PointWidth");
            Vector3 indexThumbDiff = indexPose.Position - thumbPose.Position;
            Vector3 dirVec = indexThumbDiff.normalized;
            widthDisplay.up = dirVec;
            Vector3 widthScale = widthDisplay.localScale;
            widthScale.y = indexThumbDiff.magnitude / currentFault.selectedPoint.transform.localScale.y;
            widthDisplay.localScale = widthScale;
        }
    }
    */
}
