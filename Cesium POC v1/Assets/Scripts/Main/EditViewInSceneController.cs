using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CesiumForUnity;
using TMPro;
using System;
using Unity.Mathematics;

public class EditViewInSceneController : MonoBehaviour
{
    public GameObject ViewController;
    public CesiumGlobeAnchor Camera;
    public TMP_InputField ViewTitle;

    // View-specific - assigned by enabling FlyToButton component
    public View View;
    public GameObject FlyToButton;

    public void SaveChanges()
    {
#pragma warning disable
        ViewController.GetComponent<ViewController>().EditView(View, ViewTitle.text, Camera.longitudeLatitudeHeight, 
                                                            new Vector2((180 * math.PI) * Quaternion.ToEulerAngles(Camera.rotationEastUpNorth).x,
                                                                        (180 * math.PI) * Quaternion.ToEulerAngles(Camera.rotationEastUpNorth).y));
#pragma warning restore
        FlyToButton.GetComponent<FlyToButtonScript>().DisableEditInSceneUI();
    }
    public void Cancel()
    {
        FlyToButton.GetComponent<FlyToButtonScript>().DisableEditInSceneUI();
    }
}
