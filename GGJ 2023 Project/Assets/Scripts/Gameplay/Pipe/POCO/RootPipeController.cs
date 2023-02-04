using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootPipeController<ResourceEnum, PipeInfo> where ResourceEnum : Enum
{
    public delegate PipeInfo InfoGetter(int x, int y);

    private readonly InfoGetter infoGetter;
    private PipeController<ResourceEnum, PipeInfo> pipeController;
    private PipeNode<ResourceEnum, PipeInfo>[,,] grid;
    private readonly int xSize;
    private readonly int ySize;

    public RootPipeController(int xSize, int ySize, int maxFlowPerTimestep, PipeController<ResourceEnum, PipeInfo>.OnFlow flowDelegate, InfoGetter infoGetter)
    {
        pipeController = new PipeController<ResourceEnum, PipeInfo>(maxFlowPerTimestep, flowDelegate);

        this.infoGetter = infoGetter;
        grid = new PipeNode<ResourceEnum, PipeInfo>[3, ySize, xSize];
        this.xSize = xSize;
        this.ySize = ySize;

        for (int z = 0; z < 3; z++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    grid[z, y, x] = pipeController.CreateNonCore($"[Non core ({x},{y})]", infoGetter(x,y));
                }
            }
        }
    }

    public void AddCore(int x, int y, string name)
    {
        PipeNode<ResourceEnum, PipeInfo> replacedNode = grid[1, y, x];
        PipeNode<ResourceEnum, PipeInfo> newCore = pipeController.CreateCore(name, infoGetter(x,y));
        foreach (var destination in replacedNode.GetBackReferences())
        {
            destination.RemoveAdjacent(replacedNode);
        }

        newCore.AddAdjacent(grid[0, y, x]);
        newCore.AddAdjacent(grid[2, y, x]);

        if (x > 0)
        {
            newCore.AddAdjacent(grid[1, y, x - 1]);
        }
        if (x+1 < xSize)
        {
            newCore.AddAdjacent(grid[1, y, x + 1]);
        }
        if (y > 0)
        {
            newCore.AddAdjacent(grid[1, y-1, x]);
        }
        if (y+1 < ySize)
        {
            newCore.AddAdjacent(grid[1, y+1, x]);
        }
    }

    public void AddRoot(int x, int y)
    {
        PipeNode<ResourceEnum, PipeInfo> node = grid[1, y, x];

        node.AddAdjacent(grid[0, y, x]);
        node.AddAdjacent(grid[2, y, x]);

        if (x > 0)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y, x - 1];
            if (!neigh.isCore)
            {
                node.AddAdjacent(neigh);
            }
        }
        if (x + 1 < xSize)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y, x + 1];
            if (!neigh.isCore)
            {
                node.AddAdjacent(neigh);
            }
        }
        if (y > 0)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y - 1, x];
            if (!neigh.isCore)
            {
                node.AddAdjacent(neigh);
            }
        }
        if (y + 1 < ySize)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y + 1, x];
            if (!neigh.isCore)
            {
                node.AddAdjacent(neigh);
            }
        }
    }

    public void RemoveRoot(int x, int y)
    {
        PipeNode<ResourceEnum, PipeInfo> node = grid[1, y, x];

        node.RemoveAdjacent(grid[0, y, x]);
        node.RemoveAdjacent(grid[2, y, x]);

        if (x > 0)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y, x - 1];
            node.RemoveAdjacent(neigh);
        }
        if (x + 1 < xSize)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y, x + 1];
            node.RemoveAdjacent(neigh);
        }
        if (y > 0)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y - 1, x];
            node.AddAdjacent(neigh);
        }
        if (y + 1 < ySize)
        {
            PipeNode<ResourceEnum, PipeInfo> neigh = grid[1, y + 1, x];
            node.AddAdjacent(neigh);
        }
    }

    public void DoFlows()
    {
        pipeController.DoFlows();
    }
}
