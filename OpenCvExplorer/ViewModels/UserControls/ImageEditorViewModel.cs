using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Win32;
using OpenCvExplorer.Helpers;
using OpenCvExplorer.ViewModels.Messages;
using OpenCvExplorer.Views.UserControls;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Serilog;
using SkiaSharp;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace OpenCvExplorer.ViewModels.UserControls;

public partial class ImageEditorViewModel : BaseObservableObject
{
    #region Constructors
    public ImageEditorViewModel()
    {
        _contentDialogService = App.GetService<IContentDialogService>();
        _snackbarService = App.GetService<ISnackbarService>();
        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            ImageChannelContent = App.GetStringResource(ImageChannel);
            GenerateHistogramChart();
        });
    }
    #endregion

    #region Properties
    private readonly IContentDialogService _contentDialogService;
    private readonly ISnackbarService _snackbarService;
    private Mat[]? _channelMats;
    private Mat? _presentedMat;
    public Mat? PresentedMat => _presentedMat;

    private OpenCvSharp.Size GridSize
    {
        get => new OpenCvSharp.Size(GridSizeWidth, GridSizeHeight);
    }
    #endregion

    #region Observable properties
    [ObservableProperty]
    private string _fileName = string.Empty;
    partial void OnFileNameChanged(string? oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(newValue) || !Path.Exists(newValue))
            ImageMat = null;

        ImageMat = Cv2.ImRead(FileName);
        _channelMats = Cv2.Split(ImageMat);
        ControlVisibility = Visibility.Visible;

        ImageChannels = new List<Wpf.Ui.Controls.MenuItem>();

        var menuItem = new Wpf.Ui.Controls.MenuItem() { Command = SelectImageChannelCommand, CommandParameter = "uc-imageeditor-imagechannel-all" };
        menuItem.SetResourceReference(Wpf.Ui.Controls.MenuItem.HeaderProperty, "uc-imageeditor-imagechannel-all");
        ImageChannels.Add(menuItem);

        menuItem = new Wpf.Ui.Controls.MenuItem() { Command = SelectImageChannelCommand, CommandParameter = "uc-imageeditor-imagechannel-gray" };
        menuItem.SetResourceReference(Wpf.Ui.Controls.MenuItem.HeaderProperty, "uc-imageeditor-imagechannel-gray");
        ImageChannels.Add(menuItem);

        for (int i = 0; i < ImageMat.Channels(); i++)
        {
            menuItem = new Wpf.Ui.Controls.MenuItem() { Command = SelectImageChannelCommand, CommandParameter = $"uc-imageeditor-imagechannel-channel-{i}" };
            menuItem.SetResourceReference(Wpf.Ui.Controls.MenuItem.HeaderProperty, $"uc-imageeditor-imagechannel-channel-{i}");
            ImageChannels.Add(menuItem);
        }

        CanvasWidth = ImageMat.Size().Width;
        CanvasHeight = ImageMat.Size().Height;
        Log.Logger.Verbose("Opened image: {0}", FileName);
    }
    [ObservableProperty]
    private Mat? _imageMat = null;
    partial void OnImageMatChanged(Mat? oldValue, Mat? newValue)
    {
        if (newValue == null)
            ImageSource = null;
        else
            GenerateImageSource();
    }
    [ObservableProperty]
    private BitmapSource? _imageSource = null;
    [ObservableProperty]
    private int _canvasWidth = 0;
    [ObservableProperty]
    private int _canvasHeight = 0;
    [ObservableProperty]
    private OpenCvSharp.Rect _roiRect = new(0, 0, 0, 0);
    partial void OnRoiRectChanged(OpenCvSharp.Rect oldValue, OpenCvSharp.Rect newValue)
    {
        if (newValue.X < 0 || newValue.Y < 0 || newValue.Width <= 0 || newValue.Height <= 0)
        {
            ImageEqualizeSection.BackProjectImageSource = null;
            ImageEqualizeSection.BackProjectVisibility = Visibility.Collapsed;
            return;
        }
        GenerateBackProjectImage();
    }
    [ObservableProperty]
    private Visibility _controlVisibility = Visibility.Hidden;
    [ObservableProperty]
    private Stretch _stretch = Stretch.None;
    [ObservableProperty]
    private string _imageChannel = "uc-imageeditor-imagechannel-all";
    partial void OnImageChannelChanged(string? oldValue, string newValue)
    {
        if (ImageMat != null)
            GenerateImageSource();
    }
    [ObservableProperty]
    private string _imageChannelContent = App.GetStringResource("uc-imageeditor-imagechannel-all");
    [ObservableProperty]
    private bool _bitwiseNotOn = false;
    partial void OnBitwiseNotOnChanged(bool oldValue, bool newValue)
    {
        if (ImageMat != null)
            GenerateImageSource();
    }
    [ObservableProperty]
    private List<Wpf.Ui.Controls.MenuItem>? _imageChannels = null;
    [ObservableProperty]
    private System.Windows.Point? _currentPoint = null;
    partial void OnCurrentPointChanged(System.Windows.Point? value)
    {
        var statusString = string.Empty;

        if (_presentedMat != null)
        {
            if (value.HasValue)
            {
                var pixel = _presentedMat.Get<Vec3b>((int)value.Value.Y, (int)value.Value.X);
                statusString = $"{App.GetStringResource("uc-imageeditor-status-point")}: ({value.Value.X}, {value.Value.Y}) {App.GetStringResource("uc-imageeditor-status-pixel")} (RGB): ({pixel.Item2}, {pixel.Item1}, {pixel.Item0})";
            }
        }

        ShowInformationalStatus(string.Empty, statusString);
    }
    [ObservableProperty]
    private SaveImageOptionsViewModel _imageSavingParameters = new SaveImageOptionsViewModel();

    [ObservableProperty]
    private ImageStatisticsSection _imageStatisticsSection = new ImageStatisticsSection();
    [ObservableProperty]
    private ImageEqualizeSection _imageEqualizeSection = new ImageEqualizeSection();
    [ObservableProperty]
    private int _histSize = 4;
    partial void OnHistSizeChanged(int oldValue, int newValue)
    {
        GenerateHistogramChart();
    }
    [ObservableProperty]
    private double _clipLimit = 2.0;
    partial void OnClipLimitChanged(double oldValue, double newValue)
    {
        ClipLimit = Math.Round(newValue, 1);
        GenerateEqualizeHistImages();
    }
    [ObservableProperty]
    private int _gridSizeWidth = 8;
    partial void OnGridSizeWidthChanged(int oldValue, int newValue)
    {
        GenerateEqualizeHistImages();
    }
    [ObservableProperty]
    private int _gridSizeHeight = 8;
    partial void OnGridSizeHeightChanged(int oldValue, int newValue)
    {
        GenerateEqualizeHistImages();
    }
    [ObservableProperty]
    private int _backProjHueHistSize = 32;
    partial void OnBackProjHueHistSizeChanged(int oldValue, int newValue)
    {
        GenerateBackProjectImage();
    }
    [ObservableProperty]
    private int _backProjSaturationHistSize = 32;
    partial void OnBackProjSaturationHistSizeChanged(int value)
    {
        GenerateBackProjectImage();
    }
    #endregion

    #region Relay commands
    [RelayCommand]
    private void OnSetImageStretchNone()
    {
        Stretch = Stretch.None;
    }
    [RelayCommand]
    private void OnSetImageStretchFill()
    {
        Stretch = Stretch.Fill;
    }
    [RelayCommand]
    private void OnSetImageStretchUniform()
    {
        Stretch = Stretch.Uniform;
    }
    [RelayCommand]
    private void OnSetImageStretchUniformToFill()
    {
        Stretch = Stretch.UniformToFill;
    }

    [RelayCommand]
    private async Task OnShowImageProperties(object content)
    {
        var contentGrid = content as Grid;
        if (contentGrid == null)
            return;
        var label = UIElementHelper.FindChild<Label>(contentGrid, "LabelImageType");
        if (label != null)
            label.Content = ImageMat.Type().ToString();
        label = UIElementHelper.FindChild<Label>(contentGrid, "LabelImageWidth");
        if (label != null)
            label.Content = ImageMat.Width.ToString();
        label = UIElementHelper.FindChild<Label>(contentGrid, "LabelImageHeight");
        if (label != null)
            label.Content = ImageMat.Height.ToString();
        label = UIElementHelper.FindChild<Label>(contentGrid, "LabelImageChannels");
        if (label != null)
            label.Content = ImageMat.Channels().ToString();
        label = UIElementHelper.FindChild<Label>(contentGrid, "LabelImageDepth");
        if (label != null)
            label.Content = ImageMat.Depth().ToString();

        ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(
            new SimpleContentDialogCreateOptions()
            {
                Title = App.GetStringResource("uc-imageeditor-imageprop-dialog-title"),
                Content = content,
                CloseButtonText = App.GetStringResource("button-ok")
            }
        );

        //_snackbarService.Show(
        //    App.GetStringResource("uc-imageeditor-imageprop-dialog-title"),
        //    "No Witcher's Ever Died In His Bed.",
        //    ControlAppearance.Info,
        //    new SymbolIcon(SymbolRegular.Fluent24),
        //    TimeSpan.FromSeconds(30)
        //);
    }
    [RelayCommand]
    private void OnSelectImageChannel(object parameter)
    {
        ImageChannel = parameter.ToString()!;
        ImageChannelContent = App.GetStringResource(parameter.ToString()!);
    }
    [RelayCommand]
    private async Task OnSaveImage(object content)
    {
        var options = content as SaveImageOptions;
        if (options == null)
            return;

        ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(
            new SimpleContentDialogCreateOptions()
            {
                Title = App.GetStringResource("uc-imageeditor-saveimage-dialog-title"),
                Content = content,
                PrimaryButtonText = App.GetStringResource("button-ok"),
                CloseButtonText = App.GetStringResource("button-cancel")
            }
        );
        if (result == ContentDialogResult.None)
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

        if (_presentedMat == null)
            return;

        string fileName = Path.Combine(Path.GetDirectoryName(saveFileDialog.FileName), $"{Path.GetFileNameWithoutExtension(saveFileDialog.FileName)}{extension}");
        bool savedResult = false;
        if (imageEncodingParams == null)
            savedResult = _presentedMat.SaveImage(fileName);
        else
            savedResult = _presentedMat.SaveImage(fileName, imageEncodingParams);

        if (savedResult)
            ShowSuccessStatus(App.GetStringResource("uc-saveimage-success-title"), fileName);
        else
            ShowErrorStatus(App.GetStringResource("uc-saveimage-failure-title"), fileName);
    }
    [RelayCommand]
    private void OnCloseImage()
    {
        WeakReferenceMessenger.Default.Send(new ImageClosingMessage(FileName));
    }
    [RelayCommand]
    private void OnExpandCollapseStatistics()
    {
        if (ImageStatisticsSection.Visibility == Visibility.Visible)
        {
            ImageStatisticsSection.Visibility = Visibility.Collapsed;
            ImageStatisticsSection.Icon = new SymbolIcon(SymbolRegular.ChevronDown24);
        }
        else
        {
            ImageStatisticsSection.Visibility = Visibility.Visible;
            ImageEqualizeSection.Visibility = Visibility.Collapsed;
            ImageStatisticsSection.Icon = new SymbolIcon(SymbolRegular.ChevronUp24);
        }
    }
    [RelayCommand]
    private void OnExpandCollapseEqualize()
    {
        if (ImageEqualizeSection.Visibility == Visibility.Visible)
        {
            ImageEqualizeSection.Visibility = Visibility.Collapsed;
        }
        else
        {
            ImageEqualizeSection.Visibility = Visibility.Visible;
            ImageStatisticsSection.Visibility = Visibility.Collapsed;
        }
    }
    #endregion

    #region Private functions
    private void GenerateImageSource()
    {
        _presentedMat = new Mat(ImageMat.Height, ImageMat.Width, MatType.CV_8UC1);

        switch (ImageChannel)
        {
            case "uc-imageeditor-imagechannel-all":
                _presentedMat = new Mat(ImageMat.Height, ImageMat.Width, MatType.CV_8UC3);
                ImageMat.CopyTo(_presentedMat);
                break;
            case "uc-imageeditor-imagechannel-gray":
                Cv2.CvtColor(ImageMat, _presentedMat, ColorConversionCodes.BGR2GRAY);
                break;
            case "uc-imageeditor-imagechannel-channel-0":
                if (_channelMats != null && _channelMats.Length > 0)
                    _presentedMat = CreateSingleChannelImage(0);
                break;
            case "uc-imageeditor-imagechannel-channel-1":
                if (_channelMats != null && _channelMats.Length > 1)
                    _presentedMat = CreateSingleChannelImage(1);
                break;
            case "uc-imageeditor-imagechannel-channel-2":
                if (_channelMats != null && _channelMats.Length > 2)
                    _presentedMat = CreateSingleChannelImage(2);
                break;
            case "uc-imageeditor-imagechannel-channel-3":
                if (_channelMats != null && _channelMats.Length > 3)
                    _presentedMat = CreateSingleChannelImage(3);
                break;
            default: return;
        }
        if (BitwiseNotOn)
            Cv2.BitwiseNot(_presentedMat, _presentedMat);

        Cv2.MeanStdDev(_presentedMat, out Scalar mean, out Scalar stddev);
        switch (ImageChannel)
        {
            case "uc-imageeditor-imagechannel-all":
                ImageStatisticsSection.Red.Mean = mean.Val2;
                ImageStatisticsSection.Red.StdDev = stddev.Val2;
                ImageStatisticsSection.Green.Mean = mean.Val1;
                ImageStatisticsSection.Green.StdDev = stddev.Val1;
                ImageStatisticsSection.Blue.Mean = mean.Val0;
                ImageStatisticsSection.Blue.StdDev = stddev.Val0;
                ImageStatisticsSection.Gray.Mean = 0;
                ImageStatisticsSection.Gray.StdDev = 0;
                break;
            case "uc-imageeditor-imagechannel-gray":
                ImageStatisticsSection.Red.Mean = 0;
                ImageStatisticsSection.Red.StdDev = 0;
                ImageStatisticsSection.Green.Mean = 0;
                ImageStatisticsSection.Green.StdDev = 0;
                ImageStatisticsSection.Blue.Mean = 0;
                ImageStatisticsSection.Blue.StdDev = 0;
                ImageStatisticsSection.Gray.Mean = mean.Val0;
                ImageStatisticsSection.Gray.StdDev = stddev.Val0;
                break;
            case "uc-imageeditor-imagechannel-channel-0":
                ImageStatisticsSection.Red.Mean = 0;
                ImageStatisticsSection.Red.StdDev = 0;
                ImageStatisticsSection.Green.Mean = 0;
                ImageStatisticsSection.Green.StdDev = 0;
                ImageStatisticsSection.Blue.Mean = mean.Val0;
                ImageStatisticsSection.Blue.StdDev = stddev.Val0;
                ImageStatisticsSection.Gray.Mean = 0;
                ImageStatisticsSection.Gray.StdDev = 0;

                ImageStatisticsSection.RgbSeries = new ISeries[1];
                Mat hist = new();
                Cv2.CalcHist([_presentedMat],
                    [0],
                    null, hist, 1,
                    [HistSize],
                    [new Rangef(0.0f, 256.0f)]);

                var bSeriesValues = new int[hist.Rows];
                for (int i = 0; i < hist.Rows; i++)
                {
                    bSeriesValues[i] = (int)hist.Get<float>(i);
                }
                var bSeries = new ColumnSeries<int>
                {
                    Values = bSeriesValues,
                    Name = App.GetStringResource("uc-imageeditor-imagestatistics-blue"),
                    Fill = new SolidColorPaint(SKColors.Blue)
                };
                ImageStatisticsSection.RgbSeries[0] = bSeries;
                break;
            case "uc-imageeditor-imagechannel-channel-1":
                ImageStatisticsSection.Red.Mean = 0;
                ImageStatisticsSection.Red.StdDev = 0;
                ImageStatisticsSection.Green.Mean = mean.Val1;
                ImageStatisticsSection.Green.StdDev = stddev.Val1;
                ImageStatisticsSection.Blue.Mean = 0;
                ImageStatisticsSection.Blue.StdDev = 0;
                ImageStatisticsSection.Gray.Mean = 0;
                ImageStatisticsSection.Gray.StdDev = 0;
                break;
            case "uc-imageeditor-imagechannel-channel-2":
                ImageStatisticsSection.Red.Mean = mean.Val2;
                ImageStatisticsSection.Red.StdDev = stddev.Val2;
                ImageStatisticsSection.Green.Mean = 0;
                ImageStatisticsSection.Green.StdDev = 0;
                ImageStatisticsSection.Blue.Mean = 0;
                ImageStatisticsSection.Blue.StdDev = 0;
                ImageStatisticsSection.Gray.Mean = 0;
                ImageStatisticsSection.Gray.StdDev = 0;
                break;
            case "uc-imageeditor-imagechannel-channel-3":
            default:
                break;
        }
        GenerateHistogramChart();
        GenerateEqualizeHistImages();
        GenerateBackProjectImage();
        ImageSource = _presentedMat.ToBitmapSource();
    }

    private void GenerateHistogramChart()
    {
        ImageStatisticsSection.RgbSeries = null;
        ImageStatisticsSection.HsvSeries = null;
        ImageStatisticsSection.HsvChartVisibility = Visibility.Collapsed;

        if (_presentedMat == null)
            return;

        switch (ImageChannel)
        {
            case "uc-imageeditor-imagechannel-all":
                {
                    var channels = _presentedMat.Channels();
                    ImageStatisticsSection.RgbSeries = new ISeries[channels];
                    for (int channel = 0; channel < channels; channel++)
                    {
                        Mat hist = new();
                        Cv2.CalcHist([_presentedMat],
                            [channel],
                            null, hist, 1,
                            [HistSize],
                            [new Rangef(0.0f, 256.0f)]);
                        switch (channel)
                        {
                            case 0: // blue
                                var bSeriesValues = new int[hist.Rows];
                                for (int i = 0; i < hist.Rows; i++)
                                {
                                    bSeriesValues[i] = (int)hist.Get<float>(i);
                                }
                                var bSeries = new ColumnSeries<int>
                                {
                                    Values = bSeriesValues,
                                    Name = App.GetStringResource("uc-imageeditor-imagestatistics-blue"),
                                    Fill = new SolidColorPaint(SKColors.Blue)
                                };
                                ImageStatisticsSection.RgbSeries[2] = bSeries;
                                break;
                            case 1: // green
                                var gSeriesValues = new int[hist.Rows];
                                for (int i = 0; i < hist.Rows; i++)
                                {
                                    gSeriesValues[i] = (int)hist.Get<float>(i);
                                }
                                var gSeries = new ColumnSeries<int>
                                {
                                    Values = gSeriesValues,
                                    Name = App.GetStringResource("uc-imageeditor-imagestatistics-green"),
                                    Fill = new SolidColorPaint(SKColors.Green)
                                };
                                ImageStatisticsSection.RgbSeries[1] = gSeries;
                                break;
                            case 2: // red
                                var rSeriesValues = new int[hist.Rows];
                                for (int i = 0; i < hist.Rows; i++)
                                {
                                    rSeriesValues[i] = (int)hist.Get<float>(i);
                                }
                                var rSeries = new ColumnSeries<int>
                                {
                                    Values = rSeriesValues,
                                    Name = App.GetStringResource("uc-imageeditor-imagestatistics-red"),
                                    Fill = new SolidColorPaint(SKColors.Red)
                                };
                                ImageStatisticsSection.RgbSeries[0] = rSeries;
                                break;
                            default: break;
                        }
                    }

                    Mat hsvMat = new();
                    Cv2.CvtColor(_presentedMat, hsvMat, ColorConversionCodes.BGR2HSV);
                    channels = hsvMat.Channels();
                    ImageStatisticsSection.HsvSeries = new ISeries[channels];
                    for (int channel = 0; channel < channels; channel++)
                    {
                        Mat hist = new();
                        Cv2.CalcHist([hsvMat],
                            [channel],
                            null, hist, 1,
                            [HistSize],
                            [new Rangef(0.0f, 256.0f)]);
                        switch (channel)
                        {
                            case 0: // hue
                                var hSeriesValues = new int[hist.Rows];
                                for (int i = 0; i < hist.Rows; i++)
                                {
                                    hSeriesValues[i] = (int)hist.Get<float>(i);
                                }
                                var hSeries = new ColumnSeries<int>
                                {
                                    Values = hSeriesValues,
                                    Name = App.GetStringResource("uc-imageeditor-imagestatistics-hue"),
                                    Fill = new SolidColorPaint(SKColors.Red)
                                };
                                ImageStatisticsSection.HsvSeries[0] = hSeries;
                                break;
                            case 1: // satuation
                                var sSeriesValues = new int[hist.Rows];
                                for (int i = 0; i < hist.Rows; i++)
                                {
                                    sSeriesValues[i] = (int)hist.Get<float>(i);
                                }
                                var sSeries = new ColumnSeries<int>
                                {
                                    Values = sSeriesValues,
                                    Name = App.GetStringResource("uc-imageeditor-imagestatistics-saturation"),
                                    Fill = new SolidColorPaint(SKColors.Green)
                                };
                                ImageStatisticsSection.HsvSeries[1] = sSeries;
                                break;
                            case 2: // value
                                var vSeriesValues = new int[hist.Rows];
                                for (int i = 0; i < hist.Rows; i++)
                                {
                                    vSeriesValues[i] = (int)hist.Get<float>(i);
                                }
                                var vSeries = new ColumnSeries<int>
                                {
                                    Values = vSeriesValues,
                                    Name = App.GetStringResource("uc-imageeditor-imagestatistics-value"),
                                    Fill = new SolidColorPaint(SKColors.DarkGray)
                                };
                                ImageStatisticsSection.HsvSeries[2] = vSeries;
                                break;
                            default: break;
                        }
                    }
                    ImageStatisticsSection.HsvChartVisibility = Visibility.Visible;
                }
                break;
            case "uc-imageeditor-imagechannel-gray":
                {
                    ImageStatisticsSection.RgbSeries = new ISeries[1];
                    Mat hist = new();
                    Cv2.CalcHist([_presentedMat],
                        [0],
                        null, hist, 1,
                        [HistSize],
                        [new Rangef(0.0f, 256.0f)]);

                    var gSeriesValues = new int[hist.Rows];
                    for (int i = 0; i < hist.Rows; i++)
                    {
                        gSeriesValues[i] = (int)hist.Get<float>(i);
                    }
                    var gSeries = new ColumnSeries<int>
                    {
                        Values = gSeriesValues,
                        Name = App.GetStringResource("uc-imageeditor-imagestatistics-gray"),
                        Fill = new SolidColorPaint(SKColors.Gray)
                    };
                    ImageStatisticsSection.RgbSeries[0] = gSeries;
                }
                break;
            case "uc-imageeditor-imagechannel-channel-0":
                {
                    ImageStatisticsSection.RgbSeries = new ISeries[1];
                    Mat hist = new();
                    Cv2.CalcHist([_presentedMat],
                        [0],
                        null, hist, 1,
                        [HistSize],
                        [new Rangef(0.0f, 256.0f)]);

                    var bSeriesValues = new int[hist.Rows];
                    for (int i = 0; i < hist.Rows; i++)
                    {
                        bSeriesValues[i] = (int)hist.Get<float>(i);
                    }
                    var bSeries = new ColumnSeries<int>
                    {
                        Values = bSeriesValues,
                        Name = App.GetStringResource("uc-imageeditor-imagestatistics-blue"),
                        Fill = new SolidColorPaint(SKColors.Blue)
                    };
                    ImageStatisticsSection.RgbSeries[0] = bSeries;
                }
                break;
            case "uc-imageeditor-imagechannel-channel-1":
                {
                    ImageStatisticsSection.RgbSeries = new ISeries[1];
                    Mat hist = new();
                    Cv2.CalcHist([_presentedMat],
                        [0],
                        null, hist, 1,
                        [HistSize],
                        [new Rangef(0.0f, 256.0f)]);

                    var gSeriesValues = new int[hist.Rows];
                    for (int i = 0; i < hist.Rows; i++)
                    {
                        gSeriesValues[i] = (int)hist.Get<float>(i);
                    }
                    var gSeries = new ColumnSeries<int>
                    {
                        Values = gSeriesValues,
                        Name = App.GetStringResource("uc-imageeditor-imagestatistics-green"),
                        Fill = new SolidColorPaint(SKColors.Green)
                    };
                    ImageStatisticsSection.RgbSeries[0] = gSeries;
                }
                break;
            case "uc-imageeditor-imagechannel-channel-2":
                {
                    ImageStatisticsSection.RgbSeries = new ISeries[1];
                    Mat hist = new();
                    Cv2.CalcHist([_presentedMat],
                        [0],
                        null, hist, 1,
                        [HistSize],
                        [new Rangef(0.0f, 256.0f)]);

                    var rSeriesValues = new int[hist.Rows];
                    for (int i = 0; i < hist.Rows; i++)
                    {
                        rSeriesValues[i] = (int)hist.Get<float>(i);
                    }
                    var rSeries = new ColumnSeries<int>
                    {
                        Values = rSeriesValues,
                        Name = App.GetStringResource("uc-imageeditor-imagestatistics-red"),
                        Fill = new SolidColorPaint(SKColors.Red)
                    };
                    ImageStatisticsSection.RgbSeries[0] = rSeries;
                }
                break;
            case "uc-imageeditor-imagechannel-channel-3":
            default:
                break;
        }

        if (ImageStatisticsSection.RgbSeries != null || ImageStatisticsSection.HsvSeries != null)
        {
            var labels = new List<string>();
            for (int i = 0; i < HistSize; i++)
            {
                var label = $"{(int)(i * (256.0 / HistSize))}~{(int)((i + 1) * (256.0 / HistSize)) - 1}";
                labels.Add(label);
            }
            ImageStatisticsSection.XLabels = labels.ToArray();
        }
    }

    private void GenerateEqualizeHistImages()
    {
        ImageEqualizeSection.EqualizeImageSource = null;
        ImageEqualizeSection.ClaheImageSource = null;

        if (_presentedMat == null)
            return;

        switch (ImageChannel)
        {
            case "uc-imageeditor-imagechannel-all":
                {
                    Mat hsvMat = new();
                    Cv2.CvtColor(_presentedMat, hsvMat, ColorConversionCodes.BGR2HSV);

                    Mat[] hsvMv = Cv2.Split(hsvMat);
                    Mat[] copiedHsvMv = new Mat[hsvMv.Length];
                    hsvMv.CopyTo(copiedHsvMv, 0);
                    Cv2.EqualizeHist(copiedHsvMv[2], copiedHsvMv[2]);
                    Mat copiedHsvMat = new();
                    Cv2.Merge(copiedHsvMv, copiedHsvMat);
                    Mat equalizedMat = new();
                    Cv2.CvtColor(copiedHsvMat, equalizedMat, ColorConversionCodes.HSV2BGR);
                    ImageEqualizeSection.EqualizeImageSource = equalizedMat.ToBitmapSource();

                    copiedHsvMv = new Mat[hsvMv.Length];
                    hsvMv.CopyTo(copiedHsvMv, 0);
                    Cv2.CreateCLAHE(ClipLimit, GridSize).Apply(copiedHsvMv[2], copiedHsvMv[2]);
                    copiedHsvMat = new();
                    Cv2.Merge(copiedHsvMv, copiedHsvMat);
                    equalizedMat = new();
                    Cv2.CvtColor(copiedHsvMat, equalizedMat, ColorConversionCodes.HSV2BGR);
                    ImageEqualizeSection.ClaheImageSource = equalizedMat.ToBitmapSource();
                }
                break;
            case "uc-imageeditor-imagechannel-gray":
                {
                    Mat copiedMat = new();
                    _presentedMat.CopyTo(copiedMat);
                    Cv2.EqualizeHist(copiedMat, copiedMat);
                    ImageEqualizeSection.EqualizeImageSource = copiedMat.ToBitmapSource();
                    copiedMat = new();
                    _presentedMat.CopyTo(copiedMat);
                    Cv2.CreateCLAHE(ClipLimit, GridSize).Apply(copiedMat, copiedMat);
                    ImageEqualizeSection.ClaheImageSource = copiedMat.ToBitmapSource();
                }
                break;
            case "uc-imageeditor-imagechannel-channel-0":
                {
                    Mat[] mats = Cv2.Split(_presentedMat);
                    Mat[] copiedMats = new Mat[mats.Length];
                    mats.CopyTo(copiedMats, 0);
                    Cv2.EqualizeHist(copiedMats[0], copiedMats[0]);
                    Mat equalizedMat = new();
                    Cv2.Merge(copiedMats, equalizedMat);
                    ImageEqualizeSection.EqualizeImageSource = equalizedMat.ToBitmapSource();
                    copiedMats = new Mat[mats.Length];
                    mats.CopyTo(copiedMats, 0);
                    Cv2.CreateCLAHE(ClipLimit, GridSize).Apply(copiedMats[0], copiedMats[0]);
                    equalizedMat = new();
                    Cv2.Merge(copiedMats, equalizedMat);
                    ImageEqualizeSection.ClaheImageSource = equalizedMat.ToBitmapSource();
                }
                break;
            case "uc-imageeditor-imagechannel-channel-1":
                {
                    Mat[] mats = Cv2.Split(_presentedMat);
                    Mat[] copiedMats = new Mat[mats.Length];
                    mats.CopyTo(copiedMats, 0);
                    Cv2.EqualizeHist(copiedMats[1], copiedMats[1]);
                    Mat equalizedMat = new();
                    Cv2.Merge(copiedMats, equalizedMat);
                    ImageEqualizeSection.EqualizeImageSource = equalizedMat.ToBitmapSource();
                    copiedMats = new Mat[mats.Length];
                    mats.CopyTo(copiedMats, 0);
                    Cv2.CreateCLAHE(ClipLimit, GridSize).Apply(copiedMats[1], copiedMats[1]);
                    equalizedMat = new();
                    Cv2.Merge(copiedMats, equalizedMat);
                    ImageEqualizeSection.ClaheImageSource = equalizedMat.ToBitmapSource();
                }
                break;
            case "uc-imageeditor-imagechannel-channel-2":
                {
                    Mat[] mats = Cv2.Split(_presentedMat);
                    Mat[] copiedMats = new Mat[mats.Length];
                    mats.CopyTo(copiedMats, 0);
                    Cv2.EqualizeHist(copiedMats[2], copiedMats[2]);
                    Mat equalizedMat = new();
                    Cv2.Merge(copiedMats, equalizedMat);
                    ImageEqualizeSection.EqualizeImageSource = equalizedMat.ToBitmapSource();
                    copiedMats = new Mat[mats.Length];
                    mats.CopyTo(copiedMats, 0);
                    Cv2.CreateCLAHE(ClipLimit, GridSize).Apply(copiedMats[2], copiedMats[2]);
                    equalizedMat = new();
                    Cv2.Merge(copiedMats, equalizedMat);
                    ImageEqualizeSection.ClaheImageSource = equalizedMat.ToBitmapSource();
                }
                break;
            case "uc-imageeditor-imagechannel-channel-3":
            default:
                break;
        }
    }

    private void GenerateBackProjectImage()
    {
        ImageEqualizeSection.BackProjectImageSource = null;

        if (_presentedMat == null || RoiRect.Width <= 0 || RoiRect.Height <= 0)
            return;

        Mat roiMat = new(_presentedMat, RoiRect);
        int[] histSizes = [BackProjHueHistSize, BackProjSaturationHistSize];
        switch (ImageChannel)
        {
            case "uc-imageeditor-imagechannel-all":
                {
                    Mat imageHsvMat = new();
                    Cv2.CvtColor(_presentedMat, imageHsvMat, ColorConversionCodes.BGR2HSV);
                    Mat roiHsvMat = new();
                    Cv2.CvtColor(roiMat, roiHsvMat, ColorConversionCodes.BGR2HSV);

                    Rangef[] ranges = [new Rangef(0, 180), new Rangef(0, 256)];
                    int[] channels = [0, 1];

                    Mat roiHist = new();
                    Cv2.CalcHist([roiHsvMat], channels, null, roiHist, 1, histSizes, ranges);
                    Cv2.Normalize(roiHist, roiHist, 0, 255, NormTypes.MinMax);

                    Mat backprojectMat = new();
                    Cv2.CalcBackProject([imageHsvMat], channels, roiHist, backprojectMat, ranges);

                    ImageEqualizeSection.BackProjectImageSource = backprojectMat.ToBitmapSource();
                    ImageEqualizeSection.BackProjectVisibility = Visibility.Visible;
                }
                break;
            case "uc-imageeditor-imagechannel-gray":
                {
                    Rangef[] ranges = [new Rangef(0, 256)];
                    int[] channels = [0];

                    Mat roiHist = new();
                    Cv2.CalcHist([roiMat], channels, null, roiHist, 1, histSizes, ranges);
                    Cv2.Normalize(roiHist, roiHist, 0, 255, NormTypes.MinMax);

                    Mat backprojectMat = new();
                    Cv2.CalcBackProject([_presentedMat], channels, roiHist, backprojectMat, ranges);

                    ImageEqualizeSection.BackProjectImageSource = backprojectMat.ToBitmapSource();
                    ImageEqualizeSection.BackProjectVisibility = Visibility.Visible;
                }
                break;
            case "uc-imageeditor-imagechannel-channel-0":
                {
                    Mat[] mats = Cv2.Split(_presentedMat);
                    Rangef[] ranges = [new Rangef(0, 256)];
                    int[] channels = [0];

                    Mat roiHist = new();
                    Cv2.CalcHist([roiMat], channels, null, roiHist, 1, histSizes, ranges);
                    Cv2.Normalize(roiHist, roiHist, 0, 255, NormTypes.MinMax);

                    Mat backprojectMat = new();
                    Cv2.CalcBackProject([mats[0]], channels, roiHist, backprojectMat, ranges);

                    ImageEqualizeSection.BackProjectImageSource = backprojectMat.ToBitmapSource();
                    ImageEqualizeSection.BackProjectVisibility = Visibility.Visible;
                }
                break;
            case "uc-imageeditor-imagechannel-channel-1":
                {
                    Mat[] mats = Cv2.Split(_presentedMat);
                    Rangef[] ranges = [new Rangef(0, 256)];
                    int[] channels = [0];

                    Mat roiHist = new();
                    Cv2.CalcHist([roiMat], channels, null, roiHist, 1, histSizes, ranges);
                    Cv2.Normalize(roiHist, roiHist, 0, 255, NormTypes.MinMax);

                    Mat backprojectMat = new();
                    Cv2.CalcBackProject([mats[1]], channels, roiHist, backprojectMat, ranges);

                    ImageEqualizeSection.BackProjectImageSource = backprojectMat.ToBitmapSource();
                    ImageEqualizeSection.BackProjectVisibility = Visibility.Visible;
                }
                break;
            case "uc-imageeditor-imagechannel-channel-2":
                {
                    Mat[] mats = Cv2.Split(_presentedMat);
                    Rangef[] ranges = [new Rangef(0, 256)];
                    int[] channels = [0];

                    Mat roiHist = new();
                    Cv2.CalcHist([roiMat], channels, null, roiHist, 1, histSizes, ranges);
                    Cv2.Normalize(roiHist, roiHist, 0, 255, NormTypes.MinMax);

                    Mat backprojectMat = new();
                    Cv2.CalcBackProject([mats[2]], channels, roiHist, backprojectMat, ranges);

                    ImageEqualizeSection.BackProjectImageSource = backprojectMat.ToBitmapSource();
                    ImageEqualizeSection.BackProjectVisibility = Visibility.Visible;
                }
                break;
            case "uc-imageeditor-imagechannel-channel-3":
            default:
                break;
        }
    }

    private Mat CreateSingleChannelImage(int channelIndex)
    {
        Mat[] channels = new Mat[3];
        for (int i = 0; i < 3; i++)
        {
            channels[i] = new Mat(ImageMat.Size(), MatType.CV_8UC1, new Scalar(0));
        }

        if (_channelMats != null && _channelMats.Length > channelIndex)
        {
            _channelMats[channelIndex].CopyTo(channels[channelIndex]);
        }

        Mat coloredImage = new Mat();
        Cv2.Merge(channels, coloredImage);
        return coloredImage;
    }
    #endregion
}

