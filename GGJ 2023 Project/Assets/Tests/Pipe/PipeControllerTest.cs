using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class PipeControllerTest
{
    enum WaterAndFoodResources
    {
        water,
        food
    }

    private void TestPrep(
        out PipeController<WaterAndFoodResources> controller,
        out PipeNode<WaterAndFoodResources> core,
        out PipeNode<WaterAndFoodResources>[,,] grid,
        PipeController<WaterAndFoodResources>.OnFlow flowDelegate)
    {
        controller = new PipeController<WaterAndFoodResources>(3, flowDelegate);
        core = controller.GetCore();

        grid = new PipeNode<WaterAndFoodResources>[3, 3, 3];
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

                    var neighs = new List<PipeNode<WaterAndFoodResources>>();
                    if (x > 0) neighs.Add(grid[z, y, x - 1]);
                    if (y > 0) neighs.Add(grid[z, y - 1, x]);
                    if (z > 0) neighs.Add(grid[z - 1, y, x]);
                    if (x == coreX && y == coreY)
                    {
                        if (z + 1 == coreZ || z - 1 == coreZ)
                        {
                            neighs.Add(core);
                        }
                    }
                    if (x == coreX && z == coreZ)
                    {
                        if (y + 1 == coreY || y - 1 == coreY)
                        {
                            neighs.Add(core);
                        }
                    }
                    if (z == coreZ && y == coreY)
                    {
                        if (x + 1 == coreX || x - 1 == coreX)
                        {
                            neighs.Add(core);
                        }
                    }
                    grid[z,y,x] = controller.CreateVacancy(neighs);
                }
            }
        }
        controller.UpdateAdjacencies();
    }

    // A Test behaves as an ordinary method
    [Test]
    public void PipeControllerTestSimplePasses()
    {
        TestPrep(out var controller, out var core, out var grid,
            (src, dst, resType, amnt) => {
                Debug.Log($"Moving {amnt} {resType} from {src} to {dst}");
            });

        Assert.That(!grid[0, 0, 0].IsBlockedWith(grid[0, 0, 1]));
        Assert.That(!grid[1, 1, 1].IsBlockedWith(grid[1, 1, 2]));

        Assert.That(grid[1, 1, 1].GetAdjacencies(), Has.Member(grid[1, 1, 2]));
        Assert.That(grid[1, 1, 1].GetAdjacencies(), Has.None.SameAs(grid[1, 2, 2]));
        Assert.That(grid[0, 0, 0].GetAdjacencies(), Has.None.SameAs(grid[2, 2, 2]));


        Assert.That(grid[1, 1, 1].GetDistance(), Is.EqualTo(0));
        Assert.That(grid[0, 1, 1].GetDistance(), Is.EqualTo(1));
        Assert.That(grid[0, 0, 1].GetDistance(), Is.EqualTo(2));
        Assert.That(grid[0, 0, 2].GetDistance(), Is.EqualTo(3));

        // food spawns at  z,y,x = 1,1,0
        grid[1, 1, 0].AddResource(WaterAndFoodResources.food, 5);

        controller.DoFlows();

        // confirm that there is now 3 food in the core and 2 at the original place, and 0 in another place
        Assert.That(core.GetResource(WaterAndFoodResources.food), Is.EqualTo(3));
        Assert.That(grid[1, 1, 0].GetResource(WaterAndFoodResources.food), Is.EqualTo(2));
        Assert.That(grid[0, 0, 0].GetResource(WaterAndFoodResources.food), Is.EqualTo(0));

        controller.DoFlows();

        // confirm that food has not moved
        Assert.That(core.GetResource(WaterAndFoodResources.food), Is.EqualTo(3));
        Assert.That(grid[1, 1, 0].GetResource(WaterAndFoodResources.food), Is.EqualTo(2));
        Assert.That(grid[0, 0, 0].GetResource(WaterAndFoodResources.food), Is.EqualTo(0));

        // water spawns at  z,y,x = 2,2,1
        grid[2, 2, 1].AddResource(WaterAndFoodResources.water, 2);

        controller.DoFlows();

        // confirm that water has not moved
        Assert.That(grid[2, 2, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(2));
        Assert.That(grid[1, 1, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(0));
        Assert.That(grid[1, 2, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(0));


        //Add elbow pipe at 1,2,1 that create path from 2,2,1 to core
        controller.UpdateAdjacencies(
            new List<PipeController<WaterAndFoodResources>.NodePair>() {
                // we're not removing any existing boundaries here, only creating new ones
                // in an event of an 8 way pipe or a shape that only blocks access to walls,
                // you can put something here to indicate the spot is no longer vacant but has no meaningful blocks.
            },
            new List<PipeController<WaterAndFoodResources>.NodePair>() {
                //an elbow pipe normally blocks 4 out of 6 possible directions.
                // but 1,2,1 is along a wall so one direction is already not blocked,
                // so we only put 3 blockings in this list:
            new PipeController<WaterAndFoodResources>.NodePair(){occupiedNode=grid[1, 2, 1], otherNode= grid[0, 2, 1]},
            new PipeController<WaterAndFoodResources>.NodePair(){occupiedNode=grid[1, 2, 1], otherNode= grid[1, 2, 0]},
            new PipeController<WaterAndFoodResources>.NodePair(){occupiedNode=grid[1, 2, 1], otherNode= grid[1, 2, 2]}
            });


        controller.DoFlows();

        // confirm that water has moved into the elbow, but not completely
        Assert.That(grid[2, 2, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(1));
        Assert.That(grid[1, 2, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(0));

        controller.DoFlows();

        // confirm that water has moved into the core, but not completely
        Assert.That(grid[2, 2, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(0));
        Assert.That(grid[1, 2, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(1));

        controller.DoFlows();

        //Confirm that the water stops moving
        Assert.That(grid[2, 2, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(0));
        Assert.That(grid[1, 2, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(1));
        Assert.That(grid[1, 1, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(1));

        //drain the water from the core
        core.RemoveResource(WaterAndFoodResources.water, 3);
        controller.DoFlows();

        // confirm the rest drains into it
        Assert.That(grid[2, 2, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(0));
        Assert.That(grid[1, 2, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(0));
        Assert.That(grid[1, 1, 1].GetResource(WaterAndFoodResources.water), Is.EqualTo(1));
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator PipeControllerTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
