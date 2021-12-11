using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sound
{
    engine,
    skid,
    gearchange,
}
public static class SoundManager
{
    private static GameObject driftSoundSource;
    private static GameObject gearSoundSource;


    public static void PlaySound(Sound sound,Transform pos)
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
                driftSoundSource.transform.position = pos.transform.position;
                driftSoundSource.transform.parent = pos;
                audiosource.PlayOneShot(GameAssets.instance.GetSoundClip(Sound.skid));
                audiosource.volume = 0.2f;
                audiosource.spatialBlend = 1;
                audiosource.maxDistance = 100;
                break;

            case Sound.gearchange:
                if (gearSoundSource == null)
                {
                    gearSoundSource = new GameObject("Gear Sound SOurce");
                    gearSoundSource.AddComponent<AudioSource>();

                }
                AudioSource G_audiosource = gearSoundSource.GetComponent<AudioSource>();
                gearSoundSource.transform.position = pos.transform.position;
                gearSoundSource.transform.parent = pos;
                G_audiosource.PlayOneShot(GameAssets.instance.GetSoundClip(Sound.gearchange));
                G_audiosource.volume = 0.5f;
                G_audiosource.spatialBlend = 1;
                G_audiosource.maxDistance = 100;
                break;
            default:
                break;
        }
        
    }

}
