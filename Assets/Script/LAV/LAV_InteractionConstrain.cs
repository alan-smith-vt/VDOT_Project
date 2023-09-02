using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

//Script per vincoli spostamento e scala
public class LAV_InteractionConstrain : MonoBehaviour
{
    private Vector3 objectScale;
    public float scale_min, scale_max, time_scale_constrain = 4;
    public Material NormalColor, ManipulatedColor, ChangeZColor;
    public GameObject ManipulationControl;
    //private float timer;

    //private MoveAxisConstraint axisconstrain;

    void Start()
    {
        objectScale = this.gameObject.transform.localScale;
        //axisconstrain = this.gameObject.GetComponent<MoveAxisConstraint>();
    }
    void Update()
    {
        objectScale = this.gameObject.transform.localScale;
        if (objectScale.x > scale_max)
        { this.gameObject.transform.localScale = new Vector3(scale_max, scale_max, scale_max); }

        if (objectScale.x < scale_min)
        { this.gameObject.transform.localScale = new Vector3(scale_min, scale_min, scale_min); }


        //if (ManipulationControl.activeSelf)
        //{
        //    timer += Time.deltaTime;
        //    if (timer > time_scale_constrain)
        //    {
        //        axisconstrain.enabled = false;
        //        this.gameObject.GetComponent<Renderer>().material = ChangeZColor;
        //    }
        //}
        //else
        //{
        //    timer = 0f;
        //    axisconstrain.enabled = true;
        //    this.gameObject.GetComponent<Renderer>().material = NormalColor;
        //}
    }
}
