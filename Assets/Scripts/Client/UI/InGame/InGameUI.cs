using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Linq;

public class InGameUI : MonoBehaviour
{

    public static InGameUI INSTANCE;

    private IUserInterface[] childsUI;

    private Button SitButton;

    private Player currentMouseOverPlayer;

    private PlayerInterractionMenu interractionMenu;

    void Awake()
    {
        INSTANCE = this;

        childsUI = new IUserInterface[] { new EmojiContainer(gameObject), new ChatContainer(gameObject), new OthersContainer(this), new FriendsUI(this) };

        foreach (IUserInterface ui in childsUI)
        {
            ui.initializeComponents();
        }

        interractionMenu = new PlayerInterractionMenu(this,transform.GetChild(5).gameObject);

        gameObject.SetActive(false);
    }

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    // Update is called once per frame
    void Update()
    {
        if (Client.getClient().GetNetworkUser() != null && Client.getClient().GetNetworkUser().getPlayer() != null && SitButton == null)
        {

            SitButton = transform.GetChild(3).GetComponent<Button>();

            SitButton.onClick.AddListener(delegate
            {
                Player thePlayer = Client.getClient().GetNetworkUser().getPlayer();
                thePlayer.sit(!thePlayer.getAnimator().isSitting());
            });
        }

        foreach (IUserInterface ui in childsUI)
        {
            if (ui.getInterfaceObject() == null || ui.getInterfaceObject().activeInHierarchy) ui.updateUI();
        }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hits = Physics2D.GetRayIntersectionAll(ray, 1500f);

        bool mouseOverPlayer = false;
        foreach (var hit in hits)
        {
            if(hit.collider.gameObject.CompareTag("Player"))
            {
                GameObject mouseOverObject = hit.collider.gameObject;
                if (currentMouseOverPlayer != null && mouseOverObject.name != currentMouseOverPlayer.getEntityGameObject().name)
                {
                    currentMouseOverPlayer.getOulineRenderer().displayOutline(false);
                }
                currentMouseOverPlayer = Client.getClient().GetNetworkUser().getPlayer().getCurrentWorld().getPlayersInWorld()[uint.Parse(mouseOverObject.name)];
                currentMouseOverPlayer.getOulineRenderer().displayOutline(true);

                if(Input.GetKeyDown(KeyCode.Mouse0))
                {
                    interractionMenu.setMenuOver(currentMouseOverPlayer);
                }

                mouseOverPlayer = true;
            }
        }

        if(!mouseOverPlayer && currentMouseOverPlayer != null)
        {
            currentMouseOverPlayer.getOulineRenderer().displayOutline(false);
        }

        if(interractionMenu.isActive())
        {
            if(!interractionMenu.isMouseOverMenu())
            {
                if(InterfaceUtils.mouseIsOnRect(interractionMenu.getRectTransform(),RenderMode.ScreenSpaceOverlay))
                {
                    interractionMenu.setMouseOverMenu(true);
                }
            }
            else
            {
                if(!InterfaceUtils.mouseIsOnRect(interractionMenu.getRectTransform(), RenderMode.ScreenSpaceOverlay))
                {
                    interractionMenu.resetMenuDisplay();
                    interractionMenu.setActive(false);
                }
            }
        }

    }

    public IUserInterface getInterface(int childIndex)
    {
        return childsUI[childIndex];
    }


    public class EmojiContainer : IUserInterface
    {

        private ScrollRect ScrollRect;

        public EmojiContainer(GameObject parent)
        {
            this.parent = parent;
        }

        public void initializeComponents()
        {
            ScrollRect = parent.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<ScrollRect>();
            

            List<Emoji> emojiList = RegistryManager.emojiRegistry.getValues();

            foreach(Emoji emoji in emojiList)
            {
                new EmojiObject(emoji, ScrollRect);
            }
        }

        public void onUIClose()
        {
           
        }

        private GameObject parent;

        public void updateUI()
        {
            
        }

