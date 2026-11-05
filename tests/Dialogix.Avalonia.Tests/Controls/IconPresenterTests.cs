using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;

using Dialogix.Avalonia.Controls;

using Path = Avalonia.Controls.Shapes.Path;

namespace Dialogix.Avalonia.Tests.Controls;

public class IconPresenterTests
{
    [AvaloniaFact]
    public void Geometry_renders_as_a_filled_path()
    {
        Geometry geometry = Geometry.Parse("M0 0 L10 10");
        IconPresenter icon = new IconPresenter { Source = geometry, Fill = Brushes.Red, Stroke = Brushes.Blue, StrokeThickness = 3 };

        Path path = Assert.IsType<Path>(icon.Child);
        Assert.Same(geometry, path.Data);
        Assert.Same(Brushes.Red, path.Fill);
        Assert.Same(Brushes.Blue, path.Stroke);
        Assert.Equal(3, path.StrokeThickness);
    }

    [AvaloniaFact]
    public void Image_renders_as_an_image()
    {
        DrawingImage image = new DrawingImage(new GeometryDrawing { Brush = Brushes.Red });
        IconPresenter icon = new IconPresenter { Source = image };

        Image view = Assert.IsType<Image>(icon.Child);
        Assert.Same(image, view.Source);
    }

    [AvaloniaFact]
    public void Control_is_used_as_is()
    {
        Border control = new Border();
        IconPresenter icon = new IconPresenter { Source = control };

        Assert.Same(control, icon.Child);
    }

    [AvaloniaFact]
    public void Source_change_swaps_the_child()
    {
        IconPresenter icon = new IconPresenter { Source = Geometry.Parse("M0 0 L1 1") };
        Assert.IsType<Path>(icon.Child);

        icon.Source = new DrawingImage(new GeometryDrawing { Brush = Brushes.Red });
        Assert.IsType<Image>(icon.Child);

        Border control = new Border();
        icon.Source = control;
        Assert.Same(control, icon.Child);

        icon.Source = null;
        Assert.Null(icon.Child);
    }

    [AvaloniaFact]
    public void Style_changes_apply_to_the_geometry()
    {
        IconPresenter icon = new IconPresenter { Source = Geometry.Parse("M0 0 L1 1") };
        Path path = Assert.IsType<Path>(icon.Child);

        icon.Stroke = Brushes.Blue;
        icon.StrokeThickness = 2;
        icon.Stretch = Stretch.Fill;

        Assert.Same(Brushes.Blue, path.Stroke);
        Assert.Equal(2, path.StrokeThickness);
        Assert.Equal(Stretch.Fill, path.Stretch);
    }
}