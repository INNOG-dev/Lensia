using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InformationsBox : DialogBox
{
    private string displayedText;
    private string confirmBtnName;

    public InformationsBox(string displayedText, string confirmBtnName)
    {
        this.displayedText = displayedText;
        this.confirmBtnName = confirmBtnName;
    }

    public override void initializeDialogBox()
    {
        Transform parentTransform = getDialogBoxParentTransform();

        GameObject go = Object.Instantiate(Resources.Load<GameObject>(getResourcePath() + "InformationsBox"), parentTransform);

        go.GetComponentInChildren<TextMeshProUGUI>().text = displayedText;
        Button button = go.GetComponentInChildren<Button>();
        button.onClick.AddListener(destroyDialogBox);
        button.GetComponentInChildren<TextMeshProUGUI>().text = confirmBtnName;
    }

}
