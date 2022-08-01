using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SeatInfo
{
    public string seatname;
    public int characterIdx;

    public int Idx;
    //0 준비안함
    //1 준비함
    //2 방장
}

[Serializable]
public class RoomInfo
{
    public string name;
    public int currentP;
    public int maxP;
    public SeatInfo[] seatInfos = new SeatInfo[4];
    public bool isPlaying;
    public int mapIdx;
}

public class LobyManager : MonoBehaviour
{
    public static LobyManager inst;

    private void Awake()
    {
        inst = this;
    }


    public RoomInfo[] roomInfos; //전체 방목록
    public List<RoomInfo> roomInfos2; //입장 가능한 방목록

    public int currentRoomPage; // 현재 페이지

    private int MaxRoomPage() // 최대페이지
    {
        if (roomInfos2.Count == 0)
        {
            return 0;
        }

        var count = roomInfos2.Count / 6;
        if (count <= 0) return 0;
        if (roomInfos2.Count % 6 == 0)
        {
            return count - 1;
        }

        return count;
    }

    [Header("방")] public Button[] roomBtn;
    public TextMeshProUGUI[] roomName;
    public TextMeshProUGUI[] roomCnt;

    [Header("버튼")] public Button nextBtn; //방 다음 목록
    public Button beforeBtn; //방 이전 목록

    [Header("채팅")] public Transform lobyChatParent;
    [SerializeField] private TMP_InputField lobyChatField;
    public List<GameObject> textObs;
    [Header("방만들기")] [SerializeField] private TMP_InputField lobyCreateRoomField;
    [SerializeField] private Toggle[] lobyCreateRoomToggle;


    public void OnEndEditEventMethod()
        //로비창 채팅
    {
        if (GameManager.inst.playerKey.UIChat.ChatEnd.triggered)
            //엔터키 누르면
        {
            UpdateChat();
            lobyChatField.ActivateInputField();
            lobyChatField.Select();
        }
    }

    public void UpdateChat()
        //채팅을 입력시 이벤트
    {
        if (lobyChatField.text.Equals(""))
        {
            return;
        }
        //아무것도없다면 리턴

        GameObject ob = ObjectPooler.SpawnFromPool("LobyChat", Vector3.zero);
        ob.GetComponent<TextMeshProUGUI>().text = $"> {GameManager.inst.Id} : {lobyChatField.text}";
        textObs.Add(ob);
        SocketManager.inst.socket.Emit("LobyChat", GameManager.inst.Id, lobyChatField.text);
        //딴사람들에게도 채팅내용을 받아야하니 이벤트를 보냅니다.
        lobyChatField.text = "";
    }

    public void LobyTextReset()
        //로비 채팅 대화청소
    {
        foreach (var ob in textObs)
        {
            ob.gameObject.SetActive(false);
        }

        textObs.Clear();
    }

    public void RoomClear()
        //방 전부 청소
    {
        for (int i = 0; i < roomBtn.Length; i++)
        {
            roomBtn[i].interactable = false;
            roomName[i].text = "";
            roomCnt[i].text = "";
        }

        nextBtn.interactable = false;
        beforeBtn.interactable = false;
    }

    void RoomSet(int currentPage)
        //방 설정
    {
        if (roomInfos2.Count == 0)
        {
            RoomClear();
            return;
        }

        if (currentPage < 0 || currentPage > MaxRoomPage())
        {
            Debug.LogError("오류");
            currentPage = 0;
        }

        int startIdx = (6 * currentPage);

        if (startIdx >= roomInfos2.Count)
        {
            Debug.LogError("오류");
            startIdx = 0;
        }

        for (int i = 0; i < 6; i++)
        {
            if (startIdx + i >= roomInfos2.Count)
            {
                roomBtn[i].interactable = false;
                roomName[i].text = "";
                roomCnt[i].text = "";
            }
            else
            {
                roomBtn[i].interactable = true;
                roomName[i].text = roomInfos2[startIdx + i].name;
                roomCnt[i].text = $"{roomInfos2[startIdx + i].currentP}/{roomInfos2[startIdx + i].maxP}";
            }
        }
    }

