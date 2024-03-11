using Unity.Entities;
using Unity.Mathematics;

namespace GameOfLife.ECS
{
    public struct RightBorderOfChunk : IComponentData
    {
        public ulong Value;
    }
}