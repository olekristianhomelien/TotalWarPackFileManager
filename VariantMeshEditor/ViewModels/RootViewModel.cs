using CommonDialogs.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariantMeshEditor.ViewModels
{
    public class RootViewModel : NotifyPropertyChangedImpl
    {
        SceneGraphViewModel _sceneGraph = new SceneGraphViewModel();
        public SceneGraphViewModel SceneGraph { get { return _sceneGraph; } set { SetAndNotify(ref _sceneGraph, value); } }


        string _rootDgbData = "RootDgbDataStr";
        public string RootDgbData { get { return _rootDgbData; } set { SetAndNotify(ref _rootDgbData, value); } }
    }



    public  class SceneGraphViewModel : NotifyPropertyChangedImpl
    {
        public ObservableCollection<FileSceneElement> _sceneGraphRootNodes = new ObservableCollection<FileSceneElement>();
        public ObservableCollection<FileSceneElement> SceneGraphRootNodes { get { return _sceneGraphRootNodes; } set { SetAndNotify(ref _sceneGraphRootNodes, value); } }


        FileSceneElement _selectedNode;
        public FileSceneElement SelectedNode { get { return _selectedNode; } set { SetAndNotify(ref _selectedNode, value); } }

        string _sceneGraphDgbData = "SceneGraphDgbDataStr";
        public string SceneGraphDgbData { get { return _sceneGraphDgbData; } set { SetAndNotify(ref _sceneGraphDgbData, value); } }
    }
}