        public GameObject getInterfaceObject()
        {
            return null;
        }

        public void onUIEnable()
        {
            throw new NotImplementedException();
        }

        public class EmojiObject
        { 
        
            public static GameObject emojiGraphic;

            public EmojiObject(Emoji attribuatedEmoji, ScrollRect scrollRect)
            {
                if (emojiGraphic == null) emojiGraphic = Resources.Load<GameObject>("GameResources/UI/InGameUI/EmojiBtn");

                GameObject emojiObject = Instantiate(emojiGraphic, scrollRect.content);

                emojiObject.GetComponent<Image>().sprite = attribuatedEmoji.getGraphic();

                emojiObject.GetComponent<Button>().onClick.AddListener(delegate { Client.getClient().GetNetworkUser().getPlayer().sendEmoji(attribuatedEmoji.getId()); });
            }
        }
    }

    public class ChatContainer : IUserInterface
    {
        private readonly static uint maxMessageCount = 100;


        private TMP_InputField InputField;

        private Button SendButton;

        private ScrollRect ScrollRect;

        private List<MessageObject> messageList = new List<MessageObject>();

        public ChatContainer(GameObject parent)
        {
            this.parent = parent;
        }

        public TMP_InputField getInputField()
        {
            return InputField;
        }

        public void initializeComponents()
        {
            ScrollRect = parent.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<ScrollRect>();
            SendButton = parent.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Button>();
            InputField = parent.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<TMP_InputField>();

            SendButton.onClick.AddListener(sendMessage);
        }

        public void addMessageToContainer(string username, string message)
        {
            
            if(messageList.Count == maxMessageCount)
            {
                MessageObject messageObject = messageList[0];
                deleteMessage(messageObject);
            }


            messageList.Add(new MessageObject(username, message, this.InputField, ScrollRect)); ;

            

        }

        public void deleteMessage(MessageObject messageObject)
        {
            Destroy(messageObject.getMessageObject());
            messageList.Remove(messageObject);
        }

        public void clearChat()
        {
            foreach(MessageObject message in messageList.ToList())
            {
                deleteMessage(message);
            }
        }


        public void sendMessage()
        {
            string message = InputField.text;

            if(message.Length == 0) return;


            Main.INSTANCE.getChatManager().sendMessageToServer(message);

            InputField.text = "";
            InputField.ActivateInputField();
        }

        public void onUIClose()
        {

        }

        private GameObject parent;

        public void updateUI()
        {
          
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                sendMessage();
            }
        }

        public GameObject getInterfaceObject()
        {
            return null;
        }

        public void onUIEnable()
        {
            throw new NotImplementedException();
        }

        public class MessageObject
        {

            public static GameObject MessageGraphic;

            private TextMeshProUGUI messageObject;

