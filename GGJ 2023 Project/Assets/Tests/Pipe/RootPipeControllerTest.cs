using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using PC = RootPipeController<PipeControllerTest.Res, UnityEngine.Vector3>;
using PN = PipeNode<PipeControllerTest.Res, UnityEngine.Vector3>;

public class RootPipeControllerTest
{

    // A Test behaves as an ordinary method
    [Test]
    public void RootPipeControllerTestSimplePasses()
    {

        var controller = new PC(5, 5, 3,
            (src, dst, resType, amnt) =>
            {
                Debug.Log($"Moving {amnt} {resType} from {src} (world position {src.Info}) to {dst} (world position {dst.Info})");
            }, (x, y, z) => 
            {
                return x * new Vector3(1, 0.25f, 0)
                + new Vector3(-1, 0.25f, 0) +
                z * Vector3.up;
            });
    }
}
