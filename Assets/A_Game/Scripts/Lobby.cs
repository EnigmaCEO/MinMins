using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using Tiny;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    [SerializeField] private bool _isOnline = false;
    [SerializeField] private int _maxPlayers = 2;
    [SerializeField] private int _maxPlayersNotExpectating = 2;
    [SerializeField] private int _maxRatingDifferenceToFight = 50;
    [SerializeField] private float _timeToWaitBeforeAiRival = 30; // 30 seconds

    void Start()
    {
        NetworkManager.Connect(_isOnline);
        NetworkManager.JoinLobby();

        NetworkManager.SetCustomProperty(GameInventory.RATING_KEY, GameInventory.Instance.GetRating()); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            NetworkManager.LeaveLobby();
            SceneManager.LoadScene("Login");
        }
    }

    private void OnDestroy()
    {
        removeDelegates();
        StopCoroutine(handleWaitForAiRival());
    }

    private void initializeDelegates()
    {
        NetworkManager.UpdateGameRooms += UpdateRoomGrid;
        NetworkManager.UpdateRoomList += UpdateRoomPlayers;

        NetworkManager.OnJoinedLobbyCallback += onJoinedLobby;
        NetworkManager.OnLeftLobbyCallback += onLeftLobby;
        NetworkManager.OnJoinedRoomCallback += onJoinedRoom;
        NetworkManager.OnLeftRoomCallback += onLeftRoom;
        NetworkManager.OnPhotonPlayerConnectedCallback += onPlayerConnected;
        NetworkManager.OnPhotonPlayerDisconnectedCallback += onPlayerDisconnected;
    }

    private void removeDelegates()
    {
        NetworkManager.UpdateGameRooms -= UpdateRoomGrid;
        NetworkManager.UpdateRoomList -= UpdateRoomPlayers;

        NetworkManager.OnJoinedLobbyCallback -= onJoinedLobby;
        NetworkManager.OnLeftLobbyCallback -= onLeftLobby;
        NetworkManager.OnJoinedRoomCallback -= onJoinedRoom;
        NetworkManager.OnLeftRoomCallback -= onLeftRoom;
        NetworkManager.OnPhotonPlayerConnectedCallback -= onPlayerConnected;
        NetworkManager.OnPhotonPlayerDisconnectedCallback -= onPlayerDisconnected;
    }

    private void onJoinedLobby()
    {
        handleRoomCreationAndJoin();
    }

    private void onLeftLobby()
    {

    }

    private void onJoinedRoom()
    {

    }

    private void onLeftRoom()
    {

    }

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
        RoomInfo[] rooms = NetworkManager.GetRoomList();
        if (rooms.Length == 0)
            createAndJoinRoom();
        else
        {
            GameInventory gameInventory = GameInventory.Instance;
            bool foundRoom = false;

            int roomsCount = rooms.Length;
            for (int i = 0; i < roomsCount; i++)
            {
                RoomInfo room = rooms[i];

                if (room.IsOpen)
                {
                    PhotonPlayer[] playerList = NetworkManager.GetPlayerList();
                    int playerInRoomRating = (int)playerList[0].CustomProperties[GameInventory.RATING_KEY];
                    int thisPlayerRating = gameInventory.GetRating();

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
        SceneManager.LoadScene("War");
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

    }

    private void createRoom(string roomName)
    {
        List<string> playerList = new List<string>();

        playerList.Add(NetworkManager.GetPlayerName());

        Hashtable customProps = new Hashtable();
        customProps.Add("playerList", playerList.ToArray());
        customProps.Add("host", NetworkManager.GetPlayerName());

        customProps.Add("mp", _maxPlayers);

        string[] customPropsForLobby = new string[] { "playerList", "host" };

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

    // Helper Functions
    public void SetValue(string key, string val)
    {
        NetworkManager.SetUserInfo(key, val);
    }

    public void SetValue(string key, string[] val)
    {
        string value = "";
        if (val.Length == 0)
            value = Json.Encode(val);

        NetworkManager.SetUserInfo(key, value);
    }

    public string GetValue(string key, string defaultValue = "")
    {
        return NetworkManager.GetUserInfo(key, defaultValue);
    }

    public string[] GetValueArray(string key, string[] defaultValue = null)
    {
        string value = NetworkManager.GetUserInfo(key);

        if ((value == "") && (defaultValue != null))
            return defaultValue;
        else
            return Json.Decode<string[]>(value);
    }
}
