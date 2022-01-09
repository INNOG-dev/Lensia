using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FriendsUI : IUserInterface
{

    private InGameUI parent;

    public static GameObject FriendObject;

    public static GameObject FriendRequestSenderObject;

    public static GameObject FriendRequestReceiverObject;

    public static GameObject BlacklistObject;

    public static GameObject AddFriendObject;



    private Button AddFriendBtn;

    private Button BlackListBtn;

    private TMP_InputField SearchInput;

    private Button CloseBtn;

    private ScrollRect ScrollView;

    private SocialContainer container = null;

    public SocialSlot selectedPlayer;

    private string previousSearch = "";

    private List<SocialSlot> slots = new List<SocialSlot>();
    private List<SocialSlot> visibleSlots = new List<SocialSlot>();

    public FriendsUI(InGameUI parent)
    {
        this.parent = parent;

        container = (SocialContainer)RegistryManager.containerRegistry.get(1).getContainer(Client.getClient().GetNetworkUser());

        if (FriendObject == null) FriendObject = Resources.Load<GameObject>("GameResources/UI/InGameUI/FriendsUI/FriendSlot");

        if (FriendRequestSenderObject == null) FriendRequestSenderObject = Resources.Load<GameObject>("GameResources/UI/InGameUI/FriendsUI/FriendRequestSender");

        if (FriendRequestReceiverObject == null) FriendRequestReceiverObject = Resources.Load<GameObject>("GameResources/UI/InGameUI/FriendsUI/FriendRequestReceiver");

        if (BlacklistObject == null) BlacklistObject = Resources.Load<GameObject>("GameResources/UI/InGameUI/FriendsUI/BlacklistSlot");

        if (AddFriendObject == null) AddFriendObject = Resources.Load<GameObject>("GameResources/UI/InGameUI/FriendsUI/AddFriendSlot");
    }

    public GameObject getInterfaceObject()
    {
        return parent.transform.GetChild(4).gameObject;
    }

    public List<SocialSlot> applyFilter(string searchInput)
    {
        List<SocialSlot> visibleSlots = new List<SocialSlot>();

        slots.ForEach(x => visibleSlots.Add(x));

        if (searchInput != null && searchInput.Length > 0)
        {
            List<SocialSlot> toRemove = new List<SocialSlot>();

            visibleSlots.ForEach(x =>
            {
                if (!x.getUsername().ToLower().Contains(searchInput))
                {
                    toRemove.Add(x);
                }
            });

            visibleSlots.RemoveAll(x => toRemove.Contains(x));
        }

        return visibleSlots;
    }

    public void initializeComponents()
    {
        Transform thisUI = getInterfaceObject().transform;

        SearchInput = thisUI.GetChild(4).GetComponent<TMP_InputField>();

        SearchInput.onValueChanged.AddListener(delegate
        {
            string searchText = SearchInput.text.ToLower();

            if (previousSearch != searchText)
            {
                previousSearch = searchText;

                if (container.getContainerType() == 0)
                {
                    visibleSlots.ForEach(x => x.setActive(false));

                    visibleSlots = applyFilter(previousSearch);

                    visibleSlots.ForEach(x => x.setActive(true));
                }
                else if(container.getContainerType() == 1)
                {
                    List<SocialSlot> toRemove = new List<SocialSlot>();

                    visibleSlots.ForEach(x =>
                    {
                        if (x.getRequestType() == 3)
                        {
                            toRemove.Add(x);
                        }
                    });

                    toRemove.ForEach(x =>
                    {
                        visibleSlots.Remove(x);
                        slots.Remove(x);
                        x.destroy();
                    });

                    NMSG_Social packet = new NMSG_Social(searchText);

                    Client client = Client.getClient();
                    client.sendToServer(packet, client.reliableChannel, client.gameChannel.getChannelId());

                    container.askElement();

                }
            }
        });

        AddFriendBtn = thisUI.GetChild(0).GetComponent<Button>();

        AddFriendBtn.onClick.AddListener(delegate
        {
            resetUI();

        
            if (container.getContainerType() == 0)
            {
                AddFriendBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Retour";
                container.setContainerType(1);

                TextMeshProUGUI SlotText = ScrollView.transform.GetChild(0).GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
                
                SearchInput.text = "Rechercher des amis...";

                SlotText.text = "Résultat vide";
            }
            else
            {
                AddFriendBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Ajouter des amis";
                container.setContainerType(0);

                TextMeshProUGUI SlotText = ScrollView.transform.GetChild(0).GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();

                SlotText.text = "Vous n'avez pas encore d'amis";
            }

        });

        BlackListBtn = thisUI.GetChild(1).GetComponent<Button>();

        BlackListBtn.onClick.AddListener(delegate
        {
            resetUI();


            if (container.getContainerType() == 0)
            {
                BlackListBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Retour";
                container.setContainerType(2);

                TextMeshProUGUI SlotText = ScrollView.transform.GetChild(0).GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();

                SlotText.text = "Aucun joueur n'est bloqué";
            }
            else
            {
                BlackListBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Blacklist";
                container.setContainerType(0);

                TextMeshProUGUI SlotText = ScrollView.transform.GetChild(0).GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();

                SlotText.text = "Vous n'avez pas encore d'amis";
            }
        });

        CloseBtn = thisUI.GetChild(2).GetComponent<Button>();

        ScrollView = thisUI.transform.GetChild(3).GetComponent<ScrollRect>();

        CloseBtn.onClick.AddListener(delegate
        {
            thisUI.gameObject.SetActive(false);
            onUIClose();
        });

    }

    public void onUIClose()
    {
        Client.getClient().GetNetworkUser().closeContainer(container.getContainerId());

        resetUI();
    }

    public void onUIEnable()
    {
        Client.getClient().GetNetworkUser().openContainer(container);
    }

    public void updateUI()
    {
        if(!container.getAllElementsAreSynchronized()) container.askElement();

        if (container != null)
        {
            if (container.hasNewValue())
            {

                GameObject emptyFriendGameObject = ScrollView.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;

                if (emptyFriendGameObject.activeInHierarchy)
                {
                    emptyFriendGameObject.SetActive(false);
                }

                List<ContainerSlot> containerSlots = container.getElements();

                foreach (ContainerSlot containerSlot in containerSlots)
                {
                    SocialSlot slot = (SocialSlot)containerSlot;

                    slot.instantiate(container.getContainerType(),ScrollView.transform.GetChild(0).GetChild(0));
                    slot.initializeComponent(this);

                    slots.Add(slot);
                    visibleSlots.Add(slot);
                }

                container.clearElements();
            }
        }

        if(selectedPlayer != null)
        {
            if(Input.GetMouseButtonDown(0))
            {
                if(!InterfaceUtils.mouseIsOnRect((RectTransform)selectedPlayer.getScrollingMenuTransform(),RenderMode.ScreenSpaceOverlay) && !InterfaceUtils.mouseIsOnRect((RectTransform)selectedPlayer.getSlotTransform(), RenderMode.ScreenSpaceOverlay))
                {
                    selectedPlayer.displayScrollingMenu(this, false);
                }
            }
        }
    }

    private void resetUI()
    {
        TextMeshProUGUI SlotText = ScrollView.transform.GetChild(0).GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();

        SlotText.text = "Vous n'avez pas encore d'amis";
        SearchInput.text = "Rechercher...";

        foreach (SocialSlot slot in slots)
        {
            slot.destroy();
        }

        displayEmptySlot();

        slots.Clear();
        visibleSlots.Clear();

        BlackListBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Blacklist";
        AddFriendBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Ajouter des amis";
    }

    public SocialSlot getSlot(int index)
    {
        return slots[index];
    }

    public void removeSlot(SocialSlot slot)
    {
        slots.Remove(slot);
        visibleSlots.Remove(slot);
    }

    public bool isEmpty()
    {
        return slots.Count == 0;
    }

    public void displayEmptySlot()
    {
        GameObject emptySlot = ScrollView.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;

        emptySlot.SetActive(true);
    }

    public SocialContainer getContainer()
    {
        return container;
    }

}

