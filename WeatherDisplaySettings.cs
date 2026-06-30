#nullable disable
using ModSettings;
using System.Reflection;
using UnityEngine;

namespace WeatherMod
{
    internal class WeatherModSettings : JsonModSettings
    {
        // =========================================================
        // 1. 是否开启
        // =========================================================
        [Section("General Settings / 基础设置")]
        [Name("Enable Weather HUD / 启用天气HUD")]
        [Description("Toggle the weather HUD display / 是否开启天气HUD显示")]
        public bool enableHUD = true;


        // =========================================================
        // 2. 显示语言
        // =========================================================
        [Section("Language Settings / 语言设置")]
        [Name("Display Language / 显示语言")]
        [Description("Select the language for HUD text(Translations provided by LLM)/ 选择HUD显示语言")]
        [Choice(
            "English",
            "中文",
            "Français(France)",
            "Русский"
        )]
        public int language = 0;


        // =========================================================
        // 3. 显示内容
        // =========================================================
        [Section("Display Content / 显示内容")]

        [Name("Show Weather / 显示天气")]
        [Description("Display current weather condition / 是否显示当前天气")]
        public bool showWeather = true;

        [Name("Show Wind / 显示风速")]
        [Description("Display current wind strength / 是否显示当前风速")]
        public bool showWind = true;

        [Name("Show Countdown / 显示剩余时间")]
        [Description("Display remaining time until weather change / 是否显示天气变化剩余时间")]
        public bool showCountdown = true;


        // =========================================================
        // 4. 显示条件（三选一）
        // =========================================================
        [Section("Visibility Rules / 显示条件")]

        [Name("Scene Mode / 显示场景")]
        [Description("Choose where the HUD will be visible (Indoor / Outdoor / All) / 选择HUD显示场景（室内/室外/全部）")]
        [Choice("Indoor/室内", "Outdoor/室外", "ALL/全部")]
        public int sceneMode = 2;


        // =========================================================
        // 5. UI设置
        // =========================================================
        [Section("UI Settings / UI设置")]

        [Name("Font Size / 字体大小")]
        [Description("Adjust HUD font size / 调整HUD字体大小")]
        [Slider(12f, 40f)]
        public int fontSize = 20;

        [Name("Font Color R / 字体颜色R")]
        [Description("Red channel of font color (0-255) / 字体颜色红色通道")]
        [Slider(0, 255)]
        public int fontColorR = 255;

        [Name("Font Color G / 字体颜色G")]
        [Description("Green channel of font color (0-255) / 字体颜色绿色通道")]
        [Slider(0, 255)]
        public int fontColorG = 255;

        [Name("Font Color B / 字体颜色B")]
        [Description("Blue channel of font color (0-255) / 字体颜色蓝色通道")]
        [Slider(0, 255)]
        public int fontColorB = 255;

        [Name("Background Alpha / 背景透明度")]
        [Description("HUD background transparency (0 = invisible, 1 = opaque) / 背景透明度（0透明-1不透明）")]
        [Slider(0f, 1f, 11)]
        public float backgroundAlpha = 0.6f;


        // =========================================================
        // 6. HUD位置
        // =========================================================
        [Section("HUD Position / HUD位置")]

        [Name("HUD X Position / HUD横坐标")]
        [Description("Horizontal position of HUD on screen / HUD在屏幕上的横向位置")]
        [Slider(0f, 3000f)]
        public int hudX = 20;

        [Name("HUD Y Position / HUD纵坐标")]
        [Description("Vertical position of HUD on screen / HUD在屏幕上的纵向位置")]
        [Slider(0f, 2000f)]
        public int hudY = 20;


        // =========================================================
        // 7. 读取频率
        // =========================================================
        [Section("Performance / 性能设置")]

        [Name("Weather Update Interval (sec) / 天气读取频率（秒）")]
        [Description("How often weather data is refreshed / 天气数据刷新间隔时间")]
        [Slider(0.1f, 3f, 30)]
        public float weatherReadInterval = 1.0f;
        // =========================================================
        // UI逻辑
        // =========================================================
        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            base.OnChange(field, oldValue, newValue);

            if (field.Name == nameof(enableHUD))
            {
                RefreshAll();
            }
        }

        protected override void OnConfirm()
        {
            base.OnConfirm();
            RefreshAll();
        }

        public void RefreshAll()
        {
            UpdateVisibility();
        }

        // =========================================================
        // UI显示控制
        // =========================================================
        private void UpdateVisibility()
        {
            if (!enableHUD)
            {
                SetAllHidden();
                return;
            }

            // 显示语言
            SetFieldVisible(nameof(language), true);

            // 显示内容
            SetFieldVisible(nameof(showWeather), true);
            SetFieldVisible(nameof(showWind), true);
            SetFieldVisible(nameof(showCountdown), true);

            // 显示条件
            SetFieldVisible(nameof(sceneMode), true);

            // UI设置
            SetFieldVisible(nameof(fontSize), true);
            SetFieldVisible(nameof(fontColorR), true);
            SetFieldVisible(nameof(fontColorG), true);
            SetFieldVisible(nameof(fontColorB), true);
            SetFieldVisible(nameof(backgroundAlpha), true);

            // HUD位置
            SetFieldVisible(nameof(hudX), true);
            SetFieldVisible(nameof(hudY), true);

            // 读取频率
            SetFieldVisible(nameof(weatherReadInterval), true);
        }

        private void SetAllHidden()
        {
            SetFieldVisible(nameof(language), false);

            SetFieldVisible(nameof(showWeather), false);
            SetFieldVisible(nameof(showWind), false);
            SetFieldVisible(nameof(showCountdown), false);

            SetFieldVisible(nameof(sceneMode), false);

            SetFieldVisible(nameof(fontSize), false);
            SetFieldVisible(nameof(fontColorR), false);
            SetFieldVisible(nameof(fontColorG), false);
            SetFieldVisible(nameof(fontColorB), false);
            SetFieldVisible(nameof(backgroundAlpha), false);

            SetFieldVisible(nameof(hudX), false);
            SetFieldVisible(nameof(hudY), false);

            SetFieldVisible(nameof(weatherReadInterval), false);
        }

        // =========================================================
        // 工具函数
        // =========================================================
        public Color GetFontColor()
        {
            return new Color(
                fontColorR / 255f,
                fontColorG / 255f,
                fontColorB / 255f
            );
        }
    }

    // =========================================================
    // Settings 管理
    // =========================================================
    internal static class Settings
    {
        public static WeatherModSettings options;

        public static void OnLoad()
        {
            options = new WeatherModSettings();
            options.AddToModSettings("Weather Display/天气显示");

            options.RefreshAll();
        }
    }
}