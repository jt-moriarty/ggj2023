using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootPipeController<ResourceEnum, PipeInfo> where ResourceEnum : Enum
{

    private class GridLocation
    {
        public PipeNode<ResourceEnum, PipeInfo> pipe;
        public Dictionary<ResourceEnum, GameObject> resObj = new Dictionary<ResourceEnum, GameObject>();
        public bool hasRoot;
        public int stepsAtZero;
    }
    private Dictionary<PipeNode<ResourceEnum, PipeInfo>, GridLocation> backRefs = new Dictionary<PipeNode<ResourceEnum, PipeInfo>, GridLocation>();
    public delegate PipeInfo InfoGetter(int x, int y, int z);

    private readonly InfoGetter infoGetter;
    private PipeController<ResourceEnum, PipeInfo> pipeController;
    private GridLocation[,,] grid;
    private readonly int xSize;
    private readonly int ySize;
    PipeController<ResourceEnum, PipeInfo>.OnFlow flowDelegate;

    private readonly int weakStepCount;
    private readonly int fatalStepCount;


    private HashSet<PipeInfo> newlyDeadNodes = new HashSet<PipeInfo>();
    private HashSet<PipeInfo> newlyWeakenedNodes = new HashSet<PipeInfo>();

    public RootPipeController(int xSize, int ySize, int maxFlowPerTimestep,
        int weakStepCount, int fatalStepCount,
        PipeController<ResourceEnum, PipeInfo>.OnFlow flowDelegate, InfoGetter infoGetter)
    {
        // Gonna do a pro gamer move and wrap the delegate with a cooler delegate

        pipeController = new PipeController<ResourceEnum, PipeInfo>(maxFlowPerTimestep, UpdateTimers);

        this.flowDelegate = flowDelegate;
        this.infoGetter = infoGetter;
        this.weakStepCount = weakStepCount;
        this.fatalStepCount = fatalStepCount;
        grid = new GridLocation[3, ySize, xSize];
        this.xSize = xSize;
        this.ySize = ySize;

        for (int z = 0; z < 3; z++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    grid[z, y, x] =
                        new GridLocation() {
                            pipe = pipeController.CreateNonCore($"[Non core ({x},{y})]", infoGetter(x, y, z)),
                            hasRoot = false
                        };
                    backRefs[grid[z,y,x].pipe] = grid[z,y,x];
                }
            }
        }
    }

    public void UpdateTimers(PipeNode<ResourceEnum, PipeInfo> source, PipeNode<ResourceEnum, PipeInfo> destination,
        ResourceEnum resourceType, int amount)
    {
        backRefs[source].stepsAtZero = 0;
        backRefs[destination].stepsAtZero = 0;
        flowDelegate(source, destination,resourceType, amount);
    }

    public void AddCore(int x, int y, string name)
    {
        if (IsCore(x, y))
        {
            Debug.LogError("core already exists at the specified location. Aborting. Hopefully this mistake didn't cost anything!");
            return;
        }

        GridLocation replacedNode = grid[1, y, x];
        backRefs.Remove(replacedNode.pipe);
        if (!replacedNode.hasRoot)
        {
            replacedNode.stepsAtZero = 0;
        }
        replacedNode.hasRoot = false;

        PipeNode<ResourceEnum, PipeInfo> newCore = pipeController.CreateCore(name, infoGetter(x,y,1));

        foreach (var destination in replacedNode.pipe.GetBackReferencesCopy())
        {
            destination.RemoveAdjacent(replacedNode.pipe);
        }

        newCore.AddAdjacent(grid[0, y, x].pipe);
        newCore.AddAdjacent(grid[2, y, x].pipe);

        if (x > 0)
        {
            GridLocation neigh = grid[1, y, x - 1];
            if (neigh.hasRoot)
            {
                newCore.AddAdjacent(neigh.pipe);
            }
        }
        if (x + 1 < xSize)
        {
            GridLocation neigh = grid[1, y, x + 1];
            if (neigh.hasRoot)
            {
                newCore.AddAdjacent(neigh.pipe);
            }
        }
        if (y > 0)
        {
            GridLocation neigh = grid[1, y - 1, x];
            if (neigh.hasRoot)
            {
                newCore.AddAdjacent(neigh.pipe);
            }
        }
        if (y + 1 < ySize) 
        {
            GridLocation neigh = grid[1, y+1, x];
            if (neigh.hasRoot)
            {
                newCore.AddAdjacent(neigh.pipe);
            }
        }

        foreach (ResourceEnum resourceType in System.Enum.GetValues(typeof(ResourceEnum)))
        {
            int amnt = replacedNode.pipe.GetResource(resourceType);
            newCore.AddResource(resourceType, amnt);
        }

        grid[1, y, x].pipe = newCore;
        backRefs[newCore] = grid[1, y, x];
    }

    public void AddRoot(int x, int y)
    {
        if (IsOccupied(x, y))
        {
            Debug.LogError("Root or core already exists at the specified location. Aborting. Hopefully this mistake didn't cost anything!");
            return;
        }

        GridLocation node = grid[1, y, x];
        node.stepsAtZero = 0;
        node.hasRoot = true;

        node.pipe.AddAdjacent(grid[0, y, x].pipe);
        node.pipe.AddAdjacent(grid[2, y, x].pipe);

        if (x > 0)
        {
            GridLocation neigh = grid[1, y, x - 1];
            if (neigh.pipe.isCore)
            {
                neigh.pipe.AddAdjacent(node.pipe);
            }
            else if (neigh.hasRoot)
            {
                node.pipe.AddAdjacent(neigh.pipe);
                neigh.pipe.AddAdjacent(node.pipe);
            }
        }
        if (x + 1 < xSize)
        {
            GridLocation neigh = grid[1, y, x + 1];
            if (neigh.pipe.isCore)
            {
                neigh.pipe.AddAdjacent(node.pipe);
            }
            else if (neigh.hasRoot)
            {
                node.pipe.AddAdjacent(neigh.pipe);
                neigh.pipe.AddAdjacent(node.pipe);
            }
        }
        if (y > 0)
        {
            GridLocation neigh = grid[1, y - 1, x];
            if (neigh.pipe.isCore)
            {
                neigh.pipe.AddAdjacent(node.pipe);
            }
            else if (neigh.hasRoot)
            {
                node.pipe.AddAdjacent(neigh.pipe);
                neigh.pipe.AddAdjacent(node.pipe);
            }
        }
        if (y + 1 < ySize)
        {
            GridLocation neigh = grid[1, y + 1, x];
            if (neigh.pipe.isCore)
            {
                neigh.pipe.AddAdjacent(node.pipe);
            }
            else if (neigh.hasRoot)
            {
                node.pipe.AddAdjacent(neigh.pipe);
                neigh.pipe.AddAdjacent(node.pipe);
            }
        }
    }

    public void RemoveNode(int x, int y)
    {
        Debug.Log($"removing node at ({x},{y})");
        GridLocation node = grid[1, y, x];
        PipeNode<ResourceEnum, PipeInfo> pipe = node.pipe;
        node.hasRoot = false;

        pipe.RemoveAdjacent(grid[0, y, x].pipe);
        pipe.RemoveAdjacent(grid[2, y, x].pipe);

        if (x > 0)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y, x - 1].pipe;
            pipe.RemoveAdjacent(neigh);
            neigh.RemoveAdjacent(pipe);
        }
        if (x + 1 < xSize)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y, x + 1].pipe;
            pipe.RemoveAdjacent(neigh);
            neigh.RemoveAdjacent(pipe);
        }
        if (y > 0)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y - 1, x].pipe;
            pipe.RemoveAdjacent(neigh);
            neigh.RemoveAdjacent(pipe);
        }
        if (y + 1 < ySize)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y + 1, x].pipe;
            pipe.RemoveAdjacent(neigh);
            neigh.RemoveAdjacent(pipe);
        }

        if (pipe.isCore)
        {
            backRefs.Remove(node.pipe);
            node.pipe = pipeController.CreateNonCore($"[Non core ({x},{y})]", infoGetter(x, y, 1));
            backRefs[node.pipe] = node;
        }
    }

    public bool IsOccupied(int x, int y)
    {
        return (grid[1, y, x].hasRoot || grid[1, y, x].pipe.isCore);
    }

    public bool IsCore(int x, int y)
    {
        return grid[1, y, x].pipe.isCore;
    }

    public void DoFlows()
    {
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                grid[1, y, x].stepsAtZero++;
            }
        }

        pipeController.DoFlows();

        newlyDeadNodes.Clear();
        newlyWeakenedNodes.Clear();

        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                if (!IsOccupied(x, y))
                {
                    continue;
                }
                if (grid[1, y, x].stepsAtZero == fatalStepCount)
                {
                    newlyDeadNodes.Add(grid[1, y, x].pipe.Info);
                } 
                else if (grid[1,y,x].stepsAtZero == weakStepCount)
                {
                    newlyWeakenedNodes.Add(grid[1, y, x].pipe.Info);
                }
            }
        }
    }

    public void GetNodeHealthChanges(out IEnumerable<PipeInfo> newlyDeadNodes, out IEnumerable<PipeInfo> newlyWeakenedNodes)
    {
        newlyDeadNodes = this.newlyDeadNodes;
        newlyWeakenedNodes = this.newlyWeakenedNodes;
    }

    public bool IsNodeHealthy(int x, int y)
    {
        return grid[1, y, x].stepsAtZero < weakStepCount;
    }


    public void AddResource(int x, int y, int z, ResourceEnum res, int amount)
    {
        grid[z, y, x].pipe.AddResource(res, amount);
    }

    public int RemoveResource(int x, int y, int z, ResourceEnum res, int amount)
    {
        return grid[z, y, x].pipe.RemoveResource(res, amount);
    }

    public int GetResource(int x, int y, int z, ResourceEnum res)
    {
        return grid[z, y, x].pipe.GetResource(res);
    }

    public GameObject GetResourceObj(int x, int y, int z, ResourceEnum res)
    {
        return grid[z, y, x].resObj.GetValueOrDefault(res,null);
    }

    public void SetResourceObj(int x, int y, int z, ResourceEnum res, GameObject obj)
    {
        grid[z, y, x].resObj[res] = obj;
    }
}
