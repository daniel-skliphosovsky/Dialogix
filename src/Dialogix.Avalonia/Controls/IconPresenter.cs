using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using Path = Avalonia.Controls.Shapes.Path;

namespace Dialogix.Avalonia.Controls;

public class IconPresenter : Decorator
{
    public static readonly StyledProperty<object?> SourceProperty =
        AvaloniaProperty.Register<IconPresenter, object?>(nameof(Source));

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<IconPresenter, IBrush?>(nameof(Fill));

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<IconPresenter, IBrush?>(nameof(Stroke));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<IconPresenter, double>(nameof(StrokeThickness));

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<IconPresenter, Stretch>(nameof(Stretch), Stretch.Uniform);

    private Path? _path;
    private Image? _image;

    public object? Source { get => GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public IBrush? Fill { get => GetValue(FillProperty); set => SetValue(FillProperty, value); }
    public IBrush? Stroke { get => GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }
    public double StrokeThickness { get => GetValue(StrokeThicknessProperty); set => SetValue(StrokeThicknessProperty, value); }
    public Stretch Stretch { get => GetValue(StretchProperty); set => SetValue(StretchProperty, value); }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SourceProperty)
            UpdateChild();
        else if (change.Property == StretchProperty)
            UpdateStretch();
        else if (change.Property == FillProperty ||
                 change.Property == StrokeProperty ||
                 change.Property == StrokeThicknessProperty)
            UpdatePath();
    }

    private void UpdateChild()
    {
        _path = null;
        _image = null;

        switch (Source)
        {
            case Geometry geometry:
                _path = new Path
                {
                    Data = geometry,
                    Stretch = Stretch,
                    StrokeLineCap = PenLineCap.Round,
                    StrokeJoin = PenLineJoin.Round
                };
                UpdatePath();
                Child = _path;
                break;

            case IImage image:
                _image = new Image { Source = image, Stretch = Stretch };
                Child = _image;
                break;

            case Control control:
                Child = control;
                break;

            case { } other:
                Child = new ContentControl { Content = other };
                break;

            default:
                Child = null;
                break;
        }
    }

    private void UpdatePath()
    {
        if (_path is null)
            return;

        _path.Fill = Fill;
        _path.Stroke = Stroke;
        _path.StrokeThickness = StrokeThickness;
    }

    private void UpdateStretch()
    {
        if (_path is not null)
            _path.Stretch = Stretch;

        if (_image is not null)
            _image.Stretch = Stretch;
    }
}