using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class test1 : MonoBehaviour
{
    public Tilemap map1;


    public LayerMask z1;

    public GameObject[] p1;

    public TileBase[] asd;
    public Vector2 offset;

    // Start is called before the first frame update
    void Start()
    {
        var a = Physics2D.OverlapCircleAll(transform.position,100,z1);

        // for (int i = 0; i < a.Length; i++)
        // {
        //     Vector3Int cc = Vector3Int.FloorToInt(a[i].transform.position);
        //     var x = map1.GetTile(cc);
        //     Debug.Log("뭐야");
        //     Instantiate(p1, cc, quaternion.identity);
        // }

        for (int x = 0; x <= 15; x++)
        {
            for (int y =0; y <= 13; y++)
            {
                Vector3Int cell = map1.WorldToCell(new Vector3(x+offset.x,y+offset.y));
                TileBase tile = map1.GetTile(cell);
        
                if (tile != null)
                {
                    int i = Array.IndexOf(this.asd, tile);
                    var ffff = map1.CellToWorld(cell);
                    ffff += new Vector3(0.5f, 0.5f);
                    Instantiate(p1[i], ffff, quaternion.identity);
                    
                }
            }
        }
        
        map1.gameObject.SetActive(false);
    }

    IEnumerator Itest()
    {
        Vector3 xxx=test2.transform.position + Vector3.left;
        while (test2.transform.position!=xxx)
        {
            var s = Vector3.MoveTowards(test2.transform.position,  xxx, 5*Time.deltaTime);
            test2.transform.position = s;
            yield return null;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            StartCoroutine(Itest());
        }
    }
}
