using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FreqCat.ViewModels;
using Serilog;
using System;

namespace FreqCat.Views;

public partial class MainView : UserControl
{
    private MainViewModel viewModel;
    private double _canvasWidth;
    private double _canvasHeight;

    public double canvasWidth
    {
        get { return _canvasWidth; }
        set
        {
            _canvasWidth = value;
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
                    var bounds = catCanvas.Bounds;
                    var actualWidth = bounds.Width;
                    var actualHeight = bounds.Height;
                    canvasWidth = actualWidth;
                    canvasHeight = actualHeight;
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
}
