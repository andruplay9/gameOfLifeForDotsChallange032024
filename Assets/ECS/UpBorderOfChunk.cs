using Unity.Entities;

namespace GameOfLife.ECS
{
    public struct UpBorderOfChunk : IComponentData
    {
        public int Value;
    }
}