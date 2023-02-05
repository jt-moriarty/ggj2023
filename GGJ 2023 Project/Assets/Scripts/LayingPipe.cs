using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LayingPipe : MonoBehaviour
{
    public delegate bool IsRoot(int x, int y);

    //public TileLayer tileLayer;
    public Tilemap tileMap;
    //public Vector3Int[] placesToGo;
    public Tile[] pipeTile;
    public Tile[] coreTiles;
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

    void PipeInspection(Vector3Int currentTile, bool isCore, IsRoot isCoreFunc, bool recur)
    {
        List<(Vector3Int,bool)> placesToGo = new List<(Vector3Int,bool)>();

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

        Tile[] tileList = isCore ? coreTiles : pipeTile;

        tileMap.SetTile(currentTile, tileList[BinaryIndex]);

        if (!recur)
        {
            return;
        }

        foreach ((Vector3Int,bool) nextTilePos in placesToGo)
        {
            PipeInspection(nextTilePos.Item1, nextTilePos.Item2, isCoreFunc, false);
        }
    }

    public void AddPipe(Vector3Int currentTile, IsRoot isCoreFunc)
    {
        PipeInspection(currentTile, false, isCoreFunc, true);
        //tileMap.SetTile(currentTile, pipeTile[BinaryIndex]);
    }
    
    public void AddCore(Vector3Int currentTile, IsRoot isCoreFunc)
    {
        //TODO: add from selection of core sprites.
        PipeInspection(currentTile, true, isCoreFunc, true);
       // tileMap.SetTile(currentTile, pipeTile[BinaryIndex]);
    }
}
