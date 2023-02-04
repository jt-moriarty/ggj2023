using System;
using System.Collections.Generic;
using UnityEngine;

public class PipeController<ResourceEnum> where ResourceEnum : System.Enum
{
    ICollection<PipeNode<ResourceEnum>> allPipes = new HashSet<PipeNode<ResourceEnum>>();
    ICollection<PipeNode<ResourceEnum>> cores = new HashSet<PipeNode<ResourceEnum>>();

    int maxFlowPerStep;
    OnFlow flowDelegate;

    public delegate void OnFlow(PipeNode<ResourceEnum> source, PipeNode<ResourceEnum> destination, ResourceEnum resourceType, int amount);

    public PipeController(int maxFlowPerStep, OnFlow flowDelegate)
    {
        this.maxFlowPerStep = maxFlowPerStep;
        this.flowDelegate = flowDelegate;
    }

    public PipeNode<ResourceEnum> CreateNonCore(String name)
    {
        var pipe = new PipeNode<ResourceEnum>(name);
        allPipes.Add(pipe);

        return pipe;
    }

    public PipeNode<ResourceEnum> CreateCore(String name)
    {
        PipeNode<ResourceEnum> core = this.CreateNonCore(name);
        cores.Add(core);
        return core;
    }

    public void SetAdjacency(PipeNode<ResourceEnum> destination, IEnumerable<PipeNode<ResourceEnum>> sources, bool isAdjacent)
    {
        ICollection<PipeNode<ResourceEnum>> adjacencies = destination.GetAdjacencies();
        if (isAdjacent)
        {
            foreach (var source in sources)
            {
                adjacencies.Add(source);
            }
        }
        else
        {
            foreach (var source in sources)
            {
                adjacencies.Remove(source);
            }
        }
    }

    public void DoFlows()
    {
        foreach (var node in allPipes) 
        {
            node.SetDistance(-1);
        }

        foreach (var core in cores)
        {
            core.SetDistance(0);

            var visited = new HashSet<PipeNode<ResourceEnum>>();

            // could just check all of queue but this might be faster in very connected situation
            var enqueued = new HashSet<PipeNode<ResourceEnum>>();

            var checkQueue = new Queue<PipeNode<ResourceEnum>>();
            checkQueue.Enqueue(core);
            while (checkQueue.Count > 0)
            {
                var currentPipe = checkQueue.Dequeue();
                visited.Add(currentPipe);

                foreach (var adj in currentPipe.GetAdjacencies())
                {
                    //dont absorb from a tile that has already done an absorption
                    if (visited.Contains(adj))
                    {
                        continue;
                    }

                    foreach (ResourceEnum resourceType in System.Enum.GetValues(typeof(ResourceEnum)))
                    {
                        int availableFlow = currentPipe.GetAvailableFlowAmount(adj, resourceType);
                        if (availableFlow > 0)
                        {
                            int amount = Mathf.Min(availableFlow, maxFlowPerStep);

                            adj.MoveResource(currentPipe, resourceType, amount);
                            flowDelegate(adj, currentPipe, resourceType, amount);
                        }
                    }

                    if (!enqueued.Contains(adj)) {
                        checkQueue.Enqueue(adj);
                        enqueued.Add(adj);
                    }
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
