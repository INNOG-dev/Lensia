using UnityEngine;
using UnityEngine.UI;

public class ColorBox 
{

    private SkinPart attribuatedSkinPart;

    private Image ColorBoxImage;

    public ColorBox(SkinPart attribuatedSkinPart, Image ColorBoxImage)
    {
        this.attribuatedSkinPart = attribuatedSkinPart;
        this.ColorBoxImage = ColorBoxImage;
    }

    public SkinPart getPart()
    {
        return attribuatedSkinPart;
    }

    public Image getColorBox()
    {
        return ColorBoxImage;
    }


}
