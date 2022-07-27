using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMentController : MonoBehaviour
{
    [field: SerializeField]
    public new Rigidbody2D rigidbody { get; private set;}
    private Vector2 direction = Vector2.down;
    public float speed = 5f;
    public Animator ani;
    private static readonly int Horizontal = Animator.StringToHash("horizontal");
    private static readonly int Vertical = Animator.StringToHash("vertical");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    // Start is called before the first frame update


  

    // Update is called once per frame
    void Update()
    {

        
        
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
        
        
        
        
        
        
        
        
        
        
        
        
        
    }

    private void FixedUpdate()
    {
        if (direction==Vector2.zero)
        {
            return;
        }
        Vector2 position = rigidbody.position;
        Vector2 translation = direction * (speed * Time.fixedDeltaTime);
        
        rigidbody.MovePosition(position + translation);
    }

    private void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;
        
    }
}
