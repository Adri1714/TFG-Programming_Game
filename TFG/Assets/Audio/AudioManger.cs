using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Data")]
    [SerializeField] private SoundLibrary library;
    public SoundLibrary Library => library;

    [Header("Fonts")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private int sfxPoolSize = 10;

    private AudioSource[] sfxPool;
    private int currentSfxIndex = 0;

    private const string MasterKey = "Vol_master", MusicKey = "Vol_music", SfxKey = "Vol_sfx";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildSfxPool();
    }

    private void Start() => LoadVolumes();

    private void BuildSfxPool()
    {
        sfxPool = new AudioSource[sfxPoolSize];
        for(int i = 0; i < sfxPoolSize; i++)
        {
            var go = new GameObject("SFX_" + i);
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = sfxGroup;
            sfxPool[i] = source;
        }
    }

    public void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || sfxPool == null) return;
        var source = sfxPool[currentSfxIndex];
        currentSfxIndex = (currentSfxIndex + 1) % sfxPool.Length;
        source.Stop();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
    }
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (musicSource == null || clip == null || musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = volume;
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.Play();
    }
    public void ApplyEffectLowPass()
    {
        mixer.SetFloat("LowPass", 500f);
    }
    public void RemoveEffectLowPass()
    {
        mixer.SetFloat("LowPass", 5000.00f);
    }

    public static void Play(System.Func<SoundLibrary, AudioClip> pick)
    {
        if (Instance != null && Instance.library != null) Instance.PlaySfx(pick(Instance.library), 0.5f);
    }
    public static void Music(System.Func<SoundLibrary, AudioClip> pick)
    {
        if (Instance != null && Instance.library != null) Instance.PlayMusic(pick(Instance.library), 0.5f);
    }
    public static void Stop(System.Func<SoundLibrary, AudioClip> pick)
    {
        if (Instance != null && Instance.library != null)
        {
            var clip = pick(Instance.library);
            if (clip == Instance.musicSource.clip) Instance.musicSource.Stop();
            foreach (var source in Instance.sfxPool)
            {
                if (source.clip == clip) source.Stop();
            }
        }
    }

    public void SetMasterVolume(float v) => ApplyVolume("Master", MasterKey, v);
    public void SetMusicVolume(float v)  => ApplyVolume("Music",  MusicKey,  v);
    public void SetSfxVolume(float v)    => ApplyVolume("SFX",    SfxKey,    v);

    public float GetMaster() => PlayerPrefs.GetFloat(MasterKey, 1f);
    public float GetMusic()  => PlayerPrefs.GetFloat(MusicKey, 1f);
    public float GetSfx()    => PlayerPrefs.GetFloat(SfxKey, 1f);

    private void ApplyVolume(string param, string key, float v)
    {
        v = Mathf.Clamp01(v);
        if (mixer != null) mixer.SetFloat(param, Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat(key, v);
    }

    private void LoadVolumes()
    {
        SetMasterVolume(GetMaster());
        SetMusicVolume(GetMusic());
        SetSfxVolume(GetSfx());
    }
}