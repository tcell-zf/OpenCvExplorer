using OpenCvExplorer.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace OpenCvExplorer.Views.Windows;

public partial class ConvolutionWindow : FluentWindow
{
    #region Properties
    public ConvolutionWindowViewModel ViewModel { get; }
    #endregion

    #region Constructors
    public ConvolutionWindow(ConvolutionWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
    #endregion
}
