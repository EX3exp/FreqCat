using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using FreqCat.Utils;
using FreqCat.ViewModels;
using Serilog;
using System;
using R3;
using Avalonia.Threading;

using Avalonia.Animation.Easings;
using FreqCat.Commands;
namespace FreqCat.Views;

public partial class MainView : UserControl
{
    private MainViewModel viewModel;
    private double _canvasWidth;
    private double _canvasHeight;
    private double initialCanvasWidth;
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

    const int GridHeight = 20;


    public MainView(MainViewModel v)
    {
        Log.Debug("MainView Initialize");
        InitializeComponent(v);
        this.GridCanvas = this.FindControl<Canvas>("GridCanvas");
        GridCanvas.Children.Clear();

        GridCanvas.AttachedToVisualTree += (sender, e) =>
        {
            GridCanvas.PropertyChanged += (s, args) =>
            {
                if (args.Property == Visual.BoundsProperty)
                {
                    GridCanvas.Children.Clear();
                    Rect bounds = GridCanvas.Bounds;
                    double actualWidth = bounds.Width;
                    double actualHeight = bounds.Height;
                    
                    int offsety = 0;
                    int offsetx = 0;
                    int noy = 0;
                    int nox = 0;

                    for (int i = 0; i < actualHeight; i += GridHeight)
                    {
                        offsetx = 0;
  
                        Rectangle rect = new Rectangle
                        {
                            Width = actualWidth,
                            Height = GridHeight,
                            Fill = noy % 2 == 0 ? Brushes.LightGray : Brushes.White,
                            Opacity = 0.3,
                            Stroke = Brushes.Gray,
                            StrokeThickness = noy % 2 == 0 ? 0.4 : 0,
                            Margin = new Thickness(0, 0, 0, 0)
                        };

                        Canvas.SetTop(rect, offsety);
                        GridCanvas.Children.Add(rect);
                        offsety += GridHeight;
                        ++noy;
                    }
                }
            };
        };
        

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
                        Log.Debug("IsVisualAttached");
                        if (viewModel is null)
                        {
                            Log.Error("[MainView] viewModel is null");
                            return;
                        }
                        viewModel.Ratio = 1;
                        canvasWidth = actualWidth;
                        initialCanvasWidth = actualWidth;
                        IsVisualAttached = true;
                        minCanvasWidth = ApplyScale(initialCanvasWidth, -0.8); // 20%
                        maxCanvasWidth = ApplyScale(initialCanvasWidth, 10); // 1000%
                    }
                    
