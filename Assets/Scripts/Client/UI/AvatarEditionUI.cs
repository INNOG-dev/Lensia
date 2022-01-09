using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using System.IO;

public class AvatarEditionUI : IUserInterface
{

    private static GameObject SkinSlotObject;
    private static GameObject ColorBox;

    private MainMenuUI parent;

    private TMP_Dropdown AnimationDropdown;
    private TMP_Dropdown FilterDropdown;

    private TMP_Dropdown CurrentExpandedDropdown;

    private TextMeshProUGUI SelectSkinText;
    private TextMeshProUGUI SelectedSkinNameText;

    private TMP_InputField SearchInputField;

    private Transform ScrollViewContentTransform;
    private Transform SelectedAvatarTransform;
    private Transform ColorBoxsTransform;

    private List<SkinSlot> slots = new List<SkinSlot>();
    private List<SkinSlot> visibleSlots = new List<SkinSlot>();
    private List<ColorBox> colorBoxes = new List<ColorBox>();

    private AvatarEditionContainer container = null;

    private SkinSlot selectedSlot = null;
    private SkinUI currentDisplayedSkin = null;
    

    private FlexibleColorPicker ColorPicker;
    private SkinPart selectedSkinPart;
    private Image selectedColorBox;
    private Button ResetSkinBtn;
    private Button EquipSkinBtn;
    private static readonly Vector3 defaultColorPickerPos = new Vector3(-796f, 419f, 0f);

    private Transform[] skinColorBoxsTransform = new Transform[3];

    private GameObject draggingMoveableObject;


    private byte previousFilter;
    private string previousSearch;

    

    public AvatarEditionUI(MainMenuUI parent)
    {
        this.parent = parent;

        container = (AvatarEditionContainer) RegistryManager.containerRegistry.get(0).getContainer(Client.getClient().GetNetworkUser());

        if (SkinSlotObject == null)
        {
            SkinSlotObject = Resources.Load<GameObject>("GameResources/UI/MainMenu/AvatarEditionUI/SkinSlot");
        }

        if(ColorBox == null)
        {
            ColorBox = Resources.Load<GameObject>("GameResources/UI/MainMenu/AvatarEditionUI/ColorBox");
        }
    }

    public GameObject getInterfaceObject()
    {
        return parent.transform.GetChild(5).gameObject;
    }

    public void selectSkin(SkinSlot slot)
    {
        if (getSelectedSlot() != null)
        {
            getSelectedSlot().setSprite(SkinSlot.unSelectedSprite);
            currentDisplayedSkin.getSkinComposition().savePartColor(currentDisplayedSkin.getSkin().getId());
            resetSkinDisplay();
        }

        foreach (Transform transform in skinColorBoxsTransform)
        {
            transform.parent.parent.parent.gameObject.SetActive(true);
        }

        SelectSkinText.transform.parent.gameObject.SetActive(false);

        SelectedSkinNameText.enabled = true;
        SelectedSkinNameText.text = "Skin " + slot.getSkinUI().getSkin().getRegistryName();


        slot.setSprite(SkinSlot.selectedSprite);

        setSelectedSlot(slot);

        ResetSkinBtn.gameObject.SetActive(true);
        EquipSkinBtn.gameObject.SetActive(true);

        currentDisplayedSkin = new SkinUI(slot.getSkinUI().getSkin().getId());
        currentDisplayedSkin.displaySkin(SelectedAvatarTransform,false);


        buildSkin(currentDisplayedSkin.getSkinComposition());


        Animator animator = currentDisplayedSkin.getSkinObject().GetComponent<Animator>();

        RuntimeAnimatorController ac = animator.runtimeAnimatorController;

        animator.SetLayerWeight(1, 0f);
        animator.SetLayerWeight(3, 1f);

        AnimationDropdown.ClearOptions();
        List<string> options = new List<string>();

        options.Add("Animation sélectionnée");
        options.Add("SwimAnimation");
        options.Add("WalkAnimation");
        options.Add("ClimbAnimation");
        options.Add("SitAnimation");
        options.Add("IdleAnimation");

        AnimationDropdown.AddOptions(options);

        currentDisplayedSkin.getSkinObject().name = "SelectedSkin";
        currentDisplayedSkin.getSkinObject().transform.localPosition = new Vector2(43f, 10f);
        currentDisplayedSkin.getSkinObject().transform.localScale = new Vector2(20f, 20f);
    }

