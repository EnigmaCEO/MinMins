using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using Tiny;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    [SerializeField] private int _maxRatingDifferenceToFight = 50;
    [SerializeField] private float _timeToWaitBeforeAiRival = 30; // 30 seconds
    [SerializeField] private GameObject _waitingPopUp;

    private bool _isJoiningRoom = false;

    private void Awake()
    {
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

        GameNetwork.OnStartMatch += onStartMatch;
    }

    private void removeDelegates()
    {
        NetworkManager.OnJoinedLobbyCallback -= onJoinedLobby;
        NetworkManager.OnJoinedRoomCallback -= onJoinedRoom;
        NetworkManager.OnPlayerConnectedCallback -= onPlayerConnected;
        NetworkManager.OnReceivedRoomListUpdateCallback -= OnReceivedRoomListUpdate;
        NetworkManager.OnConnectedToMasterCallback -= OnConnectedToMaster;

        GameNetwork.OnStartMatch -= onStartMatch;
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

    private void onPlayerConnected(PhotonPlayer player)
    {
        print("Lobby::onPlayerConnected");
        GameNetwork.Instance.EnemyPlayer = player;
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

            int thisPlayerRating = gameNetwork.GetLocalPlayerRating(GameNetwork.VirtualPlayerIds.ALLIES);

            int roomsCount = rooms.Length;
            for (int i = 0; i < roomsCount; i++)
            {
                RoomInfo room = rooms[i];

                if (room.IsOpen)
                {
                    PhotonPlayer[] playerList = NetworkManager.GetPlayerList();
                    PhotonPlayer player = playerList[0];
                    int playerInRoomRating = GameNetwork.Instance.GetAnyPlayerRating(player, GameNetwork.VirtualPlayerIds.ALLIES);
                    
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

    private void sendStartMatch()
    {
        GameNetwork.Instance.SendStartMatch();
    }

    private void onStartMatch()
    {
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
        sendStartMatch();
    }
}
