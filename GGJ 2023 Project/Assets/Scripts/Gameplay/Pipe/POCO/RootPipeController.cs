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
    }
    public delegate PipeInfo InfoGetter(int x, int y, int z);

    private readonly InfoGetter infoGetter;
    private PipeController<ResourceEnum, PipeInfo> pipeController;
    private GridLocation[,,] grid;
    private readonly int xSize;
    private readonly int ySize;

    public RootPipeController(int xSize, int ySize, int maxFlowPerTimestep, PipeController<ResourceEnum, PipeInfo>.OnFlow flowDelegate, InfoGetter infoGetter)
    {
        pipeController = new PipeController<ResourceEnum, PipeInfo>(maxFlowPerTimestep, flowDelegate);

        this.infoGetter = infoGetter;
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
                }
            }
        }
    }

    public void AddCore(int x, int y, string name)
    {
        GridLocation replacedNode = grid[1, y, x];
        PipeNode<ResourceEnum, PipeInfo> newCore = pipeController.CreateCore(name, infoGetter(x,y,1));
        foreach (var destination in replacedNode.pipe.GetBackReferences())
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
    }

    public void AddRoot(int x, int y)
    {
        GridLocation node = grid[1, y, x];
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
            else
            { 
                node.pipe.AddAdjacent(neigh.pipe);
            }
        }
        if (x + 1 < xSize)
        {
            GridLocation neigh = grid[1, y, x + 1];
            if (neigh.pipe.isCore)
            {
                neigh.pipe.AddAdjacent(node.pipe);
            }
            else
            {
                node.pipe.AddAdjacent(neigh.pipe);
            }
        }
        if (y > 0)
        {
            GridLocation neigh = grid[1, y - 1, x];
            if (neigh.pipe.isCore)
            {
                neigh.pipe.AddAdjacent(node.pipe);
            }
            else
            {
                node.pipe.AddAdjacent(neigh.pipe);
            }
        }
        if (y + 1 < ySize)
        {
            GridLocation neigh = grid[1, y + 1, x];
            if (neigh.pipe.isCore)
            {
                neigh.pipe.AddAdjacent(node.pipe);
            }
            else
            {
                node.pipe.AddAdjacent(neigh.pipe);
            }
        }
    }

    public void RemoveRoot(int x, int y)
    {
        GridLocation node = grid[1, y, x];
        PipeNode<ResourceEnum, PipeInfo> pipe = node.pipe;
        node.hasRoot = false;

        pipe.RemoveAdjacent(grid[0, y, x].pipe);
        pipe.RemoveAdjacent(grid[2, y, x].pipe);

        if (x > 0)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y, x - 1].pipe;
            pipe.RemoveAdjacent(neigh);
            if (neigh.isCore)
            {
                neigh.RemoveAdjacent(pipe);
            }
        }
        if (x + 1 < xSize)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y, x + 1].pipe;
            pipe.RemoveAdjacent(neigh);
            if (neigh.isCore)
            {
                neigh.RemoveAdjacent(pipe);
            }
        }
        if (y > 0)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y - 1, x].pipe;
            pipe.RemoveAdjacent(neigh);
            if (neigh.isCore)
            {
                neigh.RemoveAdjacent(pipe);
            }
        }
        if (y + 1 < ySize)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y + 1, x].pipe;
            pipe.RemoveAdjacent(neigh);
            if (neigh.isCore)
            {
                neigh.RemoveAdjacent(pipe);
            }
        }
    }

    public bool IsOccupied(int x, int y)
    {
        return (grid[1, y, x].hasRoot || grid[1, y, x].pipe.isCore);
    }

    public void DoFlows()
    {
        pipeController.DoFlows();
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