    public void initializeComponents()
    {
        Transform AvatarEditionTransform = getInterfaceObject().transform;

        SearchInputField = AvatarEditionTransform.GetChild(0).GetComponent<TMP_InputField>();
        SearchInputField.onValueChanged.AddListener(delegate
        {
            string searchText = SearchInputField.text.ToLower();

            if(previousSearch != searchText)
            {
                previousSearch = searchText;

                visibleSlots.ForEach(x => x.setActive(false));

                visibleSlots = applyFilter(previousSearch, previousFilter);

                visibleSlots.ForEach(x => x.setActive(true));

            }
        });

        FilterDropdown = AvatarEditionTransform.GetChild(3).GetComponent<TMP_Dropdown>();

        FilterDropdown.onValueChanged.AddListener(delegate
        {
            if (previousFilter != FilterDropdown.value)
            {
                previousFilter = (byte)FilterDropdown.value;


                visibleSlots.ForEach(x => x.setActive(false));

                visibleSlots = applyFilter(previousSearch, previousFilter);


                visibleSlots.ForEach(x => x.setActive(true));
            }
        });

        SelectedAvatarTransform = AvatarEditionTransform.GetChild(2);

        SelectSkinText = SelectedAvatarTransform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();

        SelectedSkinNameText = SelectedAvatarTransform.GetChild(3).GetComponent<TextMeshProUGUI>();
        SelectedSkinNameText.enabled = false;

        AnimationDropdown = SelectedAvatarTransform.GetComponentInChildren<TMP_Dropdown>();

        ColorBoxsTransform = SelectedAvatarTransform.GetChild(4);

        ColorPicker = ColorBoxsTransform.GetChild(0).GetComponent<FlexibleColorPicker>();
        ResetSkinBtn = SelectedAvatarTransform.GetChild(5).GetComponent<Button>();
        EquipSkinBtn = SelectedAvatarTransform.GetChild(6).GetComponent<Button>();

        ResetSkinBtn.onClick.AddListener(delegate
        {
            foreach(ColorBox colorBox in colorBoxes)
            {
                colorBox.getPart().resetColor();
                colorBox.getColorBox().color = colorBox.getPart().getSpriteRenderer().color;
            }
        });


        EquipSkinBtn.onClick.AddListener(delegate
        {
            uint previousEquippedSkinId = Client.getClient().GetNetworkUser().getAccountData().getEquippedSkinId();
            uint equippedSkinId = selectedSlot.getSkinUI().getSkin().getId();
            List<SkinPart> parts = selectedSlot.getSkinUI().getSkinComposition().getAllSpriteRenderer();

            Color[] equippedSkinColors = new Color[parts.Count];
            for(int i = 0; i < equippedSkinColors.Length; ++i)
            {
                equippedSkinColors[i] = parts[i].getSpriteRenderer().color;
            }

            if(previousEquippedSkinId == equippedSkinId)
            {
                DialogBox dialogBox = new InformationsBox("Skin déjà équipé", "Ok");
                dialogBox.displayDialogBox(parent.transform);
                return;
            }

            NMSG_Profil packet = NMSG_Profil.selectSkin(container.getContainerId(), equippedSkinColors,  equippedSkinId);

            packet.callback = delegate
            {
                if(packet.getResult() == 0)
                {
                    slots.ForEach(x =>
                    {
                       

                        if (x.getSkinUI().getSkin().getId() == previousEquippedSkinId)
                        {
                            x.setEquipped(false);
                        }
                        else if (x.getSkinUI().getSkin().getId() == equippedSkinId)
                        {
                            Client.getClient().GetNetworkUser().getAccountData().setEquippedSkinId(x.getSkinUI().getSkin().getId());
                            x.setEquipped(true);
                        }
                    });
                }
                else
                {
                    DialogBox dialogBox = new InformationsBox("Une erreur s'est produite", "Ok");
                    dialogBox.displayDialogBox(parent.transform);
                }
            };

            Client.getClient().processCallbackPacket(packet);

           
        });

        for (int i = 1; i < 4; i++)
        {
            skinColorBoxsTransform[i - 1] = ColorBoxsTransform.GetChild(i).GetChild(1).GetChild(0).GetChild(0);
            skinColorBoxsTransform[i - 1].parent.parent.parent.gameObject.SetActive(false);
        }

        ColorPicker.addListener(delegate
        {
            if(selectedSkinPart != null)
            {
                Color color = ColorPicker.color;

                selectedSkinPart.getSpriteRenderer().color = color;

                foreach(SkinPart duplicatePart in selectedSkinPart.getDuplicateParts())
                {
                    if (duplicatePart.hasSprite()) duplicatePart.getSpriteRenderer().color = color;
                }

                selectedColorBox.color = color;
            }
        });

        AnimationDropdown.onValueChanged.AddListener(delegate
        {

            if (AnimationDropdown.value == 0) return;

            if(currentDisplayedSkin != null)
            {
                Animator animator = currentDisplayedSkin.getSkinObject().GetComponent<Animator>();
                animator.enabled = true;

                animator.Play(AnimationDropdown.options[AnimationDropdown.value].text, 3);
            }


        });

        ScrollViewContentTransform = AvatarEditionTransform.GetChild(1).GetChild(0).GetChild(0);
    }

