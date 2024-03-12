using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameOfLife.ECS
{
    [UpdateInGroup(typeof(GameOfLifeSimulationSystemGroupSystem))]
    [UpdateAfter(typeof(CalculateBordersOfChunkSystem))]
    public partial struct CalculateNextGenSystem : ISystem
    {
        private EntityQuery _querry;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _querry = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ActualChunkInGridData, DownBorderOfChunk, UpBorderOfChunk, LeftBorderOfChunk,
                    RightBorderOfChunk>()
                .Build(ref state);
            state.RequireForUpdate(_querry);
            state.RequireForUpdate<GridSizeSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var size = SystemAPI.GetSingleton<GridSizeSingleton>().size;
            var counter = size.x * size.y;
            var downborderhashmap = new NativeArray<int>(counter, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var upborderhashmap = new NativeArray<int>(counter, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var leftborderhashmap = new NativeArray<ulong>(counter, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var rightborderhashmap = new NativeArray<ulong>(counter, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            state.Dependency=new GetDataToCalculateNextGen()
            {
                hashMapDown = downborderhashmap,
                hashMapUp = upborderhashmap,
                hashMapleft = leftborderhashmap,
                hashMapright = rightborderhashmap,
            }.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
            //state.Dependency=
            new CalculateNextGeneretionForChunkJob()
                {

                    hashMapUp = upborderhashmap.AsReadOnly(),
                    hashMapDown = downborderhashmap.AsReadOnly(),
                    hashMapright = rightborderhashmap.AsReadOnly(),
                    hashMapleft = leftborderhashmap.AsReadOnly()

                }
                .Run();
                    //.ScheduleParallel(state.Dependency);
            state.Dependency=downborderhashmap.Dispose(state.Dependency);
            state.Dependency=upborderhashmap.Dispose(state.Dependency);

            state.Dependency=leftborderhashmap.Dispose(state.Dependency);
            state.Dependency=rightborderhashmap.Dispose(state.Dependency);

        }
        [BurstCompile]
        private partial struct GetDataToCalculateNextGen:IJobEntity
        {
            [NativeDisableParallelForRestriction] public NativeArray<int> hashMapDown;
            [NativeDisableParallelForRestriction] public NativeArray< int> hashMapUp;
            [NativeDisableParallelForRestriction] public NativeArray<ulong> hashMapleft;
            [NativeDisableParallelForRestriction] public NativeArray<ulong> hashMapright;

            public void Execute(GridIndex index, DownBorderOfChunk downBorderOfChunk, UpBorderOfChunk upBorderOfChunk, LeftBorderOfChunk leftBorderOfChunk, RightBorderOfChunk rightBorderOfChunk)
            {
                hashMapDown[index.index]=downBorderOfChunk.Value;
                hashMapUp[index.index]=upBorderOfChunk.Value;
                hashMapleft[index.index]=leftBorderOfChunk.Value;
                hashMapright[index.index]=rightBorderOfChunk.Value;
            }
        }
        private partial struct CalculateNextGeneretionForChunkJob :IJobEntity
        {
            [ReadOnly] public NativeArray<int>.ReadOnly hashMapUp;
            [ReadOnly] public NativeArray<int>.ReadOnly hashMapDown;
            [ReadOnly] public NativeArray<ulong>.ReadOnly hashMapleft;
            [ReadOnly] public NativeArray<ulong>.ReadOnly hashMapright;
            
            public void Execute(ref ActualChunkInGridData chunk, in LeftNeighboorChunkInGrid leftNeighboorChunkInGrid,
                in RightNeighboorChunkInGrid rightNeighboorChunkInGrid, in UpNeighboorChunkInGrid upNeighboorChunkInGrid, 
                in DownNeighboorChunkInGrid downNeighboorChunkInGrid, in GridIndex index)
            {
                var upBorderOfDownNeighbor = (ulong)hashMapUp[downNeighboorChunkInGrid.Value];
                var downBorderOfUpNeighbor =(ulong)hashMapDown[upNeighboorChunkInGrid.Value];
                var leftBorderRightNeighbor = new NativeBitArray(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                leftBorderRightNeighbor.SetBits(0,hashMapleft[rightNeighboorChunkInGrid.Value],8);
                var rightBorderLeftNeighbor = new NativeBitArray(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                rightBorderLeftNeighbor.SetBits(0,hashMapright[leftNeighboorChunkInGrid.Value],8);
                var chunkValue = chunk.Value;
                var leftMoved = chunkValue<< 8;
                leftMoved += upBorderOfDownNeighbor;
                var rightMoved=chunkValue>> 8;
                rightMoved += downBorderOfUpNeighbor << 56;
    
                var byteArray = BitConverter.GetBytes(chunkValue);
                byte[] byteArray2 = new byte[byteArray.Length];
                byteArray.CopyTo(byteArray2,0);
                for (int i = 0; i < byteArray.Length; i++)
                {
                    byteArray[i]=(byte)((int)byteArray[i] >> 1);
                    byteArray[i] += leftBorderRightNeighbor.IsSet(i) ? (byte)(1<<7) : (byte)0;
                }
                for (int i = 0; i < byteArray2.Length; i++)
                {
                    byteArray2[i]=(byte)((int)byteArray2[i] << 1);
                    byteArray2[i] += rightBorderLeftNeighbor.IsSet(i) ? (byte)1 : (byte)0;
                }
                ulong upShift = BitConverter.ToUInt64(byteArray, 0);
                ulong downShift = BitConverter.ToUInt64(byteArray2, 0);
                ulong rightLeft = rightMoved & leftMoved;
                ulong upDown = upShift & downShift;
                ulong test2Neighbors = (upDown ^ (upShift & leftMoved) ^ (upShift & rightMoved) ^ (downShift & leftMoved)
                                        ^ (downShift & rightMoved) ^ rightLeft) & chunkValue;
                ulong test3Neighbors = upDown & (leftMoved ^ rightMoved) | rightLeft & (upShift ^ downShift);
                
                Debug.Log(index.index+"\n"+TestLogic.SpliceText(test2Neighbors)+"  \n"+TestLogic.SpliceText(test3Neighbors));
                Debug.Log(downBorderOfUpNeighbor+" "+upBorderOfDownNeighbor);
                Debug.Log(index.index+"\n"+TestLogic.SpliceText(leftMoved)+"  \n"+TestLogic.SpliceText(rightMoved));
                Debug.Log(index.index+"\n"+TestLogic.SpliceText(upShift)+"  \n"+TestLogic.SpliceText(downShift));

                chunk.Value = test2Neighbors | test3Neighbors;

            }
        }
    }
}