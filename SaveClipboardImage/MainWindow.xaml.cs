using ClipboardImageViewer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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
            switch (AppMode.Mode)
            {
                case StartMode.ModeEnum.NoGUI:
                    img.GetImage();
                    img.Save(AppMode.OutputFilrPath);
                    Close();
                    return;
                case StartMode.ModeEnum.SetFileGUI:
                case StartMode.ModeEnum.SetFileAndDirGUI:
                    AddLog($"Mode : {AppMode.Mode}");
                    break;
                default:
                    Console.WriteLine("Error");
                    Close();
                    return;
            }
            img.AddLog = AddLog;
            //ウィンドウタイトルをファイルパスにする
            var path = new List<string>(AppMode.OutputDirPath.Split('\\'));
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
                Title = AppMode.OutputDirPath;
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
        private void UpdateImage()
        {
            var result = img.GetImage();
            if(result == true)
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
        }
        private void SaveImage()
        {
            if (!img.ImageExist)
            {
                AddLog("No Image.");
                return;
            }
            if(AppMode.Mode == StartMode.ModeEnum.SetFileGUI)
            {
                img.Save(System.IO.Path.Combine(AppMode.OutputDirPath, FileName.Text));
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
            FileName.Focus();
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.KeyDownEvent, new KeyEventHandler(TextBox_KeyDown));
        }
        void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter & (sender as TextBox).AcceptsReturn == false) MoveToNextUIElement(e);
        }
        void MoveToNextUIElement(KeyEventArgs e)
        {
            // Creating a FocusNavigationDirection object and setting it to a
            // local field that contains the direction selected.
            FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;

            // MoveFocus takes a TraveralReqest as its argument.
            TraversalRequest request = new TraversalRequest(focusDirection);

            // Gets the element with keyboard focus.
            UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;

            // Change keyboard focus.
            if (elementWithFocus != null)
            {
                if (elementWithFocus.MoveFocus(request)) e.Handled = true;
            }
        }

        private void FileName_GotFocus(object sender, RoutedEventArgs e)
        {
            FileName.SelectAll();
        }

        private void FileName_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FileName.IsFocused)
            {
                return;
            }
            FileName.Focus();
            e.Handled = true;
        }
    }
}
