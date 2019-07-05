using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Enigma.CoreSystems
{
	public class Ads
	{

		public string appVersion = "1.0";

#if UNITY_ANDROID
        // App ID
        static public string appId = "app42a4a7795d6642b4ae";  //"app168a5d9b97bb47739a";
        // Video zones
        static public string[] zoneId = { "vze64e837cdaeb448494" }; //{ "vz28f417ceecca4ae4b2", "vzf2257354d2b64e08a8" };
		//If not android defaults to setting the zone strings for iOS


#else
		// App ID
		static public string appId = "app970a83943f644f9a90";
		// Video zones
		static public string[] zoneId = { "vzf8e4e97704c4445c87504e" };
		#endif

	}

	public class NetworkManager : Manageable<NetworkManager>
	{

		static private string serverUrl;
		static private string sessionID;
        static private string game;

		static public Dictionary<string, Hashtable> data;
		static public NetworkManager instance;

		static public bool connectedToUNet = false;
		static public bool timeoutDisconnected = false;
		static public bool serverDisconnected = false;
		static public bool readySpawn = false;
		static public bool playerIsServer = false;
		static public int numberOfPlayers = 0;
		static public int readyToSpawnElves = 0;


		protected override void Awake ()
		{
			instance = this;
		}

		protected override void Start ()
		{
			base.Start ();
			data = new Dictionary<string, Hashtable> ();

			//Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
			//Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
			//GameOfWhales.Instance.OnPushDelivered += OnPushDelivered;
		}

        public delegate void Callback (JSONNode data);

		public delegate void TextureCallback (Texture2D data);

		public delegate void ImageCallback ();

		static public void SetServer (string url)
		{
			serverUrl = url;
		}

        static public string GetServer()
        {
            return serverUrl;
        }

        static public void StartHeartBeat(NetworkManager.Callback onHeartBeat)
        {
            instance.StopCoroutine(heartbeat(onHeartBeat));
            instance.StartCoroutine(heartbeat(onHeartBeat));
        }

		static public void SetSessionID (string id)
		{
			sessionID = id;
		}

		static public string GetSessionID ()
		{
			return sessionID;
		}

        static public void SetGame(string gameName)
        {
            game = gameName;
        }

        static public string GetGame()
        {
            return game;
        }

		static public void Transaction (int id, Hashtable val, Callback func = null, Callback local = null, TextureCallback texture = null)
		{
			instance.StartCoroutine (httpRequest (id, val, func, local, texture));
		}

		static IEnumerator httpRequest (int id, Hashtable val, Callback func, Callback local, TextureCallback texture)
		{
			string url = serverUrl + "/trans/" + id + ".php";
            string sec = "";

			WWWForm formData = new WWWForm ();
			formData.AddField ("tid", id);
			if (sessionID != null)
				formData.AddField ("ssid", sessionID);
			foreach (DictionaryEntry pair in val) {
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
            formData.AddField("game", game);

            sec += Application.identifier + game;
            sec = md5(sec);

            formData.AddField("sec", sec);

            WWW www = new WWW (url, formData);
			Debug.Log ("url: " + url);
			yield return www;

			if (www.error != null) {
				Debug.Log (www.error + " on transaction: " + id.ToString ());

                JSONNode response = JSON.Parse(www.error);

                if (local != null)
                    local(null);
                func(null);

#if UNITY_ANDROID
                EtceteraAndroid.showAlert ("Network Error", "Error connecting to the server. Restart the app and retry.", "OK");

#endif
#if UNITY_IPHONE
				var buttons = new string[] { "OK" };
				//EtceteraBinding.showAlertWithTitleMessageAndButtons( "Network Error", "Error connecting to the server. Restart the app and retry.", buttons );
#endif
			} else {
				if (func != null) {
					JSONNode response = JSON.Parse (www.text);

					if (local != null)
						local (response);
					func (response);
				}
				if (texture != null) {

					if (www.texture != null) {
						texture (www.texture);
					}
				}
			}
		}

		static public string url_create_parameters (Hashtable my_hash)
		{
			string parameters = "";
			foreach (DictionaryEntry hash_entry in my_hash) {
				parameters = parameters + "&" + hash_entry.Key + "=" + hash_entry.Value;
			}

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

			for (int i = 0; i < hashBytes.Length; i++) {
				hashString += System.Convert.ToString (hashBytes [i], 16).PadLeft (2, '0');
			}

			return hashString.PadLeft (32, '0');
		}

        static public void Register(int registractionTransactionId, string userName, string password, string email, string game, string bundleId, Callback func = null, Hashtable extras = null)
        {
            Hashtable val = new Hashtable();

            val.Add("username", userName);
            val.Add("password", password);
            val.Add("email", email);
            val.Add("game", game);
            val.Add("bundle_id", bundleId);

            if (extras != null)
                val.Merge(extras);

            Transaction(registractionTransactionId, val, func, RegistrationResult);
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

        static public void Login (int loginTransactionId, string user, string pw, Callback func = null, Hashtable extras = null)
		{
			Hashtable val = new Hashtable ();
			val.Add ("user", user);
			val.Add ("pwhash", pw);

			if (extras != null)
				val.Merge (extras);

			Transaction (loginTransactionId, val, func, LoginResult);

		}

		static private void LoginResult (JSONNode response)
		{
            if (response == null)
                return;

			JSONNode response_hash = response [0];
			string status = response_hash ["status"].ToString ().Trim ('"');
			string ssid = response_hash ["ssid"].ToString ().Trim ('"');

			Debug.Log ("LoginResult: " + response_hash.ToString());

			if (status == "SUCCESS") {
				NetworkManager.SetSessionID (ssid);
			}
		}

		//static public List<Texture2D> LoadAssetImages (string name, ImageCallback func = null)
		//{
		//	List<Texture2D> list = new List<Texture2D> ();

		//	string url = serverUrl + "/assets/" + name + ".unity3d";
		//	instance.StartCoroutine (StartAssetLoad (list, url, func));

		//	return list;
		//}

		//static IEnumerator StartAssetLoad (List<Texture2D> list, string url, ImageCallback func)
		//{

		//	AssetBundleContainer container = AssetBundleManager.Instance.LoadBundleAsync (url);
		//	while (!container.IsReady)
		//		yield return 0;

		//	if (container.IsError) {
		//		Debug.LogError (container.ErrorMsg);
		//		yield break;
		//	} else {
		//		foreach (var asset in container.FileList) {
		//			AssetBundleRequest request = container.AssetBundle.LoadAssetAsync (asset.Name.Replace (".png", ""), typeof(Texture2D));
		//			Texture2D tex = request.asset as Texture2D;

		//			if (request.asset != null)
		//				list.Add (tex);
		//		}
		//	}

		//	if (list.Count == 0)
		//		Debug.LogError ("No assets loaded");
		//	if (func != null && list.Count > 0)
		//		func ();
		//	AssetBundleManager.Instance.UnloadBundle (container);

		//}


		static public void SetData (string key, Hashtable val)
		{
			if (data.ContainsKey (key))
				data.Remove (key);

			data.Add (key, val);
		}


		static public void GetIAP (JSONNode response)
		{
			JSONNode response_hash = response [0];
			string status = response_hash ["status"].ToString ().Trim ('"');
			JSONNode data = response_hash ["store"];

			if (status == "SUCCESS") {
#if !UNITY_STANDALONE
				//IAPManager.LoadData(data);
#endif
			}
		}

		static public string GetUserInfo (string val)
		{
            if (!NetworkManager.data.ContainsKey("Info"))
                return "";
                //NetworkManager.data.Add ("Info", new Hashtable ());

            if (!NetworkManager.data["Info"].ContainsKey("user"))
                return "";
				//NetworkManager.data ["Info"].Add ("user", new Hashtable ());

			Hashtable userData = NetworkManager.data ["Info"] ["user"] as Hashtable;
			return userData [val].ToString ().Trim ('"');
		}

		static public void SetUserInfo (string val, string text)
		{
			if (!NetworkManager.data.ContainsKey ("Info"))
				NetworkManager.data.Add ("Info", new Hashtable ());
			if (!NetworkManager.data ["Info"].ContainsKey ("user"))
				NetworkManager.data ["Info"].Add ("user", new Hashtable ());
			Hashtable userData = NetworkManager.data ["Info"] ["user"] as Hashtable;
			if (!userData.ContainsKey (val))
				userData.Add (val, text);
			else
				userData [val] = text;

		}

        static public void LoadImageFromUrl(string url, Image image, ImageCallback callback = null)
        {
            instance.StartCoroutine(loadImageFromUrlCoroutine(url, image, callback));
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
                    NetworkManager.Transaction(GameConstants.HeartBeatTransaction, new Hashtable(), onHeartBeat);
                    yield return new WaitForSeconds(300.0f);
                }
            }
        }

        //public void OnTokenReceived (object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        //{
        //	UnityEngine.Debug.Log ("Received Registration Token: " + token.Token);
        //	//GameOfWhales.Instance.UpdateToken (token.Token, GameOfWhales.PROVIDER_FCM);
        //}

        //public void OnMessageReceived (object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        //{
        //	UnityEngine.Debug.Log ("Received a new message from: " + e.Message.From);
        //}

        //private void OnPushDelivered (SpecialOffer offer, string campID, string title, string message)
        //{
        //	//Show the notification to a player and then call the following method
        //	GameOfWhales.Instance.PushReacted (campID);
        //}
    }
}
