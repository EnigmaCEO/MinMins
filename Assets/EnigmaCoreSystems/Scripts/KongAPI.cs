using UnityEngine;
using System.Collections;

public delegate void OnKongregatePurchaseCompleteDelegate(string desc, bool result);
public delegate void OnKongregateLoginSuccessfulDelegate(string username);

public class KongAPI : MonoBehaviour 
{	
#if UNITY_WEBGL
	public OnKongregatePurchaseCompleteDelegate OnKongregatePurchaseComplete;
	public OnKongregateLoginSuccessfulDelegate OnKongregateLoginSuccessful;
	// Public API (Static Members)
	public bool isConnected
	{
		get { return instance.jsAPILoaded; }
	}
	
	static bool _isGuest = false;
	public bool isGuest
	{
		get 
		{
			instance.CallJSFunction("isGuest()", "SetIsGuest"); 
			return _isGuest; 
		}
	}
	
	static string _username = "";
	public string username
	{
		get
		{ 
			instance.CallJSFunction("getUsername()", "SetUsername");
			return _username;
		}
	}
	
	static string _userId = "0";
	public string userId
	{
		get 
		{ 
			instance.CallJSFunction("getUserId()", "SetUserId"); 
			return _userId;
		}
	}
	
	static string _items = "";
	public string[] items
	{
		get
		{
			instance.CallJSFunction("getUserItems()");
			if (string.IsNullOrEmpty(_items))
				return new string[0];
			else
				return _items.Split(',');
		}
	}

	private string m_currentItem = "";
	
	public void ShowSignIn()
	{
		instance.CallJSFunction("showSignInBox()");
	}

	public void ShowRegistration()
	{
		instance.CallJSFunction("showRegistrationBox()");
	}
	
	public void SubmitStat(string statistic, int value)
	{
		instance.CallJSFunction(string.Format("submitStat('{0}', {1})", statistic, value));
	}
	
	public void PurchaseItem(string item)
	{
		m_currentItem = item;
		Debug.Log("Attempting purchase of " + item);
		instance.CallJSFunction(string.Format("purchaseItem('{0}')", item));
#if UNITY_EDITOR
        OnPurchaseSuccessful("purchase complete...");
#endif
    }
	
	private static KongAPI _instance = null;
	public static KongAPI instance
	{
		get
		{
			if (!_instance)
				new GameObject("Kongregate", typeof(KongAPI));
				
			return _instance;
		}
	}
	
	// Instance Members	
	bool jsAPILoaded;
	string jsReturnValue;
	
	void Awake()
	{
		// Only allow one instance of the API bridge
		if (_instance)
		{
			Debug.Log("Destroying GO");
			Destroy(gameObject);
		}
		
		Debug.Log("Awake kongregate");
		DontDestroyOnLoad( this );	
		_instance = this;
	}
	
