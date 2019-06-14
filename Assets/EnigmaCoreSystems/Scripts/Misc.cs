using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Misc
{
    public static IEnumerator InstantiateGameObjectFromAssetBundle(AssetBundle assetBundle, string assetName, Transform parent)
    {
        AssetBundleRequest assetLoadRequest = assetBundle.LoadAssetAsync<GameObject>(assetName);
        yield return assetLoadRequest;

        GameObject prefab = assetLoadRequest.asset as GameObject;
        GameObject instance = GameObject.Instantiate<GameObject>(prefab);
        instance.name = prefab.name;

        instance.transform.SetParent(parent);
        instance.transform.localPosition = Vector3.zero;
    }

    public static IEnumerator InstantiateSpriteFromAssetBundle(AssetBundle assetBundle, string assetName, Image image)
    {
        AssetBundleRequest assetLoadRequest = assetBundle.LoadAssetAsync<Sprite>(assetName);
        yield return assetLoadRequest;

        Sprite texture = assetLoadRequest.asset as Sprite;
        Sprite instance = GameObject.Instantiate<Sprite>(texture);

        image.sprite = instance; 
    }

    public static void HandleScreenFade(float startTime, string sceneToLoad)
    {
        float timer = 8 - Mathf.FloorToInt(Time.time - startTime);

        if (timer < 0)
            timer = 0;

        if (Input.anyKey)
            timer = 0;

        if (timer == 0)
        {
            //Debug.Log("zero");
            //SoundManager.Stop();
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
