using CesiumForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class View
{
    public string Name;
    public double3 Position;
    public Vector2 Rotation;

    public GameObject? SmallViewGameObj;
    public GameObject? EditableViewGameObj;
}

public class ViewController : MonoBehaviour
{
    // View details, current and future.
    public List<GameObject> ViewObjects = new List<GameObject>();
    public List<GameObject> EditableViews = new List<GameObject>();
    public CesiumGlobeAnchor CameraGlobeAnchor;

    public List<View> Views = new List<View>();

    public GameObject ViewNameInputContainer;
    public GameObject ViewNameInput;

    public GameObject ViewScroll;
    public GameObject ViewScrollContent;
    public GameObject JoystickLookToggle;

    public GameObject DynamicCamera;

    // Reference to ui element prefab.
    public GameObject FlyToPrefab;

    public TMP_Dropdown MovementModeSelect;

    public GameObject ViewEditScreen;
    public GameObject ViewEditScreenScrollContent;
    public GameObject EditableViewPrefab;

    public GameObject MovementModeController;
    // Bring up confirmation dialogue.
    public void CreateNewView()
    {
        DynamicCamera.GetComponent<ModifiedCameraController>().enabled = false;
        ViewNameInputContainer.SetActive(true);
    }

    // Method for accepting confirmation dialogue, via button press.
    public void SaveView(TMP_InputField name)
    {
#pragma warning disable

        View view = new View()
        {
            Name = name.text,
            Position = CameraGlobeAnchor.longitudeLatitudeHeight,
            Rotation = new Vector2((180 / math.PI) * Quaternion.ToEulerAngles(CameraGlobeAnchor.rotationEastUpNorth).x,
                                                        (180 / math.PI) * Quaternion.ToEulerAngles(CameraGlobeAnchor.rotationEastUpNorth).y)
        };
#pragma warning restore
        Views.Add(view);

        view.SmallViewGameObj = CreateSmallViewButton(view);
        view.EditableViewGameObj = CreateEditableViewButton(view);

        ViewNameInputContainer.SetActive(false);

        if(MovementModeSelect.value == 0)
            DynamicCamera.GetComponent<ModifiedCameraController>().enabled = true;
    }

    private GameObject CreateSmallViewButton(View view)
    {
        GameObject flyToButton = Instantiate(FlyToPrefab, ViewScrollContent.GetComponent<Transform>());
        flyToButton.GetComponent<FlyToButtonScript>().position = CameraGlobeAnchor.longitudeLatitudeHeight;

        // Deprecated quaternion conversion used here over modern function because Unity has it's own q class,
        // so the math.q conversion does not call correctly. Requires degrees/radian conversion.
        // Warning disabled to clear console clutter.
#pragma warning disable
        flyToButton.GetComponent<FlyToButtonScript>().yawAndPitch = new Vector2((180 / math.PI) * Quaternion.ToEulerAngles(CameraGlobeAnchor.rotationEastUpNorth).x,
                                                                                (180 / math.PI) * Quaternion.ToEulerAngles(CameraGlobeAnchor.rotationEastUpNorth).y);
#pragma warning restore
        flyToButton.GetComponentInChildren<TMP_Text>().text = ViewNameInput.GetComponent<TMP_InputField>().text;
        ViewObjects.Add(flyToButton);

        return flyToButton;
    }

    private GameObject CreateEditableViewButton(View view)
    {
        // See comment under CreateSmallViewButton.
#pragma warning disable
        float rotationX = (180 / math.PI) * Quaternion.ToEulerAngles(CameraGlobeAnchor.rotationEastUpNorth).x;
        float rotationY = (180 / math.PI) * Quaternion.ToEulerAngles(CameraGlobeAnchor.rotationEastUpNorth).y;
#pragma warning restore

        GameObject editableView = Instantiate(EditableViewPrefab, ViewEditScreenScrollContent.GetComponent<Transform>());

        editableView.GetComponent<FlyToButtonScript>().position = CameraGlobeAnchor.longitudeLatitudeHeight;
        editableView.GetComponent<FlyToButtonScript>().yawAndPitch = new Vector2(rotationX, rotationY);

        // Set UI fields
        editableView.transform.GetChild(0).GetComponent<TMP_Text>().text = ViewNameInput.GetComponent<TMP_InputField>().text;
        editableView.transform.GetChild(1).GetComponent<TMP_Text>().text = "Position: " + Math.Round(CameraGlobeAnchor.longitudeLatitudeHeight.x, 3) + ", " + Math.Round(CameraGlobeAnchor.longitudeLatitudeHeight.y, 3)+ ", " + Math.Round(CameraGlobeAnchor.longitudeLatitudeHeight.z, 3);
        editableView.transform.GetChild(2).GetComponent<TMP_Text>().text = "Rotation: " + Math.Round(rotationX, 3) + " " + Math.Round(rotationY, 3);
        
        MovementModeController.GetComponent<MovementModeController>().AssignUIVariables(editableView);

        EditableViews.Add(editableView);
    }

