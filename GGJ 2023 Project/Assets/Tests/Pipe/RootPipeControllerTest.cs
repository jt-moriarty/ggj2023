using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using RPC = RootPipeController<RootPipeControllerTest.Res, UnityEngine.Vector3>;
using PN = PipeNode<RootPipeControllerTest.Res, UnityEngine.Vector3>;

public class RootPipeControllerTest
{
    public enum Res
    {
        water,
        food
    }

    // A Test behaves as an ordinary method
    [Test]
    public void RootPipeControllerTestSimplePasses()
    {

        var controller = new RPC(5, 5, 3, 5, 10,
            (src, dst, resType, amnt) =>
            {
                Debug.Log($"Moving {amnt} {resType} from {src.pipe} (world position {src.pipe.Info}) to {dst.pipe}" +
                    $" (world position {dst.pipe.Info})");
            }, (x, y, z) => 
            {
                return x * new Vector3(1, 0.25f, 0)
                + new Vector3(-1, 0.25f, 0) +
                z * Vector3.up;
            });

        controller.AddCore(2, 2, "cool starting core");

        controller.AddResource(2, 2, 1, Res.water, 5);

        Assert.That(controller.RemoveResource(2, 2, 1, Res.water, 5), Is.EqualTo(5));
    }
}
