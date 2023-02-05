using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    enum GameResource
    {
        energy
    }

    [SerializeField]
    private TextMeshProUGUI energyText;

    [SerializeField]
    private LayingPipe pipePlacer;

    private RootPipeController<GameResource, Vector3Int> rootPipeController;

    // Nutrients + Water from the soil + sunlight = energy in different amounts.
    public double Energy { get; set; }
    public double startingEnergy = 100;
    public double energyDecay = 0.1;

    public double coreCost = 10;
    public double rootCost = 5;

    public enum TilemapLayer { Surface = 2, Root = 1, Base = 0 }

    public TilemapLayer startingLayer = TilemapLayer.Surface;

    public TilemapLayer CurrentLayer { get; set; }

    [SerializeField]
    private Tilemap[] surfaceTilemaps;

    [SerializeField]
    private Tilemap[] rootTilemaps;

    [SerializeField]
    private Tilemap[] baseTilemaps;

    [SerializeField]
    private Tilemap rootTilemap;

    [SerializeField]
    private TileLayer rootLayer;

    //[SerializeField]
    //private Tile[] rootTiles;
    
    private List<Image> uiTiles;

    [SerializeField]
    private int selectedTile = 0;

    //[SerializeField]
    //private Transform uiTileGrid;

    [SerializeField]
    private GameObject energyPrefab;
    private Coroutine flowCoro;
    private Coroutine resourceCoro;

    private Vector3Int SaveTileIndex(int x, int y, int z)
    {
        return new Vector3Int(x, y, z);
    }

    private void OnFlow(PipeNode<GameResource,Vector3Int> sourceNode, PipeNode<GameResource,Vector3Int> destNode, GameResource res, int amount)
    {
        Debug.Log($"Moving {amount} {res} from {sourceNode} (world position {sourceNode.Info}) to {destNode} (world position {destNode.Info})");

        int sourceX = sourceNode.Info.x;
        int sourceY = sourceNode.Info.y;
        int sourceZ = sourceNode.Info.z;

        int destX = destNode.Info.x;
        int destY = destNode.Info.y;
        int destZ = destNode.Info.z;

        GameObject resToMove = rootPipeController.GetResourceObj(sourceX, sourceY, sourceZ, res);

        Vector3 startPos = rootTilemap.CellToWorld((Vector3Int)sourceNode.Info - new Vector3Int(3, 3, 0));
        Vector3 endPos = rootTilemap.CellToWorld((Vector3Int)destNode.Info - new Vector3Int(3, 3, 0));
        if (sourceNode.GetResource(res) > 0 || resToMove == null)
        {
            resToMove = GameObject.Instantiate(energyPrefab, startPos, Quaternion.identity);
        }
        else 
        {
            rootPipeController.SetResourceObj(sourceX, sourceY, sourceZ, res, null);
        }

        StartCoroutine(resToMove.GetComponent<TweenToTarget>().TweenPosition(startPos, endPos, 1f, () =>
        {
            if (destNode.isCore)
            {
                GainEnergy(destNode.Info);
            }

            if (rootPipeController.GetResourceObj(destX, destY, destZ, res) != null
            || destNode.GetResource(res) == 0)
            {
                GameObject.DestroyImmediate(resToMove);
            } 
            else
            {
                rootPipeController.SetResourceObj(destX, destY, destZ, res, resToMove);
            }
        }));
    }

    // Start is called before the first frame update
    void Start()
    {
        //pipePlacer = GetComponent<LayingPipe>();
        rootPipeController = new RootPipeController<GameResource, Vector3Int>(rootLayer.gridSizeX, rootLayer.gridSizeY, 3, OnFlow, SaveTileIndex);
        //rootPipeController.AddCore(3, 3, "starting core");
        AddCore(3, 3);

        /*int i = 0;
        uiTiles = new List<Image>();
        foreach (Tile tile in rootTiles)
        {
            GameObject uiTile = new GameObject("UI Tile");
            uiTile.transform.parent = uiTileGrid;
            uiTile.transform.localScale = Vector3.one;

            Image uiImage = uiTile.AddComponent<Image>();
            uiImage.sprite = tile.sprite;
            uiTiles.Add(uiImage);
            i++;
        }*/

        //SetSelectedTile(selectedTile);

        StartGame();
    }

    void StartFlow()
    {
        flowCoro = StartCoroutine(FlowCoro(2f));
        resourceCoro = StartCoroutine(ResourcePlaceCoro(5f));
    }

    bool IsFlowContinuing()
    {
        return Energy > 0;
    }

    IEnumerator FlowCoro(float period)
    {
        var pause = new WaitForSeconds(period);
        yield return pause;
        while (IsFlowContinuing())
        {
            DoFlow();
            yield return pause;
        }
    }

    IEnumerator ResourcePlaceCoro(float period)
    {
        var pause = new WaitForSeconds(period);

        PlaceRandomResource();

        yield return pause;
        while (IsFlowContinuing())
        {
            PlaceRandomResource();
            yield return pause;
        }
    }

    void AddCore(int x, int y)
    {
        pipePlacer.AddCore(new Vector3Int(x, y, 0), IsCore);
        x += 3;
        y += 3;
        Debug.Log($"Adding core to ({x},{y})");
        rootPipeController.AddCore(x, y, "starting core");
    }

    // Update is called once per frame
    void Update()
    {
        // Update state based on player input.

        // Change the active later.
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        // TODO: mouse wheel as an option?
        if (keyboard.upArrowKey.wasPressedThisFrame && CurrentLayer < TilemapLayer.Surface)
        {
            SetActiveLayer(++CurrentLayer);
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame && CurrentLayer > TilemapLayer.Base)
        {
            SetActiveLayer(--CurrentLayer);
        }

        if (mouse.leftButton.wasPressedThisFrame && CurrentLayer == TilemapLayer.Root)
        {
            //Debug.Log("place TILE");
            Vector3 pos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            //Debug.Log(pos);
            //Debug.Log(selectedTile);
            pos.z = 0;
            Vector3Int gridPos = rootTilemap.WorldToCell(pos);

            if (rootLayer.InGridBounds(gridPos))
            {
                Vector3Int logicalPos = gridPos + new Vector3Int(3, 3, 0);
                if (!rootPipeController.IsOccupied(logicalPos.x, logicalPos.y))
                {
                    Energy -= rootCost;
                    pipePlacer.AddPipe(gridPos, IsCore);
                    Debug.Log($"Adding root to ({logicalPos.x}, {logicalPos.y})");
                    rootPipeController.AddRoot(logicalPos.x, logicalPos.y);
                }
            }
            //rootTilemap.SetTile(rootTilemap.WorldToCell(pos), rootTiles[selectedTile]);
        }
        else if (mouse.rightButton.wasPressedThisFrame && CurrentLayer == TilemapLayer.Root)
        {
            //Debug.Log("place TILE");
            Vector3 pos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            //Debug.Log(pos);
            //Debug.Log(selectedTile);
            pos.z = 0;
            Vector3Int gridPos = rootTilemap.WorldToCell(pos);

            if (rootLayer.InGridBounds(gridPos))
            {
                Vector3Int logicalPos = gridPos + new Vector3Int(3, 3, 0);
                if (!rootPipeController.IsCore(logicalPos.x, logicalPos.y))
                {
                    Energy -= coreCost;
                    //TODO: if this was already roots it should cost less.
                    pipePlacer.AddCore(gridPos, IsCore);
                    Debug.Log($"Adding core to ({logicalPos.x}, {logicalPos.y})");
                    rootPipeController.AddCore(logicalPos.x, logicalPos.y, "new core");
                }
            }
        }

        //TODO: right click to place new mushroom + core on the root layer.

        //SetSelectedTile(selectedTile);

        Energy -= energyDecay * Time.deltaTime;

        energyText.text = $"{((int)Energy).ToString()}";
        if (Energy <= 0) {
            EndGame();
        }
    }
    
    bool IsCore(int x, int y)
    {
        return rootPipeController.IsCore(x+3, y+3);
    }

    void PlaceRandomResource()
    {
        var resources = System.Enum.GetValues(typeof(GameResource));
        var res = (GameResource)resources.GetValue(Random.Range(0,resources.Length));

        List<Vector2Int> locs = new List<Vector2Int>();

        for (int y = 0; y < rootLayer.gridSizeY; y++)
        {
            for (int x = 0; x < rootLayer.gridSizeX; x++)
            {
                if (!rootPipeController.IsOccupied(x,y))
                {
                    locs.Add(new Vector2Int(x-3,y-3));
                }
            }
        }
        if (locs.Count == 0)
        {
            Debug.LogError("Can't place any more resources. Consider autophagy.");
            return;
        }

        Vector2Int idx = locs[Random.Range(0,locs.Count)];

        Vector3 gridPos = rootTilemap.CellToWorld((Vector3Int)idx);
        gridPos.y += 0.25f;
        GameObject resObj = GameObject.Instantiate(energyPrefab, gridPos, Quaternion.identity);

        idx += new Vector2Int(3, 3);
        Debug.Log($"Adding {res} to ({idx.x},{idx.y},1)");
        rootPipeController.AddResource(idx.x, idx.y, 1, res, 5);
        rootPipeController.SetResourceObj(idx.x, idx.y, 1, res, resObj);
    }

    void EndGame()
    {
        StopCoroutine(flowCoro);
        // Game over
        // TODO: more elaborate sequence or animation, for now just scene change.
        SceneManager.LoadScene("GameOverScene");
    }

    void DoFlow()
    {
        Debug.Log($"performing flow");
        rootPipeController.DoFlows();
    }

    void GainEnergy(Vector3Int info)
    {
        int gain = rootPipeController.RemoveResource(info.x, info.y, 1, GameResource.energy, 5);
        Debug.Log($"Gained {gain} energy");
        Energy += gain;

        if (rootPipeController.GetResource(info.x,info.y,1,GameResource.energy) == 0)
        {
            rootPipeController.SetResourceObj(info.x, info.y, 1, GameResource.energy, null);
        }
    }

    void SetActiveLayer (TilemapLayer newLayer)
    {
        for (int i = 0; i < surfaceTilemaps.Length; i++)
        {
            Color newColor = surfaceTilemaps[i].color;
            newColor.a = newLayer == TilemapLayer.Surface ? 1f : 0f;
            surfaceTilemaps[i].color = newColor;
        }

        for (int i = 0; i < rootTilemaps.Length; i++)
        {
            Color newColor = rootTilemaps[i].color;
            newColor.a = newLayer == TilemapLayer.Root ? 1f : 0f;
            rootTilemaps[i].color = newColor;
        }

        for (int i = 0; i < baseTilemaps.Length; i++)
        {
            Color newColor = baseTilemaps[i].color;
            newColor.a = newLayer == TilemapLayer.Base ? 1f : 0f;
            baseTilemaps[i].color = newColor;
        }
    }

    void StartGame() {
        Energy = startingEnergy;
        CurrentLayer = startingLayer;
        StartFlow();
        SetActiveLayer(CurrentLayer);
    }
}
