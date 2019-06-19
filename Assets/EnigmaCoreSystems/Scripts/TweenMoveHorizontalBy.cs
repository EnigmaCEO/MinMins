using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.CoreSystems
{
    public class TweenMoveHorizontalBy : MonoBehaviour
    {
        [SerializeField] private float _x = 2.0f;
        [SerializeField] private iTween.EaseType _easeType = iTween.EaseType.easeInOutExpo;
        [SerializeField] private iTween.LoopType _loopType = iTween.LoopType.pingPong;
        [SerializeField] private float _time = 1;
        [SerializeField] private float _delay = 0.1f;


        // Start is called before the first frame update
        void Start()
        {
            iTween.MoveBy(gameObject, iTween.Hash("x", _x, "easeType", _easeType.ToString(), "loopType", _loopType.ToString(), "delay", _delay, "time", _time));
        }
    }
}
