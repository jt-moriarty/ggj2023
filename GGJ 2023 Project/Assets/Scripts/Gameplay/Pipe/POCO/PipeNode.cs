using System.Collections.Generic;
using UnityEngine;

public class PipeNode<ResourceEnum, PipeInfo> where ResourceEnum : System.Enum
{
    /**
     * What nodes this pipe will draw from, depending on the drawing core
     */
    private ICollection<PipeNode<ResourceEnum,PipeInfo>> adjacents = new HashSet<PipeNode<ResourceEnum, PipeInfo>>();
    private ICollection<PipeNode<ResourceEnum, PipeInfo>> backReference = new HashSet<PipeNode<ResourceEnum, PipeInfo>>();
    private int distanceFromCore;
    private Dictionary<ResourceEnum, int> resourceState = new Dictionary<ResourceEnum, int>();
    private readonly string name;
    public readonly PipeInfo Info;

    public readonly bool isCore;

    override public string ToString() { return name; }

    private Dictionary<PipeNode<ResourceEnum, PipeInfo>, bool> connectedToCore = new Dictionary<PipeNode<ResourceEnum, PipeInfo>, bool>();


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
    
    public PipeNode(bool isCore, string name, PipeInfo info)
    {
        foreach(ResourceEnum resourceType in System.Enum.GetValues(typeof(ResourceEnum)))
        {
            resourceState[resourceType] = 0;
        }

        this.isCore = isCore;
        this.name = name;
        this.Info = info;
    }

    public void MoveResource(PipeNode<ResourceEnum, PipeInfo> destination, ResourceEnum resource, int amount)
    {
        resourceState[resource] -= amount;
        destination.resourceState[resource] += amount;
    }

    public IEnumerable<PipeNode<ResourceEnum, PipeInfo>> GetAdjacencies()
    {
        return adjacents;
    }

    public IEnumerable<PipeNode<ResourceEnum, PipeInfo>> GetBackReferences()
    {
        return backReference;
    }

    public void AddAdjacent(PipeNode<ResourceEnum, PipeInfo> source)
    {
        adjacents.Add(source);
        source.backReference.Add(this);
    }

    public void RemoveAdjacent(PipeNode<ResourceEnum, PipeInfo> source)
    {
        adjacents.Remove(source);
        source.backReference.Remove(this);
    }

    public int GetAvailableFlowAmount(PipeNode<ResourceEnum, PipeInfo> source, ResourceEnum res)
    {
        return Mathf.CeilToInt((source.resourceState[res] - resourceState[res]) / 2f);
    }

    public void SetDistance(int distanceFromCore) { this.distanceFromCore = distanceFromCore; }

    public bool IsConnectedToCore(PipeNode<ResourceEnum, PipeInfo> core)
    {
        return connectedToCore.GetValueOrDefault(core,false);
    }
    public void SetConnectedToCore(PipeNode<ResourceEnum, PipeInfo> core, bool onnectedToCore)
    {
        this.connectedToCore[core] = onnectedToCore;
    }
}
