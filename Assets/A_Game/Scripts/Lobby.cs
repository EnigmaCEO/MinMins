using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using Tiny;
using UnityEngine;

public class Lobby : NetworkEntity
{
    [SerializeField] private int _maxRatingDifferenceToFight = 50;
    [SerializeField] private float _timeToWaitBeforeAiRival = 30; // 30 seconds
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
            //SceneManager.LoadScene(GameConstants.Scenes.MAIN);
            NetworkManager.LoadScene(GameConstants.Scenes.MAIN);
        }
    }

    private void OnDestroy()
    {
        Debug.LogWarning("Lobby::OnDestroy");
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
        Debug.LogWarning("Lobby::onPvpAiSet");

        if (enabled)
        {
            GameStats.Instance.UsesAiForPvp = true;
            GameNetwork.Instance.GuestPlayerId = NetworkManager.GetLocalPlayerId();
            sendStartMatch();
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
            //print("Lobby::OnJoinedRoom -> room.CustomProperties: " + NetworkManager.GetRoom().CustomProperties.ToStringFull());
            gameNetwork.HostPlayerId = NetworkManager.GetLocalPlayerId();
            NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.HOST_ID, GameNetwork.Instance.HostPlayerId);

            _waitingPopUp.SetActive(true);

            if (GameHacks.Instance.ForcePvpAi)
                assignAiRival();
            else 
                StartCoroutine(handleWaitForAiRival());
        }
        else
        {
            gameNetwork.GuestPlayerId = NetworkManager.GetLocalPlayerId();
            gameNetwork.HostPlayerId = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.HOST_ID);
            GameNetwork.SetLocalPlayerRating(GameStats.Instance.Rating, GameNetwork.TeamNames.GUEST);
            NetworkManager.GetRoom().IsOpen = false;
        }

        Debug.LogWarning("Lobby::OnJoinedRoom -> HostPlayerId: " + gameNetwork.HostPlayerId + " GuestPlayerId: " + gameNetwork.GuestPlayerId);
    }

    private void onPlayerConnected(int networkPlayerId)
    {
        print("Lobby::onPlayerConnected -> networkPlayerId " + networkPlayerId);
        GameNetwork gameNetwork = GameNetwork.Instance;
        GameNetwork.Instance.GuestPlayerId = networkPlayerId;
        Debug.LogWarning("Lobby::onPlayerConnected -> HostPlayerId: " + gameNetwork.HostPlayerId + " GuestPlayerId: " + gameNetwork.GuestPlayerId);
        StopCoroutine(handleWaitForAiRival());
        sendStartMatch();
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

        if ((rooms.Length == 0) || GameHacks.Instance.ForcePvpAi)
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

                if (room.IsOpen)
                {
                    print("Lobby::handleRoomCreationAndJoin -> room.CustomProperties: " + room.CustomProperties.ToStringFull());
                    int playerInRoomRating = (int)room.CustomProperties[GameNetwork.RoomCustomProperties.HOST_RATING];

                    if (Mathf.Abs(playerInRoomRating - thisPlayerRating) <= _maxRatingDifferenceToFight)
                    {
                        GameNetwork.SetLocalPlayerRating(GameStats.Instance.Rating, GameNetwork.TeamNames.GUEST);
                        NetworkManager.JoinRoom(room.Name);
                        foundRoom = true;
                        break;
                    }
                }
            }

            if (!foundRoom)
                joinOrCreateRoom();
        }
    }

    public void sendStartMatch()
    {
        base.SendRpcToAll(nameof(receiveStartMatch));
    }

    [PunRPC]
    private void receiveStartMatch()
    {
        Debug.Log("Lobby::receiveStartMatch");
        //SceneManager.LoadScene(GameConstants.Scenes.WAR);
        NetworkManager.LoadScene(GameConstants.Scenes.WAR);
    }

    private void joinOrCreateRoom()
    {
        GameNetwork.SetLocalPlayerRating(GameStats.Instance.Rating, GameNetwork.TeamNames.HOST);
        GameNetwork.Instance.JoinOrCreateRoom();
        _isJoiningRoom = true;
    }

    private IEnumerator handleWaitForAiRival()
    {
        float timeToWait = _timeToWaitBeforeAiRival;

        if (GameHacks.Instance.TimeWaitAiPvp.Enabled)
            timeToWait = GameHacks.Instance.TimeWaitAiPvp.ValueAsFloat;

        yield return new WaitForSeconds(timeToWait);
        assignAiRival();
    }

    private void assignAiRival()
    {
        NetworkManager.GetRoom().IsOpen = false;
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.HAS_PVP_AI, "true");
    }
}
