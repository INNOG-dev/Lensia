using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceBox : DialogBox
{

    private System.Action yesAction;
    private System.Action noAction;

    private string yesBtnName;

    private string noBtnName;

    private string displayedText;

    public ChoiceBox(string displayedText, string yesBtnName, string noBtnName)
    {
        this.displayedText = displayedText;
        this.yesBtnName = yesBtnName;
        this.noBtnName = noBtnName;
    }

    public void setYesBtnCallback(System.Action callback)
    {
        this.yesAction = callback;
    }

    public void setNoBtnCallback(System.Action callback)
    {
        this.noAction = callback;
    }


    public override void initializeDialogBox()
    {
        Transform parent = getDialogBoxParentTransform();
        GameObject go = Object.Instantiate(Resources.Load<GameObject>(getResourcePath() + "ChoiceBox"), parent);
        Button[] buttons = go.GetComponentsInChildren<Button>();

        Button yesBtn = buttons[0];
        Button noBtn = buttons[1];

        displayedText = go.GetComponentInChildren<TextMeshProUGUI>().text = displayedText;

        yesBtn.onClick.AddListener(delegate
        {
            yesAction();
        });

        noBtn.onClick.AddListener(delegate
        {
            noAction();
        });

        yesBtn.GetComponentInChildren<TextMeshProUGUI>().text = yesBtnName;

        noBtn.GetComponentInChildren<TextMeshProUGUI>().text = noBtnName;
    }

}
