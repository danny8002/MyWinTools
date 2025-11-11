using System;
using System.IO;

namespace MyWinTools.Utils
{
    public static class PathUtils
    {
        public static string GetAbsolutePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, path));
        }
    }
}