using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoginManager : MonoBehaviour
{
    public static LoginManager inst;
    public PlayerInput playerInput;

    [Header("로그인")] public TMP_InputField idFiled; //아이디칸
    public TMP_InputField passwordFiled; //비밀번호 칸

    [Header("회원가입")] public TMP_InputField createIdFiled; //아이디칸
    public TMP_InputField createPasswordFiled; //비밀번호 칸
    public TMP_InputField createPasswordCheckFiled; //비밀번호 확인 칸

    public TextMeshProUGUI nickNameText;
    public TextMeshProUGUI recordText;


    private void Awake()
    {
        inst = this;
    }

    public void ReCordSetting()
    {
        recordText.text = $"{GameManager.inst.victory}승{GameManager.inst.defeat}패{GameManager.inst.draw}무";
    }

    private void Start()
    {
        idFiled.Select();
        SocketManager.inst.socket.OnUnityThread("Login", data =>
        {
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.Id = idFiled.text;
            GameManager.inst.victory = data.GetValue(0).GetInt32();
            GameManager.inst.defeat = data.GetValue(1).GetInt32();
            GameManager.inst.draw = data.GetValue(2).GetInt32();
            GameManager.inst.loginOb.SetActive(false);
            GameManager.inst.lobyOb.SetActive(true);
            ReCordSetting();

            string s = data.GetValue(3).ToString();
            if (s != "")
            {
                LobyManager.inst.roomInfos = JsonConvert.DeserializeObject<RoomInfo[]>(s);
            }

            nickNameText.text = idFiled.text;
            LobyManager.inst.RoomReset();
        });
        SocketManager.inst.socket.OnUnityThread("Create", data =>
        {
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.createOb.SetActive(false);
            idFiled.text = createIdFiled.text;
            passwordFiled.text = "";
            passwordFiled.Select();
        });
    }

    public void LoginBtn()
    {
        if (idFiled.text == "" || passwordFiled.text == "")
        {
            return;
        }

        GameManager.inst.loadingOb.SetActive(true);
        SocketManager.inst.socket.Emit("LoginCheck", idFiled.text, passwordFiled.text);
    }

    public void ExitBtn()
    {
        Application.Quit();
    }

    public void CreateBtn()
    {
        if (createIdFiled.text == "" || createPasswordFiled.text == "" || createPasswordCheckFiled.text == "")
        {
            return;
        }

        if (createPasswordFiled.text != createPasswordCheckFiled.text)
        {
            GameManager.inst.Warnning("비밀번호가 다릅니다.");
            return;
        }

        GameManager.inst.loadingOb.SetActive(true);
        SocketManager.inst.socket.Emit("CreateCheck", createIdFiled.text, createPasswordFiled.text);
    }

    public void LoginEnter()
    {
        if (GameManager.inst.playerKey.UIChat.ChatEnd.triggered)
        {
            LoginBtn();
        }
    }

    public void CreateEnter()
    {
        if (GameManager.inst.playerKey.UIChat.ChatEnd.triggered)
        {
            CreateBtn();
        }
    }


    public async UniTaskVoid OnTab()
    {
        if (GameManager.inst.loginOb.activeSelf)
        {
            await Task.Yield();
            if (idFiled.isFocused)
            {
                passwordFiled.Select();
            }

            if (passwordFiled.isFocused)
            {
                idFiled.Select();
            }

            if (createIdFiled.isFocused)
            {
                createPasswordFiled.Select();
            }

            if (createPasswordFiled.isFocused)
            {
                createPasswordCheckFiled.Select();
            }

            if (createPasswordCheckFiled.isFocused)
            {
                createIdFiled.Select();
            }
        }
    }
}