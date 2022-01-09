using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioClip : RegistryObject
{
    private UnityEngine.AudioClip audio;

    public AudioClip(string clipName)
    {
        this.setRegistryName(clipName);
    }

    public override void onRegistered()
    {
        base.onRegistered();

        audio = Resources.Load < UnityEngine.AudioClip> ("GameResources/Skins/Fx/" + getRegistryName());
    }

    public UnityEngine.AudioClip getClip()
    {
        return audio;
    }

}
