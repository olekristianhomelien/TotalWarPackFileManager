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

        SceneTreeViewController _treeViewController;

        EditorMainController _mainController;


        public RootViewModel RootViewModel { get; set; }

        public VariantMeshEditorControl()
        {

            RootViewModel = new RootViewModel();

            var rootNode = new RootElement();
            
            var slots = rootNode.AddChild(new SlotsElement(rootNode));
            slots.AddChild(new SlotElement(slots, "Test0", "Attach0"));
            slots.AddChild(new SlotElement(slots, "Test1", "Attach1"));

            rootNode.AddChild(new AnimationElement(rootNode));


            RootViewModel.SceneGraph.SceneGraphRootNodes.Add(rootNode);



            InitializeComponent();
            DataContext = RootViewModel;



            //_treeViewController = new SceneTreeViewController(EditorPanel.TreeView.tree);
            _mainController = new EditorMainController(_treeViewController, RenderView.Scene, RootViewModel);
            _mainController.LoadModel("variantmeshes\\variantmeshdefinitions\\brt_paladin.variantmeshdefinition");
            //
        }
    }
}
