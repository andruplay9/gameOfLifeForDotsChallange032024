using System;
using Unity.Entities;

namespace GameOfLife.ECS.Render
{
    public class BRG_ContainerComponent : IComponentData, IDisposable
    {
        public BRG_Container Value;

        public void Dispose()
        {
            if (Value.isInitialized)
            {
                Value.Shutdown();
            }
        }
    }
}