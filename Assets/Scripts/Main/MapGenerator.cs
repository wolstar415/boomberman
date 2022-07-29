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
    public String[] tilePrefabs;
    public TileBase[] tiles;
    public TileBase respawnTile;
    public Vector3 offset;
    public List<Vector3> temp_pos;
    public List<int> itemList;
    public List<int> itemListTemp;
    public List<MapInfo> mapInfos;
    public List<GameObject> tileObs;

    // Start is called before the first frame update
    void Start()
    {
        
        MapCreate();
    }

    public void MapCreate()
    {
        temp_pos.Clear();
        itemList.Clear();
        int itemAmount = 0;
        foreach(Vector3Int pos in maps[BoomberManager.inst.mapIdx].cellBounds.allPositionsWithin)
        {
            //offset 부분
            
            
            // 해당 좌표에 타일이 없으면 넘어간다.
            if(!maps[BoomberManager.inst.mapIdx].HasTile(pos)) continue;
            // 해당 좌표의 타일을 얻는다.
            TileBase tile = maps[BoomberManager.inst.mapIdx].GetTile(pos);
            Vector3 createPos = maps[BoomberManager.inst.mapIdx].CellToWorld(pos)+offset;

            if (tile==respawnTile)
            {
                temp_pos.Add(createPos);
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
            GameObject ob= ObjectPooler.SpawnFromPool(tilePrefabs[idx], createPos, quaternion.identity);
            tileObs.Add(ob);
            if (ob.TryGetComponent(out Brick b))
            {
                b.Idx = itemAmount;
                itemAmount++;
                itemList.Add(0);
            }
        }
        BoomberManager.inst.respawnPos = temp_pos.ToList();
        maps[BoomberManager.inst.mapIdx].gameObject.SetActive(false);
        ItemSetting();
    }

    public void MapDestory()
    {
        foreach (var ob in tileObs)
        {
            ob.SetActive(false);
        }
        tileObs.Clear();
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