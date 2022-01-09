using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class MainMenuUI : MonoBehaviour
{
    private static IUserInterface defaultUI;

    private IUserInterface[] childsUI;

    private static GameObject errorBubble;

    private static IUserInterface currentInterface;
    private static Button currentSelectedBtn;
    private static Button currentHoverBtn;

    public static Animation animationManager;

    private Image patternImage;
    public Vector2 fromPos;
    public Vector2 toPos;
    public float speed;

    public bool stopAnimation = false;

    private Button[] buttons = new Button[3];

    public Button getButton(int index)
    {
        return buttons[index];
    }

    public void setSelectedButton(Button button)
    {
        currentSelectedBtn = button;
        currentHoverBtn = button;
        button.GetComponentInChildren<Animation>().Play();
    }

    [System.Obsolete]
    void Awake()
    {
        Client.getClient().connectToServer();

        Transform navbar = transform.GetChild(1);

        buttons[0] = navbar.transform.GetChild(0).GetComponent<Button>();
        buttons[1] = navbar.transform.GetChild(1).GetComponent<Button>();
        buttons[2] = navbar.transform.GetChild(2).GetComponent<Button>();

        buttons[0].onClick.AddListener(delegate
        {
            if(Main.INSTANCE.getUserSession() == null)
            {
                currentSelectedBtn = buttons[0];
                displayInterface(1);
            }
            else
            {
                connectToGame();
            }
        });

        buttons[2].onClick.AddListener(delegate
        {
            displayProfilUI();
        });
            
        patternImage = transform.GetChild(0).GetChild(0).GetComponentInChildren<Image>();
        animationManager = GetComponent<Animation>();

        childsUI = new IUserInterface[] { new RegisterUI(this), new ConnectionUI(this), new ProfilUI(this), new AvatarEditionUI(this) };

        currentSelectedBtn = buttons[0];
        defaultUI = childsUI[1];
        

        foreach (IUserInterface ui in childsUI)
        {
            ui.initializeComponents();

            if(defaultUI != ui)
            {
                ui.getInterfaceObject().SetActive(false);
            }
            else
            {
                currentInterface = ui;
            }
        }

        defaultUI.getInterfaceObject().SetActive(true);
    }

    public IUserInterface getCurrentUserInterface()
    {
        return currentInterface;
    }


    public void displayProfilUI()
    {
        currentSelectedBtn = buttons[2];
        displayInterface(2);
    }

    void Update()
    {
        foreach (Button button in buttons)
        {
            if (currentSelectedBtn == button) continue;

            if (InterfaceUtils.mouseIsOnRect((RectTransform)button.transform,RenderMode.ScreenSpaceCamera))
            {
                if(currentHoverBtn != button)
                {
                    currentHoverBtn = button;
                    button.GetComponentInChildren<Animation>().Play();
                }
            }
            else 
            {
                if(currentHoverBtn == button)
                {
                    currentHoverBtn = null;
                }
                Animation animation = button.GetComponentInChildren<Animation>();
                if(animation.isPlaying)
                {
                    animation.Stop();
                }
                RectTransform btnTransform = (RectTransform)button.transform.GetChild(0).transform;
                btnTransform.sizeDelta = new Vector2(0, btnTransform.sizeDelta.y);
            }
        }

        if (Main.INSTANCE.getUserSession() == null)
        {
            buttons[1].gameObject.SetActive(false);
            buttons[2].gameObject.SetActive(false);
        }
        else
        {
            buttons[1].gameObject.SetActive(true);
            buttons[2].gameObject.SetActive(true);
        }

        foreach (IUserInterface ui in childsUI)
        {
            if(ui.getInterfaceObject().activeInHierarchy) ui.updateUI();
        }
    }

    public IUserInterface getInterface(int childIndex)
    {
        return childsUI[childIndex];
    }

    public void displayInterface(int childIndex)
    {

        if(currentInterface != null)
        {
            if (currentInterface == childsUI[childIndex]) return;

            currentInterface.onUIClose();
            currentInterface.getInterfaceObject().SetActive(false);
            animationManager.PlayQueued("Hide" + currentInterface.GetType().Name);
        }

        currentInterface = childsUI[childIndex];
        childsUI[childIndex].getInterfaceObject().SetActive(true);
        childsUI[childIndex].onUIEnable();
        animationManager.PlayQueued("Display" + currentInterface.GetType().Name);
    }

    public void setCheckbox(Toggle toggle, bool state)
    {
        if (!state)
        {
            toggle.transform.GetChild(0).gameObject.SetActive(false);
            toggle.transform.GetChild(1).gameObject.SetActive(true);
            //toggle.graphic = transform.GetChild(1).GetComponent<Image>();
        }
        else
        {
            toggle.transform.GetChild(0).gameObject.SetActive(true);
            toggle.transform.GetChild(1).gameObject.SetActive(false);
            //toggle.graphic = transform.GetChild(0).GetComponent<Image>();
        }
    }

    [System.Obsolete]
    public void connectToGame()
    {
        InGameUI.INSTANCE.gameObject.SetActive(true);
        NMSG_JoinGame packet = new NMSG_JoinGame();
        Client client = Client.getClient();
        client.sendToServer(packet, client.reliableChannel, client.gameChannel.getChannelId());
        gameObject.SetActive(false);
    }


    [System.Obsolete]
    void Start()
    {
        StartCoroutine(playPatternAnimation());
        Client client = Client.getClient();

        UserSession session = Main.INSTANCE.getUserSession();
       
        if (session != null)
        {
            ProgressBox progressBox = new ProgressBox("En attente d'une réponse du serveur!");
            NMSG_Authentification packet = NMSG_Authentification.connectAccount(session.getUsername(), session.getCryptedPassword());

            packet.setCallback(delegate
            {
                progressBox.destroyDialogBox();
                 if (packet.getResult() == 0)
                 {
                     displayProfilUI();
                 }
                 else
                 {
                     UserSession.destroySession();
                     InformationsBox box = new InformationsBox("Session expiré reconnectez-vous!", "Ok");
                     box.displayDialogBox(transform);
                 }
            });


            progressBox.displayDialogBox(transform);
            client.processCallbackPacket(packet);
        }
    }

    public void displayErrorMessage(Transform atTransform, string message)
    {
        if (errorBubble == null) errorBubble = Resources.Load<GameObject>("GameResources/UI/MainMenu/ErrorBubble");

        Transform errorImg = atTransform.FindChild("Error");

        if(errorImg != null && errorImg.gameObject.activeInHierarchy)
        {
            atTransform.FindChild("ErrorBubble(Clone)").GetComponentInChildren<TextMeshProUGUI>().SetText(message);
            return;
        }

        GameObject errorBubbleGO = Instantiate(errorBubble, atTransform);
        TextMeshProUGUI Text = errorBubbleGO.GetComponentInChildren<TextMeshProUGUI>();
        
        Text.SetText(message);

        errorBubbleGO.transform.localPosition = new Vector2(445f, -10f);


        if (errorImg != null)
        {
            errorImg.gameObject.SetActive(true);
        }
    }

    public void removeErrorMessageAt(Transform at)
    {
        Transform errorBubble = at.FindChild("ErrorBubble(Clone)");

        if (errorBubble != null)
        {

            Transform errorImg = at.FindChild("Error");
            if(errorImg != null) errorImg.gameObject.SetActive(false);
            Destroy(errorBubble.gameObject);
        }
    }

    private IEnumerator playPatternAnimation()
    {
        RectTransform rTransform = (RectTransform)patternImage.transform;
        rTransform.anchoredPosition = fromPos;
        while (true)
        {
            yield return new WaitForSeconds(0.01F);

            rTransform.anchoredPosition += (toPos - fromPos).normalized * speed;


            Vector2 distance = toPos - rTransform.anchoredPosition;

            if (distance.x + distance.y <= 0)
            {

                rTransform.anchoredPosition = fromPos;
            }
        }
    }

}

