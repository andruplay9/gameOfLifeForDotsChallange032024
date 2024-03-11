using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameOfLife.ECS
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
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
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var downborderhashmap = new NativeParallelHashMap<ulong, int>(6, state.WorldUpdateAllocator);
            var upborderhashmap = new NativeParallelHashMap<ulong, int>(6, state.WorldUpdateAllocator);
            var leftborderhashmap = new NativeParallelHashMap<ulong, ulong>(6, state.WorldUpdateAllocator);
            var rightborderhashmap = new NativeParallelHashMap<ulong, ulong>(6, state.WorldUpdateAllocator);

            state.Dependency=new GetDataToCalculateNextGen()
            {
                hashMapDown = downborderhashmap.AsParallelWriter(),
                hashMapUp = upborderhashmap.AsParallelWriter(),
                hashMapleft = leftborderhashmap.AsParallelWriter(),
                hashMapright = rightborderhashmap.AsParallelWriter(),
            }.ScheduleParallel(state.Dependency);
            state.Dependency=new CalculateNextGeneretionForChunkJob()
            {

                hashMapUp = upborderhashmap.AsReadOnly(),
                hashMapDown = downborderhashmap.AsReadOnly(),
                hashMapright = rightborderhashmap.AsReadOnly(),
                hashMapleft = leftborderhashmap.AsReadOnly()
                
            }.ScheduleParallel(state.Dependency);
            state.Dependency=downborderhashmap.Dispose(state.Dependency);
            state.Dependency=upborderhashmap.Dispose(state.Dependency);

            state.Dependency=leftborderhashmap.Dispose(state.Dependency);
            state.Dependency=rightborderhashmap.Dispose(state.Dependency);

        }
        private partial struct GetDataToCalculateNextGen:IJobEntity
        {
            [NativeDisableParallelForRestriction] public NativeParallelHashMap<ulong, int>.ParallelWriter hashMapDown;
            [NativeDisableParallelForRestriction] public NativeParallelHashMap<ulong, int>.ParallelWriter hashMapUp;
            [NativeDisableParallelForRestriction] public NativeParallelHashMap<ulong, ulong>.ParallelWriter hashMapleft;
            [NativeDisableParallelForRestriction] public NativeParallelHashMap<ulong, ulong>.ParallelWriter hashMapright;

            public void Execute(GridIndex index, DownBorderOfChunk downBorderOfChunk, UpBorderOfChunk upBorderOfChunk, LeftBorderOfChunk leftBorderOfChunk, RightBorderOfChunk rightBorderOfChunk)
            {
                hashMapDown.TryAdd(index.index, downBorderOfChunk.Value);
                hashMapUp.TryAdd(index.index, upBorderOfChunk.Value);
                hashMapleft.TryAdd(index.index, leftBorderOfChunk.Value);
                hashMapright.TryAdd(index.index, rightBorderOfChunk.Value);
            }
        }

        private partial struct CalculateNextGeneretionForChunkJob :IJobEntity
        {
            [ReadOnly] public NativeParallelHashMap<ulong, int>.ReadOnly hashMapUp;
            [ReadOnly] public NativeParallelHashMap<ulong, int>.ReadOnly hashMapDown;
            [ReadOnly] public NativeParallelHashMap<ulong, ulong>.ReadOnly hashMapleft;
            [ReadOnly] public NativeParallelHashMap<ulong, ulong>.ReadOnly hashMapright;
            
            public void Execute(ref ActualChunkInGridData chunk, in LeftNeighboorChunkInGrid leftNeighboorChunkInGrid,
                in RightNeighboorChunkInGrid rightNeighboorChunkInGrid, in UpNeighboorChunkInGrid upNeighboorChunkInGrid, 
                in DownNeighboorChunkInGrid downNeighboorChunkInGrid, in GridIndex index)
            {
                var upBorderOfDownNeighbor = (ulong)hashMapUp[downNeighboorChunkInGrid.Value];
                var downBorderOfUpNeighbor =(ulong) hashMapDown[upNeighboorChunkInGrid.Value];
                var leftBorderRightNeighbor = new NativeBitArray(8, Allocator.Temp);
                leftBorderRightNeighbor.SetBits(0,hashMapleft[rightNeighboorChunkInGrid.Value],8);
                var rightBorderLeftNeighbor = new NativeBitArray(8, Allocator.Temp);
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

                chunk.Value = test2Neighbors | test3Neighbors;

            }
        }
    }
}