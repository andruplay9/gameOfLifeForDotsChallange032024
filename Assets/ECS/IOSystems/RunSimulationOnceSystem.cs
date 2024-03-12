using Unity.Collections;
using Unity.Entities;

namespace GameOfLife.ECS.IOSystems
{
    public partial class RunSimulationOnceSystem : SystemBase
    {
        private EntityQuery _querry;
        protected override void OnCreate()
        {
            base.OnCreate();
            this.Enabled = false;
            _querry = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ActualChunkInGridData, GridIndex>()
                .Build(this);
        }
        protected override void OnUpdate()
        {
            if (SystemAPI.HasSingleton<DeactivateNextFrame>())
            {
                this.Enabled = false;
                EntityManager.RemoveComponent<RunSimulationTag>(SystemAPI.GetSingletonEntity<GlobalSingletonTag>());
                EntityManager.RemoveComponent<DeactivateNextFrame>(SystemAPI.GetSingletonEntity<GlobalSingletonTag>());
                return;
            }else if (!SystemAPI.HasSingleton<RunSimulationTag>() && !_querry.IsEmpty)
            {
                EntityManager.AddComponent<RunSimulationTag>(SystemAPI.GetSingletonEntity<GlobalSingletonTag>());
                EntityManager.AddComponent<DeactivateNextFrame>(SystemAPI.GetSingletonEntity<GlobalSingletonTag>());
            }
            else
            {
                this.Enabled = false;
            }
        }
    }
}