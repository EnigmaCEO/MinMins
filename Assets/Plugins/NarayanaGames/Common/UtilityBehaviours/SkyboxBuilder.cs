using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace NarayanaGames.Common.UtilityBehaviours {
    /// <summary>
    ///     This is a skybox builder based on: http://wiki.unity3d.com/index.php/New_Skybox_Generator
    ///     The main difference between the script from the Wiki and this is 
    ///     that you add this to a camera in the scene that can have any number
    ///     of image effects or special settings and the Skybox will be built
    ///     using that camera. It also uses the location of that camera as
    ///     center for the Skybox.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class SkyboxBuilder : MonoBehaviour {

        public int faceSize = 8192;
        public string directory = "Assets/Skyboxes";

        public string textureBaseName = "Skybox";
        public string[] skyBoxImage = new string[] { "Front", "Right", "Back", "Left", "Up", "Down" };
        public string[] skyBoxProps = new string[] { "_FrontTex", "_RightTex", "_BackTex", "_LeftTex", "_UpTex", "_DownTex" };

        public Material skyboxMaterial;
        public string skyboxShader = "RenderFX/Skybox";

        static Vector3[] skyDirection = new Vector3[] {
            new Vector3(0, 0, 0), new Vector3(0, -90, 0), new Vector3(0, 180, 0), new Vector3(0, 90, 0), new Vector3(-90, 0, 0), new Vector3(90, 0, 0)
        };

        [ContextMenu("Render Skybox")]
        public void RenderSkyboxTo6PNG() {
            Camera cam = GetComponent<Camera>();

            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 90;
            cam.aspect = 1.0f;

            cam.transform.rotation = Quaternion.identity;

            //Render skybox        
            for (int orientation = 0; orientation < skyDirection.Length; orientation++) {
                string assetPath = Path.Combine(directory, TextureName(orientation));
                RenderSkyBoxFaceToPNG(orientation, cam, assetPath);
            }
#if UNITY_EDITOR

            if (skyboxMaterial != null) {
                //Wire skybox material
                AssetDatabase.Refresh();

                //Material skyboxMaterial = new Material(Shader.Find(skyboxShader));
                for (int orientation = 0; orientation < skyDirection.Length; orientation++) {
                    string texPath = Path.Combine(directory, TextureName(orientation));
                    Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texPath, typeof(Texture2D));
                    tex.wrapMode = TextureWrapMode.Clamp;
                    skyboxMaterial.SetTexture(skyBoxProps[orientation], tex);
                }
                ////Save material
                //string matPath = Path.Combine(directory, t.name + "_skybox" + ".mat");
                //AssetDatabase.CreateAsset(skyboxMaterial, matPath);
            }
#endif
            Debug.LogFormat("Finished saving skybox to: {0}", directory);
        }

        private string TextureName(int orientation) {
            return string.Format("{0}_{1}.png", textureBaseName, skyBoxImage[orientation]);
        }

        private void RenderSkyBoxFaceToPNG(int orientation, Camera cam, string assetPath) {
            cam.transform.eulerAngles = skyDirection[orientation];
            RenderTexture rt = new RenderTexture(faceSize, faceSize, 24);
            {
                cam.targetTexture = rt;
                RenderTexture.active = rt;
                cam.Render();

                Texture2D screenShot = new Texture2D(faceSize, faceSize, TextureFormat.RGB24, false, true);
                screenShot.ReadPixels(new Rect(0, 0, faceSize, faceSize), 0, 0);

                RenderTexture.active = null;
                cam.targetTexture = null;

                byte[] bytes = screenShot.EncodeToPNG();
                File.WriteAllBytes(assetPath, bytes);
            }
            GameObject.DestroyImmediate(rt);
#if UNITY_EDITOR

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
#endif
        }

    }
}