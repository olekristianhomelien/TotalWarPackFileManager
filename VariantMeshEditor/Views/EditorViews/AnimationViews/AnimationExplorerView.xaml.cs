using Filetypes.RigidModel;
using System.Windows.Controls;

namespace VariantMeshEditor.Views.EditorViews.AnimationViews
{
    /// <summary>
    /// Interaction logic for AnimationExplorer.xaml
    /// </summary>
    public partial class AnimationExplorerView : UserControl
    {
        public bool IsMainAnimation { get; set; } = false;
        public AnimationFile AnimationFile { get; set; }
        public AnimationExplorerView()
        {
            InitializeComponent();
        }
    }
}
