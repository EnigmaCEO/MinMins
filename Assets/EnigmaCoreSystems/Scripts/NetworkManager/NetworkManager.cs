﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;
using Tiny;

namespace Enigma.CoreSystems
{
    public class NetworkManager : Manageable<NetworkManager>
    {
        public class Separators
        {
            public const char VALUES = '|';
            public const char KEYS = '-';
            public const char VIRTUAL_PLAYER_KEY = ':';
        }

        public class Transactions
        {
            public const int IP_AND_COUNTERY = 0;
            public const int LOGIN = 3;
            public const int REGISTRATION = 5;
            public const int COINS_EARNED = 6;
            public const int HEART_BEAT = 14;
        }

        public class TransactionKeys
        {
            public const string TID = "tid";
            public const string SSID = "ssid";
            public const string IMAGE = "image";
            public const string IMAGE_UPLOAD = "imageUpload";
            public const string BUNDLE_ID = "bundle_id";
            public const string GAME = "game";
            public const string USERNAME = "userName";
            public const string PASSWORD = "password";
            public const string EMAIL = "email";
            public const string USER = "user";
            public const string PW_HASH = "pwhash";
            public const string STATUS = "status";
            public const string STORE = "store";
            public const string ROOM = "room";
            public const string LIST = "list";
            public const string CHAT = "chat";
            public const string USER_LIST = "userList";
            public const string SEC = "sec";
            public const string USER_LOGIN = "user_login";
            public const string USER_DATA = "user_data";
            public const string COINS = "coins";
        }

        public class EnjinTransKeys
        {
            public const string ENJIN_ID = "enjin_id";
            public const string ENJIN_CODE = "enjin_code";
        }

        public class RoomPropertyOptions
        {
            public const string GAME_STATE = "gState";
            public const string START_COUNT_DOWN_TIMER = "st";
            public const string PLAYER_LIST = "playerList";
            public const string HOST = "host";
            public const string MAX_PLAYERS = "mp";
        }

        public class PlayerPropertyOptions
        {
            public const string PLAYER_STATE = "plState";
        }

        public class StatusOptions
        {
            public const string SUCCESS = "SUCCESS";
            public const string ERR_REGISTER = "ERR_REGISTER";
            public const string ERR_INVALID_PASSWORD = "ERR_INVALID_PASSWORD";
            public const string ERR_INVALID_USERNAME = "ERR_INVALID_PASSWORD";
        }

        public class DataGroups
        {
            public const string INFO = "Info";
        }

        public class DataKeys
        {
            public const string PLAYER = "Player";
            public const string ROOM = "Room";
        }

        public class RoomStates
        {
            public const string STAGING = "Staging";
            public const string READY = "Ready";
            public const string SYNC = "Sync";
            public const string LAUNCH = "Launch";
            public const string PLAY = "Play";
        }

        public class PlayerStates
        {
            public const string SYNC = "Sync";
            public const string SPAWN = "Spawn";
            public const string CREATED = "Created";
            public const string READY = "Ready";
        }

        ////Make an Enum for room state later
        //static public string ROOM_STATE_STAGING = "Staging";
        //static public string ROOM_STATE_READY = "Ready";
        //static public string ROOM_STATE_SYNC = "Sync";
        //static public string ROOM_STATE_LAUNCH = "Launch";
        //static public string ROOM_STATE_PLAY = "Play";

        public const string _PHOTON_CONNECTION_GAME_VERSION_SETTINGS = "v1.0";

        static private string _serverUrl;
        static private string _sessionID;
        static private string _game;

        static public Dictionary<string, Hashtable> Data = new Dictionary<string, Hashtable>();

        static public bool ConnectedToUNet = false;
        static public bool TimeoutDisconnected = false;
        static public bool ServerDisconnected = false;
        static public bool ReadySpawn = false;
        static public bool PlayerIsServer = false;
        static public int NumberOfPlayers = 0;

        static private bool CreatingOrJoinRoom = false;

        //static public string ROOM_CLICKED = "1";
        //static public string ROOM_NAME = "Practice";
        static public List<string> LobbyUserList = new List<string>();
        static private List<string> _updatedLobbyUserList = new List<string>();
        //static public List<string> ROOM_LEVELLIST = new List<string>();
        //static private List<string> UPDATED_ROOM_LEVELLIST = new List<string>();

        static private int _maxPlayersNotSpectating;

        //static private bool LoggedIn = false;
        static private bool _stopGetRoomData = true;

        static public bool OriginalMaster = false;
        static public string Ip;
        static public string Country;

        static private GameObject _localPlayerCharacter;

        //Delegate for updating the gamerooms grid in the Chat scene
        public delegate void UpdateGameroomsGridDelegate();
        static public UpdateGameroomsGridDelegate UpdateGamerooms;

        //Delegate for syncing player info such as player name, etc.
        public delegate void SyncPlayerInfoDelegate(int playerPhotonViewId, string playerName);
        static public SyncPlayerInfoDelegate SyncPlayerInfo;

        //Delegate for setting the player name as the player object name
        public delegate void SetPlayerNameAsObjNameDelegate(int playerPhotonViewId, string playerName);
        static public SetPlayerNameAsObjNameDelegate SetPlayerNameAsObjName;

        //Delegate for starting the countdown timer
        public delegate void StartCountdownTimerDelegate(double startTime);
        static public StartCountdownTimerDelegate StartCountdownTimer;

        //Delegate for the master to destroy objects of players who left
        public delegate void DestroyDisconnectedPlayerObjectDelegate(PhotonPlayer player);
        static public DestroyDisconnectedPlayerObjectDelegate DestroyDisconnectedPlayerObject;

        public delegate void OnPlayerConnectedDelegate(PhotonPlayer connectedPlayer);
        static public OnPlayerConnectedDelegate OnPlayerConnectedCallback;

        public delegate void OnPlayerDisconnectedDelegate(PhotonPlayer disconnectedPlayer);
        static public OnPlayerDisconnectedDelegate OnPlayerDisconnectedCallback;

        public delegate void OnPlayerCustomPropertiesChangedDelegate(PhotonPlayer player, Hashtable updatedProperties);
        static public OnPlayerCustomPropertiesChangedDelegate OnPlayerCustomPropertiesChangedCallback;

        public delegate void OnRoomCustomPropertiesChangedDelegate(Hashtable updatedProperties);
        static public OnRoomCustomPropertiesChangedDelegate OnRoomCustomPropertiesChangedCallback;

