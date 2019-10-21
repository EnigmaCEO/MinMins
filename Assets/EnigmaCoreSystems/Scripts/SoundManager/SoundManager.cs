using UnityEngine;
using System.Collections;
using Enigma.CoreSystems;
using DarkTonic.MasterAudio;
using System;

public class SoundManager : Manageable<SoundManager>
{
    public enum AudioTypes
    {
        Music,
        Sfx
    }

    //	public static int AUDIO_MUSIC = 1;
    //	public static int AUDIO_SFX = 2;

    public float FadeDelay = 2;

    static public Action OnMainAudioListenerSet;

    #region private member variables
    private float m_musicVolume = 0.5f;
    private float m_sfxVolume = 1.0f;
    private float m_totalVolume = 1.0f;
    public AudioListener m_audioListener;

    private GameObject master, group;
    public DynamicSoundGroupCreator _creator;

    private static string _lastSongPlayed = "";

    public static string SAVED_MUSIC_PREF = "MusicVol";
    public static string SAVED_SOUND_EFFECTS_PREF = "SfxVol";

    #endregion

    #region public accessors/mutators
    public static float MusicVolume
    {
        get { return Instance.m_musicVolume; }
        set { Instance.m_musicVolume = value; }
    }

    public static float SfxVolume
    {
        get { return Instance.m_sfxVolume; }
        set { Instance.m_sfxVolume = value; }
    }

    public static float TotalVolume
    {
        get { return Instance.m_totalVolume; }
        set { Instance.m_totalVolume = value; }
    }

    public static string LastSongPlayed { get { return _lastSongPlayed; } }
    #endregion

    public enum PlayType
    {
        Play = 0,
        PlayAtTransform = 1,
        PlayAtTransformAndFollow = 2
    }

