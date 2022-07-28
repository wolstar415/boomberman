using System;
using System.Collections;
using System.Collections.Generic;
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

        if (BoomberManager.inst.IsStart == false)
        {
            return;
        }
        
        
        if (Input.GetKey(KeyCode.UpArrow))
        {
            SetDirection(Vector2.up);
            ani.SetFloat(Horizontal,0);
            ani.SetFloat(Vertical,1);
            ani.SetBool(IsMoving,true);
            
            
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {

            ani.SetFloat(Horizontal,0);
            ani.SetFloat(Vertical,-1);
            SetDirection(Vector2.down);
            ani.SetBool(IsMoving,true);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {

            ani.SetFloat(Horizontal,-1);
            ani.SetFloat(Vertical,0);
            ani.SetBool(IsMoving,true);
            SetDirection(Vector2.left);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {

            ani.SetFloat(Horizontal,1);
            ani.SetFloat(Vertical,0);
            ani.SetBool(IsMoving,true);
            SetDirection(Vector2.right);
        }
        else
        {
            if (direction!=Vector2.zero)
            {
                
                ani.SetBool(IsMoving,false);
                SetDirection(Vector2.zero);
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
        if (direction==Vector2.zero)
        {
            return;
        }
        Vector2 position = rigidbody.position;
        Vector2 translation = direction * (    BoomberManager.inst.Speed*Time.fixedDeltaTime);
        rigidbody.MovePosition(position + translation);
        
        SocketManager.inst.socket.Emit("move",BoomberManager.inst.playerIdx,BoomberManager.inst.player.transform.position.x,BoomberManager.inst.player.transform.position.y);
    }

    private void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;
        
    }
}
