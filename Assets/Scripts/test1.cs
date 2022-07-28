using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class test1 : MonoBehaviour
{
    public GameObject testob;
    // Start is called before the first frame update
    void Start()
    {
        SocketManager.inst.socket.OnUnityThread("Create", data =>
        {
            GameObject ob = Instantiate(NetworkManager.inst.playerPrefabs[data.GetValue(0).GetInt32()], new Vector2(data.GetValue(1).GetSingle(), data.GetValue(2).GetSingle()), quaternion.identity);
            NetworkManager.inst.players[data.GetValue(0).GetInt32()] = ob;
            NetworkManager.inst.playersPos[data.GetValue(0).GetInt32()].x = data.GetValue(1).GetSingle();
            NetworkManager.inst.playersPos[data.GetValue(0).GetInt32()].y = data.GetValue(2).GetSingle();


        });
        SocketManager.inst.socket.OnUnityThread("move", data =>
        {
            NetworkManager.inst.playersPos[data.GetValue(0).GetInt32()].x = data.GetValue(1).GetSingle();
            NetworkManager.inst.playersPos[data.GetValue(0).GetInt32()].y = data.GetValue(2).GetSingle();


        });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            GameObject ob = Instantiate(NetworkManager.inst.playerPrefabs[0], BoomberManager.inst.respawnPos[Random.Range(0,BoomberManager.inst.respawnPos.Count)], quaternion.identity);
            BoomberManager.inst.player = ob;
            BoomberManager.inst.moveMent.rigidbody = ob.GetComponent<Rigidbody2D>();
            BoomberManager.inst.moveMent.ani = ob.GetComponentInChildren<Animator>();
            BoomberManager.inst.IsStart = true;
            BoomberManager.inst.playerIdx = 0;
            SocketManager.inst.socket.Emit("Create",BoomberManager.inst.playerIdx,-12f,7f);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            GameObject ob = Instantiate(NetworkManager.inst.playerPrefabs[0], BoomberManager.inst.respawnPos[Random.Range(0,BoomberManager.inst.respawnPos.Count)], quaternion.identity);
            BoomberManager.inst.player = ob;
            BoomberManager.inst.moveMent.rigidbody = ob.GetComponent<Rigidbody2D>();
            BoomberManager.inst.moveMent.ani = ob.GetComponentInChildren<Animator>();
            BoomberManager.inst.IsStart = true;
            BoomberManager.inst.playerIdx = 1;
            SocketManager.inst.socket.Emit("Create",BoomberManager.inst.playerIdx,0f,7f);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            testob.GetComponent<Rigidbody2D>().position = new Vector2(1, 0);
        }


        if (!BoomberManager.inst.IsStart)
        {
            return;
        }
        for (int i = 0; i < NetworkManager.inst.players.Length; i++)
        {
            if (BoomberManager.inst.playerIdx == i)
            {
                continue;
            }
            if (NetworkManager.inst.players[i]!=null)
            {
                NetworkManager.inst.players[i].transform.position =  Vector3.Lerp(NetworkManager.inst.players[i].transform.position,NetworkManager.inst.playersPos[i],Time.deltaTime*20);
            }
        }
    }
}
