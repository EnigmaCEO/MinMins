using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [SerializeField] private Transform _team1_transform;
    [SerializeField] private Transform _team2_transform;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    public void SetCameraForEnemies()
    {
        _camera.projectionMatrix = _camera.projectionMatrix * Matrix4x4.Scale(new Vector3(-1, 1, 1));
        Vector3 team2_pos = _team2_transform.position;
        transform.position = new Vector3(team2_pos.x, team2_pos.y, transform.position.z);
    }
}
