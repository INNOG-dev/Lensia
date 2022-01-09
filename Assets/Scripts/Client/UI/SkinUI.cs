using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SkinUI 
{
    private PlayerSkin skin;

    private SkinComposition composition;

    private GameObject skinObject;

    public SkinUI(uint skinId)
    {
        skin = RegistryManager.skinRegistry.get(skinId);

        if(skin == null)
        {
            throw new System.Exception("Skin doesn't exist");
        }
    }

    public void displaySkin(Transform at, bool displayMask)
    {
        skinObject = Object.Instantiate<GameObject>(skin.getSkinGameObject().transform.GetChild(0).gameObject, at);

        skinObject.GetComponentInChildren<Animator>().enabled = false;

        SortingGroup sortingGroup = skinObject.GetComponentInChildren<SortingGroup>();
        sortingGroup.sortingOrder = 10;
        sortingGroup.sortingLayerName = "SkinUI";

        skinObject.transform.GetChild(0).gameObject.SetActive(false);

        composition = new SkinComposition();

        composition.BuildParts(displayMask == true ? SkinComposition.DisplayType.MASK : SkinComposition.DisplayType.NORMAL,skinObject.transform.GetChild(1));

        composition.loadPartColor(skin.getId());
    }

    public PlayerSkin getSkin()
    {
        return skin;
    }

    public GameObject getSkinObject()
    {
        return skinObject;
    }

    public SkinComposition getSkinComposition()
    {
        return composition;
    }

    public void destroySkinDisplay()
    {
        Object.Destroy(getSkinObject());
    }
}
