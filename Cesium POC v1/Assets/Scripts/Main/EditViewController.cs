using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
using TMPro;
using UnityEngine.Rendering;


public class EditViewController : MonoBehaviour
{
    public GameObject ViewController;

    // View is the source of truth in the application, Title, Position and Rotation
    // are values that store the user's edits before they're committed.
    public View View;
    public string Title;
    public double3 Position;
    public Vector2 Rotation;


    public GameObject LatitudeEditor;
    public GameObject LongitudeEditor;
    public GameObject HeightEditor;
    public GameObject RotationXEditor;
    public GameObject RotationYEditor;
    public GameObject TitleEditor;

    public GameObject InvalidInputScreen;

    // The button that enabled the editing screen; to 'cancel' and edit.
    public GameObject FlyToButton;

    // Sync displayed/editing values with the currently stored view
    public void UpdateUIFields()
    {
        Position = View.Position;
        Rotation = View.Rotation;
        Title = View.Name;
        LatitudeEditor.GetComponent<TMP_InputField>().text = $"{Position.x}";
        LongitudeEditor.GetComponent<TMP_InputField>().text = $"{Position.y}";
        HeightEditor.GetComponent<TMP_InputField>().text = $"{Position.z}";
        RotationXEditor.GetComponent<TMP_InputField>().text = $"{Rotation.x}";
        RotationYEditor.GetComponent<TMP_InputField>().text = $"{Rotation.y}";
        TitleEditor.GetComponent<TMP_InputField>().text = $"{Title}";
    }

    public void ToggleInvalidInputScreen()
    {
        InvalidInputScreen.SetActive(!InvalidInputScreen.activeInHierarchy);
    }

    public void UpdateTitle(TMP_InputField value)
    {
        Title = value.text;
    }

    // Disallow non-numerics in position/rotation fields
    public void UpdateLatitude(TMP_InputField value)
    {
        try { Position.x = Convert.ToDouble(value.text); }
        catch { value.text = $"{Position.x}"; ToggleInvalidInputScreen(); }
    }
    public void UpdateLongitude(TMP_InputField value)
    {
        try { Position.y = Convert.ToDouble(value.text); }
        catch { value.text = $"{Position.y}"; ToggleInvalidInputScreen(); }
    }
    public void UpdateHeight(TMP_InputField value)
    {
        try { Position.z = Convert.ToDouble(value.text); }
        catch { value.text = $"{Position.z}"; ToggleInvalidInputScreen(); }
    }
    public void UpdateRotationX(TMP_InputField value)
    {
        try { Rotation.x = Convert.ToSingle(value.text); }
        catch { value.text = $"{Rotation.x}"; ToggleInvalidInputScreen(); }
    }
    public void UpdateRotationY(TMP_InputField value)
    {
        try { Rotation.y = Convert.ToSingle(value.text); }
        catch { value.text = $"{Rotation.y}"; ToggleInvalidInputScreen(); }
    }


    public void SaveChanges()
    {
        ViewController.GetComponent<ViewController>().EditView(View, Title, Position, Rotation);
        FlyToButton.GetComponent<FlyToButtonScript>().DisableViewEditorUI();
    }
    public void Cancel()
    {
        FlyToButton.GetComponent<FlyToButtonScript>().DisableViewEditorUI();
    }

    
    // Open the in-scene view editing phase.
    // Button binds here because the FlyToButton is a prefab, cannot bind directly.
    public void OpenEditInSceneUI()
    {
        FlyToButton.GetComponent<FlyToButtonScript>().OpenEditInSceneUI();
    }
}
