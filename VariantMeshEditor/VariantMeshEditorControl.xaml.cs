using Common;
using Filetypes;
using System.Collections.Generic;
using System.Linq;
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

            
           
        }


        public void SetPackFiles(List<PackFile> packFiles)
        {




            _mainController = new EditorMainController(RenderView.Scene, RootViewModel, packFiles);
        }


        public bool CanEdit(PackedFile file)
        {
            var possibleExtentions = new string[]{ "variantmeshdefinition", "wsmodel", "rigid_model_v2" };
            return possibleExtentions.Contains(file.FileExtention);
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
