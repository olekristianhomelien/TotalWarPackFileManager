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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommonDialogs.MathViews
{
    /// <summary>
    /// Interaction logic for Vector4View.xaml
    /// </summary>
    public partial class Vector4View : UserControl
    {
        public Vector4View()
        {
            InitializeComponent();
        }


        public Vector4ViewModel Vector4
        {
            get { return (Vector4ViewModel)GetValue(Vector4Property); }
            set { SetValue(Vector4Property, value); }
        }

        public static readonly DependencyProperty Vector4Property =
            DependencyProperty.Register("Vector4", typeof(Vector4ViewModel), typeof(Vector4View), new PropertyMetadata(null));
    }
}
