﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackSense.Entities;

namespace TrackSense.Configurations
{
    public class ConfigurationManager : IConfigurationManager
    {
        private readonly string _configurationFilePath = Path.Combine(FileSystem.AppDataDirectory, "user-settings.json");
        public event EventHandler ConfigurationChanged;

        public ConfigurationManager()
        {/*
            try
            {
                if (!File.Exists(_configurationFilePath) || true || false)
                {
                    Settings defaultSettings = new Settings()
                    {
                        ApiUrl = "https://tracksense-api.rapidotron.com/api",
                        Username = "batman",
                        Endpoint = "minio.rapidotron.com",
                        AccessKey = "ZUzuRtiSnBktqzWNtSCw",
                        SecretKey = "CD6BbgnuqPPXhXZQdYbh1X3NCxRdtyuOa0aUSPRL"
                    };
                    SaveSettings(defaultSettings);
                }
            }
            catch (PathTooLongException)
            {
                Debug.WriteLine("The path of the file is too long");
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"An error occurred while initializing the configuration: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An unexpected error occurred: {ex.Message}");
            }*/
        }
        public Settings LoadSettings()
        {
            Settings settings = new Settings();
            if (File.Exists(_configurationFilePath))
            {
                string json = System.IO.File.ReadAllText(_configurationFilePath);
                settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(json);
            }
#if DEBUG
            Debug.WriteLine($"Loaded settings: {settings.ApiUrl}, {settings.Username}, {settings.ScreenRotation}, {settings.Endpoint}, {settings.AccessKey}, {settings.SecretKey}");
 #endif
            return settings;
        }

        public void SaveSettings(Settings settings)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
            File.WriteAllText(_configurationFilePath, json);
            Debug.WriteLine($"Saved settings: {settings.ApiUrl}, {settings.Username}, {settings.ScreenRotation}, {settings.Endpoint}, {settings.AccessKey}, {settings.SecretKey}");
            OnConfigurationChanged();
        }

        protected virtual void OnConfigurationChanged()
        {
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
