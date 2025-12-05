using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("Volumes")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("SFX Clips")]
    public AudioClip shootClip;       // normal bullet
    public AudioClip fireballClip;    // fireball
    public AudioClip missileClip;     // missile
    public AudioClip enemyDeathClip;  // enemy death

    public AudioClip enemyshoot; //enemy shoot

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Awake()
    {

        // SINGLETON GUARD
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create two audio sources: one for music, one for SFX
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource   = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        sfxSource.playOnAwake   = false;

        musicSource.volume = musicVolume;
        sfxSource.volume   = sfxVolume;

        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Adjust this to your scene names
        if (scene.name == "MainMenu")
        {
            PlayMusic(menuMusic);
        }
        else
        {
            // Any non-main-menu scene uses game music
            PlayMusic(gameMusic);
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (!clip) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return; // already playing this track

        musicSource.Stop();      // ensure old track stops
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

        //NEW: music pause / resume for the pause menu
    public void PauseMusic()
    {
        if (musicSource && musicSource.isPlaying)
            musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource && musicSource.clip != null)
            musicSource.UnPause();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (!clip) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // Optional: allow changing volume at runtime
    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
    }
}

