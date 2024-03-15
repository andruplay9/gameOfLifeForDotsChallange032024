using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameOfLife.ECS.Render
{
    [UpdateInGroup(typeof(GameOfLifeSystemGroupSystem), OrderLast = true)]
    [UpdateAfter(typeof(GameOfLifeSimulationSystemGroupSystem))]
    [BurstCompile]
    public partial class RenderBackgroundViewSystem : SystemBase
    {
        private EntityQuery _querry;
        private static readonly Quaternion rot = new Quaternion(0.707106829f, 0, 0, 0.707106829f);
        private static readonly float4 colorA = new float4(0, 1, 0, 1);
        private static readonly float4 colorB = new float4(1, 0, 0, 1);

        protected override void OnCreate()
        {
            base.OnCreate();
            _querry = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ActualChunkInGridData, GridIndex>()
                .Build(this);
            RequireForUpdate(_querry);
            RequireForUpdate<TileMaterialAndMeshComponent>();
            RequireForUpdate<GridSizeSingleton>();
        }
        protected override void OnUpdate()
        {
            var dataEntity = SystemAPI.GetSingleton<GridSizeSingleton>();
            var component = SystemAPI.ManagedAPI.GetSingleton<TileMaterialAndMeshComponent>();
            var brgContainer = SystemAPI.ManagedAPI.GetSingleton<BRG_ContainerComponent>().Value;
            var counter = _querry.CalculateEntityCount()*64;
            if (!brgContainer.isInitialized || counter > brgContainer.MaxInstanceCount)
            {
                if (brgContainer.isInitialized)
                {
                    brgContainer.Shutdown();
                }
                brgContainer.Init(component.Mesh, component.Mat, counter, false, true);
            }
            int totalGpuBufferSize;
            int alignedWindowSize;
            NativeArray<float4> sysmemBuffer = brgContainer.GetSysmemBuffer(out totalGpuBufferSize, out alignedWindowSize);
            var _maxInstancePerWindow = alignedWindowSize / 112;
            var _windowSizeInFloat4 = alignedWindowSize / 16;
            Dependency=new DeSpawnOldTilesJob()
            {
                sysmemBuffer = sysmemBuffer,
                maxInstancePerWindow = _maxInstancePerWindow,
                windowSizeInFloat4 = _windowSizeInFloat4,
                gridColumnCount = dataEntity.size.x,
            }.ScheduleParallel(Dependency);
            Dependency.Complete();
            brgContainer.UploadGpuData(counter);
        }
        struct PackedMatrix
        {
            public float c0x;//scalex
            public float c0y;
            public float c0z;
            public float c1x;
            public float c1y;//scaley
            public float c1z;
            public float c2x;
            public float c2y;
            public float c2z;//scalez
            public float c3x;//posx
            public float c3y;//posy
            public float c3z;//posz

            public PackedMatrix(Matrix4x4 m)
            {
                c0x = m.m00;
                c0y = m.m10;
                c0z = m.m20;
                c1x = m.m01;
                c1y = m.m11;
                c1z = m.m21;
                c2x = m.m02;
                c2y = m.m12;
                c2z = m.m22;
                c3x = m.m03;
                c3y = m.m13;
                c3z = m.m23;
            }
        }

        [BurstCompile]
        private partial struct DeSpawnOldTilesJob : IJobEntity
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<float4> sysmemBuffer;

            [ReadOnly] public int maxInstancePerWindow;
            [ReadOnly] public int windowSizeInFloat4;
            [ReadOnly] public int gridColumnCount;

            public void Execute([EntityIndexInQuery] int index, in ActualChunkInGridData gridData, in GridIndex gridIndex)
            {
                int i;
                int windowId;
                PackedMatrix matrix;
                int windowOffsetInFloat4;
                PackedMatrix reverseMatrix;
                index = index * 64;
                float3 pos;
                NativeBitArray bitArray =
                    new NativeBitArray(64, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                bitArray.SetBits(0, gridData.Value, 64);

                for (int j = 0; j < 64; j++)
                {
                    pos=float3.zero;
                    pos.x = gridIndex.index % gridColumnCount * 8;
                    pos.z = gridIndex.index / gridColumnCount * 8;
                    pos.x -= j % 8;
                    pos.z -= j / 8;
                    pos.x += 0.5f;
                    pos.z += 0.5f;
                    pos.z *= -1;
                    var matrixPos = Matrix4x4.TRS(pos, rot, Vector3.one*0.99f);
                    windowId = System.Math.DivRem(index+j, maxInstancePerWindow, out i);
                    windowOffsetInFloat4 = windowId * windowSizeInFloat4;
                    matrix = new PackedMatrix(matrixPos);

                    reverseMatrix = new PackedMatrix( math.inverse(matrixPos));

                    sysmemBuffer[(windowOffsetInFloat4 + i * 3 + 0)] = new float4(matrix.c0x,matrix.c0y,matrix.c0z,matrix.c1x);
                    sysmemBuffer[(windowOffsetInFloat4 + i * 3 + 1)] = new float4(matrix.c1y,matrix.c1z,matrix.c2x,matrix.c2y);
                    sysmemBuffer[(windowOffsetInFloat4 + i * 3 + 2)] = new float4(matrix.c2z,matrix.c3x,matrix.c3y,matrix.c3z);
            
                    sysmemBuffer[(windowOffsetInFloat4 + maxInstancePerWindow * 3 * 1 + i * 3 + 0)] = new float4(matrix.c0x,matrix.c0y,matrix.c0z,matrix.c1x);
                    sysmemBuffer[(windowOffsetInFloat4 + maxInstancePerWindow * 3 * 1 + i * 3 + 1)] = new float4(matrix.c1y,matrix.c1z,matrix.c2x,matrix.c2y);
                    sysmemBuffer[(windowOffsetInFloat4 + maxInstancePerWindow * 3 * 1 + i * 3 + 2)] = new float4(matrix.c2z,matrix.c3x,matrix.c3y,matrix.c3z);
                    sysmemBuffer[windowOffsetInFloat4 + maxInstancePerWindow * 3 * 2 + i] =bitArray.IsSet(j)?colorA:colorB;
                }


            }
            [BurstCompile]
            private static bool IsBitSet(byte b, int pos)
            {
                return (b & (1 << pos)) != 0;
            }
        }
    }
}