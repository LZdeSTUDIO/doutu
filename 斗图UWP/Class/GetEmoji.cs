using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace 斗图UWP
{

    class ListInfo
    {
        public BitmapImage ima { set; get; }
        public string name { set; get; }
    }

    class GetEmoji
    {
        public List<ListInfo> ListSorce;

        public GetEmoji()
        {
            Init();
        }

        private async void Init()
        {
            try
            {
                ListSorce = new List<ListInfo>();
                var fileTemp = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Emoji/11.jpg"));
                var folder = await fileTemp.GetParentAsync();
                var files = await folder.GetFilesAsync();
                foreach (var file in files)
                {
                    var temp = new ListInfo();
                    temp.name = file.Name;
                    //var tempBitmap = new BitmapImage();// Set the source of the WriteableBitmap to the image stream
                    //using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                    //{
                    //    await tempBitmap.SetSourceAsync(fileStream);
                    //}
                    var ima = new BitmapImage(new Uri(file.Path, UriKind.Absolute));
                    temp.ima = ima;
                    ListSorce.Add(temp);
                }
            }
            catch (Exception )
            {
            }
        }
    }
}
