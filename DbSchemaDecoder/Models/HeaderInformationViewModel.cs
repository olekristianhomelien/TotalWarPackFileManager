using CommonDialogs.Common;
using DbSchemaDecoder.Controllers;
using Filetypes;
using Filetypes.DB;
using System.Collections.Generic;
using System.Linq;

namespace DbSchemaDecoder.Models
{
    public class HeaderInformationViewModel : NotifyPropertyChangedImpl
    {
        public void Update(DBFileHeader header, DataBaseFile item)
        {
            if (header != null)
            {
                TableName = item.TableType;
                Version = header.Version;
                HeaderSize = header.Length;
                ExpectedEntries = (int)header.EntryCount;
                TotalSize = item.DbFile.Data.Length;
            }
            else
            {
                TableName = "";
                Version = 0;
                HeaderSize = 0;
                ExpectedEntries = 0;
                TotalSize = 0;
                NumVersions = 0;
            }


            var allTableDefinitions = SchemaManager.Instance.GetTableDefinitionsForTable(item.TableType);

            NumVersions = allTableDefinitions.Count();
            var groupedVersions = allTableDefinitions.GroupBy(x => x.Version).ToList();
            Versions = new List<VersionViewItem>();

            var selectedIndex = -1;
            for(int i = 0; i < groupedVersions.Count(); i++)
            {
                if (groupedVersions[i].Count() == 1)
                {
                    var newitem = new VersionViewItem()
                    {
                        DisplayValue = "Version " + groupedVersions[i].Key.ToString(),
                        TypeInfo = groupedVersions[i].First()
                    };
                    Versions.Add(newitem);

                    if (groupedVersions[i].Key == Version)
                        selectedIndex = Versions.Count() -1;
                }
                else
                {
                    for (int j = 0; j < groupedVersions[i].Count(); j++)
                    {
                        var newitem = new VersionViewItem()
                        {
                            DisplayValue = "Version " + groupedVersions[i].Key.ToString() + " - Instance " + (j + 1).ToString(),
                            TypeInfo = groupedVersions[i].ElementAt(j)
                        };

                        Versions.Add(newitem);

                        if (groupedVersions[i].Key == Version && j == 0)
                            selectedIndex = Versions.Count() - 1;
                    }
                }
            }

            NumVersions = Versions.Count();


            NotifyPropertyChanged("TableName");
            NotifyPropertyChanged("Version");
            NotifyPropertyChanged("HeaderSize");
            NotifyPropertyChanged("ExpectedEntries");
            NotifyPropertyChanged("TotalSize");
            NotifyPropertyChanged("Versions");
            NotifyPropertyChanged("NumVersions");

            if (selectedIndex != -1)
                SelectedVersion = Versions[selectedIndex].DisplayValue;
        }
        public string TableName { get; set; }
        public int HeaderSize { get; set; }
        public int Version { get; set; }
        public int ExpectedEntries { get; set; }
        public int TotalSize { get;set;}
        public int NumVersions { get; set; }

        public List<VersionViewItem> Versions { get; set; } = new List<VersionViewItem>();

        string _selectedItem;
        public string SelectedVersion 
        { 
            get { return _selectedItem; } 
            set { _selectedItem = value; NotifyPropertyChanged(); } 
        }

        public class VersionViewItem
        { 
            public DbTableDefinition TypeInfo { get; set; }
            public string DisplayValue { get; set; } 
        }
    }
}
