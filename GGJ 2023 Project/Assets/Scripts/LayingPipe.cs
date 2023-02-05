using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

public class LayingPipe : MonoBehaviour
{
    public enum HealthState
    {
        HEALTHY,
        WEAK,
        DEAD
    }

    public delegate bool IsCore(int x, int y);
    public delegate HealthState GetHealth(int x, int y);


    //public TileLayer tileLayer;
    public Tilemap tileMap;
    //public Vector3Int[] placesToGo;
    public Tile[] pipeTile;

    [SerializeField]
    public Tile[] weakPipeTiles;
    public Tile[] coreTiles;

    [SerializeField]
    private Tile[] weakCoreTiles;
    //public Tile deadTile;
    //int BinaryIndex;
    //int count;

    // Start is called before the first frame update
    void Start()
    {
        //placesToGo = new Vector3Int[4];
    }

    // Update is called once per frame
    void Update()
    {
    }

    void PipeInspection(Vector3Int currentTile, bool isCore, GetHealth getHealthFunc, IsCore isCoreFunc, bool recur)
    {
        List<(Vector3Int,bool)> placesToGo = new List<(Vector3Int,bool)>();
        HealthState health = getHealthFunc(currentTile.x, currentTile.y);

        int BinaryIndex = 0;
        if (tileMap.HasTile(new Vector3Int(currentTile.x + 1,currentTile.y,currentTile.z)))
        {
            BinaryIndex++;
            Vector3Int pos = new Vector3Int(currentTile.x + 1, currentTile.y, currentTile.z);
            placesToGo.Add((pos,isCoreFunc(pos.x, pos.y)));
            //placesToGo[count] = new Vector3Int(currentTile.x + 1, currentTile.y, currentTile.z);
            //count++;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x, currentTile.y + 1, currentTile.z)))
        {
            BinaryIndex += 2;
            Vector3Int pos = new Vector3Int(currentTile.x, currentTile.y + 1, currentTile.z);
            placesToGo.Add((pos, isCoreFunc(pos.x, pos.y)));
            //placesToGo[count] = new Vector3Int(currentTile.x, currentTile.y + 1, currentTile.z);
            //count++;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x - 1, currentTile.y,currentTile.z)))
        {
            BinaryIndex += 4;
            Vector3Int pos = new Vector3Int(currentTile.x - 1, currentTile.y, currentTile.z);
            placesToGo.Add((pos, isCoreFunc(pos.x, pos.y)));
            //placesToGo[count] = new Vector3Int(currentTile.x - 1, currentTile.y, currentTile.z);
            //count++;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x, currentTile.y - 1, currentTile.z)))
        {
            BinaryIndex += 8;
            Vector3Int pos = new Vector3Int(currentTile.x, currentTile.y - 1, currentTile.z);
            placesToGo.Add((pos, isCoreFunc(pos.x, pos.y)));
            //placesToGo[count] = new Vector3Int(currentTile.x, currentTile.y - 1, currentTile.z);
            //count++;
        }

        Tile[] tileList;
        switch (health)
        {
            case HealthState.HEALTHY:
                tileList = isCore ? coreTiles : pipeTile;
                tileMap.SetTile(currentTile, tileList[BinaryIndex]);
                break;

            case HealthState.WEAK:
                tileList = isCore ? weakCoreTiles : weakPipeTiles;
                tileMap.SetTile(currentTile, tileList[BinaryIndex]);
                break;

            case HealthState.DEAD:
                tileMap.SetTile(currentTile, null);
                break;
        }

        if (!recur)
        {
            return;
        }

        foreach ((Vector3Int,bool) nextTilePos in placesToGo)
        {
            PipeInspection(nextTilePos.Item1, nextTilePos.Item2, getHealthFunc, isCoreFunc, false);
        }
    }

    public void AddPipe(Vector3Int currentTile, GetHealth getHealthFunc, IsCore isCoreFunc)
    {
        PipeInspection(currentTile, false, getHealthFunc, isCoreFunc, true);
        //tileMap.SetTile(currentTile, pipeTile[BinaryIndex]);
    }
    
    public void AddCore(Vector3Int currentTile, GetHealth getHealthFunc, IsCore isCoreFunc)
    {
        //TODO: add from selection of core sprites.
        PipeInspection(currentTile, true, getHealthFunc, isCoreFunc, true);
       // tileMap.SetTile(currentTile, pipeTile[BinaryIndex]);
    }

    public void RemoveNode(Vector3Int currentTile, GetHealth getHealthFunc, IsCore isCoreFunc)
    {
        PipeInspection(currentTile, false, getHealthFunc, isCoreFunc, true);
    }
}
