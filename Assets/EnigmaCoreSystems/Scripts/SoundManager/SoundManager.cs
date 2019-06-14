using UnityEngine;
using System.Collections;
using Enigma.CoreSystems;
using DarkTonic.MasterAudio;

public class SoundManager : Manageable<SoundManager> 
{
	#region private member variables
	private float m_musicVolume = 0.5f;
	private float m_sfxVolume = 1.0f;
	private float m_totalVolume = 1.0f;
	public AudioListener m_audioListener;
	
	public static int AUDIO_MUSIC = 1;
	public static int AUDIO_SFX = 2;

	private GameObject master, group;
	private DynamicSoundGroupCreator _creator;
	
	#endregion

	#region public accessors/mutators
	public static float MusicVolume
	{
		get{ return Instance.m_musicVolume; }
		set{ Instance.m_musicVolume = value; }
	}

	public static float SfxVolume
	{
		get{ return Instance.m_sfxVolume; }
		set{ Instance.m_sfxVolume = value; }
	}

	public static float TotalVolume
	{
		get{ return Instance.m_totalVolume; }
		set{ Instance.m_totalVolume = value; }
	}
	#endregion

	public enum PlayType
	{
		Play = 0,
		PlayAtTransform = 1,
		PlayAtTransformAndFollow = 2
	}
			
	// Use this for initialization
	protected override void Awake () 
	{	
		if(!GameObject.Find("MasterAudio")) {
			var ma = Resources.Load("Sound/MasterAudio.prefab", typeof(GameObject));
			
			master = GameObject.Instantiate(ma) as GameObject;
			
			master.name = "MasterAudio";
			master.GetComponent<MasterAudio>().persistBetweenScenes = true;
			DontDestroyOnLoad(master);
		}
	}
	
	// Update is called once per frame
	protected override void Update () 
	{
		MasterAudio.MasterVolumeLevel = Instance.m_totalVolume;
	}

	#region member functions
	public static void SetMainAudioListener()
	{
		AudioListener mcAudioListener = Camera.main.GetComponent<AudioListener>();
		Instance.m_audioListener = mcAudioListener;

		/*var pc = Resources.Load("Sound/DynamicSoundGroupCreator.prefab", typeof(GameObject));
		*/
		Instance.group = GameObject.Find("DynamicSoundGroupCreator");//GameObject.Instantiate(pc) as GameObject;
		if(Instance.group != null) {
			Instance.group.name = "DynamicSoundGroupCreator";
			Instance._creator = Instance.group.GetComponent<DynamicSoundGroupCreator>();
		}
	}

	public static void SetAudioPersistance(bool val)
	{
		Instance._creator.removeGroupsOnSceneChange = val;
	}

	public static PlaySoundResult Play(string clip, int type, bool loop = false, PlayType pt = PlayType.Play, GameObject go = null ) {
		if(!GameObject.Find("MasterAudio") || !MasterAudio.SoundsReady) return null;
		
		float vol = 1.0f;
		if(type == SoundManager.AUDIO_MUSIC) vol = Instance.m_musicVolume;
		if(type == SoundManager.AUDIO_SFX) vol = Instance.m_sfxVolume;

		//Debug.Log(clip);
		if(!GameObject.Find("MasterAudio").transform.Find(clip)) {
			AudioClip aClip = Resources.Load ("Sound/" + clip, typeof(AudioClip)) as AudioClip;
            if (aClip == null) return null;
			var clipName = UtilStrings.TrimSpace(aClip.name);
			
			var spawnedGroup = (GameObject)GameObject.Instantiate(Instance._creator.groupTemplate, Instance._creator.transform.position, Quaternion.identity);
			spawnedGroup.name = clipName;
			
			spawnedGroup.transform.parent = Instance._creator.transform;
	
			
			var myGroup = spawnedGroup.transform.GetComponent<DynamicSoundGroup>();
			
			var spawnedVar = (GameObject)GameObject.Instantiate(Instance._creator.variationTemplate, Instance._creator.transform.position, Quaternion.identity);
			spawnedVar.name = clipName;

			spawnedVar.transform.parent = spawnedGroup.transform;
			
			var dynamicVar = spawnedVar.GetComponent<DynamicGroupVariation>();
			dynamicVar.VarAudio.clip = aClip;
			dynamicVar.VarAudio.loop = loop;

			myGroup.groupVariations.Add(dynamicVar);
	
			MasterAudio.CreateSoundGroup(myGroup, Instance._creator.transform.name);
		}

		switch( pt )
		{
		case PlayType.Play:
			return MasterAudio.PlaySound(clip, vol);
		case PlayType.PlayAtTransform:
			if( go != null )
				return MasterAudio.PlaySound3DAtTransform(clip, go.transform, vol);
			else
				return MasterAudio.PlaySound( clip, vol);
		case PlayType.PlayAtTransformAndFollow:
			if( go != null )
				return MasterAudio.PlaySound3DFollowTransform(clip, go.transform, vol);
			else
				return MasterAudio.PlaySound( clip, vol);
		default:
			return MasterAudio.PlaySound(clip,vol);
		}
	}

	public static void Stop() {
		MasterAudio.StopEverything();
	}
	public static void Stop(string clip) {
		MasterAudio.StopAllOfSound(clip);
	}
	
	#endregion
}
