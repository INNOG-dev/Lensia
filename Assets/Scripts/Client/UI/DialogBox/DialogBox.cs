using UnityEngine;
using UnityEngine.UI;

public abstract class DialogBox 
{
    private static GameObject dialogBoxObject;

    protected GameObject dialogBoxInstance;


    public virtual GameObject displayDialogBox(Transform at)
    {
        if (dialogBoxObject == null) dialogBoxObject = Resources.Load<GameObject>("GameResources/UI/DialogBox/DialogBox");

        dialogBoxInstance = Object.Instantiate(dialogBoxObject, at);
        dialogBoxInstance.tag = "DialogBox";

        dialogBoxInstance.GetComponentInChildren<Button>().onClick.AddListener(destroyDialogBox);

        initializeDialogBox();

        return dialogBoxInstance;
    }

    public virtual Transform getDialogBoxParentTransform()
    {
        return dialogBoxInstance.transform.GetChild(4);
    }

    public string getResourcePath()
    {
        return "GameResources/UI/DialogBox/Types/";
    }

    public virtual void destroyDialogBox()
    {
        Object.Destroy(dialogBoxInstance);
    }

    public abstract void initializeDialogBox();



}
