using System.Collections.Generic;
using UnityEngine;

// Sound Class Define
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(.1f, 3f)]
    public float pitch = 1f;

    public bool loop;

    // AudioSource will be added by the script
    [HideInInspector]
    public AudioSource source;
}

// Main Class Define
public class AudioManager : MonoBehaviour
{
    // SINGLETON (Rend le gestionnaire accessible partout via AudioManager.instance)
    public static AudioManager instance;

    // Sound groups intern classes
    [System.Serializable]
    public class SoundGroup
    {
        public string groupName;     // Category
        public bool isRandom;        // Actives randomizer
        public List<Sound> sounds;   // Soundlist
    }

    public List<SoundGroup> soundGroups;

    // NOUVELLE VARIABLE : Nom du groupe/son à jouer au lancement de la scène
    public string musicOnStart = "";

    // Dictionnary
    private Dictionary<string, List<Sound>> soundDictionary;

    void Awake()
    {
        // 1. Mise en place du Singleton
        if (instance == null)
        {
            instance = this;
            // Garder l'objet entre les scènes (utile pour la musique persistante)
            // DontDestroyOnLoad(gameObject); 
            // NOTE: Si vous voulez que l'audio soit détruit au changement de scène,
            // laissez la ligne ci-dessus commentée.
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 2. Initialisation des données
        soundDictionary = new Dictionary<string, List<Sound>>();

        // Init at start
        foreach (SoundGroup soundGroup in soundGroups)
        {
            // Add group to dictionnary
            soundDictionary[soundGroup.groupName] = soundGroup.sounds;

            foreach (Sound s in soundGroup.sounds)
            {
                // Add Audiosource for each sound
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
                s.source.playOnAwake = false; // Autoplay off
            }
        }

        // 3. LECTURE AUTOMATIQUE DE LA MUSIQUE DE SCÈNE
        if (!string.IsNullOrEmpty(musicOnStart))
        {
            PlaySound(musicOnStart);
        }
    }

    // --- Fonctions de Lecture ---

    // Normal seek and play
    public void PlayNormalSound(string name)
    {
        foreach (var soundGroup in soundGroups)
        {
            Sound s = soundGroup.sounds.Find(sound => sound.name == name);
            if (s != null)
            {
                if (!s.source.isPlaying)
                {
                    s.source.Play();
                }
                return;
            }
        }
        Debug.LogWarning("Sound: " + name + " not found!");
    }

    // Random play
    public void PlayRdmSound(string groupName)
    {
        if (!soundDictionary.ContainsKey(groupName))
        {
            Debug.LogWarning("Sound Group: " + groupName + " not found!");
            return;
        }

        List<Sound> sounds = soundDictionary[groupName];

        if (sounds.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, sounds.Count);
        Sound randomSound = sounds[randomIndex];

        if (randomSound.source != null)
        {
            randomSound.source.Play();
        }
    }

    // Main function: calls Normal or Random
    public void PlaySound(string groupName)
    {
        if (!soundDictionary.ContainsKey(groupName))
        {
            Debug.LogWarning("Sound Group: " + groupName + " not found!");
            return;
        }

        SoundGroup sg = soundGroups.Find(group => group.groupName == groupName);

        if (sg.isRandom == true)
        {
            PlayRdmSound(groupName);
        }
        else
        {
            // If not random, the groupName is used as the specific sound name.
            PlayNormalSound(groupName);
        }
    }

    // 4. NOUVELLE FONCTION : Arrêter un son spécifique (parfait pour la musique)
    public void StopSound(string name)
    {
        foreach (var soundGroup in soundGroups)
        {
            Sound s = soundGroup.sounds.Find(sound => sound.name == name);
            if (s != null)
            {
                if (s.source.isPlaying)
                {
                    s.source.Stop();
                }
                return;
            }
        }
        Debug.LogWarning("Sound: " + name + " not found for stopping!");
    }
}