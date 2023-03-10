using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

using PC = PipeController<PipeControllerTest.Res, string>;
using PN = PipeNode<PipeControllerTest.Res, string>;

public class PipeControllerTest
{
    public enum Res
    {
        water,
        food
    }


    private void TestPrep(
        out PC controller,
        out PN core,
        out PN[,,] grid,
        PC.OnFlow flowDelegate)
    {
        controller = new PC(3, flowDelegate);
        core = controller.CreateCore("core1", "foo");

        grid = new PN[3, 3, 3];
        int coreX = 1;
        int coreY = 1;
        int coreZ = 1;
        for (int z = 0; z < 3; z++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (x == coreX && y == coreY && z == coreZ)
                    {
                        grid[z, y, x] = core;
                        continue;
                    }
                    grid[z, y, x] = controller.CreateNonCore($"({z},{y},{x})", "foo");
                }
            }
        }

        List<PN> neighs = new List<PN>
        {
            grid[0,1,1], grid[2,1,1],
            grid[1,0,1], grid[1,2,1],
            grid[1,1,0], grid[1,1,2]
        };
        controller.SetAdjacency(core, neighs, true);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void PipeControllerTestSimplePasses()
    {
        TestPrep(out var controller, out var core, out var grid,
            (src, dst, resType, amnt) => {
                Debug.Log($"Moving {amnt} {resType} from {src} to {dst}");
            });

        Assert.That(grid[1, 1, 1].GetAdjacencies(), Has.Member(grid[1, 1, 2]));
        Assert.That(grid[1, 1, 2].GetAdjacencies(), Has.None.SameAs(grid[1, 1, 1]));
        Assert.That(grid[1, 1, 1].GetAdjacencies(), Has.None.SameAs(grid[1, 2, 2]));
        Assert.That(grid[0, 0, 0].GetAdjacencies(), Has.None.SameAs(grid[2, 2, 2]));

        // food spawns at  z,y,x = 1,1,0
        grid[1, 1, 0].AddResource(Res.food, 5);

        Debug.Log("Flowing...");
        controller.DoFlows();

        // confirm that there is now 3 food in the core and 2 at the original place, and 0 in another place
        Assert.That(core.GetResource(Res.food), Is.EqualTo(3));
        Assert.That(grid[1, 1, 0].GetResource(Res.food), Is.EqualTo(2));
        Assert.That(grid[0, 0, 0].GetResource(Res.food), Is.EqualTo(0));

        Debug.Log("Flowing...");
        controller.DoFlows();

        // confirm that food has not moved
        Assert.That(core.GetResource(Res.food), Is.EqualTo(3));
        Assert.That(grid[1, 1, 0].GetResource(Res.food), Is.EqualTo(2));
        Assert.That(grid[0, 0, 0].GetResource(Res.food), Is.EqualTo(0));

        // water spawns at  z,y,x = 2,2,1
        grid[2, 2, 1].AddResource(Res.water, 2);

        Debug.Log("Flowing...");
        controller.DoFlows();

        // confirm that water has not moved
        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(2));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(0));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(0));


        //Add elbow pipe at 1,2,1 that create path from 2,2,1 to core
        List<PN> neighsForElbow = new List<PN> { grid[2, 2, 1] };
        controller.SetAdjacency(grid[1,2,1],neighsForElbow, true);

        List<PN> neighsForCore = new List<PN> { grid[1, 2, 1] };
        controller.SetAdjacency(grid[1, 1, 1], neighsForCore, true);


        Debug.Log("Flowing...");
        controller.DoFlows();

        // confirm that water has moved into the elbow, but not completely
        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(0));

        Debug.Log("Flowing...");
        controller.DoFlows();

        // confirm that water has moved into the core, but not completely
        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(0));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(1));

        Debug.Log("Flowing...");
        controller.DoFlows();

        //Confirm that the water stops moving
        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(0));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(1));

        //drain the water from the core
        core.RemoveResource(Res.water, 3);
        Debug.Log("Flowing...");
        controller.DoFlows();

        // confirm the rest drains into it
        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(0));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(0));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(1));

        //drain the water from the core and spawn more at the place before
        core.RemoveResource(Res.water, 3);
        grid[2, 2, 1].AddResource(Res.water, 2);


        Debug.Log("Flowing...");
        controller.DoFlows();

        // confirm that water has moved into the elbow, but not completely
        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(0));

        // turn the elbow so it is open to the wall and the core
        controller.SetAdjacency(grid[1, 2, 1], new List<PN> { grid[2, 2, 1] }, false);

        Debug.Log("Flowing...");
        controller.DoFlows();

        // confirm that water has moved out of the the elbow, but not into it
        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(0));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(1));

        // pour more water on the spot and turn the elbow to be open to the wet spot and another spot.

        grid[2, 2, 1].AddResource(Res.water, 4);

        neighsForElbow = new List<PN>
        {
            grid[2,2,1],
            grid[1,2,2]
        };
        controller.SetAdjacency(grid[1, 2, 1], neighsForElbow, true);

        controller.SetAdjacency(grid[1, 1, 1], new List<PN> { grid[1, 2, 1] }, false);

        Debug.Log("Flowing...");
        controller.DoFlows();

        // confirm that the water doesn't move because the elbow isnt connected to anything
        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(5));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(0));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(1));

        // Add two 6-way pipes connecting from the elbow to the core

        // creating 6 way pipes located at 1,2,2 and 1,1,2.
        // Thus: (water 2,2,1)->(elbow 1,2,1)->(6-way 1,2,2)->(6way 1,1,2)->(core 1,1,1) path
        List<PN> neighsforSixWay = new List<PN>
        {
            grid[0,2,2],grid[2,2,2],
            grid[1,1,2],
            grid[1,2,1]
        };
        controller.SetAdjacency(grid[1, 2, 2], neighsforSixWay, true);

        neighsforSixWay = new List<PN>
        {
            grid[0,1,2], grid[2,1,2],
            grid[1,0,2], grid[1,2,2]
        };
        controller.SetAdjacency(grid[1, 1, 2], neighsforSixWay, true);

        Debug.Log("Flowing...");
        controller.DoFlows();

        // confirm that the water flows into this new path
        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(2));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(3));
        Assert.That(grid[1, 2, 2].GetResource(Res.water), Is.EqualTo(0));
        Assert.That(grid[1, 1, 2].GetResource(Res.water), Is.EqualTo(0));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(1));

        Debug.Log("Flowing...");
        controller.DoFlows();

        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(2));
        Assert.That(grid[1, 2, 2].GetResource(Res.water), Is.EqualTo(2));
        Assert.That(grid[1, 1, 2].GetResource(Res.water), Is.EqualTo(0));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(1));

        Debug.Log("Flowing...");
        controller.DoFlows();

        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 2].GetResource(Res.water), Is.EqualTo(2));
        Assert.That(grid[1, 1, 2].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(1));

        Debug.Log("Flowing...");
        controller.DoFlows();

        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 2].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 2].GetResource(Res.water), Is.EqualTo(2));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(1));

        Debug.Log("Flowing...");
        controller.DoFlows();

        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 2].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 2].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(2));

        Debug.Log("Flowing...");
        controller.DoFlows();

        //Confirm that the water ceases to flow any further
        Assert.That(grid[2, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 1].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 2].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 2].GetResource(Res.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 1].GetResource(Res.water), Is.EqualTo(2));
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator PipeControllerTestWithEnumeratorPasses()
    //{
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        //yield return null;
    //}
}
