﻿using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ImoutoRebirth.Navigator.Commands;
using ImoutoRebirth.Navigator.Model;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.ImoutoViewer;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;
using MahApps.Metro.Controls.Dialogs;

namespace ImoutoRebirth.Navigator.ViewModel;

class MainWindowVM : VMBase
{
    #region Fields

    private MainWindow _view;
    private int _previewSize = 256;
    private int _totalCount;
    private bool _isLoading;
    private string _status;
    private string _statusToolTip;
    private readonly IFileService _fileService;
    private readonly IFileLoadingService _fileLoadingService;
    private readonly IImoutoViewerService _imoutoViewerService;
    private readonly DispatcherTimer _appendNewContentTimer = new() { Interval = TimeSpan.FromSeconds(5) };

    #endregion Fields

    #region Constructors

    public MainWindowVM()
    {
        _fileLoadingService = ServiceLocator.GetService<IFileLoadingService>();
        _fileService = ServiceLocator.GetService<IFileService>();
        _imoutoViewerService = ServiceLocator.GetService<IImoutoViewerService>();

        InitializeCommands();

        NavigatorList.CollectionChanged += (_, _) => OnPropertyChanged(() => LoadedCount);

        _appendNewContentTimer.Tick += async (_, _) => await LoadNew();
    }

    private async Task LoadNew()
    {
        _appendNewContentTimer.Stop();
        UpdatePreviews();

        var total = TotalCount;

        var newTotal = await _fileLoadingService.GetCount(
            TagSearchVM.SelectedCollection.Value,
            TagSearchVM.SelectedBindedTags.Select(x => x.Model).ToList());

        if (newTotal <= total)
        {
            _appendNewContentTimer.Start();
            return;
        }

        try
        {
            await _fileLoadingService.LoadFiles(
                10_000,
                _previewSize,
                TagSearchVM.SelectedCollection.Value,
                TagSearchVM.SelectedBindedTags.Select(x => x.Model).ToList(),
                x => TotalCount = x,
                (x, ct) =>
                {
                    foreach (var navigatorListEntry in x)
                    {
                        ct.ThrowIfCancellationRequested();
                        NavigatorList.Add(navigatorListEntry);
                    }
                },
                () => TotalCount = total,
                () => { },
                () => { },
                total);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Can't load images from collection: " + ex.Message);
            SetStatusError("Can't load images from collection", ex.Message);
        }

        _appendNewContentTimer.Start();
    }

    public async Task InitializeAsync()
    {
        await CollectionManager.ReloadCollectionsAsync();

        TagSearchVM = new TagSearchVM(CollectionManager.Collections);
        TagSearchVM.SelectedTagsUpdated += TagSearchVM_SelectedTagsUpdated;
        TagSearchVM.SelectedCollectionCahnged += TagSearchVMOnSelectedCollectionChanged;

        Settings.ShowPreviewOnSelectChanged += Settings_ShowPreviewOnSelectChanged;

        TagsEdit = new TagsEditVM(this);

        _view = new MainWindow
        {
            DataContext = this
        };
        _view.Loaded += _view_Loaded;
        _view.SelectedItemsChanged += OnViewOnSelectedItemsChanged;
        _view.Show();
    }

    private void OnViewOnSelectedItemsChanged(object sender, EventArgs args)
    {
        OnPropertyChanged(() => SelectedEntries);

        if (SelectedItem == null)
            return;

        TagSearchVM.UpdateCurrentTags(SelectedItem);
        FileInfoVM.UpdateCurrentInfo(SelectedItem, NavigatorList.IndexOf(SelectedItem));
    }

    #endregion Constructors

    #region Properties

    public Size SlotSize => new(_previewSize + 30, _previewSize + 30);

    private Size PreviewSize => new(_previewSize, _previewSize);

    public ObservableCollection<INavigatorListEntry> NavigatorList { get; } = new();

    public string Title => "Imouto Navigator";

    public TagSearchVM TagSearchVM { get; set; }

    public CollectionManagerVm CollectionManager { get; } = new();

    public SettingsVM Settings { get; } = new();

    public TagsEditVM TagsEdit { get; set; }

