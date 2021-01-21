using Common;
using MetaFileEditor.DataType;
using MetaFileEditor.ViewModels;
using MetaFileEditor.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaFileEditor
{
    public class MetaFileEditorController
    {
        public static MetaDataMainView CreateDecoder(List<PackFile> packFiles)
        {
            var allMetaFiles = PackFileLoadHelper.GetAllWithExtention(packFiles, "meta");
            allMetaFiles = allMetaFiles.Where(f => f.FullPath.Contains("anm.meta")).ToList();
            List<MetaDataFile> allMetaData = new List<MetaDataFile>();


            MetaDataFile master = new MetaDataFile()
            {
                FileName ="Master collection"
            };

            MetaDataFileParser parser = new MetaDataFileParser();
            foreach (var file in allMetaFiles)
            {
                var res = parser.ParseFile(file);
                allMetaData.Add(res);

                foreach (var resultDataItem in res.TagItems)
                {

                    var masterDataItem =  master.TagItems.FirstOrDefault(x => x.Name == resultDataItem.Name && x.Version == resultDataItem.Version);
                    if (masterDataItem == null)
                    {
                        master.TagItems.Add(new MetaDataTagItem() { Name = resultDataItem.Name, Version = resultDataItem.Version});
                        masterDataItem = master.TagItems.Last();
                    }

                    foreach (var tag in resultDataItem.DataItems)
                    {
                        masterDataItem.DataItems.Add(tag);
                    }

                }
            }

            var v = allMetaData.GroupBy(X => X.TagItems.Select(d=>d.Name)).ToList();



            foreach (var item in master.TagItems)
            {
                var versions = item.DataItems.Select(x => x.Version).Distinct().ToList();
                var size = item.DataItems.Select(x => x.Size).Distinct().ToList();
            }

            master.TagItems = master.TagItems.OrderBy(x => x.DisplayName).ToList();



            var view = new MetaDataMainView();
            view.DataContext = new MainViewModel(master, packFiles);
            return view;
        }

        public static void CreateEditor(PackedFile metaDataFile)
        {

            MetaDataFileParser parser = new MetaDataFileParser();
            parser.ParseFile(metaDataFile);
        }
    }
}
