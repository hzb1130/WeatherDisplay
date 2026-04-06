#nullable disable
using Il2CppTLD.Stats;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using ModSettings;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

[assembly: MelonInfo(typeof(WeatherMod.WeatherModMain), "WeatherDisplayMod", "1.0.0", "hzb1130")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace WeatherMod
{
    // ===================== Settings =====================
    internal class WeatherModSettings : JsonModSettings
    {
        [Section("Weather Display / 天气显示")]
        
        [Name("Show Weather / 显示天气")]
        public bool showWeather = true;
        
        [Name("Language / 语言")]
        [Description("English or Chinese / 英文或中文")]
        public bool useEnglish = false;
        
        [Name("Font Size / 字体大小")]
        [Slider(15f, 30f)]
        public int fontSize = 20;
        
        [Section("Display Position / 显示位置")]
        
        [Name("Weather X Position / 天气显示框X坐标")]
        [Description("Drag the text directly to move it / 可以直接拽动字体位置")]
        [Slider(0f, 3000f)]
        public int weatherX = 20;
        
        [Name("Weather Y Position / 天气显示框Y坐标")]
        [Description("Drag the text directly to move it / 可以直接拽动字体位置")]
        [Slider(0f, 2000f)]
        public int weatherY = 20;

        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            base.OnChange(field, oldValue, newValue);
            
            // 如果不显示天气，则折叠所有相关设置
            SetFieldVisible(nameof(fontSize), showWeather);
            SetFieldVisible(nameof(weatherX), showWeather);
            SetFieldVisible(nameof(weatherY), showWeather);
            SetFieldVisible(nameof(useEnglish), showWeather);
        }
    }

    // ===================== Settings管理 =====================
    internal static class Settings
    {
        public static WeatherModSettings options;
        public static void OnLoad()
        {
            options = new WeatherModSettings();
            options.AddToModSettings("Weather Display / 天气显示");
        }
    }

    // ===================== 主入口 =====================
    public class WeatherModMain : MelonMod
    {
        private GUIStyle textStyle;
        private bool dragging1;
        private Vector2 dragOffset;
        private WeatherTransition cachedWeatherTransition;
        private int lastFontSize;
        private bool lastLanguage;

        public override void OnInitializeMelon()
        {
            Settings.OnLoad();
            lastFontSize = Settings.options.fontSize;
            lastLanguage = Settings.options.useEnglish;
        }

        public override void OnUpdate()
        {
            // 检查字体大小或语言是否改变
            if (Settings.options.showWeather)
            {
                if (lastFontSize != Settings.options.fontSize)
                {
                    lastFontSize = Settings.options.fontSize;
                    textStyle = null; // 强制重建样式
                }
                
                if (lastLanguage != Settings.options.useEnglish)
                {
                    lastLanguage = Settings.options.useEnglish;
                    // 语言改变时不需要重建样式，但会更新显示文本
                }
            }
        }

        public override void OnGUI()
        {
            if (!Settings.options.showWeather) return;
            if (GUI.skin == null) return;
            
            // 重建样式以应用新字体大小
            if (textStyle == null)
            {
                textStyle = new GUIStyle(GUI.skin.label);
                textStyle.fontSize = Settings.options.fontSize;
                textStyle.normal.textColor = Color.white;
            }
            
            DrawWeatherHUD();
        }

        // ===================== 天气显示 =====================
        private void DrawWeatherHUD()
        {
            if (!Settings.options.showWeather) return;
            
            // 确保样式使用最新字体大小
            if (textStyle != null && textStyle.fontSize != Settings.options.fontSize)
            {
                textStyle.fontSize = Settings.options.fontSize;
            }
            
            string weatherText = GetWeatherText();
            Vector2 textSize = textStyle.CalcSize(new GUIContent(weatherText));
            float padding = 5f;
            float extraWidth = 20f;
            
            Rect rect = new Rect(
                Settings.options.weatherX,
                Settings.options.weatherY,
                textSize.x + padding * 2,
                textSize.y + padding
            );
            
            Color bgColor = new Color(0f, 0f, 0f, 0.65f);
            GUI.color = bgColor;
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;
            
            GUI.Label(new Rect(
                Settings.options.weatherX + padding,
                Settings.options.weatherY + padding,
                textSize.x + extraWidth,
                textSize.y
            ), weatherText, textStyle);
            
            HandleDrag(rect, ref dragging1, ref Settings.options.weatherX, ref Settings.options.weatherY);
        }

        private string GetWeatherText()
        {
            // 天气
            var weather = GameManager.GetWeatherComponent();
            string weatherStr = GetWeatherTextByLang(weather);
            
            // 风等级
            var wind = GameManager.GetWindComponent();
            string windStr = wind != null ? GetWindTextByLang(wind.GetStrength()) : "--";
            
            // 天气变化倒计时
            string countdown = GetWeatherCountdown();
            
            if (Settings.options.useEnglish)
            {
                return $"{weatherStr} {windStr}{countdown}";
            }
            else
            {
                return $"{weatherStr} {windStr}{countdown}";
            }
        }

        private string GetWeatherTextByLang(Weather w)
        {
            if (w == null) return "--";
            
            if (Settings.options.useEnglish)
            {
                return w.GetWeatherStage() switch
                {
                    WeatherStage.DenseFog => "Dense Fog",
                    WeatherStage.LightSnow => "Light Snow",
                    WeatherStage.HeavySnow => "Heavy Snow",
                    WeatherStage.PartlyCloudy => "Partly Cloudy",
                    WeatherStage.Clear => "Clear",
                    WeatherStage.Cloudy => "Cloudy",
                    WeatherStage.LightFog => "Light Fog",
                    WeatherStage.Blizzard => "Blizzard",
                    WeatherStage.ClearAurora => "Aurora",
                    WeatherStage.ToxicFog => "Toxic Fog",
                    WeatherStage.ElectrostaticFog => "Electrostatic Fog",
                    _ => "Unknown"
                };
            }
            else
            {
                return w.GetWeatherStage() switch
                {
                    WeatherStage.DenseFog => "浓雾",
                    WeatherStage.LightSnow => "小雪",
                    WeatherStage.HeavySnow => "大雪",
                    WeatherStage.PartlyCloudy => "多云",
                    WeatherStage.Clear => "晴天",
                    WeatherStage.Cloudy => "阴天",
                    WeatherStage.LightFog => "薄雾",
                    WeatherStage.Blizzard => "暴雪",
                    WeatherStage.ClearAurora => "极光",
                    WeatherStage.ToxicFog => "毒雾",
                    WeatherStage.ElectrostaticFog => "电子雾",
                    _ => "未知"
                };
            }
        }

        private string GetWindTextByLang(WindStrength w)
        {
            if (Settings.options.useEnglish)
            {
                return w switch
                {
                    WindStrength.Calm => "Calm",
                    WindStrength.SlightlyWindy => "Light Wind",
                    WindStrength.Windy => "Windy",
                    WindStrength.VeryWindy => "Strong Wind",
                    WindStrength.Blizzard => "Gale",
                    _ => "Unknown"
                };
            }
            else
            {
                return w switch
                {
                    WindStrength.Calm => "无风",
                    WindStrength.SlightlyWindy => "微风",
                    WindStrength.Windy => "有风",
                    WindStrength.VeryWindy => "大风",
                    WindStrength.Blizzard => "暴风",
                    _ => "未知"
                };
            }
        }

        private string GetWeatherCountdown()
        {
            var wt = GetWeatherTransition();
            if (wt == null) return "";
            
            string debug = wt.GetDebugString();
            var lines = debug.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                if (line.Contains(" >> "))
                {
                    var match = Regex.Match(line, @"(\d+\.?\d*)/(\d+\.?\d*)\s*hrs");
                    if (match.Success)
                    {
                        float elapsed = float.Parse(match.Groups[1].Value);
                        float total = float.Parse(match.Groups[2].Value);
                        float remaining = total - elapsed;
                        
                        if (remaining > 0.01f)
                        {
                            int hours = (int)remaining;
                            int minutes = (int)((remaining - hours) * 60);
                            
                            if (Settings.options.useEnglish)
                            {
                                return $" Remaining:{hours}:{minutes:D2}";
                            }
                            else
                            {
                                return $" 剩余:{hours}:{minutes:D2}";
                            }
                        }
                        else
                        {
                            return Settings.options.useEnglish ? " Changing soon" : " 即将变化";
                        }
                    }
                    break;
                }
            }
            return "";
        }

        private WeatherTransition GetWeatherTransition()
        {
            if (cachedWeatherTransition == null)
            {
                cachedWeatherTransition = UnityEngine.Object.FindObjectOfType<WeatherTransition>();
            }
            return cachedWeatherTransition;
        }

        private void HandleDrag(Rect rect, ref bool dragging, ref int x, ref int y)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                dragging = true;
                dragOffset = e.mousePosition - new Vector2(x, y);
            }
            if (e.type == EventType.MouseDrag && dragging)
            {
                x = (int)(e.mousePosition.x - dragOffset.x);
                y = (int)(e.mousePosition.y - dragOffset.y);
            }
            if (e.type == EventType.MouseUp)
            {
                if (dragging) Settings.options.Save();
                dragging = false;
            }
        }
    }
}