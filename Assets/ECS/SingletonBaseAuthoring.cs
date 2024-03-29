﻿using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameOfLife.ECS
{
    public class SingletonBaseAuthoring : MonoBehaviour
    {
        private int2 GridSize = new int2(0, 0);

        private class SingletonBaseAuthoringBaker : Baker<SingletonBaseAuthoring>
        {
            public override void Bake(SingletonBaseAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GridSizeSingleton() { size = authoring.GridSize });
                AddComponent<GlobalSingletonTag>(entity);
            }
        }
    }
}