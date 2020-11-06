using System.Windows.Controls;
using VariantMeshEditor.Controls;
using VariantMeshEditor.ViewModels;

namespace VariantMeshEditor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class VariantMeshEditorControl : UserControl
    {
        EditorMainController _mainController;

        public RootViewModel RootViewModel { get; set; } = new RootViewModel();

        public VariantMeshEditorControl()
        {
            InitializeComponent();
            DataContext = RootViewModel;

            _mainController = new EditorMainController(RenderView.Scene, RootViewModel);
            _mainController.LoadModel("variantmeshes\\variantmeshdefinitions\\brt_paladin.variantmeshdefinition");
        }
    }
}
