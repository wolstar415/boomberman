using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomberManager : MonoBehaviour
{
    public static BoomberManager inst;

    public bool IsStart = false;
    [Header("플레이어")] 
    public GameObject player;
    public int playerIdx;
    [Header("스텟")]
    public int bombAmount = 1;
    public float Speed = 5f;
    public int Power = 1;
    public float bombFuseTime = 3f;
    public int bombsRemaining;
    [Space(20)]
    public GameObject[] explosionPreFabs;
    public List<Vector3> respawnPos;

    public GameObject[] Items;
    public List<int> itemIdx;

    [Header("Map")] public int mapIdx;
    [Header("LayerMask")] 
    public LayerMask boomMask;
    public LayerMask explodeMask;
    public LayerMask playerMask;
    public LayerMask brickMask;
    public LayerMask moveCheckMask;
    [Header("Max")] 
    [SerializeField] private int powerMax;
    [SerializeField] private int speedMax;
    [SerializeField] private int bombMax;

    [Header("스크립트")] 
    public MoveMentController moveMent;
    public MapGenerator mapGenerator;
    private void Awake()
    {
        inst = this;
    }

    public void PowerUp()
    {
        Power++;
        if (Power >= powerMax)
        {
            Power = powerMax;
        }
    }

    public void SpeedUp()
    {
        Speed+=1f;
        if (Speed >= speedMax)
        {
            Speed = speedMax;
        }
    }

    public void BombUp()
    {
        if (bombAmount < bombMax)
        {
            bombAmount++;
            bombsRemaining++;
        }
    }
    
    

    
    // Start is called before the first frame update
    void Start()
    {
        bombsRemaining = bombAmount;
    }
    
    
    

}
