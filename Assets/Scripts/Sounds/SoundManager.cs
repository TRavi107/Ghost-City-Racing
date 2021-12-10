using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sound
{
    engine,
    skid,
}
public static class SoundManager
{
    private static GameObject driftSoundSource;

    public static void PlaySound(Sound sound,Vector3 pos)
    {
        switch (sound)
        {
            case Sound.engine:
                
                break;
            case Sound.skid:
                if(driftSoundSource == null)
                {
                    driftSoundSource = new GameObject("Drift Sound SOurce");
                    driftSoundSource.AddComponent<AudioSource>();

                }
                AudioSource audiosource = driftSoundSource.GetComponent<AudioSource>();
                driftSoundSource.transform.position = pos;
                audiosource.volume = 0.5f;
                audiosource.PlayOneShot(GameAssets.instance.GetSoundClip(Sound.skid));
                break;
            default:
                break;
        }
        
    }

}