    public void OpenViewSelect()
    {
        JoystickLookToggle.SetActive(ViewScroll.activeInHierarchy);
        ViewScroll.SetActive(!ViewScroll.activeInHierarchy);
    }

    public void EditView(string tBackup, double3 pBackup, Vector2 rBackup, string title, double3 position, Vector2 rotation)
    {
        GameObject smallView = ViewObjects.Where(t => t.GetComponentInChildren<TMP_Text>().text == tBackup
                                            && t.GetComponent<FlyToButtonScript>().position.x == pBackup.x
                                            && t.GetComponent<FlyToButtonScript>().position.y == pBackup.y
                                            && t.GetComponent<FlyToButtonScript>().position.z == pBackup.z
                                            && t.GetComponent<FlyToButtonScript>().yawAndPitch.x == rBackup.x
                                            && t.GetComponent<FlyToButtonScript>().yawAndPitch.y == rBackup.y).First();
        ViewObjects.Remove(smallView);
        smallView.GetComponent<FlyToButtonScript>().position = position;
        smallView.GetComponent<FlyToButtonScript>().yawAndPitch = rotation;
        smallView.GetComponentInChildren<TMP_Text>().text = title;
        ViewObjects.Add(smallView);

        GameObject editableView = EditableViews.Where(t => t.GetComponentInChildren<TMP_Text>().text == tBackup
                                            && t.GetComponent<FlyToButtonScript>().position.x == pBackup.x
                                            && t.GetComponent<FlyToButtonScript>().position.y == pBackup.y
                                            && t.GetComponent<FlyToButtonScript>().position.z == pBackup.z
                                            && t.GetComponent<FlyToButtonScript>().yawAndPitch.x == rBackup.x
                                            && t.GetComponent<FlyToButtonScript>().yawAndPitch.y == rBackup.y).First();
        EditableViews.Remove(editableView);
        editableView.GetComponent<FlyToButtonScript>().position = position;
        editableView.GetComponent<FlyToButtonScript>().yawAndPitch = rotation;
        editableView.transform.GetChild(0).GetComponent<TMP_Text>().text = title;
        editableView.transform.GetChild(1).GetComponent<TMP_Text>().text = "Position: " + Math.Round(position.x, 3) + ", " + Math.Round(position.y, 3) + ", " + Math.Round(position.z, 3);
        editableView.transform.GetChild(2).GetComponent<TMP_Text>().text = "Rotation: " + Math.Round(rotation.y, 3) + " " + Math.Round(rotation.y, 3);
        EditableViews.Add(editableView);
    }

    public void DeleteView(FlyToButtonScript View)
    {
        GameObject smallView = ViewObjects.Where(t => t.GetComponentInChildren<TMP_Text>().text == View.Child.GetComponent<TMP_Text>().text
                                    && t.GetComponent<FlyToButtonScript>().position.x == View.position.x
                                    && t.GetComponent<FlyToButtonScript>().position.y == View.position.y
                                    && t.GetComponent<FlyToButtonScript>().position.z == View.position.z
                                    && t.GetComponent<FlyToButtonScript>().yawAndPitch.x == View.yawAndPitch.x
                                    && t.GetComponent<FlyToButtonScript>().yawAndPitch.y == View.yawAndPitch.y).First();
        ViewObjects.Remove(smallView);
        Destroy(smallView);
        EditableViews.Remove(View.gameObject);
        Destroy(View.gameObject);
    }
}

