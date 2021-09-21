using Enigma.CoreSystems;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CodeStage.AntiCheat.Storage;
using GameEnums;
using UnityEngine.UDP;
using System;
using SimpleJSON;
using System.Collections.Generic;
using GameConstants;

public class Main : EnigmaScene
{
    [SerializeField] GameObject _loginModal;
    [SerializeField] BasicPopUp _loginForPvpPopUp;
    //[SerializeField] LoginForGameModePopUp _loginForQuestPopUp;
    [SerializeField] GameObject _restorePopUp;
    [SerializeField] RewardsInventoryPopUp _rewardsInventoryPopUp;

    [SerializeField] GameObject _enjinWindow;
    [SerializeField] private int _checkEnjinLinkingDelay = 2;
    [SerializeField] GameObject _loginButton;
    [SerializeField] GameObject _logoutButton;
    [SerializeField] Button _restoreButton;
    [SerializeField] GameObject _rewardsInventoryButton;
    [SerializeField] GameObject _enjinIcon;
    [SerializeField] Transform _enjinTokensContent;
    [SerializeField] GameObject _enjinTokenList;

    private GameObject _enjinTokenTemplate;

    //[SerializeField] Text _pvprating;

    //[SerializeField] Text _crystalsAmount;

    //private Dictionary<Quests, string> _rewardIconResNameByQuest = new Dictionary<Quests, string>();


    void Start()
    {
        //populateIconPathbyQuest();
        //NetworkManager.Transaction(GameNetwork.Transactions.GET_QUEST_DATA, onGetQuestData);

        if (!NetworkManager.LoggedIn)
        {
            GameNetwork.Instance.ResetLoginValues();
        }

        _enjinTokenTemplate = _enjinTokensContent.GetChild(0).gameObject;
        _enjinTokenTemplate.transform.SetParent(_enjinTokensContent.parent);
        _enjinTokenTemplate.SetActive(false);

        _enjinTokenList.SetActive(false);

        SoundManager.Stop();
        SoundManager.MusicVolume = 0.5f;
        SoundManager.Play("main", SoundManager.AudioTypes.Music, "", true);

        NetworkManager.Disconnect();

        _restorePopUp.SetActive(false);
        _loginModal.SetActive(false);
        _enjinIcon.SetActive(false);
        _enjinWindow.SetActive(false);
        _rewardsInventoryPopUp.Close();

        _restoreButton.onClick.AddListener(delegate { OnRestoreButtonDown(); });
        _restoreButton.gameObject.SetActive(false);

#if HUAWEI
        _crystalsAmount.text = GameInventory.Instance.GetCrystalsAmount().ToString();
#else
        //_crystalsAmount.transform.parent.gameObject.SetActive(false);
#endif

        //_pvprating.text = "";
        /*
                _kinWrapper = GameObject.Find("/KinManager").GetComponent<KinManager>();

                _kinWrapper.RegisterCallback((obj, val) =>
                {
                    //_pvprating.text += "\n" + obj.ToString();

                    if (obj.ToString() == "Account funded")
                    {
                        Text text = GameObject.Find("/Canvas/kin_icon/Text").GetComponent<Text>();
                        text.text = "50";
                        ObscuredPrefs.SetFloat("KinBalanceUser", 50f);
                    }


                });




                string kin = _kinWrapper.GetUserPublicAddress();

                if (kin != null && kin != "")
                {
                    Debug.Log(kin);


                    decimal balance = _kinWrapper.GetBalance();
                    if (balance == 0)
                    {
                        //_pvprating.text += kin + "\n" + balance;
                        _kinPopUp.SetActive(true);
                        //ObscurePrefs.SetString("Kin", "2");
                        //_kinWrapper.FundKin();
                    } else
                    {

                        Text text = GameObject.Find("/Canvas/kin_icon/Text").GetComponent<Text>();
                        text.text = balance.ToString();

                    }
                }
                else
                    _kinPopUp.SetActive(false);
        */
        Init();

        if (!GameConfig.Instance.EnableServerBackup)  //If we use server backup then we call this inside Init, that is called at start and at login
        {
            updateRestoreButton();
            handleRestorePopUp();
        }
    }

    //private void populateIconPathbyQuest()
    //{
    //    _rewardIconResNameByQuest.Add(Quests.EnjinLegend122, "122");
    //    _rewardIconResNameByQuest.Add(Quests.EnjinLegend123, "123");
    //    _rewardIconResNameByQuest.Add(Quests.EnjinLegend124, "124");
    //    _rewardIconResNameByQuest.Add(Quests.EnjinLegend125, "125");
    //    _rewardIconResNameByQuest.Add(Quests.EnjinLegend126, "126");
    //}

