using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (BoomberManager.inst.IsStart&&col.CompareTag("Player")&&BoomberManager.inst.player==col.gameObject)
        {
            BoomberManager.inst.Dead();
        }
    }
}
