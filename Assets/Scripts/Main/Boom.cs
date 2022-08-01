using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Boom : MonoBehaviour
{
    private int bombPower;
    public new BoxCollider2D collider;
    public int playerIdx;
    Collider2D[] b = new Collider2D[2];
    public bool isDead = false;

    public void BoomFunc(float fuseTime, int _power, int idx)
    {
        
        isDead = false;
        bombPower = _power;
        playerIdx = idx;
        StartCoroutine(PlaceBomb(fuseTime));
    }

    private IEnumerator PlaceBomb(float fuseTime)
    //시간이 지나면 터짐
    {
        yield return YieldInstructionCache.WaitForSeconds(fuseTime);
        DestroyFunc();
    }

    private void DestroyFunc()
    {
        Vector2 pos = transform.position;

        GameObject explosion =
            ObjectPooler.SpawnFromPool(BoomberManager.inst.explosionPreFabs[0], pos, quaternion.identity);


        Explode(pos, Vector2.up, bombPower);
        //위로 터짐
        Explode(pos, Vector2.down, bombPower);
        //아래로 터짐
        Explode(pos, Vector2.left, bombPower);
        //왼쪽
        Explode(pos, Vector2.right, bombPower);
        //오른쪽
        Dead();
    }

    private void Dead()
    {
        if (isDead)
        {
            return;
        }

        if (BoomberManager.inst.playerIdx == playerIdx)
            //자기 폭탄이라면 개수가 다시 채워지게
        {
            BoomberManager.inst.bombsRemaining++;
        }

        isDead = true;
        gameObject.SetActive(false);
        collider.isTrigger = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Physics2D.OverlapCircleNonAlloc(transform.position, 0.1f, b, BoomberManager.inst.playerMask) == 0)
                //유닛이 빠져나가면 폭탄 위를 못 지나가게 설정
            {
                collider.isTrigger = false;
            }
        }
    }



    private void Explode(Vector2 pos, Vector2 direction, int length)
    //폭파 함수
    {
        if (length <= 0)
            //길이가 끝나면 끝
        {
            return;
        }

        pos += direction;
        //위치를 방향을 더합니다.

        Collider2D hit = Physics2D.OverlapCircle(pos, 0.2f, BoomberManager.inst.boomMask);
        //위치에 뭐가있는지 확인합니다.

        if (hit != null)
        {
            if (hit.CompareTag("Wall"))
                //벽이라면 아무것도 안합니다.
            {
                return;
            }
            else if (hit.CompareTag("Brick") || hit.CompareTag("Brick_Move"))
                //파괴가능한 벽이거나 이동가능한 벽이라면
            {
                if (hit.transform.TryGetComponent(out Brick b))
                {
                    b.Func();
                    //파괴애니메이션을 보여주고 파괴시킴
                }

                return;
            }
            else if (hit.CompareTag("Item"))
                //아이템이라면 파괴
            {
                hit.gameObject.SetActive(false);
            }
            else if (hit.CompareTag("Bomb"))
                //같은 폭탄이라면
            {
                if (hit.transform.TryGetComponent(out Boom b))
                {
                    if (b.isDead == true)
                    {
                        return;
                    }

                    b.Dead();
                    //해당 폭탄을 즉시 삭제시킵니다.
                }

                GameObject explosionOb =
                    ObjectPooler.SpawnFromPool(BoomberManager.inst.explosionPreFabs[1], pos, quaternion.identity);
                //폭파 이펙트 생성
                
                explosionOb.transform.rotation =
                    Quaternion.AngleAxis(SetDirection(direction), Vector3.forward);
                //각도 설정
                
                int power = hit.GetComponent<Boom>().bombPower;
                //해당 폭탄의 파워를 가져옵니다.
                if (direction == Vector2.up)
                    //현재 방향이 위쪽이라면 위 왼쪽 오른쪽
                {
                    Explode(pos, Vector2.up, power);
                    Explode(pos, Vector2.left, power);
                    Explode(pos, Vector2.right, power);
                }
                else if (direction == Vector2.down)
                {
                    Explode(pos, Vector2.down, power);
                    Explode(pos, Vector2.left, power);
                    Explode(pos, Vector2.right, power);
                }
                else if (direction == Vector2.left)
                {
                    Explode(pos, Vector2.up, power);
                    Explode(pos, Vector2.down, power);
                    Explode(pos, Vector2.left, power);
                }
                else if (direction == Vector2.right)
                {
                    Explode(pos, Vector2.up, power);
                    Explode(pos, Vector2.down, power);
                    Explode(pos, Vector2.right, power);
                }

                return;
            }
        }

        GameObject explosionOb2 = null;

        explosionOb2 = ObjectPooler.SpawnFromPool(length > 1 ? BoomberManager.inst.explosionPreFabs[1] : BoomberManager.inst.explosionPreFabs[2], pos, Quaternion.identity);

        explosionOb2.transform.rotation = Quaternion.AngleAxis(SetDirection(direction), Vector3.forward);

        length--;

        if (length <= 0)
        {
            return;
        }

        Explode(pos, direction, length);
    }
    
    private float SetDirection(Vector2 direction)
        //폭파 이펙트 각도 설정
    {
        return Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg;
    }
}