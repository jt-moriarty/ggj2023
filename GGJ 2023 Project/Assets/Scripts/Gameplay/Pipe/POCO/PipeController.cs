using System;
using System.Collections.Generic;
using UnityEngine;

public class PipeController<ResourceEnum> where ResourceEnum : System.Enum
{
    HashSet<PipeNode<ResourceEnum>> allPipes;
    PipeNode<ResourceEnum> core;

    int maxFlowPerStep;
    OnFlow flowDelegate;

    public delegate void OnFlow(PipeNode<ResourceEnum> source, PipeNode<ResourceEnum> destination, ResourceEnum resourceType, int amount);

    public PipeController(int maxFlowPerStep, OnFlow flowDelegate)
    {
        this.maxFlowPerStep = maxFlowPerStep;
        core = new PipeNode<ResourceEnum>(new List<PipeNode<ResourceEnum>>(), true);
        allPipes = new HashSet<PipeNode<ResourceEnum>>{core};
        this.flowDelegate = flowDelegate;
    }

    public PipeNode<ResourceEnum> CreateVacancy(List<PipeNode<ResourceEnum>> connectedAdjacents)
    {
        var pipe = new PipeNode<ResourceEnum>(connectedAdjacents, false);
        allPipes.Add(pipe);
        UpdateDistances();

        return pipe;
    }

    public PipeNode<ResourceEnum> GetCore()
    {
        return core;
    }

    private void UpdateDistances()
    {
        foreach (var node in allPipes)
        {
            node.SetDistance(-1);
        }
        core.SetDistance(0);

        var checkQueue = new Queue<PipeNode<ResourceEnum>>();
        checkQueue.Enqueue(core);
        while (checkQueue.Count > 0)
        {
            var currentPipe = checkQueue.Dequeue();
            var curDist = currentPipe.GetDistance();
            var neighDist = curDist + 1;

            foreach (var adj in currentPipe.GetAdjacencies())
            {
                if (currentPipe.IsBlockedWith(adj))
                {
                    continue;
                }
                if (adj.GetDistance() == -1)
                {
                    adj.SetDistance(neighDist);
                    checkQueue.Enqueue(adj);
                }
            }
        }

        foreach (var node in allPipes)
        {
            node.SortAdjacents();
        }
    }

    public void UpdateAdjacencies()
    {
        Debug.Log("If this gets called more than once, please reconsider");
        UpdateDistances();
    }

    public void UpdateAdjacencies(List<NodePair> unblockedPairs, List<NodePair> newlyBlockedPairs)
    {
        foreach (var pair in unblockedPairs)
        {
            pair.occupiedNode.UnblockAdjacency(pair.otherNode);
            pair.occupiedNode.SetVacant(false);
        }

        foreach (var pair in newlyBlockedPairs)
        {
            pair.occupiedNode.BlockAdjacency(pair.otherNode);
            pair.occupiedNode.SetVacant(false);
        }

        UpdateDistances();
    }

    public void SetAsVacant(PipeNode<ResourceEnum> newlyVacantNode)
    {
        newlyVacantNode.IsVacant();
        foreach (var adj in newlyVacantNode.GetAdjacencies())
        {
            newlyVacantNode.UnblockAdjacency(adj);
        }
    }

    public void DoFlows()
    {
        var visited = new HashSet<PipeNode<ResourceEnum>>();

        var checkQueue = new Queue<PipeNode<ResourceEnum>>();
        checkQueue.Enqueue(core);
        while (checkQueue.Count > 0)
        {
            var currentPipe = checkQueue.Dequeue();
            visited.Add(currentPipe);            

            foreach (var adj in currentPipe.GetAdjacencies())
            {
                if (currentPipe.IsBlockedWith(adj) || visited.Contains(adj))
                {
                    continue;
                }

                foreach (ResourceEnum resourceType in System.Enum.GetValues(typeof(ResourceEnum)))
                {
                    if (adj.GetDestination(resourceType, out var destination, out int availableFlow))
                    {
                        int amount = Mathf.Min(availableFlow, maxFlowPerStep);
                        
                        adj.MoveResource(destination, resourceType, amount);
                        flowDelegate(adj, destination, resourceType, amount);
                    }
                }

                if (!adj.IsVacant())
                {
                    checkQueue.Enqueue(adj);
                }
            }
        }
    }

    public class NodePair
    {
        public PipeNode<ResourceEnum> occupiedNode;
        public PipeNode<ResourceEnum> otherNode;
    }
}
