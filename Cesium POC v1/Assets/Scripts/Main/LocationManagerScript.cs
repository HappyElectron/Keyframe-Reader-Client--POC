using UnityEngine;
using System.Collections;
using CesiumForUnity;
using Unity.Mathematics;

/// <summary>
/// Enables the behaviour of flying to the user's device location.
/// 
/// Higher accuracy (values approaching 0) will drain battery significantly faster.
/// 
/// Since high precision is kind of important for this application, but the user likely 
/// isn't going to snap to their location too often, it may be appropriate to enable/disable 
/// location services as they're needed. Unfortunately, this is quite slow. 
/// I have left this setting optionally enabled.
/// 
/// </summary>

public class LocationManagerScript : MonoBehaviour
{
    public ModifiedFlyToController FlyToController;

    // If true, will dynamically enable location tracking, as opposed to just leaving it on.
    public bool DisableLocationTracking;

    public float locationAccuracy = 0.5f;
    public float locationUpdateFloor = 0.5f;


    private void Awake()
    {
        StartCoroutine(EnableLocationService());
    }

    public void FlyToButton()
    {
        if (DisableLocationTracking)
            StartCoroutine(EnableLocationService());
        else
            FlyToDeviceLocation();
        }

    void FlyToDeviceLocation()
    {
        FlyToController.FlyToLocationLongitudeLatitudeHeight(
    new double3(Input.location.lastData.longitude, Input.location.lastData.latitude, Input.location.lastData.altitude),
    0, 0, false);
    }
    

    /// <summary>
    /// Enable location tracking, fly to the device's current location.
    /// </summary>
    /// <returns></returns>
    public IEnumerator EnableLocationService()
    {
        Debug.Log("Enabling location services...");
        // For testing
        #if UNITY_EDITOR
                yield return new WaitWhile(() => !UnityEditor.EditorApplication.isRemoteConnected);
        #elif UNITY_IOS
            if (!UnityEngine.Input.location.isEnabledByUser) {
                Debug.LogFormat("IOS and Location not enabled");
                yield break;
            }
        #endif
        // Start service before querying location
        UnityEngine.Input.location.Start(locationAccuracy, locationUpdateFloor);

        // Wait until service initializes
        int maxWait = 15;
        while (UnityEngine.Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSecondsRealtime(1);
            maxWait--;
            Debug.Log(maxWait);
        }
        // Editor has a bug which doesn't set the service status to Initializing. So extra wait in Editor.
#if UNITY_EDITOR
        int editorMaxWait = 15;
        while (Input.location.status == LocationServiceStatus.Stopped && editorMaxWait > 0)
        {
            yield return new WaitForSecondsRealtime(1);
            editorMaxWait--;
        }
#endif
        // Service didn't initialize in 15 seconds
        if (maxWait < 1)
        {
            Debug.LogFormat("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogFormat("Unable to determine device location. Failed with status {0}", Input.location.status);
            yield break;
        }
        else
        {
            Debug.LogFormat("Location service live. status {0}", Input.location.status);
            
            FlyToController.FlyToLocationLongitudeLatitudeHeight(
                new double3(Input.location.lastData.longitude, Input.location.lastData.latitude, Input.location.lastData.altitude),
                0, 0, false);
        }

        if (DisableLocationTracking)
        {
            // Disable service to save battery
            Input.location.Stop();
        }
    }
}