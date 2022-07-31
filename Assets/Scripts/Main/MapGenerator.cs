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
    public List<GameObject> brickObs;

    public GameObject groundTiles;
    public GameObject outWallTiles;
    
    public List<int> randomPos;
    public List<int> randomPosTemp;
    // Start is called before the first frame update
    void Start()
    {
        
        //MapCreate();
    }

    public void StartFunc()
    {
        BoomberManager.inst.itemDictionary.Clear();
        BoomberManager.inst.IsDead = false;
        BoomberManager.inst.moveMent.direction = Vector2.zero;
        GameManager.inst.roomOb.SetActive(false);
        GameManager.inst.playOb.SetActive(true);
        foreach (var tile in tileObs)
        {
            tile.SetActive(true);
        }
        groundTiles.SetActive(true);
        outWallTiles.SetActive(true);
        for (int i = 0; i < NetworkManager.inst.players.Length; i++)
        {
            if (NetworkManager.inst.players[i] != null)
            {
                NetworkManager.inst.players[i].SetActive(true);
            }
        }
        BoomberManager.inst.IsStart = true;
        GameManager.inst.isPlaying = true;
    }

    public void EndFunc()
    {
        BoomberManager.inst.StopAllCoroutines();
        BoomberManager.inst.timeText.text = "GameOver";
        MapDestory();
        groundTiles.SetActive(false);
        outWallTiles.SetActive(false);
        for (int i = 0; i < NetworkManager.inst.players.Length; i++)
        {
            if (NetworkManager.inst.players[i] != null)
            {
                Destroy(NetworkManager.inst.players[i]);
            }
            NetworkManager.inst.players[i] = null;
        }
        BoomberManager.inst.IsStart = false;
        GameManager.inst.roomOb.SetActive(true);
        GameManager.inst.playOb.SetActive(false);
        GameManager.inst.isPlaying = false;
        BoomberManager.inst.gameWait = 0;
        if (GameManager.inst.RoomHost)
        {
            SocketManager.inst.socket.Emit("PlayEnd",GameManager.inst.room);
        }

        BoomberManager.inst.player = null;
    }

    public void CharacterRandomFunc()
    {
        randomPos.Clear();
        randomPosTemp.Clear();

        for (int i = 0; i < temp_pos.Count; i++)
        {
            randomPosTemp.Add(i);
        }

        while (randomPosTemp.Count!=0)
        {
            int ran = Random.Range(0, randomPosTemp.Count);
            randomPos.Add(randomPosTemp[ran]);
            randomPosTemp.RemoveAt(ran);
        }
    }

    public void MapCreate()
    {
        temp_pos.Clear();
        itemList.Clear();
        int itemAmount = 0;
        foreach(Vector3Int pos in maps[BoomberManager.inst.mapIdxGo].cellBounds.allPositionsWithin)
        {
            //offset 부분
            
            
            // 해당 좌표에 타일이 없으면 넘어간다.
            if(!maps[BoomberManager.inst.mapIdxGo].HasTile(pos)) continue;
            // 해당 좌표의 타일을 얻는다.
            TileBase tile = maps[BoomberManager.inst.mapIdxGo].GetTile(pos);
            Vector3 createPos = maps[BoomberManager.inst.mapIdxGo].CellToWorld(pos)+offset;

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
            ob.SetActive(false);
            if (ob.TryGetComponent(out Brick b))
            {
                b.Idx = itemAmount;
                itemAmount++;
                itemList.Add(0);
                brickObs.Add(ob);
            }
        }
        BoomberManager.inst.respawnPos = temp_pos.ToList();
        //maps[GameManager.inst.mapIdx].gameObject.SetActive(false);
    }

    public void MapDestory()
    {
        brickObs.Clear();
        foreach (var ob in tileObs)
        {
            ob.SetActive(false);
        }

        foreach (var item in GameObject.FindGameObjectsWithTag("Item"))
        {
            item.SetActive(false);
        }
        tileObs.Clear();
    }


    public void ItemSetting()
    {
        itemListTemp.Clear();
        int powerCnt = mapInfos[BoomberManager.inst.mapIdxGo].itemPowerCnt;
        int bombCnt = mapInfos[BoomberManager.inst.mapIdxGo].itemBombCnt;
        int speedCnt = mapInfos[BoomberManager.inst.mapIdxGo].itemSpeedCnt;

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