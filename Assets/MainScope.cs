using System.Collections;
using System.Collections.Generic;
using GameOfLife.ECS.IOSystems;
using GameOfLife.ECS.Render;
using MessagePipe;
using Unity.Entities;
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
            var options = builder.RegisterMessagePipe(/* configure option */);
            builder.RegisterMessageBroker<SendEventSimulationIsRunningSystem.SimulationRunEvent>(options);
            builder.RegisterMessageBroker<SendEventSimulationGridSizeChangeSystem.GridSizeChangeEvent>(options);

            builder.RegisterSystemFromDefaultWorld<RenderBackgroundViewSystem>();
            builder.RegisterSystemFromDefaultWorld<FixedStepSimulationSystemGroup>();
            builder.RegisterSystemFromDefaultWorld<LoadBaseGridGivenSizeSystem>();
            builder.RegisterSystemFromDefaultWorld<ActiveDeactivateSystem>();
            builder.RegisterSystemFromDefaultWorld<SendEventSimulationIsRunningSystem>();
            builder.RegisterSystemFromDefaultWorld<SendEventSimulationGridSizeChangeSystem>();
            builder.RegisterSystemFromDefaultWorld<RunSimulationOnceSystem>();


        }
    }
}
