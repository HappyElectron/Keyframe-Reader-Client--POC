using CesiumForUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneManager : MonoBehaviour
{
    public GameObject ConfirmUI;
    public TMP_Text text;

    public ModifiedCameraController cam;

    public void ConfirmLoadScene()
    {
        ConfirmUI.SetActive(true);
        text.text = "Open menu?";
        cam.enabled = false;
    }

    public void OpenMenu()
    {
        cam.enabled = true;
        SceneManager.LoadScene("Menu");
    }
    public void CancelLoad()
    {
        ConfirmUI.SetActive(false);
        cam.enabled = true;
    }
}
