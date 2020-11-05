using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VariantMeshEditor.Views.EditorViews.Util
{
    /// <summary>
    /// Interaction logic for BrowsableItemView.xaml
    /// </summary>
    public partial class BrowsableItemView : UserControl
    {
        public static readonly DependencyProperty RemoveProperty = DependencyProperty.Register("Remove", typeof(ICommand), typeof(BrowsableItemView), new UIPropertyMetadata(null));
        public ICommand Remove
        {
            get { return (ICommand)GetValue(RemoveProperty); }
            set { SetValue(RemoveProperty, value); }
        }

        public static readonly DependencyProperty BrowseProperty = DependencyProperty.Register("Browse", typeof(ICommand), typeof(BrowsableItemView), new UIPropertyMetadata(null));
        public ICommand Browse
        {
            get { return (ICommand)GetValue(BrowseProperty); }
            set { SetValue(BrowseProperty, value); }
        }

        public static readonly DependencyProperty PreviewProperty = DependencyProperty.Register("Preview", typeof(ICommand), typeof(BrowsableItemView), new UIPropertyMetadata(null));
        public ICommand Preview
        {
            get { return (ICommand)GetValue(PreviewProperty); }
            set { SetValue(PreviewProperty, value); }
        }


        public static readonly DependencyProperty LabelNameProperty = DependencyProperty.Register("LabelName", typeof(string), typeof(BrowsableItemView), new UIPropertyMetadata(null));
        public string LabelName
        {
            get { return (string)GetValue(LabelNameProperty); }
            set { SetValue(LabelNameProperty, value); }
        }

        public static readonly DependencyProperty LabelLengthProperty = DependencyProperty.Register("LabelLength", typeof(double), typeof(BrowsableItemView), new PropertyMetadata(null));
        public double LabelLength
        {
            get { return (double)GetValue(LabelLengthProperty); }
            set { SetValue(LabelLengthProperty, value); }
        }




        public static readonly DependencyProperty PathTextProperty = DependencyProperty.Register("PathText", typeof(string), typeof(BrowsableItemView), new UIPropertyMetadata(null));
        public string PathText
        {
            get { return (string)GetValue(PathTextProperty); }
            set { SetValue(PathTextProperty, value); }
        }


        public static readonly DependencyProperty PathTextReadOnlyProperty = DependencyProperty.Register("PathTextReadOnly", typeof(bool), typeof(BrowsableItemView), new UIPropertyMetadata(null));
        public bool PathTextReadOnly
        {
            get { return (bool)GetValue(PathTextReadOnlyProperty); }
            set { SetValue(PathTextReadOnlyProperty, value); }
        }




        public static readonly DependencyProperty DisplayRemoveButtonProperty = DependencyProperty.Register("DisplayRemoveButton", typeof(bool), typeof(BrowsableItemView), new UIPropertyMetadata(null));
        public bool DisplayRemoveButton
        {
            get { return (bool)GetValue(DisplayRemoveButtonProperty); }
            set { SetValue(DisplayRemoveButtonProperty, value); }
        }

        public static readonly DependencyProperty DisplayPreviewButtonProperty = DependencyProperty.Register("DisplayPreviewButton", typeof(bool), typeof(BrowsableItemView), new UIPropertyMetadata(null));
        public bool DisplayPreviewButton
        {
            get { return (bool)GetValue(DisplayPreviewButtonProperty); }
            set { SetValue(DisplayPreviewButtonProperty, value); }
        }

        public static readonly DependencyProperty DisplayBrowseButtonProperty = DependencyProperty.Register("DisplayBrowseButton", typeof(bool), typeof(BrowsableItemView), new UIPropertyMetadata(null));
        public bool DisplayBrowseButton
        {
            get { return (bool)GetValue(DisplayBrowseButtonProperty); }
            set { SetValue(DisplayBrowseButtonProperty, value); }
        }

        public BrowsableItemView()
        {
            DisplayBrowseButton = true;
            DisplayPreviewButton = true;
            DisplayRemoveButton = true;
            LabelLength = double.NaN;
            InitializeComponent();
        }
    }
}
