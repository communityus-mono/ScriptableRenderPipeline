using System;
using System.Collections.Generic;

namespace UnityEngine.Graphing
{
    public interface IGraph : IOnAssetEnabled
    {
        IEnumerable<T> GetNodes<T>() where T : INode;
        IEnumerable<IEdge> edges { get; }
        void AddNode(INode node);
        void RemoveNode(INode node);
        IEdge Connect(SlotReference fromSlotRef, SlotReference toSlotRef);
        void RemoveEdge(IEdge e);
        void RemoveElements(IEnumerable<INode> nodes, IEnumerable<IEdge> edges);
        INode GetNodeFromGuid(Guid guid);
        T GetNodeFromGuid<T>(Guid guid) where T : INode;
        IEnumerable<IEdge> GetEdges(SlotReference s);
        void ValidateGraph();
    }
}
