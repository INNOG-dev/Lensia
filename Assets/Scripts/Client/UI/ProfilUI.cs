using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfilUI : IUserInterface
{

    private MainMenuUI parent;

    private TextMeshProUGUI UsernameTxt;
    private TextMeshProUGUI LevelTxt;
    private TextMeshProUGUI TittleTxt;

    private TextMeshProUGUI EmailTxt;
    private TextMeshProUGUI BirthdayTxt;
    private TextMeshProUGUI GenderTxt;

    private TextMeshProUGUI PlayTimeTxt;
    private TextMeshProUGUI LenesieTxt;
    private TextMeshProUGUI GoldTxt;

    private Toggle ReceiveFriendRequestToggle;
    private Toggle NewsletterToggle;
    private Toggle AlwaysOfflineToggle;
    private Toggle MpActiveOnlyFriendToggle;

    private Button EditEmailBtn;
    private Button EditSkinBtn;

    private GameObject EditEmailObject;

    private GameObject EditPasswordObject;

    private Transform SkinBannerTransform;
    private SkinUI currentDisplayedSkin = null;



    public ProfilUI(MainMenuUI parent)
    {
        this.parent = parent;
    }


    public GameObject getInterfaceObject()
    {
        return parent.transform.GetChild(4).gameObject;
    }

    public void initializeComponents()
    {
        Transform profilTransform = getInterfaceObject().transform;

        EditPasswordObject = profilTransform.GetChild(1).GetChild(0).gameObject;
        EditPasswordObject.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(delegate
        {
            Client client = Client.getClient();
            NetworkUser user = client.GetNetworkUser();

            TMP_InputField PasswordInput = EditPasswordObject.transform.GetChild(0).GetComponent<TMP_InputField>();
            TMP_InputField NewPasswordInput = EditPasswordObject.transform.GetChild(1).GetComponent<TMP_InputField>();
            TMP_InputField ConfirmationPasswordInput = EditPasswordObject.transform.GetChild(2).GetComponent<TMP_InputField>();

            parent.removeErrorMessageAt(PasswordInput.transform);
            parent.removeErrorMessageAt(NewPasswordInput.transform);
            parent.removeErrorMessageAt(ConfirmationPasswordInput.transform);

            if (NewPasswordInput.text != ConfirmationPasswordInput.text)
            {
                parent.displayErrorMessage(ConfirmationPasswordInput.transform, "Mots de passe non identique");
                return;
            }

            ProgressBox progressBox = new ProgressBox("Modification en cours...");
            progressBox.displayDialogBox(parent.transform);
            NMSG_Authentification packet = NMSG_Authentification.editPassword(user.getAccountData().getUsername(), EncryptionUtils.hashInput(PasswordInput.text), EncryptionUtils.hashInput(NewPasswordInput.text));
            packet.setCallback(delegate
            {
                progressBox.destroyDialogBox();
              
                if (packet.getResult() == 9)
                {
                    parent.displayErrorMessage(PasswordInput.transform, "Mots de passe incorect");
                }
                else if(packet.getResult() == 8)
                {
                    resetInterface();
                    InformationsBox box = new InformationsBox("Mots de passe modifié avec succès!", "Ok");
                    box.displayDialogBox(parent.transform);
                }
                else if (packet.getResult() == 10)
                {
                    parent.displayErrorMessage(NewPasswordInput.transform, "Vous utilisez déjà ce mots de passe");
                }
                else
                {
                    InformationsBox box = new InformationsBox("Une erreur s'est produite", "Ok");
                    box.displayDialogBox(parent.transform);
                }
            });
            client.processCallbackPacket(packet);

        });


        EditEmailObject = profilTransform.GetChild(1).GetChild(1).gameObject;
        EditEmailObject.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate
        {
            Client client = Client.getClient();
            NetworkUser user = client.GetNetworkUser();

            TMP_InputField MailInput = EditEmailObject.transform.GetChild(0).GetComponent<TMP_InputField>();
            TMP_InputField newMailInput = EditEmailObject.transform.GetChild(1).GetComponent<TMP_InputField>();
         

            parent.removeErrorMessageAt(MailInput.transform);
            parent.removeErrorMessageAt(newMailInput.transform);

            ProgressBox progressBox = new ProgressBox("Modification en cours...");
            progressBox.displayDialogBox(parent.transform);

            NMSG_Authentification packet = NMSG_Authentification.preprocessMailEdition(user.getAccountData().getUsername(), newMailInput.text);
            packet.setCallback(delegate
            {
                progressBox.destroyDialogBox();
                if(packet.getResult() == 12)
                {
                    InputBox inputBox = new InputBox("Un code de vérification a été envoyé à \n" + user.getAccountData().getEmail(), "Valider");

                    inputBox.setConfirmBtnCallback(delegate
                    {
                        int verificationCode;

                        if(!int.TryParse(inputBox.getInputField().text, out verificationCode) || inputBox.getInputField().text.Length < 6)
                        {
                            InformationsBox box = new InformationsBox("Entrez un code de vérification avec 6 chiffres", "Ok");
                            box.displayDialogBox(parent.transform);
                            return;
                        }

                        packet = NMSG_Authentification.editMail(user.getAccountData().getUsername(), verificationCode);
                        packet.setCallback(delegate
                        {
                            if (packet.getResult() == 0)
                            {
                                inputBox.destroyDialogBox();
                                InformationsBox informationsBox = new InformationsBox("Adresse e-mail modifié avec succès", "Ok");
                                informationsBox.displayDialogBox(parent.transform);
                                user.getAccountData().setEmail(newMailInput.text);
                                EmailTxt.text = "Email : " + getDisplayForEmail(newMailInput.text);
                                MailInput.text = "";
                                newMailInput.text = "";
                                resetInterface();
                            }
                            else if (packet.getResult() == 4)
                            {
                                InformationsBox informationsBox = new InformationsBox("Code de vérification incorrect", "Ok");
                                informationsBox.displayDialogBox(parent.transform);
                            }
                            else
                            {
                                InformationsBox informationsBox = new InformationsBox("Une erreur s'est produite", "Ok");
                                informationsBox.displayDialogBox(parent.transform);
                            }

                        });

                        client.processCallbackPacket(packet);
                    });

                    inputBox.displayDialogBox(parent.transform);
                }
                else if(packet.getResult() == 5)
                {
                    parent.displayErrorMessage(newMailInput.transform, "Cette adresse e-mail existe déjà");
                }
                else if (packet.getResult() == 14)
                {
                    parent.displayErrorMessage(newMailInput.transform, "Cette adresse e-mail n'est pas valide");
                }
                else 
                {
                    InformationsBox informationsBox = new InformationsBox("Une erreur s'est produite", "Ok");
                    informationsBox.displayDialogBox(parent.transform);
                }
            });
            client.processCallbackPacket(packet);






        });

        EditPasswordObject.SetActive(false);
        EditEmailObject.SetActive(false);

        Transform scrollviewContentTransform = profilTransform.GetChild(2).GetChild(0).GetChild(0);
      
        Transform bannerTransform = scrollviewContentTransform.GetChild(0);

        SkinBannerTransform = bannerTransform.GetChild(0);

        EditSkinBtn = SkinBannerTransform.GetComponent<Button>();
        EditSkinBtn.onClick.AddListener(delegate
        {
            parent.displayInterface(3);
        });


        UsernameTxt = bannerTransform.GetChild(1).GetComponent<TextMeshProUGUI>();
        LevelTxt = bannerTransform.GetChild(2).GetComponent<TextMeshProUGUI>();
        TittleTxt = bannerTransform.GetChild(3).GetComponent<TextMeshProUGUI>();

        Transform accountInformationsTransform = scrollviewContentTransform.GetChild(1);
        EmailTxt = accountInformationsTransform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
        BirthdayTxt = accountInformationsTransform.GetChild(3).GetComponent<TextMeshProUGUI>();
        GenderTxt = accountInformationsTransform.GetChild(4).GetComponent<TextMeshProUGUI>();
        EditEmailBtn = accountInformationsTransform.GetChild(2).GetChild(1).GetComponent<Button>();
        Button PasswordEditBtn = accountInformationsTransform.GetChild(1).GetChild(0).GetChild(1).GetComponent<Button>();

        PasswordEditBtn.onClick.AddListener(delegate
        {
            resetInterface();
            EditPasswordObject.SetActive(true);
        });

        Button EmailEditBtn = accountInformationsTransform.GetChild(2).GetChild(1).GetComponent<Button>();

        EmailEditBtn.onClick.AddListener(delegate
        {
            resetInterface();
            EditEmailObject.SetActive(true);
        });

        Transform statsTransform = scrollviewContentTransform.GetChild(2);
        PlayTimeTxt = statsTransform.GetChild(1).GetComponent<TextMeshProUGUI>();
        LenesieTxt = statsTransform.GetChild(2).GetComponent<TextMeshProUGUI>();
        GoldTxt = statsTransform.GetChild(3).GetComponent<TextMeshProUGUI>();

        Transform confidentialityTransform = scrollviewContentTransform.GetChild(3);
        ReceiveFriendRequestToggle = confidentialityTransform.GetChild(1).GetChild(0).GetComponent<Toggle>();
        NewsletterToggle = confidentialityTransform.GetChild(2).GetChild(0).GetComponent<Toggle>();
        AlwaysOfflineToggle = confidentialityTransform.GetChild(3).GetChild(0).GetComponent<Toggle>();
        MpActiveOnlyFriendToggle = confidentialityTransform.GetChild(4).GetChild(0).GetComponent<Toggle>();

        Client client = Client.getClient();
        ReceiveFriendRequestToggle.onValueChanged.AddListener(delegate
        {
            parent.setCheckbox(ReceiveFriendRequestToggle, ReceiveFriendRequestToggle.isOn);

            client.sendToServer(NMSG_Profil.syncConfidentiality(0, ReceiveFriendRequestToggle.isOn), client.reliableChannel, client.gameChannel.getChannelId());
        });

        NewsletterToggle.onValueChanged.AddListener(delegate
        {
            parent.setCheckbox(NewsletterToggle, NewsletterToggle.isOn);

            client.sendToServer(NMSG_Profil.syncConfidentiality(1, NewsletterToggle.isOn), client.reliableChannel, client.gameChannel.getChannelId());
        });

        AlwaysOfflineToggle.onValueChanged.AddListener(delegate
        {
            parent.setCheckbox(AlwaysOfflineToggle, AlwaysOfflineToggle.isOn);

            client.sendToServer(NMSG_Profil.syncConfidentiality(2, AlwaysOfflineToggle.isOn), client.reliableChannel, client.gameChannel.getChannelId());
        });

        MpActiveOnlyFriendToggle.onValueChanged.AddListener(delegate
        {
            parent.setCheckbox(MpActiveOnlyFriendToggle, MpActiveOnlyFriendToggle.isOn);

            client.sendToServer(NMSG_Profil.syncConfidentiality(3, MpActiveOnlyFriendToggle.isOn), client.reliableChannel, client.gameChannel.getChannelId());
        });

       

        confidentialityTransform.GetChild(5).GetComponent<Button>().onClick.AddListener(delegate
        {
            ChoiceBox choiceBox = new ChoiceBox("Êtes vous sur de supprimer votre compte ? \n Vous perdrez toutes votre progression", "Oui", "Non");

            Client client = Client.getClient();
            NetworkUser user = client.GetNetworkUser();

            choiceBox.setNoBtnCallback(delegate { choiceBox.destroyDialogBox(); });

            choiceBox.setYesBtnCallback(delegate 
            { 
                choiceBox.destroyDialogBox();
                ProgressBox box = new ProgressBox("En attente du serveur...");
                box.displayDialogBox(parent.transform);
                NMSG_Authentification packet = NMSG_Authentification.processAccountSupression(user.getAccountData().getUsername());

                packet.setCallback(delegate
                {
                    box.destroyDialogBox();
                    if(packet.getResult() == 12)
                    {
                        InputBox inputBox = new InputBox("Un code de vérification à 6 chiffre a été envoyé à \n " + EmailTxt.text, "Valider");

                        inputBox.setConfirmBtnCallback(delegate
                        {
                            int verificationCode;

                            if (!int.TryParse(inputBox.getInputField().text, out verificationCode) || inputBox.getInputField().text.Length < 6)
                            {
                                InformationsBox box = new InformationsBox("Entrez un code de vérification avec 6 chiffres", "Ok");
                                box.displayDialogBox(parent.transform);
                                return;
                            }

                            ProgressBox progressBox = new ProgressBox("Supression du compte en cours...");
                            progressBox.displayDialogBox(parent.transform);

                            packet = NMSG_Authentification.DeleteAccount(user.getAccountData().getUsername(), verificationCode);

                            packet.setCallback(delegate
                            {
                                progressBox.destroyDialogBox();
                                if(packet.getResult() == 0)
                                {
                                    inputBox.destroyDialogBox();
                                    Main.INSTANCE.disconnectAccount();
                                }
                                else if(packet.getResult() == 4)
                                {
                                    InformationsBox box = new InformationsBox("Code de vérification incorrect", "Ok");
                                    box.displayDialogBox(parent.transform);
                                }
                                else
                                {
                                    InformationsBox box = new InformationsBox("Une erreur s'est produite.", "Ok");
                                    box.displayDialogBox(parent.transform);
                                }
                            });

                            client.processCallbackPacket(packet);
                        });

                        inputBox.displayDialogBox(parent.transform);

                    }
                    else
                    {
                        InformationsBox box = new InformationsBox("Une erreur s'est produite.", "Ok");
                        box.displayDialogBox(parent.transform);
                    }
                });

                client.processCallbackPacket(packet);

            });

            choiceBox.displayDialogBox(parent.transform);
        });
    }

    public void resetInterface()
    {
        EditEmailObject.SetActive(false);
        EditPasswordObject.SetActive(false);
    }

    public void onUIClose()
    {
        resetInterface();
        currentDisplayedSkin.destroySkinDisplay();
    }

    public void onUIEnable()
    {
        parent.setSelectedButton(parent.getButton(2));
        ProgressBox progressBox = new ProgressBox("Chargement des données...");
        progressBox.displayDialogBox(parent.transform);
        Client client = Client.getClient();

        NMSG_SyncAccountData packet = new NMSG_SyncAccountData();

        packet.callback = delegate
        {
            Client client = Client.getClient();

            NetworkUser networkUser = client.GetNetworkUser();

            if (networkUser != null)
            {
                UsernameTxt.text = networkUser.getAccountData().getUsername();
                EmailTxt.text = "Email : " + getDisplayForEmail(networkUser.getAccountData().getEmail());

                BirthdayTxt.text = "Date de naissance : " + networkUser.getAccountData().getBirthday();
                GenderTxt.text = "Genre : " + getDisplayForGender(networkUser.getAccountData().getGender());
                PlayTimeTxt.text = "Temps de jeu : " + getDisplayForPlayTime(networkUser.getAccountData().getPlayingTimeInSeconds()) + "";
                LenesieTxt.text = "Lenesie : " + getLargeNumberDisplay(networkUser.getAccountData().getLenesie()) + "";
                GoldTxt.text = "Or : " + getLargeNumberDisplay(networkUser.getAccountData().getGold()) + "";

                currentDisplayedSkin = new SkinUI(networkUser.getAccountData().getEquippedSkinId());

                currentDisplayedSkin.displaySkin(SkinBannerTransform, true);

                GameObject skinObject = currentDisplayedSkin.getSkinObject();

                skinObject.transform.localScale = new Vector3(12f, 12f, skinObject.transform.localScale.z);
                skinObject.transform.localPosition = new Vector3(0f, -10f);

                currentDisplayedSkin.getSkinComposition().loadPartColor(currentDisplayedSkin.getSkin().getId());

                parent.setCheckbox(ReceiveFriendRequestToggle, networkUser.getConfidentialityData().getAcceptFriendRequests());
                parent.setCheckbox(NewsletterToggle, networkUser.getConfidentialityData().isRegisterNewsletter());
                parent.setCheckbox(AlwaysOfflineToggle, networkUser.getConfidentialityData().isAlwaysOffline());
                parent.setCheckbox(MpActiveOnlyFriendToggle, networkUser.getConfidentialityData().onlyFriendCanMp());
            }

            progressBox.destroyDialogBox();
        };
        client.processCallbackPacket(packet);
    }

    public void updateUI()
    {
        RectTransform editBtnRectT = (RectTransform)EditEmailBtn.transform;
        editBtnRectT.anchoredPosition = new Vector2(EmailTxt.rectTransform.rect.xMax + 20, -14.4445f);

        RectTransform EditSkinBtnRectT = (RectTransform)EditSkinBtn.transform.GetChild(0);

        if (InterfaceUtils.mouseIsOnRect(EditSkinBtnRectT, RenderMode.ScreenSpaceCamera))
        {
            EditSkinBtnRectT.gameObject.SetActive(true);
        }
        else
        {
            EditSkinBtnRectT.gameObject.SetActive(false);
        }
                
    }

    public string getDisplayForEmail(string email)
    {
        if (email.Length == 0) return "";

        string[] data = email.Split('@');
        string display = data[0][0] + "*****" + data[0][data[0].Length-1] + "@" + data[1];
        return display;
    }

    public string getDisplayForGender(byte gender)
    {
        if(gender == 0)
        {
            return "<color=" + AccountData.getColorFromGender(gender) + ">Masculin</color>";
        }
        else if (gender == 1)
        {
            return "<color=" + AccountData.getColorFromGender(gender) + ">Feminin</color>";
        }
        return "<color=" + AccountData.getColorFromGender(gender) +">Autre</color>";
    }

    public string getDisplayForPlayTime(ulong playedTimeInSeconds)
    {
        return (playedTimeInSeconds / 3600f).ToString("0.00") + " h(s)";
    }

    public string getLargeNumberDisplay(uint number)
    {
        if (number >= 1000000000)
        {
            float numberFloat = (float)number / 1000000000F;
            return "" + numberFloat + "Mi";
        }
        if (number >= 1000000)
        {
            float numberFloat = (float)number / 1000000;
            return "" + numberFloat + "M";
        }
        if (number >= 1000)
        {
            float numberFloat = (float)number / 1000;
            return "" + numberFloat + "K";
        }
        else
        {
            return "" + number;
        }
    }
}