    // Use this for initialization
    protected override void Awake()
    {
        if (!GameObject.Find("MasterAudio"))
        {
            var ma = Resources.Load("Sound/MasterAudio", typeof(GameObject));

            master = GameObject.Instantiate(ma) as GameObject;

            master.name = "MasterAudio";
            master.GetComponent<MasterAudio>().persistBetweenScenes = true;
            DontDestroyOnLoad(master);
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        MasterAudio.MasterVolumeLevel = Instance.m_totalVolume;
    }

    #region member functions
    public static void SetMainAudioListener()
    {
        AudioListener mcAudioListener = Camera.main.GetComponent<AudioListener>();
        Instance.m_audioListener = mcAudioListener;

        Instance.group = GameObject.Find("DynamicSoundGroupCreator");
        if (Instance.group != null)
        {
            Instance.group.name = "DynamicSoundGroupCreator";
            Instance._creator = Instance.group.GetComponent<DynamicSoundGroupCreator>();

            if (OnMainAudioListenerSet != null)
                OnMainAudioListenerSet.Invoke();
        }
    }

    public static void SetAudioPersistance(bool val)
    {
        Instance._creator.removeGroupsOnSceneChange = val;
    }

    public static void PlayAndWait(string clipName, SoundManager.AudioTypes type, string clipFolderPath = "", PlayType playType = PlayType.Play, float startDelay = 0, GameObject go = null, System.Action callback = null)
    {
        if (!handleSoundGroupCreation(clipName, type, clipFolderPath, false))
            return;

        if (type == SoundManager.AudioTypes.Music)
            _lastSongPlayed = clipName;

        if (go == null)
            simplePlayAndWait(clipName, startDelay, callback);
        else
        {
            switch (playType)
            {
                case PlayType.Play:
                    simplePlayAndWait(clipName, startDelay, callback);
                    break;
                case PlayType.PlayAtTransform:
                    Instance.StartCoroutine(MasterAudio.PlaySound3DAtTransformAndWaitUntilFinished(clipName, go.transform, 1, 1, startDelay, null, null, callback));
                    break;
                case PlayType.PlayAtTransformAndFollow:
                    Instance.StartCoroutine(MasterAudio.PlaySound3DFollowTransformAndWaitUntilFinished(clipName, go.transform, 1, 1, startDelay, null, null, callback));
                    break;
                default:
                    simplePlayAndWait(clipName, startDelay, callback);
                    break;
            }
        }
    }

    public static PlaySoundResult Play(string clipName, SoundManager.AudioTypes type, string clipFolderPath = "", bool loop = false, PlayType playType = PlayType.Play, GameObject go = null, System.Action callback = null)
    {
        if (!MasterAudio.SoundsReady || !GameObject.Find("MasterAudio") || Instance == null)
            return null;

        if (!handleSoundGroupCreation(clipName, type, clipFolderPath, loop))
            return null;

        if (type == SoundManager.AudioTypes.Music)
            _lastSongPlayed = clipName;

        float vol = 1;
        if (type == SoundManager.AudioTypes.Sfx)
            vol = Instance.m_sfxVolume;
        if (type == SoundManager.AudioTypes.Music)
            vol = Instance.m_musicVolume;

        if (go == null)
            return MasterAudio.PlaySound(clipName, vol);
        else
        {
            switch (playType)
            {
                case PlayType.Play:
                    return MasterAudio.PlaySound(clipName, vol);
                case PlayType.PlayAtTransform:
                    return MasterAudio.PlaySound3DAtTransform(clipName, go.transform, vol);
                case PlayType.PlayAtTransformAndFollow:
                    return MasterAudio.PlaySound3DFollowTransform(clipName, go.transform, vol);
                default:
                    return MasterAudio.PlaySound(clipName, vol);
            }
        }
    }

    public static void Stop()
    {
        MasterAudio.StopEverything();
        RestoreMusicVolume();
    }
    public static void Stop(string clipName)
    {
        MasterAudio.StopAllOfSound(clipName);
        RestoreMusicVolume();
    }

    public static void FadeOut(string clipName, float time, System.Action callback = null)
    {
        MasterAudio.FadeSoundGroupToVolume(clipName, 0, time, callback);
    }

    public static void FadeCurrentSong(System.Action callback = null)
    {
        FadeCurrentSong(SoundManager.Instance.FadeDelay, callback);
    }

    public static void FadeCurrentSong(float time, System.Action callback = null)
    {
        if (IsThereSongPlaying())
            FadeOut(_lastSongPlayed, time, callback);
        else
            callback.Invoke();
    }

    public static bool IsThereSongPlaying()
    {
        return ((_lastSongPlayed != "") && MasterAudio.IsSoundGroupPlaying(_lastSongPlayed));
    }

    public static void StopCurrentSong()
    {
        if (_lastSongPlayed != "")
            Stop(_lastSongPlayed);
    }

    public static void RestoreMusicVolume()
    {
        if (_lastSongPlayed != "")
            MasterAudio.SetGroupVolume(_lastSongPlayed, Instance.m_musicVolume);
    }
    #endregion

    private static bool handleSoundGroupCreation(string clipName, SoundManager.AudioTypes type, string clipFolderPath, bool loop)
    {
        if (!Instance._creator) SetMainAudioListener();

        if (!GameObject.Find("MasterAudio").transform.Find(clipName))
        {
            if (clipFolderPath != "")
                clipFolderPath += "/";

            AudioClip audioClip = Resources.Load("Sound/" + clipFolderPath + clipName, typeof(AudioClip)) as AudioClip;

            if (audioClip == null)
                return false;

            //string clipName = UtilStrings.TrimSpace(audioClip.name);

            
            GameObject spawnedGroup = (GameObject)GameObject.Instantiate(Instance._creator.groupTemplate, Instance._creator.transform.position, Quaternion.identity);
            spawnedGroup.name = clipName;

            spawnedGroup.transform.parent = Instance._creator.transform;

            DynamicSoundGroup myGroup = spawnedGroup.transform.GetComponent<DynamicSoundGroup>();

            GameObject spawnedVar = (GameObject)GameObject.Instantiate(Instance._creator.variationTemplate, Instance._creator.transform.position, Quaternion.identity);
            spawnedVar.name = clipName;

            spawnedVar.transform.parent = spawnedGroup.transform;

            DynamicGroupVariation dynamicVar = spawnedVar.GetComponent<DynamicGroupVariation>();
            dynamicVar.VarAudio.clip = audioClip;
            dynamicVar.VarAudio.loop = loop;

            myGroup.groupVariations.Add(dynamicVar);

            if (type == SoundManager.AudioTypes.Music)
                myGroup.spatialBlendType = MasterAudio.ItemSpatialBlendType.ForceTo2D;

            MasterAudio.CreateSoundGroup(myGroup, Instance._creator.transform.name);
        }

        //		if(type == SoundManager.AUDIO_MUSIC)
        //			_lastSongPlayed = clip;

        return true;
    }

    private static void simplePlayAndWait(string clip, float startDelay, System.Action callback)
    {
        Instance.StartCoroutine(MasterAudio.PlaySoundAndWaitUntilFinished(clip, 1, 1, startDelay, null, callback));
    }
}
