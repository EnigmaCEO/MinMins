using Enigma.CoreSystems;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using Tiny;
using UnityEngine;

public class GameNetwork : SingletonMonobehaviour<GameNetwork>
{
    [SerializeField] private int _ratingIncreaseOnWin = 30;
    [SerializeField] private int _ratingDecreaseOnLose = 15;

    public delegate void OnHandleRatingChangeErrorDelegate(string message);
    private OnHandleRatingChangeErrorDelegate _onHandleRatingChangeErrorCallback;

    public class Transactions
    {
        public const int CHANGE_RATING = 15;
    }

    public class TransactionKeys
    {
        public const string RATING_DELTA = "rating_delta";
        public const string RATING = "rating";
    }

    public class PlayerCustomProperties
    {
        public const string RATING = "Rating";
    }

    public void HandleMatchResults(War.MatchResults results,  OnHandleRatingChangeErrorDelegate onHandleRatingChangeErrorCallback)
    {
        _onHandleRatingChangeErrorCallback = onHandleRatingChangeErrorCallback;

        int ratingDelta = 0;
        if (results == War.MatchResults.Win)
            ratingDelta += _ratingIncreaseOnWin;
        else if (results == War.MatchResults.Lose)
            ratingDelta -= _ratingDecreaseOnLose;

        NetworkManager.Transaction(Transactions.CHANGE_RATING, TransactionKeys.RATING_DELTA, ratingDelta, onChangeRating);
    }

    private void onChangeRating(JSONNode response)
    {
        if (response != null)
        {
            Debug.Log("onCoinsEarned: " + response.ToString());

            JSONNode response_hash = response[0];
            string status = response_hash[NetworkManager.TransactionKeys.STATUS].ToString().Trim('"');

            if (status == NetworkManager.StatusOptions.SUCCESS)
            {
                int updatedRating = response_hash[GameNetwork.TransactionKeys.RATING].AsInt;
                SetRating(updatedRating);
            }
            else 
                _onHandleRatingChangeErrorCallback("Server Error");
        }
        else
            _onHandleRatingChangeErrorCallback("Connection Error");
    }

    public int GetRating()
    {
        return (int)NetworkManager.GetCustomProperty(PlayerCustomProperties.RATING);
    }

    public void SetRating(int newRating)
    {
        NetworkManager.SetCustomProperty(PlayerCustomProperties.RATING, newRating);
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