	void Start() 
	{
		Debug.Log("Trying to load Kongregate API");
		Application.ExternalEval(@"
			// Extern the JS Kongregate API

			function isGuest()
			{
				return kongregate.services.isGuest();
			}

			function getUsername()
			{
				var name = kongregate.services.getUsername();
				return name;
			}

			function getUserId()
			{
				return kongregate.services.getUserId();
			}

			function showSignInBox()
			{
				if(kongregate.services.isGuest())
  					kongregate.services.showSignInBox();
			}

			function showRegistrationBox()
			{
				if(kongregate.services.isGuest())
  					kongregate.services.showRegistrationBox();			
			}
		
			function submitStat(statName, value)
			{
				kongregate.stats.submit(statName, value);
			}

			function getUserItems()
			{
				kongregate.mtx.requestUserItemList(null, onUserItems);
			}

			function onUserItems(result)
			{
				if (result.success)
				{
					var items = '';
					for (var i = 0; i < result.data.length; i++)
					{
						items += result.data[i].id + '|' + result.data[i].remaining_uses;
						if (i < result.data.length - 1)
							items += ',';
					}
					SendUnityMessage('SetUserItems', items);
				}
			}

			function purchaseItem(item)
			{
				kongregate.mtx.purchaseItems([item], onPurchaseResult);
				SendUnityMessage('LogMessage', 'purchase sent....');				
			}

			function onPurchaseResult(result)
			{
				if (result.success)
				{
					SendUnityMessage('OnPurchaseSuccessful', 'purchase complete...');
					getUserItems();
				}
				else
				{
					SendUnityMessage('OnPurchaseFailed', 'purchase failed...');
				}
			}

			// Utility function to send data back to Unity
			function SendUnityMessage(functionName, message)
			{
				Log('Calling to: ' + functionName);
				var unity = kongregateUnitySupport.getUnityObject();
				unity.SendMessage('Kongregate', functionName, message);
			}

			function Log(message)
			{
				var unity = kongregateUnitySupport.getUnityObject();
				unity.SendMessage('Kongregate', 'LogMessage', message);
			}
		
			function registerLoginEventCallback()
			{
				kongregate.services.addEventListener('login', onKongregateInPageLogin);
			}

			function onKongregateInPageLogin(event)
			{
				var user_id = kongregate.services.getUserId();
				var username = kongregate.services.getUsername();
				var token = kongregate.services.getGameAuthToken();
				SendUnityMessage('OnLoginSuccessful', 'login successful!');
			}

			if(typeof(kongregateUnitySupport) != 'undefined')
			{
  				kongregateUnitySupport.initAPI('Kongregate', 'OnKongregateAPILoaded');
  			};
		");
		  
//		Application.ExternalCall("SendUnityMessage", "OnLoaded", "Message to myself");
//		Application.ExternalCall("loadAPI");

		DontDestroyOnLoad(gameObject);
	}
	
	public void LogMessage(string message)
	{
		Debug.Log(message);
	}

	public void OnPurchaseSuccessful(string message)
	{
		Debug.Log(message);
		OnKongregatePurchaseComplete(m_currentItem, true);
		m_currentItem = "";
	}

	public void OnPurchaseFailed( string message )
	{
		Debug.Log(message);
		OnKongregatePurchaseComplete(m_currentItem, false);
		m_currentItem = "";
	}

	public void OnLoginSuccessful( string message )
	{
		Debug.Log(message);
		OnKongregateLoginSuccessful(_username);
	}
	
	void OnLoaded(string error)
	{
		if (string.IsNullOrEmpty(error))
		{
			jsAPILoaded = true;
			Debug.Log("Connected");
		}
		else
			Debug.LogError(error);
	}
	
	public void SetIsGuest(object returnValue)
	{
		if (bool.TryParse(returnValue.ToString(), out _isGuest))
		{
			// Request username again if guest
			if (_isGuest)
				_username = "";
		}
	}
	
	public void SetUsername(object returnValue)
	{
		_username = returnValue.ToString();
	}
	
	void SetUserId(object returnValue)
	{
		_userId = returnValue.ToString();
		Debug.Log("KongAPI.SetUserId returns: " + _userId );
	}
	
	void SetUserItems(object returnValue)
	{
		_items = returnValue.ToString();
	}
	
	void CallJSFunction(string functionCall)
	{
		CallJSFunction(functionCall, null);
	}
		
	void CallJSFunction(string functionCall, string callback)
	{
		if (jsAPILoaded)
		{
			if (string.IsNullOrEmpty(callback))
			{
				Application.ExternalEval(functionCall);
			}
			else
			{
				Application.ExternalEval(string.Format(@"
					var value = {0};
					SendUnityMessage('{1}', String(value));
				", functionCall, callback));
			}
		}
	}
	
	void OnKongregateAPILoaded(string userInfo)
	{
		Debug.Log("API Loaded");
		jsAPILoaded = true;

		CallJSFunction("registerLoginEventCallback");

		string[] userParams = userInfo.Split('|');
		int userID = int.Parse(userParams[0]);
		string userName = userParams[1];
		string gameAuthToek = userParams[2];

		Debug.Log("userID is " + userID.ToString());
			
		if (userID == 0)
		{
			SetIsGuest(true);
		}
		else
		{
			SetUserId(userParams[0]);
			SetUsername(userName);
			SetIsGuest(false);
		}
	}
#endif
}