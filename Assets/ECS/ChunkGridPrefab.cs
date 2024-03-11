using Unity.Entities;

namespace GameOfLife.ECS
{
    public struct ChunkGridPrefab : IComponentData
    {
        public Entity Prefab;
    }
}