public partial class ImageStatisticsSection : BaseObservableObject
{
    [ObservableProperty]
    private Visibility _visibility = Visibility.Collapsed;
    [ObservableProperty]
    private SymbolIcon _icon = new SymbolIcon(SymbolRegular.ChevronDown24);
    [ObservableProperty]
    private ImageStatisticsMetrics _red = new();
    [ObservableProperty]
    private ImageStatisticsMetrics _green = new();
    [ObservableProperty]
    private ImageStatisticsMetrics _blue = new();
    [ObservableProperty]
    private ImageStatisticsMetrics _gray = new();
    [ObservableProperty]
    private ISeries[]? _rgbSeries;
    [ObservableProperty]
    private ISeries[]? _hsvSeries;
    [ObservableProperty]
    private Visibility _hsvChartVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private string[]? _xLabels = Array.Empty<string>();
}
public partial class ImageStatisticsMetrics : BaseObservableObject
{
    [ObservableProperty]
    private double _mean = 0;
    [ObservableProperty]
    private double _stdDev = 0;
}
public partial class ImageEqualizeSection : BaseObservableObject
{
    [ObservableProperty]
    private Visibility _visibility = Visibility.Collapsed;
    [ObservableProperty]
    private BitmapSource? _equalizeImageSource = null;
    [ObservableProperty]
    private BitmapSource? _claheImageSource = null;
    [ObservableProperty]
    private BitmapSource? _backProjectImageSource = null;
    [ObservableProperty]
    private Visibility _backProjectVisibility = Visibility.Collapsed;
}
