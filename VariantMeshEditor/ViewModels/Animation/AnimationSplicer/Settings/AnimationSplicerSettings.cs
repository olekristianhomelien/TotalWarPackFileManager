using Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VariantMeshEditor.Services;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation.AnimationSplicer.BoneMapping;
using VariantMeshEditor.ViewModels.VariantMesh;
using Viewer.Scene;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer.Settings
{
    class AnimationSplicerSettings
    {
        [JsonIgnore]
        private static readonly ILogger _logger = Logging.CreateStatic(typeof(AnimationSplicerSettings));

        public string RootMesh { get; set; }
        public FilterableAnimationSetttings TargetAnimation { get; set; }
        public FilterableAnimationSetttings ExternalAnimation { get; set; }

        public MainAnimation SelectedMainAnimation { get; set; }

        public IEnumerable<AdvBoneMappingBone> MappableBoneSettings { get; set; }

        public void PreperForSave()
        {
            TargetAnimation.PreperForSave();
            ExternalAnimation.PreperForSave();
        }

        public static void ExportCurrentConfiguration(AnimationSplicerViewModel viewModel)
        {
            _logger.Here().Information("Starting to export config");
            try
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    DefaultExt = ".splicCfg"
                };

                if (dialog.ShowDialog() == true)
                {
                    AnimationSplicerSettings settings = new AnimationSplicerSettings
                    {
                        ExternalAnimation = viewModel.ExternalAnimation.Data,
                        TargetAnimation = viewModel.TargetAnimation.Data,
                        SelectedMainAnimation = viewModel.SelectedMainAnimation,
                        MappableBoneSettings = viewModel.BoneMapping,
                        RootMesh = viewModel.VariantMeshParent?.FullPath
                    };
                    settings.PreperForSave();

                    var str = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText(dialog.FileName, str);
                }
            }
            catch (Exception e)
            {
                _logger.Here().Error(e.ToString());
                MessageBox.Show("Export failed");
            }

            _logger.Here().Information("Export config completed");
        }

        public static void ImportConfiguration(AnimationSplicerViewModel viewModel)
        {
            _logger.Here().Information("Import to export config");
            try
            {
                var dialog = new OpenFileDialog
                {
                    DefaultExt = ".splicCfg"
                };

                if (dialog.ShowDialog() == true)
                {
                    var content = File.ReadAllText(dialog.FileName);
                    var settings = JsonConvert.DeserializeObject<AnimationSplicerSettings>(content, new AdvBoneMappingBoneSettingsConverter());

                    viewModel.TargetAnimation.Data = settings.TargetAnimation;
                    viewModel.TargetAnimation.ForceUpdate();
                    viewModel.ExternalAnimation.Data = settings.ExternalAnimation;
                    viewModel.ExternalAnimation.ForceUpdate();

                    viewModel.BoneMapping = new ObservableCollection<AdvBoneMappingBone>(settings.MappableBoneSettings);
                    viewModel.SelectedMainAnimation = settings.SelectedMainAnimation;
                }
            }
            catch (Exception e)
            {
                _logger.Here().Error(e.ToString());
                MessageBox.Show("Import failed");
            }
            _logger.Here().Information("Import config completed");
        }
    }

    public class AdvBoneMappingBoneSettingsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(AdvBoneMappingBoneSettings).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);


            var ob = jo.ToObject<AdvBoneMappingBoneSettings>();
            AdvBoneMappingBoneSettings item;
            if (ob.BoneMappingForSerialization == BoneMappingType.Direct_smart)
                item = new DirectSmartAdvBoneMappingBoneSettings();
            else if (ob.BoneMappingForSerialization == BoneMappingType.AttachmentPoint)
                item = new AttachmentPointAdvBoneMappingBoneSettings();
            else
                item = new AdvBoneMappingBoneSettings();

            serializer.Populate(jo.CreateReader(), item);
            return item;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class FilterableAnimationSetttings
    {
        public string HeaderText;
        public bool EnableSkeltonBrowsing;

        [JsonIgnore]
        public PackedFile SelectedAnimation;
        [JsonIgnore]
        public PackedFile SelectedSkeleton;

        public string SelectedAnimationFileName { get; set; }
        public string SelectedSkeletonFileName { get; set; }

        public TimeMatchMethod MatchingMethod = TimeMatchMethod.TimeFit;

        public void PreperForSave()
        {
            SelectedAnimationFileName = SelectedAnimation?.FullPath;
            SelectedSkeletonFileName = SelectedSkeleton?.FullPath;
        }

        public void AfterLoad(ResourceLibary lib)
        {
            SelectedSkeleton = PackFileLoadHelper.FindFile(lib.PackfileContent, SelectedSkeletonFileName);
            SelectedAnimation = PackFileLoadHelper.FindFile(lib.PackfileContent, SelectedAnimationFileName);
        }
    }
}