    public bool IsLoading
    {
        get => _isLoading;
        private set => OnPropertyChanged(ref _isLoading, value, () => IsLoading);
    }

    public int TotalCount
    {
        get => _totalCount;
        set => OnPropertyChanged(ref _totalCount, value, () => TotalCount);
    }

    public int LoadedCount => NavigatorList.Count;

    public string Status
    {
        get => _status;
        set => OnPropertyChanged(ref _status, value, () => Status);
    }

    public string StatusToolTip
    {
        get => _statusToolTip;
        set => OnPropertyChanged(ref _statusToolTip, value, () => StatusToolTip);
    }

    public bool ShowPreview => Settings.ShowPreviewOnSelect;

    public IEnumerable<INavigatorListEntry> SelectedEntries => _view.SelectedEntries;

    public IList SelectedItems => _view.SelectedItems;

    public FileInfoVM FileInfoVM { get; } = new();

    public INavigatorListEntry? SelectedItem => _view.ListBoxElement.SelectedItem as INavigatorListEntry;

    #endregion Properties

    #region Commands

    public ICommand ShuffleCommand { get; set; }

    public ICommand ReverseCommand { get; set; }

    public ICommand ZoomInCommand { get; set; }

    public ICommand ZoomOutCommand { get; set; }

    public ICommand ZoomToRowElementsCommand { get; set; }

    public ICommand LoadPreviewsCommand { get; set; }

    public ICommand RemoveImageCommand { get; set; }

    public ICommand CopyCommand { get; set; }

    public ICommand OpenFileCommand { get; set; }

    #endregion Commands

    #region Methods

    private void InitializeCommands()
    {
        ShuffleCommand = new RelayCommand(_ => ShuffleNavigatorList());

        ReverseCommand = new RelayCommand(_ => ReverseNavigatorList());

        ZoomInCommand = new RelayCommand(_ =>
        {
            _previewSize = Convert.ToInt32(Math.Floor(_previewSize * 1.1));
            UpdatePreviews();
        });

        ZoomOutCommand = new RelayCommand(_ =>
        {
            if (_previewSize < 64)
            {
                return;
            }
            _previewSize = Convert.ToInt32(Math.Floor(_previewSize * 0.9));
            UpdatePreviews();
        });

        ZoomToRowElementsCommand = new RelayCommand(x =>
        {
            var toElements = int.Parse(x as string);

            var boxSize = _view.ViewPortWidth / toElements;

            _previewSize = (int)(boxSize - 30);

            UpdatePreviews();
        });

        LoadPreviewsCommand = new RelayCommand(_ => LoadPreviews());
        RemoveImageCommand = new RelayCommand(o => RemoveImage(o));

        CopyCommand = new RelayCommand(CopySelected);

        OpenFileCommand = new RelayCommand<INavigatorListEntry>(x => OpenFile(x));
    }

