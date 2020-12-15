using Aga.Controls.Tree;
using Common;
using System.Drawing;

namespace PackFileManager.PackedTreeView
{

    public interface IPackEntryEventHandler
    {
        void Dir_FileAdded(PackEntry entry);
        void Dir_FileRemoved(PackEntry entry);
    }

    public class TreeNode : Node
    {
        public TreeNode(string text) : base(text) { }
        public Color? Colour { get; set; } = null;
        public string ToolTipText { get; set; }
        public bool IsFolder { get; set; } = false;
    }

    public class TreeViewModelCreator
    {
        TreeViewIconCreator _treeViewIconCreator;
        public TreeViewModelCreator()
        {
            _treeViewIconCreator = new TreeViewIconCreator();
            _treeViewIconCreator.Load();
        }

        public Node CreateNode(PackEntry packEntry, IPackEntryEventHandler eventHandler)
        {
            var newNode = new TreeNode(packEntry.Name);
            newNode.Tag = packEntry;

            var dir = packEntry as VirtualDirectory;
            if (dir != null)
            {
                if (eventHandler != null)
                {
                    dir.DirectoryAdded += eventHandler.Dir_FileAdded;
                    dir.FileAdded += eventHandler.Dir_FileAdded;
                    dir.FileRemoved += eventHandler.Dir_FileRemoved;
                }
                newNode.IsFolder = true;
                newNode.Image = _treeViewIconCreator.Folder;
            }
            else
            {
                newNode.Image = _treeViewIconCreator.DefaultFile;
            }

            return newNode;
        }

        public Node Create(PackEntry packEntry, IPackEntryEventHandler eventHandler, Node node = null)
        {
            var newParent = CreateNode(packEntry, null);
            if (node != null)
                node.Nodes.Add(newParent);
            else
                node = newParent;

            var dir = packEntry as VirtualDirectory;
            if (dir != null)
            {
                foreach (var subdirs in dir.Subdirectories.Values)
                {
                    Create(subdirs, eventHandler, newParent);
                }

                foreach (var file in dir.Files.Values)
                {
                    var newFile = CreateNode(file, eventHandler);
                    newParent.Nodes.Add(newFile);
                }
            }

            return newParent;
        }
    }
}
