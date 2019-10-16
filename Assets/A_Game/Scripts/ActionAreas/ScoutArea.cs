using Enigma.CoreSystems;
using UnityEngine;

public class ScoutArea : ActionArea
{
    public const string SCOUT_SPRITE_MASKS_PARENT_FIND_PATH = "/ScoutSpriteMasksContainer";
    public const string SCOUTS_SPRITE_MASK_RESOURCE_PATH = "Prefabs/ScoutSpriteMask";

    public float DisplayTime = 2;

    protected override void Awake()
    {
        base.Awake();

        if (OwnerTeamName != _warRef.LocalPlayerTeam)
            GetComponent<SpriteMask>().enabled = false;  //Not needed.
    }

    override protected void Update()
    {
        base.Update();

        if (DisplayTime > 0)
        {
            DisplayTime -= Time.deltaTime;
            if (DisplayTime < 0)
            {
                DisplayTime = 0;
                GetComponent<SpriteRenderer>().enabled = false;
                Destroy(_effect);
                _effect = null;
            }
        }
    }

    override protected void OnTriggerEnter2D(Collider2D coll)
    {
        base.OnTriggerEnter2D(coll);

        if (base._warRef.GetIsHost())
        {
            HealerArea healerArea = coll.GetComponent<HealerArea>();
            if (healerArea != null)
            {
                if (Strenght > healerArea.Strenght)
                {
                    _warRef.RemoveHealerArea(healerArea);
                    //Debug.LogWarning("HealerArea of owner: " + healerArea.OwnerUnitName + " and team: " + healerArea.OwnerTeamName + " and Healing: " + healerArea.Healing + " was removed by scoutArea of owner " + OwnerUnitName + " and team: " + OwnerTeamName + " and Power: " + Power);
                }
                //else
                //    Debug.LogWarning("HealerArea of owner: " + healerArea.OwnerUnitName + " and team: " + healerArea.OwnerTeamName + " and Healing: " + healerArea.Healing + " endured scoutArea of owner " + OwnerUnitName + " and team: " + OwnerTeamName + " and Power: " + Power);
            }
            else
            {
                TankArea tankArea = coll.GetComponent<TankArea>();
                if (tankArea != null)
                {
                    if (Strenght > tankArea.Strenght)
                    {
                        _warRef.RemoveTankArea(tankArea);
                        //Debug.LogWarning("TankArea of owner: " + tankArea.OwnerUnitName + " and team: " + tankArea.OwnerTeamName + " and Defense: " + tankArea.Defense + " was removed by scoutArea of owner " + OwnerUnitName + " and team: " + OwnerTeamName + " and Power: " + Power);
                    }
                    //else
                    //    Debug.LogWarning("TankArea of owner: " + tankArea.OwnerUnitName + " and team: " + tankArea.OwnerTeamName + " and Defense: " + tankArea.Defense + " endured scoutArea of owner " + OwnerUnitName + " and team: " + OwnerTeamName + " and Power: " + Power);
                }
                else
                {
                    if (_warRef.GetUsesAi())
                    {
                        MinMinUnit minMinUnit = getUnitFromCollider(coll);
                        if (minMinUnit != null)
                        {
                            string targetTeam = GameNetwork.GetOppositeTeamName(OwnerTeamName);
                            if (targetTeam == minMinUnit.TeamName)  //To filter this area colliding against owner team
                                _warRef.HandleAddExposedUnit(targetTeam, minMinUnit);
                        }
                    }
                }
            }
        }
    }
}