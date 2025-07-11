using System;
using System.Diagnostics;
using System.Linq;

using CommandLine;
using MyWinTools.Commands;

namespace MyWinTools
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if(args.Any(s=> s == "-d"))
            {
                Debugger.Launch();
                args = args.Where(s => s != "-d").ToArray();
            }

            return Parser.Default.ParseArguments<DeleteCommand, MoreCommand, NotepadCommand, Notepad2Command, BeyondCompareCommand, RootCommand, CopyCommand, MsnVpCommand>(args)
                .MapResult(
                    (DeleteCommand cmd) => cmd.Execute(),
                    (MoreCommand cmd) => cmd.Execute(),
                    (NotepadCommand cmd) => cmd.Execute(),
                    (Notepad2Command cmd) => cmd.Execute(),
                    (BeyondCompareCommand cmd) => cmd.Execute(),
                    (RootCommand cmd) => cmd.Execute(),
                    (CopyCommand cmd) => cmd.Execute(),
                    (MsnVpCommand cmd) => cmd.Execute(),
                    errors => 1
                );
        }
    }
}