    public void Init()
    {
        bool loggedIn = NetworkManager.LoggedIn;

        //if (NetworkManager.LoggedIn)
        //{
        //    NetworkManager.Transaction(NetworkManager.Transactions.GIFT_PROGRESS, new Hashtable(), onGiftProgress);
        //}
        //else
        //{
        //    _questProgressPanel.SetActive(false);
        //}

        _rewardsInventoryButton.SetActive(loggedIn);

        _loginButton.gameObject.SetActive(!loggedIn);
        _logoutButton.gameObject.SetActive(loggedIn);

        _loginModal.GetComponent<EnjinLogin>().Initialize();

        if (!EnigmaHacks.Instance.PreventAutoLogin)
        {
            if (!NetworkManager.LoggedIn && _loginModal.GetComponent<EnjinLogin>().loginUsername.text != "" && _loginModal.GetComponent<EnjinLogin>().loginPassword.text != "")
            {
                _loginModal.GetComponent<EnjinLogin>().loginSubmit();
            }
        }

        //_kinPopUp.SetActive(true);

        //=========== Only needed here if using logged backup because server could have a backup when prefs do not (new phone) =====
        if (GameConfig.Instance.EnableServerBackup)
        {
            updateRestoreButton();
            handleRestorePopUp();
        }
        //===========================================================================================================================

        if (loggedIn)
        {
            UpdateEnjinDisplay();
        }

        //if (loggedIn)
        //{
        //    _pvprating.text = LocalizationManager.GetTermTranslation("PvP Rating") + ": " + GameStats.Instance.Rating;
        //}

    }

    public void OnRewardsInventoryButtonDown()
    {
        _rewardsInventoryPopUp.Open();
    }

    public void OnEnjinButtonDown()
    {
        if (_enjinTokenList.activeSelf)
        {
            GameSounds.Instance.PlayUiBackSound();
            _enjinTokenList.SetActive(false);
        }
        else
        {
            GameSounds.Instance.PlayUiAdvanceSound();
            _enjinTokenList.SetActive(true);
        }
    }

    public void OnRestoreButtonDown()
    {
        GameInventory.Instance.LoadBackupSave();
        _restoreButton.gameObject.SetActive(false);

        if (_restorePopUp.GetActive())
        {
            _restorePopUp.SetActive(false);
        }
    }

    public void OnRestorePopUpDismissButtonDown()
    {
        _restorePopUp.SetActive(false);
    }

    public void OnSinglePlayerButtonDown()
    {
        print("OnSinglePlayerButtonDown");
        GameSounds.Instance.PlayUiAdvanceSound();
        GameStats.Instance.Mode = GameModes.SinglePlayer;
        goToLevels();
    }

    public void OnPvpButtonDown()
    {
        print("OnPvpButtonDown");
        GameSounds.Instance.PlayUiAdvanceSound();

        if (NetworkManager.LoggedIn)
        {
            GameStats.Instance.Mode = GameModes.Pvp;
            goToLevels();
        }
        else
        {
            _loginForPvpPopUp.Open();
        }
    }

    public void OnStoreButtonDown()
    {
        print("OnStoreButtonDown");
        GameSounds.Instance.PlayUiAdvanceSound();
        SceneManager.LoadScene(GameConstants.Scenes.STORE);
    }

    public void OnQuestsButtonDown()
    {
        print("OnQuestsButtonDown");

        GameSounds.Instance.PlayUiAdvanceSound();
        SceneManager.LoadScene(GameConstants.Scenes.QUEST_SELECTION);
    }

    public void ShowLoginForm()
    {
        GameSounds.Instance.PlayUiAdvanceSound();
        _loginModal.SetActive(true);
        _loginModal.GetComponent<EnjinLogin>().resetForm();
    }

    private void goToLevels()
    {
        SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
    }

    public void StartEnjinQR(string enjinCode) 
    {
        _enjinWindow.SetActive(true);
        _enjinWindow.GetComponent<EnjinQRManager>().ShowImage(enjinCode);
        Debug.Log("Checking server for Link1");
        StartCoroutine(handleEnjinLinkingCheck(1));
    }

    public void OnEnjinWindowExitButtonDown()
    {
        //Enjin.CleanUpPlatform();
        StopCoroutine("handleEnjinLinkingCheck");
        GameSounds.Instance.PlayUiBackSound();
        _enjinWindow.gameObject.SetActive(false);
    }

