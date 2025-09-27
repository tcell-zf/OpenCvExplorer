using OpenCvExplorer.ViewModels.Pages;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.IO;
using Wpf.Ui.Controls;

namespace OpenCvExplorer.Views.Pages
{
    public partial class VideoPage : INavigableView<VideoViewModel>
    {
        private CancellationTokenSource? cancellationTokenSource;
        private object videoLock = new object();
        private bool isVideoPaused = false;
        private VideoCapture? videoCapture;
        public VideoViewModel ViewModel { get; }

        public VideoPage(VideoViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            ViewModel.VideoStreamInfo = new()
            {
                CameraFrameWidth = 640
            };

            InitializeComponent();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            StopStreaming();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null || btn.Tag == null)
                return;

            switch (btn.Tag.ToString())
            {
                case "Play":
                    var videoSource = GetVideoSource();
                    if (videoSource == null)
                        return;

                    if (videoCapture != null)
                    {
                        StopStreaming();
                        return;
                    }

                    if (videoSource.Value.source is int)
                    {
                        ViewModel.CurrentVideoSourceType = VideoSourceEnum.Camera;
                        await Task.Run(() =>
                        {
                            ViewModel.LoadingVisibility = Visibility.Visible;
                            videoCapture = new VideoCapture((int)videoSource.Value.source, videoSource.Value.apiPreference);
                            videoCapture.Set(VideoCaptureProperties.FrameWidth, ViewModel.VideoStreamInfo.CameraFrameWidth.Value);
                            ViewModel.LoadingVisibility = Visibility.Hidden;
                        });
                    }
                    else if (videoSource.Value.source is string)
                    {
                        await Task.Run(() =>
                        {
                            ViewModel.LoadingVisibility = Visibility.Visible;
                            videoCapture = new VideoCapture((string)videoSource.Value.source, videoSource.Value.apiPreference);
                            ViewModel.LoadingVisibility = Visibility.Hidden;
                        });
                        if (File.Exists((string)videoSource.Value.source))
                            ViewModel.CurrentVideoSourceType = VideoSourceEnum.File;
                        else
                            ViewModel.CurrentVideoSourceType = VideoSourceEnum.Link;
                    }
                    else
                    {
                        ViewModel.CurrentVideoSourceType = VideoSourceEnum.None;
                        return;
                    }

                    StartStreaming();
                    await Task.Delay(50); // wait for VideoCapture to start properly especially for file source
                    ViewModel.VideoCaptureInfo = new()
                    {
                        FrameHeight = videoCapture.Get(VideoCaptureProperties.FrameHeight),
                        FrameWidth = videoCapture.Get(VideoCaptureProperties.FrameWidth),
                    };
                    switch (ViewModel.CurrentVideoSourceType)
                    {
                        case VideoSourceEnum.File:
                            ViewModel.VideoFileInfo = new()
                            {
                                FrameCount = videoCapture.Get(VideoCaptureProperties.FrameCount),
                                FourCC = videoCapture.Get(VideoCaptureProperties.FourCC)
                            };
                            ViewModel.NormalFrameInterval = ViewModel.FrameInterval = 33;
                            ViewModel.VideoFileInfo.FourCCCodec = ((char)((int)ViewModel.VideoFileInfo.FourCC & 0xFF)).ToString()
                                + ((char)(((int)ViewModel.VideoFileInfo.FourCC >> 8) & 0xFF))
                                + ((char)(((int)ViewModel.VideoFileInfo.FourCC >> 16) & 0xFF))
                                + ((char)(((int)ViewModel.VideoFileInfo.FourCC >> 24) & 0xFF));
                            ViewModel.VideoFileInfo.FourCCCodec = ViewModel.VideoFileInfo.FourCCCodec.ToUpper();

                            if (ViewModel?.VideoStreamInfo != null)
                            {
                                ViewModel.VideoStreamInfo.Fps = null;
                                ViewModel.VideoStreamInfo.Brightness = null;
                                ViewModel.VideoStreamInfo.Contrast = null;
                                ViewModel.VideoStreamInfo.Saturation = null;
                                ViewModel.VideoStreamInfo.Hue = null;
                                ViewModel.VideoStreamInfo.Gain = null;
                                ViewModel.VideoStreamInfo.Exposure = null;
                                ViewModel.NormalFrameInterval = 33;
                            }
                            break;
                        case VideoSourceEnum.Camera:
                        case VideoSourceEnum.Link:
                            if (ViewModel?.VideoStreamInfo != null)
                            {
                                ViewModel.VideoStreamInfo.Fps = videoCapture.Get(VideoCaptureProperties.Fps);
                                ViewModel.VideoStreamInfo.Brightness = videoCapture.Get(VideoCaptureProperties.Brightness);
                                ViewModel.VideoStreamInfo.Contrast = videoCapture.Get(VideoCaptureProperties.Contrast);
                                ViewModel.VideoStreamInfo.Saturation = videoCapture.Get(VideoCaptureProperties.Saturation);
                                ViewModel.VideoStreamInfo.Hue = videoCapture.Get(VideoCaptureProperties.Hue);
                                ViewModel.VideoStreamInfo.Gain = videoCapture.Get(VideoCaptureProperties.Gain);
                                ViewModel.VideoStreamInfo.Exposure = videoCapture.Get(VideoCaptureProperties.Exposure);
                                ViewModel.VideoStreamInfo.Focus = videoCapture.Get(VideoCaptureProperties.Focus);
                                ViewModel.NormalFrameInterval = ViewModel.FrameInterval = (int)(1000 / (ViewModel.VideoStreamInfo.Fps > 0 ? ViewModel.VideoStreamInfo.Fps : 33));
                            }
                            ViewModel.VideoFileInfo = null;
                            break;
                        default: break;
                    }

                    ViewModel.IsAbleToPlay = false;
                    ViewModel.IsAbleToStop = true;
                    break;
                case "Stop":
                    StopStreaming();
                    ViewModel.CurrentVideoSourceType = VideoSourceEnum.None;
                    break;
                case "Clock":
                    ViewModel.Angel = (ViewModel.Angel + 90) % 360;
                    break;
                case "CClock":
                    ViewModel.Angel = (ViewModel.Angel - 90) % 360;
                    break;
                default: break;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Abs(e.NewValue - e.OldValue) > 10)
            {
                lock (videoLock)
                {
                    isVideoPaused = true;
                }
                videoCapture?.Set(VideoCaptureProperties.PosFrames, e.NewValue);
                if (ViewModel.VideoFileInfo != null)
                    ViewModel.VideoFileInfo.PosFrames = e.NewValue;
                lock (videoLock)
                {
                    isVideoPaused = false;
                }
            }
        }

