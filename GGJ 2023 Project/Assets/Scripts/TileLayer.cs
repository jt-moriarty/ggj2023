using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileLayer : MonoBehaviour
{
    public Tile tileImage;
    public Tilemap tileMap;
    Vector3Int tilePos;
    public int gridSizeX;
    public int gridSizeY;
    public int offsetX;
    public int offsetY;

    // Start is called before the first frame update
    void Start()
    {
        //mappyMcMap.SetTile(new Vector3Int(0,0,0), theOne);
        LeTiles(gridSizeX,gridSizeY);
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LeTiles(int xAxis, int yAxis)
    {
        for (int y = -offsetY; y < yAxis -offsetY; y++)
        {
            tilePos = new Vector3Int(tilePos.x, y, tilePos.z);
            for (int x = -offsetX; x < xAxis - offsetX; x++)
            {
                tilePos = new Vector3Int(x, tilePos.y, tilePos.z);
                tileMap.SetTile(new Vector3Int(tilePos.x, tilePos.y, tilePos.z), tileImage);
            }
        }
    }

    public bool InGridBounds(Vector3Int gridPos)
    {
        //TODO: have this check playable area bounds for the layer.
        return true;
    }
}
