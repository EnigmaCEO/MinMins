using UnityEngine;
using System.Collections;
using Enigma.CoreSystems;

namespace Enigma.CoreSystems
{
    public class SceneManager : Manageable<SceneManager>
    {
        static public string TargetScene;
        static public bool TargetSceneUsesNetworkLoad = true; 
        static public string Tags;

        // Use this for initialization
        protected override void Start()
        {


        }

        // Update is called once per frame
        protected override void Update()
        {

        }

        static public void LoadUI()
        {
            //TextAsset xml = Resources.Load("SceneUI/" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name) as TextAsset;

            //UIManager.LoadUI(xml.text);
        }

        static public void LoadScene(string name, bool usesLoadingScreen = false, bool usesNetworkSceneLoading = false)
        {
            TargetScene = name;
            TargetSceneUsesNetworkLoad = usesNetworkSceneLoading;

            string sceneToLoad = name;
        
            if (usesLoadingScreen && Application.CanStreamedLevelBeLoaded(EnigmaConstants.Scenes.LOAD))
            {
                sceneToLoad = EnigmaConstants.Scenes.LOAD;
            }

            if (usesNetworkSceneLoading)
            {
                NetworkManager.LoadScene(sceneToLoad);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
            }
        }

        static public void LoadSceneAdditive(string name)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(name, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }
}
