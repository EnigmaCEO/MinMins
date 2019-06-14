using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyBonusWindow : MonoBehaviour
{
    [SerializeField] private Text _dayNumberText;
    [SerializeField] private Text _currencyNumberText;

    [SerializeField] private Slider _progressBar;

    public void Open(int day, int bonusCurrency)
    {
        _dayNumberText.text = day.ToString();
        _currencyNumberText.text = bonusCurrency.ToString();

        _progressBar.value = Mathf.Clamp((float)day/7, 0, 1);

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnCloseButtonDown()
    {
        Close();
    }
}
