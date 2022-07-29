using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
public class LoginManager : MonoBehaviour
{
    public static LoginManager inst;

    [Header("로그인")] 
    public TMP_InputField idFiled;//아이디칸
    public TMP_InputField passwordFiled;//비밀번호 칸
    
    [Header("회원가입")]
    public TMP_InputField createIdFiled;//아이디칸
    public TMP_InputField createPasswordFiled;//비밀번호 칸
    public TMP_InputField createPasswordCheckFiled;//비밀번호 확인 칸

    public TextMeshProUGUI nickNameText;
    
    

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        SocketManager.inst.socket.OnUnityThread("Login", data =>
        {
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.Id = idFiled.text;
            GameManager.inst.victory = data.GetValue(0).GetInt32();
            GameManager.inst.defeat = data.GetValue(1).GetInt32();
            GameManager.inst.loginOb.SetActive(false);
            GameManager.inst.lobyOb.SetActive(true);

            string s = data.GetValue(2).ToString();
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
        SocketManager.inst.socket.Emit("LoginCheck",idFiled.text,passwordFiled.text);
    }

    public void ExitBtn()
    {
        Application.Quit();
    }

    public void CreateBtn()
    {
        if (createIdFiled.text == "" || createPasswordFiled.text == ""|| createPasswordCheckFiled.text == "")
        {
            return;
        }
        if (createPasswordFiled.text != createPasswordCheckFiled.text)
        {
            GameManager.inst.Warnning("비밀번호가 다릅니다.");
            return;
        }
        GameManager.inst.loadingOb.SetActive(true);
        SocketManager.inst.socket.Emit("CreateCheck",createIdFiled.text,createPasswordFiled.text);

    }

}
