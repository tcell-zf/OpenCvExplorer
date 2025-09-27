using OpenCvExplorer.ViewModels;
using OpenCvExplorer.ViewModels.UserControls;
using OpenCvExplorer.ViewModels.Windows;
using OpenCvExplorer.Views.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenCvExplorer.Views.UserControls
{
    public partial class ImageEditor : UserControl
    {
        private ImageEditorViewModel? ViewModel
        {
            get
            {
                if (DataContext == null)
                    return null;
                return DataContext as ImageEditorViewModel;
            }
        }
        private Point? _roiStart;
        private Point? _roiEnd;

        public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register(nameof(FileName), typeof(string), typeof(ImageEditor),
            new FrameworkPropertyMetadata("", new PropertyChangedCallback(FileNamePropertyChangedCallback)));
        private static void FileNamePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            var ctrl = sender as ImageEditor;
            if (ctrl != null)
                ctrl.SetFileName();
        }
        public string FileName
        {
            get { return (string)this.GetValue(FileNameProperty); }
            set { this.SetValue(FileNameProperty, value); }
        }

        public ImageEditor()
        {
            InitializeComponent();
        }

        private void DisplayedImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (ViewModel == null || ViewModel.ImageMat == null)
                return;

            var point = e.GetPosition(DisplayedImage);
            var x = (int)(point.X * ViewModel.ImageMat.Width / DisplayedImage.ActualWidth);
            var y = (int)(point.Y * ViewModel.ImageMat.Height / DisplayedImage.ActualHeight);
            if ((0 <= x && x < ViewModel.ImageMat.Width)
                && (0 <= y && y < ViewModel.ImageMat.Height))
            {
                ViewModel.CurrentPoint = new System.Windows.Point(x, y);
            }

            bool mouseCapturedStopped = false;
            if (_roiStart.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(DisplayedImage);
                double left = Math.Min(pos.X, _roiStart.Value.X);
                if (pos.X <= DisplayedImage.Margin.Left)
                {
                    left = DisplayedImage.Margin.Left;
                    DisplayedImage.ReleaseMouseCapture();
                    mouseCapturedStopped = true;
                }
                double top = Math.Min(pos.Y, _roiStart.Value.Y);
                if (pos.Y <= DisplayedImage.Margin.Top)
                {
                    top = DisplayedImage.Margin.Top;
                    DisplayedImage.ReleaseMouseCapture();
                    mouseCapturedStopped = true;
                }
                double width = Math.Abs(pos.X - _roiStart.Value.X);
                if (pos.X >= DisplayedImage.ActualWidth)
                {
                    width = DisplayedImage.ActualWidth - _roiStart.Value.X;
                    DisplayedImage.ReleaseMouseCapture();
                    mouseCapturedStopped = true;
                }
                double height = Math.Abs(pos.Y - _roiStart.Value.Y);
                if (pos.Y >= DisplayedImage.ActualHeight)
                {
                    height = DisplayedImage.ActualHeight - _roiStart.Value.Y;
                    DisplayedImage.ReleaseMouseCapture();
                    mouseCapturedStopped = true;
                }

                Canvas.SetLeft(RoiRect, left);
                Canvas.SetTop(RoiRect, top);
                RoiRect.Width = width;
                RoiRect.Height = height;
                if (mouseCapturedStopped)
                    ViewModel.RoiRect = new OpenCvSharp.Rect((int)left, (int)top, (int)width, (int)height);
            }
        }

        private void DisplayedImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (ViewModel == null)
                return;

            ViewModel.CurrentPoint = null;
        }

        private void DisplayedImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _roiStart = e.GetPosition(DisplayedImage);
            RoiRect.Visibility = Visibility.Visible;
            Canvas.SetLeft(RoiRect, _roiStart.Value.X);
            Canvas.SetTop(RoiRect, _roiStart.Value.Y);
            RoiRect.Width = 0;
            RoiRect.Height = 0;
            DisplayedImage.CaptureMouse();
        }

        private void DisplayedImage_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _roiEnd = e.GetPosition(DisplayedImage);
            DisplayedImage.ReleaseMouseCapture();

            if (_roiStart.HasValue && _roiEnd.HasValue)
            {
                ViewModel.RoiRect = new OpenCvSharp.Rect((int)_roiStart.Value.X,
                    (int)_roiStart.Value.Y,
                    (int)(_roiEnd.Value.X - _roiStart.Value.X),
                    (int)(_roiEnd.Value.Y - _roiStart.Value.Y));
            }
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var tabControl = sender as System.Windows.Controls.TabControl;
            if (tabControl == null)
                return;

            switch (tabControl.SelectedIndex)
            {
                case 0:
                    ViewModel.ImageSavingParameters.SaveImageType = ImageType.Jpeg;
                    break;
                case 1:
                    ViewModel.ImageSavingParameters.SaveImageType = ImageType.Png;
                    break;
                case 2:
                    ViewModel.ImageSavingParameters.SaveImageType = ImageType.Webp;
                    break;
                case 3:
                    ViewModel.ImageSavingParameters.SaveImageType = ImageType.Tiff;
                    break;
                default:
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var convoModel = new ConvolutionWindowViewModel()
            {
                RawImage = ViewModel.PresentedMat
            };
            ConvolutionWindow window = new ConvolutionWindow(convoModel)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = 1200,
                Height = 600,
                Owner = Application.Current.MainWindow
            };
            window.Show();
        }

        private void SetFileName()
        {
            if (ViewModel == null)
                return;

            ViewModel.FileName = FileName;
        }
    }
}
