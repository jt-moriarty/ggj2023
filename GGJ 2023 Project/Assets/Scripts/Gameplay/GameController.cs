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

    [SerializeField]
    private Tile[] rootTiles;
    
    private List<Image> uiTiles;

    [SerializeField]
    private int selectedTile = 0;

    [SerializeField]
    private Transform uiTileGrid;

    [SerializeField]
    private GameObject energyPrefab;

    private Vector3Int SaveTileIndex(int x, int y, int z)
    {
        return new Vector3Int(x, y, z);
    }

    private void OnFlow(PipeNode<GameResource,Vector3Int> sourceNode, PipeNode<GameResource,Vector3Int> destNode, GameResource res, int amount)
    {
        Debug.Log($"Moving {amount} {res} from {sourceNode} (world position {sourceNode.Info}) to {destNode} (world position {destNode.Info})");
    }

    // Start is called before the first frame update
    void Start()
    {
        //pipePlacer = GetComponent<LayingPipe>();
        rootPipeController = new RootPipeController<GameResource, Vector3Int>(rootLayer.gridSizeX, rootLayer.gridSizeY, 3, OnFlow, SaveTileIndex);
        //rootPipeController.AddCore(3, 3, "starting core");
        AddCore(3, 3);

        int i = 0;
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
        }

        SetSelectedTile(selectedTile);


        StartGame();
    }

    void AddCore(int x, int y)
    {

        pipePlacer.AddCore(new Vector3Int(x,y,0));
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
                pipePlacer.AddPipe(gridPos);
                gridPos += new Vector3Int(3, 3, 0);
                Debug.Log($"Adding root to ({gridPos.x}, {gridPos.y})");
                rootPipeController.AddRoot(gridPos.x,gridPos.y);
            }
            //rootTilemap.SetTile(rootTilemap.WorldToCell(pos), rootTiles[selectedTile]);
        }
        else if (mouse.rightButton.wasPressedThisFrame && CurrentLayer == TilemapLayer.Root)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            pos.z = 0;
            
            Vector3Int idx = rootTilemap.WorldToCell(pos);

            Vector3 gridPos = rootTilemap.CellToWorld(idx);
            Debug.Log($"pos: {pos}, int: {idx}, gridPos: {gridPos}");
            //gridPos.x -= 0.5f;
            gridPos.y += 0.25f;
            GameObject.Instantiate(energyPrefab, gridPos, Quaternion.identity);

            idx += new Vector3Int(3, 3, 0);
            Debug.Log($"Adding energy to ({idx.x},{idx.y},1)");
            rootPipeController.AddResource(idx.x, idx.y, 1, GameResource.energy, 5);
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            Debug.Log($"performing flow");
            rootPipeController.DoFlows();
            int gain = rootPipeController.RemoveResource(6, 6, 1, GameResource.energy, 5);
            Debug.Log($"Gained {gain} energy");
            Energy += gain;
        }

        //TODO: right click to place new mushroom + core on the root layer.

        SetSelectedTile(selectedTile);

        Energy -= energyDecay * Time.deltaTime;

        energyText.text = $"Energy: {Energy.ToString(".02")}";
        if (Energy <= 0) {
            // Game over
            // TODO: more elaborate sequence or animation, for now just scene change.
            SceneManager.LoadScene("GameOverScene");
        }
    }

    void SetSelectedTile (int newIndex)
    {
        int i = 0;
        foreach (Image uiImage in uiTiles)
        {
            Color tileColor = uiImage.color;
            tileColor.a = i == newIndex ? 1.0f : 0.5f;
            uiImage.color = tileColor;
            i++;
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
        SetActiveLayer(CurrentLayer);
    }
}
