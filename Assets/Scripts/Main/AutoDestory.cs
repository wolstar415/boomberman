using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestory : MonoBehaviour
{
    [SerializeField]
    private float deadTime = 1f;

    private void OnEnable()
    {
        Invoke(nameof(DestroyFunc),deadTime);
    }

    void DestroyFunc()
    {
        gameObject.SetActive(false);
    }

}
