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
using Windows.UI.ViewManagement;
using Windows.UI;
using System.Collections.ObjectModel;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace 斗图UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static int BlurEffectConunt = 5;

        private ObservableCollection<ListInfo> ListSorce;
        private ObservableCollection<EmojiUiContent> WordsListSorce;

        private StorageFile CurrentFile=null;
        private string CurrentStr=null;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        //初始化
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            BlurEffectInit();
            setStateBar();
            InitEmojiIma();
            InitEmojiWords();
        }

        private void InitEmojiWords()
        {
            WordsListSorce = new ObservableCollection<EmojiUiContent>();
            EmojiWords.ItemsSource = WordsListSorce;
            string[] EmojiWordsList = { "MDZZ", "你咋不上天", "Lumia", "跟党走", "我爱你", "去死吧", "楼上智障", "我们走" };
            foreach (var str in EmojiWordsList)
            {
                WordsListSorce.Add(new EmojiUiContent()
                {
                    words = str
                });
            }
        }

        private async void InitEmojiIma()
        {
            ListSorce = new ObservableCollection<ListInfo>();
            Emoji.ItemsSource = ListSorce;
            try
            {
                var fileTemp = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Emoji/e1.jpg"));
                var folder = await fileTemp.GetParentAsync();
                var files = await folder.GetFilesAsync();
                foreach (var file in files)
                {
                    var temp = new ListInfo();
                    temp.name = file.Name;
                    var ima = new BitmapImage(new Uri(file.Path, UriKind.Absolute));
                    temp.ima = ima;
                    ListSorce.Add(temp);
                }
            }
            catch (Exception)
            {
            }
        }

        #region 页面事件

        private async void setStateBar()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var stateBar = StatusBar.GetForCurrentView();
                await stateBar.ShowAsync();
                stateBar.BackgroundOpacity = 1;
                stateBar.BackgroundColor = Colors.WhiteSmoke;
                stateBar.ForegroundColor = Colors.Black;
            }
        }
        private void sWord_GotFocus(object sender, RoutedEventArgs e)
        {
            inputGetFocus.Visibility = Visibility.Visible;
            inputLostFocus.Visibility = Visibility.Collapsed;
        }

        private void sWord_LostFocus(object sender, RoutedEventArgs e)
        {
            inputGetFocus.Visibility = Visibility.Collapsed;
            inputLostFocus.Visibility = Visibility.Visible;
        }

        private void Commandbar_Closing(object sender, object e)
        {
            copyBoard.IsCompact = true;
            speakResult.IsCompact = true;
            changeDesLanguage.IsCompact = true;
            appBarButtonTranslate.IsCompact = true;
        }

        private void Commandbar_Opened(object sender, object e)
        {
            copyBoard.IsCompact = false;
            speakResult.IsCompact = false;
            changeDesLanguage.IsCompact = false;
            appBarButtonTranslate.IsCompact = false;
        }

        private void sWord_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                return;
            var temp = e.Key.ToString();
            if (temp.Equals("Enter"))
            {

            }
        }
        #endregion

        #region 背景模糊
        private async void BlurEffectInit()
        {
            CurrentFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/StoreLogo.png"));
            BlurEffectInit(CurrentFile);
        }
        private async void BlurEffectInit(StorageFile file)
        {
            BackgroundCHange.Begin();
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

        private void test_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MakeIma();
        }

        private async void MakeIma()
        {
            if (NetImaProgress.IsActive)
            {
                return;
            }

            NetImaProgress.IsActive = !NetImaProgress.IsActive;
            try
            {
                var file = await new NetIma().getIma(@"http://legendzealot.xyz/addTXTImage/index.php?words=" + CurrentStr+"&file="+CurrentFile.Name);
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage b = new BitmapImage();
                    b.SetSource(stream);
                    test.Source = b;
                }
                sWord.Text = string.Empty;
                try
                {
                    BlurEffectInit(file);
                }
                catch (Exception)
                {
                    throw;
                }
                NetImaProgress.IsActive = !NetImaProgress.IsActive;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message, "失败！").ShowAsync();
                NetImaProgress.IsActive = !NetImaProgress.IsActive;
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
                case "制作":
                    MakeIma();
                    break;                    
            }
        }

        private async void Emoji_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var info = (ListInfo)e.ClickedItem;
                var path = "ms-appx:///Emoji/" + info.name;
                CurrentFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(path));
                using (IRandomAccessStream stream = await CurrentFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage b = new BitmapImage();
                    b.SetSource(stream);
                    test.Source = b;
                }
                BlurEffectInit(CurrentFile);
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message,"切换出错").ShowAsync();
            }
        }

        private void EmojiWordsDelect_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void EmojiWords_ItemClick(object sender, ItemClickEventArgs e)
        {
            var temp = (EmojiUiContent)e.ClickedItem;
            CurrentStr = temp.words;
            MakeIma();
        }

        private void sWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            CurrentStr = sWord.Text;
        }
    }
}
