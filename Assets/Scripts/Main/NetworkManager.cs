using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager inst;

    public GameObject[] players;
    public Vector2[] playersPos;

    private void Awake()
    {
        inst = this;
    }

    public GameObject[] playerPrefabs;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
