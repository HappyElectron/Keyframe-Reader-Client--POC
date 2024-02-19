using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TilesetButton : MonoBehaviour
{
    public IonAssetController IONAssetController;
    public MetaDataContainer MetaDataContainer;

    public GameObject ActiveButton;
    public GameObject InactiveButton;

    public TMP_Text textChild;

    public string UIText
    {
        get { return textChild.text; }
        set { textChild.text = value; }
    }

    // Called on either button's press - update the UI, alert controller.
    public void ToggleTilesetActivity()
    {
        ActiveButton.SetActive(!ActiveButton.activeInHierarchy);
        InactiveButton.SetActive(!InactiveButton.activeInHierarchy);

        MetaDataContainer.Active = ActiveButton.activeInHierarchy;

        IONAssetController.ToggleTilesetActive(MetaDataContainer);
    }
}
