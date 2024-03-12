using Unity.Entities;

namespace GameOfLife.ECS
{
    public struct LeftNeighboorChunkInGrid : IComponentData
    {
        public int Value;
    }
    public struct RightNeighboorChunkInGrid : IComponentData
    {
        public int Value;
    }
    public struct UpNeighboorChunkInGrid : IComponentData
    {
        public int Value;
    }
    public struct DownNeighboorChunkInGrid : IComponentData
    {
        public int Value;
    }
}