public class ConnectionUI : IUserInterface
{

    public Button ConnectionBtn;

    public Button SwitchRegisterBtn;

    private TMP_InputField UsernameInput;
    private TMP_InputField PasswordInput;

    private Toggle RememberToggle;

    private MainMenuUI parent;

    private DelayedAction<bool> checkConditionsAction;

    [System.Obsolete]
    public ConnectionUI(MainMenuUI parent)
    {
        this.parent = parent;
        checkConditionsAction = DelayedAction<bool>.delayedAction(1, checkConnectionConditions, true);
    }

    public void resetInterface()
    {
        UsernameInput.text = "";
        PasswordInput.text = "";
        parent.setCheckbox(RememberToggle, false);
        parent.removeErrorMessageAt(UsernameInput.transform);
        parent.removeErrorMessageAt(PasswordInput.transform);
        parent.removeErrorMessageAt(RememberToggle.transform);
    }

    [Obsolete]
    public void initializeComponents()
    {
        Transform transform = parent.transform.GetChild(2);

        ConnectionBtn = transform.GetChild(4).GetComponent<Button>();

        UsernameInput = transform.GetChild(1).GetComponent<TMP_InputField>();
        PasswordInput = transform.GetChild(2).GetComponent<TMP_InputField>();

        RememberToggle = transform.GetChild(3).GetComponent<Toggle>();
  
        RememberToggle.onValueChanged.AddListener(delegate {
            onCheckboxInterracted();
        });

        SwitchRegisterBtn = transform.GetChild(6).GetComponent<Button>();
        SwitchRegisterBtn.onClick.AddListener(delegate 
        {
            parent.displayInterface(0);
        });

        ConnectionBtn.onClick.AddListener(finalizeConnection);
    }

