using UnityEngine;
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

        //Make an Enum for room state later
        static public string ROOM_STATE_STAGING = "Staging";
        static public string ROOM_STATE_READY = "Ready";
        static public string ROOM_STATE_SYNC = "Sync";
        static public string ROOM_STATE_LAUNCH = "Launch";
        static public string ROOM_STATE_PLAY = "Play";

        static public bool OriginalMaster = false;

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

        //Delegate for setting a player's initial health
        public delegate void SetHealthDelegate(PhotonPlayer player, int health);
        static public SetHealthDelegate SetHealth;

        //Delegate for modifying a player's health
        public delegate void ModifyHealthDelegate(PhotonPlayer player, int newHealth/*, int userPhotonViewId*/);
        static public ModifyHealthDelegate ModifyHealth;

        //Delegate for the master to destroy objects of players who left
        public delegate void DestroyDisconnectedPlayerObjectDelegate(PhotonPlayer player);
        static public DestroyDisconnectedPlayerObjectDelegate DestroyDisconnectedPlayerObject;

        public delegate void OnPhotonPlayerConnectedDelegate(PhotonPlayer connectedPlayer);
        static public OnPhotonPlayerConnectedDelegate OnPhotonPlayerConnectedCallback;

        public delegate void OnPhotonPlayerDisconnectedDelegate(PhotonPlayer disconnectedPlayer);
        static public OnPhotonPlayerDisconnectedDelegate OnPhotonPlayerDisconnectedCallback;

        public delegate void OnPhotonPlayerPropertiesChangedDelegate(object[] playerAndUpdatedProps);
        static public OnPhotonPlayerPropertiesChangedDelegate OnPhotonPlayerPropertiesChangedCallback;

        public delegate void OnPhotonCustomRoomPropertiesChangedDelegate(ExitGames.Client.Photon.Hashtable propertiesThatChanged);
        static public OnPhotonCustomRoomPropertiesChangedDelegate OnPhotonCustomRoomPropertiesChangedCallback;

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
            Debug.Log("This should be called");
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

        static public void Transaction (NetworkTransactions id, Hashtable val, Callback func = null, Callback local = null, TextureCallback texture = null)
        {
            Debug.Log(Instance);
            Instance.StartCoroutine (httpRequest (id, val, func, local, texture));
        }

        static private IEnumerator httpRequest (NetworkTransactions id, Hashtable val, Callback func, Callback local, TextureCallback texture)
        {
            string url = _serverUrl + "/trans/" + id + ".php";
            string sec = "";

            WWWForm formData = new WWWForm ();
            formData.AddField ("tid", (int) id);
            if (_sessionID != null) 
                formData.AddField ("ssid", _sessionID);
            
            foreach (DictionaryEntry pair in val)
            {
                Debug.Log (pair.Key + " " + pair.Value);
                if (pair.Key.ToString() == "image") 
                    formData.AddBinaryData("imageUpload", pair.Value as byte[], "image.png", "image/png");
                else
                {
                    //string value = Md5Sum(Application.identifier + Application.version + (string)pair.Value);
                    formData.AddField((string)pair.Key, (string)pair.Value);
                    sec += (string)pair.Value;
                }
            }

            formData.AddField("bundle_id", Application.identifier);
            formData.AddField("game", _game);

            sec += Application.identifier + _game;
            sec = md5(sec);

            formData.AddField("sec", sec);

            Debug.Log ("url: " + url);
            var www = UnityWebRequest.Post(url, formData);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log (www.error + " on transaction: " + id.ToString ());

                JSONNode response = JSON.Parse(www.error);

                if (local != null) {
                    local(null);
                }
                func(null);

                #if UNITY_ANDROID
                EtceteraAndroid.showAlert ("Network Error", "Error connecting to the server. Restart the app and retry.", "OK");
                #endif

                #if UNITY_IPHONE
                var buttons = new string[] { "OK" };
                #endif

            }
            else
            {
                if (func != null)
                {
                    JSONNode response = JSON.Parse (www.downloadHandler.text);

                    if (local != null)
                        local (response);

                    func (response);
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

        static public void Register(string userName, string password, string email, string ethAddress, string game, string bundleId, Callback func = null, Hashtable extras = null)
        {
            Hashtable val = new Hashtable();

            val.Add("username", userName);
            val.Add("password", password);
            val.Add("email", email);
            val.Add("game", game);
            val.Add("bundle_id", bundleId);

            if (extras != null)
                val.Merge(extras);

            Transaction(NetworkTransactions.Registration, val, func, RegistrationResult);
        }

        static private void RegistrationResult(JSONNode response)
        {
            JSONNode response_hash = response[0];
            string status = response_hash["status"].ToString().Trim('"');
            string ssid = response_hash["ssid"].ToString().Trim('"');

            Debug.Log("RegistrationResult: " + response_hash.ToString());

            if (status == "SUCCESS")
                NetworkManager.SetSessionID(ssid);
        }

        static public void Login (string user, string pw, Callback func = null, Hashtable extras = null)
        {
            Hashtable val = new Hashtable ();
            val.Add ("user", user);
            val.Add ("pwhash", pw);

            if (extras != null)
                val.Merge (extras);

            Transaction (NetworkTransactions.Login, val, func, LoginResult);
        }

        static private void LoginResult (JSONNode response)
        {
            if (response == null)
                return;

            JSONNode response_hash = response [0];
            string status = response_hash ["status"].ToString ().Trim ('"');
            string ssid = response_hash ["ssid"].ToString ().Trim ('"');

            Debug.Log ("LoginResult: " + response_hash.ToString());

            if (status == "SUCCESS") 
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
            string status = response_hash ["status"].ToString ().Trim ('"');
            JSONNode data = response_hash ["store"];

            if (status == "SUCCESS")
            {
        #if !UNITY_STANDALONE
            //IAPManager.LoadData(data);
        #endif
            }
        }

        static public string GetUserInfo (string val, string defaultValue = "")
        {
            if (!NetworkManager.Data.ContainsKey("Info"))
                return "";

            if (!NetworkManager.Data["Info"].ContainsKey("user"))
                return "";

            Hashtable userData = NetworkManager.Data ["Info"] ["user"] as Hashtable;

                if (userData[val] == null)
                    return defaultValue;

                return userData [val].ToString ().Trim ('"');
        }

        static public void SetUserInfo (string val, string text)
        {
            if (!NetworkManager.Data.ContainsKey ("Info"))
            NetworkManager.Data.Add ("Info", new Hashtable ());
            if (!NetworkManager.Data ["Info"].ContainsKey ("user"))
            NetworkManager.Data ["Info"].Add ("user", new Hashtable ());
            Hashtable userData = NetworkManager.Data ["Info"] ["user"] as Hashtable;
            if (!userData.ContainsKey (val))
            userData.Add (val, text);
            else
            userData [val] = text;
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
                    NetworkManager.Transaction(NetworkTransactions.HeartBeat, new Hashtable(), onHeartBeat);
                    yield return new WaitForSeconds(300.0f);
                }
            }
        }

        //Wrappers for Photon calls
        //Connect to Photon Network
        static public void Connect(bool isOffline)
        {
            if (GetConnected())
            {
                if (GetInRoom())
                    LeaveRoom();
                
                return;
            }

            Instance.gameObject.AddComponent<PhotonView>();
            PhotonNetwork.ConnectUsingSettings("v1.0");
            PhotonNetwork.logLevel = PhotonLogLevel.Full;
            PhotonNetwork.offlineMode = isOffline;
            PhotonView photonView = PhotonView.Get(Instance);
            photonView.viewID = 1;
        }

        static public void Disconnect()
        {
            PhotonNetwork.Disconnect();
        }

        static public void SendRPCtoAll(string methodName, params object[] parameters)
        {
            SendRPC(methodName, PhotonTargets.All, parameters);
        }

        static public void SendRPC(string methodName, PhotonTargets target, params object[] parameters)
        {
            PhotonView photonView = PhotonView.Get(Instance);
            photonView.RPC(methodName, target, parameters);
        }

        static public bool GetPhotonOfflineMode()
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
            {
                photonHTable.Add(pair.Key, pair.Value);
            }

            _maxPlayersNotSpectating = maxPlayersNotExpectating;

            RoomOptions options = new RoomOptions();
            options.isVisible = isVisible;
            options.isOpen = isOpen;
            options.maxPlayers = (byte)maxPlayers;
            options.customRoomProperties = photonHTable;
            options.customRoomPropertiesForLobby = propsToListInLobby;

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
            return PhotonNetwork.room.playerCount;
        }

        //Get the max number of players that can
        //be in the room
        static public int GetMaxPlayers()
        {
            return PhotonNetwork.room.maxPlayers;
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
        static public PhotonPlayer GetPlayer()
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

        static public void SetPlayerCustomProperty(string key, string value, PhotonView photonView)
        {
            if (GetPhotonOfflineMode())
                SetPlayerOfflineCustomProperty(key, value, photonView);
            else
                SetPlayerOnlineCustomProperty(key, value, photonView.owner);
        }

        static public string GetPlayerCustomProperty(string key, PhotonView photonView)
        {
            if (GetPhotonOfflineMode())
                return GetPlayerOfflineCustomProperty(key, photonView);
            else
                return GetPlayerOnlineCustomProperty(key, photonView.owner);
        }

        static public void SetCustomProperty(object key, object value)
        {
            Hashtable val = new Hashtable();
            val.Add(key, value);
            SetCustomProperties(val);
        }

        //Set player custom properties 
        static public void SetCustomProperties(Hashtable systemHTable)
        {
            ExitGames.Client.Photon.Hashtable photonHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in systemHTable)
            {
                photonHTable.Add(pair.Key, pair.Value);
            }

            PhotonNetwork.player.SetCustomProperties(photonHTable);
        }

        static public object GetCustomProperty(string key)
        {
            return GetCustomProperties()[key];
        }

        //Return player custom properties
        static public ExitGames.Client.Photon.Hashtable GetCustomProperties()
        {
            return PhotonNetwork.player.customProperties;
        }

        /// <summary>
        /// Sets a player's custom property in online mode
        /// </summary>
        /// <param name="key">Key for the property</param>
        /// <param name="value">Value for the property</param>
        /// <param name="player">Player.</param>
        static public void SetPlayerOnlineCustomProperty(object key, object value, PhotonPlayer player)
        {
            Hashtable properties = new Hashtable();
            properties.Add(key, value);

            SetPlayerOnlineCustomProperties(properties, player);
        }

        /// <summary>
        /// Sets a player's custom properties in online mode
        /// </summary>
        /// <param name="systemHTable">System H table.</param>
        /// <param name="player">Player.</param>
        static public void SetPlayerOnlineCustomProperties(Hashtable systemHTable, PhotonPlayer player)
        {
            ExitGames.Client.Photon.Hashtable photonHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in systemHTable)
                photonHTable.Add(pair.Key, pair.Value);

            if (player != null)
                player.SetCustomProperties(photonHTable);
        }

        /// <summary>
        /// Sets a player's custom property in offline mode
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        static public void SetPlayerOfflineCustomProperty(string key, string value)
        {
            SetUserInfo(key, value);
        }

        static public void SetPlayerOfflineCustomProperty(string key, string value, PhotonView photonView)
        {
            SetUserInfo(photonView.viewID.ToString() + "_" + key, value);
        }

        /// <summary>
        /// Gets the player custom property in online mode
        /// </summary>
        /// <returns>The player custom property.</returns>
        /// <param name="key">Key.</param>
        /// <param name="player">Player.</param>
        static public string GetPlayerOnlineCustomProperty(string key, PhotonPlayer player)
        {
            return player.CustomProperties[key].ToString();
        }

        /// <summary>
        /// Gets the player custom property in offline mode
        /// </summary>
        /// <returns>The player custom property.</returns>
        /// <param name="key">Key.</param>
        static public string GetPlayerOfflineCustomProperty(string key)
        {
            return GetUserInfo(key);
        }

        static public string GetPlayerOfflineCustomProperty(string key, PhotonView photonView)
        {
            return GetUserInfo(photonView.viewID.ToString() + "_" + key);
        }

        //Set player custom property
        static public void SetLocalPlayerOnlineCustomProperty(object key, object value)
        {
            Hashtable systemHTable = new Hashtable() { { key, value } };
            SetLocalPlayerOnlineCustomProperties(systemHTable);
        }

        //Set player custom properties 
        static public void SetLocalPlayerOnlineCustomProperties(Hashtable systemHTable)
        {
            ExitGames.Client.Photon.Hashtable photonHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in systemHTable)
            {
                photonHTable.Add(pair.Key, pair.Value);
            }

            PhotonNetwork.player.SetCustomProperties(photonHTable);
        }

        //Return player custom properties
        static public ExitGames.Client.Photon.Hashtable GetPlayerOnlineCustomProperties()
        {
            return PhotonNetwork.player.customProperties;
        }

        ///<summary>
        /// Set another player's custom property
        /// </summary>
        static public void SetOtherPlayerOnlineCustomProperty(PhotonPlayer otherPlayer, object key, object value)
        {
            Hashtable systemHTable = new Hashtable() { { key, value } };
            SetOtherPlayerOnlineCustomProperties(otherPlayer, systemHTable);
        }

        ///<summary>
        /// Set another player's custom properties
        /// </summary>
        static public void SetOtherPlayerOnlineCustomProperties(PhotonPlayer otherPlayer, Hashtable systemHTable)
        {
            ExitGames.Client.Photon.Hashtable photonHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in systemHTable)
            {
                photonHTable.Add(pair.Key, pair.Value);
            }

            otherPlayer.SetCustomProperties(photonHTable);
        }

        ///<summary>
        /// Set another player's custom properties with expected values
        /// </summary>
        static public void SetOtherPlayerOnlineCustomProperties(PhotonPlayer otherPlayer, Hashtable systemHTable, Hashtable expectedHTable)
        {
            ExitGames.Client.Photon.Hashtable photonHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in systemHTable)
            {
                photonHTable.Add(pair.Key, pair.Value);
            }

            ExitGames.Client.Photon.Hashtable photonExpectedHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in expectedHTable)
            {
                photonExpectedHTable.Add(pair.Key, pair.Value);
            }

            otherPlayer.SetCustomProperties(photonHTable, photonExpectedHTable);
        }

        //Helper for testing
        static public void ReadPlayerList(PhotonPlayer[] playerList)
        {
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                Debug.Log(string.Format("Player {0}: ", i + 1) + playerList[i].name);
            }

        }

        ///<summary>
        /// Set room custom property
        /// </summary>
        static public void SetRoomCustomProperty(object key, object value)
        {
            Hashtable systemHTable = new Hashtable() { { key, value } };
            SetRoomCustomProperties(systemHTable);
        }

        ///<summary>
        /// Set room custom properties
        /// </summary>
        static public void SetRoomCustomProperties(Hashtable systemHTable)
        {
            ExitGames.Client.Photon.Hashtable photonHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in systemHTable)
            {
                photonHTable.Add(pair.Key, pair.Value);
            }

            PhotonNetwork.room.SetCustomProperties(photonHTable);
        }

        ///<summary>
        /// Set room custom properties with expected values
        /// </summary>
        static public void SetRoomCustomProperties(Hashtable systemHTable, Hashtable expectedHTable)
        {
            ExitGames.Client.Photon.Hashtable photonHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in systemHTable)
            {
                photonHTable.Add(pair.Key, pair.Value);
            }

            ExitGames.Client.Photon.Hashtable photonExpectedHTable = new ExitGames.Client.Photon.Hashtable();

            foreach (DictionaryEntry pair in expectedHTable)
            {
                photonExpectedHTable.Add(pair.Key, pair.Value);
            }

            PhotonNetwork.room.SetCustomProperties(photonHTable, photonExpectedHTable);
        }

        //Instantiate network object
        static public GameObject InstantiateObject(string prefab, Vector3 vec, Quaternion quat, byte group)
        {
            return PhotonNetwork.Instantiate(prefab, vec, quat, group, null);
        }

        static public GameObject InstantiateSceneObject(string prefab, Vector3 vec, Quaternion quat, byte group)
        {
            return PhotonNetwork.InstantiateSceneObject(prefab, vec, quat, group, null);
        }

        //Get PhotonView of network object
        static public PhotonView GetPhotonView(GameObject obj)
        {
            if (obj != null) return PhotonView.Get(obj);

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
        void OnJoinedLobby()
        {
            Debug.LogError("Joined Lobby");

            //instance.StartCoroutine(LobbyPollCoroutine());

            Instance.StartCoroutine(LobbyPollRoomsCoroutine());
            //instance.StartCoroutine(GetRoomDataCoroutine());

            if (OnJoinedLobbyCallback != null)
                OnJoinedLobbyCallback();
        }

        void OnLeftLobby()
        {
            //Transaction(WGO2Configs.LeaveChatTransaction, new Hashtable(), LeaveChatLobby);

            if (OnLeftLobbyCallback != null)
                OnLeftLobbyCallback();
        }

        void OnJoinedRoom()
        {
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
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            //if (sceneName == "Combat")
            //{
            //    //Clear player faction info
            //    Hashtable val = new Hashtable();
            //    val.Add("faction", null);
            //    NetworkManager.SetLocalPlayerOnlineCustomProperties(val);

            //    SceneManager.LoadScene("Chat");
            //}
            //else if (sceneName == "Quest")
            //{
            //    NetworkManager.Disconnect();
            //    //SceneManager.LoadScene("Login");
            //    SceneManager.LoadScene("Origin");
            //}

            if (OnLeftRoomCallback != null)
                OnLeftRoomCallback();
        }

        void OnPhotonPlayerConnected(PhotonPlayer connectedPlayer)
        {
            Debug.LogError("Player: " + connectedPlayer.name + " connected");

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

            if (OnPhotonPlayerConnectedCallback != null)
                OnPhotonPlayerConnectedCallback(connectedPlayer);
        }

        void OnPhotonPlayerDisconnected(PhotonPlayer disconnectedPlayer)
        {
            Debug.LogError("Player: " + disconnectedPlayer.name + " disconnected");

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            //If the room state is currently in ready state
            //If a player disconnects, do a recount of ready players again
            if (GetRoom().CustomProperties["gState"].ToString() == ROOM_STATE_READY)
            {
                if (GetIsMasterClient())
                {
                    Debug.LogError("game state ready recount");
                    CountNumberOfReadyPlayers();
                }
            }
            //If the room state is currently in sync state
            else if (GetRoom().CustomProperties["gState"].ToString() == ROOM_STATE_SYNC)
            {
                if (GetIsMasterClient())
                {
                    SendNotifyClientsToSendPlayerInfoMessage();
                }

                //Debug.LogError("game state sync recount");
                //CountNumberOfSyncPlayers();
            }
            //If the room state is currently in launch state
            //else if (GetRoom().CustomProperties["gState"].ToString() == ROOM_STATE_LAUNCH)
            //{
            //    if (GetIsMasterClient())
            //    {
            //        Debug.LogError("game state launch recount");
            //        CountNumberOfLaunchPlayers();
            //    }
            //}
            //If the room state is currently in play state
            else if (GetRoom().CustomProperties["gState"].ToString() == ROOM_STATE_PLAY)
            {

            }

            //Have the master delete the objects of whoever disconnected
            if (GetIsMasterClient())
            {
                Debug.LogError("Delete object");
                if (DestroyDisconnectedPlayerObject != null) DestroyDisconnectedPlayerObject(disconnectedPlayer);
            }

            if (OnPhotonPlayerDisconnectedCallback != null)
                OnPhotonPlayerDisconnectedCallback(disconnectedPlayer);
        }

        void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
        {
            PhotonPlayer player = playerAndUpdatedProps[0] as PhotonPlayer;
            ExitGames.Client.Photon.Hashtable playerProperties = playerAndUpdatedProps[1] as ExitGames.Client.Photon.Hashtable;

            //Debug.LogError("Player Properties that changed: " + player.NickName + " " + playerProperties.ToStringFull());

            if (GetInRoom())
            {
                ExitGames.Client.Photon.Hashtable roomProperties = GetRoom().CustomProperties;
                if (roomProperties["gState"] != null)
                {
                    if (roomProperties["gState"].ToString() == ROOM_STATE_READY)
                    {
                        if (playerProperties["plGState"] != null)
                        {
                            if (playerProperties["plGState"].ToString() == "spawn")
                            {
                                //                            if (SpawnPlayerCharacter != null) 
                                //								SpawnPlayerCharacter(player);
                            }
                            else if (playerProperties["plGState"].ToString() == "created")
                            {
                                if (playerProperties["health"] != null)
                                {
                                    if (SetHealth != null)
                                        SetHealth(player, (int)playerProperties["health"]);
                                }

                                if (EnableReadyUI != null)
                                    EnableReadyUI();
                            }
                            else if (playerProperties["plGState"].ToString() == "ready")
                            {
                                if (GetIsMasterClient())
                                    CountNumberOfReadyPlayers();
                            }
                        }
                    }
                    else if (roomProperties["gState"].ToString() == ROOM_STATE_SYNC)
                    {
                        //A player synced properly in the Combat scene
                        if ((playerProperties["plGState"] != null) && (playerProperties["plGState"].ToString() == "sync"))
                            CountNumberOfSyncPlayers();
                    }
                    else if (roomProperties["gState"].ToString() == ROOM_STATE_PLAY)
                    {
                        checkPlayerPropertiesOnPlay(player, playerProperties);
                    }
                }
            }

            if (OnPhotonPlayerPropertiesChangedCallback != null)
                OnPhotonPlayerPropertiesChangedCallback(playerAndUpdatedProps);
        }

        void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            //Debug.LogError("Room Properties that changed: " + propertiesThatChanged.ToStringFull());     

            //Room state changes
            if (propertiesThatChanged.ContainsKey("gState"))
            {
                if (propertiesThatChanged["gState"].ToString() == ROOM_STATE_READY)
                {
                    //Load stuff here...
                    //if (SpawnPlayerCharacterTesting != null) SpawnPlayerCharacterTesting();
                }
                else if (propertiesThatChanged["gState"].ToString() == ROOM_STATE_SYNC)
                {
                    //Master sends sync message here...
                    if (GetIsMasterClient())
                    {
                        NetworkManager.SendNotifyClientsToSendPlayerInfoMessage();
                    }
                }
                else if (propertiesThatChanged["gState"].ToString() == ROOM_STATE_LAUNCH)
                {
                    //Master sets the start photon timer here...
                    if (GetIsMasterClient())
                    {
                        Hashtable startTimeProp = new Hashtable();
                        startTimeProp.Add("st", GetPhotonTime());
                        SetRoomCustomProperties(startTimeProp);
                    }
                }
                else if (propertiesThatChanged["gState"].ToString() == ROOM_STATE_PLAY)
                {

                }
            }

            if (propertiesThatChanged.ContainsKey("st"))
            {
                //Begin timer here
                Debug.LogError("Start time: " + (double)propertiesThatChanged["st"]);
                if (StartCountdownTimer != null) StartCountdownTimer((double)propertiesThatChanged["st"]);
            }

            if (propertiesThatChanged.ContainsKey("teamOneIndexes") || propertiesThatChanged.ContainsKey("teamTwoIndexes"))
            {
                //if (SpawnPlayerCharacter != null) SpawnPlayerCharacter();
                Dictionary<int, int> test = (Dictionary<int, int>)propertiesThatChanged["teamTwoIndexes"];

                Debug.LogError("TeamTwoIndex changed: " + test.ToStringFull());
            }

            if (OnPhotonCustomRoomPropertiesChangedCallback != null)
                OnPhotonCustomRoomPropertiesChanged(propertiesThatChanged);
        }

        void OnMasterClientSwitched(PhotonPlayer newMaster)
        {
            Debug.LogError("New master name: " + newMaster.NickName);

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
            if (GetInLobby() == true)
            {
                Debug.Log("OnUpdatedFriendList");
                if (PhotonNetwork.Friends != null)
                {

                }
                else
                {
                    Debug.Log("PhotonNetwork.Friends is null");
                }
            }

            if (OnUpdatedFriendListCallback != null)
                OnUpdatedFriendListCallback();
        }

        void OnDisconnectedFromPhoton()
        {
            //FOR TESTING
            Application.Quit();

            if (OnDisconnectedFromPhotonCallback != null)
                OnDisconnectedFromPhotonCallback();
        }

        //Master sends message to every client when everyone is ready to load
        static public void SendNotifyClientsToSendPlayerInfoMessage()
        {
            PhotonView photonView = PhotonView.Get(Instance);
            photonView.RPC("NotifyClientsToSendPlayerInfoMessage", PhotonTargets.All);
        }

        [PunRPC]
        private void NotifyClientsToSendPlayerInfoMessage(PhotonMessageInfo info)
        {
            int playerPhotonViewId = _localPlayerCharacter.GetComponent<PhotonView>().viewID;

            PhotonView photonView = PhotonView.Get(Instance);
            photonView.RPC("SyncPlayerInfoMessage", PhotonTargets.All, playerPhotonViewId);
        }

        [PunRPC]
        private void SyncPlayerInfoMessage(int playerPhotonViewId, PhotonMessageInfo info)
        {
            //Id was successfully retrieved
            if (playerPhotonViewId != -1)
            {
                if (SetPlayerNameAsObjName != null) SetPlayerNameAsObjName(playerPhotonViewId, info.sender.name);

                if (GetIsMasterClient())
                {
                    PhotonView playerPhotonView = PhotonView.Find(playerPhotonViewId);

                    Hashtable playerSyncProp = new Hashtable();
                    playerSyncProp.Add("plGState", "sync");
                    NetworkManager.SetOtherPlayerOnlineCustomProperties(playerPhotonView.owner, playerSyncProp);
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
                System.Object playerGameStateProp = p.CustomProperties["plGState"];

                if (playerGameStateProp != null)
                {
                    string playerGameState = playerGameStateProp.ToString();

                    if (playerGameState == "ready")
                        numberOfPlayersReady++;
                }
            }

            if (numberOfPlayersReady == NetworkManager.GetPlayerCount())
            {
                //TEST
                //if (OriginalMaster) PhotonNetwork.Disconnect();

                Hashtable roomSyncProp = new Hashtable();
                roomSyncProp.Add("gState", ROOM_STATE_SYNC);
                SetRoomCustomProperties(roomSyncProp);

                //TEST
                //if (OriginalMaster) PhotonNetwork.Disconnect();
            }
        }

        private void CountNumberOfSyncPlayers()
        {
            int numberOfPlayersSynced = 0;

            foreach (PhotonPlayer p in NetworkManager.GetPlayerList())
            {
                System.Object playerGameStateProp = p.CustomProperties["plGState"];

                if (playerGameStateProp != null)
                {
                    string playerGameState = playerGameStateProp.ToString();

                    //if (playerGameState == "sync" || playerGameState == "launch")
                    if (playerGameState == "sync")
                        numberOfPlayersSynced++;
                }
            }

            if (numberOfPlayersSynced == NetworkManager.GetPlayerCount())
            {
                //TEST
                //if (OriginalMaster) PhotonNetwork.Disconnect();

                //if (InitializePlayerUI != null) InitializePlayerUI();

                if (GetIsMasterClient())
                {
                    Hashtable roomLaunchProp = new Hashtable();
                    roomLaunchProp.Add("gState", ROOM_STATE_LAUNCH);
                    SetRoomCustomProperties(roomLaunchProp);
                }

                //Hashtable playerLaunchProp = new Hashtable();
                //playerLaunchProp.Add("plGState", "launch");
                //SetLocalPlayerCustomProperties(playerLaunchProp);

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
            {
                yield return null;
            }

            Hashtable val = new Hashtable();
            //val.Add("room", "1");
            //FOR TESTing
            val.Add("room", "2");
            //Transaction(WGO2Configs.EnterLobbyTransaction, val, EnterLobby);
        }

        //Callback for transaction 8
        //Load initial lobby UI
        static public void EnterLobby(JSONNode response)
        {
            JSONNode response_hash = response[0];
            string status = response_hash["status"].ToString().Trim('"');
            Debug.Log(response_hash);

            if (status == "SUCCESS")
            {
                Instance.StartCoroutine(GetRoomDataCoroutine());

                if (UpdateGamerooms != null) UpdateGamerooms();
            }
        }

        //Callback for transaction 9 
        //Cleanup and disconnect from Photon Network if the user
        //did not enter a Photon Room (left to main menu)
        static public void LeaveChatLobby(JSONNode response)
        {
            JSONNode response_hash = response[0];
            string status = response_hash["status"].ToString().Trim('"');
            Debug.Log(response_hash);

            if (status == "SUCCESS")
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
                {
                    yield return null;
                }
            }
        }

        //Callback for transaction 10
        //Reads returned server data
        static public void GetRoomData(JSONNode response)
        {
            JSONNode response_hash = response[0];
            string status = response_hash["status"].ToString().Trim('"');
            Debug.Log(response_hash);

            if (status == "SUCCESS")
            {
                JSONNode response_userlist = response_hash["list"];
                JSONNode response_chat = response_hash["chat"];

                //List of people in the lobby
                _updatedLobbyUserList.Clear();
                //UPDATED_ROOM_LEVELLIST.Clear();


                for (int i = 0; i < response_userlist.Count; i++)
                {
                    _updatedLobbyUserList.Add(response_userlist[i]["user_login"].ToString().Trim('"'));
                    //UPDATED_ROOM_LEVELLIST.Add(response_userlist[i]["level"].ToString().Trim('"'));
                }

                NetworkManager.CheckOtherPlayersJoinOrLeave();

                if (UpdateGamerooms != null) UpdateGamerooms();
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
            {
                yield return new WaitForSeconds(5f);
            }
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

        private void checkPlayerPropertiesOnPlay(PhotonPlayer photonPlayer, ExitGames.Client.Photon.Hashtable playerProperties)
        {
            //A player's health was modified
            if (playerProperties["health"] != null)
            {
                Debug.LogError(photonPlayer.NickName + ": " + (int)playerProperties["health"]);
                Debug.LogError("userPhotonViewId: " + (int)playerProperties["userPhotonViewId"]);

                if (ModifyHealth != null)
                    ModifyHealth(photonPlayer, (int)playerProperties["health"]/*, (int)playerProperties["userPhotonViewId"]*/);

                if (GetIsMasterClient())
                {
                    if ((int)playerProperties["health"] <= 0)
                    {
                        Debug.LogError(photonPlayer.NickName + " is dead...");
                        //CheckIfTeamLost((WGO2Enums.Faction)photonPlayer.CustomProperties["faction"]);
                    }
                }
            }
        }
    }
}
