using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InterfaceUtils 
{
    
    public static bool mouseIsOnRect(RectTransform rectTransform, RenderMode renderMode)
    {
        if(renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition);
        }
        else if(renderMode == RenderMode.ScreenSpaceCamera)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Camera.main);
        }
        return false;
    }

}
