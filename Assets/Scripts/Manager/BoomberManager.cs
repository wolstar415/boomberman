using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoomberManager : MonoBehaviour
{
    public static BoomberManager inst;

    public bool IsStart = false;
    public bool IsDead = false;
    [Header("플레이어")] public int characterIdx;
    public GameObject player;
    public int playerIdx;
    public int gameWait = 0;
    [Header("스텟")] public int bombAmount = 1;
    public float Speed = 5f;
    public int Power = 1;
    public float bombFuseTime = 3f;
    public int bombsRemaining;
    [Space(20)] public List<Vector3> respawnPos;

    public List<int> itemIdx;

    [Header("Map")] public int mapIdxGo;
    [Header("LayerMask")] public LayerMask boomMask;
    public LayerMask explodeMask;
    public LayerMask playerMask;
    public LayerMask brickMask;
    public LayerMask moveCheckMask;
    [Header("Max")] [SerializeField] private int powerMax;
    [SerializeField] private int speedMax;
    [SerializeField] private int bombMax;

    [Header("프리팹")] public string bombPrefab;
    public string[] Items;
    public string[] explosionPreFabs;
    [Header("스크립트")] public MoveMentController moveMent;
    public MapGenerator mapGenerator;

    [Header("오른쪽")] public TMP_InputField playChat;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI[] playernames;
    public Image[] playerIcon;
    public GameObject endGameOb;
    public TextMeshProUGUI endGameText;
    [Header("데이터")] public CharacterData[] characterDatas;
    private static readonly int IsDead1 = Animator.StringToHash("IsDead");
    private Coroutine coDead;

    public Dictionary<int, GameObject> itemDictionary = new Dictionary<int, GameObject>();


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
        if (RoomManager.inst.myRoomInfo.currentP == gameWait)
        {
            for (int i = 0; i < NetworkManager.inst.playerDatas.Length; i++)
            {
                NetworkManager.inst.playerDatas[i].isDead = false;
                NetworkManager.inst.playerDatas[i].isMoving = false;
                NetworkManager.inst.playerDatas[i].isMoving = false;
                NetworkManager.inst.playerDatas[i].horizontal = 0;
                NetworkManager.inst.playerDatas[i].vertical = -1;
            }


            for (int i = 0; i < RoomManager.inst.myRoomInfo.seatInfos.Length; i++)
            {
                if (RoomManager.inst.myRoomInfo.seatInfos[i].seatname == "" ||
                    RoomManager.inst.myRoomInfo.seatInfos[i].seatname == "막음")
                {
                    playernames[i].text = "";
                    playerIcon[i].sprite = null;
                }
                else
                {
                    playernames[i].text = RoomManager.inst.myRoomInfo.seatInfos[i].seatname;
                }
            }


            GameManager.inst.loadingOb.SetActive(false);
            int posInt = mapGenerator.randomPos[playerIdx];
            GameObject ob = Instantiate(NetworkManager.inst.playerPrefabs[characterIdx],
                BoomberManager.inst.respawnPos[posInt], Quaternion.identity);
            playerIcon[playerIdx].sprite = RoomManager.inst.Icons[characterIdx + 1];
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
            SocketManager.inst.socket.Emit("CharacterCreate", GameManager.inst.room, BoomberManager.inst.playerIdx,
                characterIdx);
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

        IsDead = true;
        NetworkManager.inst.playerDatas[playerIdx].isDead = true;
        NetworkManager.inst.myData.isDead = true;
        SocketManager.inst.socket.Emit("PlayDead", GameManager.inst.room, BoomberManager.inst.playerIdx);
        StartCoroutine(CoDead(player));
        player = null;
    }

    public IEnumerator DeadChead()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.5f);
        int amount = 0;
        for (int i = 0; i < NetworkManager.inst.players.Length; i++)
        {
            if (NetworkManager.inst.players[i] != null)
            {
                if (!NetworkManager.inst.playerDatas[i].isDead)
                {
                    amount++;
                }
            }
        }

        if (amount <= 1)
        {
            IsStart = false;

            if (amount == 0)
            {
                endGameText.text = "무승부!";
                GameManager.inst.draw++;
                SocketManager.inst.socket.Emit("record", "draw", GameManager.inst.Id, GameManager.inst.draw);
            }
            else
            {
                if (IsDead)
                {
                    endGameText.text = "패배!";
                    GameManager.inst.defeat++;
                    SocketManager.inst.socket.Emit("record", "defeat", GameManager.inst.Id, GameManager.inst.defeat);
                }
                else
                {
                    endGameText.text = "승리!";
                    GameManager.inst.victory++;
                    SocketManager.inst.socket.Emit("record", "victory", GameManager.inst.Id, GameManager.inst.victory);
                }
            }

            endGameOb.SetActive(true);
            LoginManager.inst.ReCordSetting();
            yield return YieldInstructionCache.WaitForSeconds(2f);
            endGameOb.SetActive(false);
            mapGenerator.EndFunc();
            coDead = null;
        }
    }


    IEnumerator CoDead(GameObject ob)
    {
        if (coDead != null)
        {
            StopCoroutine(coDead);
        }

        coDead = StartCoroutine(DeadChead());
        ob.GetComponent<CharacterInfo>().ani.SetBool(IsDead1, true);
        ob.GetComponent<CircleCollider2D>().enabled = false;
        yield return YieldInstructionCache.WaitForSeconds(1.5f);
        ob.SetActive(false);
    }

    public void SpeedUp()
    {
        Speed += 1f;
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
        SocketManager.inst.socket.OnUnityThread("GameWait", data => { GameWait(); });
        SocketManager.inst.socket.OnUnityThread("CharacterCreate", data =>
        {
            int posInt = mapGenerator.randomPos[data.GetValue(0).GetInt32()];
            GameObject ob = Instantiate(NetworkManager.inst.playerPrefabs[data.GetValue(1).GetInt32()],
                BoomberManager.inst.respawnPos[posInt], Quaternion.identity);
            NetworkManager.inst.players[data.GetValue(0).GetInt32()] = ob;


            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()].pos_x =
                BoomberManager.inst.respawnPos[posInt].x;
            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()].pos_y =
                BoomberManager.inst.respawnPos[posInt].y;
            playerIcon[data.GetValue(0).GetInt32()].sprite = RoomManager.inst.Icons[data.GetValue(1).GetInt32() + 1];
        });
        SocketManager.inst.socket.OnUnityThread("PlayChat",
            data => { Chat(data.GetValue(0).GetString(), data.GetValue(1).GetString(), data.GetValue(2).GetInt32()); });
        SocketManager.inst.socket.OnUnityThread("PlayDead", data =>
        {
            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()].isDead = true;
            StartCoroutine(CoDead(NetworkManager.inst.players[data.GetValue(0).GetInt32()]));
            //데드처리 승패 확인하기
        });
        SocketManager.inst.socket.OnUnityThread("ItemRemove", data =>
        {
            if (itemDictionary.ContainsKey(data.GetValue(0).GetInt32()))
            {
                itemDictionary[data.GetValue(0).GetInt32()].SetActive(false);
            }
        });

        SocketManager.inst.socket.OnUnityThread("PlayerExit", data =>
        {
            int idx = 0;
            for (int i = 0; i < playernames.Length; i++)
            {
                if (playernames[i].text == data.GetValue(0).GetString())
                {
                    idx = i;
                    break;
                }
            }

            if (!NetworkManager.inst.playerDatas[idx].isDead)
            {
                NetworkManager.inst.playerDatas[idx].isDead = true;
                StartCoroutine(CoDead(NetworkManager.inst.players[idx]));
            }
        });
    }

    public void OnEndEditEventMethod()
    {
        if (GameManager.inst.playerKey.UIChat.ChatEnd.triggered)
        {
            UpdateChat().Forget();
        }
    }

    public void ExitBtn()
    {
        SocketManager.inst.socket.Emit("RoomLeave", GameManager.inst.room, playerIdx);
        if (!IsDead)
        {
            SocketManager.inst.socket.Emit("PlayDead", GameManager.inst.room, BoomberManager.inst.playerIdx);
        }

        mapGenerator.EndFunc();
        RoomManager.inst.RoomLeaveFunc();
        GameManager.inst.roomOb.SetActive(false);
        GameManager.inst.lobyOb.SetActive(true);
    }

    public async UniTaskVoid UpdateChat()
        //채팅을 입력시 이벤트
    {
        if (playChat.text.Equals(""))
        {
            return;
        }
        //아무것도없다면 리턴

        Chat(GameManager.inst.Id, playChat.text, playerIdx);
        SocketManager.inst.socket.Emit("PlayChat", GameManager.inst.room, GameManager.inst.Id, playChat.text,
            playerIdx);
        playChat.text = "";
        await Task.Yield();
        playChat.gameObject.SetActive(false);
    }

    public void Chat(string name, string s, int idx)
    {
        NetworkManager.inst.players[idx].GetComponent<CharacterInfo>().Chat(name, s);
    }

    private void OnPlayChat()
    {
        if (GameManager.inst.playOb.activeSelf && !playChat.isFocused)
        {
            playChat.gameObject.SetActive(true);
            playChat.ActivateInputField();
            playChat.Select();
        }
    }
}