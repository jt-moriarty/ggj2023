using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Tilemaps;
//using static UnityEditor.PlayerSettings;

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
    public delegate int MushroomTypeFunc(int x, int y);

    //public TileLayer tileLayer;
    public Tilemap tileMap;
    public Tilemap mushroomMap;
    //public Vector3Int[] placesToGo;
    public Tile[] pipeTile;

    [SerializeField]
    public Tile[] weakPipeTiles;
    public Tile[] coreTiles;

    [SerializeField]
    private Tile[] weakCoreTiles;


    [SerializeField]
    private Tile[] smallHealthyMushrooms;

    [SerializeField]
    private Tile[] smallUnhealthyMushrooms;

    [SerializeField]
    private Tile[] healthyMushrooms;

    [SerializeField]
    private Tile[] unhealthyMushrooms;
    //public Tile deadTile;
    //int BinaryIndex;
    //int count;


    void PipeInspection(Vector3Int currentTile, GetHealth getHealthFunc, IsCore isCoreFunc,
        MushroomTypeFunc mushroomTypeFunc, bool recur)
    {
        bool isCore = isCoreFunc(currentTile.x,currentTile.y);
        List<Vector3Int> placesToGo = new List<Vector3Int>();
        HealthState health = getHealthFunc(currentTile.x, currentTile.y);

        int BinaryIndex = 0;
        if (tileMap.HasTile(new Vector3Int(currentTile.x + 1,currentTile.y,currentTile.z)))
        {
            BinaryIndex++;
            Vector3Int pos = new Vector3Int(currentTile.x + 1, currentTile.y, currentTile.z);
            placesToGo.Add(pos);
            //placesToGo[count] = new Vector3Int(currentTile.x + 1, currentTile.y, currentTile.z);
            //count++;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x, currentTile.y + 1, currentTile.z)))
        {
            BinaryIndex += 2;
            Vector3Int pos = new Vector3Int(currentTile.x, currentTile.y + 1, currentTile.z);
            placesToGo.Add(pos);
            //placesToGo[count] = new Vector3Int(currentTile.x, currentTile.y + 1, currentTile.z);
            //count++;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x - 1, currentTile.y,currentTile.z)))
        {
            BinaryIndex += 4;
            Vector3Int pos = new Vector3Int(currentTile.x - 1, currentTile.y, currentTile.z);
            placesToGo.Add(pos);
            //placesToGo[count] = new Vector3Int(currentTile.x - 1, currentTile.y, currentTile.z);
            //count++;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x, currentTile.y - 1, currentTile.z)))
        {
            BinaryIndex += 8;
            Vector3Int pos = new Vector3Int(currentTile.x, currentTile.y - 1, currentTile.z);
            placesToGo.Add(pos);
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

        if ((isCore|| health == HealthState.DEAD) && recur)
        {
            Vector3Int mushroomIdx = currentTile + new Vector3Int(1, 1);
            int tileIdx = mushroomTypeFunc(currentTile.x, currentTile.y);
            switch (health)
            {
                case HealthState.HEALTHY:
                    mushroomMap.SetTile(mushroomIdx, smallHealthyMushrooms[tileIdx]);
                    break;

                case HealthState.WEAK:
                    mushroomMap.SetTile(mushroomIdx, smallUnhealthyMushrooms[tileIdx]);
                    break;

                case HealthState.DEAD:
                    mushroomMap.SetTile(mushroomIdx, null);
                    break;
            }
        }

        if (!recur)
        {
            return;
        }

        foreach (var nextTilePos in placesToGo)
        {
            PipeInspection(nextTilePos, getHealthFunc, isCoreFunc, mushroomTypeFunc, false);
        }
    }
    
    public void UpdateNode(Vector3Int currentTile, GetHealth getHealthFunc, IsCore isCoreFunc, MushroomTypeFunc mushroomTypeFunc)
    {
        PipeInspection(currentTile, getHealthFunc, isCoreFunc, mushroomTypeFunc, true);
    }

    public void SetMushroomTiles(Tilemap mushroomMap, Tile[] smallHealthyMushrooms, Tile[] smallUnhealthyMushrooms,
        Tile[] healthyMushrooms, Tile[] unhealthyMushrooms)
    {
        this.mushroomMap = mushroomMap;
        this.smallHealthyMushrooms = smallHealthyMushrooms;
        this.smallUnhealthyMushrooms = smallUnhealthyMushrooms;
        this.healthyMushrooms = healthyMushrooms;
        this.unhealthyMushrooms = unhealthyMushrooms;
    }
}