    public List<SkinSlot> applyFilter(string searchInput, byte filter)
    {
        List<SkinSlot> visibleSlots = new List<SkinSlot>();

        switch (filter)
        {
            case 0:
                {
                    slots.ForEach(x => visibleSlots.Add(x));
                    break;
                }
            case 1:
                {
                    slots.ForEach(x =>
                    {
                        if (x.getIsFavorite())
                        {
                            visibleSlots.Add(x);
                        }
                    });
                    break;
                }
            case 2:
                {
                    slots.ForEach(x =>
                    {
                        if (!x.getIsFavorite())
                        {
                            visibleSlots.Add(x);
                        }
                    });
                    break;
                }
            default:
                {
                    break;
                }
        }

        if (searchInput != null && searchInput.Length > 0)
        {
            List<SkinSlot> toRemove = new List<SkinSlot>();
            visibleSlots.ForEach(x =>
            {
                if (!x.getSkinUI().getSkin().getRegistryName().Contains(searchInput))
                {
                    toRemove.Add(x);
                }
            });

            visibleSlots.RemoveAll(x => toRemove.Contains(x));
        }

        return visibleSlots;
    }

    public void instantiateSkinSlot(uint skinId)
    {
        GameObject slotGO = Object.Instantiate(SkinSlotObject, ScrollViewContentTransform);

        SkinUI skinUI = new SkinUI(skinId);

        SkinSlot slot = new SkinSlot(this,slotGO.transform, skinUI);

        slots.Add(slot);

        visibleSlots.Add(slot);


        skinUI.displaySkin(slotGO.transform.GetChild(1).GetChild(0), true);
    }

    private void resetUI()
    {
        foreach(SkinSlot slot in slots)
        {
            slot.destroy();
        }
        slots.Clear();

        if (currentDisplayedSkin != null) currentDisplayedSkin.getSkinComposition().savePartColor(getSelectedSlot().getSkinUI().getSkin().getId());

        FilterDropdown.value = 0;
        SearchInputField.text = "";
        previousFilter = 0;
        previousSearch = "";
        selectedSlot = null;
        selectedColorBox = null;
        selectedSkinPart = null;

        resetSkinDisplay();
    }

    private void resetSkinDisplay()
    {
        if (currentDisplayedSkin != null)
        {
            currentDisplayedSkin.destroySkinDisplay();

            foreach(Transform transform in skinColorBoxsTransform)
            {
                foreach(Transform colorboxs in transform)
                {
                    Object.Destroy(colorboxs.gameObject);
                }
                transform.parent.parent.GetComponent<ScrollRect>().horizontalScrollbar.value = 0;
                transform.parent.parent.parent.gameObject.SetActive(false);
            }
            colorBoxes.Clear();
            SelectedSkinNameText.text = "";
            SelectSkinText.transform.parent.gameObject.SetActive(true);
            AnimationDropdown.ClearOptions();
        }
    }

    public void onUIClose()
    { 
        resetUI();

        PlayerPrefs.Save();

        Client.getClient().GetNetworkUser().closeContainer(container.getContainerId());
    }

    public void onUIEnable()
    {
        Client.getClient().GetNetworkUser().openContainer(container);

        container.askElement();
    }

