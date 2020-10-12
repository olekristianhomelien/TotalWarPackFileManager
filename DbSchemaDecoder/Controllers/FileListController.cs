using Common;
using CommonDialogs.Common;
using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.Codecs;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace DbSchemaDecoder.Controllers
{
    public class DataBaseFile
    {
        public string TableType { get; set; }
        public PackedFile DbFile { get; set; }
    }


    public class FileListController : NotifyPropertyChangedImpl
    {
        // Ui related variables
        public FileListViewModel ViewModel { get; set; } = new FileListViewModel();
        public ICommand FileSelectedCommand { get; private set; }
        public ICommand FilterButtonCommand { get; private set; }
        public ICommand OnlyShowTablesWithErrorCommand { get; private set; }
        public ICommand OnlyShowUniqueTablesCommand { get; private set; }
        public ICommand OnlyShowTablesWithContentCommand { get; private set; }

        // Internal variables
        List<DataBaseFile> _internalFileList = new List<DataBaseFile>();
        WindowState _windowState;
        Thread _evaluateThradHandle;



        public FileListController(WindowState windowState, string packFileDirectory)
        {
            _windowState = windowState;
            _windowState.OnTriggerErrorCheck += (sender, arg) => StartEvaluation();
            
            Load(packFileDirectory);
            FileSelectedCommand = new RelayCommand<DatabaseFileViewModel>(OnFileSelected);
            FilterButtonCommand = new RelayCommand(OnFilter);
            OnlyShowTablesWithErrorCommand = new RelayCommand(OnFilter);
            OnlyShowUniqueTablesCommand = new RelayCommand(OnFilter);
            OnlyShowTablesWithContentCommand = new RelayCommand(OnFilter);
            StartEvaluation();
        }

        public void StartEvaluation()
        {
            BatchEvaluator batchEvaluator = new BatchEvaluator(_internalFileList);
            batchEvaluator.OnCompleted += BatchEvaluator_OnCompleted;

            // Evaluate db files in a new thread to await waiting
            _evaluateThradHandle = new Thread(new ThreadStart(batchEvaluator.Evaluate));
            _evaluateThradHandle.Start();
        }

        private void BatchEvaluator_OnCompleted(object sender, List<BatchEvaluator.Result> errorParsingResult)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ViewModel.DbFilesWithError = errorParsingResult.Count(x => x.HasError == true);
                _windowState.FileParsingErrors = errorParsingResult;
                BuildFileList();
            });
        }

        private void OnFileSelected(DatabaseFileViewModel state)
        {
            if(state != null)
                _windowState.SelectedFile = state.DataBaseFile;
        }

        private void OnFilter()
        {
            BuildFileList();
        }

        void BuildFileList()
        {
            IEnumerable<DataBaseFile> items;

            if (ViewModel.OnlyShowTablesWithErrors && _windowState.FileParsingErrors != null)
                items = _internalFileList.Where(x => _windowState.FileParsingErrors.First(e => e.TableType == x.TableType).HasError);
            else
                items = _internalFileList.AsEnumerable();

            if (ViewModel.OnlyShowUniqueTables)
                items = items.GroupBy(x => x.TableType).Select(x => x.First());

            if (ViewModel.OnlyShowTablesWithContent)
            {
                List<DataBaseFile> itemsWithContent = new List<DataBaseFile>();
                foreach (var item in items)
                {
                    DBFileHeader header = PackedFileDbCodec.readHeader(item.DbFile);
                    if (header.EntryCount != 0)
                        itemsWithContent.Add(item);
                }

                items = itemsWithContent;
            }

            if (!string.IsNullOrWhiteSpace(ViewModel.SearchFilter))
                items = items.Where(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.TableType, ViewModel.SearchFilter, CompareOptions.IgnoreCase) >= 0);

            ViewModel.FileList.Clear();
            foreach (var item in items)
            {
                var parseRessult = _windowState.FileParsingErrors?.FirstOrDefault(x => x.TableType == item.TableType);
                DatabaseFileViewModel model = new DatabaseFileViewModel();
                if (parseRessult != null && parseRessult.HasError)
                {
                    model.Color = Colors.Red;
                }
                model.DataBaseFile = item;  
                ViewModel.FileList.Add(model);
            }

            ViewModel.ItemsInFilter = ViewModel.FileList.Count;
        }

        public void Load(string gameDir)
        {
            PackLoadSequence allFiles = new PackLoadSequence
            {
                IncludePacksContaining = delegate (string s) { return true; }
            };

            List<string> packPaths = allFiles.GetPacksLoadedFrom(gameDir);
            packPaths.Reverse();

            PackFileCodec codec = new PackFileCodec();
            foreach (string path in packPaths)
            {
                PackFile pack = codec.Open(path);
                foreach (var f in pack.Files)
                {
                    var dbEntry = IsDb(f);
                    if (dbEntry.HasValue)
                    {
                        _internalFileList.Add(new DataBaseFile()
                        {
                            TableType = dbEntry.Value.Item1,
                            DbFile = dbEntry.Value.Item2,
                        });
                    }
                }
            }

            ViewModel.TotalDbFiles = _internalFileList.Count();

            BuildFileList();
        }

        (string, PackedFile)? IsDb(PackedFile file)
        {
            bool hasParent = file.Parent != null;
            if (hasParent)
            {
                bool hasParentParnet = file.Parent.Parent != null;
                if (hasParentParnet)
                {
                    if (file.Parent.Parent.Name == "db")
                    {
                        return (file.Parent.Name, file);
                    }
                }
            }
            return null;
        }
    }
}
