using System.Collections.Generic;
using UnityEngine;

public class PipeNode<ResourceEnum> where ResourceEnum : System.Enum
{
    /**
     * What nodes this pipe will draw from, depending on the drawing core
     */
    private ICollection<PipeNode<ResourceEnum>> adjacents = new HashSet<PipeNode<ResourceEnum>>();
    private int distanceFromCore;
    private Dictionary<ResourceEnum, int> resourceState = new Dictionary<ResourceEnum, int>();
    private readonly string name;

    override public string ToString() { return name; }

    private Dictionary<PipeNode<ResourceEnum>, bool> connectedToCore = new Dictionary<PipeNode<ResourceEnum>, bool>();


    public void AddResource(ResourceEnum resource, int amount)
    {
        resourceState[resource] += amount;
    }

    public int RemoveResource(ResourceEnum resource, int amount)
    {
        int removedCount = Mathf.Min(resourceState[resource], amount);
        resourceState[resource] = resourceState[resource] - removedCount;

        return removedCount;
    }

    public int GetResource(ResourceEnum resource)
    {
        return resourceState[resource];
    }
    
    public PipeNode(string name)
    {
        foreach(ResourceEnum resourceType in System.Enum.GetValues(typeof(ResourceEnum)))
        {
            resourceState[resourceType] = 0;
        }
        
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
    
    public ICollection<PipeNode<ResourceEnum>> GetAdjacencies()
    {
        return adjacents;
    }

    public int GetAvailableFlowAmount(PipeNode<ResourceEnum> source, ResourceEnum res)
    {
        return Mathf.CeilToInt((source.resourceState[res] - resourceState[res]) / 2f);
    }

    public void SetDistance(int distanceFromCore) { this.distanceFromCore = distanceFromCore; }

    public bool IsConnectedToCore(PipeNode<ResourceEnum> core)
    {
        return connectedToCore.GetValueOrDefault(core,false);
    }
    public void SetConnectedToCore(PipeNode<ResourceEnum> core, bool onnectedToCore)
    {
        this.connectedToCore[core] = onnectedToCore;
    }
}
