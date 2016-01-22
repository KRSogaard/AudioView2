using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using MahApps.Metro;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private bool isInitalizating = false;
        public List<string> Themes { get; set; }
        public List<string> Accents { get; set; }

        private string _theme;
        public string Theme
        {
            get { return _theme; }
            set
            {
                SetProperty(ref _theme, value);
                UpdateSettings();
            }
        }

        private string _accent;
        public string Accent
        {
            get { return _accent; }
            set
            {
                SetProperty(ref _accent, value);
                UpdateSettings();
            }
        }

        public SettingsViewModel()
        {
            Accents = ThemeManager.Accents.Select(x => x.Name).ToList();
            Themes = ThemeManager.AppThemes.Select(x => x.Name).ToList();
            LoadSettings();
        }

        private void UpdateSettings()
        {
            if(isInitalizating)
                return;

            ThemeManager.ChangeAppStyle(Application.Current, 
                ThemeManager.Accents.Where(x=>x.Name ==  Accent).First(), 
                ThemeManager.AppThemes.Where(x => x.Name == Theme).First());
            SaveSettings();
        }

        private void SaveSettings()
        {
            var settingsString = JsonConvert.SerializeObject(new AudioViewSettings()
            {
                Accent = Accent,
                Theme = Theme
            });
            File.WriteAllText("settings.json", settingsString);
        }

        private void LoadSettings()
        {
            if (!File.Exists("settings.json"))
            {
                isInitalizating = true;
                Theme = "BaseDark"; // ThemeManager.Accents.Select(x=>x.Name).First(),
                Accent = "Amber"; // ThemeManager.AppThemes.Select(x => x.Name).First()
                isInitalizating = false;
                UpdateSettings();
                return;
            }

            var settings = JsonConvert.DeserializeObject<AudioViewSettings>(File.ReadAllText("settings.json"));

            isInitalizating = true;
            if (!Themes.Any(x => x == settings.Theme))
            {
                settings.Theme = "BaseDark";
                SaveSettings();
            }
            Theme = settings.Theme;

            if (!Accents.Any(x => x == settings.Accent))
            {
                settings.Accent = "Amber";
                SaveSettings();
            }
            Accent = settings.Accent;

            isInitalizating = false;

            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.Accents.Where(x => x.Name == Accent).First(),
                ThemeManager.AppThemes.Where(x => x.Name == Theme).First());
        }
    }

    public class AudioViewSettings
    {
        public string Accent { get; set; }
        public string Theme { get; set; }
    }
}