    private bool checkConnectionConditions()
    {
        bool error = false;
        if (UsernameInput.isFocused)
        {
            if (UsernameInput.text.Length == 0)
            {
                parent.displayErrorMessage(UsernameInput.transform, "Entrez votre pseudo");
                error = true;
            }
            else
            {
                parent.removeErrorMessageAt(UsernameInput.transform);
            }
        }
        else if (PasswordInput.isFocused)
        {
            if (PasswordInput.text.Length == 0)
            {
                parent.displayErrorMessage(PasswordInput.transform, "Entrez votre mots de passe");
                error = true;
            }
            else
            {
                parent.removeErrorMessageAt(PasswordInput.transform);
            }
        }

        return error;

    }

    public void onCheckboxInterracted()
    {
        Transform transform = RememberToggle.transform;
        transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeInHierarchy);
        transform.GetChild(1).gameObject.SetActive(!transform.GetChild(1).gameObject.activeInHierarchy);
    }

    [Obsolete]
    private void finalizeConnection()
    {
        
        if (!checkConnectionConditions()) 
        {
            Client client = Client.getClient();
            ProgressBox progressBox = new ProgressBox("Connexion en cours");
            progressBox.displayDialogBox(parent.transform);

           
            NMSG_Authentification packet = NMSG_Authentification.connectAccount(UsernameInput.text, EncryptionUtils.hashInput(PasswordInput.text));

            packet.setCallback(delegate
            {
                progressBox.destroyDialogBox();

                switch (packet.getResult())
                {
                    case 0:
                        {
                            UserSession session;
                            if (RememberToggle.isOn)
                            {
                                session = UserSession.createSession(UsernameInput.text, EncryptionUtils.hashInput(PasswordInput.text), true);
                            }
                            else
                            {
                                session = UserSession.createSession(UsernameInput.text, EncryptionUtils.hashInput(PasswordInput.text), false);
                            }
                            Main.INSTANCE.setUserSession(session);

                            parent.displayProfilUI();

                            break;
                        }
                    case 1:
                        {
                            InformationsBox inputBox = new InformationsBox("Identifiants incorrect!", "Ok");

                            inputBox.displayDialogBox(parent.transform);
                            break;
                        }
                    case 2:
                        {
                            InputBox inputBox = new InputBox("Vous devez valider votre compte\n entrez le code d'activation", "Ok");
                            inputBox.setConfirmBtnCallback(delegate
                            {
                                if(inputBox.getInputField().text.Length < 6)
                                {
                                    InformationsBox inputBox = new InformationsBox("Entrez un code à 6 chiffres!", "Ok");
                                    inputBox.displayDialogBox(parent.transform);
                                    return;
                                }

                                NMSG_Authentification activationPacket = NMSG_Authentification.activateAccount(UsernameInput.text,int.Parse(inputBox.getInputField().text));
                                activationPacket.setCallback(delegate
                                {
                                    switch(activationPacket.getResult())
                                    {
                                        case 3:
                                        {
                                            parent.connectToGame();
                                            break;
                                        }

                                        case 4:
                                        {
                                           InformationsBox inputBox = new InformationsBox("Code d'activation incorrect", "Ok");
                                           inputBox.displayDialogBox(parent.transform);
                                           break;
                                        }
                                    }
                                });

                                client.processCallbackPacket(activationPacket);
                            });
                            inputBox.displayDialogBox(parent.transform);
                            break;
                        }
                }
            });

            client.processCallbackPacket(packet);
        }
    }


    public void onUIClose()
    {
        checkConditionsAction.stopAction();
        resetInterface();

    }

    public void updateUI()
    {

    }

    public GameObject getInterfaceObject()
    {
        return parent.transform.GetChild(2).gameObject;
    }

    public void onUIEnable()
    {
        checkConditionsAction.restartAction();
    }
}

