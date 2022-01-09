using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedAction<TReturn>
{
    private Func<TReturn> callback;

    private int delay;

    private bool repeat;

    private Coroutine actionCoroutine;


    private DelayedAction() 
    {
        
    }


    private IEnumerator startAction()
    {
        while(true)
        {
            yield return new WaitForSeconds(delay);
            callback();

            if (!repeat) yield break;
        }
    }

    public void stopAction()
    {
        Main.INSTANCE.StopCoroutine(actionCoroutine);
    }

    public void restartAction()
    {
        Main.INSTANCE.StartCoroutine(startAction()); 
    }

    //<param name="delay">Delay in seconds</param>
    //<param name="action">callback function</param>
    //<param name="repeat">should repeat action</param>
    public static DelayedAction<TReturn> delayedAction(int delay, Func<TReturn> action, bool repeat)
    {
        DelayedAction<TReturn> delayedAction = new DelayedAction<TReturn>();
        delayedAction.delay = delay;
        delayedAction.callback = action;
        delayedAction.repeat = repeat;

        delayedAction.actionCoroutine = Main.INSTANCE.StartCoroutine(delayedAction.startAction());

        return delayedAction;
    }
     


}
