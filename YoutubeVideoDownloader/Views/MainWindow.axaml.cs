using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using YoutubeVideoDownloader.ViewModels;

namespace YoutubeVideoDownloader.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Seleziona cartella di destinazione"
        });

        if (result != null && result.Count > 0)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.PathCartella = result[0].Path.LocalPath;
            }
        }
    }
}