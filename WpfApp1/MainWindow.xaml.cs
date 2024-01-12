using AvalonDock;
using AvalonDock.Layout;
using MahApps.Metro.Controls;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void DockingManager_DocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            var document = e.Document as LayoutDocument;
            if (document != null)
            {
                // 检查文档状态，例如是否有未保存的更改
                bool hasUnsavedChanges = true;
                //if (hasUnsavedChanges)
                //{
                //    var customDialog = new CustomDialogWindow(); // 你自定义的对话框窗口
                //    customDialog.ShowDialog();

                //    // 根据自定义对话框的结果来决定是否取消关闭
                //    if (customDialog.DialogResult != true)
                //    {
                //        e.Cancel = true;
                //    }
                //}
                if (hasUnsavedChanges)
                {
                    // 显示确认对话框
                    MessageBoxResult result = MessageBox.Show(
                        "您有未保存的更改。您确定要关闭吗？",
                        "确认关闭",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    // 如果用户选择“否”，则取消关闭操作
                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame frame = new  Frame();
            frame.Navigate(new Page1());
            // 创建一个新的 LayoutDocument
            LayoutDocument newDocument = new LayoutDocument
            {
                Title = "新文档",
                // Content 属性可以设置为你的用户控件或其他视图
                Content = frame // 假设你有一个用户控件叫做 MyUserControl
            };
            var dockingManager = this.dock;
            LayoutDocumentPane documentPane = dockingManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            if (documentPane == null)
            {
                documentPane = new LayoutDocumentPane();
                dockingManager.Layout.RootPanel.Children.Add(documentPane);
            }
            documentPane.Children.Add(newDocument);
            newDocument.IsActive = true;
        }
    }
}
