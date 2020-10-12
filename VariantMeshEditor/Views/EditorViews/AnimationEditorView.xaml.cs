using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VariantMeshEditor.Views.EditorViews.AnimationViews;

namespace VariantMeshEditor.Views.EditorViews
{
    /// <summary>
    /// Interaction logic for AnimationEditorView.xaml
    /// </summary>
    public partial class AnimationEditorView : UserControl
    {
        ObservableCollection<AnimationExplorerView> _animationExplorers = new ObservableCollection<AnimationExplorerView>();
        SolidColorBrush _backgroundColor0 = new SolidColorBrush(SystemColors.ControlColor);
        SolidColorBrush _backgroundColor1 = new SolidColorBrush(Colors.LightGray);

        public IList<AnimationExplorerView> Explorers { get { return _animationExplorers; } }


        public AnimationEditorView()
        {
            InitializeComponent();
        }

       

        public AnimationExplorerView CreateAnimationExplorer()
        {
            var explorer = new AnimationExplorerView();
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
                    (AnimationExplorers.Children[i] as AnimationExplorerView).Background = _backgroundColor0;
                else
                    (AnimationExplorers.Children[i] as AnimationExplorerView).Background = _backgroundColor1;
            }
        }

        public void RemoveAnimationExplorer(AnimationExplorerView explorer)
        {
            var index = AnimationExplorers.Children.IndexOf(explorer);
            AnimationExplorers.Children.RemoveAt(index);
            _animationExplorers.Remove(explorer);
            EnsureColour();
        }

    }
}
