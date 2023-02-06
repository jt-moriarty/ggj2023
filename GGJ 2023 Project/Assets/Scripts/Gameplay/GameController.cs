using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static LayingPipe;

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

    // BALANCE
    public double startingEnergy = 100;
    public double energyDecay = 0.2;

    public int coreCost = 10;
    public int rootCost = 5;
    public int resourceAmount = 5;
    public int resourceFlowAmount = 5;

    public float resourceAddPeriod = 5f;
    public int maxFlowPerTimestep = 3;

    public enum TilemapLayer { Surface = 2, Root = 1, Base = 0 }

    public TilemapLayer startingLayer = TilemapLayer.Surface;

    public TilemapLayer CurrentLayer { get; set; }

    [SerializeField]
    private Tile[] smallHealthyMushrooms;

    [SerializeField]
    private Tile[] smallUnhealthyMushrooms;

    [SerializeField]
    private Tile[] healthyMushrooms;

    [SerializeField]
    private Tile[] unhealthyMushrooms;

    [SerializeField]
    private Tilemap[] surfaceTilemaps;

    [SerializeField]
    private Tilemap[] rootTilemaps;

    [SerializeField]
    private Tilemap[] baseTilemaps;

    [SerializeField]
    private Tilemap mushroomTilemap;

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

    private Dictionary<PipeNode<GameResource, Vector3Int>, int> timeWhileZeroEnergy = new Dictionary<PipeNode<GameResource, Vector3Int>, int>();

    private Vector3Int SaveTileIndex(int x, int y, int z)
    {
        return new Vector3Int(x, y, z);
    }

    private void OnFlow(RootPipeController<GameResource,Vector3Int>.GridLocation sourceLoc,
        RootPipeController<GameResource, Vector3Int>.GridLocation destLoc, GameResource res, int amount)
    {
        PipeNode<GameResource, Vector3Int> sourceNode = sourceLoc.pipe;
        PipeNode<GameResource, Vector3Int> destNode = destLoc.pipe;

        Debug.Log($"Moving {amount} {res} from {sourceNode} (world position {sourceNode.Info})" +
            $" to {destNode} (world position {destNode.Info})");

        int sourceX = sourceNode.Info.x;
        int sourceY = sourceNode.Info.y;
        int sourceZ = sourceNode.Info.z;

        int destX = destNode.Info.x;
        int destY = destNode.Info.y;
        int destZ = destNode.Info.z;

        GameObject resToMove = rootPipeController.GetResourceObj(sourceX, sourceY, sourceZ, res);

        Vector3Int mapPosSource = sourceNode.Info - new Vector3Int(3, 3, 1);
        Vector3Int mapPosDest = destNode.Info - new Vector3Int(3, 3, 1);

        pipePlacer.UpdateNode(mapPosSource, GetHealth, IsCore, MushroomTypeGetter);

        pipePlacer.UpdateNode(mapPosDest, GetHealth, IsCore, MushroomTypeGetter);

        Vector3 startPos = rootTilemap.CellToWorld(mapPosSource) + Vector3.up * 0.25f;
        Vector3 endPos = rootTilemap.CellToWorld(mapPosDest) + Vector3.up * 0.25f;
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
            if (destLoc.pipe.isCore)
            {
                GainEnergy(destLoc.pipe.Info);
                GameObject.DestroyImmediate(resToMove);
                return;
            }

            if (rootPipeController.GetResourceObj(destX, destY, destZ, res) != null
            || destLoc.pipe.GetResource(res) == 0)
            {
                GameObject.DestroyImmediate(resToMove);
            } 
            else
            {
                rootPipeController.SetResourceObj(destX, destY, destZ, res, resToMove);
            }
        }));
    }

    private void Awake()
    {
        pipePlacer.SetMushroomTiles(mushroomTilemap, smallHealthyMushrooms, smallUnhealthyMushrooms,
            healthyMushrooms, unhealthyMushrooms);
    }

    // Start is called before the first frame update
    void Start()
    {
        //pipePlacer = GetComponent<LayingPipe>();
        rootPipeController = new RootPipeController<GameResource, Vector3Int>(rootLayer.gridSizeX, rootLayer.gridSizeY, maxFlowPerTimestep,
            5, 10, OnFlow, SaveTileIndex);
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
        resourceCoro = StartCoroutine(ResourcePlaceCoro(resourceAddPeriod));
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
        int logicalX = x + 3;
        int logicalY = y + 3;
        Debug.Log($"Adding core to ({logicalX},{logicalY})");
        rootPipeController.AddCore(logicalX, logicalY, "starting core");
        rootPipeController.SetMushroomType(logicalX, logicalY, Random.Range(0, smallHealthyMushrooms.Length));

        pipePlacer.UpdateNode(new Vector3Int(x, y, 0), GetHealth, IsCore, MushroomTypeGetter);
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

                    Debug.Log($"Adding root to ({logicalPos.x}, {logicalPos.y})");
                    rootPipeController.AddRoot(logicalPos.x, logicalPos.y);
                    
                    pipePlacer.UpdateNode(gridPos, GetHealth, IsCore, MushroomTypeGetter);
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
                    Debug.Log($"Adding core to ({logicalPos.x}, {logicalPos.y})");
                    rootPipeController.AddCore(logicalPos.x, logicalPos.y, "new core");
                    rootPipeController.SetMushroomType(logicalPos.x, logicalPos.y, Random.Range(0, smallHealthyMushrooms.Length));

                    pipePlacer.UpdateNode(gridPos, GetHealth, IsCore, MushroomTypeGetter);
                    int res = rootPipeController.GetResource(logicalPos.x, logicalPos.y, 1, GameResource.energy);
                    Energy += res;
                    rootPipeController.RemoveResource(logicalPos.x, logicalPos.y, 1, GameResource.energy,res);
                    GameObject resObj = rootPipeController.GetResourceObj(logicalPos.x, logicalPos.y, 1, GameResource.energy);
                    if (resObj != null)
                    {
                        GameObject.DestroyImmediate(resObj);
                    }
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

    LayingPipe.HealthState GetHealth(int x, int y)
    {
        x += 3;
        y += 3;
        if (rootPipeController.IsNodeHealthy(x,y))
        {
            return HealthState.HEALTHY;
        }
        if (rootPipeController.IsOccupied(x,y))
        {
            return HealthState.WEAK;
        }
        return HealthState.DEAD;
    }
    
    bool IsCore(int x, int y)
    {
        return rootPipeController.IsCore(x+3, y+3);
    }

    int MushroomTypeGetter(int x, int y)
    {
        return rootPipeController.GetMushroomType(x+3, y+3);
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
        rootPipeController.AddResource(idx.x, idx.y, 1, res, resourceAmount);
        rootPipeController.SetResourceObj(idx.x, idx.y, 1, res, resObj);
    }

    void EndGame()
    {
        StopCoroutine(flowCoro);
        StopCoroutine(resourceCoro);
        // Game over
        // TODO: more elaborate sequence or animation, for now just scene change.
        SceneManager.LoadScene("GameOverScene");
    }

    void DoFlow()
    {
        Debug.Log($"performing flow");
        rootPipeController.DoFlows();

        rootPipeController.GetNodeHealthChanges(out IEnumerable<Vector3Int> newlyDeadNodes,
            out IEnumerable<Vector3Int> newlyWeakenedNodes);
        
        foreach (Vector3Int node in newlyDeadNodes)
        {
            rootPipeController.RemoveNode(node.x,node.y);
            rootPipeController.SetMushroomType(node.x, node.y, -1);
        }

        foreach (Vector3Int node in newlyDeadNodes)
        {
            pipePlacer.UpdateNode(new Vector3Int(node.x - 3, node.y - 3, 0), GetHealth, IsCore, MushroomTypeGetter);
        }

        foreach (Vector3Int node in newlyWeakenedNodes)
        {
            pipePlacer.UpdateNode(new Vector3Int(node.x - 3, node.y - 3, 0), GetHealth, IsCore, MushroomTypeGetter);
        }
    }

    void GainEnergy(Vector3Int info)
    {
        int gain = rootPipeController.RemoveResource(info.x, info.y, 1, GameResource.energy, resourceAmount);
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
