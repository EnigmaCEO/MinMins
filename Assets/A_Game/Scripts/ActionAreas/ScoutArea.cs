using Enigma.CoreSystems;
using UnityEngine;

public class ScoutArea : ActionArea
{
    public const string SCOUT_SPRITE_MASKS_PARENT_FIND_PATH = "/ScoutSpriteMasksContainer";
    public const string SCOUTS_SPRITE_MASK_RESOURCE_PATH = "Prefabs/ScoutSpriteMask";

    public int Power = 0;
    public float DisplayTime = 2;

    protected override void Awake()
    {
        base.Awake();

        Power = int.Parse(getOwnerUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT));
        //Power = Random.Range(1, 6); // Power hack.

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

        if (NetworkManager.GetIsMasterClient())
        {
            HealerArea healerArea = coll.GetComponent<HealerArea>();
            if (healerArea != null)
            {
                if (Power > healerArea.Healing)
                {
                    _warRef.RemoveHealerArea(healerArea);
                    Debug.LogWarning("HealerArea of owner: " + healerArea.OwnerUnitName + " and team: " + healerArea.OwnerTeamName + " and Healing: " + healerArea.Healing + " was removed by scoutArea of owner " + OwnerUnitName + " and team: " + OwnerTeamName + " and Power: " + Power);
                }
                else
                    Debug.LogWarning("HealerArea of owner: " + healerArea.OwnerUnitName + " and team: " + healerArea.OwnerTeamName + " and Healing: " + healerArea.Healing + " endured scoutArea of owner " + OwnerUnitName + " and team: " + OwnerTeamName + " and Power: " + Power);
            }
            else
            {
                TankArea tankArea = coll.GetComponent<TankArea>();
                if (tankArea != null)
                {
                    if (Power > tankArea.Defense)
                    {
                        _warRef.RemoveTankArea(tankArea);
                        Debug.LogWarning("TankArea of owner: " + tankArea.OwnerUnitName + " and team: " + tankArea.OwnerTeamName + " and Defense: " + tankArea.Defense + " was removed by scoutArea of owner " + OwnerUnitName + " and team: " + OwnerTeamName + " and Power: " + Power);
                    }
                    else
                        Debug.LogWarning("TankArea of owner: " + tankArea.OwnerUnitName + " and team: " + tankArea.OwnerTeamName + " and Defense: " + tankArea.Defense + " endured scoutArea of owner " + OwnerUnitName + " and team: " + OwnerTeamName + " and Power: " + Power);
                }
            }
        }
    }
}