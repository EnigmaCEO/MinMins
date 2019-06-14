using UnityEngine;
using System.Collections;
using Enigma.CoreSystems;

public class SceneManager : Manageable<SceneManager> {
	static public string targetScene;
	static public string tags;

	// Use this for initialization
	protected override void Start () {
		
		
	}
	
	// Update is called once per frame
	protected override void Update () {
	
	}

	static public void LoadUI() {
		//TextAsset xml = Resources.Load("SceneUI/" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name) as TextAsset;
		
		//UIManager.LoadUI(xml.text);
	}

	static public void LoadScene(string name) {
		targetScene = name;
        if (Application.CanStreamedLevelBeLoaded("Load") && (name == "Missions" || name == "Level"))
            UnityEngine.SceneManagement.SceneManager.LoadScene("Load");
        else if (Application.CanStreamedLevelBeLoaded("Load") && (name == "MissionsNetwork"))
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoadNetwork");
		else
            UnityEngine.SceneManagement.SceneManager.LoadScene(name);
		
	}

    static public void LoadSceneAdditive(string name)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(name, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
}
