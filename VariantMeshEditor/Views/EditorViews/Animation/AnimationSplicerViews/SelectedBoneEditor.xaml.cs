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
using VariantMeshEditor.ViewModels.Animation;
using VariantMeshEditor.ViewModels.Animation.AnimationSplicer;
using static VariantMeshEditor.ViewModels.Skeleton.SkeletonViewModel;

namespace VariantMeshEditor.Views.EditorViews.Animation.AnimationSplicerViews
{
    /// <summary>
    /// Interaction logic for AnimationSplicerSelectedBoneEditor.xaml
    /// </summary>
    public partial class SelectedBoneEditor : UserControl
    {
        public SelectedBoneEditor()
        {
            InitializeComponent();
        }

        public ObservableCollection<SkeletonBoneNode> TargetBoneList
        {
            get { return (ObservableCollection<SkeletonBoneNode>)GetValue(TargetBoneListProperty); }
            set { SetValue(TargetBoneListProperty, value); }
        }

        public static readonly DependencyProperty TargetBoneListProperty =
            DependencyProperty.Register("TargetBoneList", typeof(ObservableCollection<SkeletonBoneNode>), typeof(SelectedBoneEditor), new PropertyMetadata(null));


        public string TargetBoneListDisplayName
        {
            get { return (string)GetValue(TargetBoneListDisplayNameProperty); }
            set { SetValue(TargetBoneListDisplayNameProperty, value); }
        }

        public static readonly DependencyProperty TargetBoneListDisplayNameProperty =
            DependencyProperty.Register("TargetBoneListDisplayName", typeof(string), typeof(SelectedBoneEditor), new PropertyMetadata(null));



        public MappableSkeletonBone SelectedBone
        {
            get { return (MappableSkeletonBone)GetValue(SelectedBoneProperty); }
            set { SetValue(SelectedBoneProperty, value); }
        }

        public static readonly DependencyProperty SelectedBoneProperty =
            DependencyProperty.Register("SelectedBone", typeof(MappableSkeletonBone), typeof(SelectedBoneEditor), new PropertyMetadata(null));
    }
}
