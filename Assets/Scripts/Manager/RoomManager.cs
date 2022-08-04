using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    public static RoomManager inst;

    public GameObject mapStartBtn;
    public GameObject mapSelectBtn;
    public GameObject mapReadyBtn;
    public GameObject kickOb;
    public Color[] colors;
    [Header("map")] [SerializeField] private Image mapIcon;
    [SerializeField] private TextMeshProUGUI mapName;
    [SerializeField] private TextMeshProUGUI mapMaxCnt;
    [SerializeField] private Sprite[] mapIcons;
    [SerializeField] private string[] mapNames;
    [SerializeField] private string[] mapCnts;
    [SerializeField] [Multiline(5)] private string[] mapInfos;
    [Header("Slot")] public Sprite xIcon;
    public Sprite[] Icons;
    public Image[] slotIcon;
    public TextMeshProUGUI[] slotName;
    public TextMeshProUGUI[] slotText;
    [Header("채팅")] public Transform roomChatParent;
    [SerializeField] private TMP_InputField roomChatField;
    public List<GameObject> textObs;
    [Header("맵선택")] public Image mapSelectIcon;
    public TextMeshProUGUI mapSelectCntText;
    public TextMeshProUGUI mapSelectInfoText;
    public GameObject[] arrows;
    [Header("캐릭터정보")] public GameObject characterInfoOb;
    public Image[] BombImages;
    public Image[] PowerImages;
    public Image[] SpeedImages;
    public Color[] infoColors;

    public int tempMapSelectIdx = 0;

    public RoomInfo myRoomInfo;

    private void Awake()
    {
        inst = this;
    }

    public void ReadyBtn()
    {
        int idx = 0;
        for (int i = 0; i < slotName.Length; i++)
        {
            if (slotName[i].text == GameManager.inst.Id)
            {
                idx = i;
                break;
            }
        }

        SocketManager.inst.socket.Emit("RoomReady", GameManager.inst.room, idx);
    }

    public void ArrowSet(int idx)
    {
        for (int i = 0; i < arrows.Length; i++)
        {
            arrows[i].SetActive(false);
        }

        arrows[idx].SetActive(true);
    }

    public void StartBtn()
    {
        if (!GameManager.inst.RoomHost)
        {
            return;
        }

        bool b = false;
        for (int i = 0; i < myRoomInfo.seatInfos.Length; i++)
        {
            if (myRoomInfo.seatInfos[i].seatname != "" && myRoomInfo.seatInfos[i].seatname != "막음")
            {
                if (myRoomInfo.seatInfos[i].Idx == 0)
                {
                    b = true;
                    break;
                }
            }
        }

        if (b)
        {
            return;
        }

        int map = 0;
        if (GameManager.inst.mapIdx == 0)
        {
            map = Random.Range(0, BoomberManager.inst.mapGenerator.maps.Length);
        }
        else
        {
            map = GameManager.inst.mapIdx - 1;
        }

        int idx = 0;
        for (int i = 0; i < slotName.Length; i++)
        {
            if (slotName[i].text == GameManager.inst.Id)
            {
                idx = i;
                BoomberManager.inst.playerIdx = i;
                break;
            }
        }

        if (GameManager.inst.characterIdx == 0)
        {
            BoomberManager.inst.characterIdx = Random.Range(0, NetworkManager.inst.playerPrefabs.Length);
        }
        else
        {
            BoomberManager.inst.characterIdx = GameManager.inst.characterIdx - 1;
        }


        BoomberManager.inst.mapIdxGo = map;
        BoomberManager.inst.mapGenerator.MapCreate();
        BoomberManager.inst.mapGenerator.ItemSetting();
        BoomberManager.inst.mapGenerator.CharacterRandomFunc();
        string s1 = JsonConvert.SerializeObject(BoomberManager.inst.itemIdx);
        string s2 = JsonConvert.SerializeObject(BoomberManager.inst.mapGenerator.randomPos);
        SocketManager.inst.socket.Emit("GameStart", GameManager.inst.room, s1, s2, map);
        GameManager.inst.loadingOb.SetActive(true);
        SocketManager.inst.socket.Emit("GameWait", GameManager.inst.room);
        //BoomberManager.inst.mapGenerator.StartFunc();
    }

    public void SelectMapOpenBtn()
    {
        tempMapSelectIdx = 0;
        SelectMapInfoSet(tempMapSelectIdx);
    }

    public void SelectMapInfoSet(int idx)
    {
        mapSelectIcon.sprite = mapIcons[idx];
        mapSelectCntText.text = mapCnts[idx];
        mapSelectInfoText.text = mapInfos[idx];
    }

    public void SelectMapBtn()
    {
        MapInfoSetting(tempMapSelectIdx);
        SocketManager.inst.socket.Emit("MapSetting", GameManager.inst.room, tempMapSelectIdx);
    }

    public void SelectMapChoice(int idx)
    {
        tempMapSelectIdx = idx;
        SelectMapInfoSet(tempMapSelectIdx);
        myRoomInfo.mapIdx = idx;
    }

    public void HostStartFunc()
    {
        mapStartBtn.GetComponent<Button>().interactable = false;
        GameManager.inst.loadingOb.SetActive(false);
        GameManager.inst.lobyOb.SetActive(false);
        GameManager.inst.roomOb.SetActive(true);
        GameManager.inst.CreateRoomOb.SetActive(false);
        GameManager.inst.characterIdx = 0;
        GameManager.inst.isPlaying = false;
        ArrowSet(0);
        BoomberManager.inst.gameWait = 0;
        
        GameManager.inst.RoomHost = true;
        MapInfoSetting(0);
        slotName[0].text = GameManager.inst.Id;
        slotText[0].text = "방장";
        slotText[0].color = colors[2];
        slotIcon[0].sprite = Icons[0];
        for (int i = 1; i < 4; i++)
        {
            slotName[i].text = "";
            slotText[i].text = "준비";
            slotText[i].color = colors[0];
            slotIcon[i].sprite = null;
        }

        GameManager.inst.roomSlot = 0;
    }

    public void CharacterBtn(int characterIdx)
    {
        int idx = 0;
        for (int i = 0; i < slotName.Length; i++)
        {
            if (slotName[i].text == GameManager.inst.Id)
            {
                idx = i;
                break;
            }
        }

        myRoomInfo.seatInfos[idx].characterIdx = characterIdx;
        SocketManager.inst.socket.Emit("CharacterChange", GameManager.inst.room, idx, characterIdx);
        GameManager.inst.characterIdx = characterIdx;
        slotIcon[idx].sprite = Icons[characterIdx];
        ArrowSet(characterIdx);
    }

    public void SlotBtn(int idx)
    {
        if (GameManager.inst.RoomHost)
        {
            if (GameManager.inst.Id == slotName[idx].text)
            {
                return;
            }

            if (slotIcon[idx].sprite == xIcon)
            {
                SocketManager.inst.socket.Emit("SlotOpen", GameManager.inst.room, idx);
            }
            else if (slotName[idx].text == "")
            {
                SocketManager.inst.socket.Emit("SlotClose", GameManager.inst.room, idx);

                //막기
            }
            else if (slotName[idx].text != "")
            {
                SocketManager.inst.socket.Emit("SlotKick", GameManager.inst.room, slotName[idx].text, idx);

                //킥
            }
        }
        else
        {
            if (slotIcon[idx].sprite == xIcon)
            {
                return;
            }

            if (slotName[idx].text != "")
            {
                return;
            }

            if (slotName[idx].text == "")
            {
                int check = 0;
                for (int i = 0; i < slotName.Length; i++)
                {
                    if (slotName[i].text == GameManager.inst.Id)
                    {
                        check = i;
                        break;
                    }
                }

                SocketManager.inst.socket.Emit("SlotMove", GameManager.inst.room, check, idx);
            }
        }
    }

    public void RoomLeave()
    {
        int idx = 0;
        for (int i = 0; i < slotName.Length; i++)
        {
            if (slotName[i].text == GameManager.inst.Id)
            {
                idx = i;
                break;
            }
        }

        SocketManager.inst.socket.Emit("RoomLeave", GameManager.inst.room, idx);
        RoomLeaveFunc();
        GameManager.inst.roomOb.SetActive(false);
        GameManager.inst.lobyOb.SetActive(true);
        LobyManager.inst.LobyTextReset();
        RoomTextReset();
    }

    public void SlotReset(RoomInfo room)
    {
        int readyCheck = 0;
        int readyCheck2 = 0;
        for (int i = 0; i < 4; i++)
        {
            if (room.seatInfos[i].seatname == "막음")
            {
                slotName[i].text = "";
                slotText[i].text = "준비";
                slotText[i].color = colors[0];
                slotIcon[i].sprite = xIcon;
            }
            else if (room.seatInfos[i].seatname == "")
            {
                slotName[i].text = "";
                slotText[i].text = "준비";
                slotText[i].color = colors[0];
                slotIcon[i].sprite = null;
            }
            else
            {
                slotName[i].text = room.seatInfos[i].seatname;
                if (room.seatInfos[i].Idx == 0)
                {
                    slotText[i].text = "준비";
                    slotText[i].color = colors[0];
                    readyCheck++;
                }
                else if (room.seatInfos[i].Idx == 1)
                {
                    slotText[i].text = "준비";
                    readyCheck2++;
                    slotText[i].color = colors[1];
                }
                else if (room.seatInfos[i].Idx == 2)
                {
                    slotText[i].text = "방장";
                    slotText[i].color = colors[2];
                    if (!GameManager.inst.RoomHost && slotName[i].text == GameManager.inst.Id)
                    {
                        GameManager.inst.RoomHost = true;
                    }
                }

                slotIcon[i].sprite = Icons[room.seatInfos[i].characterIdx];
            }
        }

        MapInfoSetting(myRoomInfo.mapIdx);
        if (GameManager.inst.RoomHost && readyCheck == 0 && readyCheck2 >= 1)
        {
            RoomManager.inst.mapStartBtn.GetComponent<Button>().interactable = true;
        }
        else
        {
            RoomManager.inst.mapStartBtn.GetComponent<Button>().interactable = false;
        }
    }
    

    public void MapInfoSetting(int idx)
    {
        GameManager.inst.mapIdx = idx;
        mapIcon.sprite = mapIcons[idx];
        mapName.text = mapNames[idx];
        mapMaxCnt.text = $"인원 : {mapCnts[idx]}";
    }

    // Start is called before the first frame update
    void Start()
    {
        SocketManager.inst.socket.OnUnityThread("SlotReset", data =>
        {
            string s = data.GetValue(0).ToString();
            myRoomInfo = JsonConvert.DeserializeObject<RoomInfo>(s);
            SlotReset(myRoomInfo);
        });
        SocketManager.inst.socket.OnUnityThread("KickCheck", data =>
        {
            if (GameManager.inst.Id == data.GetValue(0).GetString())
            {
                SocketManager.inst.socket.Emit("KickCheck", GameManager.inst.room);

                kickOb.SetActive(true);
                RoomLeaveFunc();
            }
        });
        SocketManager.inst.socket.OnUnityThread("RoomChatGet", data =>
        {
            GameObject ob = ObjectPooler.SpawnFromPool("RoomChat", Vector3.zero);
            ob.GetComponent<TextMeshProUGUI>().text =
                $"{data.GetValue(0).GetString()} : {data.GetValue(1).GetString()}";
            textObs.Add(ob);
        });
        SocketManager.inst.socket.OnUnityThread("GameStart2", data =>
        {
            int idx = 0;
            for (int i = 0; i < slotName.Length; i++)
            {
                if (slotName[i].text == GameManager.inst.Id)
                {
                    idx = i;
                    BoomberManager.inst.playerIdx = i;
                    break;
                }
            }

            if (GameManager.inst.characterIdx == 0)
            {
                BoomberManager.inst.characterIdx = Random.Range(0, NetworkManager.inst.playerPrefabs.Length);
            }
            else
            {
                BoomberManager.inst.characterIdx = GameManager.inst.characterIdx - 1;
            }

            BoomberManager.inst.mapIdxGo = data.GetValue(2).GetInt32();
            BoomberManager.inst.itemIdx = JsonConvert.DeserializeObject<List<int>>(data.GetValue(0).ToString());
            BoomberManager.inst.mapGenerator.randomPos =
                JsonConvert.DeserializeObject<List<int>>(data.GetValue(1).ToString());
            BoomberManager.inst.mapGenerator.MapCreate();
            SocketManager.inst.socket.Emit("GameWait", GameManager.inst.room);
            GameManager.inst.loadingOb.SetActive(true);
            //BoomberManager.inst.mapGenerator.StartFunc();
        });
    }

    public void RoomLeaveFunc()
    {
        GameManager.inst.room = "";
        GameManager.inst.RoomHost = false;
    }

    public void OnEndEditEventMethod()
    {
        if (GameManager.inst.playerKey.UIChat.ChatEnd.triggered)
        {
            UpdateChat();
            roomChatField.ActivateInputField();
            roomChatField.Select();
        }
    }

    public void CharacterSet(int idx)
    {
        int startPower = BoomberManager.inst.characterDatas[idx].StartPower;
        int startBomb = BoomberManager.inst.characterDatas[idx].StartBomb;
        int startSpeed = BoomberManager.inst.characterDatas[idx].StartSpeed;
        int maxPower = BoomberManager.inst.characterDatas[idx].MaxPower;
        int maxBomb = BoomberManager.inst.characterDatas[idx].MaxBomb;
        int maxSpeed = BoomberManager.inst.characterDatas[idx].MaxSpeed;

        for (int i = 0; i < PowerImages.Length; i++)
        {
            if (i <= startPower - 1)
            {
                PowerImages[i].color = infoColors[0];
            }
            else if (i <= maxPower - 1)

            {
                PowerImages[i].color = infoColors[1];
            }
            else
            {
                PowerImages[i].color = infoColors[2];
            }
        }

        for (int i = 0; i < BombImages.Length; i++)
        {
            if (i <= startBomb - 1)
            {
                BombImages[i].color = infoColors[0];
            }
            else if (i <= maxBomb - 1)

            {
                BombImages[i].color = infoColors[1];
            }
            else
            {
                BombImages[i].color = infoColors[2];
            }
        }

        for (int i = 0; i < SpeedImages.Length; i++)
        {
            if (i <= startSpeed - 1)
            {
                SpeedImages[i].color = infoColors[0];
            }
            else if (i <= maxSpeed - 1)

            {
                SpeedImages[i].color = infoColors[1];
            }
            else
            {
                SpeedImages[i].color = infoColors[2];
            }
        }

        characterInfoOb.SetActive(true);
    }
    public void RoomTextReset()
        //로비 채팅 대화청소
    {
        foreach (var ob in textObs)
        {
            ob.gameObject.SetActive(false);
        }

        textObs.Clear();
    }
    public void UpdateChat()
        //채팅을 입력시 이벤트
    {
        if (roomChatField.text.Equals(""))
        {
            return;
        }
        //아무것도없다면 리턴

        GameObject ob = ObjectPooler.SpawnFromPool("RoomChat", Vector3.zero);
        ob.GetComponent<TextMeshProUGUI>().text = $"> {GameManager.inst.Id} : {roomChatField.text}";
        textObs.Add(ob);
        SocketManager.inst.socket.Emit("RoomChat", GameManager.inst.Id, roomChatField.text, GameManager.inst.room);
        //딴사람들에게도 채팅내용을 받아야하니 이벤트를 보냅니다.
        roomChatField.text = "";
    }
}