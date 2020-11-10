using Common;
using Filetypes;
using System.Windows.Controls;
using VariantMeshEditor.Controls;
using VariantMeshEditor.ViewModels;

namespace VariantMeshEditor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class VariantMeshEditorControl : UserControl, IPackedFileEditor
    {
        EditorMainController _mainController;

        public BaseViewModel RootViewModel { get; set; } = new BaseViewModel();


        PackedFile _currentPackFile;
        public PackedFile CurrentPackedFile { 
            get { return _currentPackFile; } 
            set
            {
                _currentPackFile = value;
                _mainController.LoadModel(_currentPackFile);
            } 
        }
        public bool ReadOnly { get; set; }

        public VariantMeshEditorControl()
        {
            InitializeComponent();
            DataContext = RootViewModel;

            _mainController = new EditorMainController(RenderView.Scene, RootViewModel);
           
        }

        public bool CanEdit(PackedFile file)
        {
            return (file.FileExtention == "variantmeshdefinition");
        }

        public void Commit()
        {
            //throw new System.NotImplementedException();
        }

        public void Dispose()
        {
           // throw new System.NotImplementedException();
        }
    }
}
