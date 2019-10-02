/****************************************************
 *  (c) 2013 narayana games UG (haftungsbeschränkt) *
 *  This is just an example, do with it whatever    *
 *  you like ;-)                                    *
 ****************************************************/

using UnityEngine;
using System.Collections;

using NarayanaGames.ScoreFlashComponent;

public class ScoreAchievementStateMachine : MonoBehaviour {

    public int currentScore = 0;

    public int firstMotivationThreshold = 100;
    public string firstMotivationMessage = "First Motivational Message";

    public int secondMotivationThreshold = 500;
    public string secondMotivationMessage = "Second Motivational Message";

    public int awesomeAchievementThreshold = 1000;
    public string awesomeAchievementMessage = "Awesome Achievement Unlocked";

    private ScoreState currentScoreState = ScoreState.InitialState;

    enum ScoreState {
        InitialState,
        FirstMotivationShown,
        SecondMotivationShown,
        AwesomeAchievementShown
    }

    public void Start() {
        StartCoroutine(ScoreIncreaseCo());
    }

    private IEnumerator ScoreIncreaseCo() {
        while (true) {
            currentScore += 30;
            yield return new WaitForSeconds(1F);
        }
    }

    public void Update() {
        CheckScoreState();
    }

    private void CheckScoreState() {
        switch (currentScoreState) {
            case ScoreState.InitialState:
                if (currentScore > firstMotivationThreshold) {
                    ScoreFlash.Push(firstMotivationMessage);
                    currentScoreState = ScoreState.FirstMotivationShown;
                }
                break;
            case ScoreState.FirstMotivationShown:
                if (currentScore > secondMotivationThreshold) {
                    ScoreFlash.Push(secondMotivationMessage);
                    currentScoreState = ScoreState.SecondMotivationShown;
                }
                break;
            case ScoreState.SecondMotivationShown:
                if (currentScore > awesomeAchievementThreshold) {
                    ScoreFlash.Push(awesomeAchievementMessage);
                    currentScoreState = ScoreState.AwesomeAchievementShown;
                }
                break;
        }
    }
}