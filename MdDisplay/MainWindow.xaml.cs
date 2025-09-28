using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace MdDisplay
{
  public partial class MainWindow : Window
  {
    private const string SettingsFile = "settings.xml";

    public MainWindow()
    {
      InitializeComponent();
      LoadSettings();
    }

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
      var openFileDialog = new Microsoft.Win32.OpenFileDialog
      {
        Filter = "Fichiers Markdown (*.md)|*.md|Tous les fichiers (*.*)|*.*",
        FilterIndex = 1
      };

      if (openFileDialog.ShowDialog() == true)
      {
        txtFilePath.Text = openFileDialog.FileName;
        DisplayMarkdown(openFileDialog.FileName);
        SaveSettings();
      }
    }

    private void DisplayMarkdown(string filePath)
    {
      if (File.Exists(filePath))
      {
        try
        {
          string markdown = File.ReadAllText(filePath);
          string html = Markdig.Markdown.ToHtml(markdown);
          string htmlContent = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta http-equiv='X-UA-Compatible' content='IE=edge' />
                            <style>
                                body {{ 
                                    font-family: 'Segoe UI', Arial, sans-serif; 
                                    padding: 20px; 
                                    line-height: 1.6;
                                }}
                                pre {{ 
                                    background-color: #f5f5f5; 
                                    padding: 10px; 
                                    border-radius: 3px; 
                                    overflow-x: auto;
                                }}
                                code {{ 
                                    font-family: 'Consolas', monospace; 
                                }}
                            </style>
                        </head>
                        <body>
                            {html}
                        </body>
                        </html>";

          webBrowser.NavigateToString(htmlContent);
        }
        catch (Exception exception)
        {
          MessageBox.Show($"Erreur lors de la lecture du fichier : {exception.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }

    private void SaveSettings()
    {
      try
      {
        var settings = new Settings
        {
          WindowLeft = Left,
          WindowTop = Top,
          WindowWidth = Width,
          WindowHeight = Height,
          FilePath = txtFilePath.Text
        };

        using (var writer = new StreamWriter(SettingsFile))
        {
          var serializer = new XmlSerializer(typeof(Settings));
          serializer.Serialize(writer, settings);
        }
      }
      catch (Exception exception)
      {
        MessageBox.Show($"Erreur lors de la sauvegarde des paramètres : {exception.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
    }

    private void LoadSettings()
    {
      if (File.Exists(SettingsFile))
      {
        try
        {
          using (var reader = new StreamReader(SettingsFile))
          {
            var serializer = new XmlSerializer(typeof(Settings));
            if (serializer.Deserialize(reader) is Settings settings)
            {
              Left = settings.WindowLeft;
              Top = settings.WindowTop;
              Width = settings.WindowWidth;
              Height = settings.WindowHeight;

              if (!string.IsNullOrEmpty(settings.FilePath) && File.Exists(settings.FilePath))
              {
                txtFilePath.Text = settings.FilePath;
                DisplayMarkdown(settings.FilePath);
              }
            }
          }
        }
        catch (Exception exception)
        {
          MessageBox.Show($"Erreur lors du chargement des paramètres : {exception.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
      }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      SaveSettings();
    }
  }

  [Serializable]
  public class Settings
  {
    public double WindowLeft { get; set; }
    public double WindowTop { get; set; }
    public double WindowWidth { get; set; }
    public double WindowHeight { get; set; }
    public string FilePath { get; set; }
  }
}