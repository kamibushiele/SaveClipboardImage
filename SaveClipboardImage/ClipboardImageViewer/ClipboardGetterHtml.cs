using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SaveClipboardImage.ClipboardImageViewer
{
    internal class ClipboardGetterHtml : ClipBoardGetterImageAbs
    {
        private HttpClient web;
        public ClipboardGetterHtml(Action<string> addLog) : base(addLog)
        {
            web = new HttpClient();
        }
        override protected string ClipboardDataString() { return "HTML Format"; }
        override public ImageSourceStruct GetBitmapSourceByDataObject(IDataObject dataObject)
        {
            object data = GetData(dataObject);
            if (data != null)
            {
                string html = Clipboard.GetText(TextDataFormat.Html);
                var reg = new Regex(@"<[\s]*img[^>]*src[\s]*=[\s]*[""']([^""']*)['""][^>]*>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var match = reg.Match(html);
                if (!match.Success && match.Groups.Count != 2)
                {
                    AddLog("Not found <img> tag in clipboard");//HTMLにimgタグなし
                    return new ImageSourceStruct {};
                }
                string url = match.Groups[1].Value;
                string decodedUrl = System.Web.HttpUtility.HtmlDecode(url);//&;amp等を&等に直す
                var uri = new Uri(decodedUrl);
                var scheme = uri.Scheme.ToLower();
                if (scheme == "http" || scheme == "https")//インターネットの画像
                {
                    AddLog($"http/https Image");
                    var bytes = web.GetByteArrayAsync(uri).Result;
                    var bitmap = new BitmapImage();
                    using (var stream = new MemoryStream(bytes))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                    return new ImageSourceStruct { Source=bitmap, SourcePath = decodedUrl };
                }
                else if(scheme == "file")//ローカルの画像
                {
                    AddLog($"Local Image");
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = uri;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;//これをしないとファイルを開放しない
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return new ImageSourceStruct { Source = bitmap, SourcePath = uri.LocalPath + Uri.UnescapeDataString(uri.Fragment)};
                }
                else if(scheme == "data")//Data URL
                {
                    AddLog($"Data URL Image");
                    // "data:image/jpeg;base64,xxxxxxxxxxxx"
                    var regb64 = new Regex(@"data:image/[^;]+;base64,(.*)");
                    var b64match = regb64.Match(decodedUrl);
                    if(!b64match.Success && match.Groups.Count != 1)
                    {
                        return new ImageSourceStruct { };
                    }
                    var imgbin = System.Convert.FromBase64String(b64match.Groups[1].Value);
                    var bitmap = new BitmapImage();
                    using (var ms = new MemoryStream(imgbin))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                    return new ImageSourceStruct { Source = bitmap, SourcePath = decodedUrl };
                }
                else//不明
                {
                    AddLog($"Unsupptorted URI scheme:{scheme}");
                    return new ImageSourceStruct { };
                }
            }
            else
            {
                return new ImageSourceStruct { };
            }
        }
    }
}
