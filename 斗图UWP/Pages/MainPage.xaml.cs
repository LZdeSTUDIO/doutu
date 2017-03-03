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
using Windows.UI.Core;
using Windows.System;
using Windows.Graphics.Imaging;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Media;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace 斗图UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static int BlurEffectConunt = 5;

        private ObservableCollection<ListInfo> ListSorce;
        private ObservableCollection<EmojiUiContent> WordsListSorce;

        private WriteableBitmap new_bitmap;

        private StorageFile CurrentFile = null;

        private delegate void DoneEvent(byte[] result);
        private event DoneEvent myDoneEventDelegate;
        private bool isPlay = false;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        //初始化
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            setStateBar();
            //CurrentFileLocal = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Emoji/e1.jpg"));
            myDoneEventDelegate += MainPage_myDoneEventDelegate;
            await InitEmojiIma();
            await InitEmojiWords();
            BlurEffectInit();
        }

        private async void MainPage_myDoneEventDelegate(byte[] result)
        {
            // Open a stream to copy the image contents to the WriteableBitmap's pixel buffer 
            using (Stream stream = new_bitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(result, 0, result.Length);
            }
            back.Source = new_bitmap;
            isPlay = false;
        }

        private async Task InitEmojiWords()
        {
            WordsListSorce = new ObservableCollection<EmojiUiContent>();
            EmojiWords.ItemsSource = WordsListSorce;
            string[] EmojiWordsList = { "MDZZ", "你咋不上天", "Lumia", "跟党走", "我爱你", "去死吧", "楼上智障", "我们走" };
            foreach (var str in EmojiWordsList)
            {
                var temp = new EmojiUiContent();
                var fileF = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Pics/left.png"));
                using (IRandomAccessStream fileStream = await fileF.OpenAsync(FileAccessMode.Read))
                {
                    temp.fIma = new BitmapImage();
                    await temp.fIma.SetSourceAsync(fileStream);
                }
                var fileB = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Pics/right.png"));
                using (IRandomAccessStream fileStream = await fileB.OpenAsync(FileAccessMode.Read))
                {
                    temp.bIma = new BitmapImage();
                    await temp.bIma.SetSourceAsync(fileStream);
                }
                temp.words = str;
                WordsListSorce.Add(temp);

                //WordsListSorce.Add(new EmojiUiContent()
                //{
                //    fIma = new BitmapImage(new Uri("ms-appx:///Pics/left.png")),
                //    bIma = new BitmapImage(new Uri("ms-appx:///Pics/right.png")),
                //    words = str
                //});
                //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                //{

                //});
                //await Task.Delay(10);
            }
            //Task.Run(() => { WordWork(); });
        }

        private async Task InitEmojiIma()
        {
            ListSorce = new ObservableCollection<ListInfo>();
            Emoji.ItemsSource = ListSorce;
            try
            {
                var fileTemp = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Emoji/e1.png"));
                var folder = await fileTemp.GetParentAsync();
                var files = await folder.GetFilesAsync();
                foreach (var file in files)
                {
                    var ima = new BitmapImage();
                    using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        await ima.SetSourceAsync(fileStream);
                    }
                    var temp = new ListInfo();
                    temp.name = file.Name;
                    temp.ima = ima;
                    ListSorce.Add(temp);
                    //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //{
                    //});
                    //await Task.Delay(10);
                }
            }
            catch (Exception)
            {
            }
            //await Task.Run(() => { ImaWork(); });
        }

        public  void ImaWork()
        {
        }

        public /*async */void WordWork()
        {
        }

        #region 页面事件

        private async void setStateBar()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var stateBar = StatusBar.GetForCurrentView();
                await stateBar.ShowAsync();
                stateBar.BackgroundOpacity = 1;
                stateBar.BackgroundColor = Colors.RoyalBlue;
                stateBar.ForegroundColor = Colors.WhiteSmoke;
            }
            //那就是在电脑上运行
            else
            {
                //更改标题栏颜色
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.BackgroundColor = Colors.DodgerBlue;
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
        }

        private void sWord_GotFocus(object sender, RoutedEventArgs e)
        {
            Commandbar.IsOpen = false;
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                Commandbar.Visibility = Visibility.Collapsed;
            }
            inputGetFocus.Visibility = Visibility.Visible;
            inputLostFocus.Visibility = Visibility.Collapsed;
        }

        private void sWord_LostFocus(object sender, RoutedEventArgs e)
        {
            sWord.IsEnabled = false;
            InputOut.Begin();
            inputGetFocus.Visibility = Visibility.Collapsed;
            inputLostFocus.Visibility = Visibility.Visible;
            Commandbar.Visibility = Visibility.Visible;
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
            var temp = e.Key.ToString();
            if (temp.Equals("Enter"))
            {
                wordTextBlock.Text = sWord.Text;
                sWord.Text = string.Empty;
                sWord.IsEnabled = false;
                MakeIma();
            }
        }
        #endregion

        #region 背景模糊
        private async void BlurEffectInit()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Emoji/e1.png"));
            BlurEffectInit(file);
        }
        private async void BlurEffectInit(StorageFile file)
        {
            BackgroundCHange.Begin();
            WriteableBitmap wb;
            wb = new WriteableBitmap(1000, 1000);
            // Ensure a file was selected
            if (file != null)
            {
                try
                {
                    // Set the source of the WriteableBitmap to the image stream
                    using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        await wb.SetSourceAsync(fileStream);
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
            new_bitmap = await BlurEffect.BitmapClone(wb);
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
            myDoneEventDelegate(result);
        }
        private void BackgroundCHange_Completed(object sender, object e)
        {
            while (isPlay) { };
            BackgroundCHangeBack.Begin();
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
                requestData.Properties.Title = "斗图UWP";
                requestData.Properties.Description = "表情分享";
                //var local = ApplicationData.Current.LocalFolder;
                //var folder = await local.CreateFolderAsync("斗图", CreationCollisionOption.OpenIfExists);
                //var file = await folder.CreateFileAsync("temp" + ".png", CreationCollisionOption.OpenIfExists);
                List<IStorageItem> imageItems = new List<IStorageItem> { CurrentFile };
                requestData.SetStorageItems(imageItems);
                RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromFile(CurrentFile);
                requestData.Properties.Thumbnail = imageStreamRef;
                requestData.SetBitmap(imageStreamRef);
            }
            catch (Exception ex)
            {
                await new MessageDialog("错误代码：" + ex.Message, "请重试！!").ShowAsync();
            }
        }
        #endregion

        #region 制作表情
        private async void MakeIma()
        {
            if (NetImaProgress.IsActive)
            {
                return;
            }
            isPlay = true;
            sWord.Text = string.Empty;
            NetImaProgress.IsActive = !NetImaProgress.IsActive;
            await MakeEmoji();
            BlurEffectInit(CurrentFile);
            NetImaProgress.IsActive = !NetImaProgress.IsActive;
        }
        private async Task MakeEmoji()
        {
            var local = ApplicationData.Current.LocalFolder;
            var folder = await local.CreateFolderAsync("斗图", CreationCollisionOption.OpenIfExists);
            var sFile = await folder.CreateFileAsync("temp" + ".png", CreationCollisionOption.ReplaceExisting);
            CachedFileManager.DeferUpdates(sFile);
            //把控件变成图像  
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            //传入参数Image控件  
            LogoTextBlock.Visibility = Visibility.Visible;
            await renderTargetBitmap.RenderAsync(pictureRoot);
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            LogoTextBlock.Visibility = Visibility.Collapsed;
            using (var fileStream = await sFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                encoder.SetPixelData(
               BitmapPixelFormat.Bgra8,
               BitmapAlphaMode.Ignore,
               (uint)renderTargetBitmap.PixelWidth,
               (uint)renderTargetBitmap.PixelHeight,
               DisplayInformation.GetForCurrentView().LogicalDpi,
               DisplayInformation.GetForCurrentView().LogicalDpi,
               pixelBuffer.ToArray());
                //刷新图像  
                await encoder.FlushAsync();
            }
            CurrentFile = sFile;
        }
        #endregion


        private async void BottomItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var appButton = (AppBarButton)sender;
            var lable = appButton.Label.ToString();
            switch (lable)
            {
                case "复制":
                    if (NetImaProgress.IsActive)
                        return;
                    NetImaProgress.IsActive = true;
                    await MakeEmoji();
                    DataPackage dp = new DataPackage();
                    dp.SetBitmap(RandomAccessStreamReference.CreateFromFile(CurrentFile));
                    Clipboard.SetContent(dp);
                    NetImaProgress.IsActive = false;
                    break;
                case "分享":
                    await MakeEmoji();
                    ShareIma();break;
                case "保存":
                    NetImaProgress.IsActive = true;
                    await MakeEmoji();
                    StorageFolder storageFolder = KnownFolders.PicturesLibrary;
                    var DesFloder = await storageFolder.CreateFolderAsync("斗图UWP",CreationCollisionOption.OpenIfExists);
                    var DesFile = await DesFloder.CreateFileAsync("斗图"+ DateTime.Now.TimeOfDay.Ticks.ToString()+new Random().Next(1,3000).ToString()+".png",CreationCollisionOption.ReplaceExisting);
                    await CurrentFile.CopyAndReplaceAsync(DesFile);
                    await Task.Delay(1000);
                    NetImaProgress.IsActive = !NetImaProgress.IsActive;
                    break;
                case "制作":
                    sWord.IsEnabled = true;
                    sWord.Focus(FocusState.Keyboard);
                    InputIn.Begin();
                    break;
            }
        }

        private async void Emoji_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var info = (ListInfo)e.ClickedItem;
                var path = "ms-appx:///Emoji/" + info.name;
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(path));
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage b = new BitmapImage();
                    b.SetSource(stream);
                    test.Source = b;
                }
                MakeIma();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message,"切换出错").ShowAsync();
            }
        }

        private async void EmojiWords_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var temp = (EmojiUiContent)e.ClickedItem;
                wordTextBlock.Text = temp.words;
                MakeIma();
            }
            catch (Exception ex)
            {
                await new MessageDialog("错误代码："+ex.Message).ShowAsync();
            }
        }

        private void AdControl_AdClick(object sender, JiuYouAdUniversal.Models.AdClickEventArgs e)
        {
            //TipsTextBlock.Text = e.clickResult;
            //if (e.clickResult == "2")
            //{
            //    TipsTextBlock.Text = "";
            //}
        }

        private async void Logo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("zune:search?publisher=LZ_Studio"));
        }

        private async void SecItemList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var textBlock = (TextBlock)sender;
            var lable = textBlock.Text;
            switch (lable)
            {
                case "更新表情包": await new MessageDialog("此功能将在下一个版本到来，敬请期待！").ShowAsync(); break;
                case "提供建议": sendEmail(); break;
                case "打分本应用": scoreApp(); break;
                case "鼓励开发者": encourage(); break;
                case "关注开发者": concern(); break;
                case "关于此应用": Frame.Navigate(typeof(About)); break;
            }
        }

        #region SecBottomList
        private async Task<bool> showMsg(string title,string content,string btn1,string btn2)
        {
            try
            {
                var dialog = new MessageDialog(content, title);
                dialog.Commands.Add(new UICommand(btn1, cmd => { }, commandId: 0));
                dialog.Commands.Add(new UICommand(btn2, cmd => { }, commandId: 1));
                //设置默认按钮，不设置的话默认的确认按钮是第一个按钮
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                //获取返回值
                var result = await dialog.ShowAsync();
                if (result.Id.ToString() == "0")
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 打分应用
        /// </summary>
        private async void scoreApp()
        {
            var reselt = await showMsg("感谢您即将为我们打分", "提示：请从即将展现的应用列表中选择评价此软件", "继续", "放弃");
            if (reselt)
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("zune:search?publisher=LZ_Studio"));
            }
        }

        /// <summary>
        /// 建议
        /// </summary>
        private async void sendEmail()
        {
            var reselt = await showMsg("期待您的建议", "提示：即将发送邮件给软件作者，但由于时间有限，作者并不会回复每一个建议邮件，请见谅。", "继续","放弃");
            if (reselt)
            {
                var mailto = new Uri($"mailto:{"875626439@qq.com"}?subject={"来自斗图UWP用户的建议"}&body={"请自此输入您宝贵的建议"}");
                await Launcher.LaunchUriAsync(mailto);
            }
        }

        /// <summary>
        /// 鼓励
        /// </summary>
        private async void encourage()
        {
            var reselt = await showMsg("期待您的鼓励", "提示：请以后点击广告时，按照提示进行到底", "多点击广告", "绝不点击广告");
            if (reselt)
            {
                await new MessageDialog("谢谢支持！", "提示").ShowAsync();
            }
            else
            {
                await new MessageDialog("我的兴趣让我选择了这个平台，用户的支持决定我可以在这个平台生存多久。", "提示").ShowAsync();
            }
        }

        /// <summary>
        /// 关注
        /// </summary>
        private async void concern()
        {
            DataPackage dp = new DataPackage();
            dp.SetText("http://weibo.com/u/6128304159?refer_flag=1001030102");
            Clipboard.SetContent(dp);
            await new MessageDialog("软件作者微博已经复制到您的剪切板","提示").ShowAsync();
        }

        #endregion

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BottomGrid.Width = this.ActualWidth;
        }

        private void SplitView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth > 700)
            {
                if (splitView.IsPaneOpen == false)
                {
                    splitView.IsPaneOpen = true;
                    secFrame.Navigate(typeof(About));
                }
                splitView.OpenPaneLength = this.ActualWidth / 2;
            }
            else
            {
                splitView.IsPaneOpen = false;
            }
        }


        #region 文字移动相关
        TranslateTransform dragTranslation;
        double ImaFrameWidth = 0;
        double ImaFrameHeight = 0;
        double DelaX = 0;
        double DelaY = 0;
        private void wordTextBlock_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            DelaX += e.Delta.Translation.X;
            DelaY += e.Delta.Translation.Y;
            setPost();
        }
        private void wordTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DelaX = 0;
            DelaY = 0;
            setPost();
        }
        private void setPost()
        {
            var width = wordTextBlock.ActualWidth / 2;
            var height = wordTextBlock.ActualHeight / 2;
            if (ImaFrameWidth == 0 || ImaFrameHeight == 0)
            {
                ImaFrameWidth = pictureRoot.ActualWidth / 2;
                ImaFrameHeight = pictureRoot.ActualHeight / 2;
            }
            if (dragTranslation == null)
            {
                dragTranslation = new TranslateTransform();
            }
            wordTextBlock.RenderTransform = dragTranslation;
            var MaxX = ImaFrameWidth - width;
            DelaX = Math.Abs(DelaX) > MaxX ? MaxX * DelaX / Math.Abs(DelaX) : DelaX;
            var MaxY = ImaFrameHeight - height;
            DelaY = Math.Abs(DelaY) > MaxY ? MaxY * DelaY / Math.Abs(DelaY) : DelaY;
            dragTranslation.X = DelaX;
            dragTranslation.Y = DelaY;
        }
        #endregion
    }
}