    public void updateUI()
    {
        onDropdownInterract(AnimationDropdown);
        onDropdownInterract(FilterDropdown);
        resetDropdownInterraction();

        if (ColorPicker.isActiveAndEnabled && Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(draggingMoveableObject == null)
            {
                if(EventSystem.current.IsPointerOverGameObject())
                {
                    GraphicRaycaster raycaster = ColorPicker.transform.parent.GetComponent<GraphicRaycaster>();

                    PointerEventData pointerData = new PointerEventData(EventSystem.current)
                    {
                        position = Input.mousePosition
                    };

                    List<RaycastResult> results = new List<RaycastResult>();

                    raycaster.Raycast(pointerData, results);

                    bool canMove = true;
                    foreach(RaycastResult result in results)
                    {
                        if(result.gameObject.tag != "MoveableObject")
                        {
                            canMove = false;
                        }
                        else
                        {
                            draggingMoveableObject = result.gameObject;
                        }
                    }

                    if(!canMove)
                    {
                        draggingMoveableObject = null;
                    }
                }
            }

            if (!InterfaceUtils.mouseIsOnRect((RectTransform)ColorPicker.transform, RenderMode.ScreenSpaceCamera))
            {
                ColorPicker.gameObject.SetActive(false);
                selectedColorBox = null;
                selectedSkinPart = null;
            }
        }
        
        if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            draggingMoveableObject = null;
        }

        if(draggingMoveableObject != null)
        {
            Vector2 localPoint;

            RectTransform rt = (RectTransform) ColorBoxsTransform.transform;

            Debug.Log("test");

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, Input.mousePosition, Camera.main, out localPoint);
     
            draggingMoveableObject.transform.localPosition = new Vector2(localPoint.x, localPoint.y);

            if (draggingMoveableObject.transform.localPosition.x < -1388.426f)
            {
                draggingMoveableObject.transform.localPosition = new Vector3(-1388.426f, draggingMoveableObject.transform.localPosition.y, 0f);
            }
            if (draggingMoveableObject.transform.localPosition.x > 39)
            {
                draggingMoveableObject.transform.localPosition = new Vector3(39, draggingMoveableObject.transform.localPosition.y, 0f);
            }
            if (draggingMoveableObject.transform.localPosition.y  > 710f)
            {
                draggingMoveableObject.transform.localPosition = new Vector3(draggingMoveableObject.transform.localPosition.x, 710f, 0f);
            }
            if (draggingMoveableObject.transform.localPosition.y < 313)
            {
                draggingMoveableObject.transform.localPosition = new Vector3(draggingMoveableObject.transform.localPosition.x, 313, 0f);
            }
        }

        if(container != null)
        {
            if(container.hasNewValue())
            {
                List<ContainerSlot> containerSlots = container.getElements();

                foreach (ContainerSlot containerSlot in containerSlots)
                {
                    SkinSlot slot = (SkinSlot)containerSlot;
                    instantiateSkinSlot(slot.getSkinUI().getSkin().getId());
                }

                container.clearElements();
            }
        }
    }

    private void onDropdownInterract(TMP_Dropdown dropdown)
    {
        if (dropdown.IsExpanded && CurrentExpandedDropdown != dropdown)
        {
            resetDropdownInterraction(); 
            CurrentExpandedDropdown = dropdown;
            Transform dropdownTransform = dropdown.transform;
            dropdownTransform.GetChild(1).localScale = new Vector3(dropdownTransform.localScale.x, -dropdownTransform.localScale.y, dropdownTransform.localScale.z);
        }
    }

    private void resetDropdownInterraction()
    {
        if(CurrentExpandedDropdown != null && !CurrentExpandedDropdown.IsExpanded)
        {
            Transform dropdownTransform = CurrentExpandedDropdown.transform;

            dropdownTransform.GetChild(1).localScale = new Vector3(dropdownTransform.localScale.x, dropdownTransform.localScale.y >= 0 ? dropdownTransform.localScale.y : -dropdownTransform.localScale.y, dropdownTransform.localScale.z);

            CurrentExpandedDropdown = null;
        }
    }

    public void setSelectedSlot(SkinSlot slot)
    {
        selectedSlot = slot;
    }

    public SkinSlot getSelectedSlot()
    {
        return selectedSlot;
    }

    public void buildSkin(SkinComposition composition)
    {
        composition.loadPartColor(selectedSlot.getSkinUI().getSkin().getId());

        foreach(SkinPart part in composition.getAllSpriteRenderer())
        {
            if (composition.partIsDuplicated(part)) continue;

            if (!part.isColoreable()) continue;

            GameObject colorBox = Object.Instantiate(ColorBox, skinColorBoxsTransform[(int)part.getBodyType()]);
            Image colorBoxImage = colorBox.GetComponent<Image>();
            colorBoxImage.color = part.getSpriteRenderer().color;
            Vector3 spriteLocalPos = part.getSpriteRenderer().transform.position;

            Vector3 spriteRelativePos = part.getSpriteRenderer().transform.InverseTransformPoint(colorBox.transform.position);

            colorBox.transform.localPosition = new Vector3(spriteLocalPos.x, spriteLocalPos.y, 0);

            colorBoxes.Add(new ColorBox(part, colorBoxImage));

            colorBox.GetComponent<Button>().onClick.AddListener(delegate
            {
                selectedSkinPart = part;
                selectedColorBox = colorBoxImage;

                ColorPicker.gameObject.SetActive(true);
                ColorPicker.transform.localPosition = defaultColorPickerPos;
                ColorPicker.color = selectedColorBox.color;
            });
        }

        ColorPicker.transform.SetAsLastSibling();
    }

}

