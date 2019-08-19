/*
 * Game Of Whales SDK
 *
 * https://www.gameofwhales.com/
 * 
 * Copyright © 2018 GameOfWhales. All rights reserved.
 *
 * Licence: https://github.com/Game-of-whales/GOW-SDK-UNITY/blob/master/LICENSE
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOfWhalesSettings : ScriptableObject{
    private static GameOfWhalesSettings _instance;

    public static GameOfWhalesSettings instance{
        get{
            if (!_instance){
                _instance = (GameOfWhalesSettings) Resources.Load<GameOfWhalesSettings>("GameOfWhalesSettings");
            }

            return _instance;
        }
    }


    [SerializeField] private bool _notificationsEnabled;
    [SerializeField] private bool _debugLogging;
    [SerializeField] private string _gameID;
    [SerializeField] private string _androidProjectID;
    [SerializeField] private bool _generateNotificationService;
    [SerializeField] private string _nsebundlepostfix = "notificationservice";

    public string notificationServiceBundlePostix{
        get{ return _nsebundlepostfix; }
        set{ _nsebundlepostfix = value; }
    }

    public bool generateNotificationService{
        get{ return _generateNotificationService; }
        set{ _generateNotificationService = value; }
    }

    public bool notificationsEnabled{
        get{ return _notificationsEnabled; }
        set{ _notificationsEnabled = value; }
    }

    public bool debugLogging{
        get{ return _debugLogging; }
        set{ _debugLogging = value; }
    }

    public string gameID{
        get{ return _gameID; }
        set{ _gameID = value; }
    }

    public string androidProjectID{
        get{ return _androidProjectID; }
        set{ _androidProjectID = value; }
    }
}