using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Controls.EditorControllers.Animation;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Scene;

namespace VariantMeshEditor.Controls.EditorControllers.Animation
{
    class AnimationController
    {
        AnimationExplorerController _animationExplorerController;
        AnimationPlayerController _animationPlayerController;
        AnimationEditorView _viewModel;

        public AnimationController(ResourceLibary resourceLibary, AnimationElement animationElement, SkeletonElement skeletonElement)
        {
            _animationPlayerController = new AnimationPlayerController(animationElement);
            _animationExplorerController = new AnimationExplorerController(resourceLibary, animationElement, skeletonElement, _animationPlayerController);
        }

        public AnimationEditorView GetView()
        {
            if (_viewModel == null)
            {
                _viewModel = new AnimationEditorView();
                _animationPlayerController.PopulateExplorerView(_viewModel.Player);

                _animationExplorerController.PopulateExplorerView(_viewModel);
                _animationExplorerController.CreateTestData(_viewModel);
            }
            return _viewModel;
        }

        public void Update()
        {
            _animationPlayerController.Update();
        }

        public string GetCurrentAnimationName()
        {
            return _animationExplorerController.GetCurrentAnimationName();
        }
    }
}
