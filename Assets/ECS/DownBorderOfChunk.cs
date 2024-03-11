using Unity.Entities;

namespace GameOfLife.ECS
{
    public struct DownBorderOfChunk : IComponentData
    {
        public int Value;
    }
}