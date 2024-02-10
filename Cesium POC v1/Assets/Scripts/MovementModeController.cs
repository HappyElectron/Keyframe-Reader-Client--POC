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

    public GameObject FineCameraControls;
    public GameObject LookSpeedControl;
    public GameObject EditInSceneUI;
    public GameObject EditInSceneViewTitle;

    public GameObject RecentViews;
    public GameObject RecentViewsScroll;
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
        LookSpeedControl.SetActive(DynamicCamera.GetComponent<ModifiedCameraController>().JoystickLook);
    }

    public void ToggleFineCameraControls()
    {
        FineCameraControls.SetActive(!FineCameraControls.activeInHierarchy);
        LookSpeedControl.SetActive(DynamicCamera.GetComponent<ModifiedCameraController>().JoystickLook);
    }
    
    public void UpdateMovementSpeed(Slider slider)
    {
        DynamicCamera.GetComponent<ModifiedCameraController>().MovementSpeedScaler = slider.value;
    }

    public void UpdateLookSpeed(Slider slider)
    {
        DynamicCamera.GetComponent<ModifiedCameraController>().LookSpeedScaler = slider.value;
    }

    private void EnableFreeMovement(bool isEnabled)
    {
        FreeMovementCameraController.enabled = isEnabled;
    }

    /// <summary>
    /// Assigns variables of the button using references from this game object.
    /// Needed because the button is instantiated from a prefab, without these references.
    /// </summary>
    /// <param name="editableViewButton"></param>
    public void AssignUIVariables(GameObject editableViewButton)
    {
        editableViewButton.GetComponent<FlyToButtonScript>().FreeMovementUI = FreeMovementUI;
        editableViewButton.GetComponent<FlyToButtonScript>().EditingViewUI = EditingViewUI;
        editableViewButton.GetComponent<FlyToButtonScript>().ViewSelectUI = ViewSelectUI;
        editableViewButton.GetComponent<FlyToButtonScript>().SaveNewViewButton = SaveNewViewButton;
        editableViewButton.GetComponent<FlyToButtonScript>().MovementModeDropdown = MovementModeDropdown;
        editableViewButton.GetComponent<FlyToButtonScript>().EditInSceneUI = EditInSceneUI;
        editableViewButton.GetComponent<FlyToButtonScript>().RecentViews = RecentViews;
        editableViewButton.GetComponent<FlyToButtonScript>().RecentViewsScroll = RecentViewsScroll;
        editableViewButton.GetComponent<FlyToButtonScript>().EditInSceneViewTitle = EditInSceneViewTitle;
    }
}