public class SocialSlot : ContainerSlot
{

    private uint slotIndex;

    private bool isConnected;

    private string username;

    /*
     * 1: sender friend request
     * 2: receiver friend request
     */
    private byte requestType = 0;

    private uint mapId;

    private Image ConnectionStatusImg;

    private Transform SlotTransform;

    private GameObject scrollingMenu;
    private Button[] buttons = new Button[3];

    private static Sprite OnlineSprite;

    private static Sprite OfflineSprite;

    private ulong userUID;

    public SocialSlot()
    {

    }

    public SocialSlot(uint slotIndex, ulong uid)
    {
        this.slotIndex = slotIndex;
        this.userUID = uid;

    }

    public override bool Equals(object obj)
    {
        if(obj is SocialSlot)
        {
            SocialSlot slot = (SocialSlot)obj;
            return slot.userUID == userUID && username == slot.username;
        }
        return false;
    }



    public void instantiate(byte type, Transform at)
    {
        if (type == 0)
        {
            SlotTransform = Object.Instantiate<GameObject>(FriendsUI.FriendObject, at).transform;
        }
        else if(type == 1)
        {
            if(requestType == 1)
            {
                SlotTransform = Object.Instantiate<GameObject>(FriendsUI.FriendRequestSenderObject, at).transform;
            }
            else if(requestType == 2)
            {
                SlotTransform = Object.Instantiate<GameObject>(FriendsUI.FriendRequestReceiverObject, at).transform;
            }
            else if(requestType == 3)
            {
                SlotTransform = Object.Instantiate<GameObject>(FriendsUI.AddFriendObject, at).transform;
            }
        }
        else if (type == 2)
        {
            SlotTransform = Object.Instantiate<GameObject>(FriendsUI.BlacklistObject, at).transform;
        }
    }