                    canvasHeight = actualHeight;
                    catCanvas.Width = actualWidth;

                }
            };
        };

    }
    /// <summary>
    /// Apply scale to the original value. ratio is in percentage. e.g. 10% = 0.1
    /// </summary>
    /// <param name="originalValue"></param>
    /// <param name="ratio"></param>
    /// <returns></returns>
    double ApplyScale(double originalValue, double ratio)
    {
        return originalValue + (originalValue * ratio);
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
            IsVisualAttached = false;

            catCanvas.IsVisible = false;
            catCanvas.IsVisible = true; // invoke redraw
            viewModel.FrqSelectionChanged(canvasWidth, canvasHeight);
            

        }
    }

    double lastWheelChangedTime;
    double acceleration = 1;
    private void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        // Check if the control key is pressed
        if (OS.IsMacOS() && e.KeyModifiers.HasFlag(KeyModifiers.Meta) || (OS.IsWindows() || OS.IsLinux() ) && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            double factor = e.Delta.Y > 0 ? 0.1 : -0.1;
            double currentWheelChangedTime = MainManager.Instance.CurrentTime;
            // if wheel changed time is less than 0.1 sec, increase acceleration
            if (currentWheelChangedTime - lastWheelChangedTime < 0.1)
            {
                acceleration += 0.01;
            }
            else
            {
                acceleration = 1;
            }

            viewModel.Ratio += factor * acceleration;
            double newCanvasWidth = ApplyScale(initialCanvasWidth, viewModel.Ratio);
            if (newCanvasWidth >= minCanvasWidth && newCanvasWidth <= maxCanvasWidth)
            {
                canvasWidth = newCanvasWidth;
            }
            else if (newCanvasWidth < minCanvasWidth)
            {
                canvasWidth = minCanvasWidth;
                viewModel.Ratio = 0.2;
            }
            else if (newCanvasWidth > maxCanvasWidth)
            {
                canvasWidth = maxCanvasWidth;
                viewModel.Ratio = 10;
            }

            lastWheelChangedTime = currentWheelChangedTime;
            e.Handled = true;
            
        }
        else
        {
            e.Handled = true;
        }
    }

    Point CurrentPos = new Point(0, 0);
    Point StartPos = new Point(0, 0);
    Point EndPos = new Point(0, 0);
    double xMoved = 0;
    double yMoved = 0;
    bool isDragging = false; // when left click or right click is pressed
    bool isReseting = false; // when right click is pressed
    double drawStartTime = 0;
    LineEditCommand lineEditCommand;
    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        isDragging = true;
        Point p = e.GetPosition(catCanvas);
        drawStartTime = MainManager.Instance.CurrentTime;

        if (e.GetCurrentPoint(catCanvas).Properties.IsLeftButtonPressed)
        {
            StartPos = p;
            isReseting = false;
            lastX = p;
            lineEditCommand = new LineEditCommand(viewModel);
        }
        else if (e.GetCurrentPoint(catCanvas).Properties.IsRightButtonPressed)
        {
            StartPos = p;
            isReseting = true;
            lastX = p;
            lineEditCommand = new LineEditCommand(viewModel);
        }
        else
        {
            isDragging = false;
            isReseting = false;

        }
    }

    const int minStride = 1; // to optimize drawing with canvas size
    Point lastX = new Point(-1, 0);
    Point fixedY = new Point(0, 0); // for shift key

    Points SmootherLine(Points target, int startIndex, int endIndex, bool trim = false, int trimEnd=-1)
    {

        Points smoothedPoints = new Points();
        for (int i = 0; i < target.Count; ++i)
        {
            if (i > startIndex && i < endIndex)
            {

                Point startPoint = target[startIndex];
                Point endPoint = target[endIndex];
                double val = (target[i].X - startPoint.X) / (endPoint.X - startPoint.X);
                double smoothed = EasingFunction.Linear(startPoint.Y, endPoint.Y, val);
                smoothedPoints.Add(new Point(target[i].X, smoothed));
            }
            else
            {
                smoothedPoints.Add(target[i]);
            }
        }
        Points res = new Points();
        if (trim)
        {
            for (int i = 0; i < smoothedPoints.Count; ++i)
            {
                if (i <= trimEnd)
                {
                    res.Add(smoothedPoints[i]);
                }
            }
        }
        else
        {
            res = smoothedPoints;
        }
        return res;

    } 
    bool drawShiftStarted = false;

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        
        if (!isDragging)
        {
            return;
        }
        Log.Debug("Pointer Moved");
        Point p = e.GetPosition(catCanvas);
        xMoved = p.X - StartPos.X;
        yMoved = p.Y - StartPos.Y;
        if (e.GetCurrentPoint(catCanvas).Properties.IsLeftButtonPressed)
        {
            isReseting = false;
        }
        else if (e.GetCurrentPoint(catCanvas).Properties.IsRightButtonPressed)
        {
            isReseting = true;
        }
        int stride = 1;
        if (lastX.X != -1)
        {
            double toInterpolate = Math.Abs(p.X - lastX.X);
            int strideAdd = 0;
            if (toInterpolate > 1)
            {
                strideAdd = (int)toInterpolate;
            }
            stride = strideAdd + minStride;
        }

        bool isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        if (isShiftPressed && !drawShiftStarted)
        {
            fixedY = p;
            drawShiftStarted = true;
        }
        else if (!isShiftPressed && drawShiftStarted)
        {
            fixedY = new Point(0, 0);
            drawShiftStarted = false;
        }

        if (!isReseting)
        {
            

            Points newPoints = new Points();
            int i = 0;
            Point nextPoint;
            Point prevPoint;
            foreach (Point point in viewModel.CurrentFrqPlotPoints)
            {
                if (viewModel.CurrentFrqPlotPoints.Count > i + stride)
                {
                    nextPoint = viewModel.CurrentFrqPlotPoints[i + stride];
                }
                else
                {
                    nextPoint = new Point(point.X + stride, point.Y); // dummy point
                }
                if (i > stride - 1)
                {
                    prevPoint = viewModel.CurrentFrqPlotPoints[i - stride];
                }
                else
                {
                    prevPoint = new Point(point.X - stride, point.Y); // dummy point
                }
                if (((prevPoint.X < nextPoint.X) && p.X > prevPoint.X && p.X < nextPoint.X) 
                    || (prevPoint.X > nextPoint.X && p.X < prevPoint.X && p.X > nextPoint.X)
                    || p.X == point.X)
                {
                    double y;
                    if (isShiftPressed)
                    {
                        y = fixedY.Y;
                        if (y > 0 && y < canvasHeight)
                        {
                            newPoints.Add(new Point(point.X, fixedY.Y));
                            
                        }
                        else
                        {
                            newPoints.Add(point);
                        }
                    }
                    else
                    {
                        y = p.Y;
                        if (y >= 0 && y <= canvasHeight)
                        {
                            newPoints.Add(new Point(point.X, p.Y));
                            if (i > stride - 1)
                            {
                                newPoints = SmootherLine(newPoints, i - stride, i);
                            }
                        }
                        else
                        {
                            newPoints.Add(point);
                        }
                    }
                }
                else
                {
                    newPoints.Add(point);
                }
                ++i;
            }
            
            
            
            viewModel.CurrentFrqPlotPoints = newPoints;
            lastX = p;
        }
        else
        {
            Points newPoints = new Points();
            Points OriginalPoints = FrqPlotter.GetFrqPoints(new Frq(viewModel.DirectoryLoader.Data.Datas[viewModel.CurrentDirIndex].Datas[viewModel.CurrentFrqIndex].FilePath), canvasWidth, canvasHeight, (int)canvasHeight);
            int i = 0;
            Point nextPoint;
            Point prevPoint;
            foreach (Point point in viewModel.CurrentFrqPlotPoints)
            {
                if (viewModel.CurrentFrqPlotPoints.Count > i + stride)
                {
                    nextPoint = viewModel.CurrentFrqPlotPoints[i + stride];
                }
                else
                {
                    nextPoint = new Point(point.X + stride, point.Y); // dummy point
                }
                if (i > stride - 1)
                {
                    prevPoint = viewModel.CurrentFrqPlotPoints[i - stride];
                }
                else
                {
                    prevPoint = new Point(point.X - stride, point.Y); // dummy point
                }
                if (((prevPoint.X < nextPoint.X) && p.X > prevPoint.X && p.X < nextPoint.X)
                    || (prevPoint.X > nextPoint.X && p.X < prevPoint.X && p.X > nextPoint.X)
                    || p.X == point.X)
                {
                    double y = OriginalPoints[i].Y;
                    if (y >= 0 && y <= canvasHeight)
                    {
                        newPoints.Add(new Point(point.X, y));
                    }
                    else
                    {
                        newPoints.Add(point);
                    }
                }
                else
                {
                    newPoints.Add(point);
                }
                ++i;
                
            }
            Frq newFrq = viewModel.CurrentFrq;

            
            viewModel.CurrentFrqPlotPoints = newPoints;
            lastX = p;
        }
        
    }

    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (!isDragging)
        {
            return;
        }

        Points OriginalPoints = FrqPlotter.GetFrqPoints(viewModel.CurrentFrq, canvasWidth, canvasHeight, (int)canvasHeight);
        viewModel.CurrentFrq = new Frq(FrqPlotter.ReverseFrqPoints(viewModel.CurrentFrq, viewModel.CurrentFrqPlotPoints, canvasWidth ,canvasHeight, (int)canvasHeight));

        Point p = e.GetPosition(catCanvas);
        xMoved = p.X - StartPos.X;
        yMoved = p.Y - StartPos.Y;
        lastX = new Point(-1, 0);
        isDragging = false;
        isReseting = false;
        viewModel.FrqSelectionChanged(canvasWidth, canvasHeight);
        MainManager.Instance.cmd.ExecuteCommand(lineEditCommand);
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        await viewModel.SaveProject();
    }

    private async void OnSaveAsClick(object sender, RoutedEventArgs e)
    {
        await viewModel.SaveProject(forceSaveAs: true);
    }
}
