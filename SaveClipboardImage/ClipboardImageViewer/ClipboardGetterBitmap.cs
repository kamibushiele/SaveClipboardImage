using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SaveClipboardImage.ClipboardImageViewer
{
    internal class ClipboardGetterBitmap : ClipBoardGetterImageAbs
    {
        public ClipboardGetterBitmap(Action<string> addLog) : base(addLog) { }
        override protected string ClipboardDataString() { return "Bitmap"; }
        //override protected string ClipboardDataString() { return "System.Windows.Media.Imaging.BitmapSource"; }
        //override protected string ClipboardDataString() { return "System.Drawing.Bitmap"; }//not support .Net 6
        override public ImageSource GetBitmapSourceByDataObject(IDataObject dataObject)
        {
            object data = GetData(dataObject);
            if (data != null)
            {
                var bitmap = data as BitmapSource;
                var fmter = new FormatConvertedBitmap(bitmap,PixelFormats.Bgr32,null,0);//アルファチャネルがあるとなせか表示やpng保存がおかしくなる
                fmter.Freeze();
                return fmter;
            }
            else
            {
                return null;
            }
        }
    }
}
