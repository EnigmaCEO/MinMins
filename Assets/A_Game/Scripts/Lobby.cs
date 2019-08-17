﻿using Enigma.CoreSystems;
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

    void Start()
    {
        _waitingPopUp.SetActive(false);
        setDelegates();    
        GameStats.Instance.UsesAiForPvp = false;
        NetworkManager.JoinLobby();
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
        NetworkManager.OnPlayerConnectedCallback += onPlayerConnected;
        NetworkManager.OnPlayerDisconnectedCallback += onPlayerDisconnected;
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
    }

    private void onJoinedLobby()
    {
        print("Lobby::OnJoinedLobby");
        handleRoomCreationAndJoin();
    }

    //private void onLeftLobby()
    //{

    //}

    private void onJoinedRoom()
    {
        print("Lobby::OnJoinedRoom");
        _waitingPopUp.SetActive(true);
    }

    //private void onLeftRoom()
    //{

    //}

    private void onPlayerConnected(PhotonPlayer player)
    {
        print("Lobby::onPlayerConnected");
        StopCoroutine(handleWaitForAiRival());
        NetworkManager.SendRPCtoAll("startMatch");  //Assuming this is the Master Client of the room
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

            int thisPlayerRating = gameNetwork.GetLocalPlayerRating(GameConstants.VirtualPlayerIds.ALLIES);

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
        goToWar();
    }

    private void goToWar()
    {
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
        GameStats.Instance.UsesAiForPvp = true;
        goToWar();
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
