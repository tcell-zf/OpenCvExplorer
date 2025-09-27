using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using OpenCvExplorer.ViewModels.Messages;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace OpenCvExplorer.ViewModels.Pages
{
    public partial class ImageViewModel : BaseObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom() { }

        private void InitializeViewModel() { }

        public ImageViewModel()
        {
            WeakReferenceMessenger.Default.Register<ImageClosingMessage>(this, (r, m) =>
            {
                var filename = m.Value.Replace("\\\\", "\\");
                foreach (var f in FileNames)
                {
                    if (f.FileName.Equals(filename))
                    {
                        FileNames.Remove(f);
                        break;
                    }
                }
            });
        }

        [ObservableProperty]
        private ObservableCollection<ImageInfo> _fileNames = new();

        [RelayCommand]
        private void OnOpenImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = App.GetStringResource("dialog-select-image-title"),
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.webp, *.tiff, *.bmp)|*.jpg;*.jpeg;*.png;*.webp;*.tiff;*.bmp|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                if (!FileNames.Contains(new ImageInfo() { FileName = openFileDialog.FileName }))
                    FileNames.Add(new ImageInfo()
                    {
                        FileName = openFileDialog.FileName,
                        ImageEditorViewModel = new UserControls.ImageEditorViewModel()
                        {
                            FileName = openFileDialog.FileName
                        }
                    });
            }
        }
    }

    public class ImageInfo
    {
        public string? FileName { get; init; }
        public UserControls.ImageEditorViewModel? ImageEditorViewModel { get; init; }
    }
}
