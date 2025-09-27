using Microsoft.Win32;
using OpenCvExplorer.Helpers;
using OpenCvExplorer.Views.Pages;
using OpenCvExplorer.Views.UserControls;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace OpenCvExplorer.ViewModels.Pages
{
    public partial class VideoViewModel : BaseObservableObject, INavigationAware
    {
        private bool _isInitialized = false;
        private readonly IContentDialogService _contentDialogService;
        public int NormalFrameInterval = 33; // in milli-seconds
        public Mat? CurrentImageMat { get; set; }

        #region Constructors
        public VideoViewModel(IContentDialogService contentDialogService)
        {
            _contentDialogService = contentDialogService;
            //WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
            //{
            //    ImageChannelContent = App.GetStringResource(ImageChannel);
            //});
        }
        #endregion

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom() { }

        private void InitializeViewModel() { }

        #region Observable properties
        [ObservableProperty]
        private string _apiPreferenceString = "Any";
        [ObservableProperty]
        private VideoCaptureAPIs _apiPreference = VideoCaptureAPIs.ANY;

        [ObservableProperty]
        private List<CameraInfo>? _allCameras;
        [ObservableProperty]
        private string? _fileName = string.Empty;
        [ObservableProperty]
        private string? _link = string.Empty;
        [ObservableProperty]
        private bool _isAbleToPlay = false;
        [ObservableProperty]
        private bool _isAbleToStop = false;
        [ObservableProperty]
        private int? _angel = 0;
        [ObservableProperty]
        private double _centerX = 0;
        [ObservableProperty]
        private double _centerY = 0;

        [ObservableProperty]
        private int _frameInterval = 33; // in milli-seconds
        [ObservableProperty]
        private CommonVideoCaptureInfo? _videoCaptureInfo;
        [ObservableProperty]
        private VideoFileInfo? _videoFileInfo;
        [ObservableProperty]
        private Visibility _videoFileVisibility = Visibility.Hidden;
        [ObservableProperty]
        private VideoStreamInfo? _videoStreamInfo;
        [ObservableProperty]
        private Visibility _videoStreamVisibility = Visibility.Hidden;
        [ObservableProperty]
        private VideoSourceEnum _currentVideoSourceType = VideoSourceEnum.None;
        partial void OnCurrentVideoSourceTypeChanged(VideoSourceEnum value)
        {
            switch (value)
            {
                case VideoSourceEnum.File:
                    VideoFileVisibility = Visibility.Visible;
                    VideoStreamVisibility = Visibility.Hidden;
                    break;
                case VideoSourceEnum.Camera:
                case VideoSourceEnum.Link:
                    VideoFileVisibility = Visibility.Hidden;
                    VideoStreamVisibility = Visibility.Visible;
                    break;
                default:
                    VideoFileVisibility = Visibility.Hidden;
                    VideoStreamVisibility = Visibility.Hidden;
                    break;
            }
        }
        [ObservableProperty]
        private Stretch _stretch = Stretch.None;
        [ObservableProperty]
        private Visibility _loadingVisibility = Visibility.Hidden;
        #endregion

        #region Relay commands
        [RelayCommand]
        private void OnSetPrefAny()
        {
            ApiPreference = VideoCaptureAPIs.ANY;
            ApiPreferenceString = "Any";
        }
        [RelayCommand]
        private void OnSetPrefDShow()
        {
            ApiPreference = VideoCaptureAPIs.DSHOW;
            ApiPreferenceString = "DShow";
        }
        [RelayCommand]
        private void OnSetPrefFFMpeg()
        {
            ApiPreference = VideoCaptureAPIs.FFMPEG;
            ApiPreferenceString = "FFMpeg";
        }
        [RelayCommand]
        private void OnSetPrefImages()
        {
            ApiPreference = VideoCaptureAPIs.IMAGES;
            ApiPreferenceString = "Images";
        }
        [RelayCommand]
        private void OnCameraSelected()
        {
            FileName = string.Empty;
            Link = string.Empty;

            IsAbleToPlay = true;
            IsAbleToStop = false;
        }
        [RelayCommand]
        private void OnOpenVideoFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = App.GetStringResource("dialog-select-video-title"),
                Filter = "Video files (*.mp4;*.avi;*.mov;*.mkv;*.wmv;*.flv;*.webm)|*.mp4;*.avi;*.mov;*.mkv;*.wmv;*.flv;*.webm|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                AllCameras?.ForEach((cam) => cam.IsChecked = false);
                FileName = openFileDialog.FileName;
                Link = string.Empty;

                IsAbleToPlay = true;
                IsAbleToStop = false;
            }
        }
        [RelayCommand]
        private void OnLinkEntered()
        {
            if (!string.IsNullOrEmpty(Link))
            {
                AllCameras?.ForEach((cam) => cam.IsChecked = false);
                FileName = string.Empty;

                IsAbleToPlay = true;
                IsAbleToStop = false;
            }
        }

        [RelayCommand]
        private void OnSetVideoStretchNone()
        {
            Stretch = Stretch.None;
        }
        [RelayCommand]
        private void OnSetVideoStretchFill()
        {
            Stretch = Stretch.Fill;
        }
        [RelayCommand]
        private void OnSetVideoStretchUniform()
        {
            Stretch = Stretch.Uniform;
        }
        [RelayCommand]
        private void OnSetVideoStretchUniformToFill()
        {
            Stretch = Stretch.UniformToFill;
        }

        [RelayCommand]
        private void OnSetHalfSpeed()
        {
            FrameInterval = (int)(NormalFrameInterval * 2);
        }
        [RelayCommand]
        private void OnSetNormalSpeed()
        {
            FrameInterval = NormalFrameInterval;
        }
        [RelayCommand]
        private void OnSetOneAndHalfSpeed()
        {
            FrameInterval = (int)(NormalFrameInterval / 1.5);
        }
        [RelayCommand]
        private void OnSetDoubleSpeed()
        {
            FrameInterval = (int)(NormalFrameInterval / 2);
        }

        [RelayCommand]
        private async Task OnShowVideoProperties(object content)
        {
            var contentGrid = content as Grid;
            if (contentGrid == null)
                return;

            Grid? grid;
            Label? label;
            switch (CurrentVideoSourceType)
            {
                case VideoSourceEnum.File:
                    grid = UIElementHelper.FindChild<Grid>(contentGrid, "VideoFileInfo");
                    if (grid != null)
                        grid.Visibility = Visibility.Visible;
                    grid = UIElementHelper.FindChild<Grid>(contentGrid, "VideoStreamInfo");
                    if (grid != null)
                        grid.Visibility = Visibility.Hidden;

                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelFrameWidth");
                    if (label != null)
                        label.Content = VideoCaptureInfo?.FrameWidth.ToString();
                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelFrameHeight");
                    if (label != null)
                        label.Content = VideoCaptureInfo?.FrameHeight.ToString();
                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelFrameCount");
                    if (label != null)
                        label.Content = VideoFileInfo?.FrameCount.ToString();
                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelFourCC");
                    if (label != null)
                        label.Content = VideoFileInfo?.FourCCCodec;
                    break;
                case VideoSourceEnum.Camera:
                case VideoSourceEnum.Link:
                    grid = UIElementHelper.FindChild<Grid>(contentGrid, "VideoFileInfo");
                    if (grid != null)
                        grid.Visibility = Visibility.Hidden;
                    grid = UIElementHelper.FindChild<Grid>(contentGrid, "VideoStreamInfo");
                    if (grid != null)
                        grid.Visibility = Visibility.Visible;

                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelFrameWidth1");
                    if (label != null)
                        label.Content = VideoCaptureInfo?.FrameWidth.ToString();
                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelFrameHeight1");
                    if (label != null)
                        label.Content = VideoCaptureInfo?.FrameHeight.ToString();
                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelFps");
                    if (label != null)
                        label.Content = VideoStreamInfo?.Fps.ToString();
                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelBrightness");
                    if (label != null)
                        label.Content = VideoStreamInfo?.Brightness;
                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelContrast");
                    if (label != null)
                        label.Content = VideoStreamInfo?.Contrast;
                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelSaturation");
                    if (label != null)
                        label.Content = VideoStreamInfo?.Saturation;
                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelHue");
                    if (label != null)
                        label.Content = VideoStreamInfo?.Hue;
                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelGain");
                    if (label != null)
                        label.Content = VideoStreamInfo?.Gain;
                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelExposure");
                    if (label != null)
                        label.Content = VideoStreamInfo?.Exposure;
                    label = UIElementHelper.FindChild<Label>(contentGrid, "LabelFocus");
                    if (label != null)
                        label.Content = VideoStreamInfo?.Focus;
                    break;
                default: break;
            }

            ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(
                new SimpleContentDialogCreateOptions()
                {
                    Title = App.GetStringResource("video-tab-videoprop-dialog-title"),
                    Content = content,
                    CloseButtonText = App.GetStringResource("button-ok")
                }
            );
        }

        [RelayCommand]
        private async Task OnSaveImage(object content)
        {
            var contentGrid = content as Grid;
            if (contentGrid == null)
                return;
            if (CurrentImageMat == null)
                return;

            Mat picToSave = CurrentImageMat.Clone();
            var image = UIElementHelper.FindChild<System.Windows.Controls.Image>(contentGrid, "DisplayedImage");
            if (image != null)
                image.Source = picToSave.ToBitmapSource();

            ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(
                new SimpleContentDialogCreateOptions()
                {
                    Title = App.GetStringResource("video-tab-imagesave-dialog-title"),
                    Content = content,
                    PrimaryButtonText = App.GetStringResource("button-ok"),
                    CloseButtonText = App.GetStringResource("button-cancel")
                }
            );
            if (result == ContentDialogResult.None)
                return;

            var options = UIElementHelper.FindChild<SaveImageOptions>(contentGrid, "Options");
            if (options == null)
                return;

            string filter = string.Empty;
            string extension = string.Empty;
            ImageEncodingParam[]? imageEncodingParams = options.ImageEncodingParams;
            switch (options.SelectedImageType)
            {
                case ImageType.Jpeg:
                    filter = $"{App.GetStringResource("uc-saveimage-dialog-filter-imagefiles")} (*.jpg, *.jpeg)|*.jpg;*.jpeg|{App.GetStringResource("uc-saveimage-dialog-filter-allfiles")} (*.*)|*.*";
                    extension = ".jpg";
                    break;
                case ImageType.Png:
                    filter = $"{App.GetStringResource("uc-saveimage-dialog-filter-imagefiles")} (*.png)|*.png|{App.GetStringResource("uc-saveimage-dialog-filter-allfiles")} (*.*)|*.*";
                    extension = ".png";
                    break;
                case ImageType.Webp:
                    filter = $"{App.GetStringResource("uc-saveimage-dialog-filter-imagefiles")} (*.webp)|*.webp|{App.GetStringResource("uc-saveimage-dialog-filter-allfiles")} (*.*)|*.*";
                    extension = ".webp";
                    break;
                case ImageType.Tiff:
                    filter = $"{App.GetStringResource("uc-saveimage-dialog-filter-imagefiles")} (*.tiff)|*.tiff|{App.GetStringResource("uc-saveimage-dialog-filter-allfiles")} (*.*)|*.*";
                    extension = ".tiff";
                    break;
                case ImageType.Bmp:
                    filter = $"{App.GetStringResource("uc-saveimage-dialog-filter-imagefiles")} (*.bmp)|*.bmp|{App.GetStringResource("uc-saveimage-dialog-filter-allfiles")} (*.*)|*.*";
                    extension = ".bmp";
                    break;
                default:
                    filter = $"{App.GetStringResource("uc-saveimage-dialog-filter-allfiles")} (*.*)|*.*";
                    break;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = filter
            };
            if (saveFileDialog.ShowDialog() != true)
                return;

            string fileName = Path.Combine(Path.GetDirectoryName(saveFileDialog.FileName), $"{Path.GetFileNameWithoutExtension(saveFileDialog.FileName)}{extension}");
            bool savedResult = false;
            if (imageEncodingParams == null)
                savedResult = picToSave.SaveImage(fileName);
            else
                savedResult = picToSave.SaveImage(fileName, imageEncodingParams);

            if (savedResult)
                ShowSuccessStatus(App.GetStringResource("uc-saveimage-success-title"), fileName);
            else
                ShowErrorStatus(App.GetStringResource("uc-saveimage-failure-title"), fileName);
        }
        #endregion
    }

    public partial class CameraInfo : ObservableObject
    {
        public string? Name { get; init; }
        public int? Index { get; init; }
        [ObservableProperty]
        private bool _isChecked = false;
    }

    public partial class CommonVideoCaptureInfo : ObservableObject
    {
        public double? FrameWidth { get; set; }
        public double? FrameHeight { get; set; }
    }
    public partial class VideoFileInfo : ObservableObject
    {
        public double? FrameCount { get; set; }
        public double? FourCC { get; set; }
        public string FourCCCodec { get; set; } = string.Empty;

        [ObservableProperty]
        private double? _posMsec = 0;
        partial void OnPosMsecChanged(double? value)
        {
            if (value == null)
                Position = string.Empty;
            else
                Position = TimeSpan.FromMilliseconds(value.Value).ToString();
        }
        [ObservableProperty]
        private double? _posFrames = 0;
        [ObservableProperty]
        private double? _posAviRatio = 0;
        [ObservableProperty]
        private string _position = string.Empty;
    }
    public partial class VideoStreamInfo : ObservableObject
    {
        public double? Fps { get; set; }
        public double? Brightness { get; set; }
        public double? Contrast { get; set; }
        public double? Saturation { get; set; }
        public double? Hue { get; set; }
        public double? Gain { get; set; }
        public double? Exposure { get; set; }
        public double? Focus { get; set; }

        [ObservableProperty]
        private double? _cameraFrameWidth;
    }
}
