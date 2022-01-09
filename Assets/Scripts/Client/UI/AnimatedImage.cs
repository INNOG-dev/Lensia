using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AnimatedImage : MonoBehaviour
{
    public float duration;

    public string spritePath;
    public int spriteCount;

    [SerializeField]
    private Sprite[] sprites;

    private Image image;

    private int index = 0;

    private float timer = 0;

    void Start()
    {
        if(spritePath != "")
        {
            sprites = new Sprite[spriteCount];
            for(int i = 0; i < spriteCount; i++)
            {
                sprites[i] = Resources.Load<Sprite>(spritePath + (i + 1));
            }
        }
        image = GetComponent<Image>();
    }
    private void Update()
    {
        if ((timer += Time.deltaTime) >= (duration / sprites.Length))
        {
            timer = 0;
            image.sprite = sprites[index];
            index = (index + 1) % sprites.Length;
        }
    }
}