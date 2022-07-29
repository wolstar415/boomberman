using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EItemType
{
    none,
    Speed,
    Power,
    Bomb
}
public class Item : MonoBehaviour
{
    public EItemType itemType;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            switch (itemType)
            {
                case EItemType.none:
                    break;
                case EItemType.Speed:
                    BoomberManager.inst.SpeedUp();
                    break;
                case EItemType.Power:
                    BoomberManager.inst.PowerUp();
                    break;
                case EItemType.Bomb:
                    BoomberManager.inst.BombUp();
                    break;
                default:
                    break;
            }
            
            gameObject.SetActive(false);
        }
    }
}
