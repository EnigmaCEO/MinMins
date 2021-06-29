using Enigma.CoreSystems;
using GameConstants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewLobby : NetworkEntity
{
    [SerializeField] private float _roomSetWithPvpDelay = 30;
    [SerializeField] private float _pingUpdatesDelay = 2;
    [SerializeField] private float _roomListUpdateDelay = 2;

    [SerializeField] private GameObject _waitingPopUp;

    [SerializeField] private Button _createRoomButton;
    //[SerializeField] private Button _searchRoomButton;
    [SerializeField] private InputField _privateRoomNameInput;

    [SerializeField] private Button _backButton;

    [SerializeField] private Transform _roomsGridContent;
    [SerializeField] private GameObject _roomsGridWindowBg;

    [SerializeField] private MessagePopUp _messagePopUp;
    [SerializeField] Text _pvprating;

    private bool _isJoiningRoom = false;

    override protected void Awake()
    {
        base.Awake();
        setDelegates();
    }

    void Start()
    {
        _pvprating.text = LocalizationManager.GetTermTranslation("PvP Rating") + ": " + GameStats.Instance.Rating;

        _waitingPopUp.SetActive(false);
        GameStats.Instance.UsesAiForPvp = false;

        GameObject roomGridItemTemplate = _roomsGridContent.GetChild(0).gameObject;
        roomGridItemTemplate.SetActive(false);

        _createRoomButton.onClick.AddListener(() => { onCreateButtonDown(); });
        //_searchRoomButton.onClick.AddListener(() => { onSearchButtonDown(); });
        _backButton.onClick.AddListener(() => { onBackButtonDown(); });

        _messagePopUp.OnDismissButtonDownCallback = onMessagePopUpDismiss;

        //displayTestGrid();
    }

    override protected void Update()
    {
        base.Update();

        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    _messagePopUp.Open(GameConstants.LobbyPopUpMessages.PRIVATE_ROOM_ALREADY_USED);
        //}

        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    NetworkManager.LeaveLobby();
        //    NetworkManager.LoadScene(EnigmaConstants.Scenes.MAIN);
        //}
    }

    private void OnDestroy()
    {
        Debug.LogWarning("Lobby::OnDestroy");
        StopAllCoroutines();
        removeDelegates();
    }

    private void setDelegates()
    {
        NetworkManager.OnJoinedLobbyCallback += onJoinedLobby;
        NetworkManager.OnJoinedRoomCallback += onJoinedRoom;
        NetworkManager.OnPlayerConnectedCallback += onPlayerConnected;
        //NetworkManager.OnReceivedRoomListUpdateCallback += onReceivedRoomListUpdate;
        NetworkManager.OnConnectedToMasterCallback += OnConnectedToMaster;

        NetworkManager.OnPlayerDisconnectedCallback += onPlayerDisconnected;
        NetworkManager.OnDisconnectedFromNetworkCallback += onDisconnectedFromNetwork;

        GameNetwork.Instance.OnPvpAiSetCallback += onPvpAiSet;
    }

    private void removeDelegates()
    {
        NetworkManager.OnJoinedLobbyCallback -= onJoinedLobby;
        NetworkManager.OnJoinedRoomCallback -= onJoinedRoom;
        NetworkManager.OnPlayerConnectedCallback -= onPlayerConnected;
        //NetworkManager.OnReceivedRoomListUpdateCallback -= onReceivedRoomListUpdate;
        NetworkManager.OnConnectedToMasterCallback -= OnConnectedToMaster;

        NetworkManager.OnPlayerDisconnectedCallback -= onPlayerDisconnected;
        NetworkManager.OnDisconnectedFromNetworkCallback -= onDisconnectedFromNetwork;

        if (GameNetwork.HasInstance())
        {
            GameNetwork.Instance.OnPvpAiSetCallback -= onPvpAiSet;
        }
    }

    private void onBackButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();

        NetworkManager.Disconnect();
        NetworkManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);
    }

    private void onCreateButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();

        Debug.Log("onCreateButtonDown");
        if (_privateRoomNameInput.text == "")
        {
            GameNetwork.Instance.CreatePublicRoom();
        }
        else
        {
            RoomInfo room = NetworkManager.GetLobbyRoomByName(_privateRoomNameInput.text);

            if (room == null)
            {
                GameNetwork.Instance.CreateRoom(_privateRoomNameInput.text);
            }
            else
            {
                _messagePopUp.Open(UiMessages.PRIVATE_ROOM_ALREADY_USED);
            }
        }
    }

    //private void onSearchButtonDown()
    //{
    //    Debug.Log("OnSearchButtonDown");
    //    if (_privateRoomNameInput.text != "")
    //    {
    //        refreshRoomsDisplay();
    //    }
    //    else
    //    {
    //        _messagePopUp.Open(GameConstants.LobbyPopUpMessages.PROVIDE_NAME_FOR_ROOM_SEARCH);
    //    }
    //}

    private void onMessagePopUpDismiss(string message)
    {
        GameSounds.Instance.PlayUiBackSound();

        if (message == UiMessages.PRIVATE_ROOM_ALREADY_USED)
        {
            _privateRoomNameInput.text = "";
        }
        //else if ((message == GameConstants.LobbyPopUpMessages.NO_PRIVATE_WITH_NAME_FOUND) || (message == GameConstants.LobbyPopUpMessages.PROVIDE_NAME_FOR_ROOM_SEARCH))
        //{
        //    refreshRoomsDisplay();
        //}
    }

    private void onPvpAiSet(bool enabled)
    {
        Debug.LogWarning("Lobby::onPvpAiSet  -> pvp ai enabled: " + enabled);

        if (NetworkManager.GetIsMasterClient())
        {
            Debug.LogWarning("Lobby::onPvpAiSet  -> Is Master Client: " + NetworkManager.GetIsMasterClient());

            if (enabled)
            {
                Debug.LogWarning("Lobby::onPvpAiSet  ->  startMach");
                startMatch();
            }
        }
    }

    private void OnConnectedToMaster()
    {
        print("Lobby::OnConnectedToMaster");
        NetworkManager.JoinLobby();
    }

    private void onJoinedLobby()
    {
        Debug.LogWarning("Lobby::OnJoinedLobby");
        NetworkManager.GetRoomList();  //Request room list. Wait for OnReceivedRoomListUpdate

        StartCoroutine(handleRoomListUpdate());
        StartCoroutine(handlePingUpdates());
    }

    //Will no longer be received after entering a room
    //private void onReceivedRoomListUpdate()
    //{
    //    Debug.LogWarning("Lobby::OnReceivedRoomListUpdate");
    //    refreshRoomsDisplay("");

    //    //if (!_isJoiningRoom)
    //    //{
    //    //    handleRoomCreationAndJoin();
    //    //}
    //}

    private IEnumerator handleRoomListUpdate()
    {
        while (true)
        {
            refreshRoomsDisplay();
            yield return new WaitForSeconds(_roomListUpdateDelay);
        }
    }

    private IEnumerator handlePingUpdates()
    {
        while (true)
        {
            GameObject roomGridItemTemplate = _roomsGridContent.GetChild(0).gameObject;

            RoomInfo[] rooms = NetworkManager.GetRoomList();

            int roomsCount = rooms.Length;
            for (int i = 0; i < roomsCount; i++)
            {
                RoomInfo room = rooms[i];
                string roomName = room.Name;
                bool roomHasGuest = (room.PlayerCount > 1);

                foreach (Transform child in _roomsGridContent)
                {
                    if (child.gameObject != roomGridItemTemplate)
                    {
                        if (child.name == roomName)
                        {
                            int hostPing = (int)room.CustomProperties[GameNetwork.RoomCustomProperties.HOST_PING];
                            child.GetComponent<PvpRoomGridItem>().UpdateHostPing(hostPing);

                            if (roomHasGuest)
                            {
                                int guestPing = (int)room.CustomProperties[GameNetwork.RoomCustomProperties.GUEST_PING];
                                child.GetComponent<PvpRoomGridItem>().UpdateGuestPing(guestPing);
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(_pingUpdatesDelay);
        }
    }

    private void refreshRoomsDisplay()
    {
        string privateSearchRoomName = _privateRoomNameInput.text;

        bool isPrivateSearch = (privateSearchRoomName != "");
        //bool privateRoomFound = false;

        GameObject roomGridItemTemplate = _roomsGridContent.GetChild(0).gameObject;
        roomGridItemTemplate.SetActive(true);

        foreach (Transform child in _roomsGridContent)
        {
            if (child.gameObject != roomGridItemTemplate)
                Destroy(child.gameObject);
        }

        RoomInfo[] rooms = NetworkManager.GetRoomList();

        int roomsCount = rooms.Length;
        Debug.LogWarning("NewLobby::refreshRoomsDisplay -> roomsCount: " + roomsCount);
        for (int i = 0; i < roomsCount; i++)
        {
            RoomInfo room = rooms[i];

            string roomIsPrivate = (string)room.CustomProperties[GameNetwork.RoomCustomProperties.IS_PRIVATE];
            Debug.LogWarning("NewLobby::refreshRoomsDisplay -> room wiht index: " + i + " and name: " + room.Name + " isPrivate: " + roomIsPrivate);

            if (roomIsPrivate == null)
            {
                roomIsPrivate = "False"; //TODO: Consider removing it later.
                Debug.LogWarning("NewLobby::refreshRoomsDisplay -> isPrivate is null. Setting it to false.");
            }

            string roomName = room.Name;

            //Display room only if it is not private
            if ((!isPrivateSearch && (roomIsPrivate == "False")) || (isPrivateSearch && (roomName == privateSearchRoomName)))
            {            
                string hostName = (string)room.CustomProperties[GameNetwork.RoomCustomProperties.HOST_NAME];
                int hostRating = (int)room.CustomProperties[GameNetwork.RoomCustomProperties.HOST_RATING];
                int hostPing = (int)room.CustomProperties[GameNetwork.RoomCustomProperties.HOST_PING];

                string guestName = "";
                int guestRating = -1;
                int guestPing = -1;

                int roomPlayersCount = room.PlayerCount;

                if (roomPlayersCount > 1)
                {
                    guestName = (string)room.CustomProperties[GameNetwork.RoomCustomProperties.GUEST_NAME];
                    guestRating = (int)room.CustomProperties[GameNetwork.RoomCustomProperties.GUEST_RATING];
                    guestPing = (int)room.CustomProperties[GameNetwork.RoomCustomProperties.GUEST_PING];
                }

                string hasAI = (string)room.CustomProperties[GameNetwork.RoomCustomProperties.HAS_PVP_AI];

                if (hasAI == "True")
                {
                    guestName = (string)room.CustomProperties[GameNetwork.RoomCustomProperties.GUEST_NAME];
                }

                Debug.LogWarning("Creating room grid item with data: roomName: " + roomName + " hostName: " + hostName + " hostRating: " + hostRating + " guestName: " + guestName + " guestRating: " + guestRating);

                Transform roomItemTransform = Instantiate<GameObject>(roomGridItemTemplate, _roomsGridContent).transform;
                roomItemTransform.name = roomName;
                PvpRoomGridItem roomGridItem = roomItemTransform.GetComponent<PvpRoomGridItem>();
                roomGridItem.SetUp(hostName, hostRating, hostPing, guestName, guestRating, guestPing, roomName);
                roomGridItem.GetComponent<Button>().onClick.AddListener(() => onRoomGridItemClicked(roomName));

                if (isPrivateSearch && (roomIsPrivate == "True"))
                {
                    //privateRoomFound = true;
                    break; //Because only one private room with given name should be found/displayed
                }
            }
        }

        roomGridItemTemplate.SetActive(false);

        //if (isPrivateSearch && (privateRoomFound == false))
        //{
        //    _messagePopUp.Open(GameConstants.LobbyPopUpMessages.NO_PRIVATE_WITH_NAME_FOUND);
        //}
    }

    private void displayTestGrid()
    {
        GameObject roomGridItemTemplate = _roomsGridContent.GetChild(0).gameObject;
        roomGridItemTemplate.SetActive(true);

        foreach (Transform child in _roomsGridContent)
        {
            if (child.gameObject != roomGridItemTemplate)
                Destroy(child.gameObject);
        }

        for (int i = 0; i < 10; i++)
        {
            string roomName = "roomName " + i;
            Transform roomItemTransform = Instantiate<GameObject>(roomGridItemTemplate, _roomsGridContent).transform;
            roomItemTransform.name = roomName;
            PvpRoomGridItem roomGridItem = roomItemTransform.GetComponent<PvpRoomGridItem>();
            roomGridItem.SetUp("hostName " + i, 1, 100, "guestName " + i, 2, 200, "roomName " + i);
            roomGridItem.GetComponent<Button>().onClick.AddListener(() => onRoomGridItemClicked(roomName));
        }

        roomGridItemTemplate.SetActive(false);
    }

    private void onRoomGridItemClicked(string roomName)
    {
        Debug.LogWarning("onRoomGridItemClicked");

        RoomInfo roomInfo = NetworkManager.GetLobbyRoomByName(roomName);
        string hasPvpAI = (string)roomInfo.CustomProperties[GameNetwork.RoomCustomProperties.HAS_PVP_AI];

        if ((roomInfo.PlayerCount < 2) && (hasPvpAI == "False"))
        {
            NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.SPECTATING, "False", GameNetwork.TeamNames.GUEST);
            NetworkManager.JoinRoom(roomName);
        }
        //else
        //{
        //    NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.SPECTATING, "True", GameNetwork.TeamNames.SPECTATOR);
        //}

        //NetworkManager.JoinRoom(roomName);  //Later when spectator mode is implemented
    }

    public void sendStartMatch()
    {
        Debug.LogWarning("Lobby::sendStartMatch");
        base.SendRpcToAll(nameof(receiveStartMatch));
    }

    [PunRPC]
    private void receiveStartMatch()
    {
        Debug.LogWarning("Lobby::receiveStartMatch");

        startMatch();
    }

    private void startMatch()
    {
        Debug.LogWarning("startMatch");
        SceneManager.LoadScene(GameConstants.Scenes.WAR, true, true);
    }

    private void onJoinedRoom()
    {
        Debug.LogWarning("Lobby::OnJoinedRoom -> Is Master Client: " + NetworkManager.GetIsMasterClient());

        _roomsGridContent.gameObject.SetActive(false);
        _roomsGridWindowBg.SetActive(false);

        _privateRoomNameInput.gameObject.SetActive(false);
        _createRoomButton.gameObject.SetActive(false);
        //_searchRoomButton.gameObject.SetActive(false);

        _backButton.gameObject.SetActive(false);

        StopCoroutine(handlePingUpdates());
        StopCoroutine(handleRoomListUpdate());

        GameNetwork gameNetwork = GameNetwork.Instance;

        if (NetworkManager.GetIsMasterClient())
        {
            Room room = NetworkManager.GetRoom();
            print("Lobby::OnJoinedRoom ->  Joined first as master client in room.Name: " + room.Name + " and room.CustomProperties: " + room.CustomProperties.ToStringFull());
            gameNetwork.HostPlayerId = NetworkManager.GetLocalPlayerId();
            NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.HOST_ID, GameNetwork.Instance.HostPlayerId);
            NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.TIME_ROOM_STARTED, NetworkManager.GetNetworkTime());
            NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.SPECTATING, "false", GameNetwork.TeamNames.HOST);

            _waitingPopUp.SetActive(true);

            bool roomIsPrivate = (((string)room.CustomProperties[GameNetwork.RoomCustomProperties.IS_PRIVATE]) == "True");

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (GameHacks.Instance.ForcePvpAi)
            {
                setRoomWithPvpAi();
            }
            else
#endif
            {
                if (!roomIsPrivate)
                {
                    StartCoroutine(handleWaitForPvpAiSet());
                }
            }
        }
        else
        {
            double timeRoomStarted = double.Parse(NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.TIME_ROOM_STARTED));
            double currentTime = NetworkManager.GetNetworkTime();

            double timeToWaitAi = double.Parse(_roomSetWithPvpDelay.ToString());

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (GameHacks.Instance.TimeWaitForRoomPvpAiSet.Enabled)
            {
                timeToWaitAi = GameHacks.Instance.TimeWaitForRoomPvpAiSet.ValueAsFloat;
            }
#endif

            double timeDiff = currentTime - timeRoomStarted;

            Debug.LogWarning("Lobby::onJoinedRoom -> timeRoomStarted: " + timeRoomStarted + " currentTime: " + currentTime + "timeDiff: " + timeDiff + " timeToWaitAi: " + timeToWaitAi);

            if (timeDiff > timeToWaitAi)
            {
                Debug.LogWarning("Lobby::onJoinedRoom -> leftRoom that had Ai");
                NetworkManager.LeaveRoom();
                StopCoroutine(handleWaitForPvpAiSet());
                NetworkManager.LoadScene(GameConstants.Scenes.LOBBY);
            }
            else
            {
                int guestRating = GameStats.Instance.Rating;

                NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.GUEST_NAME, NetworkManager.GetPlayerName());
                NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.GUEST_RATING, guestRating);

                gameNetwork.GuestPlayerId = NetworkManager.GetLocalPlayerId();
                gameNetwork.HostPlayerId = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.HOST_ID);

                Debug.LogWarning("Lobby::onJoinedRoom -> joined room as guest id: " + gameNetwork.GuestPlayerId);

                GameNetwork.SetLocalPlayerRating(guestRating, GameNetwork.TeamNames.GUEST);
                NetworkManager.GetRoom().IsOpen = false;
            }
        }

        Debug.LogWarning("Lobby::OnJoinedRoom -> HostPlayerId: " + gameNetwork.HostPlayerId + " GuestPlayerId: " + gameNetwork.GuestPlayerId);
    }

    private void onPlayerDisconnected(int disconnectedPlayerId)
    {
        handleDisconnection();
    }

    private void onDisconnectedFromNetwork()
    {
        handleDisconnection();
    }

    private void handleDisconnection()
    {
        StopAllCoroutines();
        CancelInvoke();
    }

    private void onPlayerConnected(int networkPlayerId)
    {
        print("Lobby::onPlayerConnected -> networkPlayerId " + networkPlayerId);

        if (GameStats.Instance.UsesAiForPvp)
        {
            Debug.LogWarning("Lobby::onPlayerConnected -> Player connected when Ai was already set. Ignoring pvp setup.");
        }
        else
        {
            StopCoroutine(handleWaitForPvpAiSet());

            GameNetwork gameNetwork = GameNetwork.Instance;
            GameNetwork.Instance.GuestPlayerId = networkPlayerId;
            Debug.LogWarning("Lobby::onPlayerConnected -> HostPlayerId: " + gameNetwork.HostPlayerId + " GuestPlayerId: " + gameNetwork.GuestPlayerId);
            sendStartMatch();
        }
    }

    private void setRoomWithPvpAi()
    {
        Debug.LogWarning("setRoomWithPvpAi");
        GameStats.Instance.UsesAiForPvp = true;
        GameNetwork.Instance.GuestPlayerId = NetworkManager.GetLocalPlayerId();
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.HAS_PVP_AI, "True");
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.GUEST_NAME, NetworkManager.GetRandomOnlineName());
    }

    private IEnumerator handleWaitForPvpAiSet()
    {
        float timeToWait = _roomSetWithPvpDelay;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.TimeWaitForRoomPvpAiSet.Enabled)
        {
            timeToWait = GameHacks.Instance.TimeWaitForRoomPvpAiSet.ValueAsFloat;
        }
#endif

        Debug.LogWarning("handleWaitForPvpAiSet -> timeToWait: " + timeToWait);

        yield return new WaitForSeconds(timeToWait);
        setRoomWithPvpAi();
    }
}
