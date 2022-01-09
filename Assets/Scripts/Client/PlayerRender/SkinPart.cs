using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinPart 
{

    private Transform part;

    private List<SkinPart> childs = new List<SkinPart>();

    private List<SkinPart> duplicateParts = new List<SkinPart>();

    private SkinPart parentPart;

    private SpriteRenderer spriteRenderer;

    public enum SkinBodyPart
    {
        HEAD,
        BASE,
        FEET
    };

    private SkinBodyPart bodyType;

    private Color defaultColor;

    public SkinPart(SkinPart parent, Transform part, SkinBodyPart bodyType)
    {
        this.part = part;
        this.bodyType = bodyType;
        this.parentPart = parent;
    }

    public SkinBodyPart getBodyType()
    {
        return bodyType;
    }

    public SkinPart BuildPart(SkinComposition.DisplayType type)
    {
        spriteRenderer = part.GetComponent<SpriteRenderer>();

        if(spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
            if(type == SkinComposition.DisplayType.MASK)
            {
                spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }
        }

        for (int i = 0; i < part.childCount; i++)
        {
            childs.Add(new SkinPart(this,part.GetChild(i),bodyType).BuildPart(type));
        }

        return this;
    }

    public void resetColor()
    {
        if(spriteRenderer != null) spriteRenderer.color = defaultColor;
        foreach(SkinPart duplicatePart in duplicateParts)
        {
            if (duplicatePart.getSpriteRenderer() != null) duplicatePart.getSpriteRenderer().color = defaultColor;
        }
    }


    public List<SkinPart> getParts()
    {
        return childs;
    }

    public List<SkinPart> getAllParts()
    {
        List<SkinPart> allParts = new List<SkinPart>();
        foreach(SkinPart part in childs)
        {
            allParts.Add(part);
            foreach(SkinPart childOfChild in part.getAllParts())
            {
                allParts.Add(childOfChild);
            }
        }
        return allParts;
    }

    public List<SkinPart> getAllSpriteRenderer()
    {
        List<SkinPart> allParts = new List<SkinPart>();
        foreach (SkinPart part in childs)
        {
            if(part.hasSprite()) allParts.Add(part);
            foreach (SkinPart childOfChild in part.getAllSpriteRenderer())
            {
                if (childOfChild.hasSprite()) allParts.Add(childOfChild);
            }
        }
        return allParts;
    }

    public List<SkinPart> getDuplicateParts()
    {
        return duplicateParts;
    }

    public void findDuplicateParts()
    { 

        string partName = getName();
       
        if(parentPart != null)
        {
         
            foreach (SkinPart part in parentPart.childs)
            {

                if (part == this) continue;


                if (part.getName().Contains(getName()))
                {
                    duplicateParts.Add(part);
                }
            }
        }
    }

    public bool partIsDuplicated(SkinPart part)
    {
       return duplicateParts.Contains(part);
    }

    public SkinPart getParent()
    {
        return parentPart;
    }

    public bool isColoreable()
    {
        return part.tag != "NoColorableSprite";
    }

    public SpriteRenderer getSpriteRenderer()
    {
        return spriteRenderer;
    }

    public bool hasSprite()
    {
        return spriteRenderer != null;
    }

    public string getName()
    {
        return part.name;
    }

    public void savePartColor(uint skinId)
    { 
        int index = 0;

        string key = skinId + "" + index;

        PlayerPrefs.SetString(key, getSpriteRenderer().color.ToString());

        PlayerPrefs.Save();
    }

    public void loadPartColor(uint skinId)
    {
        int index = 0;

        string key = skinId + "-" + index;

        if (!PlayerPrefs.HasKey(key)) return;

        string colorHex = PlayerPrefs.GetString(key);

        Color color;
        ColorUtility.TryParseHtmlString(colorHex, out color);

        getSpriteRenderer().color = color;
    }

    public Transform getTransform()
    {
        return part;
    }
    



}
