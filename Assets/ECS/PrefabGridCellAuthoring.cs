using Unity.Entities;
using UnityEngine;

namespace GameOfLife.ECS
{
    public class PrefabGridCellAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        private class PrefabGridCellAuthoringBaker : Baker<PrefabGridCellAuthoring>
        {
            public override void Bake(PrefabGridCellAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.ManualOverride);
                AddComponent(entity, new ChunkGridPrefab(){Prefab = GetEntity(authoring._prefab,TransformUsageFlags.ManualOverride)});
            }
        }
    }
}