    private IEnumerator handleEnjinLinkingCheck(int delay)
    {
        Debug.Log("Checking server for Link2 " + delay);
        yield return new WaitForSeconds(delay);
        Debug.Log("Checking server for Link3");
        Hashtable val = new Hashtable();
        val.Add(NetworkManager.TransactionKeys.GAME, GameNetwork.TRANSACTION_GAME_NAME);
        NetworkManager.Transaction(EnigmaConstants.Transactions.ENJIN_LINKED, val, onLinkedTransaction);

        yield break;
    }

    private void handleRestorePopUp()
    {
        if (_restoreButton.gameObject.GetActive())
        {
            _restorePopUp.SetActive(true);
        }
    }

    private void updateRestoreButton()
    {
        _restoreButton.gameObject.SetActive(false);
        GameInventory gameInventory = GameInventory.Instance;

        if ((GameConfig.Instance.EnableServerBackup && NetworkManager.LoggedIn && GameStats.Instance.IsThereServerBackup)
            || GameInventory.Instance.IsTherePrefsBackupSave())
        {
            if (FileManager.Instance.CheckFileNullOrEmpty())
            {
                _restoreButton.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("updateRestoreButton ->  There is a saved file.");
            }
        }
        else
        {
            Debug.Log("updateRestoreButton ->  Restore is not available.");
        }
    }

    //private void onGiftProgress(SimpleJSON.JSONNode response)
    //{
    //    if (NetworkManager.CheckInvalidServerResponse(response, nameof(onGiftProgress)))
    //    {
    //        return;
    //    }

    //    SimpleJSON.JSONNode response_hash = response[0];

    //    string progress = response_hash["progress"].ToString().Trim('"');
    //    _questProgressFill.fillAmount = float.Parse(progress) / 1000.0f;
    //    _questProgressText.text = "$" + progress + " / $1000";

    //    _questProgressPanel.SetActive(true);
    //}

    private void onLinkedTransaction(SimpleJSON.JSONNode response)
    {
        if (NetworkManager.CheckInvalidServerResponse(response, nameof(onLinkedTransaction)))
        {
            return;
        }

        SimpleJSON.JSONNode response_hash = response[0];

        string enjinCode = response_hash["user_data"]["enjin_code"].ToString().Trim('"');
        if (enjinCode == "null")
        {
            GameNetwork.Instance.IsEnjinLinked = true;
            //enjinSupport.gameObject.SetActive(true);

            if (_enjinIcon != null)
            {
                _enjinIcon.SetActive(true);
            }

            //updateEnjinItems(response_hash);
            GameNetwork.Instance.UpdateEnjinGoodies(response_hash, true);
            UpdateEnjinDisplay();

            if (_enjinWindow != null)
            {
                _enjinWindow.gameObject.SetActive(false);
            }
        }
        else if ((_enjinWindow != null) && _enjinWindow.GetActive())
        {
            StartCoroutine(handleEnjinLinkingCheck(_checkEnjinLinkingDelay));
        }

        //else
        //{
        //    _errorText.text = UIManager.GetLocalizedText("Unable to check Enjin Linking at the moment. Retrying...");
        //    _errorText.gameObject.SetActive(true);
        //    StartCoroutine(handleEnjinLinkingCheck(_checkEnjinLinkingDelay));
        //}
    }

 
    //private void updateEnjinItems(SimpleJSON.JSONNode response_hash)
    //{

    //    SimpleJSON.JSONNode userData = response_hash["user_data"];

    //    //GameStats.Instance.HasEnjinWeapon = (userData["enjin_hammer"].AsInt == 1);
    //    //GameStats.Instance.HasEnjinShield = (userData["enjin_shield"].AsInt == 1);
    //    //GameStats.Instance.HasEnjinEnigmaToken = (userData["enigma_token"].AsInt == 1);


    //    //Enjin gear Hack =============================
    //    //PlayerStats.HasEnjinWeapon = true;  
    //    //PlayerStats.HasEnjinShield = true;
    //    //PlayerStats.HasEnjinShalwendToken = true;
    //    //PlayerStats.HasEnjinEnigmaToken = true;
    //    //=============================================

    //    /*if (PlayerStats.HasEnjinEnigmaToken)
    //    {
    //        enigmaMFT.gameObject.SetActive(true);
    //    }


    //    if (PlayerStats.HasEnjinShalwendToken)
    //    {
    //        for (int i = 0; i < data.m_BoughtWeapon.Length; i++)
    //            data.m_BoughtWeapon[i] = 1;

    //        for (int i = 0; i < data.m_BoughtShield.Length; i++)
    //            data.m_BoughtShield[i] = 1;

