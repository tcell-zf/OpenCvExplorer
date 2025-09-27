using OpenCvSharp;

namespace OpenCvExplorer.ViewModels.UserControls;

public partial class ConvolutionParamsViewModel : BaseObservableObject
{
    #region Properties
    public OpenCvSharp.Size KSize
    {
        get => new OpenCvSharp.Size(KernelWidth, KernelHeight);
    }
    public OpenCvSharp.Point Anchor
    {
        get => new OpenCvSharp.Point(AnchorX, AnchorY);
    }
    public int DDepth
    {
        get => -1; // same depth as source image MatType.CV_8U;
    }
    public bool MustBeOdd { get; set; }
    #endregion

    #region Observable properties
    #region Kernel
    [ObservableProperty]
    private int _kernelWidth = 3;
    partial void OnKernelWidthChanged(int oldValue, int newValue)
    {
        if (MustBeOdd && newValue % 2 == 0)
        {
            if (oldValue < newValue)
                KernelWidth++;
            else
                KernelWidth--;
        }
    }
    [ObservableProperty]
    private int _kernelHeight = 3;
    partial void OnKernelHeightChanged(int oldValue, int newValue)
    {
        if (MustBeOdd && newValue % 2 == 0)
        {
            if (oldValue < newValue)
                KernelHeight++;
            else
                KernelHeight--;
        }
    }
    [ObservableProperty]
    private Visibility _kernelVisibility = Visibility.Visible;
    #endregion

    #region Anchor
    [ObservableProperty]
    private int _anchorX = -1;
    partial void OnAnchorXChanged(int oldValue, int newValue)
    {
    }
    [ObservableProperty]
    private int _anchorY = -1;
    partial void OnAnchorYChanged(int oldValue, int newValue)
    {
    }
    [ObservableProperty]
    private Visibility _anchorVisibility = Visibility.Visible;
    #endregion

    #region Sigma
    [ObservableProperty]
    private double _sigmaX = 0;
    partial void OnSigmaXChanged(double oldValue, double newValue)
    {
    }
    [ObservableProperty]
    private double _sigmaY = 0;
    partial void OnSigmaYChanged(double oldValue, double newValue)
    {
    }
    [ObservableProperty]
    private Visibility _sigmaVisibility = Visibility.Visible;
    #endregion

    #region XOrder, YOrder
    [ObservableProperty]
    private int _xOrder = 0;
    partial void OnXOrderChanged(int oldValue, int newValue)
    {
    }
    [ObservableProperty]
    private int _yOrder = 0;
    partial void OnYOrderChanged(int oldValue, int newValue)
    {
    }
    [ObservableProperty]
    private Visibility _orderVisibility = Visibility.Visible;
    #endregion

    #region KSize
    [ObservableProperty]
    private int _kernelSize = 3;
    partial void OnKernelSizeChanged(int oldValue, int newValue)
    {
        if (MustBeOdd && newValue % 2 == 0)
        {
            if (oldValue < newValue)
                KernelSize++;
            else
                KernelSize--;
        }
    }
    [ObservableProperty]
    private Visibility _kernelSizeVisibility = Visibility.Visible;
    #endregion

    #region Delta
    [ObservableProperty]
    private double _delta = 0;
    partial void OnDeltaChanged(double oldValue, double newValue)
    {
    }
    [ObservableProperty]
    private Visibility _deltaVisibility = Visibility.Visible;
    #endregion

    #region Scale
    [ObservableProperty]
    private double _scale = 1;
    partial void OnScaleChanged(double oldValue, double newValue)
    {
    }
    [ObservableProperty]
    private Visibility _scaleVisibility = Visibility.Visible;
    #endregion

    #region Border type
    [ObservableProperty]
    private BorderTypes _borderType = BorderTypes.Default;
    partial void OnBorderTypeChanged(BorderTypes oldValue, BorderTypes newValue)
    {
    }
    [ObservableProperty]
    private Visibility _borderTypeVisibility = Visibility.Visible;
    #endregion
    #endregion

    #region Relay commands
    [RelayCommand]
    private void OnSetBorderTypeConstant()
    {
        BorderType = BorderTypes.Constant;
    }
    [RelayCommand]
    private void OnSetBorderTypeReplicate()
    {
        BorderType = BorderTypes.Replicate;
    }
    [RelayCommand]
    private void OnSetBorderTypeReflect()
    {
        BorderType = BorderTypes.Reflect;
    }
    [RelayCommand]
    private void OnSetBorderTypeWrap()
    {
        BorderType = BorderTypes.Wrap;
    }
    [RelayCommand]
    private void OnSetBorderTypeReflect101()
    {
        BorderType = BorderTypes.Reflect101;
    }
    [RelayCommand]
    private void OnSetBorderTypeDefault()
    {
        BorderType = BorderTypes.Default;
    }
    [RelayCommand]
    private void OnSetBorderTypeIsolated()
    {
        BorderType = BorderTypes.Isolated;
    }
    #endregion
}
