using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PvpRoomGridItem : MonoBehaviour
{
    //[SerializeField] private GameObject _tint;

    [SerializeField] private Text _hostNameText;
    [SerializeField] private Text _hostRatingText;
    [SerializeField] private Text _hostPingText;

    [SerializeField] private Text _guestNameText;
    [SerializeField] private Text _guestRatingText;
    [SerializeField] private Text _guestPingText;

    public void SetUp(string hostName, int hostRating, int hostPing, string guestName, int guestRating, int guestPing, string roomName)
    {
        _guestPingText.gameObject.SetActive(false);
        _guestRatingText.gameObject.SetActive(false);

        this.name = roomName;

        _hostNameText.text = hostName;
        _hostRatingText.text = LocalizationManager.GetTermTranslation("Rating:") + " " + hostRating.ToString();
        UpdateHostPing(hostPing);

        if (guestName != "")
        {
            _guestNameText.text = guestName;
            _guestRatingText.text = LocalizationManager.GetTermTranslation("Rating:") + " " + guestRating.ToString();
            _guestRatingText.gameObject.SetActive(true);

            UpdateGuestPing(guestPing);
        }
        else
        {
            _guestNameText.text = LocalizationManager.GetTermTranslation("Open");
        }

        //Deselect();
    }

    public void UpdateHostPing(int ping)
    {
        _hostPingText.text = LocalizationManager.GetTermTranslation("Ping:") + " " + ping.ToString();
    }

    public void UpdateGuestPing(int ping)
    {
        if (ping != -1)
        {
            _guestPingText.text = LocalizationManager.GetTermTranslation("Ping:") + " " + ping.ToString();

            if (!_guestPingText.gameObject.activeSelf)
            {
                _guestPingText.gameObject.SetActive(true);
            }
        }
    }

    //public void Select()
    //{
    //    _tint.SetActive(true);
    //}

    //public void Deselect()
    //{
    //    _tint.SetActive(false);
    //}
}
