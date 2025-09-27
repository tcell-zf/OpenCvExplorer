using OpenCvSharp;

namespace OpenCvExplorer.ViewModels.UserControls;

public partial class SaveImageOptionsViewModel : BaseObservableObject
{
    public ImageEncodingParam[]? ImageEncodingParams
    {
        get
        {
            ImageEncodingParam[]? imageEncodingParams = null;
            switch (SaveImageType)
            {
                case ImageType.Jpeg:
                    if (JpegParameters != null)
                    {
                        imageEncodingParams = new[]
                        {
                            new ImageEncodingParam(ImwriteFlags.JpegQuality, JpegParameters.Quality),
                            new ImageEncodingParam(ImwriteFlags.JpegProgressive, JpegParameters.Progressive ? 1 : 0),
                            new ImageEncodingParam(ImwriteFlags.JpegOptimize, JpegParameters.Optimize ? 1 : 0),
                            new ImageEncodingParam(ImwriteFlags.JpegRstInterval, JpegParameters.RstInterval),
                            new ImageEncodingParam(ImwriteFlags.JpegLumaQuality, JpegParameters.LumaQuality),
                            new ImageEncodingParam(ImwriteFlags.JpegChromaQuality, JpegParameters.ChromaQuality)
                        };
                    }
                    break;
                case ImageType.Png:
                    if (PngParameters != null)
                    {
                        imageEncodingParams = new[]
                        {
                            new ImageEncodingParam(ImwriteFlags.PngCompression, PngParameters.Compression),
                            new ImageEncodingParam(ImwriteFlags.PngStrategy, (int)PngParameters.Strategy),
                            new ImageEncodingParam(ImwriteFlags.PngBilevel, PngParameters.Bilevel ? 1 : 0)
                        };
                    }
                    break;
                case ImageType.Webp:
                    if (WebpParameters != null)
                    {
                        imageEncodingParams = new[]
                        {
                            new ImageEncodingParam(ImwriteFlags.WebPQuality, WebpParameters.Quality)
                        };
                    }
                    break;
                case ImageType.Tiff:
                case ImageType.Bmp:
                default:
                    break;
            }
            return imageEncodingParams;
        }
    }

    [ObservableProperty]
    private ImageType _saveImageType = ImageType.Other;
    partial void OnSaveImageTypeChanged(ImageType oldValue, ImageType newValue)
    {
        IsBmpSelected = newValue == ImageType.Bmp;
    }
    [ObservableProperty]
    private JpegParametersViewModel _jpegParameters = new JpegParametersViewModel();
    [ObservableProperty]
    private PngParametersViewModel _pngParameters = new PngParametersViewModel();
    [ObservableProperty]
    private WebpParametersViewModel _webpParameters = new WebpParametersViewModel();
    [ObservableProperty]
    private bool _isBmpSelected = false;

    [RelayCommand]
    private void OnSelectImageType(string imageTypeString)
    {
        switch (imageTypeString.ToLower())
        {
            case "bmp":
                SaveImageType = ImageType.Bmp;
                break;
            default:
                break;
        }
    }
}

public partial class JpegParametersViewModel : ObservableObject
{
    [ObservableProperty]
    private int _quality = 95;
    [ObservableProperty]
    private bool _progressive = false;
    [ObservableProperty]
    private bool _optimize = false;
    [ObservableProperty]
    private int _rstInterval = 65535;
    [ObservableProperty]
    private int _lumaQuality = 100;
    [ObservableProperty]
    private int _chromaQuality = 100;
}

public partial class PngParametersViewModel : ObservableObject
{
    [ObservableProperty]
    private int _compression = 3;
    [ObservableProperty]
    private ImwritePNGFlags _strategy = 0;
    partial void OnStrategyChanged(ImwritePNGFlags oldValue, ImwritePNGFlags newValue)
    {
        switch (newValue)
        {
            case ImwritePNGFlags.StrategyDefault:
                StrategyContent = "Default";
                break;
            case ImwritePNGFlags.StrategyFiltered:
                StrategyContent = "Filtered";
                break;
            case ImwritePNGFlags.StrategyHuffmanOnly:
                StrategyContent = "HuffmanOnly";
                break;
            case ImwritePNGFlags.StrategyRLE:
                StrategyContent = "RLE";
                break;
            case ImwritePNGFlags.StrategyFixed:
                StrategyContent = "Fixed";
                break;
            default:
                StrategyContent = "Default";
                break;
        }
    }
    [ObservableProperty]
    private bool _bilevel = false;
    [ObservableProperty]
    private string _strategyContent = "Default";

    [RelayCommand]
    private void OnSetStrategyDefault()
    {
        Strategy = ImwritePNGFlags.StrategyDefault;
    }
    [RelayCommand]
    private void OnSetStrategyFiltered()
    {
        Strategy = ImwritePNGFlags.StrategyFiltered;
    }
    [RelayCommand]
    private void OnSetStrategyHuffmanOnly()
    {
        Strategy = ImwritePNGFlags.StrategyHuffmanOnly;
    }
    [RelayCommand]
    private void OnSetStrategyRLE()
    {
        Strategy = ImwritePNGFlags.StrategyRLE;
    }
    [RelayCommand]
    private void OnSetStrategyFixed()
    {
        Strategy = ImwritePNGFlags.StrategyFixed;
    }
}

public partial class WebpParametersViewModel : ObservableObject
{
    [ObservableProperty]
    private int _quality = 1;
}
