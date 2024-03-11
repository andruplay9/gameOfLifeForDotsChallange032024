using Unity.Entities;

namespace GameOfLife.ECS
{
    public struct ActualChunkInGridData : IComponentData
    {
        public ulong Value;
    }
}