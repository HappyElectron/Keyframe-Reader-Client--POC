using CesiumForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class FineCameraControlsController : MonoBehaviour
{
    public ModifiedCameraController Camera;
    public CesiumGlobeAnchor GlobeAnchor;

    public GameObject MovementModeController;

    public TMP_InputField PositionInputX;
    public TMP_InputField PositionInputY;
    public TMP_InputField PositionInputZ;

    public TMP_InputField RotationInputX;
    public TMP_InputField RotationInputY;

    [SerializeField] bool editingFields;

    public void Update()
    {
        if (!editingFields && MovementModeController.GetComponent<MovementModeController>().FineCameraControls.activeInHierarchy)
            UpdateInputsFromCamera();
    }

    public void UpdateMovementSpeed(Slider slider)
    {
        Camera.MovementSpeedScaler = slider.value;
    }

    public void UpdateLookSpeed(Slider slider)
    {
        // LookSpeedScaler is used in a division calculation.
        // Value inversion here to preserve left-to-right low-to-high slider behaviour.
        Camera.LookSpeedScaler = (slider.maxValue - slider.value);
    }

    public void BeginEditingFields()
    {
        editingFields = true;
        Camera.enabled = false;
    }


    public void UpdateCameraFromInput()
    {
        // Set the camera position/rotation to values pulled from the input fields.
        // Input fields disallow non-decimal inputs.
        GlobeAnchor.longitudeLatitudeHeight = new double3(double.Parse(PositionInputX.text), double.Parse(PositionInputY.text), double.Parse(PositionInputZ.text));
        GlobeAnchor.rotationEastUpNorth = Quaternion.Euler(float.Parse(RotationInputX.text),
                                                           float.Parse(RotationInputY.text),
                                                           GlobeAnchor.gameObject.GetComponent<Transform>().rotation.z);
        editingFields = false;
        Camera.enabled = true;
    }

    public void UpdateInputsFromCamera()
    {
        PositionInputX.text = Convert.ToString(GlobeAnchor.longitudeLatitudeHeight.x);
        PositionInputY.text = Convert.ToString(GlobeAnchor.longitudeLatitudeHeight.y);
        PositionInputZ.text = Convert.ToString(GlobeAnchor.longitudeLatitudeHeight.z);

#pragma warning disable
        RotationInputX.text = Convert.ToString((180 / math.PI) * Quaternion.ToEulerAngles(GlobeAnchor.rotationEastUpNorth).x);
        RotationInputY.text = Convert.ToString((180 / math.PI) * Quaternion.ToEulerAngles(GlobeAnchor.rotationEastUpNorth).y);
#pragma warning restore
    }
}
