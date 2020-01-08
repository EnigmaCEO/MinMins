using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePopUp : MonoBehaviour
{
    [SerializeField] private Text _text;
    [SerializeField] private Button _dismissButton;

    public delegate void DismissButtonDownDelegate(string message);
    public DismissButtonDownDelegate OnDismissButtonDownCallback;

    private void Awake()
    {
        _dismissButton.onClick.AddListener(onDismissButtonDown);
    }

    private void Start()
    {
        Close();
    }

    public void Open(string term)
    {
        _text.text = LocalizationManager.GetTermTranslation(term);
        gameObject.SetActive(true);
    }

    private void onDismissButtonDown()
    {
        if (OnDismissButtonDownCallback != null)
            OnDismissButtonDownCallback(_text.text);

        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
