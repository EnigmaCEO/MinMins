using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using Tiny;
using UnityEngine;

public class GameNetwork : SingletonMonobehaviour<GameNetwork>
{
    [SerializeField] private int _ratingIncreaseOnWin = 30;
    [SerializeField] private int _ratingDecreaseOnLose = 15;

    public class Transactions
    {
        public const int CHANGE_RATING = 1;
    }

    public class PlayerCustomProperties
    {
        public const string RATING = "Rating";
    }

    public class TransactionKeys
    {
        public const string RATING = "Rating";
    }

    private void handleMatchResults(War.MatchResults results)
    {
        int rating = GameNetwork.Instance.GetRating();
        if (results == War.MatchResults.Win)
            rating += _ratingIncreaseOnWin;
        else if (results == War.MatchResults.Lose)
            rating -= _ratingDecreaseOnLose;

        //NetworkManager.Transaction() //TODO: Complete transaction to change rating and handle response
    }

    public int GetRating()
    {
        return int.Parse(GetValue(PlayerCustomProperties.RATING, "0"));
    }

    public void SetRating(int newRating)
    {
        SetValue(PlayerCustomProperties.RATING, newRating.ToString());
    }

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
