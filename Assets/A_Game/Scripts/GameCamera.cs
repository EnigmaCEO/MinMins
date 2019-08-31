﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public Camera MyCamera;

    [SerializeField] private Transform _team1_transform;
    [SerializeField] private Transform _team2_transform;

    [SerializeField] private float _movementDelay = 0.1f;
    [SerializeField] private float _movementTime = 1;

    private string _positionSideTeam = GameNetwork.VirtualPlayerIds.HOST;

    public delegate void OnMovementCompletedDelegate(string teamName);
    static public OnMovementCompletedDelegate OnMovementCompletedCallback;

    public void SetCameraForGuest()
    {
        MyCamera.projectionMatrix = MyCamera .projectionMatrix * Matrix4x4.Scale(new Vector3(-1, 1, 1));
        Vector3 team2_pos = _team2_transform.position;
        transform.position = new Vector3(team2_pos.x, team2_pos.y, transform.position.z);
    }

    public void HandleMovement(string teamInTurn, MinMinUnit.Types unitType)
    {
        string oppositeTeam = GameNetwork.GetOppositeTeamName(teamInTurn);

        if ((unitType == MinMinUnit.Types.Bombers) || (unitType == MinMinUnit.Types.Destroyers) || (unitType == MinMinUnit.Types.Scouts))
            moveToSide(oppositeTeam);
        else
            moveToSide(teamInTurn);
    }

    private void moveToSide(string sideToMoveTeam)
    {
        if (sideToMoveTeam == _positionSideTeam)
        {
            if (OnMovementCompletedCallback != null)
                OnMovementCompletedCallback(sideToMoveTeam);
        }
        else
        {
            Transform sideTransform = null;

            if (sideToMoveTeam == GameNetwork.VirtualPlayerIds.HOST)
                sideTransform = _team1_transform;
            else if (sideToMoveTeam == GameNetwork.VirtualPlayerIds.GUEST)
                sideTransform = _team2_transform;

            iTween.MoveTo(gameObject, iTween.Hash("position", sideTransform.position, "easeType", iTween.EaseType.easeInOutExpo,
                                                "loopType", iTween.LoopType.none, "delay", _movementDelay,
                                                "time", _movementTime, "oncomplete", "movementReady",
                                                "oncompleteparams", iTween.Hash("teamName", sideToMoveTeam)));
        }
    }

    private void movementReady(object onCompleteParams)
    {
        Hashtable hashTable = (Hashtable)onCompleteParams;

        if (OnMovementCompletedCallback != null)
            OnMovementCompletedCallback((string)hashTable["teamName"]);
    }
}
