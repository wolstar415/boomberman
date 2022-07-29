using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Boom : MonoBehaviour
{
    private int power;
    public BoxCollider2D collider;
    public int playerIdx;
    Collider2D[] b = new Collider2D[2];
    public bool IsDead = false;
    public void BoomFunc(float FuseTime,int _power,int idx)
    {
        IsDead = false;
        power = _power;
        playerIdx = idx;
        StartCoroutine(PlaceBomb(FuseTime));
    }
    private IEnumerator PlaceBomb(float FuseTime)
    {
        yield return YieldInstructionCache.WaitForSeconds(FuseTime);
        DestroyFunc();

    }

    public void DestroyFunc()
    {
        

        Vector2 pos = transform.position;

        GameObject explosion = ObjectPooler.SpawnFromPool(BoomberManager.inst.explosionPreFabs[0],pos,quaternion.identity);


        Explode(pos, Vector2.up, BoomberManager.inst.Power);
        Explode(pos, Vector2.down, BoomberManager.inst.Power);
        Explode(pos, Vector2.left, BoomberManager.inst.Power);
        Explode(pos, Vector2.right, BoomberManager.inst.Power);
        Dead();
    }

    public void Dead()
    {
        if (IsDead)
        {
            return;
        }
        if (BoomberManager.inst.playerIdx == playerIdx)
        {
            BoomberManager.inst.bombsRemaining++;
        }

        IsDead = true;
        gameObject.SetActive(false);
        collider.isTrigger = true;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //other.isTrigger = false;


            if (Physics2D.OverlapCircleNonAlloc(transform.position, 0.1f, b, BoomberManager.inst.playerMask)==0)
            {
                collider.isTrigger = false;
            }

        }
    }
    
    public float SetDirection(Vector2 direction)
    {
        return Mathf.Atan2(direction.y, direction.x);
        
        
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private void Explode(Vector2 pos, Vector2 direction, int length)
    {
        if (length <= 0)
        {
            return;
        }
        pos += direction;

        Collider2D hit = Physics2D.OverlapCircle(pos, 0.2f, BoomberManager.inst.boomMask);

        if (hit==null)
        {
        }
        else
        {
            if (hit.CompareTag("Wall"))
            {
                return;
            }
            else if (hit.CompareTag("Brick")||hit.CompareTag("Brick_Move"))
            {
                if (hit.transform.TryGetComponent(out Brick b))
                {
                    b.Func();
                }
                return;
            }
            else if (hit.CompareTag("Item"))
            {
                hit.gameObject.SetActive(false);
            }
            else if (hit.CompareTag("Bomb"))
            {
                length += BoomberManager.inst.Power;

                if (hit.transform.TryGetComponent(out Boom b))
                {
                    if (b.IsDead == true)
                    {
                        return;
                    }
                    b.Dead();
                }
                
                GameObject end_explosion = ObjectPooler.SpawnFromPool(BoomberManager.inst.explosionPreFabs[1],pos,quaternion.identity);
                end_explosion.transform.rotation = Quaternion.AngleAxis(SetDirection(direction)*Mathf.Rad2Deg, Vector3.forward);
                int power = hit.GetComponent<Boom>().power;
                if (direction == Vector2.up)
                {
                    Explode(pos, Vector2.up, power);
                    Explode(pos, Vector2.left, power);
                    Explode(pos, Vector2.right, power);
                }
                else if(direction == Vector2.down)
                {
                    Explode(pos, Vector2.down, power);
                    Explode(pos, Vector2.left, power);
                    Explode(pos, Vector2.right, power);
                }
                else if(direction == Vector2.left)
                {
                    Explode(pos, Vector2.up, power);
                    Explode(pos, Vector2.down, power);
                    Explode(pos, Vector2.left, power);
                }
                else if(direction == Vector2.right)
                {
                    Explode(pos, Vector2.up, power);
                    Explode(pos, Vector2.down, power);
                    Explode(pos, Vector2.right, power);
                }
                return;
            }
        }

        GameObject explosion = null;

        if (length > 1)
        {
            explosion = ObjectPooler.SpawnFromPool(BoomberManager.inst.explosionPreFabs[1], pos, quaternion.identity);
        }
        else
        {
            explosion = ObjectPooler.SpawnFromPool(BoomberManager.inst.explosionPreFabs[2],pos,quaternion.identity);
        }
        explosion.transform.rotation = Quaternion.AngleAxis(SetDirection(direction)*Mathf.Rad2Deg, Vector3.forward);
        
        length--;

        if (length<=0)
        {
            return;
        }
        Explode(pos, direction, length);
        
        
    }



}
