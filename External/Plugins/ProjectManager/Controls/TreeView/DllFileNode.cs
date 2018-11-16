using System;
using System.Reflection;
using System.Windows.Forms;

namespace ProjectManager.Controls.TreeView
{
    public class DllFileNode : FileNode
    {
        bool isExplored;

        public DllFileNode(string filePath) : base(filePath)
        {
            Nodes.Add(new TreeNode());
        }

        public override void Refresh(bool recursive)
        {
            base.Refresh(recursive);
            if (isExplored && !IsExpanded)
            {
                isExplored = false;
                Nodes.Clear();
            }
            if (isExplored) Explore();
        }

        public override void BeforeExpand()
        {
            if (!isExplored) Explore();
        }

        void Explore()
        {
            isExplored = true;
            TreeView.BeginUpdate();
            Nodes.Clear();
            var assembly = Assembly.LoadFile(BackingPath);
            var modules = assembly.GetLoadedModules();
            foreach (var module in modules)
            {
                Type[] types = null;
                try
                {
                    types = module.GetTypes();
                }
                catch (Exception e)
                {
                    continue;
                }
                if (types.Length > 0)
                {
                    var node = new ClassesNode(BackingPath);
                    foreach (var type in types)
                    {
                        if(type.IsPublic) node.Nodes.Add(new ClassExportNode(BackingPath, type.Name));
                    }
                    Nodes.Add(node);
                }
            }
            TreeView.EndUpdate();
        }
    }
}