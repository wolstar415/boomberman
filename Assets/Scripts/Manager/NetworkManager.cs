using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public float pos_x;
    public float pos_y;
    public float horizontal;
    public float vertical;
    public bool isMoving;
    public bool isDead;
    

}
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager inst;

    public GameObject[] players;
    public Vector2[] playersPos;

    public PlayerData[] playerDatas;
    public PlayerData myData;
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
