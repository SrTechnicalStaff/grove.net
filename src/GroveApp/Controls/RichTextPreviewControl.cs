using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GroveApp.DesignSystem;
using GroveApp.Engine;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls
{
    public class RichTextPreviewControl : Control
    {
        private string _text = string.Empty;

        public static readonly DirectProperty<RichTextPreviewControl, string> TextProperty =
            AvaloniaProperty.RegisterDirect<RichTextPreviewControl, string>(
                nameof(Text),
                o => o.Text,
                (o, v) => o.Text = v);

        public string Text
        {
            get => _text;
            set
            {
                if (SetAndRaise(TextProperty, ref _text, value))
                {
                    InvalidateMeasure();
                    InvalidateVisual();
                }
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (string.IsNullOrWhiteSpace(_text))
                return;

            double width = Math.Max(100, Bounds.Width);
            double height = Math.Max(100, Bounds.Height);

            var layout = RichTextEngine.CreateLayout(
                _text,
                Typography.SizeBody,
                Colors.NoteTextBrush,
                width,
                height,
                1.0
            );

            RichTextEngine.Render(context, new Point(0, 0), layout);
        }
    }
}
