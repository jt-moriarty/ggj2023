using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootsController : MonoBehaviour
{
    private RootPipeController<Res, UnityEngine.Vector3> rootPipeController;

    public int gridWidth = 10;
    public int gridDepth = 10;

    public enum Res
    {
        energy
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Setup()
    {
        /*rootPipeController = new RootPipeController(gridWidth, gridDepth, 3,
            (src, dst, resType, amnt) =>
        {
            Debug.Log($"Moving {amnt} {resType} from {src} (world position {src.Info}) to {dst} (world position {dst.Info})");
        }, (x, y, z) =>
        {
            return x * new Vector3(1, 0.25f, 0)
            + new Vector3(-1, 0.25f, 0) +
            z * Vector3.up;
        });*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
