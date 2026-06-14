using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource soundSource;
    public AudioSource musicSource;

    public AudioClip clickSound;
    public AudioClip moveSound;
    public AudioClip bounceSound;
    public AudioClip eraseSound;
    public AudioClip magicSound;
    public AudioClip timeFreezeSound;
    public AudioClip gameOverSound;
    public AudioClip gameWinSound;

    private bool soundEnabled = true;
    private bool musicEnabled = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        EnsureAudioSources();
    }

    private void Start()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

        EnsureAudioSources();
        soundSource.enabled = soundEnabled;
        musicSource.enabled = musicEnabled;

        if (musicEnabled && musicSource.clip != null)
        {
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void SetSoundEnabled(bool enabled)
    {
        soundEnabled = enabled;
        EnsureAudioSources();
        soundSource.enabled = enabled;
    }

    public void SetMusicEnabled(bool enabled)
    {
        musicEnabled = enabled;
        EnsureAudioSources();
        musicSource.enabled = enabled;

        if (enabled && musicSource.clip != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
        else if (!enabled)
        {
            musicSource.Stop();
        }
    }

    public void PlayClickSound()
    {
        EnsureAudioSources();
        if (soundEnabled && clickSound != null)
            soundSource.PlayOneShot(clickSound);
    }

    public void PlayMoveSound()
    {
        EnsureAudioSources();
        if (soundEnabled && moveSound != null)
            soundSource.PlayOneShot(moveSound);
    }

    public void PlayBounceSound()
    {
        EnsureAudioSources();
        if (soundEnabled && bounceSound != null)
            soundSource.PlayOneShot(bounceSound);
    }

    public void PlayEraseSound()
    {
        EnsureAudioSources();
        if (soundEnabled && eraseSound != null)
            soundSource.PlayOneShot(eraseSound);
    }

    public void PlayMagicSound()
    {
        EnsureAudioSources();
        if (soundEnabled && magicSound != null)
            soundSource.PlayOneShot(magicSound);
    }

    public void PlayTimeFreezeSound()
    {
        EnsureAudioSources();
        if (soundEnabled && timeFreezeSound != null)
            soundSource.PlayOneShot(timeFreezeSound);
    }

    public void PlayGameOverSound()
    {
        EnsureAudioSources();
        if (soundEnabled && gameOverSound != null)
            soundSource.PlayOneShot(gameOverSound);
    }

    public void PlayGameWinSound()
    {
        EnsureAudioSources();
        if (soundEnabled && gameWinSound != null)
            soundSource.PlayOneShot(gameWinSound);
    }

    private void EnsureAudioSources()
    {
        if (soundSource == null)
        {
            soundSource = gameObject.AddComponent<AudioSource>();
            soundSource.playOnAwake = false;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
    }
}
