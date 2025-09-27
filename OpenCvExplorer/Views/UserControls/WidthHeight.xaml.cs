using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace OpenCvExplorer.Views.UserControls;

public partial class WidthHeight : UserControl
{
    #region Properties
    private bool _isLinked = false;

    public static readonly DependencyProperty WProperty = DependencyProperty.Register(nameof(W), typeof(int), typeof(WidthHeight),
        new FrameworkPropertyMetadata(0, new PropertyChangedCallback(WPropertyChangedCallback)));
    private static void WPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
    {
        var ctrl = sender as WidthHeight;
        if (ctrl != null)
            ctrl.SetW();
    }
    public int W
    {
        get { return (int)GetValue(WProperty); }
        set { SetValue(WProperty, value); }
    }

    public static readonly DependencyProperty HProperty = DependencyProperty.Register(nameof(H), typeof(int), typeof(WidthHeight),
        new FrameworkPropertyMetadata(0, new PropertyChangedCallback(HPropertyChangedCallback)));
    private static void HPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
    {
    }
    public int H
    {
        get { return (int)GetValue(HProperty); }
        set { SetValue(HProperty, value); }
    }

    public static readonly DependencyProperty MinimumWidthProperty = DependencyProperty.Register(nameof(MinimumWidth), typeof(int), typeof(WidthHeight),
        new FrameworkPropertyMetadata(0, new PropertyChangedCallback(MinimumWidthPropertyChangedCallback)));
    private static void MinimumWidthPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
    {
        var ctrl = sender as WidthHeight;
        if (ctrl != null)
            ctrl.SetMinimumWidth();
    }
    public int MinimumWidth
    {
        get { return (int)GetValue(MinimumWidthProperty); }
        set { SetValue(MinimumWidthProperty, value); }
    }

    public static readonly DependencyProperty MaximumWidthProperty = DependencyProperty.Register(nameof(MaximumWidth), typeof(int), typeof(WidthHeight),
        new FrameworkPropertyMetadata(0, new PropertyChangedCallback(MaximumWidthPropertyChangedCallback)));
    private static void MaximumWidthPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
    {
        var ctrl = sender as WidthHeight;
        if (ctrl != null)
            ctrl.SetMaximumWidth();
    }
    public int MaximumWidth
    {
        get { return (int)GetValue(MaximumWidthProperty); }
        set { SetValue(MaximumWidthProperty, value); }
    }

    public static readonly DependencyProperty MinimumHeightProperty = DependencyProperty.Register(nameof(MinimumHeight), typeof(int), typeof(WidthHeight),
        new FrameworkPropertyMetadata(0, new PropertyChangedCallback(MinimumHeightPropertyChangedCallback)));
    private static void MinimumHeightPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
    {
        var ctrl = sender as WidthHeight;
        if (ctrl != null)
            ctrl.SetMinimumHeight();
    }
    public int MinimumHeight
    {
        get { return (int)GetValue(MinimumHeightProperty); }
        set { SetValue(MinimumHeightProperty, value); }
    }

    public static readonly DependencyProperty MaximumHeightProperty = DependencyProperty.Register(nameof(MaximumHeight), typeof(int), typeof(WidthHeight),
        new FrameworkPropertyMetadata(0, new PropertyChangedCallback(MaximumHeightPropertyChangedCallback)));
    private static void MaximumHeightPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
    {
        var ctrl = sender as WidthHeight;
        if (ctrl != null)
            ctrl.SetMaximumHeight();
    }
    public int MaximumHeight
    {
        get { return (int)GetValue(MaximumHeightProperty); }
        set { SetValue(MaximumHeightProperty, value); }
    }

    public static readonly DependencyProperty WidthContentProperty = DependencyProperty.Register(nameof(WidthContent), typeof(string), typeof(WidthHeight),
        new FrameworkPropertyMetadata("", new PropertyChangedCallback(WidthContentPropertyChangedCallback)));
    private static void WidthContentPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
    {
        var ctrl = sender as WidthHeight;
        if (ctrl != null)
            ctrl.SetWidthContent();
    }
    public string WidthContent
    {
        get { return (string)GetValue(WidthContentProperty); }
        set { SetValue(WidthContentProperty, value); }
    }

    public static readonly DependencyProperty HeightContentProperty = DependencyProperty.Register(nameof(HeightContent), typeof(string), typeof(WidthHeight),
        new FrameworkPropertyMetadata("", new PropertyChangedCallback(HeightContentPropertyChangedCallback)));
    private static void HeightContentPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
    {
        var ctrl = sender as WidthHeight;
        if (ctrl != null)
            ctrl.SetHeightContent();
    }
    public string HeightContent
    {
        get { return (string)GetValue(HeightContentProperty); }
        set { SetValue(HeightContentProperty, value); }
    }
    #endregion

    #region Constructors
    public WidthHeight()
    {
        InitializeComponent();
    }
    #endregion

    #region Private functions
    private void SetW()
    {
        if (_isLinked)
            H = W;
    }

    private void SetMinimumWidth()
    {
        WidthBox.Minimum = MinimumWidth;
    }
    private void SetMaximumWidth()
    {
        WidthBox.Maximum = MaximumWidth;
    }

    private void SetMinimumHeight()
    {
        HeightBox.Minimum = MinimumHeight;
    }
    private void SetMaximumHeight()
    {
        HeightBox.Maximum = MaximumHeight;
    }

    private void SetWidthContent()
    {
        WidthLabel.Content = WidthContent;
    }
    private void SetHeightContent()
    {
        HeightLabel.Content = HeightContent;
    }
    #endregion

    #region Events
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender == null)
            return;
        Wpf.Ui.Controls.Button? btn = sender as Wpf.Ui.Controls.Button;
        if (btn == null)
            return;

        if (_isLinked)
        {
            _isLinked = false;
            btn.Icon = new SymbolIcon(SymbolRegular.Link16);
            btn.ToolTip = App.GetStringResource("uc-widthheight-link-tooltip");
            HeightBox.IsEnabled = true;
        }
        else
        {
            _isLinked = true;
            btn.Icon = new SymbolIcon(SymbolRegular.LinkDismiss16);
            btn.ToolTip = App.GetStringResource("uc-widthheight-unlink-tooltip");
            H = W;
            HeightBox.IsEnabled = false;
        }
    }
    #endregion
}
