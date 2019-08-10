using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : SingletonMonobehaviour<GameConfig>
{
    [SerializeField] public Vector2 BattleFieldMaxPos = new Vector2(6, 4.5f);
    [SerializeField] public Vector2 BattleFieldMinPos = new Vector2(-6, -1);
}
