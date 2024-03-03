using CesiumForUnity;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEditor.Rendering.CameraUI;

[Serializable]
public class ViewData
{
    public string Name;
    public double3 Position;
    public Vector2 Rotation;
}

public class View
{
    public ViewData ViewData;
    public GameObject SmallViewGameObj;
    public GameObject EditableViewGameObj;
}

public class ViewController : MonoBehaviour
{
    public List<View> Views = new List<View>();

    public GameObject DynamicCamera;
    public CesiumGlobeAnchor CameraGlobeAnchor;
    public GameObject CesiumGeoreference;

    public GameObject ViewNameInputContainer;
    public GameObject ViewNameInput;

    public GameObject ViewScroll;
    public GameObject ViewScrollContent;
    public GameObject JoystickLookToggle;

    // Reference to ui element prefab.
    public GameObject FlyToPrefab;

    public GameObject MovementModeController;
    public TMP_Dropdown MovementModeSelect;

    public GameObject ViewEditScreen;
    public GameObject ViewEditScreenScrollContent;
    public GameObject EditableViewPrefab;

    // JSON fields
    string SaveFilePath;

    string SelectedViewsetIdsPath;
    string SelectedTilesetIdsPath;

    SerializableStringSetContainer ViewsetIds;
    SerializableStringSetContainer TilesetIds;

    public GameObject TilesetToDup;

    string IONAccessToken;

    private async void Awake()
    {
        SaveFilePath = Application.persistentDataPath + "/PlayerData.json";

        SelectedViewsetIdsPath = Application.persistentDataPath + "/viewsetsToLoad";
        SelectedTilesetIdsPath = Application.persistentDataPath + "/tilesetsToLoad";

        IONAccessToken = CesiumIonServer.defaultServer.defaultIonAccessToken;

        ViewsetIds = JsonUtility.FromJson<SerializableStringSetContainer>(File.ReadAllText(SelectedViewsetIdsPath));
        TilesetIds = JsonUtility.FromJson<SerializableStringSetContainer>(File.ReadAllText(SelectedTilesetIdsPath));

        // Instantiate a gameobject with a tileset component for each id from ION
        // Instantiating from a prefab does not work FSR, but this does.
        // Will investigate later.
        foreach(string strId in TilesetIds.set) {
            var tileset = Instantiate(TilesetToDup, TilesetToDup.transform.parent);
            tileset.GetComponent<Cesium3DTileset>().ionAssetID = int.Parse(strId);
            tileset.SetActive(true);
        }

        // Download & extracts archives for every viewset selected (as of 2024/03/03, should be only 1 at a time)
        // File names are {assetId}.zip and {assetId}, respectively.
        // Clears old files first.
        Directory.Delete(Application.persistentDataPath + "/archives",true);
        Directory.CreateDirectory(Application.persistentDataPath + "/archives");
        foreach(string strId in ViewsetIds.set) {
            string archivesUrl = $"https://api.cesium.com/v1/assets/{strId}/archives";
            await sendRequest(archivesUrl, strId);
        }
    }

    /// <summary>
    /// Download information about an asset's available archives, and then download the first available archive.
    /// 
    /// First tried doing this with UnityWebRequests, which did not work. 
    /// I still don't know why, it wasted a LOT of time.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="assetId"></param>
    /// <returns></returns>
    async Task sendRequest(string url, string assetId)
    {
        string SpecificArchiveURL = "";

        // Get information about the archives available
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + IONAccessToken);

            try {
                HttpResponseMessage response = await client.GetAsync(new Uri(url));

                // On success, obtain the url of the archive from the request. 
                if (response.IsSuccessStatusCode)
                {
                    string archiveIds = await response.Content.ReadAsStringAsync();
                    ListOfArchiveAssets listOfAssetArchiveIds = JsonConvert.DeserializeObject<ListOfArchiveAssets>(archiveIds);

                    // Only ever want 1 archive - there should not be multiple available, at least in our context.
                    string assetArchiveId = listOfAssetArchiveIds.items.First().id.ToString();
                    SpecificArchiveURL = $"https://api.cesium.com/v1/assets/{assetId}/archives/{assetArchiveId}/download";
                } 
                else { Debug.LogError("Error during download. Status code: " + response.StatusCode); }
            } catch (Exception e) { Debug.LogError("Exception during download: " + e.Message); }
        }

        // Download the specific archive
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + IONAccessToken);

            try {
                HttpResponseMessage response = await client.GetAsync(new Uri(SpecificArchiveURL));

                if (response.IsSuccessStatusCode)
                {
                    // Save the archive to a zip file, extract
                    byte[] content = await response.Content.ReadAsByteArrayAsync();

                    string zipPath = Application.persistentDataPath + "/archives/" + assetId + ".zip";
                    string extractPath = Application.persistentDataPath + "/archives/" + assetId;
                    File.WriteAllBytes(zipPath, content);
                    ZipFile.ExtractToDirectory(zipPath, extractPath);
                } 
                else { Debug.LogError("Error during download. Status code: " + response.StatusCode); }
            } catch (Exception e) { Debug.LogError("Exception during download: " + e.Message); }
        }
    }

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
            ViewData = new ViewData()
            {
                Name = name.text,
                Position = CameraGlobeAnchor.longitudeLatitudeHeight,
                Rotation = new Vector2((180 / math.PI) * Quaternion.ToEulerAngles(CameraGlobeAnchor.rotationEastUpNorth).x,
                                                        (180 / math.PI) * Quaternion.ToEulerAngles(CameraGlobeAnchor.rotationEastUpNorth).y)
            }
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
        ViewScroll.SetActive(!ViewScroll.activeInHierarchy);
    }

    public void EditView(View view, string title, double3 position, Vector2 rotation)
    {
        view.ViewData.Position = position;
        view.ViewData.Rotation = rotation;
        view.ViewData.Name = title;

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

    public void SaveViewChanges()
    {
        File.Delete(SaveFilePath);
        Debug.Log(SaveFilePath);

        foreach(var view in Views)
        {
            ViewData viewData = view.ViewData;
            string SaveViewData = JsonUtility.ToJson(viewData);
            File.AppendAllText(SaveFilePath, SaveViewData);
        }
    }
}


public class ArchiveAsset
{
    public int id { get; set; }
    public int assetId { get; set; }
    public string format { get; set; }
    public string status { get; set; }
    public int bytesArchived { get; set; }
}

public class ListOfArchiveAssets
{
    public List<ArchiveAsset> items { get; set; }
}
