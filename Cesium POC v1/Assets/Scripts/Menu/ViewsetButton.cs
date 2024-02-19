using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ViewsetButton : MonoBehaviour
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
    public void ToggleViewsetActivity()
    {
        ActiveButton.SetActive(!ActiveButton.activeInHierarchy);
        InactiveButton.SetActive(!InactiveButton.activeInHierarchy);

        MetaDataContainer.Active = ActiveButton.activeInHierarchy;

        if(ActiveButton.activeInHierarchy)
            IONAssetController.ToggleViewsetActive(MetaDataContainer);
    }
}
