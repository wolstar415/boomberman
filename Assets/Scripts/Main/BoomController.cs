using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BoomController : MonoBehaviour
{
    public GameObject bombPrefab;


    private void Update()
    {
        if (BoomberManager.inst.bombsRemaining >0&& Input.GetKeyDown(KeyCode.Space))
        {
            
            
            
            Vector2 position = BoomberManager.inst.player.transform.position;
            position.x = Mathf.Round(position.x);
            position.y = Mathf.Round(position.y);

            if (Physics2D.OverlapCircle(position, 0.2f, BoomberManager.inst.explodeMask))
            {
                return;
            }
       
            
            BoomberManager.inst.bombsRemaining--;
            GameObject bomb = Instantiate(bombPrefab, position, quaternion.identity);
            if (bomb.TryGetComponent(out Boom component))
            {
                component.BoomFunc(BoomberManager.inst.bombFuseTime,BoomberManager.inst.Power,BoomberManager.inst.playerIdx);
            }
            
        }
    }
    
}
