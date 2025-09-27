using OpenCvExplorer.ViewModels;
using OpenCvExplorer.ViewModels.UserControls;
using OpenCvSharp;
using System.Windows.Controls;

namespace OpenCvExplorer.Views.UserControls;

public partial class SaveImageOptions : UserControl
{
    #region properties
    private SaveImageOptionsViewModel? ViewModel
    {
        get
        {
            if (DataContext == null)
                return null;
            return DataContext as SaveImageOptionsViewModel;
        }
    }

    public ImageType SelectedImageType
    {
        get
        {
            if (ViewModel == null)
                return ImageType.Other;

            return ViewModel.SaveImageType;
        }
    }

    public ImageEncodingParam[]? ImageEncodingParams
    {
        get
        {
            if (ViewModel == null)
                return null;

            return ViewModel.ImageEncodingParams;
        }
    }
    #endregion

    #region constructors
    public SaveImageOptions()
    {
        InitializeComponent();
    }
    #endregion

    #region events
    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var tabControl = sender as TabControl;
        if (tabControl == null || ViewModel == null)
            return;

        switch (tabControl.SelectedIndex)
        {
            case 0:
                ViewModel.SaveImageType = ImageType.Jpeg;
                break;
            case 1:
                ViewModel.SaveImageType = ImageType.Png;
                break;
            case 2:
                ViewModel.SaveImageType = ImageType.Webp;
                break;
            case 3:
                ViewModel.SaveImageType = ImageType.Tiff;
                break;
            default:
                break;
        }
    }
    #endregion
}
