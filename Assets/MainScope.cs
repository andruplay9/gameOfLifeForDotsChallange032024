using System.Collections;
using System.Collections.Generic;
using GameOfLife.ECS.Render;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameOfLife
{
    
    public class MainScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterSystemFromDefaultWorld<RenderBackgroundViewSystem>();
        }
    }
}