        public delegate void OnMasterClientSwitchedDelegate(PhotonPlayer newMaster);
        static public OnMasterClientSwitchedDelegate OnMasterClientSwitchedCallback;

        public delegate void SimpleDelegate();

        static public SimpleDelegate EnableReadyUI;
        static public SimpleDelegate DisableReadyUI;
        static public SimpleDelegate CheckReadyToLaunch;
        static public SimpleDelegate UpdateGameRooms;
        static public SimpleDelegate UpdateRoomList;

        static public SimpleDelegate OnJoinedLobbyCallback;
        static public SimpleDelegate OnLeftLobbyCallback;
        static public SimpleDelegate OnJoinedRoomCallback;
        static public SimpleDelegate OnLeftRoomCallback;

        static public SimpleDelegate OnUpdatedFriendListCallback;
        static public SimpleDelegate OnDisconnectedFromPhotonCallback;

        // global active player
        static public JSONNode ActiveCharacter;
         

        protected override void Awake ()
        {
            Debug.Log("NetworkManager::Awake");
        }

        protected override void Start ()
        {
            base.Start ();
            Data = new Dictionary<string, Hashtable> ();

            //Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
            //Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
            //GameOfWhales.Instance.OnPushDelivered += OnPushDelivered;
        }

        public delegate void Callback (JSONNode data);
        public delegate void TextureCallback (Texture2D data);
        public delegate void ImageCallback ();

        static public void SetServer (string url)
        {
            _serverUrl = url;
        }

        static public string GetServer()
        {
            return _serverUrl;
        }

        static public void StartHeartBeat(NetworkManager.Callback onHeartBeat)
        {
            Instance.StopCoroutine(heartbeat(onHeartBeat));
            Instance.StartCoroutine(heartbeat(onHeartBeat));
        }

        static public void SetSessionID (string id)
        {
            _sessionID = id;
        }

        static public string GetSessionID ()
        {
            return _sessionID;
        }

        static public void SetGame(string gameName)
        {
            _game = gameName;
        }

        static public string GetGame()
        {
            return _game;
        }

        static public void Transaction(int id, Callback externalCallback = null, Callback localCallback = null, TextureCallback texture = null)
        {
            Instance.StartCoroutine(httpRequest(id, new Hashtable(), externalCallback, localCallback, texture));
        }

        static public void Transaction(int id, string transactionKey, object transactionValue, Callback externalCallback = null, Callback localCallback = null, TextureCallback texture = null)
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add(transactionKey, transactionValue);
            Instance.StartCoroutine(httpRequest(id, hashtable, externalCallback, localCallback, texture));
        }

        static public void Transaction (int id, Hashtable hashtable, Callback externalCallback = null, Callback localCallback = null, TextureCallback texture = null)
        {
            Debug.Log(Instance);
            Instance.StartCoroutine (httpRequest (id, hashtable, externalCallback, localCallback, texture));
        }

