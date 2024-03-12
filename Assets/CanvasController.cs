using System;
using GameOfLife.ECS.IOSystems;
using MessagePipe;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace GameOfLife
{
    public class CanvasController : MonoBehaviour
    {
        private ISubscriber<SendEventSimulationGridSizeChangeSystem.GridSizeChangeEvent> _subscriberGridChange;
        private ISubscriber<SendEventSimulationIsRunningSystem.SimulationRunEvent> _subscriberRunning;
        private ActiveDeactivateSystem _activeDeactivateSystem;
        private LoadBaseGridGivenSizeSystem _loadBaseGridGivenSizeSystem;
        private FixedStepSimulationSystemGroup _fixedStepSimulationSystemGroup;
        private RunSimulationOnceSystem _runSimulationOnceSystem;

        [SerializeField] private Toggle _showMenu;
        [SerializeField] private GameObject _menuView;
        [SerializeField] private Toggle _showGraphy;
        [SerializeField] private GameObject _graphy;
        [SerializeField] private Button _buttonRun;
        [SerializeField] private Button _runSimulationOnce;

        [SerializeField] private Button _buttonStop;
        [SerializeField] private TextMeshProUGUI _runText;
        [SerializeField] private TMP_InputField _xSize;
        [SerializeField] private TMP_InputField _ySize;
        [SerializeField] private Button _updateGrid;
        [SerializeField] private TextMeshProUGUI _gridText;


        [Inject]
        public void Construct(FixedStepSimulationSystemGroup fixedStepSimulationSystemGroup, LoadBaseGridGivenSizeSystem loadBaseGridGivenSizeSystem,
            ActiveDeactivateSystem activeDeactivateSystem, ISubscriber<SendEventSimulationIsRunningSystem.SimulationRunEvent> subscriberRunning, 
            IPublisher<SendEventSimulationIsRunningSystem.SimulationRunEvent> publisherRunning, ISubscriber<SendEventSimulationGridSizeChangeSystem.GridSizeChangeEvent> subscriberGridChange,
            IPublisher<SendEventSimulationGridSizeChangeSystem.GridSizeChangeEvent> publisherGridChange, SendEventSimulationGridSizeChangeSystem sendEventSimulationGridSizeChangeSystem,
            SendEventSimulationIsRunningSystem sendEventSimulationIsRunningSystem, RunSimulationOnceSystem runSimulationOnceSystem)
        {
            sendEventSimulationGridSizeChangeSystem.Set(publisherGridChange);
            sendEventSimulationIsRunningSystem.Set(publisherRunning);
            _subscriberGridChange = subscriberGridChange;
            _subscriberRunning = subscriberRunning;
            _activeDeactivateSystem = activeDeactivateSystem;
            _loadBaseGridGivenSizeSystem = loadBaseGridGivenSizeSystem;
            _fixedStepSimulationSystemGroup = fixedStepSimulationSystemGroup;
            _runSimulationOnceSystem = runSimulationOnceSystem;
            _runSimulationOnce.onClick.AddListener(() => { _runSimulationOnceSystem.Enabled = true;});
            _buttonRun.onClick.AddListener(() => { _activeDeactivateSystem.SetSimulationToRun(true);});
            _buttonStop.onClick.AddListener(() => { _activeDeactivateSystem.SetSimulationToRun(false);});
            _subscriberRunning.Subscribe((eventData) =>
            {
                var isRunning = eventData.IsRunning;
                _buttonRun.enabled = !isRunning;
                _runSimulationOnce.enabled = !isRunning;
                _buttonStop.enabled = isRunning;
                _runText.text = isRunning ? "Running" : "Stopped";
            });


            _showMenu.onValueChanged.AddListener((value) =>
            {
                _menuView.SetActive(value);
            });
            _showGraphy.onValueChanged.AddListener((value) =>
            {
                _graphy.SetActive(value);
            });
            _updateGrid.onClick.AddListener(() =>
            {
                int valueX = 0, valueY = 0;
                var firstNumber = int.TryParse(_xSize.text, out valueX);
                var secondNumber = int.TryParse(_xSize.text, out valueY);
                if(!firstNumber || !secondNumber)return;
                if(valueX<=0 || valueY<=0)return;
                _loadBaseGridGivenSizeSystem.SetNewGridSize(new int2(valueX,valueY));
            });
            _subscriberGridChange.Subscribe((eventData) =>
            {
                var size = eventData.Size;
                _xSize.text = size.x.ToString();
                _ySize.text = size.y.ToString();
                _gridText.text = _xSize.text + "x" + _ySize.text;
            });
            
        }

        private void Update()
        {
            if(Keyboard.current.escapeKey.wasReleasedThisFrame)
                Application.Quit();
        }
    }
}
