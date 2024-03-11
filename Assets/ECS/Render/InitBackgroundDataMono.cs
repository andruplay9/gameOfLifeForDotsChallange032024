using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using VContainer;

namespace GameOfLife.ECS.Render
{
    public class InitBackgroundDataMono : MonoBehaviour
    {
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _mat;

        [Inject]
        private void Construct(RenderBackgroundViewSystem renderBackgroundViewSystem)
        {
            renderBackgroundViewSystem.EntityManager.AddComponentObject(renderBackgroundViewSystem.SystemHandle, 
                new TileMaterialAndMeshComponent(){Mesh = _mesh, Mat = _mat});
            renderBackgroundViewSystem.EntityManager.AddComponentObject(renderBackgroundViewSystem.SystemHandle,
            new BRG_ContainerComponent(){Value = new BRG_Container()});
        }
    }
}

