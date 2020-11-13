using CommonDialogs.Common;
using Filetypes.RigidModel;
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

namespace VariantMeshEditor.Views.EditorViews.Util
{
    /// <summary>
    /// Interaction logic for Vector3View.xaml
    /// </summary>
    public partial class Vector3View : UserControl
    {
        public Vector3ViewData VectorData
        {
            get { return (Vector3ViewData)GetValue(VectorDataProperty); }
            set { SetValue(VectorDataProperty, value); }
        }

        public static readonly DependencyProperty VectorDataProperty = DependencyProperty.Register("VectorData", typeof(Vector3ViewData), typeof(Vector3View), new UIPropertyMetadata(null));


        public Vector3View()
        {
            InitializeComponent();
        }
    }


    public class Vector3ViewData : NotifyPropertyChangedImpl
    {
        FileVector3 _source;
        public Vector3ViewData(FileVector3 vector3, string name)
        {
            _source = vector3;
            _name = name;
        }

        string _name;
        public string Name
        {
            get { return _name; }
            set { SetAndNotify(ref _name, value); }
        }

        public float X
        {
            get { return _source.X; }
            set { _source.X = value; NotifyPropertyChanged(); }
        }

        public float Y
        {
            get { return _source.Y; }
            set { _source.Y = value; NotifyPropertyChanged(); }
        }

        public float Z
        {
            get { return _source.Z; }
            set { _source.Z = value; NotifyPropertyChanged(); }
        }
    }
}
