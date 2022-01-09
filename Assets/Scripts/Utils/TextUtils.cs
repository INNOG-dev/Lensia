using UnityEngine;
using TMPro;

public class TextUtils
{

    public static float GetWidth(TextMesh mesh)
    {
        float width = 0;
        foreach (char symbol in mesh.text)
        {
            CharacterInfo info;

            if (mesh.font.GetCharacterInfo(symbol, out info, mesh.fontSize, mesh.fontStyle))
            {
                width += info.advance;
            }
        }
        return width * mesh.characterSize * 0.1f;
    }

}
