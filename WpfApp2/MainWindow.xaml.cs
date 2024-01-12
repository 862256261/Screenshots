using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Windows.Size;


namespace WpfApp2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
        public static Bitmap CaptureAllScreens(Screen screen)
        {

            var screenBounds = screen.Bounds;
            var bitmap = new Bitmap(screenBounds.Width, screenBounds.Height);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(new System.Drawing.Point(screenBounds.Left, screenBounds.Top), new System.Drawing.Point(0, 0), screenBounds.Size);
            }
            //// 保存到本地文件系统
            //string filePath = @"D:\image.png"; // 定义文件路径和名称
            //bitmap.Save(filePath, ImageFormat.Png); // 保存为PNG格式

            
            return bitmap;
        }




        private List<MaskWindowManager> maskWindowManagers = new List<MaskWindowManager>();
        private void CaptureScreens()
        {
            foreach (var screen in Screen.AllScreens)
            {
                var screenImage = CaptureAllScreens(screen);
                var screenImageSource = BitmapToImageSource(screenImage);
                screenImage.Dispose();

                var maskWindowManager = new MaskWindowManager(screen, screenImageSource);
                maskWindowManagers.Add(maskWindowManager); // 将实例添加到列表中
                maskWindowManager.OnSelectionComplete += (croppedImage, rect) =>
                {
                    foreach (var manager in maskWindowManagers)
                    {
                        manager.Close();
                    }
                    maskWindowManagers.Clear(); // 清空列表


                    try
                    {


                        var croppedBitmap = new CroppedBitmap(croppedImage, rect);
                        cutImage.Source = croppedBitmap;
                    }
                    catch
                    {


                    }
                    if ((bool)ckVisibility.IsChecked)
                    {
                        this.Show();
                    }
                };
                maskWindowManager.OnClose += MaskWindowManager_OnClose;
                maskWindowManager.Show();
               
            }
        }

        private void MaskWindowManager_OnClose()
        {
            foreach (var manager in maskWindowManagers)
            {
                manager.Close();
            }
            maskWindowManagers.Clear(); // 清空列表
        }

        public static ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            var bitmapImage = new BitmapImage();
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ckVisibility.IsChecked)
            {
                this.Hide();
            }
            CaptureScreens();
        }
    }
}
