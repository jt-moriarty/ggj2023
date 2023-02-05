using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LayingPipe : MonoBehaviour
{
    //public TileLayer tileLayer;
    public Tilemap tileMap;
    //public Vector3Int[] placesToGo;
    public Tile[] pipeTile;
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

    void PipeInspection(Vector3Int currentTile)
    {
        List<Vector3Int> placesToGo = new List<Vector3Int>();

        int BinaryIndex = 0;
        if (tileMap.HasTile(new Vector3Int(currentTile.x + 1,currentTile.y,currentTile.z)))
        {
            BinaryIndex++;
            placesToGo.Add(new Vector3Int(currentTile.x + 1, currentTile.y, currentTile.z));
            //placesToGo[count] = new Vector3Int(currentTile.x + 1, currentTile.y, currentTile.z);
            //count++;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x, currentTile.y + 1, currentTile.z)))
        {
            BinaryIndex += 2;
            placesToGo.Add(new Vector3Int(currentTile.x, currentTile.y + 1, currentTile.z));
            //placesToGo[count] = new Vector3Int(currentTile.x, currentTile.y + 1, currentTile.z);
            //count++;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x - 1, currentTile.y,currentTile.z)))
        {
            BinaryIndex += 4;
            placesToGo.Add(new Vector3Int(currentTile.x - 1, currentTile.y, currentTile.z));
            //placesToGo[count] = new Vector3Int(currentTile.x - 1, currentTile.y, currentTile.z);
            //count++;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x, currentTile.y - 1, currentTile.z)))
        {
            BinaryIndex += 8;
            placesToGo.Add(new Vector3Int(currentTile.x, currentTile.y - 1, currentTile.z));
            //placesToGo[count] = new Vector3Int(currentTile.x, currentTile.y - 1, currentTile.z);
            //count++;
        }

        tileMap.SetTile(currentTile, pipeTile[BinaryIndex]);

        foreach (Vector3Int nextTilePos in placesToGo)
        {
            RecursionIsOverRatedAnyways(nextTilePos);
        }
            
       /* while (count > 0)
        {
            RecursionIsOverRatedAnyways(placesToGo[count - 1]);
            count--;
         }*/

    }
    void RecursionIsOverRatedAnyways(Vector3Int currentTile)
    {
        int BinaryIndex = 0;
        if (tileMap.HasTile(new Vector3Int(currentTile.x + 1, currentTile.y, currentTile.z)))
        {
            BinaryIndex++;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x, currentTile.y + 1, currentTile.z)))
        {
            BinaryIndex += 2;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x - 1, currentTile.y, currentTile.z)))
        {
            BinaryIndex += 4;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x, currentTile.y - 1, currentTile.z)))
        {
            BinaryIndex += 8;
        }
        tileMap.SetTile(currentTile, pipeTile[BinaryIndex]);
    }

    public void AddPipe(Vector3Int currentTile)
    {
        PipeInspection(currentTile);
        //tileMap.SetTile(currentTile, pipeTile[BinaryIndex]);
    }
    
    public void AddCore(Vector3Int currentTile)
    {
        //TODO: add from selection of core sprites.
        PipeInspection(currentTile);
       // tileMap.SetTile(currentTile, pipeTile[BinaryIndex]);
    }
}
