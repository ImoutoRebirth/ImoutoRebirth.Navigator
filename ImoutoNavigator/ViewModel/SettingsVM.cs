﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Imouto.Navigator.Commands;
using Imouto.Navigator.Properties;
using MahApps.Metro;

namespace Imouto.Navigator.ViewModel
{
    class SettingsVM : VMBase
    {
        #region Subclasses

        public class AccentColorMenuData
        {
            public string Name { get; set; }

            public Brush ColorBrush { get; set; }

            public void ChangeAccent()
            {
                var theme = ThemeManager.DetectAppStyle(Application.Current);
                var accent = ThemeManager.GetAccent(Name);
                ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
                Settings.Default.AccentColorName = Name;
            }
        }

        #endregion Subclasses

        #region Fields

        private AccentColorMenuData _selectedAccentColor;
        private int _selectedTheme;
        private ICommand _saveCommand;

        #endregion Fileds

        #region Constructors

        public SettingsVM()
        {
            AccentColors = ThemeManager.Accents.Select(a => new AccentColorMenuData
                                                            {
                                                                Name = a.Name,
                                                                ColorBrush = a.Resources["AccentColorBrush"] as Brush
                                                            }).ToList();
            SelectedAccentColor = AccentColors.First(x => x.Name == Settings.Default.AccentColorName);

            SelectedIndexTheme = Settings.Default.ThemeIndex;

            ShowPreviewOnSelect = Settings.Default.ActivatePreviewOnSelect;
        }

        #endregion Constructors

        #region Properties

        public bool ShowPreviewOnSelect
        {
            get
            {
                return Settings.Default.ActivatePreviewOnSelect;
            }
            set
            {
                Settings.Default.ActivatePreviewOnSelect = value;
                OnPropertyChanged(() => ShowPreviewOnSelect);
                OnShowPreviewOnSelectChanged();
            }
        }

        public List<AccentColorMenuData> AccentColors { get; }

        public AccentColorMenuData SelectedAccentColor
        {
            get
            {
                return _selectedAccentColor;
            }
            set
            {
                _selectedAccentColor = value;
                _selectedAccentColor.ChangeAccent();
            }
        }

        /// <summary>
        ///     Index of the selected theme. 0 - light, 1 - dark.
        /// </summary>
        public int SelectedIndexTheme
        {
            get
            {
                return _selectedTheme;
            }
            set
            {
                _selectedTheme = value;
                var theme = ThemeManager.DetectAppStyle(Application.Current);
                switch (value)
                {
                    case 1:
                        ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, ThemeManager.AppThemes.Last());
                        Settings.Default.ThemeIndex = 1;
                        break;
                    default:
                        ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, ThemeManager.AppThemes.First());
                        Settings.Default.ThemeIndex = 0;
                        break;
                }
            }
        }

        #endregion Properties

        #region Commands

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(x => Save()));
            }
        }

        private void Save()
        {
            Settings.Default.Save();
        }

        #endregion Commands

        #region Events

        public event EventHandler ShowPreviewOnSelectChanged;

        private void OnShowPreviewOnSelectChanged()
        {
            var handler = ShowPreviewOnSelectChanged;
            handler?.Invoke(this, new EventArgs());
        }

        #endregion Events
    }
}
