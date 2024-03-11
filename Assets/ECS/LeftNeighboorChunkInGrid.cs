using Unity.Entities;

namespace GameOfLife.ECS
{
    public struct LeftNeighboorChunkInGrid : IComponentData
    {
        public ulong Value;
    }
    public struct RightNeighboorChunkInGrid : IComponentData
    {
        public ulong Value;
    }
    public struct UpNeighboorChunkInGrid : IComponentData
    {
        public ulong Value;
    }
    public struct DownNeighboorChunkInGrid : IComponentData
    {
        public ulong Value;
    }
}