using FreqCat;
using ReactiveUI;
using System.Text;
using System;
using System.Reflection;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using FreqCat.Views;
using Serilog;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using System.IO;
using FreqCat.Format;
using FreqCat.Utils;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Raw;

namespace FreqCat.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Fcat initFcat;
    MainWindow mainWindow;
    Version Version = Assembly.GetEntryAssembly()?.GetName().Version;

    private string _title;

    public string Title
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("FreqCat v");
            sb.Append(Version.ToString());
            sb.Append(" - ");
            sb.Append(CurrentProjectPath);
            if (MainManager.Instance.cmd != null)
            {
                if (MainManager.Instance.cmd.IsNeedSave)
                {
                    sb.Append("*");
                }
            }

            return sb.ToString();
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _title, value);
            OnPropertyChanged(nameof(Title));
        }
    }

    string _currentrojectPath;
    public string CurrentProjectPath
    {
        get => _currentrojectPath;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentrojectPath, value);
            OnPropertyChanged(nameof(CurrentProjectPath));
        }
    }

    private RecentFiles _recentFiles;
    public RecentFiles RecentFiles
    {
        get => _recentFiles;
        set
        {
            this.RaiseAndSetIfChanged(ref _recentFiles, value);
            OnPropertyChanged(nameof(RecentFiles));
        }
    }

    private bool _isPaneOpen = false;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set
        {
            this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
            OnPropertyChanged(nameof(IsPaneOpen));
        }
    }

    private DirectoryLoader _directoryLoader;
    public DirectoryLoader DirectoryLoader
    {
        get => _directoryLoader;
        set
        {
            if (value is null)
            {
                return;
            }
            this.RaiseAndSetIfChanged(ref _directoryLoader, value);
            frqIndexes = new int[value.Data.Datas.Length]; // set selected frq indexes to 0
            CurrentDirs = value.Data.Datas.Select(
                x => new TabItem
                {
                    Header = x.DirName
                }).ToArray();

            // set frqs to the first directory
            if (! (value.Data.Datas.Length == 0)) 
            { 
                CurrentFrqs = value.Data.Datas[0].Datas.Select(
                x => new ListBoxItem
                {
                    Content = x.FileName
                }
                ).ToArray();
            }

            

            IsRootNotSet = false;
            OnPropertyChanged(nameof(DirectoryLoader));
        }
    }
    

    private TabItem[] _currentDirs;
    public TabItem[] CurrentDirs
    {
        get => _currentDirs;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentDirs, value);
            
            OnPropertyChanged(nameof(CurrentDirs));
        }
    }

    private int _currentDirIndex = -1;
    public int CurrentDirIndex
    {
        get => _currentDirIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentDirIndex, value);
            OnPropertyChanged(nameof(CurrentDirIndex));
        }
    }

    private int _currentFrqIndex = -1;
    public int CurrentFrqIndex
    {
        get => _currentFrqIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentFrqIndex, value);
            OnPropertyChanged(nameof(CurrentFrqIndex));
        }
    }
    private int lastDirIndex = -1;

    private int[] frqIndexes; // stores selected frq indexes for each directory

    private string _currentFrqName;
    public string CurrentFrqName
    {
        get => _currentFrqName;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentFrqName, value);
            OnPropertyChanged(nameof(CurrentFrqName));
        }
    }

    private ListBoxItem[] _currentFrqs;
    public ListBoxItem[] CurrentFrqs 
    {
        get => _currentFrqs;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentFrqs, value);
            OnPropertyChanged(nameof(CurrentFrqs));
        }
    }
    public ObservableCollection<MenuItem> RecentMenuCollection { get; set; } = new ObservableCollection<MenuItem>();
    public static FilePickerFileType FreqCatProject { get; } = new("FreqCat Project File")
    {
        Patterns = new[] { "*.fcat" },
        AppleUniformTypeIdentifiers = new[] { "com.ex3.freqcat.project" },
        MimeTypes = new[] { "FreqCatProject/*" }
    };

    
    public MainViewModel(MainWindow window)
    {
        mainWindow = window;

        mainWindow.Closing += WindowClosing;

        if (CurrentProjectPath == null || CurrentProjectPath == string.Empty)
        {
            CurrentProjectPath = (string)mainWindow.FindResource("app.defprojectname");
        }
        initFcat = new Fcat(this);
        SetGestures();
        MainManager.Instance.cmd.SetMainViewModel(this);
        OnInitialDirSelect();

    }
    
    public void DirSelectionChanged()
    {
        // directory selection changed
        CurrentFrqIndex = frqIndexes[CurrentDirIndex];
    }

    string CurrentWavPath = string.Empty;
    private Points _currentWavPlotPoints = new();
    public Points CurrentWavPlotPoints
    {
        get => _currentWavPlotPoints;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentWavPlotPoints, value);
            OnPropertyChanged(nameof(CurrentWavPlotPoints));
        }
    }
    public void FrqSelectionChanged(double canvasWidth, double canvasHeight)
    {
        if (frqIndexes is null)
        {
            return; // not initialized
        }
        if (CurrentFrqIndex == -1)
        {
            return; // not selected
        }
        if (CurrentDirIndex == -1)
        {
            return; // not selected
        }
        // frq selection changed
        frqIndexes[CurrentDirIndex] = CurrentFrqIndex;

        // change frq name
        CurrentFrqName = Path.GetFileName(DirectoryLoader.Data.Datas[CurrentDirIndex].Datas[CurrentFrqIndex].FilePath);
        CurrentWavPath = Path.Combine(DirectoryLoader.Data.Datas[CurrentDirIndex].Datas[CurrentFrqIndex].FilePath.Replace("_wav.frq", ".wav"));
        CurrentWavPlotPoints = WavPlotter.GetWavFormPoints(CurrentWavPath, canvasWidth, canvasHeight);
        
        // todo show graphics. if selected frq is not exist, show error message
    }

    bool isPlaying = false;
    public void OnPlayButtonClick()
    {
        // play wav file
        if (isPlaying && MainManager.Instance.AudioM.IsPlaying)
        {
            MainManager.Instance.AudioM.StopAudio();
            isPlaying = false;
            
        }
        else
        {
            isPlaying = true;
            MainManager.Instance.AudioM.PlayAudio(CurrentWavPath);
        }
        
    }
    public async void OnNewButtonClick()
    {

        if (MainManager.Instance.cmd.IsNeedSave)
        {
            if (!await AskIfSaveAndContinue())
            {
                return;
            }
            ClearUI();
            CurrentProjectPath = (string)mainWindow.FindResource("app.defprojectname");

            
            OnInitialDirSelect();
            MainManager.Instance.cmd.ProjectOpened();
        }
        else
        {
            ClearUI();
            CurrentProjectPath = (string)mainWindow.FindResource("app.defprojectname");

            MainManager.Instance.cmd.ProjectOpened();
            OnInitialDirSelect();

        }

    }

    void ClearUI()
    {
        CurrentDirs = null;
        CurrentFrqs = null;
        DirectoryLoader = null;
    }

    bool forceClose = false;
    public async void WindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (forceClose)
        {
            if (MainManager.Instance.Setting.ClearCacheOnQuit)
            {
                try
                {
                    Log.Information("Clearing cache...");
                    ClearCache();
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to clear cache: {ex.Message}");
                }
                Log.Information("Cache cleared.");
            }
            return;
        }


        e.Cancel = true;
        if (MainManager.Instance.cmd.IsNeedSave)
        {
            if (!await AskIfSaveAndContinue())
            {
                return;
            }
        }

        forceClose = true;
        mainWindow.Close();
    }

    private async Task<bool> AskIfSaveAndContinue()
    {

        var result = await MessageWindow.Show(mainWindow, (string)mainWindow.FindResource("app.beforeExit"), "", MessageWindow.MessageBoxButtons.YesNo);
        switch (result)
        {
            case MessageWindow.MessageBoxResult.Yes:
                await SaveProject();
                goto case MessageWindow.MessageBoxResult.No;
            case MessageWindow.MessageBoxResult.No:
                return true; // Continue.
            default:
                return false; // Cancel.
        }
    }

    public void OnZoom(double zoomAmt)
    {
        Log.Debug($"Zooming: {zoomAmt}");
        // TODO
    }
        public void SetGestures()
    {
        if (OS.IsMacOS())
        {
            
           
        }
        else
        {
            
        }
    }
    public void PaneToggle()
    {
        IsPaneOpen = !IsPaneOpen;
    }
    public async Task<bool> AskIfSaveAndContinueForUpdate()
    {

        var result = await MessageWindow.Show(mainWindow, (string)mainWindow.FindResource("app.update.beforeExit"), "", MessageWindow.MessageBoxButtons.YesNo);
        switch (result)
        {
            case MessageWindow.MessageBoxResult.Yes:
                await SaveProject();
                goto case MessageWindow.MessageBoxResult.No;
            case MessageWindow.MessageBoxResult.No:
                return true; // Continue.
            default:
                return false; // Cancel.
        }
    }

    private bool _PaneToggleselected = false;
    public bool PaneToggleSelected
    {
        get => _PaneToggleselected;
        set
        {
            this.RaiseAndSetIfChanged(ref _PaneToggleselected, value);
            OnPropertyChanged(nameof(PaneToggleSelected));
        }
    }
    private bool _isRootNotSet = true;
    public bool IsRootNotSet
    {
        get => _isRootNotSet;
        set
        {
            this.RaiseAndSetIfChanged(ref _isRootNotSet, value);
            OnPropertyChanged(nameof(IsRootNotSet));
        }
    }
    IStorageFolder LastSelectedPath;
    public async void OnInitialDirSelect()
    {
        IsRootNotSet = true;
        var topLevel = TopLevel.GetTopLevel(mainWindow);
        if (LastSelectedPath is null)
        {
            LastSelectedPath = topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Downloads).Result;
        }
        var directory = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = (string)mainWindow.FindResource("menu.files.init.desc"),
            AllowMultiple = false,
            SuggestedStartLocation = LastSelectedPath,
            SuggestedFileName = LastSelectedPath.Path.LocalPath

        });
        if (!IsPaneOpen)
        {
            PaneToggle();
            PaneToggleSelected = true;
        }
        



        if (directory.Count > 0)
        {
            if (directory[0] is null)
            {
                return;
            }
            string path = directory[0].Path.LocalPath;
            if (path == string.Empty)
            {
                path = LastSelectedPath.Path.LocalPath;
            }
            try
            {
                Log.Information($"Set Root Path as {path}");
                DirectoryLoader = new DirectoryLoader(path);
                LastSelectedPath = directory[0];
            }
            catch (Exception e)
            {
                Log.Error($"[Failed to set Root Path]{e.ToString}: {e.Message} \n>> traceback: \n\t{e.StackTrace}");
                var res = await ShowConfirmWindow("menu.files.init.failed");
            }
        }

    }


    public async Task<bool> ShowConfirmWindow(string resourceId)
    {

        var result = await MessageWindow.Show(mainWindow, (string)mainWindow.FindResource(resourceId), "", MessageWindow.MessageBoxButtons.Ok);

        switch (result)
        {
            case MessageWindow.MessageBoxResult.Ok:
                return true;
            default:
                return false;
        }
    }
    public void ClearCache()
    {
        foreach (var file in Directory.GetFiles(MainManager.Instance.PathM.CachePath))
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to delete cache file: {file} {e.Message}");
            }
        }
    }
    public async Task OpenProject(string path)
    {
        CurrentProjectPath = path;
        // tODO
        MainManager.Instance.cmd.ProjectOpened();
    }
    public Fcat project
    {
        get => new Fcat(this);
        set { }
    }
    public async Task SaveProject(bool forceSaveAs = false)
    {
        if (File.Exists(CurrentProjectPath) && !forceSaveAs)
        {
            project.Save(CurrentProjectPath);
        }
        else
        {

            var topLevel = TopLevel.GetTopLevel(mainWindow);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = (string)mainWindow.FindResource("app.saveproject"),
                DefaultExtension = "fcat",
                FileTypeChoices = new[] { FreqCatProject },
                SuggestedFileName = (string)mainWindow.FindResource("app.defprojectname"),
            });

            if (file is not null)
            {
                string path = file.Path.LocalPath;
                CurrentProjectPath = path;
                project.Save(path);

                RecentFiles.AddRecentFile(CurrentProjectPath, this);
                OnPropertyChanged(nameof(RecentMenuCollection));
            }
        }
    }
}
