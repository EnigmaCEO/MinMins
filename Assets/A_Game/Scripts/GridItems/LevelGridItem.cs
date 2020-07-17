using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma.CoreSystems;
using UnityEngine.UI;

public class LevelGridItem : MonoBehaviour
{
    public Button FightButton;
    public GameObject enjinReward;

    [SerializeField] private Text _label;
    [SerializeField] private GameObject _rawImage;
    [SerializeField] private Image _questIcon;
    //[SerializeField] private Text _labelShadow;

    private void Awake()
    {
        _questIcon.gameObject.SetActive(false);
    }

    //private void Start()
    //{
    //    int level = GameInventory.Instance.GetHigherSinglePlayerLevelCompleted();
    //    Debug.Log("Status: " + level);
    //}

    public void SetImageSprite(Sprite sprite)
    {
        _rawImage.SetActive(false);

        _questIcon.sprite = sprite;
        _questIcon.gameObject.SetActive(true);
    }

    public void SetLabel(string text)
    {
        _label.text = text;
        //_labelShadow.text = text;
    }

    public void OnFightButtonDown()
    {
        
    }

    public void EnableEnjinReward()
    {
        enjinReward.SetActive(true);
    }
}