public class RegisterUI : IUserInterface
{

    public Button RegisterBtn;

    public Button SwitchConnectionBtn;

    private TMP_InputField UsernameInput;
    private TMP_InputField PasswordInput;
    private TMP_InputField PasswordConfirmationInput;
    private TMP_InputField EmailInput;

    private Toggle CGUToggle;

    private MainMenuUI parent;

    private DelayedAction<bool> checkConditionsAction;

    [System.Obsolete]
    public RegisterUI(MainMenuUI parent)
    {
        this.parent = parent;
        checkConditionsAction = DelayedAction<bool>.delayedAction(1, checkRegistrationConditions, true);
    }

    [Obsolete]
    public void initializeComponents()
    {
        Transform transform = parent.transform.GetChild(3);

        UsernameInput = transform.GetChild(1).GetComponent<TMP_InputField>();
        PasswordInput = transform.GetChild(2).GetComponent<TMP_InputField>();
        PasswordConfirmationInput = transform.GetChild(3).GetComponent<TMP_InputField>();
        EmailInput = transform.GetChild(4).GetComponent<TMP_InputField>();

        CGUToggle = transform.GetChild(5).GetComponent<Toggle>();

        RegisterBtn = transform.GetChild(6).GetComponent<Button>();

        RegisterBtn.onClick.AddListener(finalizeRegistration);

        CGUToggle.onValueChanged.AddListener(delegate {
            onCheckboxInterracted();
        });

        SwitchConnectionBtn = transform.GetChild(7).GetComponent<Button>();
        SwitchConnectionBtn.onClick.AddListener(delegate
        {
            parent.displayInterface(1);
        });
    }

    private bool checkRegistrationConditions()
    {
        bool error = false;
        if (UsernameInput.isFocused && UsernameInput.text.Length > 0)
        {
            if(Regex.IsMatch(UsernameInput.text, "[^a-zA-Z1-9_]"))
            {
                parent.displayErrorMessage(UsernameInput.transform, "Le pseudo peut seulement contenir [A-Za-z1-9_]");
                error = true;
            }
            else if (UsernameInput.text.Length < 3)
            {
                parent.displayErrorMessage(UsernameInput.transform, "Pseudo trop court");
                error = true;
            }
            else if (UsernameInput.text.Length > 16)
            {
                parent.displayErrorMessage(UsernameInput.transform, "Pseudo trop long");
                error = true;
            }
            else
            {
                parent.removeErrorMessageAt(UsernameInput.transform);
            }

        }
        else if(EmailInput.isFocused && EmailInput.text.Length > 0)
        {
            if (!Regex.IsMatch(EmailInput.text, "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$"))
            {
                parent.displayErrorMessage(EmailInput.transform, "Entrez un email valide");
                error = true;
            }
            else
            {
                parent.removeErrorMessageAt(EmailInput.transform);
            }
        }
        else if(PasswordInput.isFocused && PasswordInput.text.Length > 0)
        {
            if(PasswordInput.text.Length < 4)
            {
                parent.displayErrorMessage(PasswordInput.transform, "Mots de passe trop court, choisissez un mots de passe compris entre 4 et 45 caractères");
                error = true;
            }
            else if (PasswordInput.text.Length > 45)
            {
                parent.displayErrorMessage(PasswordInput.transform, "Mots de passe trop long, choisissez un mots de passe compris entre 4 et 45 caractères");
                error = true;
            }
            else
            {
                parent.removeErrorMessageAt(PasswordInput.transform);
            }
        }
        else if(PasswordConfirmationInput.isFocused && PasswordConfirmationInput.text.Length > 0)
        {
            if (PasswordInput.text != PasswordConfirmationInput.text)
            {
                parent.displayErrorMessage(PasswordConfirmationInput.transform, "Vos mots de passe ne sont pas identique");
                error = true;
            }
            else
            {
                parent.removeErrorMessageAt(PasswordConfirmationInput.transform);
            }
        }
        else if(!CGUToggle.isOn)
        {
            error = true;
        }
        else
        {
            parent.removeErrorMessageAt(CGUToggle.transform);
        }

        return error;

    }

