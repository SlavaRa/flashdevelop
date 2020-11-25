﻿using System.IO;

namespace SourceControl.Sources.Mercurial
{
    class AddCommand : BaseCommand
    {
        public AddCommand(string path)
        {
            string args = $"add \"{Path.GetFileName(path)}\"";
            Run(args, Path.GetDirectoryName(path));
        }
    }
}
