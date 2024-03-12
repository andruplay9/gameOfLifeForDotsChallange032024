using MessagePipe;
using Unity.Entities;
using Unity.Mathematics;

namespace GameOfLife.ECS.IOSystems
{
    [UpdateInGroup(typeof(GameOfLifeCrationAndIOSystemGroupSystem))]
    [UpdateAfter(typeof(LoadBaseGridGivenSizeSystem))]
    public partial class SendEventSimulationGridSizeChangeSystem : SystemBase
    {
        public struct GridSizeChangeEvent
        {
            public int2 Size { get; }

            public GridSizeChangeEvent(int2 size)
            {
                Size = size;
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<GridSizeSingleton>();
        }

        private IPublisher<GridSizeChangeEvent> _publisher;
        private bool _isSet;
        private int2 _lastSize = int2.zero;
        protected override void OnUpdate()
        {
            if(!_isSet)return;
            var isRunning =SystemAPI.GetSingleton<GridSizeSingleton>().size;
            if(math.all(_lastSize==isRunning))return;
            _publisher.Publish(new GridSizeChangeEvent(isRunning));
            _lastSize = isRunning;
        }

        public void Set(IPublisher<GridSizeChangeEvent> publisher)
        {
            _publisher = publisher;
            _isSet = true;
        }
    }
}