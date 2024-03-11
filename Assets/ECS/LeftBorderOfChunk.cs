using Unity.Entities;
using Unity.Mathematics;

namespace GameOfLife.ECS
{
    public struct LeftBorderOfChunk : IComponentData
    {
        public ulong Value;
    }
}