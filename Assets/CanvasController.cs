using System;
using System.IO;
using GameOfLife.ECS.IOSystems;
using GameOfLife.ECS.Render;
using MessagePipe;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;
using VContainer;
using Newtonsoft.Json;

namespace GameOfLife
{
    public class CanvasController : MonoBehaviour
    {
        private ISubscriber<SendEventSimulationGridSizeChangeSystem.GridSizeChangeEvent> _subscriberGridChange;
        private ISubscriber<SendEventSimulationIsRunningSystem.SimulationRunEvent> _subscriberRunning;
        private ISubscriber<LoadFromDataGridGivenSizeSystem.GridFullData> _subscribeSave;
        private IPublisher<LoadFromDataGridGivenSizeSystem.GridFullData> _publisheSave;

        private ActiveDeactivateSystem _activeDeactivateSystem;
        private LoadBaseGridGivenSizeSystem _loadBaseGridGivenSizeSystem;
        private FixedStepSimulationSystemGroup _fixedStepSimulationSystemGroup;
        private RunSimulationOnceSystem _runSimulationOnceSystem;
        private RenderBackgroundViewSystem _renderBackgroundViewSystem;
        private LoadFromDataGridGivenSizeSystem _loadFromDataGridGivenSizeSystem;
        private SavemDataGridSystem _savemDataGridSystem;


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

        [SerializeField] private TMP_InputField _fileText;
        [SerializeField] private Button _importButton;
        [SerializeField] private Button _exportButton;

        [SerializeField] private Toggle _runRender;

        [SerializeField] private Slider _frameSkipSlider;
        [SerializeField] private TextMeshProUGUI _frameSkiText;


        [Inject]
        public void Construct(FixedStepSimulationSystemGroup fixedStepSimulationSystemGroup, LoadBaseGridGivenSizeSystem loadBaseGridGivenSizeSystem,
            ActiveDeactivateSystem activeDeactivateSystem, ISubscriber<SendEventSimulationIsRunningSystem.SimulationRunEvent> subscriberRunning, 
            IPublisher<SendEventSimulationIsRunningSystem.SimulationRunEvent> publisherRunning, ISubscriber<SendEventSimulationGridSizeChangeSystem.GridSizeChangeEvent> subscriberGridChange,
            IPublisher<SendEventSimulationGridSizeChangeSystem.GridSizeChangeEvent> publisherGridChange, SendEventSimulationGridSizeChangeSystem sendEventSimulationGridSizeChangeSystem,
            SendEventSimulationIsRunningSystem sendEventSimulationIsRunningSystem, RunSimulationOnceSystem runSimulationOnceSystem, SavemDataGridSystem savemDataGridSystem,
            LoadFromDataGridGivenSizeSystem loadFromDataGridGivenSizeSystem, RenderBackgroundViewSystem renderBackgroundViewSystem,
            ISubscriber<LoadFromDataGridGivenSizeSystem.GridFullData> subscribeSave, IPublisher<LoadFromDataGridGivenSizeSystem.GridFullData> publisheSave)
        {
            sendEventSimulationGridSizeChangeSystem.Set(publisherGridChange);
            sendEventSimulationIsRunningSystem.Set(publisherRunning);
            _subscribeSave = subscribeSave;
            _publisheSave = publisheSave;
            _subscriberGridChange = subscriberGridChange;
            _subscriberRunning = subscriberRunning;
            _activeDeactivateSystem = activeDeactivateSystem;
            _loadBaseGridGivenSizeSystem = loadBaseGridGivenSizeSystem;
            _fixedStepSimulationSystemGroup = fixedStepSimulationSystemGroup;
            _runSimulationOnceSystem = runSimulationOnceSystem;
            _renderBackgroundViewSystem = renderBackgroundViewSystem;
            _savemDataGridSystem = savemDataGridSystem;
            _loadFromDataGridGivenSizeSystem = loadFromDataGridGivenSizeSystem;
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
            
            _importButton.onClick.AddListener(() =>
            {
                string path =Application.streamingAssetsPath + "/"+_fileText.text;
                if(!File.Exists(path))return;
                UriBuilder uri = new UriBuilder(path);
                UnityWebRequest www = UnityWebRequest.Get(uri.Uri);
                var ww=www.SendWebRequest();

                ww.completed += (data) =>
                {
                    string json = www.downloadHandler.text;
                    var itemsContent = JsonConvert.DeserializeObject<LoadFromDataGridGivenSizeSystem.GridFullData>(json);
                    _loadFromDataGridGivenSizeSystem.LoadSet(itemsContent);
                };
            });
            _exportButton.onClick.AddListener(() =>
            {
                _savemDataGridSystem.SaveGridActivate(_publisheSave);
            });
            _subscribeSave.Subscribe((eventData) =>
            {
                string path =Application.streamingAssetsPath + "/"+_fileText.text;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                using (var file= File.CreateText(path))
                {
                    file.Write(JsonConvert.SerializeObject(eventData));
                }
     
                Debug.Log(JsonConvert.SerializeObject(eventData));
                PlayerPrefs.SetString("ExportetData",JsonConvert.SerializeObject(eventData));
            });
            

            _showMenu.onValueChanged.AddListener((value) =>
            {
                _menuView.SetActive(value);
            });
            _showGraphy.onValueChanged.AddListener((value) =>
            {
                _graphy.SetActive(value);
            });
            _runRender.onValueChanged.AddListener((value)=>_renderBackgroundViewSystem.Enabled=value);
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
            _frameSkipSlider.onValueChanged.AddListener((value) =>
            {
                _frameSkiText.text = value.ToString();
                _renderBackgroundViewSystem.UpdateFrameSkipper((int)value);
            });
        }

        private void Update()
        {
            if(Keyboard.current.escapeKey.wasReleasedThisFrame)
                Application.Quit();
        }
    }
}
