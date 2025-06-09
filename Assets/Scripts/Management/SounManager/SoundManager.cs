using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;

        [HideInInspector] public AudioSource source;
    }

    public Sound[] sounds;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Crear un AudioSource por cada sonido
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string soundName)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning($"AudioManager: sonido '{soundName}' no encontrado.");
            return;
        }
        s.source.Play();
    }

    public void Stop(string soundName)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == soundName);
        if (s == null) return;
        s.source.Stop();
    }
}
