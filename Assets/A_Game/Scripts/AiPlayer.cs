using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlayer
{
    public const float _MAX_DECISION_DELAY = 1.5f;
    public const float _MIN_DECISION_DELAY = 0.5f;

    public bool LastBomberAttackWasSuccessful = false;
    public bool LastDestroyerAttackWasSuccessful = false;

    private int _lastBomberTargetGridIndex = -1;
    private int _lastDestroyerTargetGridIndex = -1;

    private List<Vector2> _gridTargets = new List<Vector2>();

    private const int _GRID_CELLS_PER_AXIS_AMOUNT = 10;

    private List<int> _bomberGridTargetIndexesTried = new List<int>();
    private List<int> _destroyerGridTargetIndexesTried = new List<int>();
    private List<int> _scoutGridTargetIndexesTried = new List<int>();

    public AiPlayer()
    {
        createGridTargets();
    }

    public Vector2 GetWorldInput2D(MinMinUnit.Types unitType, Dictionary<string, MinMinUnit> aiPlayerTeamUnits, Dictionary<string, List<MinMinUnit>> exposedUnitsByTeam,  Dictionary<string, Dictionary<string, Dictionary<string, List<HealerArea>>>> healerAreasByOwnerByTargetByTeam)
    {
        int targetIndex = -1;
        Vector2 input = Vector2.zero;

        if (unitType == MinMinUnit.Types.Bomber)
        {
            targetIndex = getTargetIndexForAttackers(LastBomberAttackWasSuccessful, _lastBomberTargetGridIndex, _bomberGridTargetIndexesTried);
            _lastBomberTargetGridIndex = targetIndex;
            LastBomberAttackWasSuccessful = false;
        }
        else if (unitType == MinMinUnit.Types.Destroyer)
        {
            targetIndex = getTargetIndexForAttackers(LastDestroyerAttackWasSuccessful, _lastDestroyerTargetGridIndex, _destroyerGridTargetIndexesTried);
            _lastDestroyerTargetGridIndex = targetIndex;
            LastDestroyerAttackWasSuccessful = false;
        }
        else if (unitType == MinMinUnit.Types.Scout)
            targetIndex = getRandomTargetIndexNotTried(_scoutGridTargetIndexesTried);

        if (targetIndex != -1)
            input = _gridTargets[targetIndex];
        else if (unitType == MinMinUnit.Types.Tank)
            input = getTankTargetPosition(aiPlayerTeamUnits);
        else if (unitType == MinMinUnit.Types.Healer)
            input = getHealerTargetPosition(aiPlayerTeamUnits, exposedUnitsByTeam, healerAreasByOwnerByTargetByTeam);

        return input;
    }

    private void createGridTargets()
    {
        GameConfig gameConfig = GameConfig.Instance;

        float horizontalWidth = gameConfig.BattleFieldMaxPos.x - gameConfig.BattleFieldMinPos.x;
        float horizontalCellSize = horizontalWidth / _GRID_CELLS_PER_AXIS_AMOUNT;

        float verticalWidth = gameConfig.BattleFieldMaxPos.y - gameConfig.BattleFieldMinPos.y;
        float verticalCellSize = verticalWidth / _GRID_CELLS_PER_AXIS_AMOUNT;

        int amountOfTargetsPerAxis = _GRID_CELLS_PER_AXIS_AMOUNT - 1;

        float x = gameConfig.BattleFieldMinPos.x;

        for (int i = 0; i < amountOfTargetsPerAxis; i++)
        {
            x += horizontalCellSize;
            float y = gameConfig.BattleFieldMinPos.y;
            for (int j = 0; j < amountOfTargetsPerAxis; j++)
            {
                y += verticalCellSize;
                _gridTargets.Add(new Vector2(x, y));
            }
        }
    }

    private int getTargetIndexForAttackers(bool lastAttackWasSuccessful, int lastTargetGridIndex, List<int> targetIndexesTried)
    {
        int targetIndex = -1;
        if (lastAttackWasSuccessful)
            targetIndex = lastTargetGridIndex;
        else
            targetIndex = getRandomTargetIndexNotTried(targetIndexesTried);

        return targetIndex;
    }

    private int getRandomTargetIndexNotTried(List<int> targetIndexesTried)
    {
        List<int> candidateIndexes = new List<int>();
        for (int i = 0; i < _gridTargets.Count; i++)
        {
            if (!targetIndexesTried.Contains(i))
                candidateIndexes.Add(i);
        }

        int selectedIndex = Random.Range(0, candidateIndexes.Count);
        targetIndexesTried.Add(selectedIndex);

        return selectedIndex;
    }

    private Vector2 getTankTargetPosition(Dictionary<string, MinMinUnit> aiPlayerTeamUnits)
    {
        Vector2 input = Vector2.zero;

        int maxInjury = 0;
        MinMinUnit maxInjuryUnit = null;
        List<MinMinUnit> attackerUnits = new List<MinMinUnit>();
        List<MinMinUnit> aliveUnits = new List<MinMinUnit>();

        foreach (MinMinUnit unit in aiPlayerTeamUnits.Values)
        {
            if (unit.gameObject.GetActive())
            {
                aliveUnits.Add(unit);

                if ((unit.Type == MinMinUnit.Types.Bomber) || (unit.Type == MinMinUnit.Types.Destroyer))
                    attackerUnits.Add(unit);

                int injury = GetUnitInjury(unit.name);
                if (injury > 0)
                {
                    if (injury > maxInjury)
                    {
                        maxInjury = injury;
                        maxInjuryUnit = unit;
                    }
                    else if (injury == maxInjury)
                    {
                        if (Random.Range(0, 2) == 1)  //So selected among units with same injury is not the last unit
                            maxInjuryUnit = unit;
                    }
                }
            }
        }

        if (maxInjuryUnit != null)
            input = maxInjuryUnit.GetBattlefieldPosition();
        else
        {
            if (attackerUnits.Count > 0)
                input = getRandomUnitFromList(attackerUnits).GetBattlefieldPosition();
            else
                input = getRandomUnitFromList(aliveUnits).GetBattlefieldPosition();
        }

        return input;
    }

    private Vector2 getHealerTargetPosition(Dictionary<string, MinMinUnit> aiPlayerTeamUnits, Dictionary<string, List<MinMinUnit>> exposedUnitsByTeam, Dictionary<string, Dictionary<string, Dictionary<string, List<HealerArea>>>> healerAreasByOwnerByTargetByTeam)
    {
        Vector2 input = Vector2.zero;

        int maxInjury = 0;
        MinMinUnit maxInjuryUnit = null;

        List<MinMinUnit> injuredUnitsWithNoHealingPending = new List<MinMinUnit>();
        List<MinMinUnit> aliveUnits = new List<MinMinUnit>();

        foreach (MinMinUnit unit in aiPlayerTeamUnits.Values)
        {
            if (unit.gameObject.GetActive())
            {
                aliveUnits.Add(unit);

                int injury = GetUnitInjury(unit.name);
                if (injury > 0)
                {
                    if (!healerAreasByOwnerByTargetByTeam[GameNetwork.TeamNames.GUEST].ContainsKey(unit.name))
                    {
                        if (injury > maxInjury)
                        {
                            maxInjury = injury;
                            maxInjuryUnit = unit;
                        }
                        else if (injury == maxInjury)
                        {
                            if (Random.Range(0, 2) == 1)  //So selected among units with same injury is not the last unit
                                maxInjuryUnit = unit;
                        }
                    }
                }
            }
        }

        if (maxInjuryUnit != null)
            input = maxInjuryUnit.GetBattlefieldPosition();
        else
        {
            List<MinMinUnit> exposedUnits = exposedUnitsByTeam[GameNetwork.TeamNames.GUEST];
            if (exposedUnits.Count > 0)
                input = getRandomUnitFromList(exposedUnits).GetBattlefieldPosition();
            else
                input = getRandomUnitFromList(aliveUnits).GetBattlefieldPosition();
        }

        return input;
    }

    private int GetUnitInjury(string unitName)
    {
        int unitHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.HEALTH, GameNetwork.TeamNames.GUEST, unitName);
        int unitMaxHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.MAX_HEALTH, GameNetwork.TeamNames.GUEST, unitName);

        int injury = unitMaxHealth - unitHealth;

        return injury;
    }

    private MinMinUnit getRandomUnitFromList(List<MinMinUnit> units)
    {
        MinMinUnit selectedUnit = null;
        if (units.Count > 0)
        {
            if (units.Count == 1)
                selectedUnit = units[0];
            else
            {
                int randomIndex = Random.Range(0, units.Count);
                selectedUnit = units[randomIndex];
            }
        }

        return selectedUnit;
    }
}
