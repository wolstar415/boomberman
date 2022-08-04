using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Brick : MonoBehaviour
{
    [SerializeField] private Animator ani;


    private static readonly int Dead = Animator.StringToHash("Dead");

    public int Idx = 0;

    public bool isMoving = false;

    // Start is called before the first frame update
    public void Func()
    {
        StartCoroutine(IFunc());
    }

    public void Move(Vector3 direction)
    {
        if (isMoving)
        {
            return;
        }

        isMoving = true;
        StartCoroutine(CoMove(direction));
    }

    IEnumerator CoMove(Vector3 direction)
    {
        Vector3 pos = transform.position + direction;
        float t = 0;
        while (Vector3.Distance(transform.position,pos)>0.1f)
        {
            if (t >= 1f || isMoving == false)
            {
                yield break;
            }

            Vector3 movePos = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * 5);
            transform.position = movePos;
            t += Time.deltaTime;


            yield return null;
        }

        transform.position = pos;
        isMoving = false;
    }

    IEnumerator IFunc()
    {
        ani.SetTrigger(Dead);
        yield return YieldInstructionCache.WaitForSeconds(0.3f);
        int item = BoomberManager.inst.itemIdx[Idx];
        if (item >= 1)
        {
            GameObject ob = ObjectPooler.SpawnFromPool(BoomberManager.inst.Items[item], transform.position,
                quaternion.identity);
            ob.GetComponent<Item>().Idx = Idx;
            BoomberManager.inst.itemDictionary.Add(Idx, ob);
        }

        gameObject.SetActive(false);
    }
}