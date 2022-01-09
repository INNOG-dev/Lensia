using UnityEngine;
using TMPro;
using System;

public class OverlayUI : MonoBehaviour
{
    private TextMeshProUGUI FrameDisplayerTxt;
    private TextMeshProUGUI PingDisplayerTxt;

    private string fpsFormatDisplay;
    private string pingFormatDisplay;

    private DelayedAction<bool> gameStatsAction;


    void Awake()
    {
        FrameDisplayerTxt = transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        PingDisplayerTxt = transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();

        fpsFormatDisplay = FrameDisplayerTxt.text;
        pingFormatDisplay = PingDisplayerTxt.text;

        FrameDisplayerTxt.text = fpsFormatDisplay.Replace("%value%","" + getFps());
        PingDisplayerTxt.text = fpsFormatDisplay.Replace("%value%", "" + Client.getClient().getPing());

        gameStatsAction = DelayedAction<bool>.delayedAction(1, delegate ()
        {
            FrameDisplayerTxt.text = fpsFormatDisplay.Replace("%value%", "" + getFps());
            PingDisplayerTxt.text = pingFormatDisplay.Replace("%value%", "" + Client.getClient().getPing());
            return true;
        }, true);

    }

    public float getFps()
    {
        return (int)(1f / Time.unscaledDeltaTime);
    }

}
