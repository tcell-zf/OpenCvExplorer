using CommunityToolkit.Mvvm.Messaging;
using OpenCvExplorer.Helpers;
using OpenCvExplorer.ViewModels.Messages;
using OpenCvExplorer.ViewModels.UserControls;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace OpenCvExplorer.ViewModels.Windows;

public partial class ConvolutionWindowViewModel : ObservableObject
{
    #region Constructors
    public ConvolutionWindowViewModel()
    {
        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            if (_noisyImage != null)
                SetTooltip(_noisyImage);
            else
                SetTooltip(_rawImage);
        });
    }
    #endregion

    #region Properties
    private Mat? _rawImage, _noisyImage;
    public Mat? RawImage
    {
        get => _rawImage;
        set
        {
            if (_rawImage != value)
            {
                _rawImage = value;
                SetTooltip(value);
                RawImageSource = value?.ToBitmapSource();
                MeanBlur = new MeanBlur(value);
                GaussianBlur = new GaussianBlur(value);
                MedianBlur = new MedianBlur(value);
                CustomBlur = new CustomBlur(value);
                Sobel = new Sobel(value);
                Scharr = new Scharr(value);
                Canny = new Canny(value);
                BilateralFilter = new BilateralFilter(value);
                Laplacian = new Laplacian(value);
            }
        }
    }
    #endregion

    #region Observable properties
    [ObservableProperty]
    private BitmapSource? _rawImageSource;
    [ObservableProperty]
    private string _imageTooltip = string.Empty;
    [ObservableProperty]
    private bool _isRawImage = true;
    partial void OnIsRawImageChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            IsSaltAndPepperNoise = false;
            IsGaussianNoise = false;
            SaltAndPepperVisibility = Visibility.Collapsed;
            GaussianVisibility = Visibility.Collapsed;
            SetTooltip(_rawImage);
            RawImageSource = _rawImage?.ToBitmapSource();
            _noisyImage = null;

            GenerateAllBlurImages(_rawImage);
        }
    }
    [ObservableProperty]
    private bool _isSaltAndPepperNoise = false;
    partial void OnIsSaltAndPepperNoiseChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            IsRawImage = false;
            IsGaussianNoise = false;
            SaltAndPepperVisibility = Visibility.Visible;
            GaussianVisibility = Visibility.Collapsed;

            GenerateSaltAndPepperImage();
            GenerateAllBlurImages(_noisyImage);
        }
    }
    [ObservableProperty]
    private bool _isGaussianNoise = false;
    partial void OnIsGaussianNoiseChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            IsRawImage = false;
            IsSaltAndPepperNoise = false;
            SaltAndPepperVisibility = Visibility.Collapsed;
            GaussianVisibility = Visibility.Visible;

            GenerateGaussianImage();
            GenerateAllBlurImages(_noisyImage);
        }
    }
    [ObservableProperty]
    private Visibility _saltAndPepperVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private Visibility _gaussianVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private uint _noiseCount = 5000;
    partial void OnNoiseCountChanged(uint oldValue, uint newValue)
    {
        if (IsSaltAndPepperNoise)
        {
            GenerateSaltAndPepperImage();
            GenerateAllBlurImages(_noisyImage);
        }
    }
    [ObservableProperty]
    private int _mean = 15;
    partial void OnMeanChanged(int oldValue, int newValue)
    {
        if (IsGaussianNoise)
        {
            GenerateSaltAndPepperImage();
            GenerateAllBlurImages(_noisyImage);
        }
    }
    [ObservableProperty]
    private int _stddev = 30;
    partial void OnStddevChanged(int oldValue, int newValue)
    {
        if (IsGaussianNoise)
        {
            GenerateSaltAndPepperImage();
            GenerateAllBlurImages(_noisyImage);
        }
    }
    [ObservableProperty]
    private MeanBlur _meanBlur;
    [ObservableProperty]
    private GaussianBlur _gaussianBlur;
    [ObservableProperty]
    private MedianBlur _medianBlur;
    [ObservableProperty]
    private CustomBlur _customBlur;
    [ObservableProperty]
    private Sobel _sobel;
    [ObservableProperty]
    private Scharr _scharr;
    [ObservableProperty]
    private Canny _canny;
    [ObservableProperty]
    private BilateralFilter _bilateralFilter;
    [ObservableProperty]
    private Laplacian _laplacian;
    #endregion

    #region Private functions
    private void GenerateSaltAndPepperImage()
    {
        if (_rawImage == null)
            return;

        var rng = Cv2.GetTheRNG();
        rng.State = (ulong)DateTime.Now.Ticks;
        _noisyImage = _rawImage.Clone();
        for (int i = 0; i < NoiseCount; i++)
        {
            int x = rng.Uniform(0, _noisyImage.Cols);
            int y = rng.Uniform(0, _noisyImage.Rows);
            if (i % 2 == 0)
                _noisyImage.Set<Vec3b>(y, x, new Vec3b(255, 255, 255));
            else
                _noisyImage.Set<Vec3b>(y, x, new Vec3b(0, 0, 0));
        }
        SetTooltip(_noisyImage);
        RawImageSource = _noisyImage.ToBitmapSource();
    }

    private void GenerateGaussianImage()
    {
        if (_rawImage == null)
            return;

        // generate Gaussian noise image
        Mat noise = Mat.Zeros(_rawImage.Size(), _rawImage.Type());
        Cv2.Randn(noise, new Scalar(Mean), new Scalar(Stddev));
        // add the noise to the original image
        _noisyImage = new();
        Cv2.Add(_rawImage, noise, _noisyImage);
        SetTooltip(_noisyImage);
        RawImageSource = _noisyImage.ToBitmapSource();
    }

    private void GenerateAllBlurImages(Mat image)
    {
        MeanBlur = new MeanBlur(image);
        GaussianBlur = new GaussianBlur(image);
        MedianBlur = new MedianBlur(image);
        CustomBlur = new CustomBlur(image);

        Sobel = new Sobel(image);
        Scharr = new Scharr(image);
        Canny = new Canny(image);
        BilateralFilter = new BilateralFilter(image);
        Laplacian = new Laplacian(image);
    }

    private void SetTooltip(Mat mat)
    {
        var sharpness = Cv2Helper.CalcSharpness(mat);
        ImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
    }
    #endregion
}

