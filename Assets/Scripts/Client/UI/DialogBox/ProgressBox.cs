using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProgressBox : DialogBox
{
    private string displayedText;

    public ProgressBox(string displayedText)
    {
        this.displayedText = displayedText;
    }

    public override void initializeDialogBox()
    {
        Transform parentTransform = getDialogBoxParentTransform();

        GameObject go = Object.Instantiate(Resources.Load<GameObject>(getResourcePath() + "ProgressBox"), parentTransform);
        go.GetComponentInChildren<TextMeshProUGUI>().text = displayedText;
    }

}
