using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoomberManager : MonoBehaviour
{
    public static BoomberManager inst;

    public bool IsStart = false;
    public bool IsDead = false;
    [Header("플레이어")] 
    public int characterIdx;
    public GameObject player;
    public int playerIdx;
    public int gameWait = 0;
    [Header("스텟")]
    public int bombAmount = 1;
    public float Speed = 5f;
    public int Power = 1;
    public float bombFuseTime = 3f;
    public int bombsRemaining;
    [Space(20)]
    public List<Vector3> respawnPos;

    public List<int> itemIdx;

    [Header("Map")] public int mapIdxGo;
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

    [Header("프리팹")]
    public string bombPrefab;
    public string[] Items;
    public string[] explosionPreFabs;
    [Header("스크립트")] 
    public MoveMentController moveMent;
    public MapGenerator mapGenerator;

    [Header("오른쪽")] 
    public TMP_InputField playChat;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI[] playernames;
    [Header("데이터")] public CharacterData[] characterDatas;
    private static readonly int IsDead1 = Animator.StringToHash("IsDead");


    private void Awake()
    {
        inst = this;
    }

    public void TimeStart()
    {
        StartCoroutine(Co_TimeStart());
    }

    IEnumerator Co_TimeStart()
    {
        int min = 3;
        int second = 0;
        timeText.text = $"{min:D2}:{second}";
        yield return YieldInstructionCache.WaitForSeconds(1);
        while (true)
        {
            second--;
            if (second < 0)
            {
                second = 59;
                min--;
            }

            if (min < 0)
            {
                break;
            }
            timeText.text = $"{min:D2}:{second}";
            yield return YieldInstructionCache.WaitForSeconds(1);
        }

        //게임 끝
        timeText.text = "GameOver";
        

    }

    public void GameWait()
    {
        gameWait++;
        if (RoomManager.inst.check.currentP == gameWait)
        {
            for (int i = 0; i < RoomManager.inst.check.seatInfos.Length; i++)
            {
                if (RoomManager.inst.check.seatInfos[i].seatname != "" &&
                    RoomManager.inst.check.seatInfos[i].seatname == "막음")
                {
                    playernames[i].text = "";
                }
                else
                {
                    playernames[i].text = RoomManager.inst.check.seatInfos[i].seatname;
                }
                
            }
            
            

            
            
            GameManager.inst.loadingOb.SetActive(false);
            int posInt = mapGenerator.randomPos[playerIdx];
            GameObject ob = Instantiate(NetworkManager.inst.playerPrefabs[characterIdx], BoomberManager.inst.respawnPos[posInt], Quaternion.identity);
            BoomberManager.inst.player = ob;
            NetworkManager.inst.myData.isDead = false;
            NetworkManager.inst.myData.isMoving = false;
            NetworkManager.inst.myData.horizontal = 0;
            NetworkManager.inst.myData.vertical = -1;
            NetworkManager.inst.myData.pos_x = BoomberManager.inst.respawnPos[posInt].x;
            NetworkManager.inst.myData.pos_y = BoomberManager.inst.respawnPos[posInt].y;
            //ob.GetComponent<CircleCollider2D>().enabled = true;
            ob.transform.GetChild(1).gameObject.SetActive(true);
            BoomberManager.inst.moveMent.rigidbody = ob.GetComponent<Rigidbody2D>();
            BoomberManager.inst.moveMent.ani = ob.GetComponentInChildren<Animator>();
            NetworkManager.inst.players[playerIdx] = ob;
            SocketManager.inst.socket.Emit("CharacterCreate",GameManager.inst.room,BoomberManager.inst.playerIdx,characterIdx);
            characterdataSetting();
            
            
            
            mapGenerator.StartFunc();
            TimeStart();

        }
    }

    public void characterdataSetting()
    {
        BoomberManager.inst.Power = characterDatas[characterIdx].StartPower;
        BoomberManager.inst.bombAmount = characterDatas[characterIdx].StartBomb;
        BoomberManager.inst.Speed = characterDatas[characterIdx].StartSpeed;

        BoomberManager.inst.powerMax = characterDatas[characterIdx].MaxPower;
        BoomberManager.inst.bombMax = characterDatas[characterIdx].MaxBomb;
        BoomberManager.inst.speedMax = characterDatas[characterIdx].MaxSpeed;
        
        BoomberManager.inst.bombsRemaining = BoomberManager.inst.bombAmount;

    }

    public void PowerUp()
    {
        Power++;
        if (Power >= powerMax)
        {
            Power = powerMax;
        }
    }

    public void Dead()
    {
        if (IsDead)
        {
            return;
        }
        SocketManager.inst.socket.Emit("PlayDead",GameManager.inst.room,BoomberManager.inst.playerIdx);
        StartCoroutine(CoDead(player));
        IsDead = true;
        NetworkManager.inst.myData.isDead = true;
    }

    IEnumerator CoDead(GameObject ob)
    {
        ob.GetComponent<CharacterInfo>().ani.SetBool(IsDead1,true);
        ob.GetComponent<CircleCollider2D>().enabled = false;
        yield return YieldInstructionCache.WaitForSeconds(1.5f);
        Destroy(ob);
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
        
        SocketManager.inst.socket.OnUnityThread("GameWait", data =>
        {
            GameWait();

        });
        SocketManager.inst.socket.OnUnityThread("CharacterCreate", data =>
        {
            int posInt = mapGenerator.randomPos[data.GetValue(0).GetInt32()];
            GameObject ob = Instantiate(NetworkManager.inst.playerPrefabs[data.GetValue(1).GetInt32()], BoomberManager.inst.respawnPos[posInt], Quaternion.identity);
            NetworkManager.inst.players[data.GetValue(0).GetInt32()] = ob;

            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()].isDead = false;
            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()].isMoving = false;
            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()].isMoving = false;
            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()].horizontal = 0;
            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()].vertical = -1;
            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()].pos_x = BoomberManager.inst.respawnPos[posInt].x;
            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()].pos_y = BoomberManager.inst.respawnPos[posInt].y;
            
            




        });
        SocketManager.inst.socket.OnUnityThread("PlayChat", data =>
        {
            Chat(data.GetValue(0).GetString(), data.GetValue(1).GetString(), data.GetValue(2).GetInt32());
        });
        SocketManager.inst.socket.OnUnityThread("PlayDead", data =>
        {
            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()].isDead = true;
            StartCoroutine(CoDead(NetworkManager.inst.players[data.GetValue(0).GetInt32()]));
            //데드처리 승패 확인하기
        });
    }
    
    public void OnEndEditEventMethod()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UpdateChat();

        }
    }
    public void UpdateChat()
        //채팅을 입력시 이벤트
    {
        if (playChat.text.Equals(""))
        {
            playChat.gameObject.SetActive(false);
            return;
        }
        //아무것도없다면 리턴

        Chat(GameManager.inst.Id,playChat.text,playerIdx);
        SocketManager.inst.socket.Emit("PlayChat", GameManager.inst.room,GameManager.inst.Id,playChat.text,playerIdx);
        playChat.text = "";
        playChat.gameObject.SetActive(false);
    }
    public void Chat(string name,string s, int idx)
    {
       NetworkManager.inst.players[idx].GetComponent<CharacterInfo>().Chat(name,s);
    }

    public void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Return)&&GameManager.inst.playOb.activeSelf&&!playChat.isFocused)
        {
            playChat.gameObject.SetActive(true);
            playChat.Select();
        }
        
    }
}
