using System;
using System.IO;
using CommandLine;
using MyWinTools.Utils;

namespace MyWinTools.Commands
{
    [Verb("--root", HelpText = "Find the git repository root directory")]
    public class RootCommand
    {
        [Value(0, Required = false, HelpText = "Optional path to start searching from")]
        public string Path { get; set; }

        public int Execute()
        {
            try
            {
                string startPath = Path != null 
                    ? PathUtils.GetAbsolutePath(Path)
                    : Environment.CurrentDirectory;

                if (File.Exists(startPath))
                {
                    startPath = System.IO.Path.GetDirectoryName(startPath);
                }

                if (!Directory.Exists(startPath))
                {
                    Console.WriteLine($"Path not found: {startPath}");
                    return 1;
                }

                string gitRoot = FindGitRoot(startPath);
                if (gitRoot != null)
                {
                    Console.WriteLine(gitRoot);
                    if (Path == null)
                    {
                        Directory.SetCurrentDirectory(gitRoot);
                    }
                    return 0;
                }
                else
                {
                    Console.WriteLine("No git repository found in parent directories");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        private string FindGitRoot(string path)
        {
            while (path != null)
            {
                if (Directory.Exists(System.IO.Path.Combine(path, ".git")))
                {
                    return path;
                }
                path = System.IO.Path.GetDirectoryName(path);
            }
            return null;
        }
    }
}