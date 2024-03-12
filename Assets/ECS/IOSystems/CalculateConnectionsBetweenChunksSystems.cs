using GameOfLife.ECS.IOSystems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace GameOfLife.ECS.IOSystems
{
    [UpdateInGroup(typeof(GameOfLifeCrationAndIOSystemGroupSystem))]
    [UpdateAfter(typeof(LoadBaseGridGivenSizeSystem))]
    public partial struct CalculateConnectionsBetweenChunksSystems : ISystem
    {
        private EntityQuery _querry;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _querry = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<GridIndex, LeftNeighboorChunkInGrid, RightNeighboorChunkInGrid, UpNeighboorChunkInGrid,
                    DownNeighboorChunkInGrid>()
                .Build(ref state);
            state.RequireForUpdate<GridSizeSingleton>();
            state.RequireForUpdate<RecalculateConnectionCommand>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.RemoveComponent<RecalculateConnectionCommand>(SystemAPI.GetSingletonEntity<GlobalSingletonTag>());
            var size = SystemAPI.GetSingleton<GridSizeSingleton>().size;

            state.Dependency = new SetNeighboorsReferences()
            {
                gridSize = size
            }.ScheduleParallel(state.Dependency);
        }
        
        private partial struct SetNeighboorsReferences : IJobEntity
        {
            [ReadOnly] public int2 gridSize;
            public void Execute(in GridIndex index, ref LeftNeighboorChunkInGrid leftNeighboor, ref RightNeighboorChunkInGrid rightNeighboor,
                ref UpNeighboorChunkInGrid upNeighboor, ref DownNeighboorChunkInGrid downNeighboor)
            {
                var gridSizeX = gridSize.x;
                var indexX = index.index % gridSizeX;
                var indexY = index.index / gridSizeX;
                if (indexX == 0)
                {
                    leftNeighboor.Value = indexY * gridSizeX + gridSizeX - 1;
                }
                else
                {
                    leftNeighboor.Value = indexY * gridSizeX + indexX - 1;
                }
                if (indexX == gridSizeX - 1)
                {
                    rightNeighboor.Value = indexY * gridSizeX;
                }
                else
                {
                    rightNeighboor.Value = indexY * gridSizeX + indexX + 1;
                }

                var gridSizeY =  gridSize.y;
                if (indexY== 0)
                {
                    upNeighboor.Value = (gridSizeY-1) * gridSizeX + indexX;
                }
                else
                {
                    upNeighboor.Value = (indexY-1) * gridSizeX + indexX;
                }
                if (indexY== gridSizeY - 1)
                {
                    downNeighboor.Value = indexX;
                }
                else
                {
                    downNeighboor.Value = (indexY+1) * gridSizeX + indexX;
                }

            }
        }
    }
}