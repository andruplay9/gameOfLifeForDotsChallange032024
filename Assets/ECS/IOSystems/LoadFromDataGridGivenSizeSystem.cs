using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace GameOfLife.ECS.IOSystems
{
    [UpdateInGroup(typeof(GameOfLifeCrationAndIOSystemGroupSystem))]
    [UpdateBefore(typeof(ActiveDeactivateSystem))]
    public partial class LoadFromDataGridGivenSizeSystem : SystemBase
    {
        private GridFullData Data;
        private EntityQuery _querry;
        protected override void OnCreate()
        {
            base.OnCreate();
            this.Enabled = false;
            _querry = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ActualChunkInGridData, GridIndex>()
                .Build(this);
            RequireForUpdate<GridSizeSingleton>();
        }
        protected override void OnUpdate()
        {
            this.Enabled = false;
            
            if(!_querry.IsEmpty)
                EntityManager.DestroyEntity(_querry);
            var counter = Data.GridSize.x * Data.GridSize.y;
            SystemAPI.GetSingletonRW<GridSizeSingleton>().ValueRW.size = Data.GridSize;
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
                ComponentType.ReadWrite<LeftDownBorderOfChunk>(),
                ComponentType.ReadWrite<RightUpBorderOfChunk>(),
                ComponentType.ReadWrite<LeftUpBorderOfChunk>(),
                ComponentType.ReadWrite<RightDownBorderOfChunk>(),
                ComponentType.ReadWrite<LeftNeighboorChunkInGrid>(),
                ComponentType.ReadWrite<RightNeighboorChunkInGrid>(),
                ComponentType.ReadWrite<UpNeighboorChunkInGrid>(),
                ComponentType.ReadWrite<DownNeighboorChunkInGrid>(),
                ComponentType.ReadWrite<DownLeftNeighboorChunkInGrid>(),
                ComponentType.ReadWrite<DownRightNeighboorChunkInGrid>(),
                ComponentType.ReadWrite<UpLeftNeighboorChunkInGrid>(),
                ComponentType.ReadWrite<UpRightNeighboorChunkInGrid>(),
            });
            EntityManager.CreateEntity(archetype, counter);
            var dataHashMap = new NativeHashMap<int, ulong>(counter,Allocator.TempJob);
            foreach (var cell in Data.GridCellArray)
            {
                dataHashMap.Add(cell.Index, cell.Value);
            }
            Dependency = new SetGridIndexForNewOnes()
            {
                UniGridData = dataHashMap.AsReadOnly()
            }.ScheduleParallel(Dependency);
            Dependency = dataHashMap.Dispose(Dependency);
        }
        private partial struct SetGridIndexForNewOnes : IJobEntity
        {
            [ReadOnly] public NativeHashMap<int,ulong>.ReadOnly UniGridData;
            public void Execute([EntityIndexInQuery] int indexInt, ref GridIndex indexEntity, ref ActualChunkInGridData gridData)
            {
                indexEntity.index = indexInt;
                ulong cellValue = 0;
                if(!UniGridData.TryGetValue(indexInt, out cellValue))cellValue = 0;
                gridData.Value = cellValue;
            }
        }

        public void LoadSet(GridFullData data)
        {
            Data = data;
            this.Enabled = true;
        }
        [System.Serializable]
        public struct GridFullData
        {
            public readonly List<GridCellData> GridCellArray;
            public readonly int2 GridSize;
            public GridFullData(int2 gridSize, List<GridCellData> gridCellArray)
            {
                GridSize = gridSize;
                GridCellArray = gridCellArray;
            }
        }
        [System.Serializable]
        public struct GridCellData
        {
            public readonly ulong Value;
            public readonly int Index;
            public GridCellData(ulong value, int index)
            {
                Value = value;
                Index = index;
            }
        }
    }
}