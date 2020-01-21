using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PvpRoomGridItem : MonoBehaviour
{
    //[SerializeField] private GameObject _tint;

    [SerializeField] private Text _hostNameText;
    [SerializeField] private Text _hostRatingText;

    [SerializeField] private Text _guestNameText;
    [SerializeField] private Text _guestRatingText;

    public void SetUp(string hostName, int hostRating, string guestName, int guestRating, string roomName)
    {
        this.name = roomName;

        _hostNameText.text = hostName;
        _hostRatingText.text = hostRating.ToString();

        if (guestName != "")
        {
            _guestNameText.text = guestName;
            _guestRatingText.text = guestRating.ToString();
        }
        else
        {
            _guestNameText.text = "Open";
            _guestRatingText.gameObject.SetActive(false);;
        }

        //Deselect();
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
