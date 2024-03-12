using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace GameOfLife.ECS.IOSystems
{
    [UpdateInGroup(typeof(GameOfLifeCrationAndIOSystemGroupSystem))]
    [UpdateBefore(typeof(ActiveDeactivateSystem))]
    public partial class LoadBaseGridGivenSizeSystem : SystemBase
    {
        private int2 GridSize;
        private EntityQuery _querry;
        protected override void OnCreate()
        {
            base.OnCreate();
            this.Enabled = false;
            _querry = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ActualChunkInGridData, GridIndex>()
                .Build(this);
            RequireForUpdate<ChunkGridPrefab>();
            RequireForUpdate<GridSizeSingleton>();
        }
        protected override void OnUpdate()
        {
            this.Enabled = false;
            
            if(!_querry.IsEmpty)
                EntityManager.DestroyEntity(_querry);
            var prefab = SystemAPI.GetSingleton<ChunkGridPrefab>().Prefab;
            var counter = GridSize.x * GridSize.y;
            SystemAPI.GetSingletonRW<GridSizeSingleton>().ValueRW.size = GridSize;
            EntityManager.RemoveComponent<RunSimulationTag>(SystemAPI.GetSingletonEntity<GlobalSingletonTag>());
            if(counter==0)
                return;
            EntityManager.AddComponent<RecalculateConnectionCommand>(SystemAPI.GetSingletonEntity<GlobalSingletonTag>());
            var archetype = EntityManager.CreateArchetype(new[]
            {
                ComponentType.ReadWrite<GridIndex>(),
                ComponentType.ReadWrite<ActualChunkInGridData>(),
                ComponentType.ReadWrite<RightBorderOfChunk>(),
                ComponentType.ReadWrite<LeftBorderOfChunk>(),
                ComponentType.ReadWrite<UpBorderOfChunk>(),
                ComponentType.ReadWrite<DownBorderOfChunk>(),
                ComponentType.ReadWrite<LeftNeighboorChunkInGrid>(),
                ComponentType.ReadWrite<RightNeighboorChunkInGrid>(),
                ComponentType.ReadWrite<UpNeighboorChunkInGrid>(),
                ComponentType.ReadWrite<DownNeighboorChunkInGrid>(),
            });
            EntityManager.CreateEntity(archetype, counter);
            Dependency = new SetGridIndexForNewOnes()
            {
                UniGridData = EntityManager.GetComponentData<ActualChunkInGridData>(prefab).Value
            }.ScheduleParallel(Dependency);
        }
        private partial struct SetGridIndexForNewOnes : IJobEntity
        {
            [ReadOnly] public ulong UniGridData;
            public void Execute([EntityIndexInQuery] int indexInt, ref GridIndex indexEntity, ref ActualChunkInGridData gridData)
            {
                indexEntity.index = indexInt;
                gridData.Value = UniGridData;
            }
        }

        public void SetNewGridSize(int2 gridSize)
        {
            GridSize = gridSize;
            this.Enabled = true;
        }
    }
}