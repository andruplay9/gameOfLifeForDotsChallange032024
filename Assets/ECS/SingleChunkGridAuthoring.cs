using Unity.Entities;
using UnityEngine;

namespace GameOfLife.ECS
{
    public class SingleChunkGridAuthoring : MonoBehaviour
    {
        [SerializeField] private ulong _startValue;
        [SerializeField] private int index=0;
        private class SingleCunkGridAuthoringBaker : Baker<SingleChunkGridAuthoring>
        {
            public override void Bake(SingleChunkGridAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<ActualChunkInGridData>(entity, new ActualChunkInGridData(){Value = authoring._startValue});
                AddComponent<LeftBorderOfChunk>(entity);
                AddComponent<RightBorderOfChunk>(entity);
                AddComponent<UpBorderOfChunk>(entity);
                AddComponent<DownBorderOfChunk>(entity);
                AddComponent<GridIndex>(entity, new GridIndex(){index = (ulong)authoring.index});
                AddComponent<LeftNeighboorChunkInGrid>(entity);
                AddComponent<RightNeighboorChunkInGrid>(entity);
                AddComponent<UpNeighboorChunkInGrid>(entity);
                AddComponent<DownNeighboorChunkInGrid>(entity);
            }
        }
    }
}