    public override void ReadFromStream(BinaryReader reader)
    {
        slotIndex = reader.ReadUInt32();
        isConnected = reader.ReadBoolean();
        username = reader.ReadString();

        if (isConnected)
        {
            mapId = reader.ReadUInt32();
        }

        requestType = reader.ReadByte();
    }

    public override void WriteToStream(BinaryWriter writer)
    {
        writer.Write(slotIndex);
        writer.Write(isConnected);
        writer.Write(username);

        if (isConnected)
        {
            writer.Write(mapId);
        }

        writer.Write(requestType);
        
    }

    public string getUsername()
    {
        return username;
    }

    public void setUsername(string username)
    {
        this.username = username;
    }

    public void setIsConnected(bool isConnected)
    {
        this.isConnected = isConnected;
    }

    public bool getIsConnected()
    {
        return isConnected;
    }

    public ulong getUserUID()
    {
        return userUID;
    }

    public void setMapId(uint mapId)
    {
        this.mapId = mapId;
    }

    public void setRequestType(byte type)
    {
        this.requestType = type;
    }

    public byte getRequestType()
    {
        return requestType;
    }

    public bool isFriendRequest()
    {
        return requestType > 0;
    }

    public Transform getSlotTransform()
    {
        return SlotTransform;
    }

    public Transform getScrollingMenuTransform()
    {
        return scrollingMenu.transform;
    }

    public void displayScrollingMenu(FriendsUI parent, bool state)
    {
        if(parent.selectedPlayer != null && parent.selectedPlayer != this)
        {
            parent.selectedPlayer.displayScrollingMenu(parent, false);
        }

        scrollingMenu.SetActive(state);

        if (state) parent.selectedPlayer = this;
        else parent.selectedPlayer = null;
    }

    public void setActive(bool state)
    {
        getSlotTransform().gameObject.SetActive(state);
    }

