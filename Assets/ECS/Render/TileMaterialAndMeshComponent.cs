using Unity.Entities;
using UnityEngine;

namespace GameOfLife.ECS.Render
{
    public class TileMaterialAndMeshComponent : IComponentData
    {
        public Material Mat;
        public Mesh Mesh;
    }
}