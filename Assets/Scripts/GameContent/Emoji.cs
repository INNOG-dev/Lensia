using UnityEngine;

public class Emoji : RegistryObject
{

    public static GameObject EmojiObject;

    private Sprite emojiSprite;

    private int categoryIndex;

    public Emoji(string registryName,int categoryIndex)
    {
        if (EmojiObject == null) EmojiObject = Resources.Load<GameObject>("GameResources/Emoji/EmojiObject");

        setRegistryName(registryName);

        this.categoryIndex = categoryIndex;
    }

    public Sprite getGraphic()
    {
        return emojiSprite;
    }

    public int getCategory()
    {
        return categoryIndex;
    }

    public override void onRegistered()
    {
        Debug.Log("GameResources/Emoji/" + getId() + "/Texture/emoji");
        emojiSprite = Resources.Load<Sprite>("GameResources/Emoji/" + getId() + "/Texture/emoji");

        base.onRegistered();
    }


}