    //        shalwendMFT.gameObject.SetActive(true);
    //    }

    //    if (PlayerStats.HasEnjinWeapon)
    //    {
    //        if (data.m_Weapon == 3)
    //            data.m_Weapon = 4;

    //        data.m_BoughtWeapon[3] = 1;

    //        enjinHammer.gameObject.SetActive(true);
    //    }

    //    if (PlayerStats.HasEnjinShield)
    //    {
    //        if (data.m_Shield == 3)
    //            data.m_Shield = 4;

    //        data.m_BoughtShield[3] = 1;

    //        enjinShield.gameObject.SetActive(true);
    //    }

    //    data.SaveData();*/
    //}

    public void closeQRDialog()
    {
        GameSounds.Instance.PlayUiBackSound();
        _enjinWindow.SetActive(false);
    }

    public void Logout()
    {
        //_questButton.gameObject.SetActive(false);
        //_questProgressPanel.SetActive(false);

        NetworkManager.Logout();

        GameSounds.Instance.PlayUiBackSound();

        GameNetwork.Instance.ResetLoginValues();

        _loginButton.gameObject.SetActive(true);
        _logoutButton.gameObject.SetActive(false);

        //Text text = _enjinIcon.GetComponentInChildren<Text>();
        //text.text = "";
        _enjinTokensContent.DestroyChildren();
        _enjinIcon.SetActive(false);
        _rewardsInventoryButton.SetActive(false);

        //_pvprating.text = "";
        StopCoroutine("handleEnjinLinkingCheck");
        _enjinWindow.gameObject.SetActive(false);
    }

    private void addToEnjinItemDisplay(string enjinKey)
    {
        if (GameNetwork.Instance.GetIsEnjinKeyAvailable(enjinKey))
        {
            GameObject newEnjinTokenItem = Instantiate<GameObject>(_enjinTokenTemplate, _enjinTokensContent);
            string displayName = GameInventory.Instance.GetEnjinKeyDisplayName(enjinKey, false);
            newEnjinTokenItem.name = displayName;
            newEnjinTokenItem.GetComponent<Text>().text = displayName;
            newEnjinTokenItem.SetActive(true);
        }
    }

    //private void addToEnjinItemDisplay(Text text, bool flag, string label)
    //{
    //    if (flag)
    //    {
    //        text.text += "\n" + label;
    //    }
    //}

