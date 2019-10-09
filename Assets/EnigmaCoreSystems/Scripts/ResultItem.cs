using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultItem : MonoBehaviour
{
    [SerializeField] private Text _questionName;
    [SerializeField] private Text _answerText;
    [SerializeField] private Text _timeText;
    [SerializeField] private Text _pointsText;
    [SerializeField] private GameObject _bonusTagGameObject;

    public void SetData(GameResults.ResultData resultData)
    {
        _questionName.text = "#" + resultData.QuestionNumber;
        LocalizationManager.TranslateText(_answerText, resultData.AnswerTerm);
        if (resultData.Points > 0)
            _answerText.color = Color.white;
        else
            _answerText.color = Color.red;

        _timeText.text = resultData.Time.ToString();
        _pointsText.text = resultData.Points.ToString();

        _bonusTagGameObject.SetActive(resultData.IsBonus);
    }
}
