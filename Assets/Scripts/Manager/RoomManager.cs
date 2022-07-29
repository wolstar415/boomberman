using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
    public static RoomManager inst;

    public GameObject mapStartBtn;
    public GameObject mapSelectBtn;
    public GameObject kickOb;
    public Color[] colors;
    [Header("map")] 
    [SerializeField] private Image mapIcon;
    [SerializeField] private TextMeshProUGUI mapName;
    [SerializeField] private TextMeshProUGUI mapMaxCnt;
    [SerializeField] private Sprite[] mapIcons;
    [SerializeField] private string[] mapNames;
    [SerializeField] private string[] mapCnts;
    [SerializeField] [Multiline(5)] private string[] mapInfos;
    [Header("Slot")]
    public Sprite xIcon;
    public Sprite[] Icons;
    public Image[] slotIcon;
    public TextMeshProUGUI[] slotName;
    public TextMeshProUGUI[] slotText;
    [Header("채팅")]
    public Transform roomChatParent;
    [SerializeField] private TMP_InputField roomChatField;
    public List<GameObject> textObs;
    
    public RoomInfo check;
    private void Awake()
    {
        inst = this;
    }

    public void HostStartFunc()
    {
        
        GameManager.inst.RoomHost = true;
        RoomManager.inst.MapInfoSetting(0);
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
                SocketManager.inst.socket.Emit("SlotOpen",GameManager.inst.room,idx);
                slotName[idx].text = "";
                slotText[idx].text = "준비";
                slotText[idx].color = colors[0];
                slotIcon[idx].sprite = null;
            }
            else if (slotName[idx].text == "")
            {
                SocketManager.inst.socket.Emit("SlotClose",GameManager.inst.room,idx);
                slotName[idx].text = "";
                slotText[idx].text = "준비";
                slotText[idx].color = colors[0];
                slotIcon[idx].sprite = xIcon;
                //막기
            }
            else if (slotName[idx].text != "")
            {
                SocketManager.inst.socket.Emit("SlotKick",GameManager.inst.room,slotName[idx].text,idx);
                slotName[idx].text = "";
                slotText[idx].text = "준비";
                slotText[idx].color = colors[0];
                slotIcon[idx].sprite = null;
                //킥
            }
        }
        else
        {
            if (slotIcon[idx].sprite == xIcon)
            {
                return;
            }
            if (slotText[idx].text != "")
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
                SocketManager.inst.socket.Emit("SlotMove",GameManager.inst.room,check,idx);
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
        SocketManager.inst.socket.Emit("RoomLeave",GameManager.inst.room,idx);
        RoomLeaveFunc();
        GameManager.inst.roomOb.SetActive(false);
        GameManager.inst.lobyOb.SetActive(true);
    }

    public void SlotReset(RoomInfo room)
    {
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
                }
                else if (room.seatInfos[i].Idx == 1)
                {
                    slotText[i].text = "준비";
                    slotText[i].color = colors[1];
                }
                else if (room.seatInfos[i].Idx == 2)
                {
                    slotText[i].text = "방장";
                    slotText[i].color = colors[2];
                }
                slotIcon[i].sprite = Icons[room.seatInfos[i].characterIdx];
            }
            
        }
    }

    public void SlotReSet(int idx)
    {
        for (int i = 0; i < 4; i++)
        {
            if (LobyManager.inst.roomInfos[idx].seatInfos[i].seatname == "막음")
            {
                slotName[i].text = "";
                slotText[i].text = "준비";
                slotText[i].color = colors[0];
                slotIcon[i].sprite = xIcon;
            }
            else if (LobyManager.inst.roomInfos[idx].seatInfos[i].seatname == "")
            {
                slotName[i].text = "";
                slotText[i].text = "준비";
                slotText[i].color = colors[0];
                slotIcon[i].sprite = null;
            }
            else
            {
                slotName[i].text = LobyManager.inst.roomInfos[idx].seatInfos[i].seatname;
                if (LobyManager.inst.roomInfos[idx].seatInfos[i].Idx == 0)
                {
                    slotText[i].text = "준비";
                    slotText[i].color = colors[0];
                }
                else if (LobyManager.inst.roomInfos[idx].seatInfos[i].Idx == 1)
                {
                    slotText[i].text = "준비";
                    slotText[i].color = colors[1];
                }
                else if (LobyManager.inst.roomInfos[idx].seatInfos[i].Idx == 2)
                {
                    slotText[i].text = "방장";
                    slotText[i].color = colors[2];
                }
                slotIcon[i].sprite = Icons[LobyManager.inst.roomInfos[idx].seatInfos[i].characterIdx];
            }
            
        }
    }

    public void MapInfoSetting(int idx)
    {
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
            check = JsonConvert.DeserializeObject<RoomInfo>(s);
            SlotReset(check);
        });
        SocketManager.inst.socket.OnUnityThread("KickCheck", data =>
        {
            if (GameManager.inst.Id == data.GetValue(0).GetString())
            {
                SocketManager.inst.socket.Emit("KickCheck",GameManager.inst.room);

                kickOb.SetActive(true);
                RoomLeaveFunc();
            }
        });
        SocketManager.inst.socket.OnUnityThread("RoomChat", data =>
        {
            GameObject ob = ObjectPooler.SpawnFromPool("RoomChat",Vector3.zero);
            ob.GetComponent<TextMeshProUGUI>().text = $"{data.GetValue(0).GetString()} : {data.GetValue(1).GetString()}";
            textObs.Add(ob);
        });
        
    }

    public void RoomLeaveFunc()
    {
        GameManager.inst.room = "";
        GameManager.inst.RoomHost = false;

    }
    
    public void OnEndEditEventMethod()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UpdateChat();
            roomChatField.ActivateInputField();
            roomChatField.Select();
        }
    }
    public void UpdateChat()
        //채팅을 입력시 이벤트
    {
        if (roomChatField.text.Equals(""))
        {
            return;
        }
        //아무것도없다면 리턴

        GameObject ob = ObjectPooler.SpawnFromPool("RoomChat",Vector3.zero);
        ob.GetComponent<TextMeshProUGUI>().text = $"> {GameManager.inst.Id} : {roomChatField.text}";
        textObs.Add(ob);
        SocketManager.inst.socket.Emit("RoomChat", GameManager.inst.Id, roomChatField.text,GameManager.inst.room);
        //딴사람들에게도 채팅내용을 받아야하니 이벤트를 보냅니다.
        roomChatField.text = "";
    }

}
