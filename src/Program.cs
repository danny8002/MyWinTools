using System;
using CommandLine;
using MyWinTools.Commands;

namespace MyWinTools
{
    internal class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<DeleteCommand, MoreCommand, NotepadCommand, Notepad2Command, BeyondCompareCommand, RootCommand, CopyCommand>(args)
                .MapResult(
                    (DeleteCommand cmd) => cmd.Execute(),
                    (MoreCommand cmd) => cmd.Execute(),
                    (NotepadCommand cmd) => cmd.Execute(),
                    (Notepad2Command cmd) => cmd.Execute(),
                    (BeyondCompareCommand cmd) => cmd.Execute(),
                    (RootCommand cmd) => cmd.Execute(),
                    (CopyCommand cmd) => cmd.Execute(),
                    errors => 1
                );
        }
    }
}
