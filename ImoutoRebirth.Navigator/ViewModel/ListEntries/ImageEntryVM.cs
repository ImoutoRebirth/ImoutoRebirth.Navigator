﻿using System.Windows;
using System.Windows.Media.Imaging;
using Imouto;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Navigator.Model;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal class ImageEntryVM : BaseEntryVM, INavigatorListEntry
{
    #region Constructors

    public ImageEntryVM(
        string imagePath,
        IImoutoRebirthLilinWebApiClient lilinWebApiClient,
        Size initPreviewSize,
        Guid? dbId)
        : base(dbId, lilinWebApiClient)
    {
        ImageEntry = new ImageEntry(imagePath, initPreviewSize);
        ImageEntry.ImageChanged += (s, e) =>
        {
            OnPropertyChanged(() => IsLoading);
            OnPropertyChanged(() => Image);
        };

        Type = ImageEntry.ImageFormat switch
        {
            ImageFormat.GIF => ListEntryType.Gif,
            ImageFormat.PNG => ListEntryType.Png,
            _ => ListEntryType.Image
        };

        DbId = dbId;
    }

    #endregion Constructors

    #region Properties

    public bool IsLoading => ImageEntry.IsLoading;

    public BitmapSource Image => ImageEntry.Image;

    public Size ViewPortSize => ImageEntry.ViewPort;

    public ListEntryType Type { get; private set; }

    private ImageEntry ImageEntry { get; }

    public Guid? DbId { get; }

    public string Path => ImageEntry.FullName;

    #endregion Properties

    #region Commands

    #endregion Commands

    #region Public methods

    public void UpdatePreview(Size previewSize)
    {
        ImageEntry.UpdatePreview(previewSize);
        OnPropertyChanged(() => ViewPortSize);
    }

    public async void Load()
    {
        ImageEntry.DoLoadAsyns();
        LoadRating();
    }

    #endregion Public methods

    #region IDragable members

    public object Data => new DataObject(DataFormats.FileDrop, new[]
    {
        ImageEntry.FullName
    });

    public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;

    #endregion IDragable members
}