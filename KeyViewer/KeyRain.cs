using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace KeyViewer
{
    public partial class KeyRain : MonoBehaviour
    {
        private bool stretching;
        internal Image image;
        private Key key;
        private RectTransform rt;
        private bool initialized = false;

        private static readonly string[] dirNames = new string[4] { "Up", "Down", "Left", "Right" };
        private static readonly Direction[] dirValues = (Direction[])Enum.GetValues(typeof(Direction));

        public bool IsAlive { get; private set; }

        private RainConfig config => key.config.RainConfig;

        private void Awake()
        {
            image = base.gameObject.AddComponent<Image>();
            rt = image.rectTransform;
            rt.anchoredPosition = Vector2.zero;
        }

        public void Init(Key key)
        {
            this.key = key;
            if (File.Exists(config.RainImage))
            {
                image.sprite = Main.GetSprite(config.RainImage);
            }
            image.color = config.RainColor;
            ResetSizePos();
        }

        public void Press()
        {
            stretching = true;
        }

        public void ResetSizePos()
        {
            rt.sizeDelta = GetInitialSize();
            rt.anchoredPosition = GetPosition(config.Direction);
        }

        public void Release()
        {
            stretching = false;
        }

        private void Update()
        {
            IsAlive = IsVisible(config.Direction);
            if (IsAlive)
            {
                Vector2 delta = GetDelta(config.Direction, Time.deltaTime * config.RainSpeed);
                Vector2 sizeDelta = rt.sizeDelta;
                if (stretching)
                {
                    Vector2 zero = Vector2.zero;
                    if ((double)delta.x != 0.0)
                    {
                        zero.x += Mathf.Abs(delta.x);
                    }
                    else if ((double)delta.y != 0.0)
                    {
                        zero.y += Mathf.Abs(delta.y);
                    }
                    rt.sizeDelta = sizeDelta + zero;
                    rt.anchoredPosition += delta * 0.5f;
                }
                else
                {
                    rt.anchoredPosition += delta;
                }
            }
            else
            {
                stretching = false;
                ResetSizePos();
                base.gameObject.SetActive(value: false);
            }
        }

        internal static bool DrawConfigGUI(KeyCode code, RainConfig config)
        {
            bool flag = false;
            float num = MoreGUILayout.NamedSlider("Offset X", config.OffsetX, -100f, 100f, 300f);
            bool flag2;
            if (flag2 = flag | ((double)num != (double)config.OffsetX))
            {
                config.OffsetX = num;
            }
            float num2 = MoreGUILayout.NamedSlider("Offset Y", config.OffsetY, -100f, 100f, 300f);
            bool flag3;
            if (flag3 = flag2 | ((double)num2 != (double)config.OffsetY))
            {
                config.OffsetY = num2;
            }
            float num3 = MoreGUILayout.NamedSlider("Rain Speed", config.RainSpeed, 0f, 1000f, 300f);
            bool flag4;
            if (flag4 = flag3 | ((double)num3 != (double)config.RainSpeed))
            {
                config.RainSpeed = num3;
            }
            float num4 = MoreGUILayout.NamedSlider("Rain Length", config.RainLength, 0f, 1000f, 300f);
            bool flag5;
            if (flag5 = flag4 | ((double)num4 != (double)config.RainLength))
            {
                config.RainLength = num4;
            }
            float num5 = MoreGUILayout.NamedSlider("Rain Width", config.RainWidth, -1f, 1000f, 300f);
            bool flag6;
            if (flag6 = flag5 | ((double)num5 != (double)config.RainWidth))
            {
                config.RainWidth = num5;
            }
            float num6 = MoreGUILayout.NamedSlider("Rain Height", config.RainHeight, -1f, 1000f, 300f);
            bool flag7;
            if (flag7 = flag6 | ((double)num6 != (double)config.RainHeight))
            {
                config.RainHeight = num6;
            }
            float num7 = MoreGUILayout.NamedSlider("Rain Pool Size", config.RainPoolSize, 1f, 100f, 300f, 1f);
            if ((double)num7 != (double)config.RainPoolSize)
            {
                config.RainPoolSize = (int)num7;
                Main.KeyManager.UpdateKeys();
            }
            float num8 = MoreGUILayout.NamedSlider("Disappear Softness", config.Softness, 0f, 500f, 300f, 1f);
            bool flag8;
            if (flag8 = flag7 | ((double)num8 != (double)config.Softness))
            {
                config.Softness = (int)num8;
            }
            if (config.ColorExpanded = GUILayout.Toggle(config.ColorExpanded, "Rain Color"))
            {
                MoreGUILayout.BeginIndent();
                Color color = MoreGUILayout.ColorRgbaSliders(config.RainColor);
                if (flag8 |= color != config.RainColor)
                {
                    config.RainColor = color;
                }
                string text = ColorUtility.ToHtmlStringRGBA(config.RainColor);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Hex");
                GUILayoutOption[] options = Array.Empty<GUILayoutOption>();
                if (ColorUtility.TryParseHtmlString(GUILayout.TextField(text, options), out var color2))
                {
                    config.RainColor = color2;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                MoreGUILayout.EndIndent();
            }
            string text2 = MoreGUILayout.NamedTextField("Rain Image", config.RainImage);
            bool result;
            if (result = flag8 | (text2 != config.RainImage))
            {
                config.RainImage = text2;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Rain Direction");
            if (DrawDirection($"{code} Rain Direction", ref config.Direction))
            {
                Main.KeyManager.UpdateKeys();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return result;
        }

        private bool IsVisible(Direction dir)
        {
            return dir switch
            {
                Direction.U => (double)rt.anchoredPosition.y - (double)rt.sizeDelta.y <= (double)config.RainLength,
                Direction.D => 0.0 - (double)rt.anchoredPosition.y - (double)rt.sizeDelta.y <= (double)config.RainLength,
                Direction.L => 0.0 - (double)rt.anchoredPosition.x - (double)rt.sizeDelta.x <= (double)config.RainLength,
                Direction.R => (double)rt.anchoredPosition.x - (double)rt.sizeDelta.x <= (double)config.RainLength,
                _ => false,
            };
        }

        private Vector2 GetInitialSize()
        {
            switch (config.Direction)
            {
                case Direction.U:
                case Direction.D:
                    return ((double)config.RainWidth <= 0.0) ? new Vector2(key.config.Width, 0f) : new Vector2(config.RainWidth, 0f);
                case Direction.L:
                case Direction.R:
                    {
                        int num = (key.config.keyManager.Profile.ShowKeyPressTotal ? 50 : 0) + 5;
                        return ((double)config.RainHeight <= 0.0) ? new Vector2(0f, key.config.Height + (float)num) : new Vector2(0f, config.RainHeight);
                    }
                default:
                    return Vector2.zero;
            }
        }

        private Vector2 GetDelta(Direction dir, float value)
        {
            return dir switch
            {
                Direction.U => new Vector2(0f, value),
                Direction.D => new Vector2(0f, 0f - value),
                Direction.L => new Vector2(0f - value, 0f),
                Direction.R => new Vector2(value, 0f),
                _ => Vector2.zero,
            };
        }

        private Vector2 GetPosition(Direction dir)
        {
            Vector2 sizeDelta = key.rainMaskRt.sizeDelta;
            return dir switch
            {
                Direction.U => new Vector2(0f, (float)((0.0 - (double)sizeDelta.y) / 2.0) + (float)config.Softness),
                Direction.D => new Vector2(0f, sizeDelta.y / 2f - (float)config.Softness),
                Direction.L => new Vector2(sizeDelta.x / 2f - (float)config.Softness, 0f),
                Direction.R => new Vector2((float)((0.0 - (double)sizeDelta.x) / 2.0) + (float)config.Softness, 0f),
                _ => Vector2.zero,
            };
        }

        private static bool DrawDirection(string title, ref Direction direction)
        {
            int selected = Array.IndexOf(dirValues, direction);
            int num = (UnityModManager.UI.PopupToggleGroup(ref selected, dirNames, title, null) ? 1 : 0);
            direction = dirValues[selected];
            return num != 0;
        }
    }

}
