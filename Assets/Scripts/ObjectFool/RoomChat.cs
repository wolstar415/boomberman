using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomChat : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.SetParent(RoomManager.inst.roomChatParent);
    }

    private void OnDisable()
    {
        ObjectPooler.ReturnToPool(gameObject);
    }
}
