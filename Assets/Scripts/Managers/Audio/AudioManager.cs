using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ShootAudioSource;
    [SerializeField] private AudioSource NotificationAudioSource;
    [SerializeField] private AudioSource PlayBackAudioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip backgroundAudio;
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip savedSFX;
    [SerializeField] private AudioClip shipDestroyedSFX;
    [SerializeField] private AudioClip gameWonSFX;
    [SerializeField] private AudioClip gameLoseSFX;

    private void Awake()
    {
        #region Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(NotificationAudioSource.gameObject);
        DontDestroyOnLoad(PlayBackAudioSource.gameObject);
        DontDestroyOnLoad(ShootAudioSource.gameObject);
        #endregion
        
    }
    private void Start()
    {
        PlayBackAudioSource.Play();
    }
    public void StopBGSFX()
    {
        Debug.Log("Stopping bg sfx");
        PlayBackAudioSource.Stop();
    }
    public void PlayOnClickButton()
    {
        NotificationAudioSource.PlayOneShot(buttonClickSFX);
    }
    public void PlayOnSave()
    {
        NotificationAudioSource.PlayOneShot(savedSFX);
    }
    public void PlayShootSFX(AudioClip clip)
    {
        ShootAudioSource.pitch=Random.Range(1.0f,1.5f);
        ShootAudioSource.PlayOneShot(clip);
    }
    public void PlaySpecialAbilityShotSFX()
    {
        ShootAudioSource.pitch = 5f;
        ShootAudioSource.PlayOneShot(shipDestroyedSFX);
    }
    public void PlayDestroyedSFX()
    {
        ShootAudioSource.PlayOneShot(shipDestroyedSFX);
    }
    public void PlayGameWonSFX()
    {
        if(!PlayBackAudioSource.isPlaying)PlayBackAudioSource.clip = gameWonSFX;

        PlayBackAudioSource.Play();
    }

    public void PlayGameLostSFX()
    {
        if (!PlayBackAudioSource.isPlaying) PlayBackAudioSource.clip = gameLoseSFX;

        PlayBackAudioSource.Play();
    }
}
