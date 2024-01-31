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

    public GameObject SmallViewGameObj;
    public GameObject EditableViewGameObj;
}

public class ViewController : MonoBehaviour
{
    public List<View> Views = new List<View>();

    public CesiumGlobeAnchor CameraGlobeAnchor;

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
    // Creates a 'View' object, which holds view metadata, and two
    // UI components based off said View.
    public void SaveView(TMP_InputField name)
    {
        // Deprecated quaternion conversion used here over modern function because Unity has it's own q class,
        // meaning the math.q conversion does not call correctly. Requires degrees/radian conversion.
        // Warning disabled to clear console clutter.
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

        flyToButton.GetComponent<FlyToButtonScript>().View = view;

        // Set UI fields
        flyToButton.GetComponent<FlyToButtonScript>().UpdateSmallUIFields();

        return flyToButton;
    }

    private GameObject CreateEditableViewButton(View view)
    {
        GameObject editableView = Instantiate(EditableViewPrefab, ViewEditScreenScrollContent.GetComponent<Transform>());

        editableView.GetComponent<FlyToButtonScript>().View = view;

        // Set UI fields
        editableView.GetComponent<FlyToButtonScript>().UpdateEditableUIFields();

        // Assign UI 'phase' fields, using MovementModeController's references.
        MovementModeController.GetComponent<MovementModeController>().AssignUIVariables(editableView);

        return editableView;
    }

    public void OpenViewSelect()
    {
        JoystickLookToggle.SetActive(ViewScroll.activeInHierarchy);
        ViewScroll.SetActive(!ViewScroll.activeInHierarchy);
    }

    public void EditView(View view, string title, double3 position, Vector2 rotation)
    {
        view.Position = position;
        view.Rotation = rotation;
        view.Name = title;

        view.SmallViewGameObj.GetComponent<FlyToButtonScript>().UpdateSmallUIFields();
        view.EditableViewGameObj.GetComponent<FlyToButtonScript>().UpdateEditableUIFields();
    }

    // Destroy UI objects, delete reference to view - GC will clean.
    public void DeleteView(View View)
    {
        Destroy(View.EditableViewGameObj);
        Destroy(View.SmallViewGameObj);
        Views.Remove(View);
    }
}

