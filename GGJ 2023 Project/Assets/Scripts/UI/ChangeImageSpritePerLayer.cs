using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeImageSpritePerLayer : MonoBehaviour
{
    [SerializeField]
    private Sprite[] layerSprites;

    [SerializeField]
    private GameController.TilemapLayer prevLayer;

    private GameController controller;

    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        prevLayer = controller.CurrentLayer;
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (prevLayer != controller.CurrentLayer)
        {
            prevLayer = controller.CurrentLayer;
        }
    }

    void SetImage (GameController.TilemapLayer layer)
    {
        image.sprite = layerSprites[(int)layer];
    }
}
