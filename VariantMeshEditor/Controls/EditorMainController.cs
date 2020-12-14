
using Common;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.ViewModels.Animation;
using VariantMeshEditor.ViewModels.Skeleton;
using Viewer.Scene;
using WpfTest.Scenes;
using static Filetypes.RigidModel.VariantMeshDefinition;
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
            _scene3d.SetResourceLibary(_resourceLibary);
            _scene3d.On3dWorldReady += Create3dWorld;



            //DumpRmv2Files dumper = new DumpRmv2Files();
            //dumper.Dump(_resourceLibary, @"C:\temp\DataDump\");

           //Dictionary<string, List<string>> slots = new Dictionary<string, List<string>>();
           //
           //var files = PackFileLoadHelper.GetAllWithExtention(_resourceLibary.PackfileContent, "variantmeshdefinition");
           //foreach (var file in files)
           //{
           //    var content = file.Data;
           //    var fileContent = Encoding.Default.GetString(content);
           //    try
           //    {
           //        VariantMeshFile meshFile = VariantMeshDefinition.Create(fileContent);
           //
           //        foreach (var slot in meshFile.VARIANT_MESH.SLOT)
           //        {
           //            var key = slot.Name;
           //            if(slot.AttachPoint.Length != 0)
           //                key = slot.Name + " " + slot.AttachPoint;
           //
           //            key = key.ToLower();
           //            if (!slots.ContainsKey(key))
           //                slots.Add(key, new List<string>());
           //            slots[key].Add(file.FullPath);
           //        }
           //    }
           //    catch(Exception e)
           //    { 
           //    }
           //}
           //
           //
           //var without = slots.Where(x => x.Key.Contains(" ") == false).ToList();
           //var with = slots.Where(x => x.Key.Contains(" ") == true).ToList();

        }

        public void LoadModel(PackedFile model)
        {
            _modelToLoad = model;
            if(_is3dWorldCreated)
                LoadModelAfterWorldCreated();
        }

        void Create3dWorld(GraphicsDevice device)
        {
           
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

        void PaladinAndDragon(RootElement rootNode,
            bool loadPaladin = true,
            bool loadDragon = false, 
            bool loadGoblin = false,
            bool loadArkan = false)
        {
            if (loadPaladin)
            {
                var paladinFile = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent,
                    @"variantmeshes\variantmeshdefinitions\brt_paladin.variantmeshdefinition");
                //chs_mirror_guard
                //brt_paladin
                var paladinMesh = rootNode.LoadModel(paladinFile, _resourceLibary, _scene3d);
                var paladinAnim = SceneElementHelper.GetFirstChild<AnimationElement>(paladinMesh);
                var skeleton = SceneElementHelper.GetFirstChild<SkeletonElement>(paladinMesh);
                var mainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\sword_and_shield\combat_idles\hu1_sws_combat_idle_02.anim");

              
           
       //        var testAnim = AnimationFile.Create(new Filetypes.ByteParsing.ByteChunk(mainAnim.Data));
       //    try
       //    {
       //           var file = new Viewer.Animation.AnimationClip(testAnim);
       //           var fileToSave = file.ConvertToFileFormat(skeleton.Skeleton);
       //           AnimationFile.Write(testAnim, @"C:\temp\Animation\animationFileTest2.anim");
       //    }
       //    catch (Exception e)
       //    { 
       //    
       //    }
           
          //
          //var mainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\rider\horse01\lancer\attacks\hu1_hr1_lancer_rider1_attack_02.anim");
                var handAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\hands\hu1_hand_pose_clench.anim");


                //var mainAnim = Filetypes.ByteParsing.ByteChunk.FromFile(@"C:\temp\Animation\animationFileTest.anm");
                //var file = AnimationFile.Create(mainAnim);
                //var clip = new Viewer.Animation.AnimationClip(file);
                //paladinAnim.AnimationPlayer.SetAnimation(clip, skeleton.Skeleton);

             paladinAnim.AnimationExplorerViewModel.AnimationList[0].SelectedAnimationPackFile = mainAnim;
             var secondAnimNode = paladinAnim.AnimationExplorerViewModel.AddNewAnimationNode();
             secondAnimNode.SelectedAnimationPackFile = handAnim;

               // paladinAnim.AnimationPlayer.Settings.FreezeAnimationRoot = true;
                //paladinAnim.AnimationPlayer.Pause();
            }

            if (loadDragon)
            {
                var dragonFile = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent,
                    @"variantmeshes\variantmeshdefinitions\hef_sun_dragon_mount.variantmeshdefinition");
                var dragonMesh = rootNode.LoadModel(dragonFile, _resourceLibary, _scene3d);

                var dragonAnim = SceneElementHelper.GetFirstChild<AnimationElement>(dragonMesh);
                //var dragonMainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\dragon01\combat_idles\dr1_combat_idle_02.anim");
                var dragonMainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\dragon01\attacks\dr1_breath_attack_01.anim");

                dragonAnim.AnimationExplorerViewModel.AnimationList[0].SelectedAnimationPackFile = dragonMainAnim;
            }

            if (loadGoblin)
            {
                var goblinFile = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent,
                 @"variantmeshes\variantmeshdefinitions\grn_forest_goblins_base.variantmeshdefinition");
                var goblinMesh = rootNode.LoadModel(goblinFile, _resourceLibary, _scene3d);

                var goblinAnim = SceneElementHelper.GetFirstChild<AnimationElement>(goblinMesh);
                //var dragonMainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\dragon01\combat_idles\dr1_combat_idle_02.anim");
                var goblinMainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid05\dual_sword\stand\hu5_ds_stand_idle_01.anim");

                goblinAnim.AnimationExplorerViewModel.AnimationList[0].SelectedAnimationPackFile = goblinMainAnim;
            }

            if (loadArkan)
            {

                var file = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"variantmeshes\variantmeshdefinitions\tmb_ch_arkhan.variantmeshdefinition");
                var mesh = rootNode.LoadModel(file, _resourceLibary, _scene3d);



                var animNode = SceneElementHelper.GetFirstChild<AnimationElement>(mesh);
                var skeleton = SceneElementHelper.GetFirstChild<SkeletonElement>(mesh);

                /*
                 animations\skeletons\humanoid01b.anim

animations\battle\humanoid01b\subset\spellsinger\sword\stand\hu1b_elf_spellsinger_sw_stand_idle_01.anim
                 */

                //var mainAnim = Filetypes.ByteParsing.ByteChunk.FromFile(@"C:\temp\Animation\floatyBoi.anim");
                //var inimFile = AnimationFile.Create(mainAnim);
                //var clip = new Viewer.Animation.AnimationClip(inimFile);
                //animNode.AnimationPlayer.SetAnimation(clip, skeleton.Skeleton);


                //var dragonMainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\dragon01\combat_idles\dr1_combat_idle_02.anim");
                //var animFile = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid05\dual_sword\stand\hu5_ds_stand_idle_01.anim");

                // goblinAnim.AnimationExplorerViewModel.AnimationList[0].SelectedAnimationPackFile = goblinMainAnim;

            }
        }
    }
}
