
using Common;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.ViewModels.Animation;
using Viewer.Scene;
using WpfTest.Scenes;
using Game = Common.Game;

namespace VariantMeshEditor.Controls
{
    class EditorMainController
    {
        Scene3d _scene3d;
        ResourceLibary _resourceLibary;
        PackedFile _modelToLoad;
        bool _is3dWorldCreated = false;

        public BaseViewModel RootViewModel { get; set; }

        public EditorMainController(Scene3d scene3d, BaseViewModel rootViewModel, List<PackFile> packFile)
        {
            _scene3d = scene3d;
            RootViewModel = rootViewModel;

            _resourceLibary = new ResourceLibary(packFile);
            _scene3d.LoadScene += Create3dWorld;



            //DumpRmv2Files dumper = new DumpRmv2Files();
            //dumper.Dump(_resourceLibary, @"C:\temp\DataDump\");
        }

        public void LoadModel(PackedFile model)
        {
            _modelToLoad = model;
            if(_is3dWorldCreated)
                LoadModelAfterWorldCreated();
        }

        void Create3dWorld(GraphicsDevice device)
        {
            _scene3d.SetResourceLibary(_resourceLibary);
            _is3dWorldCreated = true;
            LoadModelAfterWorldCreated();
        }

        void LoadModelAfterWorldCreated()
        {
            bool isFirstTime = true;
            var existingNode = RootViewModel.SceneGraph.SceneGraphRootNodes.FirstOrDefault();
            if (existingNode != null)
            {
                existingNode.Dispose();
                RootViewModel.SceneGraph.SceneGraphRootNodes.Clear();
                _scene3d.SceneGraphRootNode = null;
                isFirstTime = false;
            }

            var rootNode = new RootElement();
            rootNode.CreateContent(_scene3d, _resourceLibary);
            if (isFirstTime)
                PaladinAndDragon(rootNode);
            else
                rootNode.LoadModel(_modelToLoad, _resourceLibary, _scene3d);

            // SceneLoader sceneLoader = new SceneLoader(_resourceLibary);
            //var rootElement = sceneLoader.Load(_modelToLoad, );
            //rootElement.CreateContent(_scene3d, _resourceLibary);

            _scene3d.SceneGraphRootNode = rootNode;
            RootViewModel.SceneGraph.SceneGraphRootNodes.Add(rootNode);
        }


        void CreateTestWorldA(RootElement rootNode)
        {
           //var paladinFile = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent,
           //    @"variantmeshes\variantmeshdefinitions\brt_paladin.variantmeshdefinition");
           //
           //var paladinMesh = rootNode.LoadModel(paladinFile, _resourceLibary, _scene3d);
           //var anim = SceneElementHelper.GetFirstChild<AnimationElement>(paladinMesh);
           //
           //
           //bool loadDebugData = true;
           //if (loadDebugData)
           //{
           //    var mainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\sword_and_shield\combat_idles\hu1_sws_combat_idle_02.anim");
           //    AnimationList[0].SelectedAnimationPackFile = mainAnim;
           //
           //    AddNewAnimationNode();
           //    var handAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\hands\hu1_hand_pose_clench.anim");
           //    AnimationList[1].SelectedAnimationPackFile = handAnim;
           //}
           //
           //var pegasusFile = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent,
           //    @"variantmeshes\variantmeshdefinitions\brt_royal_pegasus.variantmeshdefinition");
           //var pegaMesh = rootNode.LoadModel(pegasusFile, _resourceLibary, _scene3d);



        }

        void PaladinAndDragon(RootElement rootNode, bool loadDragon = true)
        {
            var paladinFile = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent,
                @"variantmeshes\variantmeshdefinitions\brt_paladin.variantmeshdefinition");
            
            var paladinMesh = rootNode.LoadModel(paladinFile, _resourceLibary, _scene3d);
            var paladinAnim = SceneElementHelper.GetFirstChild<AnimationElement>(paladinMesh);

            //var mainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\sword_and_shield\combat_idles\hu1_sws_combat_idle_02.anim");
            var mainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\rider\horse01\lancer\attacks\hu1_hr1_lancer_rider1_attack_02.anim");
            var handAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\hands\hu1_hand_pose_clench.anim");

            paladinAnim.AnimationExplorer.AnimationList[0].SelectedAnimationPackFile = mainAnim;
            var secondAnimNode = paladinAnim.AnimationExplorer.AddNewAnimationNode();
            secondAnimNode.SelectedAnimationPackFile = handAnim;

            paladinAnim.AnimationPlayer.Settings.FreezeAnimationRoot = true;

            if (loadDragon)
            {
                var dragonFile = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent,
                    @"variantmeshes\variantmeshdefinitions\hef_sun_dragon_mount.variantmeshdefinition");
                var dragonMesh = rootNode.LoadModel(dragonFile, _resourceLibary, _scene3d);

                var dragonAnim = SceneElementHelper.GetFirstChild<AnimationElement>(dragonMesh);
                //var dragonMainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\dragon01\combat_idles\dr1_combat_idle_02.anim");
                var dragonMainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\dragon01\attacks\dr1_breath_attack_01.anim");

                dragonAnim.AnimationExplorer.AnimationList[0].SelectedAnimationPackFile = dragonMainAnim;
            }
        }
    }
}
