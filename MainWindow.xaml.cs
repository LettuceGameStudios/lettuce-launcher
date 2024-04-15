using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace GameLauncher
{
    public partial class MainWindow : Window
    {
        private const string GameExePath = @"C:\Program Files\mGBA\mGBA.exe";
        private const string UpdateFileUrl = "https://lettucegamestudios.github.io/humans.txt";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                string updateFileContent = await httpClient.GetStringAsync(UpdateFileUrl);
                if (updateFileContent != "1.0.0") // Replace with the current version of your game
                {
                    await DownloadUpdateAsync(updateFileContent);
                }
                else
                {
                    UpdateMessageTextBlock.Text = "No updates available.";
                }
            }
            catch (Exception ex)
            {
                UpdateMessageTextBlock.Text = $"Error checking for updates: {ex.Message}";
            }
        }

        private async Task DownloadUpdateAsync(string version)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(UpdateFileUrl);
                if (response.IsSuccessStatusCode)
                {
                    string updateFilePath = Path.Combine(Path.GetDirectoryName(GameExePath), $"update_{version}.zip");
                    using (FileStream fileStream = File.Create(updateFilePath))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                    await ExtractUpdateAsync(updateFilePath);
                    UpdateMessageTextBlock.Text = "Update downloaded and installed successfully.";
                }
                else
                {
                    UpdateMessageTextBlock.Text = $"Error downloading update: {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                UpdateMessageTextBlock.Text = $"Error downloading update: {ex.Message}";
            }
        }

        private async Task ExtractUpdateAsync(string updateFilePath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(updateFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string destinationPath = Path.Combine(Path.GetDirectoryName(GameExePath), entry.FullName);
                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }

                    using (FileStream fileStream = File.Create(destinationPath))
                    {
                        await entry.Open().CopyToAsync(fileStream);
                    }
                }
            }
        }

        private void StartGameButton_Click(object sender,RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(GameExePath) { UseShellExecute = true });
        }
    }
}