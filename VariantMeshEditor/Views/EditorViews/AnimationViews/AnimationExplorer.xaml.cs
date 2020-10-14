using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VariantMeshEditor.Views.EditorViews.AnimationViews
{
    /// <summary>
    /// Interaction logic for AnimationExplorer.xaml
    /// </summary>
    public partial class AnimationExplorer : UserControl
    {
        ObservableCollection<AnimationExplorerItemView> _animationExplorers = new ObservableCollection<AnimationExplorerItemView>();
        SolidColorBrush _backgroundColor0 = new SolidColorBrush(SystemColors.ControlColor);
        SolidColorBrush _backgroundColor1 = new SolidColorBrush(Colors.LightGray);

        public IList<AnimationExplorerItemView> Explorers { get { return _animationExplorers; } }


        public AnimationExplorer()
        {
            InitializeComponent();
        }

        public AnimationExplorerItemView CreateAnimationExplorer()
        {
            var explorer = new AnimationExplorerItemView();
            DockPanel.SetDock(explorer, Dock.Top);
            AnimationExplorers.Children.Add(explorer);
            _animationExplorers.Add(explorer);

            EnsureColour();
            return explorer;
        }

        void EnsureColour()
        {
            for (int i = 0; i < AnimationExplorers.Children.Count; i++)
            {
                if (i % 2 == 0)
                    (AnimationExplorers.Children[i] as AnimationExplorerItemView).Background = _backgroundColor0;
                else
                    (AnimationExplorers.Children[i] as AnimationExplorerItemView).Background = _backgroundColor1;
            }
        }

        public void RemoveAnimationExplorer(AnimationExplorerItemView explorer)
        {
            var index = AnimationExplorers.Children.IndexOf(explorer);
            AnimationExplorers.Children.RemoveAt(index);
            _animationExplorers.Remove(explorer);
            EnsureColour();
        }
    }
}
