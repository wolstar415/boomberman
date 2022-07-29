using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobyChat : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.SetParent(LobyManager.inst.lobyChatParent);
    }

    private void OnDisable()
    {
        ObjectPooler.ReturnToPool(gameObject);
    }
}
