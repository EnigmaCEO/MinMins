using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using Tiny;
using UnityEngine;

public class Lobby : NetworkEntity
{
    [SerializeField] private int _maxRatingDifferenceToFight = 50;
    [SerializeField] private float _roomSetWithPvpDelay = 30; 

    [SerializeField] private GameObject _waitingPopUp;

    private bool _isJoiningRoom = false;

    override protected void Awake()
    {
        base.Awake();
        setDelegates();
    }

    void Start()
    {
        _waitingPopUp.SetActive(false);   
        GameStats.Instance.UsesAiForPvp = false;
    }

    override protected void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            NetworkManager.LeaveLobby();
            NetworkManager.LoadScene(EnigmaConstants.Scenes.MAIN);
        }
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
        NetworkManager.OnReceivedRoomListUpdateCallback += onReceivedRoomListUpdate;
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
        NetworkManager.OnReceivedRoomListUpdateCallback -= onReceivedRoomListUpdate;
        NetworkManager.OnConnectedToMasterCallback -= OnConnectedToMaster;

        NetworkManager.OnPlayerDisconnectedCallback -= onPlayerDisconnected;
        NetworkManager.OnDisconnectedFromNetworkCallback -= onDisconnectedFromNetwork;

        GameNetwork.Instance.OnPvpAiSetCallback -= onPvpAiSet;
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
        print("Lobby::OnJoinedLobby");
        NetworkManager.GetRoomList();  //Request room list. Wait for OnReceivedRoomListUpdate
    }

    private void onReceivedRoomListUpdate()
    {
        print("Lobby::OnReceivedRoomListUpdate");
        if(!_isJoiningRoom)
            handleRoomCreationAndJoin();
    }

    private void onJoinedRoom()
    {
        Debug.LogWarning("Lobby::OnJoinedRoom -> Is Master Client: " + NetworkManager.GetIsMasterClient());
        GameNetwork gameNetwork = GameNetwork.Instance;

        if (NetworkManager.GetIsMasterClient())
        {
            Room room = NetworkManager.GetRoom();
            print("Lobby::OnJoinedRoom ->  Joined first as master client in room.Name: " + room.Name + " and room.CustomProperties: " + room.CustomProperties.ToStringFull());
            gameNetwork.HostPlayerId = NetworkManager.GetLocalPlayerId();
            NetworkManager.SetRoomCustomProperty(GameRoomProperties.HOST_ID, GameNetwork.Instance.HostPlayerId);
            NetworkManager.SetRoomCustomProperty(GameRoomProperties.TIME_ROOM_STARTED, NetworkManager.GetNetworkTime());

            _waitingPopUp.SetActive(true);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (GameHacks.Instance.ForcePvpAi)
            {
                setRoomWithPvpAi();
            }
            else
#endif
            {
                StartCoroutine(handleWaitForPvpAiSet());
            }
        }
        else
        {
            double timeRoomStarted = double.Parse(NetworkManager.GetRoomCustomProperty(GameRoomProperties.TIME_ROOM_STARTED));
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
                gameNetwork.GuestPlayerId = NetworkManager.GetLocalPlayerId();
                gameNetwork.HostPlayerId = NetworkManager.GetRoomCustomPropertyAsInt(GameRoomProperties.HOST_ID);

                Debug.LogWarning("Lobby::onJoinedRoom -> joined room as guest id: " + gameNetwork.GuestPlayerId);

                GameNetwork.SetLocalPlayerRating(GameStats.Instance.Rating, TeamNames.GUEST);
                NetworkManager.GetRoom().IsOpen = false;
            }
        }

        Debug.LogWarning("Lobby::OnJoinedRoom -> HostPlayerId: " + gameNetwork.HostPlayerId + " GuestPlayerId: " + gameNetwork.GuestPlayerId);
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

    private void handleRoomCreationAndJoin()
    {
        print("Lobby::handleRoomCreationAndJoin");
        RoomInfo[] rooms = NetworkManager.GetRoomList();

        bool forcePvpAi = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        forcePvpAi = GameHacks.Instance.ForcePvpAi;
#endif

        if ((rooms.Length == 0) || forcePvpAi)
            joinOrCreateRoom();
        else
        {
            GameInventory gameInventory = GameInventory.Instance;
            bool foundRoom = false;

            int thisPlayerRating = GameStats.Instance.Rating;

            int roomsCount = rooms.Length;
            for (int i = 0; i < roomsCount; i++)
            {
                RoomInfo room = rooms[i];
                Debug.LogWarning("Lobby::handleRoomCreationAndJoin -> room.Name: " + room.Name + " room.IsOpen: " + room.IsOpen);

                if (room.IsOpen)
                {
                    Debug.LogWarning("Lobby::handleRoomCreationAndJoin -> room.CustomProperties: " + room.CustomProperties.ToStringFull());             
          
                    int playerInRoomRating = (int)room.CustomProperties[GameRoomProperties.HOST_RATING];
                    int ratingDifference = Mathf.Abs(playerInRoomRating - thisPlayerRating);

                    Debug.LogWarning("Lobby::handleRoomCreationAndJoin-> playerInRoomRating: " + playerInRoomRating + " ratingDifference: " + ratingDifference + " _maxRatingDifferenceToFight: " + _maxRatingDifferenceToFight);

                    if (ratingDifference <= _maxRatingDifferenceToFight)
                    {
                        bool hasPvpAi = bool.Parse(room.CustomProperties[GameRoomProperties.HAS_PVP_AI].ToString());
                        Debug.LogWarning("Lobby::handleRoomCreationAndJoin -> room: " + room.Name + " hasPvpAi: " + hasPvpAi);

                        if (!hasPvpAi)
                        {
                            Debug.LogWarning("Lobby::handleRoomCreationAndJoin -> joined room: " + room.Name);

                            GameNetwork.SetLocalPlayerRating(GameStats.Instance.Rating, TeamNames.GUEST);
                            NetworkManager.JoinRoom(room.Name);
                            foundRoom = true;
                            break;
                        }
                    }
                }
            }

            if (!foundRoom)
                joinOrCreateRoom();
        }
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
        //NetworkManager.LoadScene(GameConstants.Scenes.WAR);
        SceneManager.LoadScene(GameConstants.Scenes.WAR, true, true);
    }

    private void joinOrCreateRoom()
    {
        GameNetwork.SetLocalPlayerRating(GameStats.Instance.Rating, TeamNames.HOST);
        GameNetwork.Instance.CreatePublicRoom();
        _isJoiningRoom = true;
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

    private void setRoomWithPvpAi()
    {
        Debug.LogWarning("setRoomWithPvpAi");
        GameStats.Instance.UsesAiForPvp = true;
        GameNetwork.Instance.GuestPlayerId = NetworkManager.GetLocalPlayerId();
        NetworkManager.SetRoomCustomProperty(GameRoomProperties.HAS_PVP_AI, "True");
    }
}
