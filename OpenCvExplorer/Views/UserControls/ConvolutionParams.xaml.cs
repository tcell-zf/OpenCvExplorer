using OpenCvExplorer.ViewModels.UserControls;
using System.Windows.Controls;

namespace OpenCvExplorer.Views.UserControls;

public partial class ConvolutionParams : UserControl
{
    private ConvolutionParamsViewModel? ViewModel
    {
        get
        {
            if (DataContext == null)
                return null;
            return DataContext as ConvolutionParamsViewModel;
        }
    }

    public static readonly DependencyProperty MustBeOddProperty = DependencyProperty.Register(nameof(MustBeOdd), typeof(bool), typeof(ConvolutionParams),
        new FrameworkPropertyMetadata(false, new PropertyChangedCallback(MustBeOddPropertyChangedCallback)));
    private static void MustBeOddPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
    {
        var ctrl = sender as ConvolutionParams;
        if (ctrl != null)
            ctrl.SetMustBeOdd();
    }
    public bool MustBeOdd
    {
        get { return (bool)this.GetValue(MustBeOddProperty); }
        set { this.SetValue(MustBeOddProperty, value); }
    }

    public ConvolutionParams()
    {
        InitializeComponent();
    }

    private void SetMustBeOdd()
    {
        if (ViewModel == null)
            return;

        ViewModel.MustBeOdd = MustBeOdd;
    }
}
