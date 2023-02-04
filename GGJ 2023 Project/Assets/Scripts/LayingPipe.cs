using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LayingPipe : MonoBehaviour
{
    //public TileLayer tileLayer;
    public Tilemap tileMap;
    //public Vector3Int currentTile;
    public Tile[] pipeTile;
    int BinaryIndex;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void PipeInspection(Vector3Int currentTile)
    {
        BinaryIndex = 0;
        if (tileMap.HasTile(new Vector3Int(currentTile.x + 1,currentTile.y,currentTile.z)))
        {
            BinaryIndex++;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x, currentTile.y + 1, currentTile.z)))
        {
            BinaryIndex += 2;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x - 1, currentTile.y,currentTile.z)))
        {
            BinaryIndex += 4;
        }
        if (tileMap.HasTile(new Vector3Int(currentTile.x, currentTile.y - 1, currentTile.z)))
        {
            BinaryIndex += 8;
        }
    }

    public void AddPipe(Vector3Int currentTile)
    {
        PipeInspection(currentTile);
        tileMap.SetTile(currentTile, pipeTile[BinaryIndex]);
    }
}
