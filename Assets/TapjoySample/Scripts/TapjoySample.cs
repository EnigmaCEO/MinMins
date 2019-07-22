// Define USE_FIREBASE_MESSAGING to enable firebase push notificaiton
// #define USE_FIREBASE_MESSAGING

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using TapjoyUnity;

#if USE_FIREBASE_MESSAGING
using Firebase;
using Firebase.Messaging;
#endif

public class TapjoySample : MonoBehaviour {
	private enum UIState {
		Placement,
		Event,
		User
	}

	private UIState uiState;

	private PlacementExample mainUIView;
	private EventExample eventUIView;
	private UserExample userUIView;

	public bool viewIsShowing = false;
	public bool isConnected = false;
    public bool isFirebaseInitialized = false;
	
	void Start() {
		Debug.Log("C#: TapjoySample start and adding Tapjoy Delegates");
		// Grab references to sample application components
		mainUIView = gameObject.GetComponentsInChildren<PlacementExample>(true)[0];
		eventUIView = gameObject.GetComponentsInChildren<EventExample>(true)[0];
		userUIView = gameObject.GetComponentsInChildren<UserExample>(true)[0];

#if USE_FIREBASE_MESSAGING
        // Firebase Initialization
        FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            Debug.Log("C# FIREBASE dependency initialization status:\t" + dependencyStatus);
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies. " +
                    "Firebase SDK is {0}", dependencyStatus));
            }
        });
#endif

		// Connect Delegates
		Tapjoy.OnConnectSuccess += HandleConnectSuccess;
		Tapjoy.OnConnectFailure += HandleConnectFailure;
	}

#if USE_FIREBASE_MESSAGING
    // Setup message event handlers.
    void InitializeFirebase()
    {
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    // Firebase Messaging Push Notification
    public void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("C# Received fcm registration token: " + token.Token);
        Tapjoy.SetDeviceToken(token.Token);
    }

    public void OnMessageReceived(object sender, MessageReceivedEventArgs receivedEventArgs)
    {
        Debug.Log("C# Push notification message received from: " + receivedEventArgs.Message.From);
        var remoteMessage = new Dictionary<string, string>(receivedEventArgs.Message.Data);
        if(remoteMessage == null){
            Debug.LogError("C# remoteMessage received null");
        }
        Tapjoy.SetReceiveRemoteNotification(remoteMessage);
    }
#endif

	void OnDisable()
	{
		Debug.Log("C#: Disabling and removing Tapjoy Delegates");
		// Connect Delegates
		Tapjoy.OnConnectSuccess -= HandleConnectSuccess;
		Tapjoy.OnConnectFailure -= HandleConnectFailure;
	}

	void Update() {
		// Quit app on BACK key.
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit(); 
		}
	}

	#region Connect Delegate Handlers
	public void HandleConnectSuccess() {
		Debug.Log("C#: Handle Connect Success");
		isConnected = true;
		ChangeState(UIState.Placement);
	}
	
	public void HandleConnectFailure() {
		Debug.Log("C#: Handle Connect Failure");
	}
	#endregion

	#region View Delegate Handlers
	public void HandleViewWillOpen(int viewType) {
		Debug.Log("C#: HandleViewWillOpen, viewType: " + viewType);
	}
	
	public void HandleViewDidOpen(int viewType) {
		Debug.Log("C#: HandleViewDidOpen, viewType: " + viewType);
		viewIsShowing = true;
	}
	
	public void HandleViewWillClose(int viewType) {
		Debug.Log("C#: HandleViewWillClose, viewType: " + viewType);
	}
	
	public void HandleViewDidClose(int viewType) {
		Debug.Log("C#: HandleViewDidClose, viewType: " + viewType);
		viewIsShowing = false;
	}
	#endregion

	#region Global UI for Sample app
	GUIStyle labelStyle;
	int fontSize = 24;

	float centerX;
	float tabWidth;
	float tabHeight;

	float yPadding = 50;

	void OnGUI() {
		// TODO: check if needed
		if (viewIsShowing) {
			return;
		}

		labelStyle = new GUIStyle();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.normal.textColor = Color.white;
		labelStyle.wordWrap = true;
		labelStyle.fontSize = fontSize;
		
		centerX = Screen.width / 2;
		tabWidth = Screen.width / 3;
		tabHeight = Screen.height / 20;
		yPadding = tabHeight + 10;

		Rect position;
		float yPosition = 0;


		// Render buttons to transition UI states
		if (uiState == UIState.Placement || !isConnected) {
			GUI.enabled = false;
		}
		position = new Rect(centerX - (tabWidth + (tabWidth / 2)), yPosition, tabWidth, tabHeight);
		if (GUI.Button(position, "Placement")) {
			ChangeState(UIState.Placement);
		}
		if (uiState == UIState.Placement || !isConnected) {
			GUI.enabled = true;
		}

		if (uiState == UIState.Event || !isConnected) {
			GUI.enabled = false;
		}
		position = new Rect(centerX - (tabWidth / 2), yPosition, tabWidth, tabHeight);
		if (GUI.Button(position, "Event")) {
			ChangeState(UIState.Event);
		}
		if (uiState == UIState.Event || !isConnected) {
			GUI.enabled = true;
		}

		if (uiState == UIState.User || !isConnected) {
			GUI.enabled = false;
		}
		position = new Rect(centerX + (tabWidth / 2), yPosition, tabWidth, tabHeight);
		if (GUI.Button(position, "User")) {
			ChangeState(UIState.User);
		}
		if (uiState == UIState.User || !isConnected) {
			GUI.enabled = true;
		}
		yPosition += yPadding;

		// Render header
		position = new Rect(centerX - 200, yPosition, 400, 25);
		GUI.Label(position, "Tapjoy Connect Sample App", labelStyle);

		if (!isConnected) {
			yPosition += yPadding;
			position = new Rect(centerX - 200, yPosition, 400, 25);
			GUI.Label(position, "Trying to connect to Tapjoy...", labelStyle);
		}
	}

	private void ChangeState(UIState state) {
		Debug.Log("C#: change state: " + state);
		switch (state) {
		case UIState.Placement:
			mainUIView.enabled = true;
			eventUIView.enabled = false;
			userUIView.enabled = false;
			break;
		case UIState.Event:
			mainUIView.enabled = false;
			eventUIView.enabled = true;
			userUIView.enabled = false;
			break;
		case UIState.User:
			mainUIView.enabled = false;
			eventUIView.enabled = false;
			userUIView.enabled = true;
			break;
		}
		uiState = state;
	}
	#endregion
}
