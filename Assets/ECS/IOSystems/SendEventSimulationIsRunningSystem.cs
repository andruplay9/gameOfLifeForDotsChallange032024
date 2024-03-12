using MessagePipe;
using Unity.Entities;

namespace GameOfLife.ECS.IOSystems
{
    [UpdateInGroup(typeof(GameOfLifeCrationAndIOSystemGroupSystem))]
    [UpdateAfter(typeof(ActiveDeactivateSystem))]
    public partial class SendEventSimulationIsRunningSystem : SystemBase
    {
        public struct SimulationRunEvent
        {
            public bool IsRunning { get; }

            public SimulationRunEvent(bool isRunning)
            {
                IsRunning = isRunning;
            }
        }

        private IPublisher<SimulationRunEvent> _publisher;
        private bool _isSet;
        private bool _wasActive = false;
        protected override void OnUpdate()
        {
            if(!_isSet)return;
            var isRunning = SystemAPI.HasSingleton<RunSimulationTag>();
            if(_wasActive==isRunning)return;
            _publisher.Publish(new SimulationRunEvent(isRunning));
            _wasActive = isRunning;
        }

        public void Set(IPublisher<SimulationRunEvent> publisher)
        {
            _publisher = publisher;
            _isSet = true;
        }
    }
}