using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using GroveApp.DesignSystem;
using GroveApp.Engine;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls
{
    public partial class LayerManagerSlate : UserControl
    {
        private ISpatialLayerStateService? _layerService;
        private int? _editingZIndex;
        private int? _confirmDeleteZIndex;
        private string _filterQuery = string.Empty;

        public event Action? Closed;

        public LayerManagerSlate()
        {
            InitializeComponent();

            BtnClose.Click += (_, _) => Close();
            BtnNewLayer.Click += (_, _) => InsertLayerAboveActive();

            TxtFilter.TextChanged += (_, _) =>
            {
                _filterQuery = TxtFilter.Text ?? string.Empty;
                RebuildList();
            };

            KeyDown += OnSlateKeyDown;
        }

        public void Open()
        {
            IsVisible = true;
            RebuildList();
            Focus();
        }

        public void Close()
        {
            if (!IsVisible)
            {
                return;
            }

            IsVisible = false;
            Closed?.Invoke();
        }

        public void Toggle()
        {
            if (IsVisible)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        private void InsertLayerAboveActive()
        {
            if (_layerService == null)
            {
                return;
            }

            _layerService.InsertLayerAbove(_layerService.ActiveLayer.ZIndex);
            RebuildList();
        }

        public void BindLayerService(ISpatialLayerStateService layerService)
        {
            if (_layerService != null)
            {
                _layerService.LayerStackChanged -= OnLayerStackChanged;
                _layerService.ActiveLayerChanged -= OnActiveLayerChanged;
            }

            _layerService = layerService;

            if (_layerService != null)
            {
                _layerService.LayerStackChanged += OnLayerStackChanged;
                _layerService.ActiveLayerChanged += OnActiveLayerChanged;
            }

            RebuildList();
        }

        private void OnLayerStackChanged()
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(RebuildList);
        }

        private void OnActiveLayerChanged(SpatialLayerModel model)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(RebuildList);
        }

        public void RebuildList()
        {
            LayerItemsPanel.Children.Clear();
            if (_layerService == null) return;

            var layers = _layerService.Layers;
            TxtLayerCount.Text = $"{layers.Count} LAYERS";
            var filter = _filterQuery.Trim().ToLowerInvariant();

            foreach (var layer in layers)
            {
                if (!string.IsNullOrEmpty(filter) &&
                    !layer.DisplayName.ToLowerInvariant().Contains(filter) &&
                    !layer.Label.ToLowerInvariant().Contains(filter))
                {
                    continue;
                }

                var rowControl = CreateLayerRow(layer);
                LayerItemsPanel.Children.Add(rowControl);
            }
        }

        private Control CreateLayerRow(SpatialLayerModel layer)
        {
            var isEditing = _editingZIndex == layer.ZIndex;
            var isDeleting = _confirmDeleteZIndex == layer.ZIndex;

            var rowBorder = new Border
            {
                BorderThickness = new Thickness(1),
                BorderBrush = layer.IsActive
                    ? Colors.SignalInteractionBrush
                    : Colors.GridMajorInkBrush,
                Background = layer.IsActive
                    ? new SolidColorBrush(Colors.SurfaceRaised)
                    : Colors.SurfaceChromeBrush,
                CornerRadius = new CornerRadius(0),
                Padding = new Thickness(6, 4),
                Margin = new Thickness(0, 1),
                Cursor = new Cursor(StandardCursorType.Hand)
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("40, *, 16, Auto")
            };

            // Stable 4ch Monospaced Label (01, 02, B01, B02)
            // Left padded / 4ch monospaced column
            string label4Ch = layer.Label.PadRight(4);
            var lblBlock = new TextBlock
            {
                Text = label4Ch,
                FontFamily = new FontFamily(Typography.FontFamilyMono),
                FontSize = Typography.SizeLabel,
                FontWeight = FontWeight.Bold,
                Foreground = layer.IsActive
                    ? Colors.SignalInteractionBrush
                    : Colors.TextMetaBrush,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(lblBlock, 0);
            grid.Children.Add(lblBlock);

            if (isEditing)
            {
                // Inline rename TextBox
                var txtEdit = new TextBox
                {
                    Text = layer.DisplayName,
                    FontSize = Typography.SizeCaption,
                    FontFamily = new FontFamily(Typography.FontFamilyUi),
                    Background = Colors.SurfaceGridBrush,
                    Foreground = Colors.TextPrimaryBrush,
                    BorderBrush = Colors.SignalInteractionBrush,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(4, 2),
                    VerticalAlignment = VerticalAlignment.Center
                };

                txtEdit.KeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        CommitRename(layer.ZIndex, txtEdit.Text);
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Escape)
                    {
                        _editingZIndex = null;
                        RebuildList();
                        e.Handled = true;
                    }
                };

                txtEdit.LostFocus += (s, e) =>
                {
                    CommitRename(layer.ZIndex, txtEdit.Text);
                };

                Grid.SetColumn(txtEdit, 1);
                grid.Children.Add(txtEdit);

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    txtEdit.Focus();
                    txtEdit.SelectAll();
                });
            }
            else if (isDeleting)
            {
                // Inline Delete Confirmation
                var deletePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var txtConfirm = new TextBlock
                {
                    Text = $"Remove Layer {layer.Label} and move its content to Layer 01?",
                    FontSize = Typography.SizeLabel,
                    Foreground = Colors.SignalRefusalBrush,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var btnConfirm = new Button
                {
                    Content = "Yes",
                    Padding = new Thickness(6, 2),
                    Background = Colors.KEditBorderBrush,
                    Foreground = Colors.NoteTextBrush,
                    FontSize = 11,
                    CornerRadius = new CornerRadius(0)
                };
                btnConfirm.Click += (_, _) =>
                {
                    ConfirmDelete(layer.ZIndex);
                };

                var btnCancel = new Button
                {
                    Content = "Cancel",
                    Padding = new Thickness(6, 2),
                    Background = Brushes.Transparent,
                    Foreground = Colors.TextMetaBrush,
                    FontSize = 11,
                    CornerRadius = new CornerRadius(0)
                };
                btnCancel.Click += (_, _) =>
                {
                    _confirmDeleteZIndex = null;
                    RebuildList();
                };

                deletePanel.Children.Add(txtConfirm);
                deletePanel.Children.Add(btnConfirm);
                deletePanel.Children.Add(btnCancel);

                Grid.SetColumn(deletePanel, 1);
                grid.Children.Add(deletePanel);
            }
            else
            {
                // Normal Display Name TextBlock
                var nameBlock = new TextBlock
                {
                    Text = layer.DisplayName,
                    FontFamily = new FontFamily(Typography.FontFamilyUi),
                    FontSize = 12,
                    Foreground = layer.IsActive
                        ? Colors.TextPrimaryBrush
                        : Colors.TextSecondaryBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                Grid.SetColumn(nameBlock, 1);
                grid.Children.Add(nameBlock);
            }

            var visibilityButton = new Button
            {
                Content = layer.IsVisible ? "●" : "○",
                Padding = new Thickness(4, 1),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = layer.IsVisible ? Colors.TextSecondaryBrush : Colors.TextUnavailableBrush,
                FontSize = 10,
                CornerRadius = new CornerRadius(0),
            };
            ToolTip.SetTip(visibilityButton, layer.IsVisible ? "Hide layer" : "Show layer");
            visibilityButton.Click += (_, e) =>
            {
                _layerService?.SetLayerVisibility(layer.ZIndex, !layer.IsVisible);
                e.Handled = true;
            };

            var lockButton = new Button
            {
                Content = layer.IsLocked ? "▣" : "□",
                Padding = new Thickness(4, 1),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = layer.IsLocked ? Colors.SignalRefusalBrush : Colors.TextUnavailableBrush,
                FontSize = 10,
                CornerRadius = new CornerRadius(0),
                IsEnabled = !layer.IsProtected
            };
            ToolTip.SetTip(lockButton, layer.IsLocked ? "Unlock layer" : "Lock layer");
            lockButton.Click += (_, e) =>
            {
                _layerService?.SetLayerLocked(layer.ZIndex, !layer.IsLocked);
                e.Handled = true;
            };

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 2
            };
            actions.Children.Add(visibilityButton);
            actions.Children.Add(lockButton);

            var colorSwatch = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.Parse(layer.ColorHex)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            ToolTip.SetTip(colorSwatch, $"Layer color {layer.ColorHex}");
            Grid.SetColumn(colorSwatch, 2);
            grid.Children.Add(colorSwatch);

            Grid.SetColumn(actions, 3);
            grid.Children.Add(actions);

            // Click row to select layer
            rowBorder.PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(rowBorder).Properties.IsLeftButtonPressed)
                {
                    _layerService?.SetActiveLayer(layer.ZIndex);
                    e.Handled = true;
                }
            };

            // Double Click to trigger inline rename
            rowBorder.DoubleTapped += (s, e) =>
            {
                _editingZIndex = layer.ZIndex;
                RebuildList();
                e.Handled = true;
            };

            rowBorder.Child = grid;
            return rowBorder;
        }

        private void CommitRename(int zIndex, string? newName)
        {
            _editingZIndex = null;
            if (_layerService != null && !string.IsNullOrWhiteSpace(newName))
            {
                _layerService.RenameLayer(zIndex, newName, out _);
            }
            RebuildList();
        }

        private void ConfirmDelete(int zIndex)
        {
            _confirmDeleteZIndex = null;
            if (_layerService != null)
            {
                // Transfer items to Main Ground Layer ZIndex 0 (label 01).
                _layerService.RemoveLayer(zIndex, 0);
            }
            RebuildList();
        }

        public bool ProcessKeyDown(KeyEventArgs e)
        {
            if (_layerService == null) return false;

            var activeZ = _layerService.ActiveLayer.ZIndex;

            // 1. Shift+[ / Shift+] (Jump Bottom / Top)
            if (e.Key == Key.OemOpenBrackets && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                _layerService.JumpToBottom();
                e.Handled = true;
                return true;
            }
            if (e.Key == Key.OemCloseBrackets && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                _layerService.JumpToTop();
                e.Handled = true;
                return true;
            }

            // 2. [ / ] (Navigate Down / Up)
            if (e.Key == Key.OemOpenBrackets && e.KeyModifiers == KeyModifiers.None)
            {
                _layerService.SetActiveLayer(activeZ - 1);
                e.Handled = true;
                return true;
            }
            if (e.Key == Key.OemCloseBrackets && e.KeyModifiers == KeyModifiers.None)
            {
                _layerService.SetActiveLayer(activeZ + 1);
                e.Handled = true;
                return true;
            }

            // 3. Ctrl+Shift+N (Insert Layer Above)
            if (e.Key == Key.N &&
                e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
                e.KeyModifiers.HasFlag(KeyModifiers.Shift) &&
                !e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                _layerService.InsertLayerAbove(activeZ);
                e.Handled = true;
                return true;
            }

            // 4. Ctrl+Alt+Shift+N (Insert Layer Below)
            if (e.Key == Key.N &&
                e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
                e.KeyModifiers.HasFlag(KeyModifiers.Shift) &&
                e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                _layerService.InsertLayerBelow(activeZ);
                e.Handled = true;
                return true;
            }

            // 5. Alt+Up / Alt+Down (Reorder Swap Up / Down)
            if (e.Key == Key.Up && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                _layerService.ReorderSwap(activeZ, activeZ + 1);
                e.Handled = true;
                return true;
            }
            if (e.Key == Key.Down && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                _layerService.ReorderSwap(activeZ, activeZ - 1);
                e.Handled = true;
                return true;
            }

            // 6. F2 (Inline Rename)
            if (e.Key == Key.F2)
            {
                _editingZIndex = activeZ;
                RebuildList();
                e.Handled = true;
                return true;
            }

            // 7. Del (Inline Remove Confirm)
            if (e.Key == Key.Delete)
            {
                _confirmDeleteZIndex = activeZ;
                RebuildList();
                e.Handled = true;
                return true;
            }

            return false;
        }

        private void OnSlateKeyDown(object? sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                ProcessKeyDown(e);
            }
        }
    }
}
