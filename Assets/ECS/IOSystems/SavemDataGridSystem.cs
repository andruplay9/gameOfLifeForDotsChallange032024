using System.Collections.Generic;
using MessagePipe;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameOfLife.ECS.IOSystems
{
    [UpdateInGroup(typeof(GameOfLifeCrationAndIOSystemGroupSystem))]
    [UpdateAfter(typeof(ActiveDeactivateSystem))]

    public partial class SavemDataGridSystem : SystemBase
    {
        private EntityQuery _querry;
        private IPublisher<LoadFromDataGridGivenSizeSystem.GridFullData> _publisher;

        protected override void OnCreate()
        {
            base.OnCreate();
            this.Enabled = false;
            _querry = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ActualChunkInGridData, GridIndex>()
                .Build(this);
            RequireForUpdate(_querry);
            RequireForUpdate<GridSizeSingleton>();
        }
        protected override void OnUpdate()
        {
            this.Enabled = false;
            
            var counter = _querry.CalculateEntityCount();
            var gridSize =
                EntityManager.GetComponentData<GridSizeSingleton>(SystemAPI.GetSingletonEntity<GlobalSingletonTag>()).size;
            EntityManager.RemoveComponent<RunSimulationTag>(SystemAPI.GetSingletonEntity<GlobalSingletonTag>());
            if(counter==0)
                return;
            var dataHashMap = new NativeParallelHashMap<int, ulong>(counter,Allocator.TempJob);
            Dependency.Complete();
            Dependency = new SaveGridData()
            {
                UniGridData = dataHashMap.AsParallelWriter()
            }.ScheduleParallel(Dependency);
            Dependency.Complete();
            List<LoadFromDataGridGivenSizeSystem.GridCellData> array =
                new List<LoadFromDataGridGivenSizeSystem.GridCellData>();
            foreach (var key in dataHashMap.GetKeyArray(Allocator.Temp))
            {
                array.Add(new LoadFromDataGridGivenSizeSystem.GridCellData(dataHashMap[key], key));
            }
            LoadFromDataGridGivenSizeSystem.GridFullData outputData = new LoadFromDataGridGivenSizeSystem.GridFullData(gridSize,array);
            _publisher.Publish(outputData);
            
            Dependency = dataHashMap.Dispose(Dependency);
        }
        private partial struct SaveGridData : IJobEntity
        {
            public NativeParallelHashMap<int,ulong>.ParallelWriter UniGridData;
            public void Execute(in GridIndex indexEntity, in ActualChunkInGridData gridData)
            {
                if(gridData.Value==0)return;
                UniGridData.TryAdd(indexEntity.index, gridData.Value);
            }
        }

        public void SaveGridActivate(IPublisher<LoadFromDataGridGivenSizeSystem.GridFullData> publisher)
        {
            this.Enabled = true;
            _publisher = publisher;
        }
    }
}