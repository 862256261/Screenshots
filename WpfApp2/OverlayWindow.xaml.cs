using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// OverlayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OverlayWindow : Window
    {
        private Point startPoint;
     

        public OverlayWindow()
        {
            InitializeComponent();
            this.MaxWidth = SystemParameters.VirtualScreenWidth;
            this.MaxHeight = SystemParameters.VirtualScreenHeight;
            this.Left = SystemParameters.VirtualScreenLeft;
            this.Top = SystemParameters.VirtualScreenTop;
          
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(this);
            selectionRectangle.Visibility = Visibility.Visible;
            Canvas.SetLeft(selectionRectangle, startPoint.X);
            Canvas.SetTop(selectionRectangle, startPoint.Y);
            selectionRectangle.Width = 0;
            selectionRectangle.Height = 0;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);
                var x = Math.Min(currentPoint.X, startPoint.X);
                var y = Math.Min(currentPoint.Y, startPoint.Y);
                var width = Math.Max(currentPoint.X, startPoint.X) - x;
                var height = Math.Max(currentPoint.Y, startPoint.Y) - y;
                Canvas.SetLeft(selectionRectangle, x);
                Canvas.SetTop(selectionRectangle, y);
                selectionRectangle.Width = width;
                selectionRectangle.Height = height;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Here you would handle the final selection and perhaps close the overlay window
            // You can also add logic to capture the selected area as an image
        }
    }
}