public class SkinSlot : ContainerSlot
{

    public static Sprite selectedSprite;

    public static Sprite unSelectedSprite;

    private SkinUI skinUI;

    private Button slotBtn;

    private Button favoriteBtn;

    private Transform slotTransform;

    private Animation animation;

    private bool isFavorite;

    private GameObject EquippedContainer;

    public SkinSlot()
    {

    }

    public SkinSlot(AvatarEditionUI parent, Transform slotTransform, SkinUI skinUI)
    {
        this.slotTransform = slotTransform;

        this.skinUI = skinUI;

        isFavorite = PlayerPrefs.GetInt("skin-" + skinUI.getSkin().getId() + "-isFavorite") == 1 ? true : false;

        if(selectedSprite == null)
        {
            selectedSprite = Resources.Load<Sprite>("Textures/Gui/AvatarEditionUI/slot_selected");
            unSelectedSprite = slotTransform.GetComponent<Image>().sprite;
        }

        initializeComponent(parent);
    }

    public SkinUI getSkinUI()
    {
        return skinUI;
    }

    public void initializeComponent(AvatarEditionUI parent)
    {
        slotBtn = slotTransform.GetComponent<Button>();

        slotBtn.onClick.AddListener(delegate
        {
            parent.selectSkin(this);
        });

        animation = slotBtn.GetComponent<Animation>();

        favoriteBtn = slotTransform.GetChild(0).GetComponent<Button>();

        EquippedContainer = slotTransform.GetChild(2).gameObject;

        if(Client.getClient().GetNetworkUser().getAccountData().getEquippedSkinId() != skinUI.getSkin().getId())
        {
            EquippedContainer.SetActive(false);
        }

        RectTransform starRect = (RectTransform)favoriteBtn.transform.GetChild(1);

        if (isFavorite)
        {
            starRect.sizeDelta = new Vector2(53, starRect.sizeDelta.y);
            favoriteBtn.transform.GetChild(0).gameObject.SetActive(false);
        }

        favoriteBtn.onClick.AddListener(delegate
        {
            string path = "skin-" + getSkinUI().getSkin().getId() + "-isFavorite";
            if (!isFavorite)
            {
                animation.PlayQueued("Favorite");
                isFavorite = true;
                PlayerPrefs.SetInt(path, 1);
            }
            else
            {
                if (animation.isPlaying) animation.Stop();
                favoriteBtn.transform.GetChild(0).gameObject.SetActive(true);
                starRect.sizeDelta = new Vector2(0, starRect.sizeDelta.y);
                isFavorite = false;
                PlayerPrefs.SetInt(path, 0);
            }
        });
    }

    public void setSkin(uint id)
    {
        skinUI = new SkinUI(id);
    }

    public bool isEquipped()
    {
        return EquippedContainer.activeInHierarchy;
    }

    public void setEquipped(bool state)
    {
        EquippedContainer.SetActive(state);
    }

    public void setActive(bool state)
    {
        slotTransform.gameObject.SetActive(state);
    }

    public bool isActive()
    {
        return slotTransform.gameObject.activeInHierarchy;
    }

    public void setSprite(Sprite sprite)
    {
        slotTransform.GetComponent<Image>().sprite = sprite;
    }

    public bool getIsFavorite()
    {
        return isFavorite;
    }

    public void destroy()
    {
        Object.Destroy(slotTransform.gameObject);
    }

    public override void WriteToStream(BinaryWriter writer)
    {
        writer.Write(skinUI.getSkin().getId());
    }

    public override void ReadFromStream(BinaryReader reader)
    {
        skinUI = new SkinUI(reader.ReadUInt32());
    }

}
