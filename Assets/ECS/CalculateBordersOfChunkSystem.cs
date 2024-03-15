using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace GameOfLife.ECS
{
    [UpdateInGroup(typeof(GameOfLifeSimulationSystemGroupSystem))]

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
                ref RightBorderOfChunk rightBorder, ref LeftUpBorderOfChunk leftUpBorderOfChunk, ref LeftDownBorderOfChunk leftDownBorderOfChunk,
                    ref RightUpBorderOfChunk rightUpBorderOfChunk, ref RightDownBorderOfChunk rightDownBorderOfChunk, in ActualChunkInGridData chunk)
            {
                upBorder.Value = (int)(chunk.Value >> 56);
                downBorder.Value = (int)(chunk.Value & 255);
                var byteArray = new NativeBitArray(64, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                byteArray.SetBits(0,chunk.Value,64);
                rightDownBorderOfChunk.Value = byteArray.IsSet(0);
                leftDownBorderOfChunk.Value = byteArray.IsSet(7);
                rightUpBorderOfChunk.Value = byteArray.IsSet(56);
                leftUpBorderOfChunk.Value = byteArray.IsSet(63);
                var leftBorderArray =new NativeBitArray(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var rightBorderArray =new NativeBitArray(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                for (int i = 0; i < 8; i++)
                {
                    leftBorderArray.Set(i,byteArray.IsSet(i*8));
                    rightBorderArray.Set(i,byteArray.IsSet((i+1)*8-1));
                }

                leftBorder.Value = rightBorderArray.GetBits(0, 8);
                rightBorder.Value = leftBorderArray.GetBits(0, 8);
                leftBorderArray.Dispose();
                rightBorderArray.Dispose();
                byteArray.Dispose();
            }
        }
        
    }
}