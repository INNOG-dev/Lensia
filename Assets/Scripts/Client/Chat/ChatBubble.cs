using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatBubble 
{
    private static Transform bubbleSpawnParent;

    private static GameObject bubbleObject;

    private Player owner;

    private GameObject bubbleInstance;

    private TextMeshProUGUI Text;

    private readonly int maxHorizontalSizeX = 320;
    private readonly float referenceHeight = 25.7f;

    public ChatBubble(Player owner, string message)
    { 
        if (bubbleSpawnParent == null) bubbleSpawnParent = GameObject.Find("GameOverlay").transform;

        if (bubbleObject == null) bubbleObject = Resources.Load<GameObject>("GameResources/Chat/ChatBubble");
       
        this.owner = owner;

        bubbleInstance = Object.Instantiate(bubbleObject, bubbleSpawnParent);

        ContentSizeFitter contentSizeFitter = bubbleInstance.GetComponentInChildren<ContentSizeFitter>();
        LayoutElement layoutElement = bubbleInstance.GetComponentInChildren<LayoutElement>();

        Text = bubbleInstance.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

        Text.text = message;

        if (Text.preferredWidth > maxHorizontalSizeX)
        {
            layoutElement.minWidth = maxHorizontalSizeX;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
        }

        Object.Destroy(bubbleInstance, 5f);
    }

    public void Update()
    {
        if (bubbleInstance == null) return;


        Vector2 scaleFactor = new Vector2((float)Screen.width / Main.referenceWidth, (float)Screen.height / Main.referenceHeight);


        float newPositionY = (Text.preferredHeight - referenceHeight) / 2f;

        RectTransform rectTransform = (RectTransform)bubbleInstance.transform.GetChild(0);
      

        bubbleInstance.transform.position = Camera.main.WorldToScreenPoint(owner.getEntityGameObject().transform.position + new Vector3(1.5f * bubbleInstance.transform.localScale.x, 2f, 0f));


        bubbleInstance.transform.position += new Vector3(0, newPositionY * scaleFactor.y, 0);


        if (!new Rect(0, 0, Screen.width, Screen.height).Contains(new Vector2(bubbleInstance.transform.position.x + (rectTransform.sizeDelta.x * scaleFactor.x), bubbleInstance.transform.position.y)))
        {
            Text.transform.localScale = new Vector3(-1, Text.transform.localScale.y, Text.transform.localScale.z);
            bubbleInstance.transform.localScale = new Vector3(-1, bubbleInstance.transform.localScale.y, bubbleInstance.transform.localScale.z);
        }
    }

    public void Destroy()
    {
        Object.Destroy(bubbleInstance);
    }





}
