using Common;
using CommonDialogs.Common;
using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.Codecs;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using static DbSchemaDecoder.Models.BruteForceViewModel;

namespace DbSchemaDecoder.Controllers
{
    public class BruteForceController : NotifyPropertyChangedImpl
    {
        ILogger _logger = Logging.Create<BruteForceController>();

        public enum BruteForceCalculatorType
        { 
            BruteForceUsingCaSchama = 0,
            BruteForce,
            BruteForceUsingExistingTables,
            BruteForceUnknownTableCount,
            BruteForceUsingExistingTableUnknownTableCount
        };

        public BruteForceViewModel ViewModel { get; set; } = new BruteForceViewModel();
        Util.WindowState _windowState;
        TimedThreadProcess<BruteForceParser>[] _timedProcess;

        public ICommand ComputeBruteForceCommand { get; private set; }
        public ICommand SaveResultCommand { get; private set; }
        public ICommand OnClickCommand { get; private set; }

        public BruteForceController(Util.WindowState windowState)
        {
            _logger.Information("Created");
            _windowState = windowState;
            _windowState.OnCaSchemaLoaded += (sender, caSchema) => { ViewModel.ColumnCount = caSchema.Count(); };

            _windowState.OnFileSelected += (sender, file) => 
            {
                _logger.Information($"File selected => {file.DbFile.FullPath}");
                Cancel(); 
                ViewModel.Values.Clear(); 
            };

            ComputeBruteForceCommand = new RelayCommand(OnCompute);
            SaveResultCommand = new RelayCommand(OnSave);
            OnClickCommand = new RelayCommand<ItemView>(OnItemDoubleClicked);

            ViewModel.RunningStatus = "Not run";
            ViewModel.CalculateButtonText = "Calculate";
            UpdateBruteForceDisplay();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.ComputeType))
                UpdateBruteForceDisplay();
        }

        void UpdateBruteForceDisplay()
        {
            _logger.Information($"Updating");

            if (_windowState.CaSchema != null)
                ViewModel.ColumnCount = _windowState.CaSchema.Count();

            var bruteForceMethod = (BruteForceCalculatorType)ViewModel.ComputeType;
            if (bruteForceMethod == BruteForceCalculatorType.BruteForce)
            {
                ViewModel.BruteForceColumnCountText = "No. Table Columns:";
                ViewModel.ColumnCountEditable = true;
                if (_windowState.DbSchemaFields != null)
                    ViewModel.ColumnCount = _windowState.DbSchemaFields.Count();
                return;
            }
            else if (bruteForceMethod == BruteForceCalculatorType.BruteForceUnknownTableCount)
            {
                ViewModel.BruteForceColumnCountText = "Max Columns:";
                ViewModel.ColumnCountEditable = true;
                if (_windowState.DbSchemaFields != null)
                    ViewModel.ColumnCount = _windowState.DbSchemaFields.Count();
                return;
            }
            else if(bruteForceMethod == BruteForceCalculatorType.BruteForceUsingCaSchama)
            {
                ViewModel.BruteForceColumnCountText = "No. Ca Columns:";
                ViewModel.ColumnCountEditable = false;
                return;
            }
            else if(bruteForceMethod == BruteForceCalculatorType.BruteForceUsingExistingTables)
            {
                ViewModel.BruteForceColumnCountText = "No.Total Table Columns:";
                if(_windowState.DbSchemaFields != null)
                    ViewModel.ColumnCount = _windowState.DbSchemaFields.Count();
                ViewModel.ColumnCountEditable = true;
                return;
            }
            else if (bruteForceMethod == BruteForceCalculatorType.BruteForceUsingExistingTableUnknownTableCount)
            {
                ViewModel.BruteForceColumnCountText = "No.Total Max Table Columns:";
                if (_windowState.DbSchemaFields != null)
                    ViewModel.ColumnCount = _windowState.DbSchemaFields.Count();
                ViewModel.ColumnCountEditable = true;
                return;
            }

            throw new NotImplementedException("Unknown compute type");
        }

        void OnItemDoubleClicked(ItemView clickedItem)
        {
            if (clickedItem == null)
                return;

            _logger.Information($"Item clicked {clickedItem.Value}");

            List<DbColumnDefinition> dbColumnDefinitions = new List<DbColumnDefinition>();
            for (int i = 0; i < clickedItem.Enums.Count; i++)
            {
                dbColumnDefinitions.Add(new DbColumnDefinition()
                {
                    Type = clickedItem.Enums[i],
                    Name = "Unknown" + i
                });
            }

            _windowState.DbSchemaFields = dbColumnDefinitions;
        }

        void OnCompute()
        {
            if (Cancel())
            {
                _logger.Information($"Computing cancled");
                return;
            }

            if (_windowState.SelectedFile == null)
            {
                _logger.Information($"No file selected");
                return;
            }

            _logger.Information($"Computing");
            var bruteForceMethod = (BruteForceCalculatorType)ViewModel.ComputeType;
            IBruteForceCombinationProvider combinationProvider = GetCombinationProvider(bruteForceMethod);
            if (combinationProvider == null)
                return;

            ViewModel.Values.Clear();
            ViewModel.PossibleCombinationsFound = "0";
            ViewModel.RunTime = "";
            ViewModel.RunningStatus = "Pre Calc";
            ViewModel.TotalPossibleCombinations = "";
            ViewModel.EvaluatedCombinations = "";
            ViewModel.SkippedEarlyCombinations = "";
            ViewModel.ComputedPerSec = "";
            ViewModel.PossibleFirstRows = "";
            ViewModel.CalculateButtonText = "Cancel";

            _viewHolder = new Dictionary<BruteForceParser, TempDisplayValues>();

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            DBFileHeader header = PackedFileDbCodec.readHeader(_windowState.SelectedFile.DbFile);
            var preCalc = new PreCalc();
            preCalc.PreCompute(_windowState.SelectedFile.DbFile.Data, header.Length);
            Mouse.OverrideCursor = null;

            ViewModel.RunningStatus = "Running";

            if (bruteForceMethod == BruteForceCalculatorType.BruteForceUnknownTableCount ||
                bruteForceMethod == BruteForceCalculatorType.BruteForceUsingExistingTableUnknownTableCount)
            {
                _possibleCombinations = 0;
                _timedProcess = new TimedThreadProcess<BruteForceParser>[ViewModel.ColumnCount];
                for (int i = 1; i < ViewModel.ColumnCount+1; i++)
                    _possibleCombinations += BruteForceParser.PossibleCombinations(i);

                for (int i = 1; i < ViewModel.ColumnCount + 1; i++)
                    BruteForce(preCalc, _windowState.SelectedFile, i, combinationProvider, i - 1);

                ViewModel.ColumnVariationsCompleted = "0/" + ViewModel.ColumnCount;
                ViewModel.TotalPossibleCombinations = _possibleCombinations.ToString("N0");
            }
            else
            {
                _possibleCombinations = BruteForceParser.PossibleCombinations(ViewModel.ColumnCount);
                _timedProcess = new TimedThreadProcess<BruteForceParser>[1];
                BruteForce(preCalc, _windowState.SelectedFile, ViewModel.ColumnCount, combinationProvider, 0);

                ViewModel.ColumnVariationsCompleted = "0/1";
                ViewModel.TotalPossibleCombinations = _possibleCombinations.ToString("N0"); ;
            }
        }

        void OnSave()
        {
            File.WriteAllLines(@"C:\temp\output.text", ViewModel.Values.Select(x => x.Value));
        }

        void BruteForce(PreCalc precalc, DataBaseFile file, int maxNumberOfFields, IBruteForceCombinationProvider combinationProvider, int threadIndex)
        {
            var bruteForceparser = new BruteForceParser(precalc, file, combinationProvider, maxNumberOfFields);
            bruteForceparser.OnCombintionFoundEvent += CombinationFoundEventHandler;
            _viewHolder.Add(bruteForceparser, new TempDisplayValues());
            _timedProcess[threadIndex] = new TimedThreadProcess<BruteForceParser>(bruteForceparser);
            _timedProcess[threadIndex].OnThreadCompletedEvent += BruteForceController_OnThreadCompletedEvent;
            _timedProcess[threadIndex].OnUpdate += BruteForceController_OnUpdate;
            _timedProcess[threadIndex].Start(bruteForceparser.Compute);
        }

        private void BruteForceController_OnUpdate(object sender, TimedThreadEvent<BruteForceParser> arg)
        {
            Update(arg);
        }
        private void BruteForceController_OnThreadCompletedEvent(object sender, TimedThreadEvent<BruteForceParser> arg)
        {
            var doneProcesses = _timedProcess
                .Where(x =>x != null)
                .Count(x => x.IsRunning == false);
            if (doneProcesses == _timedProcess.Length)
            {
                ViewModel.RunningStatus = "Done";
                ViewModel.CalculateButtonText = "Calculate";
            }
            ViewModel.ColumnVariationsCompleted = $"{doneProcesses}/{_timedProcess.Length}";

            _logger.Information($"Brute force compute completed");
            Update(arg);
        }

        IBruteForceCombinationProvider GetCombinationProvider(BruteForceCalculatorType type)
        {
            try
            {
                switch (type)
                {
                    case BruteForceCalculatorType.BruteForce:
                    case BruteForceCalculatorType.BruteForceUnknownTableCount:
                        return new AllCombinations();

                    case BruteForceCalculatorType.BruteForceUsingCaSchama:
                        return new CaTableCombinations(_windowState.CaSchema);

                    case BruteForceCalculatorType.BruteForceUsingExistingTables:
                    case BruteForceCalculatorType.BruteForceUsingExistingTableUnknownTableCount:
                        return new AppendTableCombinations(_windowState.DbSchemaFields.Select(x => x.Type).ToArray());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }

            _logger.Fatal($"Unknown compute type {type}");
            throw new NotImplementedException("Unknown compute type");
        }

        void CombinationFoundEventHandler(object sender, DbTypesEnum[] val)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var values = val.Select(x => ByteParserFactory.Create(x).TypeName);
                var valuesAsList = string.Join(", ", values);
                _logger.Information($"New combination found {valuesAsList}");

                lock(ViewModel)
                {
                    ViewModel.Values.Add(new ItemView()
                    {
                        Idx = ViewModel.Values.Count() + 1,
                        Value = valuesAsList,
                        Enums = val.ToList(),
                        Columns = val.Count(),
                    });

                    ViewModel.PossibleCombinationsFound = ViewModel.Values.Count().ToString();
                }
            });
        }

        class TempDisplayValues
        {
            public BigInteger EvaluatedCombinations { get; set; } = 0;
            public BigInteger SkippedEarlyCombinations { get; set; } = 0;
            public BigInteger ComputedPerSec { get; set; } = 0;
            public BigInteger PossibleFirstRows { get; set; } = 0;
        }

        Dictionary<BruteForceParser, TempDisplayValues> _viewHolder = new Dictionary<BruteForceParser, TempDisplayValues>();
        BigInteger _possibleCombinations = 0;
        void Update(TimedThreadEvent<BruteForceParser> arg)
        {
            lock (_viewHolder)
            {
                _logger.Information($"Updating after thead trigger");

                var parser = arg.TaskHandler;
                var current = _viewHolder[parser];
                current.EvaluatedCombinations = parser.EvaluatedCombinations;
                current.SkippedEarlyCombinations = parser.SkippedEarlyCombinations;
                current.PossibleFirstRows = parser.PossibleFirstRows;

                BigInteger EvaluatedCombinations = 0;
                BigInteger SkippedEarlyCombinations = 0;
                BigInteger PossibleFirstRows = 0;

                foreach (var item in _viewHolder.Values)
                {
                    EvaluatedCombinations += item.EvaluatedCombinations;
                    SkippedEarlyCombinations += item.SkippedEarlyCombinations;
                    PossibleFirstRows += item.PossibleFirstRows;
                }

                ViewModel.RunTime = (int)arg.Process.RunTimeInSec() + "s";

                ViewModel.EvaluatedCombinations = EvaluatedCombinations.ToString("N0");
                ViewModel.SkippedEarlyCombinations = SkippedEarlyCombinations.ToString("N0");
                ViewModel.PossibleFirstRows = PossibleFirstRows.ToString("N0");

                BigFloat bigFloatEvaluatedCombinations = new BigFloat(EvaluatedCombinations);
                BigFloat bigFloatTotal = new BigFloat(_possibleCombinations);

                var percentageDone = ((bigFloatEvaluatedCombinations / bigFloatTotal) * 100).ToString().Take(10);
                ViewModel.EstimatedRunTime = new string(percentageDone.ToArray()) + "%";
            }
        }

        public bool Cancel()
        {
            if (_timedProcess == null)
                return false;

            bool foundRunninProcess = false;
            for(int i = 0; i < _timedProcess.Length; i++)
            {
                if (_timedProcess[i].IsRunning)
                {
                    foundRunninProcess = true;
                    _timedProcess[i].Stop(true);
                }
            }
            ViewModel.CalculateButtonText = "Calculate";
            ViewModel.RunningStatus = "Stopped";
            return foundRunninProcess;
        }
    }
}