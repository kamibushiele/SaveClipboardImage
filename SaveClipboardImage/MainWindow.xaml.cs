using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Microsoft.Win32;

namespace SaveClipboardImage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private ClipboardImage img;
        private StartMode AppMode;
        public MainWindow()
        {
            InitializeComponent();
            var cli = new Commandline();
            AppMode = cli.Mode;
            img = new ClipboardImage((str) => { Console.WriteLine(str); });
            switch (AppMode.mode)
            {
                case StartMode.Mode.NoGUI:
                    UpdateImage();
                    img.Save(AppMode.outputFilrPath);
                    Close();
                    return;
                case StartMode.Mode.SetFileGUI:
                case StartMode.Mode.SetFileAndDirGUI:
                    AddLog($"Mode:{AppMode.mode}");
                    break;
                default:
                    Console.WriteLine("Error");
                    Close();
                    return;
            }
            img.AddLog = AddLog;
            //ウィンドウタイトルをファイルパスにする
            var path = new List<string>(AppMode.outputDirPath.Split('\\'));
            const int prefix = 2;//c:\xxx
            const int suffix = 3;//\yyy\zzz\
            if (path.Count > prefix+suffix)
            {
                path.RemoveRange(prefix, path.Count-(prefix + suffix));
                path.Insert(2,"...");
                Title = string.Join("\\", path);
            }
            else
            {
                Title = AppMode.outputDirPath;
            }
        }
        private void AddLog(string message)
        {
            Log.Text += message + "\n";
            Log.ScrollToEnd();
        }

        private void DoClearLog()
        {
            Log.Text = "";
        }
        private void ImageUpdated(object sender, EventArgs e)
        {
            if (img.ImageExist && img.Image != null)
            {
                Preview.Source = img.Image;
                string size;
                if (img.Image is BitmapSource)
                {
                    var bmpsrc = (BitmapSource)img.Image;
                    size = $"{bmpsrc.PixelWidth} x {bmpsrc.PixelHeight}";
                }
                else
                {
                    size = $"{img.Image.Width} x {img.Image.Height}";
                }
                imgSizeTextBlock.Text = $"W x H = {size}";
                AddLog($"size: {size}");
            }
            else
            {

                Preview.Source = null;
                imgSizeTextBlock.Text = "";
            }
        }
        private void UpdateImage()
        {
            if(img.GetImage(new EventHandler(ImageUpdated)))
            {

            }
        }
        private void SaveImage()
        {
            if (!img.ImageExist)
            {
                AddLog("No Image.");
                return;
            }
            if(AppMode.mode == StartMode.Mode.SetFileGUI)
            {
                img.Save(System.IO.Path.Combine(AppMode.outputDirPath, FileName.Text));
                Close();
                return;
            }
            var dialog = new SaveFileDialog();
            dialog.Filter = ImageExtension.DialogFilter;
            dialog.FileName = FileName.Text;
            if (dialog.ShowDialog() == true)
            {
                img.Save(dialog.FileName);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveImage();
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            UpdateImage();
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            DoClearLog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateImage();
            FileName.Text = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        }
    }
}
