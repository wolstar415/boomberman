using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoomController : MonoBehaviour
{
    

    public void Start()
    {
        SocketManager.inst.socket.OnUnityThread("Bomb", data =>
        {
            GameObject bomb = ObjectPooler.SpawnFromPool(BoomberManager.inst.bombPrefab, new Vector3(data.GetValue(3).GetSingle(),data.GetValue(4).GetSingle()), quaternion.identity);
            if (bomb.TryGetComponent(out Boom component))
            {
                component.BoomFunc(data.GetValue(0).GetSingle(),data.GetValue(1).GetInt32(),data.GetValue(2).GetInt32());
            }
        });
    }

    private void OnBomb(InputValue value)
    {
        if (BoomberManager.inst.IsStart == false||BoomberManager.inst.IsDead)
        {
            return;
        }
        
        if (!BoomberManager.inst.playChat.isFocused&&BoomberManager.inst.bombsRemaining >0)
        {
            Vector2 position = BoomberManager.inst.player.transform.position;
            position.x = Mathf.Round(position.x);
            position.y = Mathf.Round(position.y);

            if (Physics2D.OverlapCircle(position, 0.2f, BoomberManager.inst.explodeMask))
            {
                return;
            }
       
            
            BoomberManager.inst.bombsRemaining--;
            GameObject bomb = ObjectPooler.SpawnFromPool(BoomberManager.inst.bombPrefab, position, quaternion.identity);
            SocketManager.inst.socket.Emit("Bomb",GameManager.inst.room,BoomberManager.inst.bombFuseTime,BoomberManager.inst.Power,BoomberManager.inst.playerIdx,position.x,position.y);
            if (bomb.TryGetComponent(out Boom component))
            {
                component.BoomFunc(BoomberManager.inst.bombFuseTime,BoomberManager.inst.Power,BoomberManager.inst.playerIdx);
            }
            
        }
    }
    
}
