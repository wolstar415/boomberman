using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BoomController : MonoBehaviour
{
    public GameObject bombPrefab;
    public float bombFuseTime = 3f;
    public int bombAmount = 1;
    private int bombsRemaining;


    private void OnEnable()
    {
        bombsRemaining = bombAmount;
    }

    private void Update()
    {
        if (bombsRemaining >0&& Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(PlaceBomb());
        }
    }

    private IEnumerator PlaceBomb()
    {
        Vector2 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);
        GameObject bomb = Instantiate(bombPrefab, position, quaternion.identity);
        bombsRemaining--;

        yield return YieldInstructionCache.WaitForSeconds(bombFuseTime);
        Destroy(bomb);
        bombsRemaining++;
    }

    // private void OnDisable()
    // {
    //     throw new NotImplementedException();
    // }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bomb"))
        {
            other.isTrigger = false;
        }
    }
}
