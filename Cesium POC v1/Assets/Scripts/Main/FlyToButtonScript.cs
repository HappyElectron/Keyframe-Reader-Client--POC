using CesiumForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class FlyToButtonScript : MonoBehaviour
{
    public View View;

    public GameObject Camera;
    public GameObject Child;
    public bool CanInterruptByMoving = true;

    public GameObject FreeMovementUI;
    public GameObject EditingViewUI;
    public GameObject ViewSelectUI;

    public GameObject SaveNewViewButton;
    public GameObject MovementModeDropdown;

    public GameObject ViewController;

    public GameObject RecentViews;
    public GameObject RecentViewsScroll;

    public GameObject EditInSceneUI;
    public GameObject EditInSceneViewTitle;

    private int FreeMovementPhaseID = 0;

    private void Awake()
    {
        Camera = GameObject.Find("DynamicCamera");
    }

    public void FlyToLocation()
    {
        Camera.GetComponent<ModifiedFlyToController>().FlyToLocationLongitudeLatitudeHeight(View.ViewData.Position, View.ViewData.Rotation.y, View.ViewData.Rotation.x, CanInterruptByMoving);
    }

    public void EnableViewEditor()
    {
        FreeMovementUI.SetActive(false);
        ViewSelectUI.SetActive(false);
        SaveNewViewButton.SetActive(false);
        MovementModeDropdown.SetActive(false);

        // Prepare the editing screen
        EditingViewUI.GetComponent<EditViewController>().View = View;
        EditingViewUI.GetComponent<EditViewController>().FlyToButton = gameObject;
        EditingViewUI.GetComponent<EditViewController>().UpdateUIFields();
        EditingViewUI.SetActive(true);
    }

    public void DisableViewEditorUI()
    {
        FreeMovementUI.SetActive(false);
        ViewSelectUI.SetActive(true);
        SaveNewViewButton.SetActive(true);
        MovementModeDropdown.SetActive(true);
        EditingViewUI.SetActive(false);
    }

    public void DeleteView()
    {
        // I'm reasonably sure that this is not best practice.
        // However, Find() doesn't work, while this does, so I'll look into it later.
        ViewController[] activeAndInactive = GameObject.FindObjectsOfType<ViewController>(true);
        activeAndInactive.First().DeleteView(View);
    }


    // For updating the UI fields of the 'small' view, accessible from the free movement UI phase.
    public void UpdateSmallUIFields()
    {
        gameObject.GetComponentInChildren<TMP_Text>().text = View.ViewData.Name;
    }

    // For updating the UI fields of the 'large' editable view, accessible from the view selecting UI phase.
    public void UpdateEditableUIFields()
    {
        gameObject.transform.GetChild(0).GetComponent<TMP_Text>().text = View.ViewData.Name;
        gameObject.transform.GetChild(1).GetComponent<TMP_Text>().text = "Position: " + Math.Round(View.ViewData.Position.x, 3) + ", " + Math.Round(View.ViewData.Position.y, 3) + ", " + Math.Round(View.ViewData.Position.z, 3);
        gameObject.transform.GetChild(2).GetComponent<TMP_Text>().text = "Rotation: " + Math.Round(View.ViewData.Rotation.x, 3) + " " + Math.Round(View.ViewData.Rotation.y, 3);
    }


    // Enable/Disable UI components to edit a view with the free camera controls.
    public void OpenEditInSceneUI()
    {
        EditingViewUI.SetActive(false);
        FreeMovementUI.SetActive(true);
        RecentViews.SetActive(false);
        RecentViewsScroll.SetActive(false);
        EditInSceneUI.SetActive(true);

        Camera.GetComponent<ModifiedCameraController>().enabled = true;


        EditInSceneUI.GetComponent<EditViewInSceneController>().View = View;
        EditInSceneUI.GetComponent<EditViewInSceneController>().FlyToButton = this.gameObject;
        EditInSceneViewTitle.GetComponent<TMP_InputField>().text = View.ViewData.Name;
    }

    public void DisableEditInSceneUI()
    {
        EditingViewUI.SetActive(false);
        RecentViews.SetActive(true);
        EditInSceneUI.SetActive(false);
        SaveNewViewButton.SetActive(true);
        MovementModeDropdown.SetActive(true);

        MovementModeDropdown.GetComponent<TMP_Dropdown>().value = FreeMovementPhaseID;
    }
}