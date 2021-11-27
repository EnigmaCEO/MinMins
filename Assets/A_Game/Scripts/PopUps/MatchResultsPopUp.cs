using Enigma.CoreSystems;
using GameConstants;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MatchResultsPopUp : MonoBehaviour
{
    //public Button DismissButton;

    public Action<bool> DismissButtonDown;

    [SerializeField] private Text _winnerText;

    [SerializeField] private Text _damageDealtValue;
    [SerializeField] private Text _damageReceivedValue;
    [SerializeField] private Text _unitsKilledValue;
    [SerializeField] private Text _matchDurationValue;

    [SerializeField] private GameObject _uploadReplayButton;
    [SerializeField] private Text _replayUploadStatusText;
    [SerializeField] private Text _uploadButtonText;

    [SerializeField] private Transform _unitsAliveGridContent;
    [SerializeField] private Transform _rewardsGridContent;

    private ReplayManager _replayManager;

    private const string _STATUS_TERM_UPLOADING_ = "Uploading...";
    private const string _STATUS_TERM_SERVER_ERROR = "Retry replay upload";
    private const string _JENJ = "JENJ";
    private const string _REPLAY_ERROR = "Replay failed";
    private const string _PREPARING_REPLAY = "Preparing replay...";

    private War _warRef;
    private bool _questWasCompleted = false;
    private bool _localPlayerWon = false;
    private bool _resultsAreReady = false;

    private void Awake()
    {
        _replayManager = FindObjectOfType<ReplayManager>();

        _warRef = War.GetSceneInstance();

        _uploadReplayButton.SetActive(false);
      
        _replayManager.RecordingFailedCallback += onRecordingFailed;
        _replayManager.RecordingPreviewReadyCallback += onRecordingPreviewReady;
    }

    private void OnDestroy()
    {
        _replayManager.RecordingFailedCallback -= onRecordingFailed;
        _replayManager.RecordingPreviewReadyCallback -= onRecordingPreviewReady;
    }

    public void OnDismissButtonDown()
    {
        if (DismissButtonDown != null)
        {
            DismissButtonDown(_questWasCompleted);
        }

        if (_replayManager.IsPreviewAvailable())
        {
            _replayManager.DiscardPreview();
        }
    }

    private void onRecordingPreviewReady()
    {
        _replayManager.StartCoroutine(handleRecordingPreviewReadyUi());
    }

    private IEnumerator handleRecordingPreviewReadyUi()
    {
        while (true)
        {
            if (!_resultsAreReady)
            {
                yield return null;
            }
            else
            {
                if (_localPlayerWon)
                {
                    _replayUploadStatusText.gameObject.SetActive(false);
                    _uploadReplayButton.SetActive(true);
                }

                break;
            }
        }
    }

    private void onRecordingFailed()
    {
        _replayManager.StartCoroutine(handleRecordingFailedUi());
    }

    private IEnumerator handleRecordingFailedUi()
    {
        while (true)
        {
            if (!_resultsAreReady)
            {
                yield return null;
            }
            else
            {
                if (_localPlayerWon)
                {
                    _replayUploadStatusText.text = LocalizationManager.GetTermTranslation(_REPLAY_ERROR);
                    _replayUploadStatusText.gameObject.SetActive(true);
                }

                break;
            }
        }
    }

    public void OnUploadReplayButtonDown()
    {
        string replayVideoPath = _replayManager.GetRecordingFile();

        Hashtable hashtable = new Hashtable
        {
            { GameNodeKeys.WINNER_NICKNAME, NetworkManager.GetRoomCustomProperty(GameRoomProperties.WINNER_NICKNAME)},
            { GameNodeKeys.LOSER_NICKNAME, NetworkManager.GetRoomCustomProperty(GameRoomProperties.LOSER_NICKNAME)},
            { EnigmaNodeKeys.VIDEO_PATH, replayVideoPath }
        };

        NetworkManager.Transaction(GameTransactions.UPLOAD_REPLAY, hashtable, onReplayUploaded);

        _uploadReplayButton.SetActive(false);

        _replayUploadStatusText.gameObject.SetActive(true);
        _replayUploadStatusText.text = LocalizationManager.GetTermTranslation(_STATUS_TERM_UPLOADING_);
    }

    private void onReplayUploaded(JSONNode response)
    {
        //if(response != null)
        //Debug.Log("onReplayUploaded -> response: " + response.ToString());
        bool serverError = true;

        if (!NetworkManager.CheckInvalidServerResponse(response, nameof(onReplayUploaded)))
        {
            JSONNode response_hash = response[0];

            JSONNode rewardNode = NetworkManager.CheckValidNode(response_hash, GameNodeKeys.REWARD);

            if (rewardNode != null)
            {
                if (float.TryParse(rewardNode.ToString().Trim('"'), out float rewardFloat))
                {
                    GameStats.Instance.EnjBalance += rewardFloat;

                    _replayUploadStatusText.text = " + " + rewardFloat.ToString() + LocalizationManager.GetTermTranslation(_JENJ);
                    serverError = false;
                }
            }
        }

        if (serverError)
        {
            _replayUploadStatusText.gameObject.SetActive(false);

            _uploadReplayButton.SetActive(true);
            _uploadButtonText.text = LocalizationManager.GetTermTranslation(_STATUS_TERM_SERVER_ERROR);
        }
    }

    public void Open(bool questWasCompleted)
    {
        _questWasCompleted = questWasCompleted;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void SetValues(War.MatchLocalData matchLocalData, TeamBoostItemGroup boostItemGroup)
    {
        string winnerNickname = NetworkManager.GetRoomCustomProperty(GameRoomProperties.WINNER_NICKNAME);
        string localPlayerNickname = NetworkManager.GetLocalPlayerNickname();

        if (localPlayerNickname == winnerNickname)
        {
            _localPlayerWon = true;
            _winnerText.text = "You win!";
            SoundManager.Play(GameConstants.SoundNames.WIN, SoundManager.AudioTypes.Sfx);
        }
        else
        {
            _winnerText.text = "You lose!";
            SoundManager.Play(GameConstants.SoundNames.LOSE, SoundManager.AudioTypes.Sfx);
        }

        if (_localPlayerWon && (GameStats.Instance.Mode == GameEnums.GameModes.Pvp))
        {
            _replayUploadStatusText.text = LocalizationManager.GetTermTranslation(_PREPARING_REPLAY);
        }
        else
        {
            _replayUploadStatusText.gameObject.SetActive(false);
        }

        LocalizationManager.LocalizeText(_winnerText);

        string localTeamName = _warRef.LocalPlayerTeam;

        _damageDealtValue.text = GameNetwork.GetTeamRoomProperty(GameTeamRoomProperties.DAMAGE_DEALT, localTeamName);
        _damageReceivedValue.text = GameNetwork.GetTeamRoomProperty(GameTeamRoomProperties.DAMAGE_RECEIVED, localTeamName);
        _unitsKilledValue.text = GameNetwork.GetTeamRoomProperty(GameTeamRoomProperties.UNITS_KILLED, localTeamName);

        double matchDuration = double.Parse(NetworkManager.GetRoomCustomProperty(GameRoomProperties.MATCH_DURATION));
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(matchDuration);
        string matchDurationString = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        _matchDurationValue.text = matchDurationString;

        GameObject unitGridItemTemplate = _unitsAliveGridContent.GetChild(0).gameObject;

        string localTeam = _warRef.LocalPlayerTeam;
        string[] localTeamUnitNames = GameNetwork.GetTeamUnitNames(_warRef.LocalPlayerTeam);

        foreach(string unitName in localTeamUnitNames)
        {
            int unitHealth = GameNetwork.GetUnitRoomPropertyAsInt(UnitRoomProperties.HEALTH, localTeam, unitName);
            bool unitIsAlive = (unitHealth > 0);

            if (!unitIsAlive)
            {
                GameObject unit = Instantiate<GameObject>(unitGridItemTemplate, _unitsAliveGridContent);
                unit.GetComponent<UnitAliveGridItem>().SetUp(unitName, unitIsAlive);
            }
        }

        unitGridItemTemplate.SetActive(false);

        Dictionary<int, int> boxTiersWithAmountRewards = matchLocalData.BoxTiersWithAmountsRewards;
        GameObject unitRewardGridItemTemplate = _rewardsGridContent.Find("UnitsRewardGridItem").gameObject;
        GameObject boostRewardGridItemTemplate = _rewardsGridContent.Find("BoostRewardGridItem").gameObject;

        GameInventory gameInventory = GameInventory.Instance;

        foreach (KeyValuePair<int, int> amountByTier in boxTiersWithAmountRewards)
        {
            int count = amountByTier.Value;
            for (int i = 0; i < count; i++)
            {
                createRewardGridItem(unitRewardGridItemTemplate, amountByTier.Key, false);
                gameInventory.ChangeLootBoxAmount(amountByTier.Value, amountByTier.Key, true, false);
            }
        }

        if (boostItemGroup != null)
        {
            GameObject boostReward = Instantiate<GameObject>(boostRewardGridItemTemplate, _rewardsGridContent);
            boostReward.GetComponent<BoostRewardGridItem>().SetUp(boostItemGroup.Category, boostItemGroup.Bonus, true);
        }

        gameInventory.SaveLootBoxes();

        if (matchLocalData.EnjinCollected)
        {
            createRewardGridItem(unitRewardGridItemTemplate, BoxTiers.GOLD, true);
        }

        unitRewardGridItemTemplate.SetActive(false);
        boostRewardGridItemTemplate.SetActive(false);

        _resultsAreReady = true;
    }

    private void createRewardGridItem(GameObject rewardGridItemTemplate, int tier, bool isEnjin)
    {
        GameObject reward = Instantiate<GameObject>(rewardGridItemTemplate, _rewardsGridContent);
        reward.GetComponent<BoxRewardGridItem>().SetUp(tier, isEnjin);
    }
}
