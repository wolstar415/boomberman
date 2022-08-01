using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public float pos_x;
    //현재위치 x
    public float pos_y;
    //현재위치 y
    
    
    
    public float horizontal;
    public float vertical;
    public bool isMoving;
    public bool isDead;
    //애니메이션
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager inst;
    private void Awake() => inst = this;
    
    
    public GameObject[] players;
    public Vector2[] playersPos;
    public PlayerData[] playerDatas;
    public PlayerData myData;

    

    public GameObject[] playerPrefabs;
}