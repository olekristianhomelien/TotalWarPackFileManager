﻿using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
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
    class BruteForceController : NotifyPropertyChangedImpl
    {
        public enum BruteForceCalculatorType
        { 
            BruteForceUsingCaSchama = 0,
            BruteForce,
            BruteForceUsingExistingTables,
            BruteForceUnknownTableCount,
            BruteForceUsingExistingTableUnknownTableCount
        };

        public BruteForceViewModel ViewModel { get; set; } = new BruteForceViewModel();

        DataBaseFile _selectedFile;
        List<CaSchemaEntry> _caSchemaEntryList;
        List<FieldInfo> _dbSchemaList;
        Thread _threadHandle;
        BruteForceParser _bruteForceparser;
        DateTime _startTime;
        EventHub _eventHub;

        private readonly System.Timers.Timer _timer;
        public ICommand ComputeBruteForceCommand { get; private set; }
        public ICommand SaveResultCommand { get; private set; }
        public ICommand OnClickCommand { get; private set; }

        public BruteForceController(EventHub eventHub)
        {
            _eventHub = eventHub;
            _eventHub.OnCaSchemaLoaded += _eventHub_OnCaSchemaLoaded;
            _eventHub.OnDbSchemaChanged += (sender, schema) => { _dbSchemaList = schema; };
            _eventHub.OnFileSelected += (sender, file) => { _selectedFile = file; Cancel(); };

            ComputeBruteForceCommand = new RelayCommand(OnCompute);
            SaveResultCommand = new RelayCommand(OnSave);
            OnClickCommand = new RelayCommand<ItemView>(OnItemDoubleClicked);

            _timer = new System.Timers.Timer(500);
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
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
            if(_caSchemaEntryList != null)
                ViewModel.ColumnCount = _caSchemaEntryList.Count();

            var bruteForceMethod = (BruteForceCalculatorType)ViewModel.ComputeType;
            if (bruteForceMethod == BruteForceCalculatorType.BruteForce)
            {
                ViewModel.BruteForceColumnCountText = "No. Table Columns:";
                ViewModel.ColumnCountEditable = true;
                if (_dbSchemaList != null)
                    ViewModel.ColumnCount = _dbSchemaList.Count();
                return;
            }
            else if (bruteForceMethod == BruteForceCalculatorType.BruteForceUnknownTableCount)
            {
                ViewModel.BruteForceColumnCountText = "Max Columns:";
                ViewModel.ColumnCountEditable = true;
                if (_dbSchemaList != null)
                    ViewModel.ColumnCount = _dbSchemaList.Count();
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
                if(_dbSchemaList != null)
                    ViewModel.ColumnCount = _dbSchemaList.Count();
                ViewModel.ColumnCountEditable = true;
                return;
            }
            else if (bruteForceMethod == BruteForceCalculatorType.BruteForceUsingExistingTableUnknownTableCount)
            {
                ViewModel.BruteForceColumnCountText = "No.Total Max Table Columns:";
                if (_dbSchemaList != null)
                    ViewModel.ColumnCount = _dbSchemaList.Count();
                ViewModel.ColumnCountEditable = true;
                return;
            }

            throw new NotImplementedException("Unknown compute type");
        }

        private void _eventHub_OnCaSchemaLoaded(object sender, List<CaSchemaEntry> e)
        {
            _caSchemaEntryList = e;
            ViewModel.ColumnCount = _caSchemaEntryList.Count();
        }

        void OnItemDoubleClicked(ItemView clickedItem)
        {
            var items = clickedItem.Enums.Select(x => FieldParser.CreateFromEnum(x).Instance()).ToList();
            for (int i = 0; i < items.Count(); i++)
                items[i].Name = "Unknown" + i;

            _eventHub.TriggerSetDbSchema(this, items);
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var runTimeSec = (e.SignalTime - _startTime).TotalSeconds;
            Update(runTimeSec);
        }

        void OnCompute()
        {
            BruteForce(_selectedFile, ViewModel.ColumnCount);
        }

        void OnSave()
        {
            File.WriteAllLines(@"C:\temp\output.text", ViewModel.Values.Select(x => x.Value));
        }

        void BruteForce(DataBaseFile file, int maxNumberOfFields)
        {
            if (_threadHandle == null)
            {
                var bruteForceMethod = (BruteForceCalculatorType)ViewModel.ComputeType;
                IBruteForceCombinationProvider combinationProvider = GetCombinationProvider(bruteForceMethod);
                if (combinationProvider == null)
                    return;

                ViewModel.Values.Clear();
                _startTime = DateTime.Now;
                _timer.Start();
                ViewModel.RunTime = "";
                ViewModel.RunningStatus = "Running";
                ViewModel.TotalPossibleCombinations = "";
                ViewModel.EvaluatedCombinations = "";
                ViewModel.SkippedEarlyCombinations = "";
                ViewModel.ComputedPerSec = "";
                ViewModel.PossibleFirstRpws = "";
                ViewModel.CalculateButtonText = "Cancel";

                _bruteForceparser = new BruteForceParser(file, combinationProvider, maxNumberOfFields);
                ViewModel.TotalPossibleCombinations = _bruteForceparser.PossibleCombinations.ToString("N0");

                _bruteForceparser.OnParsingCompleteEvent += ComputationDoneEventHandler;
                _bruteForceparser.OnCombintionFoundEvent += CombinationFoundEventHandler;

                _threadHandle = new Thread(new ThreadStart(_bruteForceparser.Compute));
                _threadHandle.Start();
            }
            else
            {
                Cancel();
            }
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
                        return new CaTableCombinations(_caSchemaEntryList);

                    case BruteForceCalculatorType.BruteForceUsingExistingTables:
                    case BruteForceCalculatorType.BruteForceUsingExistingTableUnknownTableCount:
                        return new AppendTableCombinations(_dbSchemaList.Select(x => x.TypeEnum).ToArray());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }

            throw new NotImplementedException("Unknown compute type");
        }

        void CombinationFoundEventHandler(object sender, FieldParserEnum[] val)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var values = val.Select(x => FieldParser.CreateFromEnum(x).InstanceName());
                var v = string.Join(", ", values);

                ViewModel.Values.Add(new ItemView() 
                { 
                    Idx= ViewModel.Values.Count() + 1,
                    Value = v,
                    Enums = val.ToList(),
                    Columns = val.Count(),
                });

                ViewModel.PossibleCombinationsFound = ViewModel.Values.Count().ToString();
            });
        }

        void ComputationDoneEventHandler(object sender, EventArgs args)
        {
            ViewModel.RunningStatus = "Done";
            ViewModel.CalculateButtonText = "Calculate";
            var runTimeSec = (DateTime.Now - _startTime).TotalSeconds;
            _timer.Stop();
            _threadHandle = null;

            Update(runTimeSec);
        }

        void Update(double runTimeSec)
        {
            ViewModel.RunTime = (int)runTimeSec + "s";

            ViewModel.EvaluatedCombinations =  _bruteForceparser.EvaluatedCombinations.ToString("N0");
            ViewModel.SkippedEarlyCombinations =  _bruteForceparser.SkippedEarlyCombinations.ToString("N0");

            var bigIntTime = new BigInteger(runTimeSec);
            if (bigIntTime == 0)
                bigIntTime = 1;
            ViewModel.ComputedPerSec = (_bruteForceparser.EvaluatedCombinations / bigIntTime).ToString("N0");
            ViewModel.PossibleFirstRpws =  _bruteForceparser.PossibleFirstRows.ToString("N0");
   
            BigFloat bigFloatEvaluatedCombinations = new BigFloat(_bruteForceparser.EvaluatedCombinations);
            BigFloat bigFloatTotal = new BigFloat(_bruteForceparser.PossibleCombinations);

            var ar = ((bigFloatEvaluatedCombinations / bigFloatTotal) * 100).ToString().Take(5).ToArray();
            string firstFivChar = new string(ar);
            ViewModel.EstimatedRunTime = firstFivChar + "%";
        }

        public void Cancel()
        {
            if (_threadHandle != null)
            {
                _threadHandle.Abort();
                _threadHandle = null;
                ViewModel.CalculateButtonText = "Calculate";
                ViewModel.RunningStatus = "Stopped";
                _timer.Stop();
            }
        }
    }
}