using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static GameAssets instance;
    public List<SoundClips> soundClips;
    public Color[] availableColor;

    [SerializeField]
    public Queue<MissileController> availableMissiles = new Queue<MissileController>();

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

    public MissileController SpawnFromPool(Vector3 position,Quaternion rotation,Transform target)
    {
        MissileController missile = availableMissiles.Dequeue();
        missile.gameObject.SetActive(true);
        missile.transform.position = position;
        missile.transform.rotation = rotation;
        missile.target = target;
        missile.Init();
        return missile;
    }

    public void ThrowBackToPool(MissileController missile)
    {
        missile.gameObject.SetActive(false);
        availableMissiles.Enqueue(missile);
    }

}