    public void resetInterface()
    {
        EmailInput.text = "";
        PasswordInput.text = "";
        PasswordConfirmationInput.text = "";
        UsernameInput.text = "";
        parent.setCheckbox(CGUToggle, false);
        parent.removeErrorMessageAt(EmailInput.transform);
        parent.removeErrorMessageAt(PasswordInput.transform);
        parent.removeErrorMessageAt(UsernameInput.transform);
        parent.removeErrorMessageAt(PasswordConfirmationInput.transform);
        parent.removeErrorMessageAt(CGUToggle.transform);
    }

    public void onCheckboxInterracted()
    {
        Transform transform = CGUToggle.transform;
        transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeInHierarchy);
        transform.GetChild(1).gameObject.SetActive(!transform.GetChild(1).gameObject.activeInHierarchy);
    }

    private void finalizeRegistration()
    {
        if (!CGUToggle.isOn)
        {
            parent.displayErrorMessage(CGUToggle.transform, "Vous devez cocher le CGU");
        }

        if (!checkRegistrationConditions())
        {
            Client client = Client.getClient();

            ProgressBox progressBox = new ProgressBox("Inscription en cours");
            progressBox.displayDialogBox(parent.transform);

            NMSG_Authentification packet = NMSG_Authentification.registerAccount(UsernameInput.text, PasswordInput.text, EmailInput.text);

            packet.setCallback(delegate
            {
                progressBox.destroyDialogBox();

                if (packet.getResult() > 0)
                {
                    switch(packet.getResult())
                    {
                        case 2:
                        {
                                InputBox verificationBox = new InputBox("Vous devez valider votre compte\n entrez le code d'activation envoyé à l'adresse : " + EmailInput.text, "Ok");
                                verificationBox.setConfirmBtnCallback(delegate
                                {
                                    if (verificationBox.getInputField().text.Length < 6)
                                    {
                                        InformationsBox inputBox = new InformationsBox("Entrez un code à 6 chiffres!", "Ok");
                                        inputBox.displayDialogBox(parent.transform);
                                        return;
                                    }

                                    NMSG_Authentification activationPacket = NMSG_Authentification.activateAccount(UsernameInput.text, int.Parse(verificationBox.getInputField().text));
                                    activationPacket.setCallback(delegate
                                    {
                                        switch (activationPacket.getResult())
                                        {
                                            case 3:
                                                {
                                                    parent.connectToGame();
                                                    break;
                                                }

                                            case 4:
                                                {
                                                    InformationsBox inputBox = new InformationsBox("Code d'activation incorrect", "Ok");
                                                    inputBox.displayDialogBox(parent.transform);
                                                    break;
                                                }
                                        }
                                    });

                                    client.processCallbackPacket(activationPacket);
                                });
                                verificationBox.displayDialogBox(parent.transform);
                                break;
                        }
                        case 5:
                        {
                            parent.displayErrorMessage(EmailInput.transform, "Cette adresse mail existe déjà");
                            break;
                        }
                        case 6:
                        {
                            parent.displayErrorMessage(UsernameInput.transform, "Ce pseudo est déjà utilisé");
                            break;
                        }
                    }

                    if (packet.getResult() != 2)
                    {
                        InformationsBox inputBox = new InformationsBox("Echec de l'inscription, un ou plusieurs champs sont incorrects", "Ok");

                        inputBox.displayDialogBox(parent.transform);
                    }
                }
                else
                {
                    InformationsBox inputBox = new InformationsBox("Inscription réussi!", "Ok");
                }

            });

            client.processCallbackPacket(packet);
        }
    }

    public void onUIClose()
    {
        checkConditionsAction.stopAction();
        resetInterface();
    }

    public void updateUI()
    {

    }

    public GameObject getInterfaceObject()
    {
        return parent.transform.GetChild(3).gameObject;
    }

    public void onUIEnable()
    {
        checkConditionsAction.restartAction();
    }
}
