using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GroveApp.Models;

namespace GroveApp.Controls;

public partial class GallerySlateControl : UserControl
{
    public GallerySlateControl()
    {
        InitializeComponent();
    }

    public event Action? Closed;

    public void Open(IEnumerable<GridImage> images)
    {
        ArgumentNullException.ThrowIfNull(images);
        GalleryWall.Children.Clear();
        foreach (GridImage image in images)
        {
            if (image.LoadedBitmap is not Bitmap bitmap)
            {
                continue;
            }

            GalleryWall.Children.Add(new Viewbox
            {
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.DownOnly,
                Child = new Image
                {
                    Source = bitmap,
                    Width = bitmap.PixelSize.Width,
                    Height = bitmap.PixelSize.Height,
                    Stretch = Stretch.Uniform
                }
            });
        }

        IsVisible = true;
        Focus();
    }

    public void Close()
    {
        if (!IsVisible)
        {
            return;
        }

        GalleryWall.Children.Clear();
        IsVisible = false;
        Closed?.Invoke();
    }
}
