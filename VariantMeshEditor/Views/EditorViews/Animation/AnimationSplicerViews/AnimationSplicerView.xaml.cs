using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VariantMeshEditor.Views.EditorViews.Animation.AnimationSplicerViews
{
    /// <summary>
    /// Interaction logic for AnimationSplicerEditorView.xaml
    /// </summary>
    public partial class AnimationSplicerView : UserControl
    {
        public AnimationSplicerView()
        {
            InitializeComponent();
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(
                    e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
