using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackEffect : MonoBehaviour
{
    public int Power;

    private War _war;

    public void SetWar(War war)
    {
        _war = war;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.Log(coll.gameObject.name + " hit " + name);
        if (!_war.GetComponent<War>().Ready)
        {
            WarUnitSprite warUnit = coll.gameObject.GetComponent<WarUnitSprite>();
            GameObject unit = warUnit.Unit;
            //UIProgressBar val = unit.GetComponentInChildren<UIProgressBar>();
            //Image lifeFill = warUnit.LifeFill;

            //if (lifeFill != null)
            //    lifeFill.fillAmount -= 0.25f;

            //int type = 1;
            //if (lifeFill.fillAmount <= 0)
            //{
            //    type = 2;
            //    Destroy(coll.transform.parent.gameObject);
            //}

            //GameObject hit = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/Hits/" + type));
            //hit.transform.localPosition = transform.localPosition;
            //hit.AddComponent<VFXSorter>().sortingOrder = 100;
            //Destroy(gameObject);

            //EndAttack();
        }
    }

    void EndAttack()
    {
        Debug.Log("Attack done");
        //_war.Switch();
        Destroy(transform.gameObject);
    }
}
