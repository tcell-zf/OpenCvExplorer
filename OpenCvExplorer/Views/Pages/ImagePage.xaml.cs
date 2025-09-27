using OpenCvExplorer.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace OpenCvExplorer.Views.Pages
{
    public partial class ImagePage : INavigableView<ImageViewModel>
    {
        public ImageViewModel ViewModel { get; set; }

        public ImagePage(ImageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
