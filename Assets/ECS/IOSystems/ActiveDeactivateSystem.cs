using Unity.Collections;
using Unity.Entities;

namespace GameOfLife.ECS.IOSystems
{
    [UpdateInGroup(typeof(GameOfLifeCrationAndIOSystemGroupSystem))]
    public partial class ActiveDeactivateSystem : SystemBase
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
            this.Enabled = false;
            var test = !SystemAPI.HasSingleton<RunSimulationTag>();
            if (test)
            {
                EntityManager.AddComponent<RunSimulationTag>(SystemAPI.GetSingletonEntity<GlobalSingletonTag>());
            }
            else
            {
                EntityManager.RemoveComponent<RunSimulationTag>(SystemAPI.GetSingletonEntity<GlobalSingletonTag>());
            }
        }
/// <summary>
/// active or deactivate simulation
/// </summary>
/// <param name="isTrue"></param>
/// <returns>return false if this method dont change anythink (if run or not or no cells to simulate)</returns>
        public bool SetSimulationToRun(bool isTrue = true)
        {
            var test = SystemAPI.HasSingleton<RunSimulationTag>();
            if (_querry.IsEmpty) return false;
            if (test == isTrue) return false;
            this.Enabled = true;
            return true;
        }
    }
}