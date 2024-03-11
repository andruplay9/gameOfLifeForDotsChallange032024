using Unity.Collections;
using Unity.Entities;

namespace GameOfLife.ECS
{
    public struct GridHashMap : IComponentData
    {
        public NativeHashMap<ulong, Entity> HashMap;
    }
}