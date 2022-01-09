using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{

    public void playSound(AudioSource source, uint soundId)
    {
        source.clip = RegistryManager.audioRegistry.get(soundId).getClip();
        source.Play();
    }

    public void playSoundOneshot(AudioSource source, uint soundId)
    {
        source.PlayOneShot(RegistryManager.audioRegistry.get(soundId).getClip());
    }

}
