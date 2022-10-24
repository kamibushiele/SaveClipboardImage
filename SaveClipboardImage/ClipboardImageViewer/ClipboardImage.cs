using SaveClipboardImage.ClipboardImageViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClipboardImageViewer
{
    public class ImageExtension
    {
        public string FilePathWithExtension { get; private set; }
        public BitmapEncoder Encoder { get; private set; }
        private static string[] JpgExtension = {
            ".jpg",
            ".jpe",
            ".jpeg",
        };
        private static string[] PngExtension = {
            ".png",
        };
        public static string DialogFilter 
        {
            get {
                string ret ="";
                ret += "PNG image|";
                foreach (var ext in PngExtension)
                {
                    ret += "*" + ext + ";";
                }
                ret += "|Jpeg image|";
                foreach (var ext in JpgExtension)
                {
                    ret += "*" + ext + ";";
                }
                return ret;
            }
        }
        public ImageExtension(string filepath)
        {
            var extention = Path.GetExtension(filepath).ToLower();
            if (JpgExtension.Contains(extention))
            {
                FilePathWithExtension = filepath;
                Encoder =  new JpegBitmapEncoder();
            }
            else if (PngExtension.Contains(extention))
            {
                FilePathWithExtension = filepath;
                Encoder = new PngBitmapEncoder();
            }
            else
            {
                FilePathWithExtension = filepath + ".png";
                Encoder = new PngBitmapEncoder();
            }
        }
    }
    public class ClipboardImage
    {
        enum ImageState
        {
            None,
            URL,
            BitmapSource,
        }
        public enum ClipboardMode
        {
            Undefined,
            System_Windows,
            System_Windows_Forms
        }
        private ImageState nowImageType = ImageState.None;
        public bool ImageExist {get{return (nowImageType != ImageState.None);} }
        private Uri imageUri;
        private BitmapSource imageBmpSource;
        public ImageSource Image { get; private set; }//ImageSource.Sourceに代入する値
        public Action<string> AddLog { get; set; }
        public ClipboardImage(Action<string> addlog)
        {
            AddLog = addlog;
        }
        private void ImageApply(Uri imageUri, ImageSource source)
        {
            this.imageUri = imageUri;
            nowImageType = ImageState.URL;
            Image = source;
        }
        private void ImageApply(BitmapSource imageBmpSource, ImageSource source)
        {
            this.imageBmpSource = imageBmpSource;
            nowImageType = ImageState.BitmapSource;
            Image = source;
        }
        public bool GetImage()
        {
            var getterList = new List<ClipBoardGetterImageAbs>();

            getterList.Add(new ClipboardGetterPng(AddLog));
            //getterList.Add(new ClipboardGetterFormat17(AddLog));//不具合があるから未実装
            getterList.Add(new ClipboardGetterHtml(AddLog));
            getterList.Add(new ClipboardGetterBitmap(AddLog));

            var clipboard = Clipboard.GetDataObject();
            //MessageBox.Show(string.Join("\n", clipboard.GetFormats(true)));
            foreach (var getter in getterList)
            {
                var bitmap = getter.GetBitmapSourceByDataObject(clipboard);
                if(bitmap != null)
                {
                    ImageApply(bitmap as BitmapSource, bitmap);
                    return true;
                }
            }
            return false;
        }
        public bool Save(string path)
        {
                BitmapFrame frame;
                switch (nowImageType)
                {
                    case ImageState.URL:
                        frame = BitmapFrame.Create(imageUri);
                        break;
                    case ImageState.BitmapSource:
                        frame = BitmapFrame.Create(imageBmpSource);//インターネット画像を保存しようとするとNotSupportedException(内部で握りつぶされているので問題なし)
                    break;
                    default:
                        return false;
            }
            try
            {
                var imgextension = new ImageExtension(path);
                AddLog($"save to {imgextension.FilePathWithExtension}");
                using (Stream stream = new FileStream(imgextension.FilePathWithExtension, FileMode.Create))
                {
                    var encoder = imgextension.Encoder;
                    encoder.Frames.Add(frame);
                    encoder.Save(stream);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
