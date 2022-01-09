using UnityEngine;
using UnityEngine.EventSystems;

public class Main : MonoBehaviour
{

    public static Main INSTANCE;

    private static bool gameInitialized = false;

    private readonly static ChatManager chatManager = new ChatManager();
    private readonly static AudioManager audioManager = new AudioManager();

    private System.TimeZoneInfo timezone;

    public readonly static short referenceWidth = 1920;
    public readonly static short referenceHeight = 1024;

    private UserSession userSession;

    void Awake()
    {
        INSTANCE = this;

        userSession = UserSession.getSession();

        RegistryManager.register();

        gameInitialized = true;
    }

    public bool gameIsInitialized()
    {
        return gameInitialized;
    }

    public ChatManager getChatManager()
    {
        return chatManager;
    }

    public AudioManager getAudioManager()
    {
        return audioManager;
    }

    public bool isFocusedInGame()
    {
        return EventSystem.current.currentSelectedGameObject == null;
    }

    public System.TimeZoneInfo getTimezone()
    {
        return timezone;
    }

    public void setTimezoneId(string timezoneId)
    {
        timezone = System.TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
    }

    public MainMenuUI getMainMenuUI()
    {
       return GameObject.Find("MainMenuUI").GetComponent<MainMenuUI>();
    }

    public InGameUI getInGameUI()
    {
        return GameObject.Find("InGameUI").GetComponent<InGameUI>();
    }

    public Transform getCurrentInterfaceParent()
    {
        return GameObject.FindGameObjectWithTag("UserInterface").transform;
    }

    public void clearAllDialogBoxs()
    {
        GameObject[] dialogBoxs = GameObject.FindGameObjectsWithTag("DialogBox");

        foreach (GameObject dialogBox in dialogBoxs)
        {
            Destroy(dialogBox);
        }
    }

    public void setUserSession(UserSession session)
    {
        this.userSession = session;
    }

    public void disconnectAccount()
    {
        UserSession.destroySession();
        Main.INSTANCE.setUserSession(null);
        if(!Main.INSTANCE.getMainMenuUI().gameObject.activeInHierarchy) Main.INSTANCE.getMainMenuUI().gameObject.SetActive(true);

        Main.INSTANCE.getMainMenuUI().displayInterface(0);

        InformationsBox box = new InformationsBox("Vous venez d'être déconnecté merci de vous reconnectez ", "Ok");
        box.displayDialogBox(getCurrentInterfaceParent());
    }

    public UserSession getUserSession()
    {
        return userSession;
    }







}
