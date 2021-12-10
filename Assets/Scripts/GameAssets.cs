using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static GameAssets instance;
    public List<SoundClips> soundClips;
    public Color[] availableColor;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    [System.Serializable]
    public class SoundClips
    {
        public Sound sound;
        public AudioClip audioClip;
    }

    public AudioClip GetSoundClip(Sound sound)
    {
        foreach (SoundClips clips in soundClips)
        {
            if (clips.sound == sound)
                return clips.audioClip;
        }

        return null;
    }
}
