using CesiumForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CesiumAssetsTest : MonoBehaviour
{
    public CesiumIonServer IonServer;
    public CesiumIonServerManager IonServerManager;
    public void Test()
    {
        IonServer = (CesiumIonServer)ScriptableObject.CreateInstance("CesiumIonServer");
        Debug.Log(IonServer.serverUrl);
    }
}