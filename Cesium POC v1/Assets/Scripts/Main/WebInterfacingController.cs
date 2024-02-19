using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebInterfacingController : MonoBehaviour
{
    public ViewController ViewController;
    public void SaveUserViewChanges()
    {
        ViewController.SaveViewChanges();
    }
}