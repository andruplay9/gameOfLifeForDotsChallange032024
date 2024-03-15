using Unity.Entities;
using Unity.Mathematics;

namespace GameOfLife.ECS
{
    public struct RightBorderOfChunk : IComponentData
    {
        public ulong Value;
    }
    public struct RightUpBorderOfChunk : IComponentData
    {
        public bool Value;
    }
    public struct RightDownBorderOfChunk : IComponentData
    {
        public bool Value;
    }
    public struct LeftUpBorderOfChunk : IComponentData
    {
        public bool Value;
    }
    public struct LeftDownBorderOfChunk : IComponentData
    {
        public bool Value;
    }
}