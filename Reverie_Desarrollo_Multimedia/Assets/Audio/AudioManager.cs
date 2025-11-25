using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource transitionSource;

    [Header("Música de Exploración")]
    [SerializeField] private AudioClip musicSpringExploration;
    [SerializeField] private AudioClip musicAutumnExploration;
    [SerializeField] private AudioClip musicWinterExploration;

    [Header("Música de Batalla y Final")]
    [SerializeField] private AudioClip musicBattle;
    [SerializeField] private AudioClip musicFinalCabin;

    [Header("Configuración")]
    [SerializeField] private float fadeTime = 2f;
    [SerializeField] private float maxVolume = 0.7f;

    [Header("Configuración Inicial")]
    [SerializeField] private bool playMusicOnStart = true;
    [SerializeField] private AudioClip initialMusic; 

    private AudioClip currentClip;
    private bool isTransitioning = false;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        if (playMusicOnStart && initialMusic != null)
        {
            PlayMusic(initialMusic);
        }
    }

    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        if (transitionSource == null)
        {
            transitionSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        transitionSource.loop = true;
        musicSource.volume = maxVolume;
        transitionSource.volume = 0f;
    }


    public void PlaySpringExploration()
    {
        PlayMusic(musicSpringExploration);
    }

    public void PlayAutumnExploration()
    {
        PlayMusic(musicAutumnExploration);
    }

    public void PlayWinterExploration()
    {
        PlayMusic(musicWinterExploration);
    }

    public void PlayBattleMusic()
    {
        PlayMusic(musicBattle);
    }

    public void PlayFinalCabinMusic()
    {
        PlayMusic(musicFinalCabin);
    }


    private void PlayMusic(AudioClip newClip)
    {
        if (newClip == null)
        {
            Debug.LogWarning("AudioClip es null!");
            return;
        }


        if (currentClip == newClip && musicSource.isPlaying)
        {
            return;
        }

        currentClip = newClip;


        if (!musicSource.isPlaying)
        {
            musicSource.clip = newClip;
            musicSource.Play();
        }
        else if (!isTransitioning)
        {
            StartCoroutine(CrossfadeMusic(newClip));
        }
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        isTransitioning = true;


        transitionSource.clip = newClip;
        transitionSource.Play();
        transitionSource.volume = 0f;

        float elapsed = 0f;


        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;

            musicSource.volume = Mathf.Lerp(maxVolume, 0f, t);
            transitionSource.volume = Mathf.Lerp(0f, maxVolume, t);

            yield return null;
        }


        musicSource.Stop();
        AudioSource temp = musicSource;
        musicSource = transitionSource;
        transitionSource = temp;

        musicSource.volume = maxVolume;
        transitionSource.volume = 0f;

        isTransitioning = false;
    }


    public void StopMusic()
    {
        StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeOutMusic()
    {
        float elapsed = 0f;
        float startVolume = musicSource.volume;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = maxVolume;
    }
}