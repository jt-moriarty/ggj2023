using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeImageSpritePerLayer : MonoBehaviour
{
    [SerializeField]
    private Sprite[] layerSprites;

    [SerializeField]
    private GameController.TilemapLayer prevLayer;

    private GameController controller;

    //private Image

    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        prevLayer = controller.CurrentLayer;

    }

    // Update is called once per frame
    void Update()
    {
        if (prevLayer != controller.CurrentLayer)
        {
            prevLayer = controller.CurrentLayer;
        }
    }

    void SetImage ()
    {

    }
}
