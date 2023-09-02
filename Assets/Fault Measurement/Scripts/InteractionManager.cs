using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionManager : MonoBehaviour//, IMixedRealityPointerHandler
{
    public enum WidthSelectionType
    {
        Slider = 0,
        FingerRuler
    }

    public enum LengthSelectionType
    {
        Poke = 0,
        Trace
    }

    public WidthSelectionType widthSelectionMethod = WidthSelectionType.FingerRuler;
    public LengthSelectionType lengthSelectionMethod = LengthSelectionType.Poke;
    private TextMeshPro sliderVal;
    private GameObject widthSlider;

    private LineManager lineManager;

    void Awake()
    {
        //widthSlider = GameObject.Find("HandMenu/MenuContent/BackPlateSlider");
        //sliderVal = GameObject.Find("HandMenu/MenuContent/BackPlateSlider/PinchSlider/ThumbRoot/SliderValue")
        //    .GetComponent<TextMeshPro>();

        //AddHandMenuListeners();

        //CoreServices.InputSystem.RegisterHandler<IMixedRealityPointerHandler>(this);
    }

    void Update()
    {
        
    }

    private void AddHandMenuListeners() {
        Transform handMenuButtons = GameObject.Find("HandMenu/MenuContent/ButtonCollection").transform;
        /*
        handMenuButtons.Find("LengthPokeButton").GetComponent<Interactable>().OnClick.AddListener(
            delegate { lengthSelectionMethod = LengthSelectionType.Poke; });
        handMenuButtons.Find("LengthTraceButton").GetComponent<Interactable>().OnClick.AddListener(
            delegate { lengthSelectionMethod = LengthSelectionType.Trace; });
        */
        handMenuButtons.Find("NewLineButton").GetComponent<Interactable>().OnClick.AddListener(
            delegate { lineManager.CreateNewLine(false); });
        handMenuButtons.Find("ResetLineButton").GetComponent<Interactable>().OnClick.AddListener(
            delegate { lineManager.CreateNewLine(true); });
        handMenuButtons.Find("WidthSliderButton").GetComponent<Interactable>().OnClick.AddListener(
            delegate { widthSelectionMethod = WidthSelectionType.Slider;
                widthSlider.SetActive(true);
            });
        handMenuButtons.Find("WidthRulerButton").GetComponent<Interactable>().OnClick.AddListener(
            delegate { widthSelectionMethod = WidthSelectionType.FingerRuler;
                widthSlider.SetActive(false);
            });

        //GameObject.Find("HandMenu/MenuContent/BackPlateSlider/PinchSlider")
        //    .GetComponent<PinchSlider>().OnValueUpdated.AddListener(SliderValueUpdated);
    }

    /*
    private void SliderValueUpdated(SliderEventData eventData) {
        UpdateFaultWidth(15f * eventData.NewValue);
        //sliderVal.SetText((faultWidth * 100).ToString("F1") + "cm");
    }
    */
}
