using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestory : MonoBehaviour
{
    [SerializeField]
    private float deadTime = 1f;
    void Start()
    {
        Destroy(gameObject,deadTime);
    }

}
