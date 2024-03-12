using Unity.Entities;

namespace GameOfLife.ECS
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class GameOfLifeSystemGroupSystem : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
    [UpdateInGroup(typeof(GameOfLifeSystemGroupSystem))]
    public partial class GameOfLifeSimulationSystemGroupSystem : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<RunSimulationTag>();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
    [UpdateInGroup(typeof(GameOfLifeSystemGroupSystem))]
    [UpdateBefore(typeof(GameOfLifeSimulationSystemGroupSystem))]
    public partial class GameOfLifeCrationAndIOSystemGroupSystem : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}