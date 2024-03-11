using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace GameOfLife.ECS
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(CalculateConnectionsBetweenChunksSystems))]

    public partial struct CalculateBordersOfChunkSystem : ISystem
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
            state.Dependency = 
                new CalculateBordersOfChunkJob()
                    .ScheduleParallel(state.Dependency);
        }
        [BurstCompile]
        private partial struct CalculateBordersOfChunkJob : IJobEntity
        {
            private const ulong Int16Ones = 65535;
            public void Execute(ref DownBorderOfChunk downBorder, ref UpBorderOfChunk upBorder, ref LeftBorderOfChunk  leftBorder,
                ref RightBorderOfChunk rightBorder, in ActualChunkInGridData chunk, in GridIndex index)
            {
                upBorder.Value = (int)(chunk.Value >> 56);
                downBorder.Value = (int)(chunk.Value & Int16Ones);
                var byteArray = new NativeBitArray(64, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                byteArray.SetBits(0,chunk.Value,64);
                var leftBorderArray =new NativeBitArray(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var rightBorderArray =new NativeBitArray(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                for (int i = 0; i < 8; i++)
                {
                    rightBorderArray.Set(i,byteArray.IsSet(i*8));
                    leftBorderArray.Set(i,byteArray.IsSet((i+1)*8-1));
                }

                leftBorder.Value = rightBorderArray.GetBits(0, 8);
                rightBorder.Value = leftBorderArray.GetBits(0, 8);
                leftBorderArray.Dispose();
                rightBorderArray.Dispose();
            }
        }
        
    }
}