﻿using Unity.Entities;

namespace GameOfLife.ECS
{
    public struct GridIndex : IComponentData
    {
        public int index;
    }
}