    public void RoomReset()
    {
        roomInfos2.Clear();

        for (int i = 0; i < roomInfos.Length; i++)
        {
            if (roomInfos[i].currentP >= roomInfos[i].maxP)
            {
                continue;
            }

            if (roomInfos[i].isPlaying)
            {
                continue;
            }

            roomInfos2.Add(roomInfos[i]);
        }

        if (currentRoomPage > MaxRoomPage() || currentRoomPage < 0)
        {
            currentRoomPage = 0;
        }

        if (currentRoomPage >= 1)
        {
            beforeBtn.interactable = true;
        }
        else
        {
            beforeBtn.interactable = false;
        }

        if (MaxRoomPage() > currentRoomPage)
        {
            nextBtn.interactable = true;
        }
        else
        {
            nextBtn.interactable = false;
        }

        RoomSet(currentRoomPage);
    }

    public void JoinBtn(int idx)
        //방 클릭
    {
        if ((currentRoomPage * 6) + idx >= roomInfos2.Count)
        {
            Debug.Log("오류");
            return;
        }

        GameManager.inst.loadingOb.SetActive(true);
        SocketManager.inst.socket.Emit("JoinRoomCheck", roomName[idx].text, GameManager.inst.Id);
    }

    public void MoveRoomBtn(int idx)
        //방목록버튼 클릭
    {
        if (idx == 0)
        {
            currentRoomPage--;
            RoomReset();
        }
        else
        {
            currentRoomPage++;
            RoomReset();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SocketManager.inst.socket.OnUnityThread("LobyChatGet", data =>
            //로비 채팅
        {
            GameObject ob = ObjectPooler.SpawnFromPool("LobyChat", Vector3.zero);
            ob.GetComponent<TextMeshProUGUI>().text =
                $"{data.GetValue(0).GetString()} : {data.GetValue(1).GetString()}";
            textObs.Add(ob);
        });


        SocketManager.inst.socket.OnUnityThread("CreateRoom", data =>
            //방 생성
        {
            GameManager.inst.room = lobyCreateRoomField.text;
            RoomManager.inst.HostStartFunc();
        });
        SocketManager.inst.socket.OnUnityThread("RoomReset", data =>
            //방정보들 리셋
        {
            string s = data.GetValue(0).ToString();
            roomInfos = s != "" ? JsonConvert.DeserializeObject<RoomInfo[]>(s) : Array.Empty<RoomInfo>();
            RoomReset();
        });
        SocketManager.inst.socket.OnUnityThread("Join", data =>
            //참가
        {
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.lobyOb.SetActive(false);
            GameManager.inst.roomOb.SetActive(true);
            GameManager.inst.CreateRoomOb.SetActive(false);
            GameManager.inst.room = data.GetValue(0).GetString();
            string s = data.GetValue(1).ToString();
            RoomManager.inst.myRoomInfo = JsonConvert.DeserializeObject<RoomInfo>(s);
            RoomManager.inst.SlotReset(RoomManager.inst.myRoomInfo);
            GameManager.inst.characterIdx = 0;
            GameManager.inst.isPlaying = false;
            RoomManager.inst.ArrowSet(0);
            BoomberManager.inst.gameWait = 0;
        });
    }

    public void CreateFunc1()
        //방만들기 버튼
    {
        GameManager.inst.CreateRoomOb.SetActive(true);
        lobyCreateRoomToggle[0].isOn = true;
        lobyCreateRoomField.text = $"{GameManager.inst.Id}의 방";
        lobyCreateRoomField.Select();
    }

    public void CreatRoomBtn()
    //방생성버튼
    {
        if (lobyCreateRoomField.text == "")
        {
            return;
        }

        GameManager.inst.loadingOb.SetActive(true);
        SocketManager.inst.socket.Emit("CreateRoomCheck", lobyCreateRoomField.text, GameManager.inst.Id);
    }
}