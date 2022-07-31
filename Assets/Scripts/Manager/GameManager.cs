using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager inst;

    public GameObject loadingOb;
    public GameObject warningOb;
    public TextMeshProUGUI warningText;
    //public PlayerInput input;
    public PlayerKey playerKey;
    [Header("정보")] 
    public string Id;
    public string room;
    public int roomSlot;
    public int victory;
    public int defeat;
    public int draw;
    private bool _RoomHost;
    public int characterIdx = 0;
    public int mapIdx = 0;
    public bool isPlaying;

    [Header("패널들")] 
    public GameObject loginOb;
    public GameObject createOb;
    public GameObject lobyOb;
    public GameObject CreateRoomOb;
    public GameObject roomOb;
    public GameObject playOb;

    private void Awake()
    {
        inst = this;
        playerKey = new PlayerKey();
        
    }

    private void OnEnable()
    {
        playerKey.Enable();
    }

    private void OnDisable()
    {
        playerKey.Disable();
    }

    public bool RoomHost
    {
        get
        {
            return _RoomHost;
        }
        set
        {
            RoomManager.inst.mapStartBtn.SetActive(value);
            RoomManager.inst.mapSelectBtn.SetActive(value);
            _RoomHost = value;
            RoomManager.inst.mapReadyBtn.SetActive(!value);
        }
    }

    void Start()
    {
        SocketManager.inst.socket.OnUnityThread("Warnning",(data) =>
        {
            Warnning(data.GetValue(0).GetString());
            
        });
    }

    public void Warnning(string s)
    {
        loadingOb.SetActive(false);
        warningText.text = s;
        warningOb.SetActive(true);
    }


    void OnChatEnd()
    {
        if (warningOb.activeSelf)
        {
            warningOb.SetActive(false);
        }
    }
}
