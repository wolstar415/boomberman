using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjeckStart : MonoBehaviour
{
    private void OnDisable()
    {
        CancelInvoke();
        ObjectPooler.ReturnToPool(gameObject);
    }
}
