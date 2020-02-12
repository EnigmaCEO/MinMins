using UnityEngine;
using System.Collections;
using Enigma.CoreSystems;
using UnityEngine.UI;

public class Load : MonoBehaviour
{
    [SerializeField] private Image _progressImage;

    private float _startTime;
    private int _state;

    void Start()
    {
        _startTime = Time.time;
        _progressImage.fillAmount = 0;
        _state = 0;
    }

    void Update()
    {
        switch (_state)
        {
            case 0:
                float timer = 1 - Mathf.FloorToInt(Time.time - _startTime);

                if (timer < 0)
                {
                    timer = 0;
                }

                if (timer == 0)
                {
                    _startTime = Time.time;
                    _state = 1;
                }
                break;

            case 1:
                float timer2 = 1 - (Time.time - _startTime);

                if (timer2 < 0)
                {
                    timer2 = 0;
                }

                if (timer2 == 0)
                {
                    _state = 2;
                }
                else
                {
                    if (timer2 > 0.2f)
                    {
                        _progressImage.fillAmount = (1.0f - timer2);
                    }
                }
                break;

            case 2:
                _state = 3;
                StartCoroutine("LoadScene");
                break;
        }
    }

    IEnumerator LoadScene()
    {
        //Debug.LogError(SceneManager.targetScene);
        AsyncOperation async = null;

        if (SceneManager.TargetSceneUsesNetworkLoad)
        {
            async = NetworkManager.LoadSceneAsync(SceneManager.TargetScene);
        }
        else
        {
            async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneManager.TargetScene);
        }

        //if (EnigmaHacks.Instance.FreezeLoadingScreenDelay.Enabled)
        //{
        //    yield return new WaitForSeconds(EnigmaHacks.Instance.FreezeLoadingScreenDelay.ValueAsFloat);
        //}

        while (!async.isDone)
        {
            _progressImage.fillAmount = async.progress;
            yield return null;
        }
    }
}

