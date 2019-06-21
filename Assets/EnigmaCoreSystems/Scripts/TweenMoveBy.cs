using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.CoreSystems
{
    public class TweenMoveBy : MonoBehaviour
    {
        [SerializeField] private float _movement = 2.0f;

        [SerializeField] private TweenConstants.MoveTypes _moveType = TweenConstants.MoveTypes.Horizontal;
        [SerializeField] private iTween.EaseType _easeType = iTween.EaseType.easeInOutExpo;
        [SerializeField] private iTween.LoopType _loopType = iTween.LoopType.pingPong;
        [SerializeField] private float _time = 1;
        [SerializeField] private float _delay = 0.1f;


        // Start is called before the first frame update
        void Start()
        {
            string movementAxis = "x";
            if (_moveType == TweenConstants.MoveTypes.Vertical)
                movementAxis = "y";

            iTween.MoveBy(gameObject, iTween.Hash(movementAxis, _movement, "easeType", _easeType.ToString(), "loopType", _loopType.ToString(), "delay", _delay, "time", _time));
        }
    }
}
