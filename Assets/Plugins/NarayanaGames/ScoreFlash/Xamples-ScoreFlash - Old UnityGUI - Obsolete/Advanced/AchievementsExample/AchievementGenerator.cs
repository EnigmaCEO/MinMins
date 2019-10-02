/****************************************************
 *  (c) 2013 narayana games UG (haftungsbeschränkt) *
 *  This is just an example, do with it whatever    *
 *  you like ;-)                                    *
 ****************************************************/

using UnityEngine;
using System.Collections;

using NarayanaGames.ScoreFlashComponent;

public class AchievementGenerator : MonoBehaviour {

    /// <summary>
    ///     This is the custom renderer that renders our awesome achievements.
    ///     It uses a custom implementation of ScoreFlashRendererBase. In most
    ///     cases, you'll want to create your own implementations of this,
    ///     specific to your individual needs. But you can use this and, if
    ///     you're using NGUI, the NGUI example as a starting point.
    /// </summary>
    public AchievementsCustomRenderer achievementPrefab;

	public void Start() {
        StartCoroutine(GenerateAchievementsCo());
	}

    private IEnumerator GenerateAchievementsCo() {
        while (true) {
            // push messages with various different types
            yield return new WaitForSeconds(0.5F);
            ScoreFlash.Instance.PushLocal(10, Color.green); // int
            yield return new WaitForSeconds(0.2F);
            ScoreFlash.Instance.PushLocal(30, Color.green); // int
            yield return new WaitForSeconds(0.2F);
            ScoreFlash.Instance.PushLocal(-10L, Color.red); // long
            yield return new WaitForSeconds(0.2F);
            ScoreFlash.Instance.PushLocal(-25L, Color.red); // long
            yield return new WaitForSeconds(0.2F);
            ScoreFlash.Instance.PushLocal(Mathf.PI, Color.white); // float => 0.00
            yield return new WaitForSeconds(0.2F);
            ScoreFlash.Instance.PushLocal(System.Math.PI, Color.gray); // double => 0.0000
            yield return new WaitForSeconds(0.2F);
            
            // example of changing the value dynamically and also changing the color
            int someScore = -600;
            ScoreMessage msg = ScoreFlash.Instance.PushLocal(someScore, Color.red);
            for (int i = 0; i < 8; i++) {
                yield return new WaitForSeconds(0.1F);
                someScore += 50;
                if (someScore > 0) {
                    msg.UpdateColor(Color.green);
                }
                msg.UpdateMessage(someScore);
            }
            ScoreFlash.Instance.PushLocal("Get ready for awesome!");

            // waiting six more seconds and changing the text again
            for (int i = 0; i < 20; i++) {
                yield return new WaitForSeconds(0.3F);
                someScore += 50;
                if (someScore > 0) {
                    msg.UpdateColor(Color.green);
                }
                msg.UpdateMessage(someScore);
            }


            // and here's the achievements :-)
            ScoreFlash.Instance.PushLocal(GenerateAchievement(
                "Awesome Achievement",
                "This is an awesome achievement that I have unlocked: Flashing Achievements through ScoreFlash. This flashes me. Seriously!"));
            yield return new WaitForSeconds(5);

            ScoreFlash.Instance.PushLocal(GenerateAchievement(
                "Saved the World",
                "Honestly, you don't really want to be 'The Savior' ... it just gives you trouble. Just be nice instead!"));

            yield return new WaitForSeconds(5);

            ScoreFlash.Instance.PushLocal(GenerateAchievement(
                "A Few More",
                "Achievements are coming your way, my friend ... first this, then that ... then that other"));

            yield return new WaitForSeconds(1);

            ScoreFlash.Instance.PushLocal(GenerateAchievement(
                "Three More",
                "Achievements are coming your way, my friend ... first this, then that ... then that other"));

            yield return new WaitForSeconds(1);

            ScoreFlash.Instance.PushLocal(GenerateAchievement(
                "Two More",
                "Achievements are coming your way, my friend ... first this, then that ... then that other"));

            yield return new WaitForSeconds(1);

            ScoreFlash.Instance.PushLocal(GenerateAchievement(
                "One More",
                "Achievements are coming your way, my friend ... first this, then that ... then that other"));

            yield return new WaitForSeconds(1);
            ScoreFlash.Instance.PushLocal(GenerateAchievement(
                "Final one ... for now :-)",
                "Achievements are coming your way, my friend ... first this, then that ... then that other"));

            yield return new WaitForSeconds(10);
        }
    }

    private AchievementsCustomRenderer GenerateAchievement(string title, string description) {
        AchievementsCustomRenderer achievement = (AchievementsCustomRenderer) achievementPrefab.CreateInstance(this.transform);
        achievement.AchievementUnlocked(title, description);
        return achievement;
    }
}
