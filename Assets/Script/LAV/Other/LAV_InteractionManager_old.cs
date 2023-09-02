using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LAV_InteractionManager_old : MonoBehaviour
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

    public InteractionType InteractionTypeMethod = InteractionType.Proximity_Pointer_Right;

    void Start()
    {
    }

    void Update()
    {
    }
}

