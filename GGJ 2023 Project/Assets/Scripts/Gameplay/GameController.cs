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
    [SerializeField]
    private TextMeshProUGUI energyText;

    [SerializeField]
    private LayingPipe pipePlacer;

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

    // Start is called before the first frame update
    void Start()
    {
        pipePlacer = GetComponent<LayingPipe>();
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

        /*if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            selectedTile--;
            if (selectedTile < 0)
                selectedTile = uiTiles.Count - 1;
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            selectedTile++;
            if (selectedTile > uiTiles.Count - 1)
                selectedTile = 0;
        }*/

        if (mouse.leftButton.wasPressedThisFrame && CurrentLayer == TilemapLayer.Root)
        {
            //Debug.Log("place TILE");
            Vector3 pos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            //Debug.Log(pos);
            //Debug.Log(selectedTile);
            pos.z = 0;

            Vector3Int gridPos = rootTilemap.WorldToCell(pos);

            if (rootLayer.InGridBounds(gridPos))
                pipePlacer.AddPipe(gridPos);
            //rootTilemap.SetTile(rootTilemap.WorldToCell(pos), rootTiles[selectedTile]);
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
