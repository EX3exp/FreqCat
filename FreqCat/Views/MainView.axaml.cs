using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FreqCat.Utils;
using FreqCat.ViewModels;
using Serilog;
using System;

namespace FreqCat.Views;

public partial class MainView : UserControl
{
    private MainViewModel viewModel;
    private double _canvasWidth;
    private double _canvasHeight;
    private double lastCanvasWidth;
    public double canvasWidth
    {
        get { return _canvasWidth; }
        set
        {
            Log.Debug($"canvasWidth: {value}");
            _canvasWidth = value;
            catCanvas.Width = value;
            viewModel.FrqSelectionChanged(canvasWidth, canvasHeight);
        }
    }

    public double canvasHeight
    {
        get { return _canvasHeight; }
        set
        {
            _canvasHeight = value;
            viewModel.FrqSelectionChanged(canvasWidth, canvasHeight);
        }
    }

    private double _minCanvasWidth;
    public double minCanvasWidth
    {
        get { return _minCanvasWidth; }
        set
        {
            _minCanvasWidth = value;
        }
    }

    private double _maxCanvasWidth;
    public double maxCanvasWidth
    {
        get { return _maxCanvasWidth; }
        set
        {
            _maxCanvasWidth = value;
        }
    }
    bool IsVisualAttached = false;
    public MainView(MainViewModel v)
    {
        Log.Debug("MainView Initialize");
        InitializeComponent(v);
        this.catCanvas = this.FindControl<Canvas>("catCanvas");

        catCanvas.AttachedToVisualTree += (sender, e) =>
        {
            catCanvas.PropertyChanged += (s, args) =>
            {
                if (args.Property == Visual.BoundsProperty)
                {
                    Log.Debug("Bounds Changed");
                    var bounds = catCanvas.Bounds;
                    var actualWidth = bounds.Width;
                    var actualHeight = bounds.Height;
                    if (!IsVisualAttached)
                    {
                        canvasWidth = actualWidth;
                        IsVisualAttached = true;
                        minCanvasWidth = 800;
                        maxCanvasWidth = 10000;
                    }
                    
                    canvasHeight = actualHeight;
                    catCanvas.Width = actualWidth;
                }
            };
        };

    }

    private void InitializeComponent(MainViewModel v)
    {
        this.DataContext = viewModel = v;
        AvaloniaXamlLoader.Load(this);
        
    }

    public void OnTabSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is TabItem selectedItem)
        {
            string selectedTabName = selectedItem.Header.ToString();
            int selectedTabIndex = viewModel.CurrentDirIndex;
            Log.Information($"Selected Tab: {selectedTabIndex} / {selectedTabName}");

            viewModel.DirSelectionChanged();
            viewModel.FrqSelectionChanged(canvasWidth, canvasHeight);
            
        }
    }

    public void OnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem selectedItem)
        {
            string selectedFileName = selectedItem.Content.ToString();
            int selectedFrqIndex = viewModel.CurrentFrqIndex;
            Log.Information($"Selected File: {selectedFrqIndex} / {selectedFileName}");

            viewModel.FrqSelectionChanged(canvasWidth, canvasHeight);
            

        }
    }

    double ZoomAmt = 0;
    private void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        // Check if the control key is pressed
        if (OS.IsMacOS() && e.KeyModifiers.HasFlag(KeyModifiers.Meta) || (OS.IsWindows() || OS.IsLinux() ) && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            double zoomChange = e.Delta.Y > 0 ? 0.1 : -0.1;

            double newCanvasWidth = canvasWidth * (1 + zoomChange);
            if (newCanvasWidth >= minCanvasWidth && newCanvasWidth <= maxCanvasWidth)
            {
                canvasWidth = newCanvasWidth;
            }
            else if (newCanvasWidth < minCanvasWidth)
            {
                canvasWidth = minCanvasWidth;
            }
            else if (newCanvasWidth > maxCanvasWidth)
            {
                canvasWidth = maxCanvasWidth;
            }

            e.Handled = true;
        }
        else
        {
            e.Handled = true;
        }
    }
}
