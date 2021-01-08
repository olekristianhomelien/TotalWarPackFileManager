using CommonDialogs.Common;
using Filetypes.RigidModel;
using Filetypes.RigidModel.LodHeader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariantMeshEditor.ViewModels.RigidModel
{
    public class LodHeaderViewModel : NotifyPropertyChangedImpl, IDisposable
    {
        public LodHeaderViewModel(RmvLodHeader header, string name, bool isVisible)
        {
            LodHeader = header;
            LodName = name;
            IsVisible = isVisible;
        }

        public RmvLodHeader LodHeader { get; private set; }
        public string LodName { get; private set; }


        bool _isVisible;
        public bool IsVisible { get { return _isVisible; } set{ SetAndNotify(ref _isVisible, value); } }

        public byte QualityLvl { get { return LodHeader.QualityLvl; } set { LodHeader.QualityLvl = value; NotifyPropertyChanged(); } }
        public float LodCameraDistance { get { return LodHeader.LodCameraDistance; } set { LodHeader.LodCameraDistance = value; NotifyPropertyChanged(); } }
        public ObservableCollection<ModelViewModel> Models { get; set; } = new ObservableCollection<ModelViewModel>();

        public void Dispose()
        {
            foreach (var model in Models)
                model.Dispose();
        }
    }
}
