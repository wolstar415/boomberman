using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SocketIOClient;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SocketManager : MonoBehaviour
{
    public SocketIOUnity socket;

    public static SocketManager inst;

    private void Awake()
    {
        if (inst == null) //instance가 null. 즉, 시스템상에 존재하고 있지 않을때
        {
            inst = this; //내자신을 instance로 넣어줍니다.
            DontDestroyOnLoad(gameObject); //OnLoad(씬이 로드 되었을때) 자신을 파괴하지 않고 유지
        }
        else
        {
            if (inst != this) //instance가 내가 아니라면 이미 instance가 하나 존재하고 있다는 의미
                Destroy(this.gameObject); //둘 이상 존재하면 안되는 객체이니 방금 AWake된 자신을 삭제
        }
    }


    async UniTask Start()
    {
        Setting();

        await socket.ConnectAsync();
        SceneManager.LoadScene("Main");
    }

    public void Setting()
    {
        var uri = new Uri("http://127.0.0.1:7777");

        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                { "token", "UNITY" },
                { "name", "bomberman" },
                { "version", "0.1" }
            },
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        socket.OnConnected += (sender, e) => { Debug.Log("연결 성공"); };

        socket.OnDisconnected += (sender, e) => { Debug.Log("연결 끊김: " + e); };
    }

    private void OnDestroy()
    {
        socket.Disconnect();
        socket.Dispose();
    }
}