    private void OpenFile(INavigatorListEntry navigatorListEntry)
    {
        switch (navigatorListEntry)
        {
            case ImageEntryVM image:
                _imoutoViewerService.OpenFile(
                    image.Path,
                    CollectionManager.SelectedCollection.Id,
                    TagSearchVM.SelectedBindedTags.Select(x => x.Model));
                break;
            case VideoEntryVM video:
            {
                video.Pause();
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(video.Path)
                    {
                        UseShellExecute = true
                    }
                };

                process.Start();
                break;
            }
            default:
                Debug.WriteLine("Can't open unsupported entry type" + navigatorListEntry.GetType().FullName);
                break;
        }
    }

    private void ShuffleNavigatorList()
    {
        lock (NavigatorList)
        {
            var newCollection = NavigatorList.ToList();
            Shuffle(newCollection);

            NavigatorList.Clear();

            foreach (var navigatorListEntry in newCollection)
            {
                NavigatorList.Add(navigatorListEntry);
            }
        }
    }

    private void ReverseNavigatorList()
    {
        lock (NavigatorList)
        {
            var newCollection = NavigatorList.ToList();
            for (int i = 0; i < newCollection.Count / 2; i++)
            {
                // swap newCollection[i] and newCollection[newCollection.Count - 1 - i]
                (newCollection[i], newCollection[newCollection.Count - 1 - i])
                    = (newCollection[newCollection.Count - 1 - i], newCollection[i]);
            }

            NavigatorList.Clear();
            foreach (var navigatorListEntry in newCollection)
            {
                NavigatorList.Add(navigatorListEntry);
            }
        }
    }

    private static void Shuffle<T>(IList<T> list)
    {
        var randomGenerator = new Random();

        var count = list.Count;

        while (count > 1)
        {
            count--;

            var k = randomGenerator.Next(count + 1);
            (list[k], list[count]) = (list[count], list[k]);
        }
    }

    public void SetStatusError(string error, string message)
    {
        Status = error;
        StatusToolTip = message;
    }

    private async Task Reload()
    {
        try
        {
            await _fileLoadingService.LoadFiles(
                10_000,
                _previewSize,
                TagSearchVM.SelectedCollection.Value,
                TagSearchVM.SelectedBindedTags.Select(x => x.Model).ToList(),
                x => TotalCount = x,
                (x, ct) =>
                {
                    IsLoading = false;
                    foreach (var navigatorListEntry in x)
                    {
                        ct.ThrowIfCancellationRequested();
                        NavigatorList.Add(navigatorListEntry);
                    }
                },
                () =>
                {
                    TotalCount = 0;
                    NavigatorList.Clear();
                    IsLoading = false;
                },
                () =>
                {
                    _appendNewContentTimer.Stop();
                    IsLoading = true;
                    NavigatorList.Clear();

                },
                () =>
                {
                    IsLoading = false;
                    _appendNewContentTimer.Start();
                });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Can't load images from collection: " + ex.Message);
            SetStatusError("Can't load images from collection", ex.Message);
        }
    }

    private void UpdatePreviews()
    {
        OnPropertyChanged(() => SlotSize);

        //Performance ?
        lock (NavigatorList)
        {
            foreach (var imageEntry in NavigatorList)
            {
                imageEntry.UpdatePreview(PreviewSize);
            }
        }

        LoadPreviews();
    }

    private void LoadPreviews()
    {
        ImageEntry.PreviewLoadingThreadQueue.ClearQueue();

        foreach (var listEntry in _view.VisibleItems)
        {
            listEntry.Load();
        }
    }

    private async void RemoveImage(object o)
    {
        var selectedItem = o as INavigatorListEntry;

        var dbId = selectedItem?.DbId;

        if (dbId == null)
        {
            return;
        }

        var mySettings = new MetroDialogSettings
        {
            AffirmativeButtonText = "Yes",
            NegativeButtonText = "No",
            ColorScheme = MetroDialogColorScheme.Accented
        };
        var result = await _view.ShowMessageDialog(
            "Remove Element",
            "Are you sure you want to remove this item from collection?",
            MessageDialogStyle.AffirmativeAndNegative,
            mySettings);

        if (result != MessageDialogResult.Affirmative)
            return;

        await _fileService.RemoveFile(dbId.Value);

        NavigatorList.Remove(selectedItem!);

        await _view.ShowMessageDialog(
            "Remove Element",
            "Element successfully removed.",
            MessageDialogStyle.Affirmative,
            new MetroDialogSettings
            {
                ColorScheme = MetroDialogColorScheme.Accented
            });
    }

    private void CopySelected(object o)
    {
        var lastItems = SelectedEntries.Select(x => x.Path).ToArray();
        if (!lastItems.Any())
        {
            return;
        }

        var fileCollection = new StringCollection();
        fileCollection.AddRange(lastItems);
        Clipboard.SetFileDropList(fileCollection);
    }

    #endregion Methods

    #region Event handlers

    private async void _view_Loaded(object sender, RoutedEventArgs e) => await Reload();

    private async void TagSearchVM_SelectedTagsUpdated(object sender, EventArgs e) => await Reload();

    private async void TagSearchVMOnSelectedCollectionChanged(object sender, EventArgs eventArgs) => await Reload();

    private void Settings_ShowPreviewOnSelectChanged(object sender, EventArgs e) => OnPropertyChanged(() => ShowPreview);

    #endregion Event handlers
}
