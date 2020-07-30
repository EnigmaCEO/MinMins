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

        if (!_messagesTermPerQuest.ContainsKey(activeQuest))
        {
            Debug.LogError("There is not message set for Active Quest: " + activeQuest.ToString());
        }
        else
        {
            _message.text = LocalizationManager.GetTermTranslation(_messagesTermPerQuest[activeQuest]);

            string rewardImagePath = "Images/Quests/" + activeQuest.ToString() + " Reward";

            Sprite questSprite = (Sprite)Resources.Load<Sprite>(rewardImagePath);

            if (questSprite != null)
            {
                _questIcon.sprite = questSprite;
            }
            else
            {
                Debug.Log("Quest reward image was not found at path: " + rewardImagePath + " . Please check active quest is correct and image is in the right path.");
            }

            gameObject.SetActive(true);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
