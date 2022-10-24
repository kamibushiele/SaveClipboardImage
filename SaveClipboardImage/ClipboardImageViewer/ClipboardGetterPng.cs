using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace SaveClipboardImage.ClipboardImageViewer
{
    internal class ClipboardGetterPng : ClipBoardGetterImageAbs
    {
        public ClipboardGetterPng(Action<string> addLog) : base(addLog) { }
        override protected string ClipboardDataString() { return "PNG"; }
        override public ImageSource GetBitmapSourceByDataObject(IDataObject dataObject)
        {
            object data = GetData(dataObject);
            if (data != null)
            {
                var bitmap = GetBitmapImageByStream(data as MemoryStream);
                return bitmap;
            }
            else
            {
                return null;
            }
        }
    }
}