    public void initializeComponent(FriendsUI parent)
    {
        Client client = Client.getClient();

        if(parent.getContainer().getContainerType() == 0)
        {
            SlotTransform.GetComponent<Button>().onClick.AddListener(delegate
            {
                displayScrollingMenu(parent, !scrollingMenu.activeInHierarchy);
            });

            Image StatusImg = SlotTransform.GetChild(0).GetComponent<Image>();

            if (OnlineSprite == null) OnlineSprite = StatusImg.sprite;

            if (OfflineSprite == null) OfflineSprite = Resources.Load<Sprite>("Textures/Gui/InGameMenu/FriendsUI/offline");

            if (!isConnected)
            {
                StatusImg.sprite = OfflineSprite;
            }
            else
            {
                SlotTransform.SetAsFirstSibling();
            }

            scrollingMenu = SlotTransform.GetChild(2).gameObject;

            int i = 0;
            foreach (Button button in scrollingMenu.GetComponentsInChildren<Button>())
            {
                buttons[i] = button;
                i++;
            }

            buttons[2].onClick.AddListener(delegate
            {
                NMSG_Social social = new NMSG_Social(1, slotIndex);

                ProgressBox box = new ProgressBox("Supression de l'amis en cours...");
                box.displayDialogBox(InGameUI.INSTANCE.transform);

                social.setCallback(delegate
                {
                    box.destroyDialogBox();
                    if (social.getResult() == 1)
                    {
                        parent.selectedPlayer = null;
                        destroy();
                        parent.removeSlot(this);

                        if (parent.isEmpty())
                        {
                            parent.displayEmptySlot();
                        }
                    }
                    else
                    {
                        InformationsBox box = new InformationsBox("L'amis n'a pas pu être supprimé une erreur s'est produite", "Ok");
                        box.displayDialogBox(InGameUI.INSTANCE.transform);
                    }
                });


                client.processCallbackPacket(social);
            });

            buttons[1].onClick.AddListener(delegate
            {
                NMSG_Social social = new NMSG_Social(4, slotIndex);

                ProgressBox box = new ProgressBox("Blacklist de l'amis en cours...");
                box.displayDialogBox(InGameUI.INSTANCE.transform);

                social.setCallback(delegate
                {
                    box.destroyDialogBox();
                    if (social.getResult() == 1)
                    {
                        parent.selectedPlayer = null;
                        destroy();
                        parent.removeSlot(this);

                        if (parent.isEmpty())
                        {
                            parent.displayEmptySlot();
                        }
                    }
                    else
                    {
                        InformationsBox box = new InformationsBox("L'amis n'a pas pu être bloqué une erreur s'est produite", "Ok");
                        box.displayDialogBox(InGameUI.INSTANCE.transform);
                    }
                });


                client.processCallbackPacket(social);
            });

            buttons[0].onClick.AddListener(delegate
            {
                Player thePlayer = client.GetNetworkUser().getPlayer();

                if(isConnected)
                {
                    thePlayer.sendMessage("");
                    thePlayer.sendMessage("------INFORMATIONS------");
                    thePlayer.sendMessage("Pseudo: " + username);
                    thePlayer.sendMessage("Map: " + RegistryManager.mapRegistry.get(mapId).getRegistryName());
                    thePlayer.sendMessage("----------------------------");
                }
                else
                {
                    thePlayer.sendMessage("<color=red>Ce joueur n'est pas connecté</color>");
                }
            });

            SlotTransform.GetChild(1).GetComponent<TextMeshProUGUI>().text = username;
        }
        else if(parent.getContainer().getContainerType() == 1)
        {
            if (requestType == 1)
            {
                SlotTransform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate
                {
                    NMSG_Social social = new NMSG_Social(1, slotIndex);

                    ProgressBox box = new ProgressBox("Suppresion de la requête d'amis...");
                    box.displayDialogBox(InGameUI.INSTANCE.transform);

                    social.setCallback(delegate
                    {
                        box.destroyDialogBox();
                        if (social.getResult() == 1)
                        {
                            parent.selectedPlayer = null;
                            destroy();
                            parent.removeSlot(this);

                            if (parent.isEmpty())
                            {
                                parent.displayEmptySlot();
                            }
                        }
                        else
                        {
                            InformationsBox box = new InformationsBox("La requête n'a pas plus être annulé une erreur s'est produite", "Ok");
                            box.displayDialogBox(InGameUI.INSTANCE.transform);
                        }
                    });


                    client.processCallbackPacket(social);
                });


                SlotTransform.GetChild(2).GetComponent<TextMeshProUGUI>().text = username;
            }
            else if (requestType == 2)
            {
                SlotTransform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate
                {
                    NMSG_Social social = new NMSG_Social(2, slotIndex);

                    ProgressBox box = new ProgressBox("Acceptation de la requête...");
                    box.displayDialogBox(InGameUI.INSTANCE.transform);

                    social.setCallback(delegate
                    {
                        box.destroyDialogBox();
                        if (social.getResult() == 1)
                        {
                            parent.selectedPlayer = null;
                            destroy();
                            parent.removeSlot(this);

                            if (parent.isEmpty())
                            {
                                parent.displayEmptySlot();
                            }
                        }
                        else
                        {
                            InformationsBox box = new InformationsBox("La requête n'a pas plus être accepté une erreur s'est produite", "Ok");
                            box.displayDialogBox(InGameUI.INSTANCE.transform);
                        }
                    });


                    client.processCallbackPacket(social);
                });

                SlotTransform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate
                {
                    NMSG_Social social = new NMSG_Social(3, slotIndex);

                    ProgressBox box = new ProgressBox("Suppresion de la requête d'amis...");
                    box.displayDialogBox(InGameUI.INSTANCE.transform);

                    social.setCallback(delegate
                    {
                        box.destroyDialogBox();
                        if (social.getResult() == 1)
                        {
                            parent.selectedPlayer = null;
                            destroy();
                            parent.removeSlot(this);

                            if (parent.isEmpty())
                            {
                                parent.displayEmptySlot();
                            }
                        }
                        else
                        {
                            InformationsBox box = new InformationsBox("La requête n'a pas plus être supprimé une erreur s'est produite", "Ok");
                            box.displayDialogBox(InGameUI.INSTANCE.transform);
                        }
                    });


                    client.processCallbackPacket(social);
                });


