using UnityEngine;
using System;
using System.Collections;

namespace Enigma.CoreSystems
{
    public class RatingManager : Manageable<RatingManager> 
    {
        static private int myRating;
        static private int placementMatches;

    	protected override void Start () 
        {
            base.Start();

            //For testing purposes, remove this 
            //PlayerPrefs.SetInt("Rating", 1000);
           
            if (!PlayerPrefs.HasKey("Rating"))
                PlayerPrefs.SetInt("Rating", 1000);

            if (!PlayerPrefs.HasKey("Placement"))
                PlayerPrefs.SetInt("Placement", 0);

            myRating = PlayerPrefs.GetInt("Rating");
            placementMatches = PlayerPrefs.GetInt("Placement");
    	}
    	
        static public void SetRating (int newRating)
        {
            myRating = newRating;
            PlayerPrefs.SetInt("Rating", myRating);
        }

        static public int GetRating ()
        {
            return myRating;
        }

        static public void IncrementPlacementMatch ()
        {
            if (placementMatches != 10)
            {
                placementMatches++;
                PlayerPrefs.SetInt("Placement", placementMatches);
            }
        }
            
        //Simple ELO rating formula
        //R' = R + K * (S - E)
        //R' = new rating
        //K = max value for increase/decrease of rating
        //S = score for a game
        //E = expected score for a game
        //Expected score is calculated by
        //E(A) = 1 / [ 1 + 10 ^ ( [R(B) - R(A)] / 400 ) ]
        //E(B) = 1 / [ 1 + 10 ^ ( [R(A) - R(B)] / 400 ) ]
        static public int CalculateRating (int myOldRating, int opponentRating, float score)
        {
            int maxValue = 32;
            int calculatedRating = myOldRating;
            float expectedScore;

            //Draw
            if (score == 0.5f)
            {
                //My rating is lower than opponent's so slight gain in elo
                if (myOldRating < opponentRating)
                {
                    expectedScore = 1 / (float)(1 + Math.Pow(10f, ((opponentRating - myOldRating) / (float)400)));
                    calculatedRating = myOldRating + (int)Math.Round(maxValue * (score - expectedScore), 2);
                }
            }
            //Win or loss
            else
            {
                expectedScore = 1 / (float)(1 + Math.Pow(10f, ((opponentRating - myOldRating) / (float)400)));
                calculatedRating = myOldRating + (int)Math.Round(maxValue * (score - expectedScore), 2);
            }

            //If calculated rating falls below 1, set it to 1
            if (calculatedRating < 1)
            {
                calculatedRating = 1;
            }

            //If calculated rating rises above 5000, set it to 5000
            if (calculatedRating > 5000)
            {
                calculatedRating = 5000;
            }
                
            Debug.LogError("calc: " + calculatedRating);

            return calculatedRating;
        }
    }
}
