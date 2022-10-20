using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public enum Mode
        {
            SetFileAndDirGUI,//出力場所・ファイル名をGUIで決める
            SetFileGUI,//ファイル名だけGUIで決める
            NoGUI,//GUIを出さない
            Error,
        }
        public Mode mode { get; private set; }
        public string outputDirPath { get; private set; }
        public string outputFilrPath { get; private set; }
        public StartMode(string outputPath)
        {
            try
            {
                if (outputPath == null || outputPath == "")
                {
                    mode = Mode.SetFileAndDirGUI;
                    outputFilrPath = "";
                    outputDirPath = Path.GetFullPath("./");
                }
                else if (Path.HasExtension(outputPath))
                {
                    mode = Mode.NoGUI;
                    outputFilrPath = Path.GetFullPath(outputPath);
                    outputDirPath = "";
                }
                else
                {
                    mode = Mode.SetFileGUI;
                    outputFilrPath = "";
                    outputDirPath = Path.GetFullPath(outputPath);
                }
            }
            catch (Exception)
            {
                mode = Mode.Error;
                outputFilrPath = "";
                outputDirPath = "";
                Console.WriteLine("path error. invaild path.");
            }
        }
        public StartMode()
        {
            mode = Mode.Error;
            outputFilrPath = "";
            outputDirPath = "";
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
                    Mode = new StartMode(res.OutputPath);
                    break;
                default:
                    Mode = new StartMode();
                    break;
            }
        }
    }
}
