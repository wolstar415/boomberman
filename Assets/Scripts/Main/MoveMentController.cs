using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class MoveMentController : MonoBehaviour
{
    public new Rigidbody2D rigidbody;
    public Vector2 direction = Vector2.down;
    public Animator ani;
    private static readonly int Horizontal = Animator.StringToHash("horizontal");
    private static readonly int Vertical = Animator.StringToHash("vertical");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    [SerializeField] private GameObject moveOb;
    [SerializeField] private float moveTime;
    

    

    // Start is called before the first frame update


    
  

    // Update is called once per frame
    void Update()
    {

        if (BoomberManager.inst.IsStart == false||BoomberManager.inst.IsDead)
        {
            return;
        }
        
        
        if (Input.GetKey(KeyCode.UpArrow))
        {
            SetDirection(Vector2.up);
            ani.SetFloat(Horizontal,0);
            ani.SetFloat(Vertical,1);
            ani.SetBool(IsMoving,true);
            NetworkManager.inst.myData.horizontal = 0;
            NetworkManager.inst.myData.vertical = 1;
            NetworkManager.inst.myData.isMoving = true;


        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {

            ani.SetFloat(Horizontal,0);
            ani.SetFloat(Vertical,-1);
            SetDirection(Vector2.down);
            ani.SetBool(IsMoving,true);
            NetworkManager.inst.myData.horizontal = 0;
            NetworkManager.inst.myData.vertical = -1;
            NetworkManager.inst.myData.isMoving = true;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {

            ani.SetFloat(Horizontal,-1);
            ani.SetFloat(Vertical,0);
            ani.SetBool(IsMoving,true);
            SetDirection(Vector2.left);
            NetworkManager.inst.myData.horizontal = -1;
            NetworkManager.inst.myData.vertical = 0;
            NetworkManager.inst.myData.isMoving = true;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {

            ani.SetFloat(Horizontal,1);
            ani.SetFloat(Vertical,0);
            ani.SetBool(IsMoving,true);
            SetDirection(Vector2.right);
            NetworkManager.inst.myData.horizontal = 1;
            NetworkManager.inst.myData.vertical = 0;
            NetworkManager.inst.myData.isMoving = true;
        }
        else
        {
            if (direction!=Vector2.zero)
            {
                
                ani.SetBool(IsMoving,false);
                SetDirection(Vector2.zero);
                NetworkManager.inst.myData.isMoving = false;
                string s = JsonConvert.SerializeObject(NetworkManager.inst.myData);
                SocketManager.inst.socket.Emit("Playmove",GameManager.inst.room,BoomberManager.inst.playerIdx,s);

            }
            
        }

        if (direction != Vector2.zero)
        {
            RaycastHit2D check = Physics2D.Raycast(BoomberManager.inst.player.transform.position, direction, 1f,
                BoomberManager.inst.brickMask);
            if (!check)
            {
                moveOb = null;
                moveTime = 0;
            }
            else
            {
                if (check.transform.CompareTag("Brick_Move"))
                {
                    if (moveOb != check.transform.gameObject)
                    {
                        moveTime = 0;
                        moveOb = check.transform.gameObject;
                
                    }
                    else
                    {
                        moveTime += Time.deltaTime;
                        if (moveTime >= 0.2f)
                        {
                            moveTime = 0;
                            if (!Physics2D.OverlapCircle(check.transform.position+ (Vector3)direction, 0.2f,
                                    BoomberManager.inst.moveCheckMask))
                            {
                                if (moveOb.TryGetComponent(out Brick b))
                                {
                                    if (b.isMoving == false)
                                    {
                                        b.Move(direction);
                                        SocketManager.inst.socket.Emit("BrickMove",GameManager.inst.room,direction.x,direction.y,b.Idx);
                                    }
                                }
                            }
                            
                        }
                    }
                }
            }
            
            
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
    }

    private void FixedUpdate()
    {
        if (BoomberManager.inst.IsStart == false)
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
                Vector3 pos = new Vector3(NetworkManager.inst.playerDatas[i].pos_x,NetworkManager.inst.playerDatas[i].pos_y);
                float horizontal = NetworkManager.inst.playerDatas[i].horizontal;
                float vertical = NetworkManager.inst.playerDatas[i].vertical;
                bool move=NetworkManager.inst.playerDatas[i].isMoving;
                if (NetworkManager.inst.players[i].TryGetComponent(out CharacterInfo info))
                {
                    info.ani.SetFloat(Horizontal,horizontal);
                    info.ani.SetFloat(Vertical,vertical);
                    info.ani.SetBool(IsMoving,move);
                }
                NetworkManager.inst.players[i].transform.position =  Vector3.Lerp(NetworkManager.inst.players[i].transform.position,pos,Time.deltaTime*20);
            }
        }
        
        if (direction==Vector2.zero)
        {
            return;
        }
        Vector2 position = rigidbody.position;
        Vector2 translation = direction * (    BoomberManager.inst.Speed*Time.fixedDeltaTime);
        rigidbody.MovePosition(position + translation);
        var position1 = rigidbody.position;
        NetworkManager.inst.myData.pos_x = position1.x;
        NetworkManager.inst.myData.pos_y = position1.y;
        string s = JsonConvert.SerializeObject(NetworkManager.inst.myData);
        SocketManager.inst.socket.Emit("Playmove",GameManager.inst.room,BoomberManager.inst.playerIdx,s);
        
        
    }

    public void Start()
    {
        SocketManager.inst.socket.OnUnityThread("Playmove", data =>
        {

            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()] =
                JsonConvert.DeserializeObject<PlayerData>(data.GetValue(1).ToString());

        });
        SocketManager.inst.socket.OnUnityThread("BrickMove", data =>
        {

            GameObject ob = BoomberManager.inst.mapGenerator.brickObs[data.GetValue(2).GetInt32()];
            if (ob.TryGetComponent(out Brick b))
            {
                b.Move(new Vector3(data.GetValue(0).GetSingle(),data.GetValue(1).GetSingle()));
            }


        });
        
    }

    private void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;
        
    }
}
