using CesiumForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

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

    private void Awake()
    {
        SaveFilePath = Application.persistentDataPath + "/PlayerData.json";

        SelectedViewsetIdsPath = Application.persistentDataPath + "/viewsetsToLoad";
        SelectedTilesetIdsPath = Application.persistentDataPath + "/tilesetsToLoad";

        ViewsetIds = JsonUtility.FromJson<SerializableStringSetContainer>(File.ReadAllText(SelectedViewsetIdsPath));
        TilesetIds = JsonUtility.FromJson<SerializableStringSetContainer>(File.ReadAllText(SelectedTilesetIdsPath));

        // Instantiate a gameobject with a tileset component for each id from ION
        // Instantiating from a prefab does not work FSR, but this does.
        // Will investigate later.
        foreach(string strId in TilesetIds.set)
        {
            var tileset = Instantiate(TilesetToDup, TilesetToDup.transform.parent);
            tileset.GetComponent<Cesium3DTileset>().ionAssetID = int.Parse(strId);
            tileset.SetActive(true);
        }

        foreach(string strId in ViewsetIds.set)
        {
            StartCoroutine(GetGeoJSON(strId));
        }
    }

    private IEnumerator GetGeoJSON(string strId)
    {
        Debug.Log("In ienumerator");

        // Get archive info
        string archivesUrl = $"https://api.cesium.com/v1/assets/{strId}/archives";
        UnityWebRequest assetArchives = UnityWebRequest.Get(archivesUrl);
        assetArchives.SetRequestHeader("Authorization", "Bearer " + "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzZmQzNTAxOS01ODU4LTQxNzktOTUxMy1hY2U1MWJiZGI0YjYiLCJpZCI6MTg3MDM3LCJpYXQiOjE3MDgyMTIzNDl9.Mld81dY1EHqZMN4dTWbpdC_ROHZLSzrkFBTVQOXZuQE");
        yield return assetArchives.SendWebRequest();
        string output = assetArchives.downloadHandler.text;
        Debug.Log(output);
        ListOfArchiveAssets listOfAssetArchives = JsonUtility.FromJson<ListOfArchiveAssets>(output);
        Debug.Log(listOfAssetArchives.items);
        string assetArchiveId = listOfAssetArchives.items.First().id.ToString();

        string archiveUrl = $"https://api.cesium.com/v1/assets/{strId}/archives/{assetArchiveId}/download";
        UnityWebRequest archive = UnityWebRequest.Get(archiveUrl);
        archive.SetRequestHeader("Authorization", "Bearer " + "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzZmQzNTAxOS01ODU4LTQxNzktOTUxMy1hY2U1MWJiZGI0YjYiLCJpZCI6MTg3MDM3LCJpYXQiOjE3MDgyMTIzNDl9.Mld81dY1EHqZMN4dTWbpdC_ROHZLSzrkFBTVQOXZuQE");
        yield return archive.SendWebRequest();
        output = archive.downloadHandler.text;

        Debug.Log(output);
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
