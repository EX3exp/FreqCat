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

namespace FreqCat.ViewModels;

public class MainViewModel : ViewModelBase
{
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

    private bool _isPaneOpen;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set
        {
            this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
            OnPropertyChanged(nameof(IsPaneOpen));
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
                    //Log.Information("Clearing cache...");
                    //ClearCache();
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
    public void clearCache()
    {
        // TODO
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
