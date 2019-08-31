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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            NetworkManager.LeaveLobby();
            SceneManager.LoadScene(GameConstants.Scenes.MAIN);
        }
    }

    private void OnDestroy()
    {
        removeDelegates();
    }

    private void setDelegates()
    {
        NetworkManager.OnJoinedLobbyCallback += onJoinedLobby;
        NetworkManager.OnJoinedRoomCallback += onJoinedRoom;
        NetworkManager.OnPlayerConnectedCallback += onPlayerConnected;
        NetworkManager.OnReceivedRoomListUpdateCallback += OnReceivedRoomListUpdate;
        NetworkManager.OnConnectedToMasterCallback += OnConnectedToMaster;
    }

    private void removeDelegates()
    {
        NetworkManager.OnJoinedLobbyCallback -= onJoinedLobby;
        NetworkManager.OnJoinedRoomCallback -= onJoinedRoom;
        NetworkManager.OnPlayerConnectedCallback -= onPlayerConnected;
        NetworkManager.OnReceivedRoomListUpdateCallback -= OnReceivedRoomListUpdate;
        NetworkManager.OnConnectedToMasterCallback -= OnConnectedToMaster;
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

    private void OnReceivedRoomListUpdate()
    {
        print("Lobby::OnReceivedRoomListUpdate");
        if(!_isJoiningRoom)
            handleRoomCreationAndJoin();
    }

    private void onJoinedRoom()
    {
        print("Lobby::OnJoinedRoom -> Is Master Client: " + NetworkManager.GetIsMasterClient());
        if (NetworkManager.GetIsMasterClient())
        {
            _waitingPopUp.SetActive(true);
            StartCoroutine(handleWaitForAiRival());
        }
        else
            NetworkManager.GetRoom().IsOpen = false;
    }

    private void onPlayerConnected(int networkPlayerId)
    {
        print("Lobby::onPlayerConnected");
        GameNetwork.Instance.GuestPlayerId = networkPlayerId;
        StopCoroutine(handleWaitForAiRival());
        sendStartMatch();
    }

    private void handleRoomCreationAndJoin()
    {
        print("Lobby::handleRoomCreationAndJoin");
        RoomInfo[] rooms = NetworkManager.GetRoomList();
        if (rooms.Length == 0)
            joinOrCreateRoom();
        else
        {
            GameInventory gameInventory = GameInventory.Instance;
            GameNetwork gameNetwork = GameNetwork.Instance;
            bool foundRoom = false;

            int thisPlayerRating = gameNetwork.GetLocalPlayerRating(GameNetwork.VirtualPlayerIds.HOST);

            int roomsCount = rooms.Length;
            for (int i = 0; i < roomsCount; i++)
            {
                RoomInfo room = rooms[i];

                if (room.IsOpen)
                {
                    int[] playerIdList = NetworkManager.GetPlayerIdList();
                    int playerId = playerIdList[0];
                    int playerInRoomRating = GameNetwork.Instance.GetAnyPlayerRating(playerId, GameNetwork.VirtualPlayerIds.HOST);
                    
                    if (Mathf.Abs(playerInRoomRating - thisPlayerRating) <= _maxRatingDifferenceToFight)
                    {
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
        base.SendRpcToAll("receiveStartMatch");
    }

    [PunRPC]
    private void receiveStartMatch()
    {
        Debug.Log("Lobby::receiveStartMatch");
        SceneManager.LoadScene(GameConstants.Scenes.WAR);
    }

    private void joinOrCreateRoom()
    {
        GameNetwork.Instance.JoinOrCreateRoom();
        _isJoiningRoom = true;
    }

    private IEnumerator handleWaitForAiRival()
    {
        yield return new WaitForSeconds(_timeToWaitBeforeAiRival);
        assignAiRival();
    }

    private void assignAiRival()
    {
        GameStats.Instance.UsesAiForPvp = true;
        GameNetwork.Instance.GuestPlayerId = NetworkManager.GetLocalPlayerId();
        sendStartMatch();
    }
}
