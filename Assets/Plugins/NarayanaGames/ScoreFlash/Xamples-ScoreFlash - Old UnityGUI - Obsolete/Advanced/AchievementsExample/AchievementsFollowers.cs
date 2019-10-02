/****************************************************
 *  (c) 2013 narayana games UG (haftungsbeschränkt) *
 *  This is just an example, do with it whatever    *
 *  you like ;-)                                    *
 ****************************************************/

using UnityEngine;
using System.Collections;

using NarayanaGames.ScoreFlashComponent;

public class AchievementsFollowers : MonoBehaviour {

    private ScoreMessage msg;
    private int counter = 1;

    public void OnCollisionEnter(Collision ignored) {
        if (msg == null) {
            msg = ScoreFlash.Instance.PushWorld(GetComponent<ScoreFlashFollow3D>(), "Bumping");
            msg.FreezeOnRead = true;
        }
        msg.Text = string.Format("Bumped {0}", counter++);
    }
}
