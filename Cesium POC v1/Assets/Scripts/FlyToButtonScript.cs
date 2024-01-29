using CesiumForUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class FlyToButtonScript : MonoBehaviour
{
    public GameObject Camera;
    public GameObject Child;
    public bool CanInterruptByMoving = true;

    public double3 position;
    public Vector2 yawAndPitch;

    public GameObject FreeMovementUI;
    public GameObject EditingViewUI;
    public GameObject ViewSelectUI;
    public GameObject SaveNewViewButton;
    public GameObject MovementModeDropdown;

    public GameObject ViewController;

    private void Awake()
    {
        Camera = GameObject.Find("DynamicCamera");
    }

    public void FlyToLocation()
    {
        Camera.GetComponent<CesiumFlyToController>().FlyToLocationLongitudeLatitudeHeight(position, yawAndPitch.y, yawAndPitch.x, CanInterruptByMoving);
    }
    public void EnableViewEditor()
    {
        FreeMovementUI.SetActive(false);
        ViewSelectUI.SetActive(false);
        SaveNewViewButton.SetActive(false);
        MovementModeDropdown.SetActive(false);

        // Fields set before activating gameobject; the script
        // calls 'Awake' to configure the UI text fields.
        EditingViewUI.GetComponent<EditViewController>().Title = Child.GetComponent<TMP_Text>().text;
        EditingViewUI.GetComponent<EditViewController>().Position = position;
        EditingViewUI.GetComponent<EditViewController>().Rotation = yawAndPitch;
        EditingViewUI.GetComponent<EditViewController>().FlyToButton = gameObject;
        EditingViewUI.SetActive(true);
    }

    public void CancelViewEditor()
    {
        FreeMovementUI.SetActive(false);
        ViewSelectUI.SetActive(true);
        SaveNewViewButton.SetActive(true);
        MovementModeDropdown.SetActive(true);
        EditingViewUI.SetActive(false);
    }

    public void DeleteView()
    {
        ViewController[] activeAndInactive = GameObject.FindObjectsOfType<ViewController>(true);
        activeAndInactive.First().DeleteView(gameObject.GetComponent<FlyToButtonScript>());
    }
}
