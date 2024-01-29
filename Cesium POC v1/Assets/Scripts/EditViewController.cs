using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
using TMPro;


public class EditViewController : MonoBehaviour
{
    public GameObject ViewController;

    public string Title, tBackup;
    public double3 Position, pBackup;
    public Vector2 Rotation, rBackup;


    public GameObject LatitudeEditor;
    public GameObject LongitudeEditor;
    public GameObject HeightEditor;

    public GameObject RotationXEditor;
    public GameObject RotationYEditor;

    public GameObject InvalidInputScreen;

    // The button that enabled the editing screen; to 'cancel' and edit.
    public GameObject FlyToButton;

    private void Awake()
    {
        LatitudeEditor.GetComponent<TMP_InputField>().text = $"{Position.x}";
        LongitudeEditor.GetComponent<TMP_InputField>().text = $"{Position.y}";
        HeightEditor.GetComponent<TMP_InputField>().text = $"{Position.z}";
        RotationXEditor.GetComponent<TMP_InputField>().text = $"{Rotation.x}";
        RotationYEditor.GetComponent<TMP_InputField>().text = $"{Rotation.y}";
        pBackup = Position;
        rBackup = Rotation;
        tBackup = Title;
    }

    public void ToggleInvalidInputScreen()
    {
        InvalidInputScreen.SetActive(!InvalidInputScreen.activeInHierarchy);
    }

    public void UpdateTitle(TMP_InputField value)
    {
        Title = value.text;
    }

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
        ViewController.GetComponent<ViewController>().EditView(tBackup, pBackup, rBackup, Title, Position, Rotation);
        FlyToButton.GetComponent<FlyToButtonScript>().CancelViewEditor();
    }
    public void Cancel()
    {
        FlyToButton.GetComponent<FlyToButtonScript>().CancelViewEditor();
    }
}