        static private IEnumerator httpRequest (int id, Hashtable hashtable, Callback externalCallback, Callback localCallback, TextureCallback texture)
        {
            string url = _serverUrl + "/trans/" + id + ".php";
            string sec = "";

            WWWForm formData = new WWWForm ();
            formData.AddField (TransactionKeys.TID, id);
            if (_sessionID != null) 
                formData.AddField (TransactionKeys.SSID, _sessionID);
            
            foreach (DictionaryEntry pair in hashtable)
            {
                Debug.Log (pair.Key + " " + pair.Value);
                if (pair.Key.ToString() == TransactionKeys.IMAGE) 
                    formData.AddBinaryData(TransactionKeys.IMAGE_UPLOAD, pair.Value as byte[], "image.png", "image/png");
                else
                {
                    //string value = Md5Sum(Application.identifier + Application.version + (string)pair.Value);
                    formData.AddField((string)pair.Key, (string)pair.Value);
                    sec += (string)pair.Value;
                }
            }

            formData.AddField(TransactionKeys.BUNDLE_ID, Application.identifier);
            formData.AddField(TransactionKeys.GAME, _game);

            sec += Application.identifier + _game;
            sec = md5(sec);

            formData.AddField(TransactionKeys.SEC, sec);

            Debug.Log ("url: " + url);
            var www = UnityWebRequest.Post(url, formData);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log (www.error + " on transaction: " + id.ToString ());

                JSONNode response = JSON.Parse(www.error);

                if (localCallback != null) 
                    localCallback(response);
                
                externalCallback(response);

                #if UNITY_ANDROID
                EtceteraAndroid.showAlert ("Network Error", "Error connecting to the server. Restart the app and retry.", "OK");
                #endif

                #if UNITY_IPHONE
                var buttons = new string[] { "OK" };
                #endif

            }
            else
            {
                if (externalCallback != null)
                {
                    JSONNode response = JSON.Parse (www.downloadHandler.text);

                    if (localCallback != null)
                        localCallback (response);

                    externalCallback (response);
                }
                if (texture != null) 
                    texture (((DownloadHandlerTexture)www.downloadHandler).texture);
            }
        }

        static public string Url_create_parameters (Hashtable my_hash)
        {
            string parameters = "";
            foreach (DictionaryEntry hash_entry in my_hash) 
                parameters = parameters + "&" + hash_entry.Key + "=" + hash_entry.Value;

            return parameters;
        }

        // from unity wiki
        static public string Md5Sum (string strToEncrypt)
        {
            System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding ();
            byte[] bytes = ue.GetBytes (strToEncrypt);

            // encrypt bytes
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider ();
            byte[] hashBytes = md5.ComputeHash (bytes);

            // Convert the encrypted bytes back to a string (base 16)
            string hashString = "";

            for (int i = 0; i < hashBytes.Length; i++) 
                hashString += System.Convert.ToString (hashBytes [i], 16).PadLeft (2, '0');
            

            return hashString.PadLeft (32, '0');
        }

        static public void Register(string userName, string password, string email, string ethAddress, string game, string bundleId, Callback callback = null, Hashtable extras = null)
        {
            Hashtable hashtable = new Hashtable();

            hashtable.Add(TransactionKeys.USERNAME, userName);
            hashtable.Add(TransactionKeys.PASSWORD, password);
            hashtable.Add(TransactionKeys.EMAIL, email);
            hashtable.Add(TransactionKeys.GAME, game);
            hashtable.Add(TransactionKeys.BUNDLE_ID, bundleId);

            if (extras != null)
                hashtable.Merge(extras);

            Transaction(Transactions.REGISTRATION, hashtable, callback, RegistrationResult);
        }

        static private void RegistrationResult(JSONNode response)
        {
            JSONNode response_hash = response[0];
            string status = response_hash[TransactionKeys.STATUS].ToString().Trim('"');
            string ssid = response_hash[TransactionKeys.SSID].ToString().Trim('"');

            Debug.Log("RegistrationResult: " + response_hash.ToString());

            if (status == StatusOptions.SUCCESS)
                NetworkManager.SetSessionID(ssid);
        }

        static public void Login (string user, string pw, Callback callback = null, Hashtable extras = null)
        {
            Hashtable hashtable = new Hashtable ();
            hashtable.Add (TransactionKeys.USER, user);
            hashtable.Add (TransactionKeys.PW_HASH, pw);

            if (extras != null)
                hashtable.Merge (extras);

            Transaction (Transactions.LOGIN, hashtable, callback, LoginResult);
        }

        static private void LoginResult (JSONNode response)
        {
            if (response == null)
                return;

            JSONNode response_hash = response [0];
            string status = response_hash [TransactionKeys.STATUS].ToString ().Trim ('"');
            string ssid = response_hash [TransactionKeys.SSID].ToString ().Trim ('"');

            Debug.Log ("LoginResult: " + response_hash.ToString());

            if (status == StatusOptions.SUCCESS) 
                NetworkManager.SetSessionID (ssid);
        }

        static public void SetData (string key, Hashtable val)
        {
            if (Data.ContainsKey (key))
                Data.Remove (key);

            Data.Add (key, val);
        }

        static public void GetIAP (JSONNode response)
        {
            JSONNode response_hash = response [0];
            string status = response_hash [TransactionKeys.STATUS].ToString ().Trim ('"');
            JSONNode data = response_hash [TransactionKeys.STORE];

            if (status == StatusOptions.SUCCESS)
            {
        #if !UNITY_STANDALONE
            //IAPManager.LoadData(data);
        #endif
            }
        }

        static public void LoadImageFromUrl(string url, Image image, ImageCallback callback = null)
        {
            Instance.StartCoroutine(loadImageFromUrlCoroutine(url, image, callback));
        }

        static private IEnumerator loadImageFromUrlCoroutine(string url, Image image, ImageCallback callback = null)
        {
            using (WWW www = new WWW(url))
            {
                // Wait for download to complete
                yield return www;

                Texture2D texture = www.texture;
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                if (callback != null)
                    callback();
            }
        }

        static private IEnumerator heartbeat(NetworkManager.Callback onHeartBeat)
        {
            while (true)
            {
                if (NetworkManager.GetSessionID() == null)
                    yield return new WaitForSeconds(5f);
                else
                {
                    NetworkManager.Transaction(Transactions.HEART_BEAT, onHeartBeat);
                    yield return new WaitForSeconds(300.0f);
                }
            }
        }

        //Wrappers for Photon calls
        //Connect to Photon Network
        static public void Connect(bool isOffline)
        {
            print("NetworkManager::Connect -> isOffline: " + isOffline);
            if (GetConnected())
            {
                if (GetInRoom())
                    LeaveRoom();
                
                return;
            }

            Instance.gameObject.AddComponent<PhotonView>();
            PhotonNetwork.ConnectUsingSettings(_PHOTON_CONNECTION_GAME_VERSION_SETTINGS);
            PhotonNetwork.logLevel = PhotonLogLevel.Full;
            PhotonNetwork.offlineMode = isOffline;
            GetLocalPlayerPhotonView().viewID = 1;

            PhotonNetwork.InstantiateInRoomOnly = !isOffline;
        }

        static public void Disconnect()
        {
            if(PhotonNetwork.connected)
                PhotonNetwork.Disconnect();
        }

        static public void SendRPCtoAll(string methodName, params object[] parameters)
        {
            SendRPC(methodName, PhotonTargets.All, parameters);
        }

        static public void SendRPC(string methodName, PhotonTargets target, params object[] parameters)
        {
            GetLocalPlayerPhotonView().RPC(methodName, target, parameters);
        }

        static public bool IsPhotonOffline()
        {
            return PhotonNetwork.offlineMode;
        }

        //Check if the user is connected to Photon
        //Network
        static public bool GetConnected()
        {
            return PhotonNetwork.connected;
        }

        //Join Photon lobby
        static public void JoinLobby()
        {
            PhotonNetwork.JoinLobby();
        }

        //Leave Photon lobby
        static public void LeaveLobby()
        {
            PhotonNetwork.LeaveLobby();
        }

        // Check if Photon room exists
        static public bool CheckRoomExists(string roomName)
        {
            RoomInfo[] rooms = GetRoomList();
            foreach (RoomInfo room in rooms)
            {
                if (room.Name == roomName)
                    return true;
            }

            return false;
        }

        // Joins a Photon room if it exists, or creates it with the given parameters
        static public bool JoinOrCreateRoom(string roomName, bool isVisible, bool isOpen, int maxPlayers, Hashtable customRoomProperties,
                                            string[] propsToListInLobby, int maxPlayersNotExpectating)
        {
            if (CheckRoomExists(roomName))
                return JoinRoom(roomName);
            else
                return CreateRoom(roomName, isVisible, isOpen, maxPlayers, customRoomProperties, propsToListInLobby, maxPlayersNotExpectating);
        }

        //Create a Photon Room using passed in parameters
        static public bool CreateRoom(string roomName, bool isVisible, bool isOpen, int maxPlayers, Hashtable customRoomProperties,
                                        string[] propsToListInLobby, int maxPlayersNotExpectating)
        {
            ExitGames.Client.Photon.Hashtable photonHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in customRoomProperties)
                photonHTable.Add(pair.Key, pair.Value);

            _maxPlayersNotSpectating = maxPlayersNotExpectating;

            RoomOptions options = new RoomOptions();
            options.IsVisible = isVisible;
            options.IsOpen = isOpen;
            options.MaxPlayers = (byte)maxPlayers;
            options.CustomRoomProperties = photonHTable;
            options.CustomRoomPropertiesForLobby = propsToListInLobby;

            SetCreatingOrJoinRoom(true);

            return PhotonNetwork.CreateRoom(roomName, options, null);
        }

        //Join a Photon Room
        static public bool JoinRoom(string roomName)
        {
            return PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
        }

        //Leave a Photon Room
        static public bool LeaveRoom()
        {
            return PhotonNetwork.LeaveRoom();
        }

        //Get the room that user is currently in
        static public Room GetRoom()
        {
            return PhotonNetwork.room;
        }

        //Get the number of rooms from the lobby
        static public int GetRoomCount()
        {
            return PhotonNetwork.GetRoomList().Length;
        }

        //Get a list of the rooms from the lobby
        static public RoomInfo[] GetRoomList()
        {
            return PhotonNetwork.GetRoomList();
        }

        //Helper for testing
        static public void ReadRoomList()
        {
            RoomInfo[] roomList = GetRoomList();

            foreach (RoomInfo r in roomList)
                Debug.Log(r);
        }

        //Get number of players in the room
        static public int GetPlayerCount()
        {
            return PhotonNetwork.room.PlayerCount;
        }

        //Get the max number of players that can
        //be in the room
        static public int GetMaxPlayers()
        {
            return PhotonNetwork.room.MaxPlayers;
        }

        //Set the user's player name in Photon Network
        static public void SetPlayerName(string playerName)
        {
            PhotonNetwork.playerName = playerName;
        }

        //Get the user's player name in Photon Network
        static public string GetPlayerName()
        {
            return PhotonNetwork.playerName;
        }

        //Get the local PhotonPlayer (local user)
        static public PhotonPlayer GetLocalPlayer()
        {
            return PhotonNetwork.player;
        }

        //Set a PhotonPlayer's tag object
        static public void SetPlayerTagObject(PhotonPlayer pp, GameObject go)
        {
            pp.TagObject = go;
        }

        //Get a PhotonPlayer's tag object
        static public GameObject GetPlayerTagObject(PhotonPlayer pp)
        {
            return (GameObject)pp.TagObject;
        }

        //Check if user is inside the lobby or not
        static public bool GetInLobby()
        {
            return PhotonNetwork.insideLobby;
        }

        //Check if user is inside a room or not
        static public bool GetInRoom()
        {
            return PhotonNetwork.inRoom;
        }

        //Check if the user is the master client of a room
        static public bool GetIsMasterClient()
        {
            return PhotonNetwork.isMasterClient;
        }

        static public bool GetIsMasterClientOrDisconnected()
        {
            return (!NetworkManager.GetConnected() || GetIsConnectedAndMasterClient());
        }

        static public bool GetIsConnectedAndMasterClient()
        {
            return (NetworkManager.GetConnected() && NetworkManager.GetIsMasterClient());
        }

        //Get the list of players in the room, including
        //the user
        static public PhotonPlayer[] GetPlayerList()
        {
            return PhotonNetwork.playerList;
        }

        //Get the list of players in the room, excluding
        //the user
        static public PhotonPlayer[] GetOtherPlayerList()
        {
            return PhotonNetwork.otherPlayers;
        }

        //Takes a string[] and looks for all the players in
        //that string[] then storing their info in PhotonNetwork.friends
        static public bool FindFriends(string[] friendsToFind)
        {
            return PhotonNetwork.FindFriends(friendsToFind);
        }

        //Get variable used to check if user is joining/creating a room
        //(determines whether to disconnect from Photon Network when
        //leaving lobby)    
        static public bool GetCreatingOrJoinRoom()
        {
            return CreatingOrJoinRoom;
        }

        //Set variable used to check if user is joining/creating a room
        //(determines whether to disconnect from Photon Network when
        //leaving lobby)
        static public void SetCreatingOrJoinRoom(bool creatingOrJoinRoom)
        {
            CreatingOrJoinRoom = creatingOrJoinRoom;
        }

        static public void SetAnyPlayerCustomProperty(string key, string value, string virtualPlayerId, PhotonPlayer player)
        {
            string virtualPlayerKey = virtualPlayerId + Separators.VIRTUAL_PLAYER_KEY + key;

            if (IsPhotonOffline())
            {
                if (!Data.ContainsKey(DataGroups.INFO))
                    Data.Add(DataGroups.INFO, new Hashtable());

                if (!Data[DataGroups.INFO].ContainsKey(DataKeys.PLAYER))
                    Data[DataGroups.INFO].Add(DataKeys.PLAYER, new Hashtable());
               
                Hashtable userData = Data[DataGroups.INFO][DataKeys.PLAYER] as Hashtable;
                
                if (!userData.ContainsKey(virtualPlayerKey))
                {
                    if (value != null)
                        userData.Add(virtualPlayerKey, value);
                }
                else
                {
                    if (value == null)
                        userData.Remove(virtualPlayerKey);
                    else
                        userData[virtualPlayerKey] = value;
                }

                if (value != null)
                {
                    Hashtable updatedProperties = new Hashtable();
                    updatedProperties.Add(virtualPlayerKey, value);
                    if (OnPlayerCustomPropertiesChangedCallback != null)
                        OnPlayerCustomPropertiesChangedCallback(GetLocalPlayer(), updatedProperties);
                }
            }
            else  //Online
            {
                ExitGames.Client.Photon.Hashtable photonHTable = new ExitGames.Client.Photon.Hashtable();
                photonHTable.Add(virtualPlayerKey, value);
                player.SetCustomProperties(photonHTable);
            }
        }

        static public void SetLocalPlayerCustomProperty(string key, string value, string virtualPlayerId)
        {
            SetAnyPlayerCustomProperty(key, value, virtualPlayerId, GetLocalPlayer());
        }

        static public int GetAnyPlayerCustomPropertyAsInt(string key, string virtualPlayerId, PhotonPlayer player)
        {
            return int.Parse(GetAnyPlayerCustomProperty(key, virtualPlayerId, player));
        }

        static public string GetAnyPlayerCustomProperty(string key, string virtualPlayerId, PhotonPlayer player)
        {
            string virtualPlayerKey = virtualPlayerId + Separators.VIRTUAL_PLAYER_KEY + key;

            if (IsPhotonOffline())
            {
                if (!Data.ContainsKey(DataGroups.INFO))
                    return "";

                if (!Data[DataGroups.INFO].ContainsKey(DataKeys.PLAYER))
                    return "";

                
                Hashtable userData = Data[DataGroups.INFO][DataKeys.PLAYER] as Hashtable;
                return userData[virtualPlayerKey].ToString().Trim('"');
            }
            else  // Online
                return player.CustomProperties[virtualPlayerKey].ToString();
        }

        static public int GetLocalPlayerCustomPropertyAsInt(string key, string virtualPlayerId)
        {
            return int.Parse(GetLocalPlayerCustomProperty(key, virtualPlayerId));
        }

        static public string GetLocalPlayerCustomProperty(string key, string virtualPlayerId)
        {
            return GetAnyPlayerCustomProperty(key, virtualPlayerId, GetLocalPlayer());
        }

        //Helper for testing
        static public void ReadPlayerList(PhotonPlayer[] playerList)
        {
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                Debug.Log(string.Format("Player {0}: ", i + 1) + playerList[i].NickName);
        }

        ///<summary>
        /// Set room custom property
        /// </summary>
        static public void SetRoomCustomProperty(object key, object value)
        {
            if (IsPhotonOffline())
            {
                if (!Data.ContainsKey(DataGroups.INFO))
                    Data.Add(DataGroups.INFO, new Hashtable());

                if (!Data[DataGroups.INFO].ContainsKey(DataKeys.ROOM))
                    Data[DataGroups.INFO].Add(DataKeys.ROOM, new Hashtable());

                Hashtable roomData = Data[DataGroups.INFO][DataKeys.ROOM] as Hashtable;

                if (!roomData.ContainsKey(key)) 
                    roomData.Add(key, value);
                else
                    roomData[key] = value;

                if (value != null)
                {
                    Hashtable updatedProperties = new Hashtable();
                    updatedProperties.Add(key, value);
                    if (OnRoomCustomPropertiesChangedCallback != null)
                        OnRoomCustomPropertiesChangedCallback(updatedProperties);
                }
            }
            else // Online
            {
                ExitGames.Client.Photon.Hashtable photonHTable = new ExitGames.Client.Photon.Hashtable();
                photonHTable.Add(key, value);
                PhotonNetwork.room.SetCustomProperties(photonHTable);
            }
        }

        static public int GetRoomCustomPropertyAsInt(object key)
        {
            return int.Parse(GetRoomCustomProperty(key));
        }

        static public string GetRoomCustomProperty(object key)
        {
            string virtualPlayerKey = PhotonNetwork.room.Name + Separators.VIRTUAL_PLAYER_KEY + key;

            if (IsPhotonOffline())
            {
                if (!Data.ContainsKey(DataGroups.INFO))
                    return "";

                if (!Data[DataGroups.INFO].ContainsKey(DataKeys.ROOM))
                    return "";

                Hashtable userData = Data[DataGroups.INFO][DataKeys.ROOM] as Hashtable;
                return userData[virtualPlayerKey].ToString().Trim('"');
            }
            else  // Online
                return PhotonNetwork.room.CustomProperties[virtualPlayerKey].ToString();
        }

        /*
        ///<summary>
        /// Set room custom properties
        /// </summary>
        static public void SetRoomCustomProperties(Hashtable systemHTable)
        {
            ExitGames.Client.Photon.Hashtable photonHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in systemHTable)
                photonHTable.Add(pair.Key, pair.Value);

            PhotonNetwork.room.SetCustomProperties(photonHTable);
        }
        */

        /*
        ///<summary>
        /// Set room custom properties with expected values
        /// </summary>
        static public void SetRoomCustomProperties(Hashtable systemHTable, Hashtable expectedHTable)
        {
            ExitGames.Client.Photon.Hashtable photonHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in systemHTable)
                photonHTable.Add(pair.Key, pair.Value);

            ExitGames.Client.Photon.Hashtable photonExpectedHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in expectedHTable)
                photonExpectedHTable.Add(pair.Key, pair.Value);

            PhotonNetwork.room.SetCustomProperties(photonHTable, photonExpectedHTable);
        }
        */

        //Instantiate network object
        static public GameObject InstantiateObject(string prefab, Vector3 vec, Quaternion quat, byte group)
        {
            return PhotonNetwork.Instantiate(prefab, vec, quat, group, null);
        }

        static public GameObject InstantiateSceneObject(string prefab, Vector3 vec, Quaternion quat, byte group)
        {
            return PhotonNetwork.InstantiateSceneObject(prefab, vec, quat, group, null);
        }

        static public PhotonView GetLocalPlayerPhotonView()
        {
            return PhotonView.Get(Instance);
        }

        //Get PhotonView of network object
        static public PhotonView GetPhotonView(GameObject obj)
        {
            if (obj != null)
                return PhotonView.Get(obj);

            return null;
        }

        //Get Photon Network time
        static public double GetPhotonTime()
        {
            return PhotonNetwork.time;
        }

        //Load another scene with every player in the room
        static public void PhotonLoadLevel(string sceneName)
        {
            PhotonNetwork.LoadLevel(sceneName);
        }

        //Destroy a Photon Instantiated GameObject
        static public void PhotonDestroy(GameObject gameObj)
        {
            PhotonNetwork.Destroy(gameObj);
        }

        //Destroy a Photon Instantiated GameObject with delay
        static public IEnumerator PhotonDestroyWithDelayCoroutine(GameObject gameObj, float delay)
        {
            yield return new WaitForSeconds(delay);
            PhotonNetwork.Destroy(gameObj);
        }

        //Whether the user wants to automatically sync scene with the master
        static public void SetAutomaticallySyncScene(bool automaticallySyncScene)
        {
            PhotonNetwork.automaticallySyncScene = automaticallySyncScene;
        }

        //Whether the user wants to have Photon automatically clean up objects
        //in a room
        static public void SetAutoCleanUpPlayerObjects(bool autoCleanUpPlayerObjects)
        {
            PhotonNetwork.autoCleanUpPlayerObjects = autoCleanUpPlayerObjects;
        }

        //Helper for setting a reference to the player's character
        static public void SetLocalPlayerCharacter(GameObject playerCharacter)
        {
            _localPlayerCharacter = playerCharacter;
        }

        static public GameObject GetLocalPlayerCharacter()
        {
            return _localPlayerCharacter;
        }

        //PUN event callbacks
        private void OnConnectedToPhoton()
        {
            Debug.Log("NetworkManager::OnConnectedToPhoton");
        }

        private void OnConnectionFail(DisconnectCause cause)
        {
            Debug.Log("NetworkManager::OnConnectionFail -> cause: " + cause.ToString());
        }

        void OnJoinedLobby()
        {
            Debug.Log("NetworkManager::Joined Lobby");

            //instance.StartCoroutine(LobbyPollCoroutine());

            Instance.StartCoroutine(LobbyPollRoomsCoroutine());
            //instance.StartCoroutine(GetRoomDataCoroutine());

            if (OnJoinedLobbyCallback != null)
                OnJoinedLobbyCallback();
        }

        void OnLeftLobby()
        {
            Debug.Log("NetworkManager::OnLeftLobby");
            //Transaction(WGO2Configs.LeaveChatTransaction, new Hashtable(), LeaveChatLobby);

            if (OnLeftLobbyCallback != null)
                OnLeftLobbyCallback();
        }

        void OnJoinedRoom()
        {
            Debug.Log("NetworkManager::OnJoinedRoom");

            //UNCOMMENT WHEN LINKING GAME TOGETHER WITH STAGING SCENE
            //Debug.Log("Joined Room " + PhotonNetwork.room.name);
            //Debug.Log ("PlayerCount: " + PhotonNetwork.room.playerCount + " MaxPlayers: " + PhotonNetwork.room.maxPlayers);

            //FOR TESTING
            /*
            if (NetworkManager.GetIsMasterClient())
            {
                Hashtable roomReadyProp = new Hashtable();
                roomReadyProp.Add("gState", NetworkManager.ROOM_STATE_READY);
                NetworkManager.SetRoomCustomProperties(roomReadyProp);
            }
            */

            if (OnJoinedRoomCallback != null)
                OnJoinedRoomCallback();
        }

        void OnLeftRoom()
        {
            Debug.Log("NetworkManager::OnLeftRoom");

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (OnLeftRoomCallback != null)
                OnLeftRoomCallback();
        }

        void OnPhotonPlayerConnected(PhotonPlayer connectedPlayer)
        {
            Debug.Log("NetworkManager::Player: " + connectedPlayer.NickName + " connected");

            //if (GetRoom().customProperties["gState"].ToString() == ROOM_STATE_STAGING)

            //UpdateGameRoomInfo(GetRoom().CustomProperties["gm"].ToString(),
            //    GetRoom().CustomProperties["sm"].ToString(),
            //    GetRoom().PlayerCount.ToString(),
            //    GetRoom().CustomProperties["mp"].ToString());

            if (GetIsMasterClient())
            {
                //UpdateRoomList();
                if (CheckReadyToLaunch != null) CheckReadyToLaunch();
            }

            if (OnPlayerConnectedCallback != null)
                OnPlayerConnectedCallback(connectedPlayer);
        }

        void OnPhotonPlayerDisconnected(PhotonPlayer disconnectedPlayer)
        {
            Debug.Log("NetworkManager::Player: " + disconnectedPlayer.NickName + " disconnected");

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            //If the room state is currently in ready state
            //If a player disconnects, do a recount of ready players again
            if (GetRoom().CustomProperties[RoomPropertyOptions.GAME_STATE].ToString() == RoomStates.READY)
            {
                if (GetIsMasterClient())
                {
                    Debug.Log("game state ready recount");
                    CountNumberOfReadyPlayers();
                }
            }
            //If the room state is currently in sync state
            else if (GetRoom().CustomProperties[RoomPropertyOptions.GAME_STATE].ToString() == RoomStates.SYNC)
            {
                if (GetIsMasterClient())
                    SendNotifyClientsToSendPlayerInfoMessage();

                //Debug.LogError("game state sync recount");
                //CountNumberOfSyncPlayers();
            }
            //If the room state is currently in launch state
            //else if (GetRoom().CustomProperties["gState"].ToString() == RoomStates.LAUNCH)
            //{
            //    if (GetIsMasterClient())
            //    {
            //        Debug.LogError("game state launch recount");
            //        CountNumberOfLaunchPlayers();
            //    }
            //}
            //If the room state is currently in play state
            else if (GetRoom().CustomProperties[RoomPropertyOptions.GAME_STATE].ToString() == RoomStates.PLAY)
            {

            }

            //Have the master delete the objects of whoever disconnected
            if (GetIsMasterClient())
            {
                Debug.Log("Delete object");
                if (DestroyDisconnectedPlayerObject != null) DestroyDisconnectedPlayerObject(disconnectedPlayer);
            }

            if (OnPlayerDisconnectedCallback != null)
                OnPlayerDisconnectedCallback(disconnectedPlayer);
        }

        void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
        {
            Debug.Log("NetworkManager::OnPhotonPlayerPropertiesChanged");

            PhotonPlayer player = playerAndUpdatedProps[0] as PhotonPlayer;
            ExitGames.Client.Photon.Hashtable photonUpdatedProps = playerAndUpdatedProps[1] as ExitGames.Client.Photon.Hashtable;
            Hashtable updatedProperties = new Hashtable();
            foreach (object key in photonUpdatedProps.Keys)
                updatedProperties.Add(key, photonUpdatedProps[key]);

            //Debug.LogError("Player Properties that changed: " + player.NickName + " " + playerProperties.ToStringFull());
            /*
            if (GetInRoom())
            {
                ExitGames.Client.Photon.Hashtable roomProperties = GetRoom().CustomProperties;
                if (roomProperties[RoomPropertyOptions.GAME_STATE] != null)
                {
                    if (roomProperties[RoomPropertyOptions.GAME_STATE].ToString() == RoomStates.READY)
                    {
                        if (photonUpdatedProps[PlayerPropertyOptions.PLAYER_STATE] != null)
                        {
                            if (photonUpdatedProps[PlayerPropertyOptions.PLAYER_STATE].ToString() == PlayerStates.SPAWN)
                            {
                                //                            if (SpawnPlayerCharacter != null) 
                                //								SpawnPlayerCharacter(player);
                            }
                            else if (photonUpdatedProps[PlayerPropertyOptions.PLAYER_STATE].ToString() == PlayerStates.CREATED)
                            {


                                if (EnableReadyUI != null)
                                    EnableReadyUI();
                            }
                            else if (photonUpdatedProps[PlayerPropertyOptions.PLAYER_STATE].ToString() == PlayerStates.READY)
                            {
                                if (GetIsMasterClient())
                                    CountNumberOfReadyPlayers();
                            }
                        }
                    }
                    else if (roomProperties[RoomPropertyOptions.GAME_STATE].ToString() == RoomStates.SYNC)
                    {
                        //A player synced properly in the Combat scene
                        if ((photonUpdatedProps[PlayerPropertyOptions.PLAYER_STATE] != null) && (photonUpdatedProps[PlayerPropertyOptions.PLAYER_STATE].ToString() == "sync"))
                            CountNumberOfSyncPlayers();
                    }
                    else if (roomProperties[RoomPropertyOptions.GAME_STATE].ToString() == RoomStates.PLAY)
                    {
                        checkPlayerPropertiesOnPlay(player, photonUpdatedProps);
                    }
                }
            }
            */
            if (OnPlayerCustomPropertiesChangedCallback != null)
                OnPlayerCustomPropertiesChangedCallback(player, updatedProperties);
        }

        void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable photonUpdatedProps)
        {
            Debug.Log("NetworkManager::OnPhotonCustomRoomPropertiesChanged-> Room Properties that changed: " + photonUpdatedProps.ToStringFull());
            Hashtable updatedProperties = convertPhotonHashTable(photonUpdatedProps);
            /*
            //Room state changes
            if (photonUpdatedProps.ContainsKey(RoomPropertyOptions.GAME_STATE))
            {
                if (photonUpdatedProps[RoomPropertyOptions.GAME_STATE].ToString() == RoomStates.READY)
                {
                    //Load stuff here...
                    //if (SpawnPlayerCharacterTesting != null) SpawnPlayerCharacterTesting();
                }
                else if (photonUpdatedProps[RoomPropertyOptions.GAME_STATE].ToString() == RoomStates.SYNC)
                {
                    //Master sends sync message here...
                    if (GetIsMasterClient())
                        NetworkManager.SendNotifyClientsToSendPlayerInfoMessage();
                }
                else if (photonUpdatedProps[RoomPropertyOptions.GAME_STATE].ToString() == RoomStates.LAUNCH)
                {
                    //Master sets the start photon timer here...
                    if (GetIsMasterClient())
                        SetRoomCustomProperty(RoomPropertyOptions.START_COUNT_DOWN_TIMER, GetPhotonTime());
                }
                else if (photonUpdatedProps[RoomPropertyOptions.GAME_STATE].ToString() == RoomStates.PLAY)
                {

                }
            }
            */

            if (photonUpdatedProps.ContainsKey(RoomPropertyOptions.START_COUNT_DOWN_TIMER))
            {
                //Begin timer here
                Debug.Log("Start time: " + (double)photonUpdatedProps[RoomPropertyOptions.START_COUNT_DOWN_TIMER]);
                if (StartCountdownTimer != null)
                    StartCountdownTimer((double)photonUpdatedProps[RoomPropertyOptions.START_COUNT_DOWN_TIMER]);
            }

            if (OnRoomCustomPropertiesChangedCallback != null)
                OnRoomCustomPropertiesChangedCallback(updatedProperties);
        }

        void OnMasterClientSwitched(PhotonPlayer newMaster)
        {
            Debug.Log("NetworkManager::OnMasterClientSwitched -> New master name: " + newMaster.NickName);

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            //if (sceneName == "Combat")
            //{
            //    if (GetRoom().CustomProperties["gState"].ToString() == ROOM_STATE_STAGING)
            //    {
            //        if (GetIsMasterClient())
            //        {
            //            Hashtable roomReadyProp = new Hashtable();
            //            roomReadyProp.Add("gState", NetworkManager.ROOM_STATE_READY);
            //            NetworkManager.SetRoomCustomProperties(roomReadyProp);
            //        }
            //    }
            //}

            if (OnMasterClientSwitchedCallback != null)
                OnMasterClientSwitchedCallback(newMaster);
        }

        void OnUpdatedFriendList()
        {
            Debug.Log("NetworkManager::OnUpdatedFriendList");

            if (GetInLobby() == true)
            {
                //Debug.Log("NetworkManager::OnUpdatedFriendList");
                if (PhotonNetwork.Friends != null)
                {

                }
                else
                    Debug.Log("PhotonNetwork.Friends is null");
            }

            if (OnUpdatedFriendListCallback != null)
                OnUpdatedFriendListCallback();
        }

        void OnDisconnectedFromPhoton()
        {
            Debug.Log("NetworkManager::OnDisconnectedFromPhoton");

            //FOR TESTING
            Application.Quit();

            if (OnDisconnectedFromPhotonCallback != null)
                OnDisconnectedFromPhotonCallback();
        }

        //Master sends message to every client when everyone is ready to load
        static public void SendNotifyClientsToSendPlayerInfoMessage()
        {
            GetLocalPlayerPhotonView().RPC("NotifyClientsToSendPlayerInfoMessage", PhotonTargets.All);
        }

        [PunRPC]
        private void NotifyClientsToSendPlayerInfoMessage(PhotonMessageInfo info)
        {
            int playerPhotonViewId = _localPlayerCharacter.GetComponent<PhotonView>().viewID;
            GetLocalPlayerPhotonView().RPC("SyncPlayerInfoMessage", PhotonTargets.All, playerPhotonViewId);
        }

        [PunRPC]
        private void SyncPlayerInfoMessage(int playerPhotonViewId, PhotonMessageInfo info, string virtualPlayerId)
        {
            //Id was successfully retrieved
            if (playerPhotonViewId != -1)
            {
                if (SetPlayerNameAsObjName != null)
                    SetPlayerNameAsObjName(playerPhotonViewId, info.sender.NickName);

                if (GetIsMasterClient())
                {
                    PhotonView playerPhotonView = PhotonView.Find(playerPhotonViewId);
                    NetworkManager.SetAnyPlayerCustomProperty(PlayerPropertyOptions.PLAYER_STATE, PlayerStates.SYNC, virtualPlayerId, playerPhotonView.owner);
                }
            }
            //Id was not successfully retrieved, do something here... 
            else
            {

            }
        }


        //Helpers for RPCs
        private void CountNumberOfReadyPlayers()
        {
            int numberOfPlayersReady = 0;

            foreach (PhotonPlayer p in NetworkManager.GetPlayerList())
            {
                System.Object playerGameStateProp = p.CustomProperties[PlayerPropertyOptions.PLAYER_STATE];

                if (playerGameStateProp != null)
                {
                    string playerGameState = playerGameStateProp.ToString();

                    if (playerGameState == PlayerStates.READY)
                        numberOfPlayersReady++;
                }
            }

            if (numberOfPlayersReady == NetworkManager.GetPlayerCount())
            {
                //TEST
                //if (OriginalMaster) PhotonNetwork.Disconnect();

                SetRoomCustomProperty(RoomPropertyOptions.GAME_STATE, RoomStates.SYNC);

                //TEST
                //if (OriginalMaster) PhotonNetwork.Disconnect();
            }
        }

        private void CountNumberOfSyncPlayers()
        {
            int numberOfPlayersSynced = 0;

            foreach (PhotonPlayer p in NetworkManager.GetPlayerList())
            {
                System.Object playerGameStateProp = p.CustomProperties[PlayerPropertyOptions.PLAYER_STATE];

                if (playerGameStateProp != null)
                {
                    string playerGameState = playerGameStateProp.ToString();

                    //if (playerGameState == "sync" || playerGameState == "launch")
                    if (playerGameState == PlayerStates.SYNC)
                        numberOfPlayersSynced++;
                }
            }

            if (numberOfPlayersSynced == NetworkManager.GetPlayerCount())
            {
                //TEST
                //if (OriginalMaster) PhotonNetwork.Disconnect();

                //if (InitializePlayerUI != null) InitializePlayerUI();

                if (GetIsMasterClient())
                    SetRoomCustomProperty(RoomPropertyOptions.GAME_STATE, RoomStates.LAUNCH);

                //TEST
                //if (OriginalMaster) PhotonNetwork.Disconnect();
            }
        }

        //Methods for Chat.cs
        //Coroutine for joining lobby chat 
        static public void InitializeChat()
        {
            Instance.StartCoroutine(AutoJoinChatLobbyCoroutine());
        }

        //Wait until user connects to Photon lobby, then use transaction 8
        //to enter server chat lobby
        static public IEnumerator AutoJoinChatLobbyCoroutine()
        {
            while (PhotonNetwork.insideLobby == false)
                yield return null;

            Hashtable val = new Hashtable();
            //val.Add("room", "1");
            //FOR TESTing
            val.Add(TransactionKeys.ROOM, "2");
            //Transaction(WGO2Configs.EnterLobbyTransaction, val, EnterLobby);
        }

        //Callback for transaction 8
        //Load initial lobby UI
        static public void EnterLobby(JSONNode response)
        {
            JSONNode response_hash = response[0];
            string status = response_hash[TransactionKeys.STATUS].ToString().Trim('"');
            Debug.Log(response_hash);

            if (status == StatusOptions.SUCCESS)
            {
                Instance.StartCoroutine(GetRoomDataCoroutine());

                if (UpdateGamerooms != null)
                    UpdateGamerooms();
            }
        }

        //Callback for transaction 9 
        //Cleanup and disconnect from Photon Network if the user
        //did not enter a Photon Room (left to main menu)
        static public void LeaveChatLobby(JSONNode response)
        {
            JSONNode response_hash = response[0];
            string status = response_hash[TransactionKeys.STATUS].ToString().Trim('"');
            Debug.Log(response_hash);

            if (status == StatusOptions.SUCCESS)
            {
                Instance.StopAllCoroutines();
                LobbyUserList.Clear();
                _updatedLobbyUserList.Clear();

                /*
                if (!GetCreatingOrJoinRoom())
                {
                    PhotonNetwork.Disconnect();
                }
                */
            }
        }

        //Coroutine used to get server chat lobby data every few
        //seconds (only works if connected to Photon lobby)
        static public IEnumerator GetRoomDataCoroutine()
        {
            while (PhotonNetwork.insideLobby)
            {
                if (_stopGetRoomData)
                {
                    //Transaction(WGO2Configs.GetRoomDataTransaction, new Hashtable(), GetRoomData);
                    yield return new WaitForSeconds(2f);
                }
                else
                    yield return null;
            }
        }

        //Callback for transaction 10
        //Reads returned server data
        static public void GetRoomData(JSONNode response)
        {
            JSONNode response_hash = response[0];
            string status = response_hash[TransactionKeys.STATUS].ToString().Trim('"');
            Debug.Log(response_hash);

            if (status == StatusOptions.SUCCESS)
            {
                JSONNode response_userlist = response_hash[TransactionKeys.LIST];
                JSONNode response_chat = response_hash[TransactionKeys.CHAT];

                //List of people in the lobby
                _updatedLobbyUserList.Clear();
                //UPDATED_ROOM_LEVELLIST.Clear();


                for (int i = 0; i < response_userlist.Count; i++)
                {
                    _updatedLobbyUserList.Add(response_userlist[i][TransactionKeys.USER_LOGIN].ToString().Trim('"'));
                    //UPDATED_ROOM_LEVELLIST.Add(response_userlist[i]["level"].ToString().Trim('"'));
                }

                NetworkManager.CheckOtherPlayersJoinOrLeave();

                if (UpdateGamerooms != null)
                    UpdateGamerooms();
            }
        }

        //Takes read data from GetRoomData and updates UI
        //(whether users joined or left the lobby)
        static private void CheckOtherPlayersJoinOrLeave()
        {
            if (_updatedLobbyUserList.Count < LobbyUserList.Count)
            {
                for (int i = 0; i < LobbyUserList.Count; i++)
                {
                    if (!_updatedLobbyUserList.Contains(LobbyUserList[i]))
                    {
                        string leavetxt = "[00FFFFFF]" + LobbyUserList[i] + "[-] [FF4000FF]has left the room[-]";
                        //UIManager.GetObject("chatText").GetComponent<UITextList>().Add(leavetxt);
                        LobbyUserList.RemoveAt(i);
                        //ROOM_LEVELLIST.RemoveAt(i);
                    }
                }
            }
            else if (_updatedLobbyUserList.Count > LobbyUserList.Count)
            {
                for (int i = 0; i < _updatedLobbyUserList.Count; i++)
                {
                    if (!LobbyUserList.Contains(_updatedLobbyUserList[i]))
                    {
                        //string jointxt = "[00FFFFFF]" + UpdatedLobbyUserList[i] + "[-] [FF4000FF]has joined the room[-]";
                        string jointxt = "<color=00FFFFFF>" + _updatedLobbyUserList[i] + "</color><color=FF4000FF>has joined the room</color>";

                        //UIManager.GetObject("chatText").GetComponent<UITextList>().Add(jointxt);
                        LobbyUserList.Add(_updatedLobbyUserList[i]);
                        //ROOM_LEVELLIST.Add(UPDATED_ROOM_LEVELLIST[i]);
                    }
                }
            }
        }

        //Coroutine for polling Lobby info (specifically friend info)
        static public IEnumerator LobbyPollCoroutine()
        {
            while (PhotonNetwork.insideLobby == true)
                yield return new WaitForSeconds(5f);
        }

        //Coroutine for polling Lobby info (specifically room info)
        static public IEnumerator LobbyPollRoomsCoroutine()
        {
            while (PhotonNetwork.insideLobby == true)
            {
                PhotonNetwork.GetRoomList();
                //ReadRoomList();
                yield return null;
            }
        }

        private Hashtable convertPhotonHashTable(ExitGames.Client.Photon.Hashtable photonHashTable)
        {
            Hashtable hashTable = new Hashtable();
            foreach (object key in photonHashTable.Keys)
                hashTable.Add(key, photonHashTable[key]);

            return hashTable;
        }

        private void checkPlayerPropertiesOnPlay(PhotonPlayer photonPlayer, ExitGames.Client.Photon.Hashtable playerProperties)
        {

        }
    }
}
