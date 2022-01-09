using System.Collections.Generic;
using UnityEngine;

public class SkinComposition 
{

    private List<SkinPart> skinParts = new List<SkinPart>();

    public enum DisplayType
    {
        MASK,
        NORMAL
    };

    public SkinComposition BuildParts(DisplayType type, Transform skinsGraphics)
    {
        for(int i = 0; i < skinsGraphics.childCount; i++)
        {
            skinParts.Add(new SkinPart(null,skinsGraphics.GetChild(i), (SkinPart.SkinBodyPart)i).BuildPart(type));
        }

        mergeDuplicateParts();

        return this;
    }

    public SkinPart getSkinPart(int index)
    {
        return skinParts[index];
    }

    public List<SkinPart> getAllParts()
    {
        List<SkinPart> parts = new List<SkinPart>();

        foreach(SkinPart part in skinParts)
        {
            foreach(SkinPart childPart in part.getAllParts())
            {
                parts.Add(childPart);
            }
        }

        return parts;
    }

    public List<SkinPart> getAllSpriteRenderer()
    {
        List<SkinPart> parts = new List<SkinPart>();

        foreach (SkinPart part in skinParts)
        {
            foreach (SkinPart childPart in part.getAllParts())
            {
                if (childPart.hasSprite() && childPart.isColoreable()) parts.Add(childPart);
            }
        }

        return parts;
    }

    public int getPartCount()
    {
        return skinParts.Count;
    }

    public bool partIsDuplicated(SkinPart part)
    { 
        if (part.getParent() == null) return false;

        foreach (SkinPart skinPart in part.getParent().getParts())
        {
            if (skinPart == part) continue;

            if(skinPart.getDuplicateParts().Contains(part)) return true;
        }

        return false;
    }

    public void mergeDuplicateParts()
    {
        foreach(SkinPart part in getAllParts())
        {
            part.findDuplicateParts();
        }
    }

    public void applyColors(Color[] colors)
    {
        if (colors.Length > 0)
        {
            List<SkinPart> parts = getAllSpriteRenderer();
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i].getSpriteRenderer().color = colors[i];
            }
        }
    }

    public void savePartColor(uint skinId)
    {
        int index = 0;
        List<SkinPart> parts = getAllSpriteRenderer();
        string username = Client.getClient().GetNetworkUser().getAccountData().getUsername();
        PlayerPrefs.SetInt(username + "-" + skinId + "-partCount", parts.Count);
        foreach (SkinPart part in parts)
        {
            string key = username + "-" + skinId + "-" + index;
            PlayerPrefs.SetString(key, "#" + ColorUtility.ToHtmlStringRGB(part.getSpriteRenderer().color));
            index++;
        }

        PlayerPrefs.Save();
    }

    public void loadPartColor(uint skinId)
    {
        int index = 0;
        string username = Client.getClient().GetNetworkUser().getAccountData().getUsername();
        foreach (SkinPart part in getAllSpriteRenderer())
        {
            string key = username + "-" + skinId + "-" + index;

            index++;

            if (!PlayerPrefs.HasKey(key)) continue;

            string colorHex = PlayerPrefs.GetString(key);

            Color color;
            ColorUtility.TryParseHtmlString(colorHex, out color);

            part.getSpriteRenderer().color = color;
        }
    }

}