    public void UpdateEnjinDisplay() 
    {
        GameNetwork gameNetwork = GameNetwork.Instance;

        //if (NetworkManager.LoggedIn)
        //{
        //    Quests activeQuest = GameStats.Instance.ActiveQuest;
        //    _questButton.SetActive(activeQuest != Quests.None);

        //    if (_questButton.activeInHierarchy)
        //    {
        //        _questButton.transform.Find("QuestName").GetComponent<Text>().text = activeQuest.ToString();

        //        string iconPath = "Images/Quests/" + activeQuest.ToString() + " Icon";
        //        Sprite questIcon = (Sprite)Resources.Load<Sprite>(iconPath);

        //        if (questIcon != null)
        //        {
        //            _questButton.transform.Find("icon").GetComponent<Image>().sprite = questIcon;
        //        }
        //        else
        //        {
        //            Debug.Log("Quest Icon image was not found at path: " + iconPath + " . Please check active quest is correct and image is in the right path.");
        //        }
        //    }
        //}

        if (gameNetwork.IsEnjinLinked)
        {
            _enjinIcon.SetActive(true);

            _enjinTokensContent.DestroyChildren();
            //Text text = _enjinIcon.GetComponentInChildren<Text>();
            //text.text = ""; 

            GameInventory gameInventory = GameInventory.Instance;

            addToEnjinItemDisplay(EnigmaConstants.EnjinTokenKeys.ENJIN_MFT);
            addToEnjinItemDisplay(EnigmaConstants.EnjinTokenKeys.ENIGMA_TOKEN);

            addToEnjinItemDisplay(EnjinTokenKeys.MINMINS_TOKEN);

            addToEnjinItemDisplay(EnjinTokenKeys.QUEST_DEADLY_KNIGHT_SHALWEND);
            addToEnjinItemDisplay(EnjinTokenKeys.QUEST_WARGOD_SHALWEND);

            addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_MAXIM);
            addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_WITEK);
            addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_BRYANA);
            addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_TASSIO);
            addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_SIMON);

            addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_ESTHER);
            addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_ALEX);
            addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_EVAN);
            addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_LIZZ);
            addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_BRAD);

            addToEnjinItemDisplay(EnjinTokenKeys.SHALWEND_DEADLY_KNIGHT);
            addToEnjinItemDisplay(EnjinTokenKeys.KNIGHT_HEALER);
            addToEnjinItemDisplay(EnjinTokenKeys.KNIGHT_BOMBER);
            addToEnjinItemDisplay(EnjinTokenKeys.KNIGHT_DESTROYER);
            addToEnjinItemDisplay(EnjinTokenKeys.KNIGHT_SCOUT);
            addToEnjinItemDisplay(EnjinTokenKeys.KNIGHT_TANK);

            addToEnjinItemDisplay(EnjinTokenKeys.DEMON_HEALER);
            addToEnjinItemDisplay(EnjinTokenKeys.DEMON_BOMBER);
            addToEnjinItemDisplay(EnjinTokenKeys.DEMON_DESTROYER);
            addToEnjinItemDisplay(EnjinTokenKeys.DEMON_SCOUT);
            addToEnjinItemDisplay(EnjinTokenKeys.DEMON_TANK);

            addToEnjinItemDisplay(EnjinTokenKeys.NARWHAL_BLUE);
            addToEnjinItemDisplay(EnjinTokenKeys.NARWHAL_CHEESE);
            addToEnjinItemDisplay(EnjinTokenKeys.NARWHAL_EMERALD);
            addToEnjinItemDisplay(EnjinTokenKeys.NARWHAL_CRIMSON);
            addToEnjinItemDisplay(EnjinTokenKeys.NARWHAL_DIAMOND);

            addToEnjinItemDisplay(EnjinTokenKeys.SWISSBORG_CYBORG);
            addToEnjinItemDisplay(EnjinTokenKeys.SHALWEND_WARGOD);
            addToEnjinItemDisplay(EnjinTokenKeys.GOD_ENIGMA);
            addToEnjinItemDisplay(EnjinTokenKeys.GOD_BOMBER);
            addToEnjinItemDisplay(EnjinTokenKeys.GOD_DESTROYER_1);
            addToEnjinItemDisplay(EnjinTokenKeys.GOD_DESTROYER_2);
            addToEnjinItemDisplay(EnjinTokenKeys.GOD_HEALER);
            addToEnjinItemDisplay(EnjinTokenKeys.GOD_SCOUT);
            addToEnjinItemDisplay(EnjinTokenKeys.GOD_TANK_1);
            addToEnjinItemDisplay(EnjinTokenKeys.GOD_TANK_2);

            addToEnjinItemDisplay(EnjinTokenKeys.COZY_BLOBBY);
            addToEnjinItemDisplay(EnjinTokenKeys.ENJINEER_BLOBBY);
            addToEnjinItemDisplay(EnjinTokenKeys.VR_BLOBBY);

            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_1, "Damage Ore +1");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_2, "Damage Ore +2");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_3, "Damage Ore +3");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_4, "Damage Ore +4");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_5, "Damage Ore +5");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_6, "Damage Ore +6");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_7, "Damage Ore +7");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_8, "Damage Ore +8");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_9, "Damage Ore +9");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_10, "Damage Ore +10");

            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_1, "Defense Ore +1");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_2, "Defense Ore +2");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_3, "Defense Ore +3");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_4, "Defense Ore +4");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_5, "Defense Ore +5");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_6, "Defense Ore +6");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_7, "Defense Ore +7");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_8, "Defense Ore +8");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_9, "Defense Ore +9");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_10, "Defense Ore +10");

            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_1, "Health Ore +1");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_2, "Health Ore +2");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_3, "Health Ore +3");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_4, "Health Ore +4");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_5, "Health Ore +5");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_6, "Health Ore +6");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_7, "Health Ore +7");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_8, "Health Ore +8");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_9, "Health Ore +9");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_10, "Health Ore +10");

            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_1, "Power Ore +1");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_2, "Power Ore +2");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_3, "Power Ore +3");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_4, "Power Ore +4");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_5, "Power Ore +5");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_6, "Power Ore +6");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_7, "Power Ore +7");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_8, "Power Ore +8");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_9, "Power Ore +9");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_10, "Power Ore +10");

            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_1, "Size Ore +1");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_2, "Size Ore +2");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_3, "Size Ore +3");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_4, "Size Ore +4");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_5, "Size Ore +5");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_6, "Size Ore +6");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_7, "Size Ore +7");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_8, "Size Ore +8");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_9, "Size Ore +9");
            //addToEnjinItemDisplay(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_10, "Size Ore +10");
        }
    }
}
