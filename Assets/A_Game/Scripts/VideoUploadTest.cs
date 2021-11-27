using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma.CoreSystems;
using SimpleJSON;

public class VideoUploadTest : MonoBehaviour
{
    void Start()
    {
        Hashtable hashtable = new Hashtable
        {
            { GameNodeKeys.WINNER_NICKNAME, "Winner Nickname Test" },
            { GameNodeKeys.LOSER_NICKNAME, "Loser Nickname Test" },
            { EnigmaNodeKeys.VIDEO_PATH, "Boost.mp4" }
        };

        NetworkManager.Transaction(GameTransactions.UPLOAD_REPLAY, hashtable, onReplayUploaded);
    }

    private void onReplayUploaded(JSONNode response)
    {
        NetworkManager.CheckInvalidServerResponse(response, nameof(onReplayUploaded));

        Debug.Log("VideoUploadTest::onReplayUploaded -> response: " + response.ToString());
    }
}
