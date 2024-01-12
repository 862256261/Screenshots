using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Drawing;

using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using System.Runtime.InteropServices;

namespace WpfApp2
{
    public class MaskWindowManager
    {
        private Window maskWindow;
        private System.Windows.Controls.Image imageControl;
        private System.Windows.Shapes.Path overlay;
        private System.Windows.Shapes.Rectangle selectionRectangle;
        private bool isDrawing = false;
        private Point startPoint;

        public event Action<BitmapSource, Int32Rect> OnSelectionComplete;
        public event Action OnClose;


        #region 添加控制点和追踪变量
        private List<System.Windows.Shapes.Rectangle> resizeHandles;
        private System.Windows.Shapes.Rectangle selectedHandle;
        private bool isResizing = false;
        #endregion
        #region 右下角悬浮按钮
        private StackPanel toolbarPanel;
        private System.Windows.Controls.Button confirmButton;
        private System.Windows.Controls.Button cancelButton;
        private void InitializeToolbar()
        {
            if (toolbarPanel != null)
            {

                (maskWindow.Content as Canvas)?.Children.Remove(toolbarPanel);

            }

            toolbarPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            confirmButton = new System.Windows.Controls.Button
            {
                Width = 50,
                Content = "√",
                Margin = new Thickness(2)
            };
            confirmButton.Click += ConfirmButton_Click;

            cancelButton = new System.Windows.Controls.Button
            {
                Width = 50,
                Content = "×",
                Margin = new Thickness(2)
            };
            cancelButton.Click += CancelButton_Click;

            toolbarPanel.Children.Add(confirmButton);
            toolbarPanel.Children.Add(cancelButton);


            toolbarPanel.Visibility = Visibility.Hidden;

            (maskWindow.Content as Canvas)?.Children.Add(toolbarPanel);
        }
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {

            var rect = new Int32Rect
            {
                X = (int)(Canvas.GetLeft(selectionRectangle) / imageControl.ActualWidth * canvas.ActualWidth * M11),
                Y = (int)(Canvas.GetTop(selectionRectangle) / imageControl.ActualHeight * canvas.ActualHeight * M11),
                Width = (int)(selectionRectangle.Width / imageControl.ActualWidth * canvas.ActualWidth * M11),
                Height = (int)(selectionRectangle.Height / imageControl.ActualHeight * canvas.ActualHeight * M11)
            };

            OnSelectionComplete?.Invoke(imageControl.Source as BitmapSource, rect);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnClose?.Invoke();
            //撤销
            //if (selectionRectangle != null)
            //{
            //    (maskWindow.Content as Canvas)?.Children.Remove(selectionRectangle);
            //    selectionRectangle = null;
            //}
            //if (resizeHandles != null)
            //{
            //    foreach (var item in resizeHandles)
            //    {
            //        (maskWindow.Content as Canvas)?.Children.Remove(item);
            //    }
            //}
            //if (toolbarPanel != null)
            //{

            //    (maskWindow.Content as Canvas)?.Children.Remove(toolbarPanel);

            //}
            //if (overlay != null)
            //{
            //    overlay.Data = new RectangleGeometry(new Rect(0, 0, maskWindow.Width, maskWindow.Height));
            //}
        }
        private void UpdateToolbarPosition()
        {
            if (selectionRectangle == null || toolbarPanel == null) return;


            double left = Canvas.GetLeft(selectionRectangle);
            double top = Canvas.GetTop(selectionRectangle);
            double width = selectionRectangle.Width;
            double height = selectionRectangle.Height;

            // Position the toolbar at the bottom-right corner of the selection rectangle
            Canvas.SetLeft(toolbarPanel, left + width - toolbarPanel.ActualWidth);
            Canvas.SetTop(toolbarPanel, maskWindow.Height - (top + height) < 50 ? maskWindow.Height - 30 : top + height);

            // Make sure the toolbar is visible
            toolbarPanel.Visibility = Visibility.Visible;
        }
        #endregion
        public MaskWindowManager(Screen screen, ImageSource screenImage)
        {
            InitializeMaskWindow(screen, screenImage);
            maskWindow.KeyDown += MainWindow_KeyDown;
        }
        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // 检查按下的键是否是 ESC
            if (e.Key == Key.Escape)
            {
                OnClose?.Invoke();
            }
        }

