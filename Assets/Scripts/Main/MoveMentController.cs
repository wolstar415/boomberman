using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;

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
    
    private void OnMovement(InputValue value)
    //키보드 방향키를 누르면 발동
    {
        if (BoomberManager.inst.IsStart == false || BoomberManager.inst.IsDead || BoomberManager.inst.player == null)
            //시작전이면 발동 X
        {
            return;
        }

        Vector2 v = value.Get<Vector2>();
        //현재 키보드 방향 구함
        
        if (v == Vector2.up)
            //위쪽
        {
            direction = Vector2.up;
            ani.SetFloat(Horizontal, 0);
            ani.SetFloat(Vertical, 1);
            ani.SetBool(IsMoving, true);
            NetworkManager.inst.myData.horizontal = 0;
            NetworkManager.inst.myData.vertical = 1;
            NetworkManager.inst.myData.isMoving = true;
        }
        else if (v == Vector2.down)
            //아래
        {
            ani.SetFloat(Horizontal, 0);
            ani.SetFloat(Vertical, -1);
            direction = Vector2.down;
            ani.SetBool(IsMoving, true);
            NetworkManager.inst.myData.horizontal = 0;
            NetworkManager.inst.myData.vertical = -1;
            NetworkManager.inst.myData.isMoving = true;
        }
        else if (v == Vector2.left)
            //왼쪽
        {
            ani.SetFloat(Horizontal, -1);
            ani.SetFloat(Vertical, 0);
            ani.SetBool(IsMoving, true);
            direction = Vector2.left;
            NetworkManager.inst.myData.horizontal = -1;
            NetworkManager.inst.myData.vertical = 0;
            NetworkManager.inst.myData.isMoving = true;
        }
        else if (v == Vector2.right)
            //오른쪽
        {
            ani.SetFloat(Horizontal, 1);
            ani.SetFloat(Vertical, 0);
            ani.SetBool(IsMoving, true);
            direction = Vector2.right;
            NetworkManager.inst.myData.horizontal = 1;
            NetworkManager.inst.myData.vertical = 0;
            NetworkManager.inst.myData.isMoving = true;
        }
        else if (v == Vector2.zero)
            //움직임 키를 뗌
        {
            ani.SetBool(IsMoving, false);
            direction = Vector2.zero;
            NetworkManager.inst.myData.isMoving = false;
            string s = JsonConvert.SerializeObject(NetworkManager.inst.myData);
            SocketManager.inst.socket.Emit("Playmove", GameManager.inst.room, BoomberManager.inst.playerIdx, s);
        }
    }

    private void Update()
    {
        if (BoomberManager.inst.IsStart == false || BoomberManager.inst.IsDead || BoomberManager.inst.player == null)
        {
            return;
        }

        if (direction != Vector2.zero)
            //벽을 이동시키는 함수
        {
            BrickMoveCheck();
        }
    }

    private void BrickMoveCheck()
    {
        RaycastHit2D check = Physics2D.Raycast(BoomberManager.inst.player.transform.position, direction, 1f,
            BoomberManager.inst.brickMask);
        //바라보는쪽에 이동 가능한 벽이 있다면
        if (!check)
        {
            moveOb = null;
            moveTime = 0;
        }
        else
        {
            //0.2초동안 벽을 향해서 이동한다면 이동
            if (check.transform.CompareTag("Brick_Move"))
            {
                if (moveOb != check.transform.gameObject)
                    //혹시 같은 벽이 아니라면 버그
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
                        if (!Physics2D.OverlapCircle(check.transform.position + (Vector3)direction, 0.2f,
                                BoomberManager.inst.moveCheckMask))
                        {
                            if (moveOb.TryGetComponent(out Brick b))
                            {
                                if (b.isMoving == false)
                                {
                                    b.Move(direction);
                                    var position = b.transform.position;
                                    SocketManager.inst.socket.Emit("BrickMove", GameManager.inst.room, position.x,
                                        position.y, direction.x, direction.y, b.Idx);
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    private void FixedUpdate()
    //움직이는 부분
    {
        if (BoomberManager.inst.IsStart == false)
        {
            return;
        }

        for (int i = 0; i < NetworkManager.inst.players.Length; i++)
            //현재 방에있는 플레이어들의 정보를 가져옵니다.
        {
            if (BoomberManager.inst.playerIdx == i)
            {
                continue;
            }

            if (NetworkManager.inst.players[i] != null)
                //해당 플레이어가 존재한다면 실행합니다.
            {
                Vector3 pos = new Vector3(NetworkManager.inst.playerDatas[i].pos_x,
                    NetworkManager.inst.playerDatas[i].pos_y);
                float horizontal = NetworkManager.inst.playerDatas[i].horizontal;
                float vertical = NetworkManager.inst.playerDatas[i].vertical;
                bool move = NetworkManager.inst.playerDatas[i].isMoving;
                if (NetworkManager.inst.players[i].TryGetComponent(out CharacterInfo info))
                {
                    info.ani.SetFloat(Horizontal, horizontal);
                    info.ani.SetFloat(Vertical, vertical);
                    info.ani.SetBool(IsMoving, move);
                }

                NetworkManager.inst.players[i].transform.position = pos;

                // NetworkManager.inst.players[i].transform.position =
                //     Vector3.Lerp(NetworkManager.inst.players[i].transform.position, pos, Time.deltaTime * 20);
            }
        }

        if (direction == Vector2.zero)
        {
            return;
        }

        if (BoomberManager.inst.IsDead)
        {
            return;
        }

        Vector2 position = rigidbody.position;
        Vector2 translation = direction * (BoomberManager.inst.Speed * Time.fixedDeltaTime);
        rigidbody.MovePosition(position + translation);
        var position1 = rigidbody.position;
        NetworkManager.inst.myData.pos_x = position1.x;
        NetworkManager.inst.myData.pos_y = position1.y;
        string s = JsonConvert.SerializeObject(NetworkManager.inst.myData);
        SocketManager.inst.socket.Emit("Playmove", GameManager.inst.room, BoomberManager.inst.playerIdx, s);
        //본인의 정보를 같은 방에있는 클라이언트에게 보냅니다.
        
    }

    public void Start()
    {
        SocketManager.inst.socket.OnUnityThread("Playmove", data =>
        //유닛의 정보를 받습니다.
        {
            NetworkManager.inst.playerDatas[data.GetValue(0).GetInt32()] =
                JsonConvert.DeserializeObject<PlayerData>(data.GetValue(1).ToString());
        });
        SocketManager.inst.socket.OnUnityThread("BrickMove", data =>
        //벽을 이동시킵니다.
        {
            GameObject ob = BoomberManager.inst.mapGenerator.brickObs[data.GetValue(0).GetInt32()];
            if (ob.TryGetComponent(out Brick b))
            {
                b.transform.position = new Vector3(data.GetValue(1).GetSingle(), data.GetValue(2).GetSingle());
                b.Move(new Vector3(data.GetValue(3).GetSingle(), data.GetValue(4).GetSingle()));
            }
        });
    }
    
}