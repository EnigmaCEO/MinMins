using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardEntry : MonoBehaviour
{
    [SerializeField] private Text _playerName;
    [SerializeField] private Text _playerScore;

    public void SetPlayerName(string name)
    {
        _playerName.text = name;
    }

    public void SetPlayerScore(int score)
    {
        _playerScore.text = score.ToString();
    }
}
