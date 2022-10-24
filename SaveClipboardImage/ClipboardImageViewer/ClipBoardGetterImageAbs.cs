using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SaveClipboardImage.ClipboardImageViewer
{
    internal abstract class ClipBoardGetterImageAbs
    {
        abstract protected string ClipboardDataString();
        protected Action<string> AddLog { get; private set; }
        public ClipBoardGetterImageAbs(Action<string> addLog)
        {
            AddLog = addLog;
        }
        abstract public ImageSource GetBitmapSourceByDataObject(IDataObject dataObject);
        protected object GetData(IDataObject dataObject)
        {
            var data = dataObject.GetData(ClipboardDataString());
            var txt = $"{ClipboardDataString()}? : ";
            if (data != null)
            {
                AddLog($"{txt} Yes");
                //AddLog($"data type : {data.GetType().ToString()}");
            }
            else
            {
                AddLog($"{txt} No");
            }
            return data;

        }
        protected BitmapImage GetBitmapImageByStream(Stream stream)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
    }
}