                SlotTransform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Demande d'amis de " + username;
            }
            else if(requestType == 3)
            {
                SlotTransform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate
                {
                    NMSG_Social social = new NMSG_Social(0, slotIndex);

                    ProgressBox box = new ProgressBox("Envoie de la requête d'amis");
                    box.displayDialogBox(InGameUI.INSTANCE.transform);

                    social.setCallback(delegate
                    {
                        box.destroyDialogBox();
                        if (social.getResult() == 0)
                        {
                            parent.selectedPlayer = null;
                            destroy();
                            parent.removeSlot(this);

                            if (parent.isEmpty())
                            {
                                parent.displayEmptySlot();
                            }
                        }
                        else
                        {
                            InformationsBox box = new InformationsBox("La requête n'a pas pu atteindre son destinataire merci de réessayer", "Ok");
                            box.displayDialogBox(InGameUI.INSTANCE.transform);
                        }
                    });


                    client.processCallbackPacket(social);
                });


                SlotTransform.GetChild(2).GetComponent<TextMeshProUGUI>().text = username;
            }
        }
        else if (parent.getContainer().getContainerType() == 2)
        {
            SlotTransform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate
            {
                NMSG_Social social = new NMSG_Social(5, slotIndex);

                ProgressBox box = new ProgressBox("Suppresion de la blacklist en cours...");
                box.displayDialogBox(InGameUI.INSTANCE.transform);

                social.setCallback(delegate
                {
                    box.destroyDialogBox();
                    if (social.getResult() == 1)
                    {
                        parent.selectedPlayer = null;
                        destroy();
                        parent.removeSlot(this);

                        if (parent.isEmpty())
                        {
                            parent.displayEmptySlot();
                        }
                    }
                    else
                    {
                        InformationsBox box = new InformationsBox("Le joueur n'a pas pu être débloqué une erreur s'est produite", "Ok");
                        box.displayDialogBox(InGameUI.INSTANCE.transform);
                    }
                });


                client.processCallbackPacket(social);
            });

            SlotTransform.GetChild(1).GetComponent<TextMeshProUGUI>().text = username;
        }
    }

    public void destroy()
    {
        Object.Destroy(SlotTransform.gameObject);
    }
}
