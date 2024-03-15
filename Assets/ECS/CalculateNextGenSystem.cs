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
            
            var downLeftborderhashmap = new NativeArray<bool>(counter, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var downRightborderhashmap = new NativeArray<bool>(counter, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var upLeftborderhashmap = new NativeArray<bool>(counter, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var upRightborderhashmap = new NativeArray<bool>(counter, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);


            state.Dependency=new GetDataToCalculateNextGen()
            {
                hashMapDown = downborderhashmap,
                hashMapUp = upborderhashmap,
                hashMapleft = leftborderhashmap,
                hashMapright = rightborderhashmap,
                hashMapDownLeft = downLeftborderhashmap,
                hashMapUpLeft = upLeftborderhashmap,
                hashMapDownRight = downRightborderhashmap,
                hashMapUpRight = upRightborderhashmap
            }.ScheduleParallel(state.Dependency);
            //state.Dependency.Complete();
            state.Dependency=
            new CalculateNextGeneretionForChunkJob()
                {
                    hashMapUp = upborderhashmap.AsReadOnly(),
                    hashMapDown = downborderhashmap.AsReadOnly(),
                    hashMapright = rightborderhashmap.AsReadOnly(),
                    hashMapleft = leftborderhashmap.AsReadOnly(),
                    hashMapDownLeft = downLeftborderhashmap.AsReadOnly(),
                    hashMapDownRight = downRightborderhashmap.AsReadOnly(),
                    hashMapUpLeft = upLeftborderhashmap.AsReadOnly(),
                    hashMapUpRight = upRightborderhashmap.AsReadOnly(),
                }
                //.Run();
                    .ScheduleParallel(state.Dependency);
            state.Dependency=downborderhashmap.Dispose(state.Dependency);
            state.Dependency=upborderhashmap.Dispose(state.Dependency);

            state.Dependency=leftborderhashmap.Dispose(state.Dependency);
            state.Dependency=rightborderhashmap.Dispose(state.Dependency);
            state.Dependency=downLeftborderhashmap.Dispose(state.Dependency);
            state.Dependency=downRightborderhashmap.Dispose(state.Dependency);

            state.Dependency=upLeftborderhashmap.Dispose(state.Dependency);
            state.Dependency=upRightborderhashmap.Dispose(state.Dependency);

        }
        [BurstCompile]
        private partial struct GetDataToCalculateNextGen:IJobEntity
        {
            [NativeDisableParallelForRestriction] public NativeArray<int> hashMapDown;
            [NativeDisableParallelForRestriction] public NativeArray< int> hashMapUp;
            [NativeDisableParallelForRestriction] public NativeArray<ulong> hashMapleft;
            [NativeDisableParallelForRestriction] public NativeArray<ulong> hashMapright;
            [NativeDisableParallelForRestriction] public NativeArray<bool> hashMapDownLeft;
            [NativeDisableParallelForRestriction] public NativeArray<bool> hashMapUpLeft;
            [NativeDisableParallelForRestriction] public NativeArray<bool> hashMapDownRight;
            [NativeDisableParallelForRestriction] public NativeArray<bool> hashMapUpRight;

            public void Execute(in GridIndex index, in DownBorderOfChunk downBorderOfChunk, in UpBorderOfChunk upBorderOfChunk, in LeftBorderOfChunk leftBorderOfChunk, in RightBorderOfChunk rightBorderOfChunk,
               in LeftDownBorderOfChunk leftDownBorderOfChunk, in LeftUpBorderOfChunk leftUpBorderOfChunk, in RightUpBorderOfChunk rightUpBorderOfChunk, in RightDownBorderOfChunk rightDownBorderOfChunk)
            {
                hashMapDown[index.index]=downBorderOfChunk.Value;
                hashMapUp[index.index]=upBorderOfChunk.Value;
                hashMapleft[index.index]=leftBorderOfChunk.Value;
                hashMapright[index.index]=rightBorderOfChunk.Value;
                hashMapDownLeft[index.index] = leftDownBorderOfChunk.Value;
                hashMapDownRight[index.index] = rightDownBorderOfChunk.Value;
                hashMapUpLeft[index.index] = leftUpBorderOfChunk.Value;
                hashMapUpRight[index.index] = rightUpBorderOfChunk.Value;
            }
        }
        private partial struct CalculateNextGeneretionForChunkJob :IJobEntity
        {
            [ReadOnly] public NativeArray<int>.ReadOnly hashMapUp;
            [ReadOnly] public NativeArray<int>.ReadOnly hashMapDown;
            [ReadOnly] public NativeArray<ulong>.ReadOnly hashMapleft;
            [ReadOnly] public NativeArray<ulong>.ReadOnly hashMapright;
            [ReadOnly] public NativeArray<bool>.ReadOnly hashMapDownLeft;
            [ReadOnly] public NativeArray<bool>.ReadOnly hashMapUpLeft;
            [ReadOnly] public NativeArray<bool>.ReadOnly hashMapDownRight;
            [ReadOnly] public NativeArray<bool>.ReadOnly hashMapUpRight;
            
            public void Execute(ref ActualChunkInGridData chunk, in LeftNeighboorChunkInGrid leftNeighboorChunkInGrid,
                in RightNeighboorChunkInGrid rightNeighboorChunkInGrid, in UpNeighboorChunkInGrid upNeighboorChunkInGrid, 
                in DownNeighboorChunkInGrid downNeighboorChunkInGrid, in UpLeftNeighboorChunkInGrid upLeftNeighboorChunkInGrid,
                in DownLeftNeighboorChunkInGrid downLeftNeighboorChunkInGrid, UpRightNeighboorChunkInGrid upRightNeighboorChunkInGrid,
                DownRightNeighboorChunkInGrid downRightNeighboorChunkInGrid, in GridIndex index)
            {
                var upBorderOfDownNeighbor = (ulong)hashMapUp[downNeighboorChunkInGrid.Value];
                var downBorderOfUpNeighbor =(ulong)hashMapDown[upNeighboorChunkInGrid.Value];
                var leftBorderRightNeighbor = new NativeBitArray(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                leftBorderRightNeighbor.SetBits(0,hashMapleft[rightNeighboorChunkInGrid.Value],8);
                var rightBorderLeftNeighbor = new NativeBitArray(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                rightBorderLeftNeighbor.SetBits(0,hashMapright[leftNeighboorChunkInGrid.Value],8);
                var upleftPointOfDownRightNeighboor = hashMapUpLeft[downRightNeighboorChunkInGrid.Value];
                var uprightPointOfDownLeftNeighboor = hashMapUpRight[downLeftNeighboorChunkInGrid.Value];
                var downleftPointOfUpRightNeighboor = hashMapDownLeft[upRightNeighboorChunkInGrid.Value];
                var downrightPointOfUpLeftNeighboor = hashMapDownRight[upLeftNeighboorChunkInGrid.Value];
                
                var uprightBoardOfUpLeftNeighboor = (upBorderOfDownNeighbor >> 1) + (ulong)((uprightPointOfDownLeftNeighboor?1:0)<<7);
                var downrightBoardOfUpLeftNeighboor = (downBorderOfUpNeighbor >> 1) + (ulong)((downrightPointOfUpLeftNeighboor?1:0)<<7);
                
                var upleftBoardOfDownRightNeighboor = (upBorderOfDownNeighbor << 1) + ((ulong)(upleftPointOfDownRightNeighboor?1:0));
                var downleftBoardOfUpRightNeighboor = (downBorderOfUpNeighbor << 1) + ((ulong)(downleftPointOfUpRightNeighboor?1:0));

                var chunkValue = chunk.Value;
                var chunkOneDown = chunkValue<< 8;
                chunkOneDown += upBorderOfDownNeighbor;
                var chunkOneUp=chunkValue>> 8;
                chunkOneUp += downBorderOfUpNeighbor << 56;
                NativeArray<byte> byteArray =
                    new NativeArray<byte>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                NativeArray<byte> byteArray2 =
                    new NativeArray<byte>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                
                var byteToCrop = chunk.Value;
                for (int i = 0; i < byteArray.Length; i++)
                {
                    byteArray[i] =(byte) (byteToCrop & 255);
                    byteToCrop=byteToCrop >> 8;
                }
                byteArray2.CopyFrom(byteArray);
                for (int i = 0; i < byteArray.Length; i++)
                {
                    byteArray[i]=(byte)((int)byteArray[i] >> 1);
                    byteArray[i] += rightBorderLeftNeighbor.IsSet(i) ? (byte)(1<<7) : (byte)0;
                }
                for (int i = 0; i < byteArray2.Length; i++)
                {
                    byteArray2[i]=(byte)((int)byteArray2[i] << 1);
                    byteArray2[i] += leftBorderRightNeighbor.IsSet(i) ? (byte)1 : (byte)0;
                }
                ulong chunkOneLeft=0;
                ulong chunkOneRight=0;
                for (int i = 0; i < 8; i++)
                {
                    chunkOneLeft |= (ulong)byteArray[i] << (i * 8);
                }   
                for (int i = 0; i < 8; i++)
                {
                    chunkOneRight |= (ulong)byteArray2[i] << (i * 8);
                }

                ulong chunkOneUpLeft = (chunkOneLeft >> 8) + (downrightBoardOfUpLeftNeighboor << 56);
                ulong chunkOneUpRight = (chunkOneRight >> 8) + (downleftBoardOfUpRightNeighboor << 56);
                ulong chunkOneDownLeft = (chunkOneLeft << 8) + (uprightBoardOfUpLeftNeighboor); 
                ulong chunkOneDownRight = (chunkOneRight << 8) + (upleftBoardOfDownRightNeighboor);
                 
                var (test0neighbors1, test1neighbors1, test3Neighbors1, test2neighbors1) =
                    GetTestIn4(chunkOneUp, chunkOneDown, chunkOneLeft, chunkOneRight);
                var (test0neighbors2, test1neighbors2, test3Neighbors2, test2neighbors2) =
                    GetTestIn4(chunkOneUpLeft, chunkOneUpRight, chunkOneDownLeft, chunkOneDownRight);

     

                var test3Neighbors = (test1neighbors1 & test2neighbors2) | (test1neighbors2 & test2neighbors1) |
                                     (test0neighbors1 & test3Neighbors2) | (test0neighbors2 & test3Neighbors1);
                var test2Neighbors =
                    ((test1neighbors1 & test1neighbors2) | (test0neighbors1 & test2neighbors2) |
                     (test0neighbors2 & test2neighbors1)) & chunk.Value;
                /*if (index.index == 0)
                {
                    Debug.Log(index.index+"\n"+TestLogic.SpliceText(chunkOneDown)+"  \n"+TestLogic.SpliceText(chunkOneUp));
                    Debug.Log(index.index+"\n"+TestLogic.SpliceText(chunkOneLeft)+"  \n"+TestLogic.SpliceText(chunkOneRight));
                    Debug.Log(index.index+"\n"+TestLogic.SpliceText(chunkOneUpLeft)+"  \n"+TestLogic.SpliceText(chunkOneUpRight));
                    Debug.Log(index.index+"\n"+TestLogic.SpliceText(chunkOneDownLeft)+"  \n"+TestLogic.SpliceText(chunkOneDownRight));
                }*/
                chunk.Value = test2Neighbors | test3Neighbors;

            }
            [BurstCompile]
            private static (ulong test0neighbors, ulong test1neighbors, ulong test3Neighbors, ulong test2neighbors) 
                GetTestIn4(ulong chunkOneUp, ulong chunkOneDown, ulong chunkOneLeft, ulong chunkOneRight)
            {
                ulong upDown = chunkOneUp & chunkOneDown;
                ulong leftRight = chunkOneLeft & chunkOneRight;
                var leftDown = (chunkOneLeft & chunkOneDown);
                var leftUp = (chunkOneLeft & chunkOneUp);
                var rightDown = (chunkOneRight & chunkOneDown);
                var rightUp = (chunkOneRight & chunkOneUp);
                var downOrUp = chunkOneDown ^ chunkOneUp;
                var leftOrRight = (chunkOneLeft ^ chunkOneRight);
                
                ulong test0neighbors = ~chunkOneLeft & ~chunkOneRight & ~chunkOneUp & ~chunkOneDown;
                ulong test1neighbors = ~(upDown | leftRight) & (downOrUp ^ leftOrRight);
                ulong test3Neighbors = leftRight & downOrUp | upDown & leftOrRight;
                ulong test4neighbors = leftUp & rightDown;
                ulong test2neighbors = ~(test0neighbors | test1neighbors | test3Neighbors | test4neighbors);
                return (test0neighbors, test1neighbors, test3Neighbors, test2neighbors);
            }
        }
    }
}