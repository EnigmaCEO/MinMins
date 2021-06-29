using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestConfirmPopUp : MonoBehaviour
{
    [SerializeField] private Transform _rewardsGridContent;

    [SerializeField] private GameObject _unitRewardTemplate;
    [SerializeField] private GameObject _teamBoostRewardTemplate;
    [SerializeField] private GameObject _boxRewardTemplate;

    private string _sceneToLoad = "";

    private void Start()
    {
        prepareGridTemplate(_unitRewardTemplate);
        prepareGridTemplate(_teamBoostRewardTemplate);
        prepareGridTemplate(_boxRewardTemplate);
    }

    private void prepareGridTemplate(GameObject gridTemplate)
    {
        gridTemplate.transform.SetParent(_rewardsGridContent.parent);
        gridTemplate.SetActive(false);
    }

    public void Open(string sceneToLoad, Dictionary<int, int> boxTiersWithAmountRewards, List<TeamBoostItemGroup> boostItemGroups, List<string> unitNames)
    {
        _sceneToLoad = sceneToLoad;

        if (boxTiersWithAmountRewards != null)
        {
            foreach (KeyValuePair<int, int> amountByTier in boxTiersWithAmountRewards)
            {
                int count = amountByTier.Value;
                for (int i = 0; i < count; i++)
                {
                    GameObject boxReward = Instantiate<GameObject>(_boxRewardTemplate, _rewardsGridContent);
                    boxReward.SetActive(true);
                }
            }
        }

        if (boostItemGroups != null)
        {
            foreach (TeamBoostItemGroup boostItemGroup in boostItemGroups)
            {
                GameObject boostReward = Instantiate<GameObject>(_teamBoostRewardTemplate, _rewardsGridContent);
                boostReward.GetComponent<BoostRewardGridItem>().SetUp(boostItemGroup.Category, boostItemGroup.Bonus);
                boostReward.SetActive(true);
            }
        }

        if (unitNames != null)
        {
            foreach (string unitName in unitNames)
            {
                GameObject unitReward = Instantiate<GameObject>(_unitRewardTemplate, _rewardsGridContent);
                unitReward.GetComponent<UnitRewardGridItem>().Setup(unitName);
            }
        }

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnOkButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();
        GameStats.Instance.Mode = GameStats.Modes.Quest;
        SceneManager.LoadScene(_sceneToLoad);
    }

    public void OnCancelButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        Close();
    }
}