            public MessageObject(string username, string message, TMP_InputField input, ScrollRect scrollRect)
            {
                if (MessageGraphic == null) MessageGraphic = Resources.Load<GameObject>("GameResources/UI/InGameUI/MessageObject");

                messageObject = Instantiate(MessageGraphic, scrollRect.content).GetComponent<TextMeshProUGUI>();

                if (!username.Contains("<color=red>[Server]</color>"))
                {
                    messageObject.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        input.text = "/mp " + username;
                    });
                    messageObject.text = "<u>"+username + "</u>: " + message;
                }
                else
                {
                    messageObject.GetComponent<Button>().enabled = false;
                    messageObject.text = username + ": " + message;
                }

                
            }

            public TextMeshProUGUI getMessageObject()
            {
                return messageObject;
            }

  
        }
    }

    public class OthersContainer : IUserInterface
    {

        private TextMeshProUGUI TimeTxt;

        private InGameUI parent;

        private Button SocialBtn;

        public OthersContainer(InGameUI parent)
        {
            this.parent = parent;
        }

        public GameObject getInterfaceObject()
        {
            return null;
        }

        public void initializeComponents()
        {
            TimeTxt = parent.transform.GetChild(0).GetChild(3).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
            
            SocialBtn = parent.transform.GetChild(0).GetChild(3).GetChild(3).GetComponent<Button>();

            SocialBtn.onClick.AddListener(delegate 
            {
                IUserInterface userInterface = parent.getInterface(3);

                userInterface.getInterfaceObject().SetActive(true);

                userInterface.onUIEnable();
            });
        }

      
        public void onUIClose()
        {

        }

        public void onUIEnable()
        {
            throw new NotImplementedException();
        }

        public void updateUI()
        {
            if(Client.getClient().GetNetworkUser() != null)
                TimeTxt.text = TimeZoneInfo.ConvertTime(DateTime.Now, Main.INSTANCE.getTimezone()).ToString("HH:mm:ss");
        }

    }

    public class PlayerInterractionMenu
    {
        private GameObject menuObject;

        private Player thePlayer;

        private bool mouseOverMenu = false;

        public PlayerInterractionMenu(InGameUI parent, GameObject menuObject)
        {
            this.menuObject = menuObject;

            menuObject.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate
            {
                ChatContainer container = (ChatContainer)parent.getInterface(1);
                container.getInputField().text = "/mp " + thePlayer.getName();
            });

            Transform ActionBtnTransform = menuObject.transform.GetChild(1);
            Transform GamesBtnTransform = menuObject.transform.GetChild(2);

            ActionBtnTransform.GetComponent<Button>().onClick.AddListener(delegate
            {
                bool state = !ActionBtnTransform.GetChild(1).gameObject.activeInHierarchy;
                ActionBtnTransform.GetChild(1).gameObject.SetActive(state);
                ActionBtnTransform.GetChild(0).gameObject.SetActive(state);

                if(GamesBtnTransform.gameObject.activeInHierarchy)
                {
                    GamesBtnTransform.GetChild(1).gameObject.SetActive(false);
                    GamesBtnTransform.GetChild(0).gameObject.SetActive(false);
                }
            });

            GamesBtnTransform.GetComponent<Button>().onClick.AddListener(delegate
            {
                bool state = !GamesBtnTransform.GetChild(1).gameObject.activeInHierarchy;
                GamesBtnTransform.GetChild(1).gameObject.SetActive(state);
                GamesBtnTransform.GetChild(0).gameObject.SetActive(state);

                if (ActionBtnTransform.gameObject.activeInHierarchy)
                {
                    ActionBtnTransform.GetChild(1).gameObject.SetActive(false);
                    ActionBtnTransform.GetChild(0).gameObject.SetActive(false);
                }
            });
        }

        public RectTransform getRectTransform()
        {
            return (RectTransform) menuObject.transform;
        }

        public void setMenuOver(Player p)
        {
            menuObject.transform.position = Camera.main.WorldToScreenPoint(p.getEntityGameObject().transform.position) + new Vector3(300f,0f);
            setActive(true);
            thePlayer = p;
        }

        public void setActive(bool state)
        {
            menuObject.SetActive(state);
        }

        public bool isActive()
        {
            return menuObject.activeInHierarchy;
        }

        public bool isMouseOverMenu()
        {
            return mouseOverMenu;
        }

        public void setMouseOverMenu(bool state)
        {
            this.mouseOverMenu = state;
        }

        public void resetMenuDisplay()
        {
            setMouseOverMenu(false);

            Transform ActionBtnTransform = menuObject.transform.GetChild(1);
            Transform GamesBtnTransform = menuObject.transform.GetChild(2);

            if (GamesBtnTransform.gameObject.activeInHierarchy)
            {
                GamesBtnTransform.GetChild(1).gameObject.SetActive(false);
                GamesBtnTransform.GetChild(0).gameObject.SetActive(false);
            }

            if (ActionBtnTransform.gameObject.activeInHierarchy)
            {
                ActionBtnTransform.GetChild(1).gameObject.SetActive(false);
                ActionBtnTransform.GetChild(0).gameObject.SetActive(false);
            }
        }

    }

}
