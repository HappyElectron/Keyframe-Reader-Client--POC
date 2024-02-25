using CesiumForUnity;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class IonAssetController : MonoBehaviour
{
    public TMP_Dropdown IONToken;
    public GameObject TokenErrorScreen;
    public TMP_Text TokenErrorScreenReason;

    public GameObject ViewsetScrollviewContent;
    public GameObject TilesetScrollviewContent;

    Root DeserializedIONMetadata;
    List<IONMetadataObject> DeserializedKeyframeViewSetList;
    List<IONMetadataObject> DeserializedTilesetList;

    List<MetaDataContainer> ViewsetContainers = new();
    List<MetaDataContainer> TilesetContainers = new();

    public GameObject ViewsetButtonPrefab;
    public GameObject TilesetButtonPrefab;

    string ViewsetPath;
    string TilesetPath;

    public Dictionary<string, string> Tokens = new Dictionary<string, string>();

    private void Awake()
    {
        ViewsetPath = Application.persistentDataPath + "/viewsetsToLoad";
        TilesetPath = Application.persistentDataPath + "/tilesetsToLoad";

        StartCoroutine(GetTokens());
    }

    /// <summary>
    /// Retrieve a list of cesium ION tokens, and initialise the dropdown values
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetTokens()
    {
        string IONTokenURLs = "https://api.cesium.com/v2/tokens";
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(IONTokenURLs);

        // I don't really know what's happening here - building for xcode might explain?
        string accessToken = CesiumIonServer.defaultServer.defaultIonAccessToken;

        unityWebRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
        yield return unityWebRequest.SendWebRequest();

        TokenList myDeserializedClass = JsonConvert.DeserializeObject<TokenList>(unityWebRequest.downloadHandler.text);

        IONToken.options.Clear();
        Tokens.Clear();
        foreach (TokenItem t in myDeserializedClass.items)
        {
            IONToken.options.Add(new TMP_Dropdown.OptionData() { text = t.name, image = null });
            Tokens.Add(t.name, t.token);
        }
    }

    /// <summary>
    /// Called by UI, on dropdown change
    /// Updates the token used to pull assets.
    /// </summary>
    /// <param name="token"></param>
    public void RefreshAssetList(TMP_Dropdown token)
    {
        string tokenString = Tokens[token.options[token.value].text];
        StartCoroutine(SendGetAssetListRequest(tokenString));
    }


    /// <summary>
    /// Retrieve all ION assets accessible by the given token
    /// </summary>
    /// <returns></returns>
    public IEnumerator SendGetAssetListRequest(string token) {
        //Modelled cURL request: 'curl "https://api.cesium.com/v1/assets" -H "Authorization: Bearer <your_access_token>"'
        UnityWebRequest request = UnityWebRequest.Get("https://api.cesium.com/v1/assets");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
            // SHOULD be the only two errors possible...
            // Haven't included anything for if ION is down. Maybe later.
            if (request.error == "HTTP/1.1 401 Unauthorized")
                ShowTokenErrorScreen("Invalid token");
            if (request.error == "HTTP/1.1 404 Not Found")
                ShowTokenErrorScreen("Token requires list, read, and write permissions");
        }
        else {
            // Deserialize data, split into sets of keyframe views, and non-geojson data (ie tilesets)
            // Overwrites existing data
            DeserializedIONMetadata = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
            DeserializedKeyframeViewSetList = DeserializedIONMetadata.items.Where(x => x.name.StartsWith("Keyframe viewset:") && x.type == "GEOJSON").ToList();

            // Do not select the globe - included in scene by default.
            DeserializedTilesetList = DeserializedIONMetadata.items.Where(x => x.type != "GEOJSON" && x.id != 1).ToList();


            // Clear existing viewset/tileset metadata, and associated gameobjects
            ViewsetContainers.Clear();
            TilesetContainers.Clear();
            foreach(Transform g in ViewsetScrollviewContent.GetComponent<Transform>())
                Destroy(g.gameObject);
            foreach (Transform g in TilesetScrollviewContent.GetComponent<Transform>())
                Destroy(g.gameObject);

            // Create a button for each item, container class holds button ref + data.
            foreach (var v in DeserializedKeyframeViewSetList)
            {
                GameObject vButton = Instantiate(ViewsetButtonPrefab, ViewsetScrollviewContent.GetComponent<Transform>());
                MetaDataContainer m = new MetaDataContainer() { Data = v, Button = vButton, Active = false };
                ViewsetContainers.Add(m);
                vButton.GetComponent<ViewsetButton>().MetaDataContainer = m;
                vButton.GetComponent<ViewsetButton>().IONAssetController = this;
                vButton.GetComponent<ViewsetButton>().UIText = v.name;
            }

            foreach (var t in DeserializedTilesetList)
            {
                GameObject tButton = Instantiate(TilesetButtonPrefab, TilesetScrollviewContent.GetComponent<Transform>());
                MetaDataContainer m = new MetaDataContainer() { Data = t, Button = tButton, Active = false };
                TilesetContainers.Add(m);
                tButton.GetComponent<TilesetButton>().MetaDataContainer = m;
                tButton.GetComponent<TilesetButton>().IONAssetController = this;
                tButton.GetComponent<TilesetButton>().UIText = t.name;
            }
        }
    }

    /// <summary>
    /// Called by a button
    /// Write the selected asset ids to files (viewsets vs tilesets)
    /// Open main scene
    /// </summary>

    public void OpenMainScene()
    {
        SerializableStringSetContainer ViewsetIdsContainer = new();
        SerializableStringSetContainer TilesetIdsContainer = new();

        ViewsetIdsContainer.set = ViewsetContainers.Where(x => x.Active).Select(x => x.Data.id.ToString()).ToList();
        File.WriteAllText(ViewsetPath, JsonUtility.ToJson(ViewsetIdsContainer));
        TilesetIdsContainer.set = TilesetContainers.Where(x => x.Active).Select(x => x.Data.id.ToString()).ToList();
        File.WriteAllText(TilesetPath, JsonUtility.ToJson(TilesetIdsContainer));

        SceneManager.LoadScene("Main");
    }


    // Called by Viewset buttons, updates whether the viewset is to be used by the main scene.
    // Presently, only allow 1 viewset at a time.
    public void ToggleViewsetActive(MetaDataContainer metaDataContainer)
    {
        foreach(var v in ViewsetContainers.Where(v => v != metaDataContainer))
        {
            v.Button.GetComponent<ViewsetButton>().ActiveButton.SetActive(false);
            v.Button.GetComponent<ViewsetButton>().InactiveButton.SetActive(true);
            v.Active = false;
        }
    }

    public void ToggleTilesetActive(MetaDataContainer metaDataContainer)
    {

    }


    public void ShowTokenErrorScreen(string reason)
    {
        TokenErrorScreen.SetActive(true);
        TokenErrorScreenReason.text = reason;
    }

    public void CloseTokenErrorScreen()
    {
        TokenErrorScreen.SetActive(false);
    }
}