public partial class MeanBlur : ObservableObject
{
    #region Constructors
    public MeanBlur(Mat? rawImage)
    {
        _rawImage = rawImage;
        ConvolutionParams.PropertyChanged += ConvolutionParams_PropertyChanged;
        GenerateMeanBlurImage();
        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            SetTooltip();
        });
    }
    #endregion

    #region Properties
    private Mat? _rawImage, _blurredImage;
    #endregion

    #region Observable properties
    [ObservableProperty]
    private BitmapSource? _meanBlurImageSource;
    [ObservableProperty]
    private string _imageTooltip = string.Empty;

    [ObservableProperty]
    private ConvolutionParamsViewModel _convolutionParams = new()
    {
        SigmaVisibility = Visibility.Collapsed,
        OrderVisibility = Visibility.Collapsed,
        KernelSizeVisibility = Visibility.Collapsed,
        DeltaVisibility = Visibility.Collapsed,
        ScaleVisibility = Visibility.Collapsed
    };
    private void ConvolutionParams_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        GenerateMeanBlurImage();
    }
    #endregion

    #region Private functions
    private void GenerateMeanBlurImage()
    {
        if (_rawImage == null)
            return;

        _blurredImage = new();
        try
        {
            Cv2.Blur(_rawImage, _blurredImage, ConvolutionParams.KSize, ConvolutionParams.Anchor, ConvolutionParams.BorderType);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "MeanBlur Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        SetTooltip();
        MeanBlurImageSource = _blurredImage.ToBitmapSource();
    }

    private void SetTooltip()
    {
        var sharpness = Cv2Helper.CalcSharpness(_blurredImage);
        ImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
    }
    #endregion
}
public partial class GaussianBlur : ObservableObject
{
    #region Constructors
    public GaussianBlur(Mat? rawImage)
    {
        _rawImage = rawImage;
        ConvolutionParams.PropertyChanged += ConvolutionParams_PropertyChanged;
        GenerateGaussianBlurImage();
        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            SetTooltip();
        });
    }
    #endregion

    #region Properties
    private Mat? _rawImage, _blurredImage;
    #endregion

    #region Observable properties
    [ObservableProperty]
    private BitmapSource? _gaussianBlurImageSource;
    [ObservableProperty]
    private string _imageTooltip = string.Empty;

    [ObservableProperty]
    private ConvolutionParamsViewModel _convolutionParams = new()
    {
        AnchorVisibility = Visibility.Collapsed,
        OrderVisibility = Visibility.Collapsed,
        KernelSizeVisibility = Visibility.Collapsed,
        DeltaVisibility = Visibility.Collapsed,
        ScaleVisibility = Visibility.Collapsed,
        MustBeOdd = true
    };
    private void ConvolutionParams_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        GenerateGaussianBlurImage();
    }
    #endregion

    #region Private functions
    private void GenerateGaussianBlurImage()
    {
        if (_rawImage == null)
            return;

        _blurredImage = new();
        try
        {
            Cv2.GaussianBlur(_rawImage, _blurredImage, ConvolutionParams.KSize, ConvolutionParams.SigmaX, ConvolutionParams.SigmaY, ConvolutionParams.BorderType);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        SetTooltip();
        GaussianBlurImageSource = _blurredImage.ToBitmapSource();
    }

    private void SetTooltip()
    {
        var sharpness = Cv2Helper.CalcSharpness(_blurredImage);
        ImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
    }
    #endregion
}
public partial class MedianBlur : ObservableObject
{
    #region Constructors
    public MedianBlur(Mat? rawImage)
    {
        _rawImage = rawImage;
        GenerateMedianBlurImage();
        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            SetTooltip();
        });
    }
    #endregion

    #region Properties
    private Mat? _rawImage, _blurredImage;
    #endregion

    #region Observable properties
    [ObservableProperty]
    private BitmapSource? _medianBlurImageSource;
    [ObservableProperty]
    private string _imageTooltip = string.Empty;
    [ObservableProperty]
    private int _kernelSize = 3;
    partial void OnKernelSizeChanged(int oldValue, int newValue)
    {
        if (newValue % 2 == 0)
        {
            if (oldValue < newValue)
                KernelSize++;
            else
                KernelSize--;
        }

        GenerateMedianBlurImage();
    }
    #endregion

    #region Private functions
    private void GenerateMedianBlurImage()
    {
        if (_rawImage == null)
            return;

        _blurredImage = new();
        try
        {
            Cv2.MedianBlur(_rawImage, _blurredImage, KernelSize);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        SetTooltip();
        MedianBlurImageSource = _blurredImage.ToBitmapSource();
    }

    private void SetTooltip()
    {
        var sharpness = Cv2Helper.CalcSharpness(_blurredImage);
        ImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
    }
    #endregion
}
public partial class CustomBlur : ObservableObject
{
    #region Constructors
    public CustomBlur(Mat? rawImage)
    {
        _rawImage = rawImage;
        ConvolutionParams.PropertyChanged += ConvolutionParams_PropertyChanged;
        GenerateCustomBlurImage();
        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            SetTooltip();
        });
    }
    #endregion

    #region Properties
    private Mat? _rawImage, _customBlurImage, _horizontalBlurImage, _verticalBlurImage, _diagonalBlurImage;

    private OpenCvSharp.Size HorizontalKSize
    {
        get => new OpenCvSharp.Size(ConvolutionParams.KernelWidth, 1);
    }
    private OpenCvSharp.Size VerticalKSize
    {
        get => new OpenCvSharp.Size(1, ConvolutionParams.KernelHeight);
    }
    private OpenCvSharp.Size DiagonalKSize
    {
        get => new OpenCvSharp.Size(ConvolutionParams.KernelWidth, ConvolutionParams.KernelHeight);
    }
    #endregion

    #region Observable properties
    [ObservableProperty]
    private BitmapSource? _customBlurImageSource;
    [ObservableProperty]
    private string _customImageTooltip = string.Empty;
    [ObservableProperty]
    private BitmapSource? _horizontalBlurImageSource;
    [ObservableProperty]
    private string _horizontalImageTooltip = string.Empty;
    [ObservableProperty]
    private BitmapSource? _verticalBlurImageSource;
    [ObservableProperty]
    private string _verticalImageTooltip = string.Empty;
    [ObservableProperty]
    private BitmapSource? _diagonalBlurImageSource;
    [ObservableProperty]
    private string _diagonalImageTooltip = string.Empty;

    [ObservableProperty]
    private ConvolutionParamsViewModel _convolutionParams = new()
    {
        SigmaVisibility = Visibility.Collapsed,
        OrderVisibility = Visibility.Collapsed,
        KernelSizeVisibility = Visibility.Collapsed,
        ScaleVisibility = Visibility.Collapsed,
    };
    private void ConvolutionParams_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        GenerateCustomBlurImage();
    }
    #endregion

    #region Private functions
    private void GenerateCustomBlurImage()
    {
        if (_rawImage == null)
            return;

        _customBlurImage = new();
        try
        {
            Mat kernel = Mat.Ones(ConvolutionParams.KSize, MatType.CV_32F) / (float)(ConvolutionParams.KernelWidth * ConvolutionParams.KernelHeight);
            Cv2.Filter2D(_rawImage, _customBlurImage, ConvolutionParams.DDepth, kernel, ConvolutionParams.Anchor, ConvolutionParams.Delta, ConvolutionParams.BorderType);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "CustomBlur Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        CustomBlurImageSource = _customBlurImage.ToBitmapSource();

        _horizontalBlurImage = new();
        try
        {
            Mat kernel = Mat.Ones(HorizontalKSize, MatType.CV_32F) / (float)(ConvolutionParams.KernelWidth * 1);
            Cv2.Filter2D(_rawImage, _horizontalBlurImage, ConvolutionParams.DDepth, kernel, ConvolutionParams.Anchor, ConvolutionParams.Delta, ConvolutionParams.BorderType);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "HorizontalBlur Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        HorizontalBlurImageSource = _horizontalBlurImage.ToBitmapSource();

        _verticalBlurImage = new();
        try
        {
            Mat kernel = Mat.Ones(VerticalKSize, MatType.CV_32F) / (float)(1 * ConvolutionParams.KernelHeight);
            Cv2.Filter2D(_rawImage, _verticalBlurImage, ConvolutionParams.DDepth, kernel, ConvolutionParams.Anchor, ConvolutionParams.Delta, ConvolutionParams.BorderType);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "VerticalBlur Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        VerticalBlurImageSource = _verticalBlurImage.ToBitmapSource();

        _diagonalBlurImage = new();
        try
        {
            Mat kernel = Mat.Eye(DiagonalKSize, MatType.CV_32F) / (float)(ConvolutionParams.KernelWidth * ConvolutionParams.KernelHeight);
            Cv2.Filter2D(_rawImage, _diagonalBlurImage, ConvolutionParams.DDepth, kernel, ConvolutionParams.Anchor, ConvolutionParams.Delta, ConvolutionParams.BorderType);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "DiagonalBlur Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        DiagonalBlurImageSource = _diagonalBlurImage.ToBitmapSource();

        SetTooltip();
    }

    private void SetTooltip()
    {
        var sharpness = Cv2Helper.CalcSharpness(_customBlurImage);
        CustomImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
        sharpness = Cv2Helper.CalcSharpness(_horizontalBlurImage);
        HorizontalImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
        sharpness = Cv2Helper.CalcSharpness(_verticalBlurImage);
        VerticalImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
        sharpness = Cv2Helper.CalcSharpness(_diagonalBlurImage);
        DiagonalImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
    }
    #endregion
}

public partial class Sobel : ObservableObject
{
    #region Constructors
    public Sobel(Mat? rawImage)
    {
        _rawImage = rawImage;
        ConvolutionParams.PropertyChanged += ConvolutionParams_PropertyChanged;
        GenerateSobelImage();
        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            SetTooltip();
        });
    }
    #endregion

    #region Properties
    private Mat? _rawImage, _xSobelImage, _ySobelImage;
    #endregion

    #region Observable properties
    [ObservableProperty]
    private BitmapSource? _xGradImageSource, _yGradImageSource;
    [ObservableProperty]
    private string _xImageTooltip = string.Empty, _yImageTooltip = string.Empty;

    [ObservableProperty]
    private ConvolutionParamsViewModel _convolutionParams = new()
    {
        KernelVisibility = Visibility.Collapsed,
        AnchorVisibility = Visibility.Collapsed,
        SigmaVisibility = Visibility.Collapsed,
        OrderVisibility = Visibility.Collapsed,
        MustBeOdd = true
    };
    private void ConvolutionParams_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        GenerateSobelImage();
    }
    #endregion

    #region Private functions
    private void GenerateSobelImage()
    {
        if (_rawImage == null)
            return;

        _xSobelImage = new();
        _ySobelImage = new();
        try
        {
            Cv2.Sobel(_rawImage, _xSobelImage, MatType.CV_32F, 1, 0, ConvolutionParams.KernelSize,
                ConvolutionParams.Scale, ConvolutionParams.Delta, ConvolutionParams.BorderType);
            Cv2.Normalize(_xSobelImage, _xSobelImage, 0, 1.0, NormTypes.MinMax);
            _xSobelImage.ConvertTo(_xSobelImage, MatType.CV_8UC3, 255.0);
            XGradImageSource = _xSobelImage.ToBitmapSource();

            Cv2.Sobel(_rawImage, _ySobelImage, MatType.CV_32F, 0, 1, ConvolutionParams.KernelSize,
                ConvolutionParams.Scale, ConvolutionParams.Delta, ConvolutionParams.BorderType);
            Cv2.Normalize(_ySobelImage, _ySobelImage, 0, 1.0, NormTypes.MinMax);
            _ySobelImage.ConvertTo(_ySobelImage, MatType.CV_8UC3, 255.0);
            YGradImageSource = _ySobelImage.ToBitmapSource();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Sobel Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        SetTooltip();
    }

    private void SetTooltip()
    {
        var sharpness = Cv2Helper.CalcSharpness(_xSobelImage);
        XImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
        sharpness = Cv2Helper.CalcSharpness(_ySobelImage);
        YImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
    }
    #endregion
}
public partial class Scharr : ObservableObject
{
    #region Constructors
    public Scharr(Mat? rawImage)
    {
        _rawImage = rawImage;
        ConvolutionParams.PropertyChanged += ConvolutionParams_PropertyChanged;
        GenerateScharrImage();
        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            SetTooltip();
        });
    }
    #endregion

    #region Properties
    private Mat? _rawImage, _xScharrImage, _yScharrImage;
    #endregion

    #region Observable properties
    [ObservableProperty]
    private BitmapSource? _xGradImageSource, _yGradImageSource;
    [ObservableProperty]
    private string _xImageTooltip = string.Empty, _yImageTooltip = string.Empty;

    [ObservableProperty]
    private ConvolutionParamsViewModel _convolutionParams = new()
    {
        KernelVisibility = Visibility.Collapsed,
        AnchorVisibility = Visibility.Collapsed,
        SigmaVisibility = Visibility.Collapsed,
        OrderVisibility = Visibility.Collapsed,
        KernelSizeVisibility = Visibility.Collapsed
    };
    private void ConvolutionParams_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        GenerateScharrImage();
    }
    #endregion

    #region Private functions
    private void GenerateScharrImage()
    {
        if (_rawImage == null)
            return;

        _xScharrImage = new();
        _yScharrImage = new();
        try
        {
            Cv2.Scharr(_rawImage, _xScharrImage, MatType.CV_32F, 1, 0, ConvolutionParams.Scale, ConvolutionParams.Delta, ConvolutionParams.BorderType);
            Cv2.Normalize(_xScharrImage, _xScharrImage, 0, 1.0, NormTypes.MinMax);
            _xScharrImage.ConvertTo(_xScharrImage, MatType.CV_8UC3, 255.0);
            XGradImageSource = _xScharrImage.ToBitmapSource();

            Cv2.Scharr(_rawImage, _yScharrImage, MatType.CV_32F, 0, 1, ConvolutionParams.Scale, ConvolutionParams.Delta, ConvolutionParams.BorderType);
            Cv2.Normalize(_yScharrImage, _yScharrImage, 0, 1.0, NormTypes.MinMax);
            _yScharrImage.ConvertTo(_yScharrImage, MatType.CV_8UC3, 255.0);
            YGradImageSource = _yScharrImage.ToBitmapSource();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Sobel Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        SetTooltip();
    }

    private void SetTooltip()
    {
        var sharpness = Cv2Helper.CalcSharpness(_xScharrImage);
        XImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
        sharpness = Cv2Helper.CalcSharpness(_yScharrImage);
        YImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
    }
    #endregion
}
public partial class Canny : ObservableObject
{
    #region Constructors
    public Canny(Mat? rawImage)
    {
        _rawImage = rawImage;
        GenerateScharrImage();
        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            SetTooltip();
        });
    }
    #endregion

    #region Properties
    private Mat? _rawImage, _cannyImage, _coloredImage;
    #endregion

    #region Observable properties
    [ObservableProperty]
    private BitmapSource? _edgeImageSource, _colorEdgeImageSource;
    [ObservableProperty]
    private string _edgeImageTooltip = string.Empty, _colorEdgeImageTooltip = string.Empty;
    [ObservableProperty]
    private double _lowerThreshold = 150;
    partial void OnLowerThresholdChanged(double oldValue, double newValue)
    {
        GenerateScharrImage();
    }
    [ObservableProperty]
    private double _upperThreshold = 300;
    partial void OnUpperThresholdChanged(double oldValue, double newValue)
    {
        GenerateScharrImage();
    }
    [ObservableProperty]
    private int _apertureSize = 3;
    partial void OnApertureSizeChanged(int oldValue, int newValue)
    {
        if (newValue % 2 == 0)
        {
            if (oldValue < newValue)
                ApertureSize++;
            else
                ApertureSize--;
        }
        GenerateScharrImage();
    }
    [ObservableProperty]
    private bool _l2Gradient = false;
    partial void OnL2GradientChanged(bool oldValue, bool newValue)
    {
        GenerateScharrImage();
    }
    #endregion

    #region Private functions
    private void GenerateScharrImage()
    {
        if (_rawImage == null)
            return;

        _cannyImage = new();
        _coloredImage = new();
        try
        {
            Cv2.Canny(_rawImage, _cannyImage, LowerThreshold, UpperThreshold, ApertureSize, L2Gradient);
            EdgeImageSource = _cannyImage.ToBitmapSource();

            Cv2.BitwiseAnd(_rawImage, _rawImage, _coloredImage, _cannyImage);
            ColorEdgeImageSource = _coloredImage.ToBitmapSource();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Sobel Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        SetTooltip();
    }

    private void SetTooltip()
    {
        var sharpness = Cv2Helper.CalcSharpness(_cannyImage);
        EdgeImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
        sharpness = Cv2Helper.CalcSharpness(_coloredImage);
        ColorEdgeImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
    }
    #endregion
}
public partial class BilateralFilter : ObservableObject
{
    #region Constructors
    public BilateralFilter(Mat? rawImage)
    {
        _rawImage = rawImage;
        ConvolutionParams.PropertyChanged += ConvolutionParams_PropertyChanged;
        GenerateBilateralImage();
        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            SetTooltip();
        });
    }
    #endregion

    #region Properties
    private Mat? _rawImage, _filterredImage;
    #endregion

    #region Observable properties
    [ObservableProperty]
    private BitmapSource? _bilateralImageSource;
    [ObservableProperty]
    private string _imageTooltip = string.Empty;
    [ObservableProperty]
    private int _diameter = 0;
    partial void OnDiameterChanged(int oldValue, int newValue)
    {
        GenerateBilateralImage();
    }
    [ObservableProperty]
    private double _sigmaColor = 75;
    partial void OnSigmaColorChanged(double oldValue, double newValue)
    {
        GenerateBilateralImage();
    }
    [ObservableProperty]
    private double _sigmaSpace = 75;
    partial void OnSigmaSpaceChanged(double oldValue, double newValue)
    {
        GenerateBilateralImage();
    }

    [ObservableProperty]
    private ConvolutionParamsViewModel _convolutionParams = new()
    {
        KernelVisibility = Visibility.Collapsed,
        AnchorVisibility = Visibility.Collapsed,
        SigmaVisibility = Visibility.Collapsed,
        OrderVisibility = Visibility.Collapsed,
        KernelSizeVisibility = Visibility.Collapsed,
        DeltaVisibility = Visibility.Collapsed,
        ScaleVisibility = Visibility.Collapsed
    };
    private void ConvolutionParams_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        GenerateBilateralImage();
    }

    [ObservableProperty]
    private Visibility _loadingVisibility = Visibility.Hidden;
    #endregion

    #region Private functions
    private async Task GenerateBilateralImage()
    {
        if (_rawImage == null)
            return;

        try
        {
            LoadingVisibility = Visibility.Visible;
            _filterredImage = await Task<Mat>.Run(() =>
            {
                Mat bilateralImage = new();
                Cv2.BilateralFilter(_rawImage, bilateralImage, Diameter, SigmaColor, SigmaSpace, ConvolutionParams.BorderType);
                return bilateralImage;
            });
            BilateralImageSource = _filterredImage?.ToBitmapSource();
            SetTooltip();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Bilateral Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            LoadingVisibility = Visibility.Hidden;
        }
    }

    private void SetTooltip()
    {
        var sharpness = Cv2Helper.CalcSharpness(_filterredImage);
        ImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
    }
    #endregion
}
public partial class Laplacian : ObservableObject
{
    #region Constructors
    public Laplacian(Mat? rawImage)
    {
        _rawImage = rawImage;
        ConvolutionParams.PropertyChanged += ConvolutionParams_PropertyChanged;
        GenerateLaplacianImage();
        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            SetTooltip();
        });
    }
    #endregion

    #region Properties
    private Mat? _rawImage, _laplacianImage, _sharpenedImage;
    #endregion

    #region Observable properties
    [ObservableProperty]
    private BitmapSource? _laplacianImageSource, _sharpenedLaplacianImageSource;
    [ObservableProperty]
    private string _laplacianImageTooltip = string.Empty, _sharpenedImageTooltip = string.Empty;

    [ObservableProperty]
    private ConvolutionParamsViewModel _convolutionParams = new()
    {
        KernelVisibility = Visibility.Collapsed,
        SigmaVisibility = Visibility.Collapsed,
        OrderVisibility = Visibility.Collapsed,
        MustBeOdd = true
    };
    private void ConvolutionParams_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        GenerateLaplacianImage();
    }
    #endregion

    #region Private functions
    private void GenerateLaplacianImage()
    {
        if (_rawImage == null)
            return;

        _laplacianImage = new();
        _sharpenedImage = new();
        try
        {
            Cv2.Laplacian(_rawImage, _laplacianImage, MatType.CV_32F, ConvolutionParams.KernelSize, ConvolutionParams.Scale, ConvolutionParams.Delta, ConvolutionParams.BorderType);
            Cv2.Normalize(_laplacianImage, _laplacianImage, 0, 1.0, NormTypes.MinMax);
            _laplacianImage.ConvertTo(_laplacianImage, MatType.CV_8UC3, 255.0);

            float[,] data = new float[,]
            {
                { 0, -1, 0 },
                { -1, 5, -1 },
                { 0, -1, 0 }
            };
            Mat kernel = Mat.FromArray(data);
            Cv2.Filter2D(_rawImage, _sharpenedImage, -1, kernel, ConvolutionParams.Anchor, ConvolutionParams.Delta, ConvolutionParams.BorderType);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Laplacian Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        LaplacianImageSource = _laplacianImage.ToBitmapSource();
        SharpenedLaplacianImageSource = _sharpenedImage.ToBitmapSource();
        SetTooltip();
    }

    private void SetTooltip()
    {
        var sharpness = Cv2Helper.CalcSharpness(_laplacianImage);
        LaplacianImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
        sharpness = Cv2Helper.CalcSharpness(_sharpenedImage);
        SharpenedImageTooltip = String.Format(App.GetStringResource("wnd-convolution-sharpness"), sharpness);
    }
    #endregion
}