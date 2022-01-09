using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InputBox : DialogBox
{
    private TMP_InputField input;

    private System.Action confirmAction;

    private string confirmBtnName;

    private string displayedText;

    public InputBox(string displayedText,string confirmBtnName)
    {
        this.confirmBtnName = confirmBtnName;
        this.displayedText = displayedText;
    }

    public void setConfirmBtnCallback(System.Action callback)
    {
        this.confirmAction = callback;
    }

    public override void initializeDialogBox()
    {
        Transform parent = getDialogBoxParentTransform();
        GameObject go = Object.Instantiate(Resources.Load<GameObject>(getResourcePath() + "InputBox"), parent);

        Button button = go.GetComponentInChildren<Button>();

        displayedText = go.GetComponentInChildren<TextMeshProUGUI>().text = displayedText;

        button.onClick.AddListener(delegate 
        { 
            confirmAction(); 
        });

        button.GetComponentInChildren<TextMeshProUGUI>().text = confirmBtnName;

        input = go.GetComponentInChildren<TMP_InputField>();
    }

    public TMP_InputField getInputField()
    {
        return input;
    }
}
