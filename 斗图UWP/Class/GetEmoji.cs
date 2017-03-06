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
        public StorageFile file { set; get; }
    }
}