        private VideoSourceType? GetVideoSource()
        {
            VideoSourceType? videoSource = null;
            if (ViewModel.AllCameras != null && ViewModel.AllCameras.Count > 0)
            {
                foreach (var cam in ViewModel.AllCameras)
                {
                    if (cam.IsChecked)
                    {
                        videoSource = new VideoSourceType()
                        {
                            apiPreference = ViewModel.ApiPreference,
                            source = cam.Index
                        };
                        break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(ViewModel.FileName))
            {
                videoSource = new VideoSourceType()
                {
                    apiPreference = ViewModel.ApiPreference,
                    source = ViewModel.FileName
                };
            }
            else if (!string.IsNullOrEmpty(ViewModel.Link))
            {
                videoSource = new VideoSourceType()
                {
                    apiPreference = ViewModel.ApiPreference,
                    source = ViewModel.Link
                };
            }
            else { }

            return videoSource;
        }

        private void StartStreaming()
        {
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                using (var frame = new Mat())
                {
                    while (!cancellationTokenSource.IsCancellationRequested
                        && videoCapture != null
                        && videoCapture.IsOpened())
                    {
                        lock (videoLock)
                        {
                            if (isVideoPaused)
                            {
                                Task.Delay(50).Wait();
                                continue;
                            }
                        }

                        if (videoCapture.Read(frame) && !frame.Empty())
                        {
                            ViewModel.CurrentImageMat = frame.Clone();

                            var lastFrameBitmapImage = frame.ToBitmapSource();
                            lastFrameBitmapImage.Freeze();

                            video.Dispatcher.Invoke(() => video.Source = lastFrameBitmapImage);
                            ViewModel.CenterX = video.ActualWidth / 2;
                            ViewModel.CenterY = video.ActualHeight / 2;
                            switch (ViewModel.CurrentVideoSourceType)
                            {
                                case VideoSourceEnum.File:
                                    if (ViewModel.VideoFileInfo != null)
                                    {
                                        ViewModel.VideoFileInfo.PosMsec = videoCapture.Get(VideoCaptureProperties.PosMsec);
                                        ViewModel.VideoFileInfo.PosFrames = videoCapture.Get(VideoCaptureProperties.PosFrames);
                                        ViewModel.VideoFileInfo.PosAviRatio = videoCapture.Get(VideoCaptureProperties.PosAviRatio);
                                    }
                                    break;
                                case VideoSourceEnum.Camera:
                                case VideoSourceEnum.Link:
                                    if (ViewModel.VideoStreamInfo != null)
                                    {
                                    }
                                    break;
                                default: break;
                            }

                            await Task.Delay(ViewModel.FrameInterval);
                        }
                        else { break; }
                    }

                    video.Dispatcher.Invoke(() => video.Source = null);
                    if (videoCapture?.IsDisposed == false)
                    {
                        videoCapture?.Dispose();
                        videoCapture = null;
                    }
                    ViewModel.IsAbleToPlay = true;
                    ViewModel.IsAbleToStop = false;
                    ViewModel.CurrentVideoSourceType = VideoSourceEnum.None;
                }
            }, cancellationTokenSource.Token);
        }

        private void StopStreaming()
        {
            cancellationTokenSource?.Cancel();
            video.Dispatcher.Invoke(() => video.Source = null);
        }
    }

    internal struct VideoSourceType
    {
        public VideoCaptureAPIs apiPreference;
        public object source;
    }

    public enum VideoSourceEnum
    {
        None, Camera, File, Link
    }
}
