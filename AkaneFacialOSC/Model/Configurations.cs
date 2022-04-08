using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Azw.FacialOsc.View;

namespace Azw.FacialOsc.Model
{
    internal class Configurations : INotifyPropertyChanged
    {
        public Controller? Controller;

        private bool _isDirty = false;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                NotifyPropertyChanged(nameof(IsDirty));
            }
        }
        void SetDirty() { IsDirty = true; }

        public Dictionary<string, string> LanguageList { get; set; } = new Dictionary<string, string>()
        {
            {"en-US",   "English"      },
            {"ja-JP",   "日本語"       },
            {"ko-KR",   "한국어"       },
            {"zh-Hant", "中文（正體）" },
        };

        private string language = CultureInfo.CurrentCulture.Name;
        public string Language
        {
            get { return language; }
            set
            {
                if (language == value) return;
                language = value;
                NotifyPropertyChanged(nameof(Language));
                SetDirty();
                Controller?.LanguageChanged(value);
            }
        }

        public Dictionary<AkaneThemes.Themes, string> ApplicationThemeList { get; set; } = Enum.GetValues(typeof(AkaneThemes.Themes)).Cast<AkaneThemes.Themes>().ToDictionary(t => t, t => t.ToString());

        public AkaneThemes.Themes applicationTheme = AkaneThemes.Themes.AkaneTheme;
        public AkaneThemes.Themes ApplicationTheme
        {
            get { return applicationTheme; }
            set
            {
                if (applicationTheme == value) return;
                applicationTheme = value;
                NotifyPropertyChanged(nameof(ApplicationTheme));
                AkaneThemes.Use(value);
                SetDirty();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PreferencesV2.ApplicationPreference ToPreference()
        {
            return new PreferencesV2.ApplicationPreference()
            {
                language = Language,
                theme = ApplicationTheme.ToString(),
            };
        }
    }
}
