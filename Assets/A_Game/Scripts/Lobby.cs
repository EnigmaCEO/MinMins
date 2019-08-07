using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using Tiny;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    [SerializeField] private bool _isOffline = false;
    [SerializeField] private int _maxPlayers = 2;
    [SerializeField] private int _maxPlayersNotExpectating = 2;
    [SerializeField] private int _maxRatingDifferenceToFight = 50;
    [SerializeField] private float _timeToWaitBeforeAiRival = 30; // 30 seconds
    [SerializeField] private GameObject _waitingPopUp;

    void Start()
    {
        _waitingPopUp.SetActive(false);

        setDelegates();
        NetworkManager.Connect(_isOffline); //No need to call JoinLobby as Auto-Join Lobby is true in PhotonServerSettings 
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
        StopCoroutine(handleWaitForAiRival());
    }

    private void setDelegates()
    {
        //NetworkManager.UpdateGameRooms += UpdateRoomGrid;
        //NetworkManager.UpdateRoomList += UpdateRoomPlayers;

        NetworkManager.OnJoinedLobbyCallback += onJoinedLobby;
        //NetworkManager.OnLeftLobbyCallback += onLeftLobby;
        NetworkManager.OnJoinedRoomCallback += onJoinedRoom;
        //NetworkManager.OnLeftRoomCallback += onLeftRoom;
        NetworkManager.OnPhotonPlayerConnectedCallback += onPlayerConnected;
        NetworkManager.OnPhotonPlayerDisconnectedCallback += onPlayerDisconnected;
    }

    private void removeDelegates()
    {
        //NetworkManager.UpdateGameRooms -= UpdateRoomGrid;
        //NetworkManager.UpdateRoomList -= UpdateRoomPlayers;

        NetworkManager.OnJoinedLobbyCallback -= onJoinedLobby;
        //NetworkManager.OnLeftLobbyCallback -= onLeftLobby;
        NetworkManager.OnJoinedRoomCallback -= onJoinedRoom;
        //NetworkManager.OnLeftRoomCallback -= onLeftRoom;
        NetworkManager.OnPhotonPlayerConnectedCallback -= onPlayerConnected;
        NetworkManager.OnPhotonPlayerDisconnectedCallback -= onPlayerDisconnected;
    }

    private void onJoinedLobby()
    {
        handleRoomCreationAndJoin();
    }

    //private void onLeftLobby()
    //{

    //}

    private void onJoinedRoom()
    {
        _waitingPopUp.SetActive(true);
    }

    //private void onLeftRoom()
    //{

    //}

    private void onPlayerConnected(PhotonPlayer player)
    {
        StopCoroutine(handleWaitForAiRival());
        NetworkManager.SendRPCtoAll("startMatch");  //Assuming this is the Master Client of the room
    }

    private void onPlayerDisconnected(PhotonPlayer player)
    {
        
    }

    private void handleRoomCreationAndJoin()
    {
        print("handleRoomCreationAndJoin");
        RoomInfo[] rooms = NetworkManager.GetRoomList();
        if (rooms.Length == 0)
            createAndJoinRoom();
        else
        {
            GameInventory gameInventory = GameInventory.Instance;
            GameNetwork gameNetwork = GameNetwork.Instance;
            bool foundRoom = false;

            int thisPlayerRating = gameNetwork.GetLocalPlayerRating();

            int roomsCount = rooms.Length;
            for (int i = 0; i < roomsCount; i++)
            {
                RoomInfo room = rooms[i];

                if (room.IsOpen)
                {
                    PhotonPlayer[] playerList = NetworkManager.GetPlayerList();
                    int playerInRoomRating = (int)playerList[0].CustomProperties[GameNetwork.PlayerCustomProperties.RATING];
                    
                    if (Mathf.Abs(playerInRoomRating - thisPlayerRating) <= _maxRatingDifferenceToFight)
                    {
                        NetworkManager.JoinRoom(room.Name);
                        NetworkManager.GetRoom().IsOpen = false;
                        foundRoom = true;
                        break;
                    }
                }
            }

            if (!foundRoom)
                createAndJoinRoom();
        }
    }

    [PunRPC]
    private void startMatch(PhotonMessageInfo messageInfo)
    {
        Debug.Log("startMatch -> sender nickname: " + messageInfo.sender.NickName);
        SceneManager.LoadScene(GameConstants.Scenes.WAR);
    }

    private void createAndJoinRoom()
    {
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
        //TODO: Assign Ai Rival
        GameStats.Instance.UsesAiForPvp = true;
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
