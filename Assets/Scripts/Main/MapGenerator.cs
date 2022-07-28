using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[Serializable]
public class MapInfo
{
    public int itemPowerCnt;
    public int itemBombCnt;
    public int itemSpeedCnt;
    public int playerMax;
}
public class MapGenerator : MonoBehaviour
{
    public Tilemap[] maps;
    public GameObject[] obs;
    public TileBase[] tiles;
    public TileBase respawnTile;
    public Vector2 offset;
    public List<Vector3> temp_pos;
    public List<int> itemList;
    public List<int> itemListTemp;
    public List<MapInfo> mapInfos;

    // Start is called before the first frame update
    void Start()
    {
        temp_pos.Clear();
        itemList.Clear();
        int itemAmount = 0;
        for (int x = 0; x <= 15; x++)
        {
            for (int y = 0; y <= 13; y++)
            {
                Vector3Int cell = maps[BoomberManager.inst.mapIdx].WorldToCell(new Vector3(x + offset.x, y + offset.y));
                TileBase tile = maps[BoomberManager.inst.mapIdx].GetTile(cell);

                if (tile == null)
                {
                    continue;
                }

                if (tile==respawnTile)
                {
                    temp_pos.Add(maps[BoomberManager.inst.mapIdx].CellToWorld(cell) + new Vector3(0.5f, 0.5f));
                    continue;
                }
                int idx = 0;

                for (int i = 0; i < tiles.Length; i++)
                {
                    if (tile == tiles[i])
                    {
                        idx = i;
                        break;
                    }
                }
                var pos = maps[BoomberManager.inst.mapIdx].CellToWorld(cell);
                pos += new Vector3(0.5f, 0.5f);
                GameObject Brick= Instantiate(obs[idx], pos, quaternion.identity);
                if (Brick.TryGetComponent(out Brick b))
                {
                    b.Idx = itemAmount;
                    itemAmount++;
                    itemList.Add(0);
                }
                
            }
        }

        BoomberManager.inst.respawnPos = temp_pos.ToList();
        maps[BoomberManager.inst.mapIdx].gameObject.SetActive(false);
        ItemSetting();
    }

    public void ItemSetting()
    {
        itemListTemp.Clear();
        int powerCnt = mapInfos[BoomberManager.inst.mapIdx].itemPowerCnt;
        int bombCnt = mapInfos[BoomberManager.inst.mapIdx].itemBombCnt;
        int speedCnt = mapInfos[BoomberManager.inst.mapIdx].itemSpeedCnt;

        itemListTemp = itemList.ToList();
        for (int i = 0; i < powerCnt; i++)
        {
            if (itemListTemp.Count == 0)
            {
                return;
            }

            int ran = Random.Range(0, itemListTemp.Count);
            itemList[ran] = 1;
            itemListTemp.RemoveAt(ran);
        }
        for (int i = 0; i < bombCnt; i++)
        {
            if (itemListTemp.Count == 0)
            {
                return;
            }

            int ran = Random.Range(0, itemListTemp.Count);
            itemList[ran] = 2;
            itemListTemp.RemoveAt(ran);
        }
        for (int i = 0; i < speedCnt; i++)
        {
            if (itemListTemp.Count == 0)
            {
                return;
            }

            int ran = Random.Range(0, itemListTemp.Count);
            itemList[ran] = 3;
            itemListTemp.RemoveAt(ran);
        }


        BoomberManager.inst.itemIdx = itemList.ToList();
    }
}