/// <summary>
/// Exists to serialize 2 lists, in IONAssetsController.OpenMainMenu
/// </summary>
[Serializable]
class SerializableStringSetContainer
{
    public List<string> set;
}


public class MetaDataContainer
{
    public IONMetadataObject Data { get; set; }
    public GameObject Button { get; set; }
    public bool Active { get; set; }
}

public class IONMetadataObject
{
    public int id { get; set; }
    public string type { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public int bytes { get; set; }
    public string attribution { get; set; }
    public DateTime dateAdded { get; set; }
    public bool exportable { get; set; }
    public string status { get; set; }
    public int percentComplete { get; set; }
    public bool archivable { get; set; }
    public int userId { get; set; }
    public List<object> labels { get; set; }
    public bool isPremium { get; set; }
    public bool isExternal { get; set; }
    public object error { get; set; }
    public string progressMessage { get; set; }
}

public class Root
{
    public List<IONMetadataObject> items { get; set; }
}

public class TokenItem
{
    public string id { get; set; }
    public string name { get; set; }
    public string token { get; set; }
    public bool isDefault { get; set; }
    public DateTime dateLastUsed { get; set; }
    public DateTime dateAdded { get; set; }
    public DateTime dateModified { get; set; }
    public List<string> scopes { get; set; }
    public object allowedUrls { get; set; }
}

public class TokenList
{
    public List<TokenItem> items { get; set; }
}