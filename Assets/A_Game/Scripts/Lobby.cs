using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using Tiny;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    [SerializeField] private int _maxPlayers = 2;
    [SerializeField] private int _maxPlayersNotExpectating = 2;
    [SerializeField] private int _maxRatingDifferenceToFight = 50;
    [SerializeField] private float _timeToWaitBeforeAiRival = 30; // 30 seconds
    [SerializeField] private GameObject _waitingPopUp;

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
        //StopCoroutine(handleWaitForAiRival());
    }

    private void setDelegates()
    {
        //NetworkManager.UpdateGameRooms += UpdateRoomGrid;
        //NetworkManager.UpdateRoomList += UpdateRoomPlayers;

        NetworkManager.OnJoinedLobbyCallback += onJoinedLobby;
        //NetworkManager.OnLeftLobbyCallback += onLeftLobby;
        NetworkManager.OnJoinedRoomCallback += onJoinedRoom;
        //NetworkManager.OnLeftRoomCallback += onLeftRoom;
        NetworkManager.OnPlayerConnectedCallback += onPlayerConnected;
        NetworkManager.OnPlayerDisconnectedCallback += onPlayerDisconnected;
        NetworkManager.OnReceivedRoomListUpdateCallback += OnReceivedRoomListUpdate;
        NetworkManager.OnConnectedToMasterCallback += OnConnectedToMaster;

        GameNetwork.OnStartMatch += onStartMatch;
    }

    private void removeDelegates()
    {
        //NetworkManager.UpdateGameRooms -= UpdateRoomGrid;
        //NetworkManager.UpdateRoomList -= UpdateRoomPlayers;

        NetworkManager.OnJoinedLobbyCallback -= onJoinedLobby;
        //NetworkManager.OnLeftLobbyCallback -= onLeftLobby;
        NetworkManager.OnJoinedRoomCallback -= onJoinedRoom;
        //NetworkManager.OnLeftRoomCallback -= onLeftRoom;
        NetworkManager.OnPlayerConnectedCallback -= onPlayerConnected;
        NetworkManager.OnPlayerDisconnectedCallback -= onPlayerDisconnected;
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
        handleRoomCreationAndJoin();
    }

    //private void onLeftLobby()
    //{

    //}

    private void onJoinedRoom()
    {
        print("Lobby::OnJoinedRoom -> Is Master Client: " + NetworkManager.GetIsMasterClient());
        if(NetworkManager.GetIsMasterClient())
            _waitingPopUp.SetActive(true);
        else
            NetworkManager.GetRoom().IsOpen = false;
    }

    //private void onLeftRoom()
    //{

    //}

    private void onPlayerConnected(PhotonPlayer player)
    {
        print("Lobby::onPlayerConnected");
        GameNetwork.Instance.EnemyPlayer = player;
        StopCoroutine(handleWaitForAiRival());
        sendStartMatch();
    }

    private void onPlayerDisconnected(PhotonPlayer player)
    {
        
    }

    private void handleRoomCreationAndJoin()
    {
        print("Lobby::handleRoomCreationAndJoin");
        RoomInfo[] rooms = NetworkManager.GetRoomList();
        if (rooms.Length == 0)
            createAndJoinRoom();
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
                createAndJoinRoom();
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

    private void createAndJoinRoom()
    {
        print("Lobby::createAndJoinRoom");
        string roomName = "1v1 - " + NetworkManager.GetPlayerName();
        createRoom(roomName);
        NetworkManager.JoinRoom(roomName);
        StartCoroutine(handleWaitForAiRival());
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

    private void createRoom(string roomName)
    {
        List<string> playerList = new List<string>();

        playerList.Add(NetworkManager.GetPlayerName());

        Hashtable customProps = new Hashtable();
        customProps.Add(NetworkManager.RoomPropertyOptions.PLAYER_LIST, playerList.ToArray());
        customProps.Add(NetworkManager.RoomPropertyOptions.HOST, NetworkManager.GetPlayerName());

        customProps.Add(NetworkManager.RoomPropertyOptions.MAX_PLAYERS, _maxPlayers);

        string[] customPropsForLobby = new string[] { NetworkManager.RoomPropertyOptions.PLAYER_LIST, NetworkManager.RoomPropertyOptions.HOST };

        NetworkManager.CreateRoom(roomName, true, true, _maxPlayers, customProps, customPropsForLobby, _maxPlayersNotExpectating);
    }

    private void UpdateRoomGrid()
    {
        //if (NetworkManager > 0)
        //{
        //    if (LAST_ROOM_SELECTED == NetworkManager.GetRoomList()[i].customProperties["gt"].ToString())
        //    {
        //    }
        //}
    }

    private void UpdateRoomPlayers()
    {

    }
}
