using CesiumForUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovementModeController : MonoBehaviour
{
    public ModifiedCameraController FreeMovementCameraController;

    public GameObject DynamicCamera;

    public GameObject FreeMovementUI;
    public GameObject ViewSelectUI;
    public GameObject EditingViewUI;

    public GameObject SaveNewViewButton;
    public GameObject MovementModeDropdown;

    public void MovementModeChanged(TMP_Dropdown mode)
    {
        switch (mode.value)
        {
            // WASD/panning camera
            case 0:
                EnableFreeMovement(true);
                FreeMovementUI.SetActive(true);
                ViewSelectUI.SetActive(false);
                break;
            // View select screen
            case 1:
                EnableFreeMovement(false);
                FreeMovementUI.SetActive(false);
                ViewSelectUI.SetActive(true);
                break;
        }
    }

    public void ToggleJoystickLook()
    {
        FreeMovementCameraController.JoystickLook = !FreeMovementCameraController.JoystickLook;
    }

    private void EnableFreeMovement(bool isEnabled)
    {
        FreeMovementCameraController.enabled = isEnabled;
    }

    public void AssignUIVariables(GameObject editableViewButton)
    {
        editableViewButton.GetComponent<FlyToButtonScript>().FreeMovementUI = FreeMovementUI;
        editableViewButton.GetComponent<FlyToButtonScript>().EditingViewUI = EditingViewUI;
        editableViewButton.GetComponent<FlyToButtonScript>().ViewSelectUI = ViewSelectUI;
        editableViewButton.GetComponent<FlyToButtonScript>().SaveNewViewButton = SaveNewViewButton;
        editableViewButton.GetComponent<FlyToButtonScript>().MovementModeDropdown = MovementModeDropdown;
    }
}