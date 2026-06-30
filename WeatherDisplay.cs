#nullable disable
using MelonLoader;
using UnityEngine;
using Il2CppTLD.Stats;
using System;
using System.Text.RegularExpressions;

[assembly: MelonInfo(typeof(WeatherMod.WeatherModMain), "WeatherDisplayMod", "1.0.1", "hzb1130")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace WeatherMod
{
    public class WeatherModMain : MelonMod
    {
        private GUIStyle textStyle;
        private WeatherTransition cachedTransition;

        private bool dragging1;
        private Vector2 dragOffset;

        private float lastUpdateTime;

        private string cachedWeather = "--";
        private string cachedWind = "";

        // =========================================================
        // 语言：0 EN / 1 CN / 2 FR / 3 RU
        // =========================================================
        private static readonly string[][] WeatherLang =
        {
            new[] { "Dense Fog", "浓雾", "Brouillard dense", "Густой туман" },// DenseFog
            new[] { "Light Snow", "小雪", "Neige légère", "Лёгкий снег" },// LightSnow
            new[] { "Heavy Snow", "大雪", "Neige forte", "Сильный снег" },// HeavySnow
            new[] { "Partly Cloudy", "多云", "Partiellement nuageux", "Переменная облачность" },// PartlyCloudy
            new[] { "Clear", "晴天", "Dégagé", "Ясно" },// Clear
            new[] { "Cloudy", "阴天", "Couvert", "Облачно" },// Cloudy
            new[] { "Light Fog", "薄雾", "Brouillard léger", "Лёгкий туман" },// LightFog
            new[] { "Blizzard", "暴雪", "Blizzard", "Метель" },// Blizzard
            new[] { "Aurora", "极光", "Aurore", "Северное сияние" },// ClearAurora
            new[] { "Toxic Fog", "毒雾", "Brouillard toxique", "Токсичный туман" },// ToxicFog
            new[] { "Electric Fog", "电雾", "Brouillard électrique", "Электрический туман" }// ElectrostaticFog
        };

        private static readonly string[][] WindLang =
        {
            new[] { "Calm", "无风", "Calme", "Штиль" },
            new[] { "Slightly Windy", "微风", "Vent léger", "Лёгкий ветер" },
            new[] { "Windy", "有风", "Venteux", "Ветрено" }, 
            new[] { "Very Windy", "大风", "Vent fort", "Сильный ветер" },
            new[] { "Blizzard", "暴风", "Blizzard", "Метель" }
        };

        private static readonly string[] CountdownLang =
        {
            "Remaining",
            "剩余",
            "Restant",
            "Осталось"
        };

        private static readonly string[] ChangingSoonLang =
        {
            "Changing soon",
            "即将变化",
            "Changement imminent",
            "Скоро сменится"
        };

        // =========================================================
        public override void OnInitializeMelon()
        {
            Settings.OnLoad();
        }

        // =========================================================
        public override void OnUpdate()
        {
            if (!Settings.options.enableHUD)
                return;

            float interval = Mathf.Max(0.5f, Settings.options.weatherReadInterval);

            if (Time.time - lastUpdateTime >= interval)
            {
                lastUpdateTime = Time.time;

                cachedWeather = GetWeatherRaw();
                cachedWind = GetWindRaw();
            }
        }

        // =========================================================
        public override void OnGUI()
        {
            if (!Settings.options.enableHUD)
                return;
            if (!ShouldShowByScene())
                return;

            EnsureStyle();
            DrawHUD();
        }

        // =========================================================
        private bool ShouldShowByScene()
        {
            int mode = Settings.options.sceneMode;

            // 全部：直接显示
            if (mode == 2)
                return true;

            var weather = GameManager.GetWeatherComponent();
            if (weather == null)
                return true;

            bool isOutdoors = !weather.IsIndoorEnvironment()
                        && !weather.IsIndoorScene();

            // 室内模式
            if (mode == 0)
                return !isOutdoors;

            // 室外模式
            if (mode == 1)
                return isOutdoors;

            return true;
        }

        // =========================================================
        private void EnsureStyle()
        {
            if (textStyle == null)
                textStyle = new GUIStyle(GUI.skin.label);

            textStyle.fontSize = Settings.options.fontSize;
            textStyle.normal.textColor = Settings.options.GetFontColor();
            textStyle.wordWrap = false;
        }

        // =========================================================
        private void DrawHUD()
        {
            string text = BuildText();

            // ✔ 三个都关闭 → 不显示
            if (string.IsNullOrWhiteSpace(text))
                return;

            Vector2 textSize = textStyle.CalcSize(new GUIContent(text));

            float padX = 12f;
            float padY = 10f;

            float width = textSize.x + padX * 2;
            float height = textSize.y + padY * 2;

            Rect rect = new Rect(
                Settings.options.hudX,
                Settings.options.hudY,
                width,
                height
            );

            GUI.color = new Color(0f, 0f, 0f, Settings.options.backgroundAlpha);
            GUI.Box(rect, GUIContent.none);

            GUI.color = Color.white;

            GUI.Label(
                new Rect(
                    rect.x + padX,
                    rect.y + padY,
                    textSize.x,
                    textSize.y
                ),
                text,
                textStyle
            );

            HandleDrag(rect);
        }

        // =========================================================
        // ✔ 关键修复：统一语言系统（不再 useEnglish）
        // =========================================================
        private int GetLangIndexSafe()
        {
            return Settings.options.language; // ✔ 统一入口
        }

        // =========================================================
        private string BuildText()
        {
            string weather = Settings.options.showWeather ? cachedWeather + " " : "";
            string wind = Settings.options.showWind ? cachedWind + " " : "";
            string countdown = Settings.options.showCountdown ? GetCountdown() + " " : "";

            return (weather + wind + countdown).TrimEnd();
        }

        // =========================================================
        private string GetWeatherRaw()
        {
            var w = GameManager.GetWeatherComponent();
            if (w == null) return "";

            int lang = GetLangIndexSafe();

            return w.GetWeatherStage() switch
            {
                WeatherStage.DenseFog => WeatherLang[0][lang],
                WeatherStage.LightSnow => WeatherLang[1][lang],
                WeatherStage.HeavySnow => WeatherLang[2][lang],
                WeatherStage.PartlyCloudy => WeatherLang[3][lang],
                WeatherStage.Clear => WeatherLang[4][lang],
                WeatherStage.Cloudy => WeatherLang[5][lang],
                WeatherStage.LightFog => WeatherLang[6][lang],
                WeatherStage.Blizzard => WeatherLang[7][lang],
                WeatherStage.ClearAurora => WeatherLang[8][lang],
                WeatherStage.ToxicFog => WeatherLang[9][lang],
                WeatherStage.ElectrostaticFog => WeatherLang[10][lang],
                _ => ""
            };
        }

        // =========================================================
        private string GetWindRaw()
        {
            var wind = GameManager.GetWindComponent();
            if (wind == null) return "";

            int lang = GetLangIndexSafe();

            return wind.GetStrength() switch
            {
                WindStrength.Calm => WindLang[0][lang],
                WindStrength.SlightlyWindy => WindLang[1][lang],
                WindStrength.Windy => WindLang[2][lang],
                WindStrength.VeryWindy => WindLang[3][lang],
                WindStrength.Blizzard => WindLang[4][lang],
                _ => ""
            };
        }

        // =========================================================
        private string GetCountdown()
        {
            var wt = GetTransition();
            if (wt == null) return "";

            string debug = wt.GetDebugString();
            var lines = debug.Split('\n');

            foreach (var line in lines)
            {
                if (!line.Contains(" >> "))
                    continue;

                var m = Regex.Match(line, @"(\d+\.?\d*)/(\d+\.?\d*)\s*hrs");
                if (!m.Success)
                    continue;

                float elapsed = float.Parse(m.Groups[1].Value);
                float total = float.Parse(m.Groups[2].Value);

                float remain = total - elapsed;

                int lang = GetLangIndexSafe();

                if (remain <= 0.01f)
                    return ChangingSoonLang[lang];

                int h = (int)remain;
                int min = (int)((remain - h) * 60);

                return $"{CountdownLang[lang]}:{h}:{min:D2}";
            }

            return "";
        }

        // =========================================================
        private WeatherTransition GetTransition()
        {
            if (cachedTransition == null)
                cachedTransition = UnityEngine.Object.FindObjectOfType<WeatherTransition>();

            return cachedTransition;
        }

        // =========================================================
        private void HandleDrag(Rect rect)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                dragging1 = true;
                dragOffset = e.mousePosition - new Vector2(Settings.options.hudX, Settings.options.hudY);
            }

            if (e.type == EventType.MouseDrag && dragging1)
            {
                Settings.options.hudX = (int)(e.mousePosition.x - dragOffset.x);
                Settings.options.hudY = (int)(e.mousePosition.y - dragOffset.y);
            }

            if (e.type == EventType.MouseUp)
            {
                if (dragging1) Settings.options.Save();
                dragging1 = false;
            }
        }
    }
}