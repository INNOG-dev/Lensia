using UnityEngine;
using UnityEngine.UI;

public class MouseOnRectTest : MonoBehaviour
{

    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if(InterfaceUtils.mouseIsOnRect((RectTransform)img.transform, RenderMode.ScreenSpaceOverlay))
        {
            img.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
    }
}
