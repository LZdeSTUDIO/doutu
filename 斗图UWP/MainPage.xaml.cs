using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace 斗图UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static int BlurEffectConunt = 2;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        //初始化
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            BlurEffectInit();
        }

        #region 背景模糊
        private async void BlurEffectInit()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/StoreLogo.png"));
            BlurEffectInit(file);
        }
        private async void BlurEffectInit(StorageFile file)
        {
            WriteableBitmap wb;
            wb = new WriteableBitmap(1000, 1000);
            // Ensure a file was selected
            if (file != null)
            {
                // Set the source of the WriteableBitmap to the image stream
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await wb.SetSourceAsync(fileStream);
                }
            }
            WriteableBitmap new_bitmap = await BlurEffect.BitmapClone(wb);
            // 添加高斯滤镜效果
            MyImage mi = new MyImage(new_bitmap);
            GaussianBlurFilter filter = new GaussianBlurFilter();
            filter.Sigma = BlurEffectConunt;
            filter.process(mi);
            // 图片添加完滤镜的 int[] 数组
            int[] array = mi.colorArray;
            // byte[] 数组的长度是 int[] 数组的 4倍
            byte[] result = new byte[array.Length * 4];
            // 通过自加，来遍历 byte[] 数组中的值
            int j = 0;
            for (int i = 0; i < array.Length; i++)
            {
                // 同时把 int 值中 a、r、g、b 的排列方式，转换为 byte数组中 b、g、r、a 的存储方式 
                result[j++] = (byte)(array[i]);       // Blue
                result[j++] = (byte)(array[i] >> 8);  // Green
                result[j++] = (byte)(array[i] >> 16); // Red
                result[j++] = (byte)(array[i] >> 24); // Alpha
            }
            // Open a stream to copy the image contents to the WriteableBitmap's pixel buffer 
            using (Stream stream = new_bitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(result, 0, result.Length);
            }
            back.Source = new_bitmap;
        }
        #endregion

        #region 图片分享
        private void ShareIma()
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        //从指定位置加载文件
        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            try
            {
                DataPackage requestData = args.Request.Data;
                requestData.Properties.Title = "test";
                requestData.Properties.Description = "content";
                var local = ApplicationData.Current.LocalFolder;
                var folder = await local.CreateFolderAsync("斗图", CreationCollisionOption.OpenIfExists);
                var file = await folder.CreateFileAsync("temp" + ".png", CreationCollisionOption.OpenIfExists);
                List<IStorageItem> imageItems = new List<IStorageItem> { file };
                requestData.SetStorageItems(imageItems);
                RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromFile(file);
                requestData.Properties.Thumbnail = imageStreamRef;
                requestData.SetBitmap(imageStreamRef);
            }
            catch (Exception ex)
            {
                await new MessageDialog("错误代码：" + ex.Message, "请重试！!").ShowAsync();
            }
        }
        #endregion

        private async void test_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var  file = await new NetIma().getIma(@"http://legendzealot.xyz/addTXTImage/index.php");
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage b = new BitmapImage();
                    b.SetSource(stream);
                    test.Source = b;
                }
                try
                {
                    BlurEffectInit(file);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message, "失败！").ShowAsync();
            }
        }

        private void BottomItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var appButton = (AppBarButton)sender;
            var lable = appButton.Label.ToString();
            switch (lable)
            {
                case "分享":
                    ShareIma();
                    break;
            }
        }
    }
}
