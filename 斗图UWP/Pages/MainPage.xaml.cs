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
            myDoneEventDelegate += MainPage_myDoneEventDelegate;            
            await InitEmojiIma();
            BlurEffectInit();
        }
        
        #region 初始化表情集
        private async Task InitEmojiIma()
        {
            var fileTemp = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///EmojiList/E1/e1.png"));
            var folder = await fileTemp.GetParentAsync();
            await InitEmojiIma(folder);
        }
        private async Task InitEmojiIma(StorageFolder folder)
        {
            if (ListSorce == null)
            {
                ListSorce = new ObservableCollection<ListInfo>();
            }
            else
            {
                while (ListSorce.Count > 0)
                {
                    ListSorce.RemoveAt(0);
                }
            }
            Emoji.ItemsSource = ListSorce;
            try
            {
                var files = await folder.GetFilesAsync();
                foreach (var file in files)
                {
                    var ima = new BitmapImage();
                    using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        await ima.SetSourceAsync(fileStream);
                    }
                    var temp = new ListInfo();
                    temp.file = file;
                    temp.ima = ima;
                    ListSorce.Add(temp);
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region 页面事件

        private async void setStateBar()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var stateBar = StatusBar.GetForCurrentView();
                await stateBar.ShowAsync();
                stateBar.BackgroundOpacity = 1;
                stateBar.BackgroundColor = Colors.LightGray;
                stateBar.ForegroundColor = Colors.Black;
            }
            //那就是在电脑上运行
            else
            {
                //更改标题栏颜色
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.BackgroundColor = Colors.LightGray;
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
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///EmojiList/E1/e1.png"));
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
        private async void MainPage_myDoneEventDelegate(byte[] result)
        {
            using (Stream stream = new_bitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(result, 0, result.Length);
            }
            back.Source = new_bitmap;
            isPlay = false;
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
            await renderTargetBitmap.RenderAsync(pictureRoot);
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
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
                CurrentFile = info.file;
                using (IRandomAccessStream stream = await CurrentFile.OpenAsync(FileAccessMode.Read))
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
                var temp = (TextBlock)e.ClickedItem;
                wordTextBlock.Text = temp.Text;
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
                case "更新表情包": await new MessageDialog("由于资金问题，此功能正在努力协调，敬请期待！").ShowAsync(); break;
                case "提供建议": sendEmail(); break;
                case "打分本应用": scoreApp(); break;
                case "鼓励开发者": encourage(); break;
                case "关注开发者": concern(); break;
                case "捐赠作者": juanzeng(); break;
                case "关于此应用": Frame.Navigate(typeof(About)); break;
            }
        }

        #region SecBottomList

        /// <summary>
        /// 捐赠
        /// </summary>
        private async void juanzeng()
        {
            DataPackage dp = new DataPackage();
            dp.SetText("qq875626439521@outlook.com");
            Clipboard.SetContent(dp);
            await new MessageDialog("软件作者支付宝已经复制到您的剪切板", "请备注 斗图UWP").ShowAsync();
        }

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
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://weibo.com/u/6128304159?refer_flag=1001030102"));
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
                if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                }
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

        /// <summary>
        /// UI功能按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void functionBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            LeftGridSplitView.IsPaneOpen = !LeftGridSplitView.IsPaneOpen;
        }

        /// <summary>
        /// 改变文字颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Color_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Commandbar.IsOpen = false;
            var  temp = (TextBlock)sender;
            wordTextBlock.Foreground = temp.Foreground;
        }

        /// <summary>
        ///改变图集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeIma_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                Commandbar.IsOpen = false;
                var ima = (Image)sender;
                CurrentEmojiPacket.Text = ima.Name.ToString();
                var tag = ima.Tag.ToString();
                var sFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(tag));
                var sFlieFather = await sFile.GetParentAsync();
                await InitEmojiIma(sFlieFather);
            }
            catch (Exception)
            {
            }
        }
    }
}
