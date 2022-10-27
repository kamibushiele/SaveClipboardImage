using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;

namespace SaveClipboardImage
{
    public class CliOptions
    {
        [CommandLine.Option('o', "output", Required = false, HelpText = "Output file path or output directory.")]
        public string OutputPath { get; set; }
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("set output file and directory in GUI", new CliOptions {}),
                    new Example(@"set output directory to ""C:\output""", new CliOptions { OutputPath = @"C:\output" }),
                    new Example(@"save file to ""C:\output.png""", new CliOptions { OutputPath = @"C:\output.png" }),
                };
            }
        }
    }
    public class StartMode
    {
        public enum ModeEnum
        {
            SetFileAndDirGUI,//出力場所・ファイル名をGUIで決める
            SetFileGUI,//ファイル名だけGUIで決める
            NoGUI,//GUIを出さない
            Error,
        }
        public ModeEnum Mode { get; private set; }
        public string OutputDirPath { get; private set; }
        public string OutputFilrPath { get; private set; }
        public StartMode(CliOptions option)
        {
            var outputPath = option.OutputPath;
            try
            {
                if (outputPath == null || outputPath == "")
                {
                    Mode = ModeEnum.SetFileAndDirGUI;
                    OutputFilrPath = "";
                    OutputDirPath = Path.GetFullPath("./");
                }
                else if (Directory.Exists(outputPath))
                {
                    Mode = ModeEnum.SetFileGUI;
                    OutputFilrPath = "";
                    OutputDirPath = Path.GetFullPath(outputPath);
                }
                else
                {
                    Mode = ModeEnum.NoGUI;
                    OutputFilrPath = Path.GetFullPath(outputPath);
                    OutputDirPath = "";
                }
            }
            catch (Exception)
            {
                Mode = ModeEnum.Error;
                OutputFilrPath = "";
                OutputDirPath = "";
                Console.WriteLine("path error. invaild path.");
            }
            Console.WriteLine($"Mode : {Mode}");
        }
        public StartMode()
        {
            Mode = ModeEnum.Error;
            OutputFilrPath = "";
            OutputDirPath = "";
        }
    }
    internal class Commandline
    {
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]//コマンドライン表示に必要
        public static extern bool AttachConsole(int processId);//コマンドライン表示に必要

        public StartMode Mode{ get; private set; }
        public Commandline()
        {
            AttachConsole(-1);//コマンドライン表示に必要
            //https://github.com/commandlineparser/commandline/wiki
            var parserResult = Parser.Default.ParseArguments<CliOptions>(Environment.GetCommandLineArgs());
            switch (parserResult.Tag)
            {
                case ParserResultType.Parsed:
                    var res = parserResult.Value;
                    Mode = new StartMode(res);
                    break;
                default:
                    Mode = new StartMode();
                    break;
            }
        }
    }
}
