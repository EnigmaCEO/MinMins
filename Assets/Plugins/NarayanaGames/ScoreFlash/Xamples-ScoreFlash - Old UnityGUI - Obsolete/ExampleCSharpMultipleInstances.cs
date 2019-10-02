/****************************************************
 *  (c) 2012 narayana games UG (haftungsbeschränkt) *
 *  This is just an example, do with it whatever    *
 *  you like ;-)                                    *
 ****************************************************/

using UnityEngine;
using System.Collections;

public class ExampleCSharpMultipleInstances : MonoBehaviour {

    public void OnGUI() {
        if (GUI.Button(new Rect(Screen.width - 180F, 10F, 170F, 30F), "Push to SF_Amadeus")) {
            ScoreFlashManager.Get("SF_Amadeus").PushLocal("SF_Amadeus, yay!");
        }

        if (GUI.Button(new Rect(Screen.width - 180F, 50F, 170F, 30F), "Push to SF_Destroy")) {
            ScoreFlashManager.Get("SF_Destroy").PushLocal("SF Destroy, you see?");
        }

        if (GUI.Button(new Rect(Screen.width - 180F, 90F, 170F, 30F), "Push to SF_Eraser")) {
            ScoreFlashManager.Get("SF_Eraser").PushLocal("SF_Eraser, play with me!");
        }




        // this is just a hack to switch autogenerate messages on or off
        ScoreFlash instanceA = (ScoreFlash)ScoreFlashManager.Get("SF_Amadeus");
        ScoreFlash instanceB = (ScoreFlash)ScoreFlashManager.Get("SF_Eraser");
        ScoreFlash instanceC = (ScoreFlash)ScoreFlashManager.Get("SF_Destroy");
        // allow the player to auto generate messages ... or not ;-)
        instanceA.isTestAutogenerateMessages
            = GUI.Toggle(new Rect(10F, 10F, 200F, 30F), instanceA.isTestAutogenerateMessages, "Autogenerate Messages?");
        instanceB.isTestAutogenerateMessages = instanceA.isTestAutogenerateMessages;
        instanceC.isTestAutogenerateMessages = instanceA.isTestAutogenerateMessages;
    }
}
