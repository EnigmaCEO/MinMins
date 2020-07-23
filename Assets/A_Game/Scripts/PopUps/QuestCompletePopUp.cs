using GameEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestCompletePopUp : MonoBehaviour
{
    [SerializeField] private Text _message;
    [SerializeField] private Image _questIcon;

    private Dictionary<Quests, string> _messagesTermPerQuest = new Dictionary<Quests, string>();

    private void Start()
    {
        _messagesTermPerQuest.Add(Quests.Swissborg, "Swissborg Reward");

        Close();
    }

    public void OnOkButtonDown()
    {
        Close();
    }

    public void Open()
    {
        Quests activeQuest = GameStats.Instance.ActiveQuest;

        _message.text = LocalizationManager.GetTermTranslation(_messagesTermPerQuest[activeQuest]);

        Sprite questSprite = (Sprite)Resources.Load<Sprite>("Images/Quests/" + activeQuest.ToString() + " Reward");
        _questIcon.sprite = questSprite;

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
