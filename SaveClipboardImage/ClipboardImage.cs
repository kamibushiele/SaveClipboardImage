using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SaveClipboardImage
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
        private HttpClient web;
        private ImageState nowImageType = ImageState.None;
        public bool ImageExist {get{return (nowImageType != ImageState.None);} }
        private Uri imageUri;
        private BitmapSource imageBmpSource;
        public ImageSource Image { get; private set; }//ImageSource.Sourceに代入する値
        public Action<string> AddLog { get; set; }
        public ClipboardImage(Action<string> addlog)
        {
            web = new HttpClient();
            AddLog = addlog;
        }
        EventHandler completeGetImage;
        private void ImageApply(Uri imageUri, ImageSource source, bool callbackNow = false)
        {
            this.imageUri = imageUri;
            nowImageType = ImageState.URL;
            ImageApply_(source, callbackNow);
        }
        private void ImageApply(BitmapSource imageBmpSource, ImageSource source, bool callbackNow = false)
        {
            this.imageBmpSource = imageBmpSource;
            nowImageType = ImageState.BitmapSource;
            ImageApply_(source, callbackNow);
        }
        private void ImageApply_(ImageSource source , bool callbackNow)
        {
            Image = source;
            if (callbackNow)
            {
                completeGetImage(null, new EventArgs());
            }
        }
        async Task<BitmapImage> GetHttpImage(Uri uri)//https://pierre3.hatenablog.com/entry/2015/10/24/152131
        {
            var bytes = await web.GetByteArrayAsync(uri).ConfigureAwait(false);
            using (var stream = new MemoryStream(bytes))
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
        public bool GetImage(EventHandler imageDownloaded)
        {
            completeGetImage = imageDownloaded;
            if (Clipboard.ContainsImage())
            {
                Bitmap tmpBmp = (Bitmap)Clipboard.GetData(DataFormats.Bitmap);
                AddLog("Found image");
                imageBmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    tmpBmp.GetHbitmap(),
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                ImageApply(imageBmpSource, imageBmpSource, true);//Bitmap Source
                return true;
            }
            if (Clipboard.ContainsText(TextDataFormat.Html))
            {
                string html = Clipboard.GetText(TextDataFormat.Html);
                var reg = new Regex(@"<[\s]*img[^>]*src[\s]*=[\s]*[""']([^""']*)['""][^>]*>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var match = reg.Match(html);
                if (!match.Success && match.Groups.Count != 2)
                {
                    AddLog("Not found <img> tag in clipboard");//HTMLにimgタグなし
                    return false;
                }
                string url = match.Groups[1].Value;
                string decodedUrl = System.Web.HttpUtility.HtmlDecode(url);//&;amp等を&等に直す
                BitmapImage bitmap;
                var uri = new Uri(decodedUrl);
                var scheme = uri.Scheme.ToLower();
                if (scheme == "http" || scheme == "https")//インターネットの画像
                {
                    var task = GetHttpImage(uri);
                    bitmap = task.Result;
                    ImageApply(bitmap, bitmap, true);
                    AddLog($"Found image in {url}");
                    return true;
                }
                else//(おそらく)ローカルの画像
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = uri;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;//これをしないとファイルを開放しない
                    bitmap.EndInit();
                    bitmap.Freeze();
                    ImageApply(uri, bitmap, true);
                    AddLog($"Found image in {url}");
                    return true;
                }
            }
            AddLog("Not found image or HTML in clipboard");
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
