﻿#nullable enable
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Imouto;
using ImoutoRebirth.Lilin.WebApi.Client;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal static class EntryVMFactory
{
    public static INavigatorListEntry? CreateListEntry(
        string path,
        Size initPreviewSize,
        IImoutoRebirthLilinWebApiClient lilinWebApiClient,
        Guid? dbId = null)
    {
        // todo disk replacement
        path = "Q" + path.Substring(1);

        if (path.IsImage())
            return new ImageEntryVM(path, lilinWebApiClient, initPreviewSize, dbId);

        if (!FileExists(path))
            return null;

        if (path.IsVideo() || path.EndsWith(".m4v"))
            return new VideoEntryVM(path, lilinWebApiClient, initPreviewSize, dbId);

        if (path.EndsWith(".zip"))
            return new UgoiraEntryVM(path, lilinWebApiClient, initPreviewSize, dbId);

        return null;
    }

    [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool PathFileExists(StringBuilder path);

    private static bool FileExists(string path)
    {
        // A StringBuilder is required for interops calls that use strings
        var builder = new StringBuilder();
        builder.Append(path);
        return PathFileExists(builder);
    }
}