        public double GetScreensDpiScale(Screen screen)
        {

            // Get the monitor handle
            IntPtr monitorHandle = MonitorFromPoint(new POINT(screen.Bounds.Left + 1, screen.Bounds.Top + 1), MONITOR_DEFAULTTONEAREST);

            // Get the DPI
            GetDpiForMonitor(monitorHandle, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);

            float scaleX = dpiX / 96f;
            float scaleY = dpiY / 96f;

            return scaleX;

        }
        public double GetScreensDpi()
        {
            float dpiX, dpiY;
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                dpiX = graphics.DpiX;
                dpiY = graphics.DpiY;
            }
            return dpiX / 96f;
        }

        [DllImport("User32.dll", SetLastError = true)]
        static extern IntPtr MonitorFromPoint([In] POINT pt, [In] uint dwFlags);

        [DllImport("Shcore.dll")]
        static extern int GetDpiForMonitor([In] IntPtr hmonitor, [In] MONITOR_DPI_TYPE dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

        private enum MONITOR_DPI_TYPE
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
            MDT_DEFAULT = MDT_EFFECTIVE_DPI
        }

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        //PresentationSource source;
        Canvas canvas;
        double M11;
        double HM11;
        private void InitializeMaskWindow(Screen screen, ImageSource screenImage)
        {
            HM11 = GetScreensDpi();
            //source = PresentationSource.FromVisual(System.Windows.Application.Current.MainWindow);//HM11,
            M11 = GetScreensDpiScale(screen);
            maskWindow = new Window
            {
                Left = screen.Bounds.Left / HM11,
                Top = screen.Bounds.Top / HM11,
                Width = screen.Bounds.Width / HM11,
                Height = screen.Bounds.Height / HM11,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                Topmost = true
            };

            imageControl = new System.Windows.Controls.Image
            {
                Source = screenImage,
                Stretch = Stretch.Fill
            };

            overlay = new System.Windows.Shapes.Path
            {
                Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Data = new RectangleGeometry(new Rect(0, 0, screen.Bounds.Width, screen.Bounds.Height))
            };

            canvas = new Canvas();

            //canvas.Background = new SolidColorBrush(Color.FromArgb(255, 135, 206, 235)); // 使用ARGB值
            canvas.SizeChanged += Canvas_SizeChanged;
            canvas.Children.Add(imageControl);
            canvas.Children.Add(overlay);

            maskWindow.Content = canvas;

            maskWindow.MouseDown += MaskWindow_MouseDown;
            maskWindow.MouseMove += MaskWindow_MouseMove;
            maskWindow.MouseUp += MaskWindow_MouseUp;



        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            imageControl.Width = canvas.ActualWidth;
            imageControl.Height = canvas.ActualHeight;
        }


        private void InitializeResizeHandles()
        {
            if (resizeHandles != null)
            {
                foreach (var item in resizeHandles)
                {
                    (maskWindow.Content as Canvas)?.Children.Remove(item);
                }
            }
            resizeHandles = new List<System.Windows.Shapes.Rectangle>();
            for (int i = 0; i < 8; i++)
            {
                var handle = new System.Windows.Shapes.Rectangle
                {
                    Fill = Brushes.Blue,
                    Width = 10,
                    Height = 10,
                    Visibility = Visibility.Hidden
                };

                handle.MouseDown += Handle_MouseDown;
                handle.MouseMove += Handle_MouseMove;
                handle.MouseUp += Handle_MouseUp;

                resizeHandles.Add(handle);
                //Canvas.SetZIndex(handle, 99); 
                (maskWindow.Content as Canvas)?.Children.Add(handle);
            }
        }
        public void Show()
        {
            maskWindow.Show();
        }


        private void Handle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var handle = sender as System.Windows.Shapes.Rectangle;
            if (handle != null)
            {
                isResizing = true;
                selectedHandle = handle;
                startPoint = e.GetPosition(maskWindow.Content as Canvas); // 设置开始点为当前鼠标位置
                Mouse.Capture(handle); // 捕获鼠标
                e.Handled = true;
            }
        }



        private void Handle_MouseMove(object sender, MouseEventArgs e)
        {


            if (isResizing && selectedHandle != null && selectionRectangle != null)
            {
                var canvas = maskWindow.Content as Canvas;
                var mousePosition = e.GetPosition(canvas);


                double deltaX = mousePosition.X - startPoint.X;
                double deltaY = mousePosition.Y - startPoint.Y;


                double left = Canvas.GetLeft(selectionRectangle);
                double top = Canvas.GetTop(selectionRectangle);
                double width = selectionRectangle.Width;
                double height = selectionRectangle.Height;


                switch (resizeHandles.IndexOf(selectedHandle))
                {
                    case 0: // Top-left
                        double newLeft = left + deltaX;
                        double newTop = top + deltaY;
                        double newWidthTL = width - deltaX;
                        double newHeightTL = height - deltaY;
                        double newLeftTL = Canvas.GetLeft(selectionRectangle) + newWidthTL;
                        if (newWidthTL > 0 && newLeft >= 0 && newLeftTL >= 0)
                        {
                            Canvas.SetLeft(selectionRectangle, newLeft);
                            selectionRectangle.Width = newWidthTL;
                            startPoint.X = mousePosition.X;
                        }

                        if (newHeightTL > 0 && newTop >= 0)
                        {
                            Canvas.SetTop(selectionRectangle, newTop);
                            selectionRectangle.Height = newHeightTL;
                            startPoint.Y = mousePosition.Y;
                        }
                        break;
                    case 1: // Top-center
                        double newHeightTC = height - deltaY;
                        if (newHeightTC > 0 && top + deltaY >= 0)
                        {
                            Canvas.SetTop(selectionRectangle, top + deltaY);
                            selectionRectangle.Height = newHeightTC;
                            startPoint.Y = mousePosition.Y;
                        }
                        break;

                    case 2: // Top-right
                        double newWidthTR = width + deltaX;
                        double newHeightTR = height - deltaY;
                        double newRightTR = Canvas.GetLeft(selectionRectangle) + newWidthTR;
                        if (newWidthTR > 0 && newRightTR <= maskWindow.Width)
                        {
                            selectionRectangle.Width = newWidthTR;
                            startPoint.X = mousePosition.X;
                        }
                        if (newHeightTR > 0 && top + deltaY >= 0)
                        {
                            Canvas.SetTop(selectionRectangle, top + deltaY);
                            selectionRectangle.Height = newHeightTR;
                            startPoint.Y = mousePosition.Y;
                        }
                        break;

                    case 3: // Right-middle
                        double newWidthRM = width + deltaX;
                        double newRight = Canvas.GetLeft(selectionRectangle) + newWidthRM;
                        if (newWidthRM > 0 && newRight <= maskWindow.Width)
                        {
                            selectionRectangle.Width = newWidthRM;
                            startPoint.X = mousePosition.X;
                        }
                        break;

                    case 4: // Bottom-right
                        double newWidthBR = width + deltaX;
                        double newHeightBR = height + deltaY;
                        double newRightBR = Canvas.GetLeft(selectionRectangle) + newWidthBR;
                        if (newWidthBR > 0 && newRightBR <= maskWindow.Width)
                        {
                            selectionRectangle.Width = newWidthBR;
                            startPoint.X = mousePosition.X;
                        }
                        if (newHeightBR > 0)
                        {
                            selectionRectangle.Height = newHeightBR;
                            startPoint.Y = mousePosition.Y;
                        }
                        break;

                    case 5: // Bottom-center
                        double newHeightBC = height + deltaY;
                        if (newHeightBC > 0)
                        {
                            selectionRectangle.Height = newHeightBC;
                            startPoint.Y = mousePosition.Y;
                        }
                        break;

                    case 6: // Bottom-left
                        double newWidthBL = width - deltaX;
                        double newHeightBL = height + deltaY;
                        double newLeftBL = Canvas.GetLeft(selectionRectangle) + newWidthBL;
                        if (newWidthBL > 0 && left + deltaX >= 0 && newLeftBL >= 0)
                        {
                            Canvas.SetLeft(selectionRectangle, left + deltaX);
                            selectionRectangle.Width = newWidthBL;
                            startPoint.X = mousePosition.X;
                        }
                        if (newHeightBL > 0)
                        {
                            selectionRectangle.Height = newHeightBL;
                            startPoint.Y = mousePosition.Y;
                        }
                        break;

                    case 7: // Left-middle
                        double newWidthLM = width - deltaX;
                        double newLeftLM = Canvas.GetLeft(selectionRectangle) + newWidthLM;
                        if (newWidthLM > 0 && left + deltaX >= 0 && newLeftLM >= 0)
                        {
                            Canvas.SetLeft(selectionRectangle, left + deltaX);
                            selectionRectangle.Width = newWidthLM;
                            startPoint.X = mousePosition.X;
                        }
                        break;
                }

                e.Handled = true;
                UpdateOverlay();

                UpdateResizeHandles();
                UpdateToolbarPosition();
            }

        }

        private void UpdateOverlay()
        {
            if (overlay == null || selectionRectangle == null) return;


            double x = Canvas.GetLeft(selectionRectangle);
            double y = Canvas.GetTop(selectionRectangle);
            double width = selectionRectangle.Width;
            double height = selectionRectangle.Height;


            var selectionGeometry = overlay.Data as CombinedGeometry;
            if (selectionGeometry != null)
            {
                var rectGeometry = selectionGeometry.Geometry2 as RectangleGeometry;
                if (rectGeometry != null)
                {
                    rectGeometry.Rect = new Rect(x, y, width, height);
                }
            }
            else
            {

                var canvasGeometry = new RectangleGeometry(new Rect(0, 0, maskWindow.Width, maskWindow.Height));
                selectionGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, canvasGeometry, new RectangleGeometry(new Rect(x, y, width, height)));
                overlay.Data = selectionGeometry;
            }
        }



        private void Handle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isResizing = false;
            selectedHandle = null;
            Mouse.Capture(null); // 释放鼠标捕获

        }

        private void UpdateResizeHandles()
        {
            if (selectionRectangle == null || resizeHandles == null) return;

            var canvas = selectionRectangle.Parent as Canvas;
            if (canvas == null) return;

            double left = Canvas.GetLeft(selectionRectangle);
            double top = Canvas.GetTop(selectionRectangle);
            double right = left + selectionRectangle.Width;
            double bottom = top + selectionRectangle.Height;
            double midX = left + selectionRectangle.Width / 2;
            double midY = top + selectionRectangle.Height / 2;

            // Top-left corner
            SetHandlePosition(resizeHandles[0], left, top);
            // Top-middle
            SetHandlePosition(resizeHandles[1], midX, top);
            // Top-right corner
            SetHandlePosition(resizeHandles[2], right, top);
            // Right-middle
            SetHandlePosition(resizeHandles[3], right, midY);
            // Bottom-right corner
            SetHandlePosition(resizeHandles[4], right, bottom);
            // Bottom-middle
            SetHandlePosition(resizeHandles[5], midX, bottom);
            // Bottom-left corner
            SetHandlePosition(resizeHandles[6], left, bottom);
            // Left-middle
            SetHandlePosition(resizeHandles[7], left, midY);

            // Make sure handles are visible
            foreach (var handle in resizeHandles)
            {
                handle.Visibility = Visibility.Visible;
            }
        }

        private void SetHandlePosition(System.Windows.Shapes.Rectangle handle, double left, double top)
        {
            Canvas.SetLeft(handle, left - handle.Width / 2);
            Canvas.SetTop(handle, top - handle.Height / 2);
        }



        private void MaskWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(maskWindow);
            isDrawing = true;

            if (selectionRectangle != null)
            {
                (maskWindow.Content as Canvas)?.Children.Remove(selectionRectangle);
            }
            InitializeResizeHandles();
            InitializeToolbar();
            if (overlay != null)
            {
                overlay.Data = new RectangleGeometry(new Rect(0, 0, maskWindow.Width, maskWindow.Height));
            }

            selectionRectangle = new System.Windows.Shapes.Rectangle
            {
                Stroke = Brushes.Green,
                StrokeThickness = 2
            };

            (maskWindow.Content as Canvas)?.Children.Add(selectionRectangle);

        }

        private void MaskWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isResizing) return;

            if (!isDrawing) return;

            var currentPoint = e.GetPosition(maskWindow);
            var x = Math.Min(currentPoint.X, startPoint.X);
            var y = Math.Min(currentPoint.Y, startPoint.Y);
            var width = Math.Max(currentPoint.X, startPoint.X) - x;
            var height = Math.Max(currentPoint.Y, startPoint.Y) - y;

            Canvas.SetLeft(selectionRectangle, x);
            Canvas.SetTop(selectionRectangle, y);
            selectionRectangle.Width = width;
            selectionRectangle.Height = height;

            var selectionGeometry = new RectangleGeometry(new Rect(x, y, width, height));

            var combinedGeometry = new CombinedGeometry
            {
                Geometry1 = new RectangleGeometry(new Rect(0, 0, maskWindow.Width, maskWindow.Height)),
                Geometry2 = selectionGeometry,
                GeometryCombineMode = GeometryCombineMode.Exclude
            };

            overlay.Data = combinedGeometry;

        }

        private void MaskWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            if (!double.IsNaN(selectionRectangle.Width) && !double.IsNaN(selectionRectangle.Height))
            {

                //var rect = new Int32Rect
                //{
                //    X = (int)(Canvas.GetLeft(selectionRectangle) / imageControl.ActualWidth * canvas.ActualWidth * M11),
                //    Y = (int)(Canvas.GetTop(selectionRectangle) / imageControl.ActualHeight * canvas.ActualHeight * M11),
                //    Width = (int)(selectionRectangle.Width / imageControl.ActualWidth * canvas.ActualWidth * M11),
                //    Height = (int)(selectionRectangle.Height / imageControl.ActualHeight * canvas.ActualHeight * M11)
                //};

                //OnSelectionComplete?.Invoke(imageControl.Source as BitmapSource, rect);
                UpdateResizeHandles();
                UpdateToolbarPosition();
            }
        }
        public void Close()
        {
            maskWindow.MouseDown -= MaskWindow_MouseDown;
            maskWindow.MouseMove -= MaskWindow_MouseMove;
            maskWindow.MouseUp -= MaskWindow_MouseUp;
            maskWindow.KeyDown -= MainWindow_KeyDown;
            maskWindow.Close();

        }
    }
}
