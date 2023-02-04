using System;
using System.Collections.Generic;
using UnityEngine;

public class PipeController<ResourceEnum, PipeInfo> where ResourceEnum : System.Enum
{
    ICollection<PipeNode<ResourceEnum, PipeInfo>> allPipes = new HashSet<PipeNode<ResourceEnum, PipeInfo>>();
    ICollection<PipeNode<ResourceEnum, PipeInfo>> cores = new HashSet<PipeNode<ResourceEnum, PipeInfo>>();

    int maxFlowPerStep;
    OnFlow flowDelegate;

    public delegate void OnFlow(PipeNode<ResourceEnum, PipeInfo> source, PipeNode<ResourceEnum, PipeInfo> destination,
        ResourceEnum resourceType, int amount);

    public PipeController(int maxFlowPerStep, OnFlow flowDelegate)
    {
        this.maxFlowPerStep = maxFlowPerStep;
        this.flowDelegate = flowDelegate;
    }

    public PipeNode<ResourceEnum, PipeInfo> CreateNonCore(String name, PipeInfo info)
    {
        var pipe = new PipeNode<ResourceEnum, PipeInfo>(false, name, info);
        allPipes.Add(pipe);

        return pipe;
    }

    public void RemoveNonCore(PipeNode<ResourceEnum, PipeInfo> pipe)
    {
        allPipes.Remove(pipe);
    }

    public void RemoveCore(PipeNode<ResourceEnum, PipeInfo> core)
    {
        allPipes.Remove(core);
        cores.Remove(core);
    }

    public PipeNode<ResourceEnum, PipeInfo> CreateCore(String name, PipeInfo info)
    {
        var core = new PipeNode<ResourceEnum, PipeInfo>(true, name, info);
        allPipes.Add(core);
  
        cores.Add(core);
        return core;
    }

    public void SetAdjacency(PipeNode<ResourceEnum, PipeInfo> destination, IEnumerable<PipeNode<ResourceEnum, PipeInfo>> sources, bool isAdjacent)
    {
        IEnumerable<PipeNode<ResourceEnum, PipeInfo>> adjacencies = destination.GetAdjacencies();
        if (isAdjacent)
        {
            foreach (var source in sources)
            {
                destination.AddAdjacent(source);
            }
        }
        else
        {
            foreach (var source in sources)
            {
                destination.RemoveAdjacent(source);
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

            var visited = new HashSet<PipeNode<ResourceEnum, PipeInfo>>();

            // could just check all of queue but this might be faster in very connected situation
            var enqueued = new HashSet<PipeNode<ResourceEnum, PipeInfo>>();

            var checkQueue = new Queue<PipeNode<ResourceEnum, PipeInfo>>();
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
        public PipeNode<ResourceEnum, PipeInfo> occupiedNode;
        public PipeNode<ResourceEnum, PipeInfo> otherNode;
    }
}
