using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EnjinQRManager : MonoBehaviour
{
    public Text linkCodeText;
    [SerializeField] private string _linkCode = "AAAAAA";
    [SerializeField] private RawImage _rawImage;

    public void ShowImage(string linkCode)
    {
        _linkCode = linkCode;
        linkCodeText.text = _linkCode;
        StartCoroutine(DownloadImage("https://chart.googleapis.com/chart?chs=512x512&cht=qr&chl=" + _linkCode + "&chld=L|1"));
    }

    IEnumerator DownloadImage(string MediaUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if(request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            _rawImage.texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
    }
}
