using Common;
using CommonDialogs.Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Controls.EditorControllers.Animation;
using VariantMeshEditor.Util;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Animation;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels
{




    public class AnimationExplorerNodeViewModel : NotifyPropertyChangedImpl
    {
        AnimationExplorerViewModel Parent { get; set; }
        public ICommand RemoveCommand { get; set; }

        string _animationName;
        public string AnimationName
        {
            get { return _animationName; }
            set { SetAndNotify(ref _animationName, value); }
        }


        bool _onlyDisplayAnimationsForCurrentSkeleton;
        public bool OnlyDisplayAnimationsForCurrentSkeleton
        {
            get { return _onlyDisplayAnimationsForCurrentSkeleton; }
            set 
            {
                SetAndNotify(ref _onlyDisplayAnimationsForCurrentSkeleton, value);
                if (value)
                    FilterList = Parent.AnimationFilesForSkeleton;
                else
                    FilterList = Parent.AnimationFiles;
            }
        }

        List<PackedFile> _filterList;
        public List<PackedFile> FilterList { get { return _filterList; } set { SetAndNotify(ref _filterList, value); } }

        public bool DisplayErrorMessage { get { return !string.IsNullOrWhiteSpace(ErrorMessage); } }

        string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                SetAndNotify(ref _errorMessage, value);
                NotifyPropertyChanged(nameof(DisplayErrorMessage));
            }
        }





        public bool Filter(object item, Regex expression)
        {
            return true;
        }

        public AnimationExplorerNodeViewModel(AnimationExplorerViewModel parent)
        {
            Parent = parent;
            RemoveCommand = new RelayCommand(OnRemoveButtonClicked);
            OnlyDisplayAnimationsForCurrentSkeleton = true;
        }

        void OnRemoveButtonClicked()
        {
            //Parent.AnimationsTemp.Remove(this);
        }
    }

    public class AnimationExplorerViewModel : NotifyPropertyChangedImpl
    {
        ResourceLibary _resourceLibary;
        string _skeletonName;

        public ICommand AddNewAnimationCommad { get; set; }


        public List<PackedFile> AnimationFiles { get; set; } = new List<PackedFile>();
        public List<PackedFile> AnimationFilesForSkeleton { get; set; } = new List<PackedFile>();

        public AnimationExplorerViewModel(ResourceLibary resourceLibary, string skeletonName)
        {
           


            _resourceLibary = resourceLibary;
            _skeletonName = skeletonName;
            
            FindAllAnimations();

            AnimationsTemp.Add(new AnimationExplorerNodeViewModel(this) { AnimationName = "Animation0", FilterList=AnimationFiles });
            AnimationsTemp.Add(new AnimationExplorerNodeViewModel(this) { AnimationName = "Animation1", FilterList=AnimationFiles });
            AnimationsTemp.Add(new AnimationExplorerNodeViewModel(this) { AnimationName = "Animation2", FilterList=AnimationFiles });
            AnimationsTemp.Add(new AnimationExplorerNodeViewModel(this) { AnimationName = "Animation3", FilterList= AnimationFiles });

            AddNewAnimationCommad = new RelayCommand(OnAddNewAnimationClicked);

            
        }

        void OnAddNewAnimationClicked()
        {
            AnimationsTemp.Add(new AnimationExplorerNodeViewModel(this));
        }

        void FindAllAnimations()
        {
            AnimationFiles = PackFileLoadHelper.GetAllWithExtention(_resourceLibary.PackfileContent, "anim");

            foreach (var animation in AnimationFiles)
            {
                var animationSkeletonName = AnimationFile.GetAnimationHeader(new ByteChunk(animation.Data)).SkeletonName;
                if (animationSkeletonName == _skeletonName)
                    AnimationFilesForSkeleton.Add(animation);
            }
        }

        public ObservableCollection<AnimationExplorerNodeViewModel> AnimationsTemp { get; set; } = new ObservableCollection<AnimationExplorerNodeViewModel>();
    }














    public class AnimationElement : FileSceneElement
    {
        public AnimationExplorerViewModel AnimationExplorer { get; set; }


        public override FileSceneElementEnum Type => FileSceneElementEnum.Animation;
        public AnimationPlayer AnimationPlayer { get; set; } = new AnimationPlayer();

        AnimationController _controller;

        public AnimationElement(FileSceneElement parent) : base(parent, "", "", "Animation")
        {
            ApplyElementCheckboxVisability = System.Windows.Visibility.Hidden;


        }










        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            var skeleton = SceneElementHelper.GetAllOfTypeInSameVariantMesh<SkeletonElement>(this);
            if (skeleton.Count == 1)
            {
                AnimationExplorer = new AnimationExplorerViewModel(resourceLibary, skeleton.First().SkeletonFile.Header.SkeletonName);


                //_controller = new AnimationController(resourceLibary, this, skeleton.First());
            }
        }

        protected override void UpdateNode(GameTime time)
        {
            AnimationPlayer.Update(time);
            //DisplayName = "Animation - " + _controller.GetCurrentAnimationName();
            //_controller.Update();
        }



    }
}
