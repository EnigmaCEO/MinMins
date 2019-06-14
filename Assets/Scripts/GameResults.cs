using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameResults : MonoBehaviour
{
    [SerializeField] private string _resultsFindPath = "Canvas/Layout/Results";

    private List<ResultData> _resultDatas = new List<ResultData>();

    private GameObject _resultsGameObject;
    private Transform _resultItemsContentTransform;
    private GameObject _resultItemPrefab;

    private Text _answersCorrectPercentText;
    private Text _averageTimeText;
    private Text _totalPointsText;

    public static GameResults Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SetUI()
    {
        _resultsGameObject = GameObject.Find(_resultsFindPath);
        _resultItemsContentTransform = findResultsUI<Transform>("ResultItems/ScrollViewVertical/Viewport/Content");
        _resultItemPrefab = _resultItemsContentTransform.GetChild(0).gameObject;
        _resultItemPrefab.SetActive(false);

        _answersCorrectPercentText = findResultsUI<Text>("AnswersCorrectPercent");
        _averageTimeText = findResultsUI<Text>("AverageTime");
        _totalPointsText = findResultsUI<Text>("Points");
    }

    public void AddData(ResultData resultData)
    {
        _resultDatas.Add(resultData);
    }

    public void ShowResults()
    {
        int pointsSum = 0;
        float timeSum = 0;
        int correctSum = 0;

        foreach (ResultData data in _resultDatas)
        {
            GameObject resultItemInstance = Instantiate<GameObject>(_resultItemPrefab, _resultItemsContentTransform);
            ResultItem resultItem = resultItemInstance.GetComponent<ResultItem>();
            resultItem.SetData(data);

            pointsSum += data.Points;
            timeSum += data.Time;

            if (data.Points > 0)
                correctSum++;

            resultItemInstance.SetActive(true);
        }

        int questionsCount = _resultDatas.Count;
        int correctPercent = Mathf.RoundToInt((float)(correctSum * 100) / (float)questionsCount);
        _answersCorrectPercentText.text = correctPercent.ToString() + "%";

        float averageTime = timeSum / questionsCount;
        _averageTimeText.text = averageTime.ToString("0.00");

        _totalPointsText.text = pointsSum.ToString();

        _resultsGameObject.SetActive(true);
    }

    public void HideResults()
    {
        _resultsGameObject.SetActive(false);
        _resultDatas.Clear();
        _resultItemPrefab.transform.SetParent(null);
        _resultItemsContentTransform.DestroyChildren();
        _resultItemPrefab.transform.SetParent(_resultItemsContentTransform);
    }
    
    public class ResultData
    {
        public int QuestionNumber = 0;
        public string AnswerTerm = "";
        public bool WasCorrect;
        public float Time = 0;
        public int Points = 0;
        public bool IsBonus = false;

        public ResultData(int questionNumber, string answerTerm, float time, int points, bool isBonus)
        {
            QuestionNumber = questionNumber;
            AnswerTerm = answerTerm;
            Time = time;
            Points = points;
            IsBonus = isBonus;
        }
    }

    private T findResultsUI<T>(string resultsPath) where T : Component
    {
        return GameObject.Find(_resultsFindPath + "/" + resultsPath).GetComponent<T>();
    }
}
