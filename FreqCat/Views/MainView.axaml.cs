using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FreqCat.ViewModels;
using Serilog;

namespace FreqCat.Views;

public partial class MainView : UserControl
{
    private MainViewModel viewModel;
    public MainView(MainViewModel v)
    {
        Log.Debug("MainView Initialize");
        InitializeComponent(v);
        
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

        }
    }

    public void OnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem selectedItem)
        {
            string selectedFileName = selectedItem.Content.ToString();
            int selectedFrqIndex = viewModel.CurrentFrqIndex;
            Log.Information($"Selected File: {selectedFrqIndex} / {selectedFileName}");
            viewModel.FrqSelectionChanged();

            //YourFunction(selectedFileName); // 선택된 항목에 따라 특정 함수 실행
        }
    }
}
