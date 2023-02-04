using System.Collections.Generic;
using UnityEngine;

public class PipeNode<ResourceEnum> where ResourceEnum : System.Enum
{
    private bool isVacant;
    private readonly bool isCore;
    private List<PipeNode<ResourceEnum>> adjacents;
    private HashSet<PipeNode<ResourceEnum>> blockedAdjacents;
    private int distanceFromCore;
    private Dictionary<ResourceEnum, int> resourceState;
    private readonly string name;

    override public string ToString() { return name; }

    private bool IsDisconnected { get { return distanceFromCore > -1; } }

    /**
     * Tells you which pipe node the given element is going to flow, or null if none. Also tells you how much could flow. 
     * Returns true if any flow  happens out from this node
     */
    public bool GetDestination(ResourceEnum resource, out PipeNode<ResourceEnum> destination, out int availableFlow)
    {
        destination = null;
        availableFlow = 0;
        int curState = resourceState[resource];
        foreach(var adj in adjacents)
        {
            if (IsBlockedWith(adj) || adj.isVacant)
            {
                continue;
            }
            int adjState = adj.resourceState[resource];
            if (adjState < curState)
            {
                destination = adj;
                availableFlow = Mathf.CeilToInt((curState - adjState)/2f);
                return true;
            }
        }
        return false;
    }


    public void AddResource(ResourceEnum resource, int amount)
    {
        resourceState[resource] += amount;
    }
    public void RemoveResource(ResourceEnum resource, int amount)
    {
        resourceState[resource] = Mathf.Max(0,resourceState[resource] - amount);
    }

    public int GetResource(ResourceEnum resource)
    {
        return resourceState[resource];
    }

    /**********************************************
     ************* HERE BE DRAGONS ****************
     **********************************************/

    /**
     * Please don't call this outside PipeController.
     */
    public PipeNode(List<PipeNode<ResourceEnum>> adjacents, bool isCore, string name)
    {
        resourceState = new Dictionary<ResourceEnum, int>();
        foreach(ResourceEnum resourceType in System.Enum.GetValues(typeof(ResourceEnum)))
        {
            resourceState[resourceType] = 0;
        }

        this.adjacents = new List<PipeNode<ResourceEnum>>(adjacents);
        foreach (var adj in adjacents)
        {
            adj.adjacents.Add(this);
        }
        blockedAdjacents = new HashSet<PipeNode<ResourceEnum>>();

        this.isCore = isCore;
        isVacant = !isCore;
        this.name = name;
    }


    /**
     * Please don't call this outside PipeController.
     */
    public void MoveResource(PipeNode<ResourceEnum> destination, ResourceEnum resource, int amount)
    {
        resourceState[resource] -= amount;
        destination.resourceState[resource] += amount;
    }

    /**
     * Please don't call this outside PipeController.
     */
    public void SetVacant(bool isVacant) { this.isVacant = isVacant; }

    /**
     * Please don't call this outside PipeController.
     */
    public bool IsVacant() { return isVacant; }

    /**
     * Please dont call this outside PipeController. Sorts adjacencies in order of distance from core
     */
    public void SortAdjacents()
    {
        adjacents.Sort((pn1, pn2) => pn1.distanceFromCore - pn2.distanceFromCore);
    }

    /**
     * Please don't call this outside PipeController 
     */
    public void BlockAdjacency(PipeNode<ResourceEnum> other)
    {
        blockedAdjacents.Add(other);
    }

    /**
     * Please don't call this outside PipeController 
     */
    public void UnblockAdjacency(PipeNode<ResourceEnum> other)
    {
        blockedAdjacents.Remove(other);
    }

    /**
     * Please don't call this outside PipeController 
     */
    public List<PipeNode<ResourceEnum>> GetAdjacencies()
    {
        return adjacents;
    }

    
    /**
     * Please don't call this outside PipeController 
     */
    public int GetDistance() { return this.distanceFromCore; }
    /**
     * Please don't call this outside PipeController
     */
    public void SetDistance(int distance) { this.distanceFromCore = distance; }

    /**
     * Please dont call this outside PipeController. Tells you if this is blocking from other or vice versa.
     */
    public bool IsBlockedWith(PipeNode<ResourceEnum> other)
    {
        return blockedAdjacents.Contains(other) || other.blockedAdjacents.Contains(this);
    }
}
