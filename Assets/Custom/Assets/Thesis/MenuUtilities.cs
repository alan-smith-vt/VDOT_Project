using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.XR.WSA;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;

public class MenuUtilities : MonoBehaviour
{

    //public PredictiveMeshHandler_V7 predictiveMeshHandlerReference;
    //public GameObject debugConsole;

    //Start is called before the first frame update

    //private void Awake()
    //{
    //    predictiveMeshHandlerReference = meshManipulatorReference.GetComponent<PredictiveMeshHandler_V7>();
    //}

    //void Start()
    //{

    //}

    //Update is called once per frame
    //void Update()
    //{

    //}

    //public void ToggleSpatialMesh(bool state)
    //{
    //    var spatialAwarenessService = CoreServices.SpatialAwarenessSystem;
    //    var dataProviderAccess = spatialAwarenessService as IMixedRealityDataProviderAccess;
    //    var meshObserver = dataProviderAccess.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
    //    var meshObserverName = "OpenXR Spatial Mesh Observer";
    //    var observer = dataProviderAccess.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>(meshObserverName);
    //    if (observer == null)
    //    {
    //        Debug.Log("The observer is null");
    //    }
    //    else
    //    {
    //        Debug.Log("The observer was found");
    //        Debug.Log("Observer Name: " + observer.Name);
    //        Debug.Log("Observer Display Option: " + observer.DisplayOption);
    //        Debug.Log("Observer Source Name: " + observer.SourceName);
    //        Debug.Log("Observer Source ID: " + observer.SourceId);
    //        if (state)
    //        {
    //            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;
    //        }
    //        else if (state == false)
    //        {
    //            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
    //        }
    //    }
    //}

    //public void ToggleSpatialMappingActive(bool state)
    //{
    //    var spatialAwarenessService = CoreServices.SpatialAwarenessSystem;
    //    var dataProviderAccess = spatialAwarenessService as IMixedRealityDataProviderAccess;
    //    var meshObserver = dataProviderAccess.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
    //    var meshObserverName = "OpenXR Spatial Mesh Observer";
    //    var observer = dataProviderAccess.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>(meshObserverName);

    //    if (state)
    //    {
    //        observer.Resume();
    //    }
    //    else
    //    {
    //        observer.Suspend();
    //    }
    //}

    //public void ToggleHandPlane(bool state)
    //{
    //    Renderer[] rs = predictiveMeshHandlerReference.raycastPlane.GetComponentsInChildren<MeshRenderer>();
    //    foreach (MeshRenderer mr in rs)
    //    {
    //        mr.enabled = state;
    //    }
    //    predictiveMeshHandlerReference.raycastPlane.GetComponent<MeshRenderer>().enabled = state;
    //}

    //public void ToggleRaycastLines(bool state)
    //{
    //    predictiveMeshHandlerReference.IsLineRendereringEnabled = state;
    //}

    //public void ToggleDebugConsole(bool state)
    //{
    //    debugConsole.SetActive(state);
    //}
}
