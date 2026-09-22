using System.Collections.Generic;
public class MergeNode
{
    public MergeNode Parent { get; private set; }
    public List<MergeNode> Child { get; private set; }
    public Slot Slot { get; private set; }

    public MergeNode(Slot slot)
    {
        this.Slot = slot;
        Child = new List<MergeNode>();
    }
    public void SetParent(MergeNode parent)
    {
        Parent = parent;
    }

    public void RemoveChild(MergeNode node)
    {
        if(!Child.Contains(node)) return;
        Child.Remove(node);
        node.SetParent(null);
    }

    public void AddChild(MergeNode node)
    {
        node.SetParent(this);
        Child.Add(node);
    }
        
}