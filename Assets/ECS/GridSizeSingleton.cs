using Unity.Entities;
using Unity.Mathematics;

namespace GameOfLife.ECS
{
    public struct GridSizeSingleton : IComponentData
    {
        public int2 size;
    }

}