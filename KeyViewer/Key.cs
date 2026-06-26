using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using DG.Tweening;
using KeyViewer.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace KeyViewer
{
    public partial class Key : MonoBehaviour
    {
        public static uint Total;

        private bool initialized;

        private bool layoutUpdated;

        private bool isSpecial;

        private bool prevPressed;

        private Vector2 position;

        private KeyManager keyManager;

        internal Config config;

        internal EnsurePool<KeyRain> rains;

        private KeyRain toRelease;

        internal GameObject rainContainer;

        internal Transform rainContainerTransform;

        internal RectMask2D rainMask;

        internal RectTransform rainMaskRt;

        private static Color? forceBgColor;

        internal static readonly Ease[] eases;

        internal static readonly string[] easeNames;

        internal static readonly string[] keyNames;

        internal static readonly Dictionary<KeyCode, int> codeIndex;

        internal static readonly Dictionary<KeyCode, ushort> KeyCodeToNative;

        private static readonly Dictionary<KeyCode, string> KeyString;

        private static LangManager lang => Main.Lang;

        public uint Count
        {
            get
            {
                return config.Count;
            }
            set
            {
                config.Count = value;
            }
        }

        public KeyCode Code => config.Code;

        public SpecialKeyType SpecialType => config.SpecialType;

        public bool Pressed { get; private set; }

        public Image Outline { get; private set; }

        public string TweenID { get; private set; }

        public Image Background { get; private set; }

        public TextMeshProUGUI Text { get; private set; }

        public TextMeshProUGUI CountText { get; private set; }

        private Vector2 offsetVec => new Vector2(config.OffsetX, config.OffsetY);

        private Profile Profile => keyManager.Profile;

        public Key Init(KeyManager keyManager, Config config)
        {
            this.config = config ?? (config = new Config());
            Count = this.config.Count;
            this.keyManager = keyManager;
            base.transform.SetParent(keyManager.keysCanvas.transform);
            isSpecial = config.SpecialType != SpecialKeyType.None;
            if (!isSpecial)
            {
                rainContainer = new GameObject("Rain Container");
                rainContainerTransform = rainContainer.transform;
                rainContainerTransform.SetParent(base.transform);
                rainContainer.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
                rainMask = rainContainer.AddComponent<RectMask2D>();
                rainMaskRt = rainMask.rectTransform;
                RectTransform rectTransform = rainMaskRt;
                Vector2 anchorMin = (rainMaskRt.anchorMax = Vector2.zero);
                rectTransform.anchorMin = anchorMin;
                rains = new EnsurePool<KeyRain>(delegate
                {
                    GameObject obj = new GameObject($"Rain {Code}");
                    obj.transform.parent = rainMask.transform;
                    KeyRain keyRain = obj.AddComponent<KeyRain>();
                    keyRain.Init(this);
                    return keyRain;
                }, (KeyRain kr) => !kr.IsAlive, delegate (KeyRain kr)
                {
                    kr.gameObject.SetActive(value: true);
                }, UnityEngine.Object.Destroy);
                rains.Fill(config.RainConfig.RainPoolSize);
            }
            GameObject gameObject = new GameObject("Background");
            gameObject.transform.SetParent(base.transform);
            Background = gameObject.AddComponent<Image>();
            Background.sprite = Main.KeyBackground;
            Background.color = config.ReleasedBackgroundColor;
            Background.type = Image.Type.Sliced;
            GameObject gameObject2 = new GameObject("Outline");
            gameObject2.transform.SetParent(base.transform);
            Outline = gameObject2.AddComponent<Image>();
            Outline.sprite = Main.KeyOutline;
            Outline.color = config.ReleasedOutlineColor;
            Outline.type = Image.Type.Sliced;
            GameObject gameObject3 = new GameObject("Text");
            gameObject3.transform.SetParent(base.transform);
            ContentSizeFitter contentSizeFitter = gameObject3.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Text = gameObject3.AddComponent<TextMeshProUGUI>();
            Text.font = FontManager.GetFont(config.Font).fontTMP;
            Text.color = Color.white;
            Text.enableVertexGradient = true;
            Text.alignment = TextAlignmentOptions.Midline;
            if (isSpecial)
            {
                Text.text = SpecialType.ToString();
            }
            else if (config.KeyTitle != null)
            {
                Text.text = config.KeyTitle;
            }
            else
            {
                if (!KeyString.TryGetValue(Code, out var value))
                {
                    value = Code.ToString();
                }
                Text.text = (config.KeyTitle = value);
            }
            GameObject gameObject4 = new GameObject("CountText");
            gameObject4.transform.SetParent(base.transform);
            ContentSizeFitter contentSizeFitter2 = gameObject4.AddComponent<ContentSizeFitter>();
            contentSizeFitter2.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter2.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            CountText = gameObject4.AddComponent<TextMeshProUGUI>();
            CountText.font = FontManager.GetFont(config.Font).fontTMP;
            CountText.color = Color.white;
            CountText.enableVertexGradient = true;
            CountText.alignment = TextAlignmentOptions.Midline;
            switch (SpecialType)
            {
                case SpecialKeyType.None:
                    CountText.text = config.Count.ToString();
                    break;
                case SpecialKeyType.KPS:
                    CountText.text = KPSCalculatorSync.Kps.ToString();
                    break;
                case SpecialKeyType.Total:
                    {
                        TextMeshProUGUI countText = CountText;
                        List<Config> activeKeys = Profile.ActiveKeys;
                        uint num = (Total = activeKeys.Select((Config c) => c.Count).Sum());
                        string text = num.ToString();
                        countText.text = text;
                        break;
                    }
            }
            TweenID = ((!isSpecial) ? $"KeyViewer.{Code}Tween" : $"KeyViewer.{SpecialType}Tween");
            initialized = true;
            return this;
        }

        internal void ForceReleaseKey()
        {
            Pressed = false;
        }

        private void Update()
        {
            if (!initialized || !layoutUpdated)
            {
                return;
            }
            switch (SpecialType)
            {
                case SpecialKeyType.KPS:
                    CountText.text = KPSCalculatorSync.Kps.ToString();
                    return;
                case SpecialKeyType.Total:
                    CountText.text = Total.ToString();
                    return;
            }
            Pressed = InputAPI.GetKey(Code);
            if (config.SpareCode != 0)
            {
                Pressed |= InputAPI.GetKey(config.SpareCode);
            }
            if (Pressed == prevPressed)
            {
                return;
            }
            prevPressed = Pressed;
            if (DOTween.IsTweening(TweenID))
            {
                DOTween.Kill(TweenID);
            }
            if (Pressed)
            {
                CountText.text = (++Count).ToString();
                KPSCalculatorSync.Press();
                Total++;
            }
            Color white = Color.white;
            Vector3 scale = new Vector3(1f, 1f, 1f);
            Color color;
            Color color2;
            VertexGradient colorGradient;
            VertexGradient colorGradient2;
            if (Pressed)
            {
                color = config.PressedBackgroundColor;
                color2 = config.PressedOutlineColor;
                colorGradient = config.PressedTextColor;
                colorGradient2 = config.PressedCountTextColor;
                if (Profile.AnimateKeys)
                {
                    scale *= config.ShrinkFactor;
                }
                if (config.RainEnabled)
                {
                    (toRelease = rains.Get()).Press();
                }
            }
            else
            {
                color = config.ReleasedBackgroundColor;
                color2 = config.ReleasedOutlineColor;
                colorGradient = config.ReleasedTextColor;
                colorGradient2 = config.ReleasedCountTextColor;
                if (config.RainEnabled)
                {
                    toRelease?.Release();
                }
            }
            if (Pressed && forceBgColor.HasValue)
            {
                Background.color = forceBgColor.Value;
                forceBgColor = null;
            }
            else
            {
                Background.color = color;
            }
            Outline.color = color2;
            Text.colorGradient = colorGradient;
            CountText.colorGradient = colorGradient2;
            if (Profile.AnimateKeys)
            {
                Background.rectTransform.DOScale(scale, config.EaseDuration).SetId(TweenID).SetEase(config.Ease)
                    .SetUpdate(isIndependentUpdate: true)
                    .OnKill(delegate
                    {
                        Background.rectTransform.localScale = scale;
                    });
                Outline.rectTransform.DOScale(scale, config.EaseDuration).SetId(TweenID).SetEase(config.Ease)
                    .SetUpdate(isIndependentUpdate: true)
                    .OnKill(delegate
                    {
                        Outline.rectTransform.localScale = scale;
                    });
                Text.rectTransform.DOScale(scale, config.EaseDuration).SetId(TweenID).SetEase(config.Ease)
                    .SetUpdate(isIndependentUpdate: true)
                    .OnKill(delegate
                    {
                        Text.rectTransform.localScale = scale;
                    });
                CountText.rectTransform.DOScale(scale, config.EaseDuration).SetId(TweenID).SetEase(config.Ease)
                    .SetUpdate(isIndependentUpdate: true)
                    .OnKill(delegate
                    {
                        CountText.rectTransform.localScale = scale;
                    });
            }
            else
            {
                Background.rectTransform.localScale = scale;
                Outline.rectTransform.localScale = scale;
                Text.rectTransform.localScale = scale;
                CountText.rectTransform.localScale = scale;
            }
        }

        public void UpdateLayout(ref float x, ref float tempX, int updateCount)
        {
            EventAPI.UpdateLayout(this);
            if (FontManager.TryGetFont(config.Font, out var font))
            {
                Text.font = font.fontTMP;
                CountText.font = font.fontTMP;
            }
            if (isSpecial && Profile.MakeBarSpecialKeys)
            {
                int num = updateCount * 10;
                int count = keyManager.specialKeys.Count;
                Vector2 vector = new Vector2(0f, (float)(75.0 * ((double)config.Height / 100.0)));
                vector.x = (float)(((double)x - 10.0) / (double)count * ((double)config.Width / 100.0)) - (float)(num / 2);
                position = new Vector2(vector.x / 2f + tempX + (float)(num / 2), (float)(0.0 - ((double)config.Height / 2.0 - 10.0)));
                Vector2 vector2 = position + offsetVec;
                tempX += vector.x;
                Background.rectTransform.anchorMin = Vector2.zero;
                Background.rectTransform.anchorMax = Vector2.zero;
                Background.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                Background.rectTransform.sizeDelta = vector;
                Background.rectTransform.anchoredPosition = vector2;
                Outline.rectTransform.anchorMin = Vector2.zero;
                Outline.rectTransform.anchorMax = Vector2.zero;
                Outline.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                Outline.rectTransform.sizeDelta = vector;
                Outline.rectTransform.anchoredPosition = vector2;
                float num2 = config.Height / 5f;
                Text.rectTransform.anchorMin = Vector2.zero;
                Text.rectTransform.anchorMax = Vector2.zero;
                Text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                Text.rectTransform.sizeDelta = vector * new Vector2(1f, vector.y * 1.03f);
                Text.rectTransform.anchoredPosition = vector2 + new Vector2(config.TextOffsetX, config.TextOffsetY + num2);
                Text.fontSize = config.TextFontSize - 15f;
                Text.fontSizeMax = config.TextFontSize - 15f;
                Text.enableAutoSizing = true;
                CountText.rectTransform.anchorMin = Vector2.zero;
                CountText.rectTransform.anchorMax = Vector2.zero;
                CountText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                CountText.rectTransform.sizeDelta = vector * new Vector2(1f, vector.y * 0.8f);
                CountText.rectTransform.anchoredPosition = vector2 + new Vector2(config.CountTextOffsetX, config.CountTextOffsetY - num2);
                CountText.fontSizeMin = 0f;
                CountText.fontSize = config.CountTextFontSize - 5f;
                CountText.fontSizeMax = config.CountTextFontSize - 5f;
                CountText.enableAutoSizing = true;
                CountText.gameObject.SetActive(value: true);
                Background.color = config.ReleasedBackgroundColor;
                Outline.color = config.ReleasedOutlineColor;
                Text.colorGradient = config.ReleasedTextColor;
                CountText.colorGradient = config.ReleasedCountTextColor;
                layoutUpdated = true;
                return;
            }
            float width = config.Width;
            float num3 = config.Height;
            DOTween.Kill(TweenID);
            if (Profile.ShowKeyPressTotal)
            {
                num3 += 50f;
            }
            position = new Vector2(width / 2f + x, num3 / 2f);
            Vector2 vector3 = position + offsetVec;
            Background.rectTransform.anchorMin = Vector2.zero;
            Background.rectTransform.anchorMax = Vector2.zero;
            Background.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            Background.rectTransform.sizeDelta = new Vector2(width, num3);
            Background.rectTransform.anchoredPosition = vector3;
            Outline.rectTransform.anchorMin = Vector2.zero;
            Outline.rectTransform.anchorMax = Vector2.zero;
            Outline.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            Outline.rectTransform.sizeDelta = new Vector2(width, num3);
            Outline.rectTransform.anchoredPosition = vector3;
            float num4 = num3 / 4f;
            Text.rectTransform.anchorMin = Vector2.zero;
            Text.rectTransform.anchorMax = Vector2.zero;
            Text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            Text.rectTransform.sizeDelta = new Vector2(width, num3 * 1.03f);
            if (Profile.ShowKeyPressTotal)
            {
                Text.rectTransform.anchoredPosition = vector3 + new Vector2(config.TextOffsetX, config.TextOffsetY + num4);
            }
            else
            {
                Text.rectTransform.anchoredPosition = vector3 + new Vector2(config.TextOffsetX, config.TextOffsetY);
            }
            Text.fontSize = config.TextFontSize;
            Text.fontSizeMax = config.TextFontSize;
            Text.enableAutoSizing = true;
            CountText.rectTransform.anchorMin = Vector2.zero;
            CountText.rectTransform.anchorMax = Vector2.zero;
            CountText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            CountText.rectTransform.sizeDelta = new Vector2(width, num3 * 0.8f);
            CountText.rectTransform.anchoredPosition = vector3 + new Vector2(config.CountTextOffsetX, config.CountTextOffsetY - num4);
            CountText.fontSizeMin = 0f;
            CountText.fontSize = config.CountTextFontSize;
            CountText.fontSizeMax = config.CountTextFontSize;
            CountText.enableAutoSizing = true;
            CountText.gameObject.SetActive(Profile.ShowKeyPressTotal);
            if (isSpecial && Profile.MakeBarSpecialKeys)
            {
                return;
            }
            Vector3 one = Vector3.one;
            if (Pressed)
            {
                Background.color = config.PressedBackgroundColor;
                Outline.color = config.PressedOutlineColor;
                Text.colorGradient = config.PressedTextColor;
                CountText.colorGradient = config.PressedCountTextColor;
                one *= config.ShrinkFactor;
            }
            else
            {
                Background.color = config.ReleasedBackgroundColor;
                Outline.color = config.ReleasedOutlineColor;
                Text.colorGradient = config.ReleasedTextColor;
                CountText.colorGradient = config.ReleasedCountTextColor;
            }
            Background.rectTransform.localScale = one;
            Outline.rectTransform.localScale = one;
            Text.rectTransform.localScale = one;
            CountText.rectTransform.localScale = one;
            x += width + 10f;
            if (!isSpecial)
            {
                rainContainer.SetActive(config.RainEnabled);
                if (config.RainEnabled)
                {
                    KeyRain.RainConfig rConfig = config.RainConfig;
                    RectMask2D rectMask2D = rainMask;
                    Vector2Int softness;
                    switch (rConfig.Direction)
                    {
                        case Direction.U:
                        case Direction.D:
                            softness = new Vector2Int(0, rConfig.Softness);
                            break;
                        case Direction.L:
                        case Direction.R:
                            softness = new Vector2Int(rConfig.Softness, 0);
                            break;
                        default:
                            softness = Vector2Int.zero;
                            break;
                    }
                    rectMask2D.softness = softness;
                    SetAnchor(rConfig.Direction);
                    rainMaskRt.sizeDelta = GetSizeDelta(rConfig.Direction);
                    rainMaskRt.anchoredPosition = GetMaskPosition(rConfig.Direction);
                    Sprite sprite = Main.GetSprite(rConfig.RainImage);
                    if (sprite != null)
                    {
                        rains.ForEach(delegate (KeyRain kr)
                        {
                            kr.image.sprite = sprite;
                        });
                    }
                    rains.ForEach(delegate (KeyRain kr)
                    {
                        kr.image.color = rConfig.RainColor;
                    });
                    rains.ForEach(delegate (KeyRain kr)
                    {
                        kr.ResetSizePos();
                    });
                }
            }
            layoutUpdated = true;
        }

        public void RenderGUI()
        {
            if (isSpecial)
            {
                if (!(config.Editing = GUILayout.Toggle(config.Editing, $"{SpecialType} Setting")))
                {
                    return;
                }
            }
            else if (!(config.Editing = GUILayout.Toggle(config.Editing, $"{Code} Setting")))
            {
                return;
            }
            MoreGUILayout.BeginIndent();
            if (!isSpecial)
            {
                GUILayout.BeginHorizontal();
                for (int i = 0; i < Profile.KeyGroups.Count; i++)
                {
                    Group group = Profile.KeyGroups[i];
                    if (!group.IsAdded(config))
                    {
                        if (GUILayout.Button("Add This At " + group.Name))
                        {
                            group.AddConfig(config);
                        }
                    }
                    else if (GUILayout.Button("Remove This At " + group.Name))
                    {
                        group.RemoveConfig(config);
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            bool flag = GUILayout.Toggle(config.RainEnabled, "Raining Key");
            if (flag)
            {
                MoreGUILayout.BeginIndent();
                if (KeyRain.DrawConfigGUI(Code, config.RainConfig))
                {
                    keyManager.UpdateLayout();
                }
                MoreGUILayout.EndIndent();
            }
            if (config.RainEnabled != flag)
            {
                config.RainEnabled = flag;
                keyManager.UpdateKeys();
            }
            GUILayout.BeginHorizontal();
            float num = MoreGUILayout.NamedSliderContent(lang.GetString("OFFSET_X"), config.OffsetX, -Screen.width, Screen.width, 300f);
            float num2 = MoreGUILayout.NamedSliderContent(lang.GetString("OFFSET_Y"), config.OffsetY, -Screen.height, Screen.height, 300f);
            if ((double)num != (double)config.OffsetX)
            {
                config.OffsetX = num;
                keyManager.UpdateLayout();
            }
            if ((double)num2 != (double)config.OffsetY)
            {
                config.OffsetY = num2;
                keyManager.UpdateLayout();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            string text = MoreGUILayout.NamedTextField(Main.Lang.GetString("FONT"), config.Font, 300f);
            if (text != config.Font)
            {
                config.Font = text;
                keyManager.UpdateLayout();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            float num3 = MoreGUILayout.NamedSliderContent(lang.GetString("WIDTH"), config.Width, -Screen.width, Screen.width, 300f);
            float num4 = MoreGUILayout.NamedSliderContent(lang.GetString("HEIGHT"), config.Height, -Screen.height, Screen.height, 300f);
            if ((double)num3 != (double)config.Width)
            {
                config.Width = num3;
                keyManager.UpdateLayout();
            }
            if ((double)num4 != (double)config.Height)
            {
                config.Height = num4;
                keyManager.UpdateLayout();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            string text2 = MoreGUILayout.NamedTextField(Main.Lang.GetString("KEY_TITLE"), config.KeyTitle, 300f);
            if (text2 != config.KeyTitle)
            {
                config.KeyTitle = text2;
                Text.text = text2;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            float num5 = MoreGUILayout.NamedSliderContent(Main.Lang.GetString("TEXT_OFFSET_X"), config.TextOffsetX, -300f, 300f, 200f);
            float num6 = MoreGUILayout.NamedSliderContent(Main.Lang.GetString("TEXT_OFFSET_Y"), config.TextOffsetY, -300f, 300f, 200f);
            if ((double)num5 != (double)config.TextOffsetX)
            {
                config.TextOffsetX = num5;
                keyManager.UpdateLayout();
            }
            if ((double)num6 != (double)config.TextOffsetY)
            {
                config.TextOffsetY = num6;
                keyManager.UpdateLayout();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            float num7 = MoreGUILayout.NamedSliderContent(Main.Lang.GetString("COUNT_TEXT_OFFSET_X"), config.CountTextOffsetX, -300f, 300f, 200f);
            float num8 = MoreGUILayout.NamedSliderContent(Main.Lang.GetString("COUNT_TEXT_OFFSET_Y"), config.CountTextOffsetY, -300f, 300f, 200f);
            if ((double)num7 != (double)config.CountTextOffsetX)
            {
                config.CountTextOffsetX = num7;
                keyManager.UpdateLayout();
            }
            if ((double)num8 != (double)config.CountTextOffsetY)
            {
                config.CountTextOffsetY = num8;
                keyManager.UpdateLayout();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            float num9 = MoreGUILayout.NamedSliderContent(lang.GetString("TEXT_FONT_SIZE"), config.TextFontSize, 0f, 300f, 200f);
            float num10 = MoreGUILayout.NamedSliderContent(lang.GetString("COUNT_TEXT_FONT_SIZE"), config.CountTextFontSize, 0f, 300f, 200f);
            if ((double)num9 != (double)config.TextFontSize)
            {
                config.TextFontSize = num9;
                keyManager.UpdateLayout();
            }
            if ((double)num10 != (double)config.CountTextFontSize)
            {
                config.CountTextFontSize = num10;
                keyManager.UpdateLayout();
            }
            GUILayout.EndHorizontal();
            if (isSpecial)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(lang.GetString("OUTLINE_COLOR"), GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
                GUILayout.Space(8f);
                GUILayout.Label(lang.GetString("BACKGROUND_COLOR"), GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
                GUILayout.Space(20f);
                GUILayout.EndHorizontal();
                MoreGUILayout.BeginIndent();
                var (color, color2) = MoreGUILayout.ColorRgbaSlidersPair(config.ReleasedOutlineColor, config.ReleasedBackgroundColor);
                if (color != config.ReleasedOutlineColor)
                {
                    config.ReleasedOutlineColor = color;
                    keyManager.UpdateLayout();
                }
                if (color2 != config.ReleasedBackgroundColor)
                {
                    config.ReleasedBackgroundColor = color2;
                    keyManager.UpdateLayout();
                }
                var (text3, text4) = MoreGUILayout.NamedTextFieldPair("Hex:", "Hex:", config.ReleasedOutlineColorHex, config.ReleasedBackgroundColorHex, 100f, 40f);
                if (text3 != config.ReleasedOutlineColorHex && ColorUtility.TryParseHtmlString("#" + text3, out color))
                {
                    config.ReleasedOutlineColor = color;
                    keyManager.UpdateLayout();
                }
                if (text4 != config.ReleasedBackgroundColorHex && ColorUtility.TryParseHtmlString("#" + text4, out color2))
                {
                    config.ReleasedBackgroundColor = color2;
                    keyManager.UpdateLayout();
                }
                MoreGUILayout.EndIndent();
                config.Gradient = GUILayout.Toggle(config.Gradient, "Gradient");
                GUILayout.BeginHorizontal();
                GUILayout.Label(lang.GetString("TEXT_COLOR"), GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
                GUILayout.Space(8f);
                GUILayout.Label(lang.GetString("COUNT_TEXT_COLOR"), GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
                GUILayout.Space(20f);
                GUILayout.EndHorizontal();
                MoreGUILayout.BeginIndent();
                if (config.Gradient)
                {
                    var (vertexGradient, vertexGradient2) = MoreGUILayout.VertexGradientSlidersPair(config.ReleasedTextColor, config.ReleasedCountTextColor);
                    if (vertexGradient.Inequals(config.ReleasedTextColor))
                    {
                        config.ReleasedTextColor = vertexGradient;
                        keyManager.UpdateLayout();
                    }
                    if (vertexGradient2.Inequals(config.ReleasedCountTextColor))
                    {
                        config.ReleasedCountTextColor = vertexGradient2;
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text5, text6) = MoreGUILayout.NamedTextFieldPair("Top Left Hex:", "Top Left Hex:", config.ReleasedTextColorHex[0], config.ReleasedCountTextColorHex[0], 100f, 100f);
                    GUILayout.EndHorizontal();
                    if (text5 != config.ReleasedTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text5, out var color3))
                    {
                        VertexGradient releasedTextColor = config.ReleasedTextColor;
                        config.ReleasedTextColor = new VertexGradient(color3, releasedTextColor.topRight, releasedTextColor.bottomLeft, releasedTextColor.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    if (text6 != config.ReleasedCountTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text6, out var color4))
                    {
                        VertexGradient releasedCountTextColor = config.ReleasedCountTextColor;
                        config.ReleasedCountTextColor = new VertexGradient(color4, releasedCountTextColor.topRight, releasedCountTextColor.bottomLeft, releasedCountTextColor.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text7, text8) = MoreGUILayout.NamedTextFieldPair("Top Right Hex:", "Top Right Hex:", config.ReleasedTextColorHex[1], config.ReleasedCountTextColorHex[1], 100f, 100f);
                    GUILayout.EndHorizontal();
                    if (text7 != config.ReleasedTextColorHex[1] && ColorUtility.TryParseHtmlString("#" + text7, out color3))
                    {
                        VertexGradient releasedTextColor2 = config.ReleasedTextColor;
                        config.ReleasedTextColor = new VertexGradient(releasedTextColor2.topLeft, color3, releasedTextColor2.bottomLeft, releasedTextColor2.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    if (text8 != config.ReleasedCountTextColorHex[1] && ColorUtility.TryParseHtmlString("#" + text8, out color4))
                    {
                        VertexGradient releasedCountTextColor2 = config.ReleasedCountTextColor;
                        config.ReleasedCountTextColor = new VertexGradient(releasedCountTextColor2.topLeft, color4, releasedCountTextColor2.bottomLeft, releasedCountTextColor2.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text9, text10) = MoreGUILayout.NamedTextFieldPair("Bottom Left Hex:", "Bottom Left Hex:", config.ReleasedTextColorHex[2], config.ReleasedCountTextColorHex[2], 100f, 100f);
                    GUILayout.EndHorizontal();
                    if (text9 != config.ReleasedTextColorHex[2] && ColorUtility.TryParseHtmlString("#" + text9, out color3))
                    {
                        VertexGradient releasedTextColor3 = config.ReleasedTextColor;
                        config.ReleasedTextColor = new VertexGradient(releasedTextColor3.topLeft, releasedTextColor3.topRight, color3, releasedTextColor3.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    if (text10 != config.ReleasedCountTextColorHex[2] && ColorUtility.TryParseHtmlString("#" + text10, out color4))
                    {
                        VertexGradient releasedCountTextColor3 = config.ReleasedCountTextColor;
                        config.ReleasedCountTextColor = new VertexGradient(releasedCountTextColor3.topLeft, releasedCountTextColor3.topRight, color4, releasedCountTextColor3.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text11, text12) = MoreGUILayout.NamedTextFieldPair("Bottom Right Hex:", "Bottom Right Hex:", config.ReleasedTextColorHex[3], config.ReleasedCountTextColorHex[3], 100f, 110f);
                    GUILayout.EndHorizontal();
                    if (text11 != config.ReleasedTextColorHex[3] && ColorUtility.TryParseHtmlString("#" + text11, out color3))
                    {
                        VertexGradient releasedTextColor4 = config.ReleasedTextColor;
                        config.ReleasedTextColor = new VertexGradient(releasedTextColor4.topLeft, releasedTextColor4.topRight, releasedTextColor4.bottomLeft, color3);
                        keyManager.UpdateLayout();
                    }
                    if (text12 != config.ReleasedCountTextColorHex[3] && ColorUtility.TryParseHtmlString("#" + text12, out color4))
                    {
                        VertexGradient releasedCountTextColor4 = config.ReleasedCountTextColor;
                        config.ReleasedCountTextColor = new VertexGradient(releasedCountTextColor4.topLeft, releasedCountTextColor4.topRight, releasedCountTextColor4.bottomLeft, color4);
                        keyManager.UpdateLayout();
                    }
                }
                else
                {
                    var (color5, color6) = MoreGUILayout.ColorRgbaSlidersPair(config.ReleasedTextColor.topLeft, config.ReleasedCountTextColor.topLeft);
                    if (color5 != config.ReleasedTextColor.topLeft)
                    {
                        config.ReleasedTextColor = new VertexGradient(color5);
                        keyManager.UpdateLayout();
                    }
                    if (color6 != config.ReleasedCountTextColor.topLeft)
                    {
                        config.ReleasedCountTextColor = new VertexGradient(color6);
                        keyManager.UpdateLayout();
                    }
                    var (text13, text14) = MoreGUILayout.NamedTextFieldPair("Hex:", "Hex:", config.ReleasedTextColorHex[0], config.ReleasedCountTextColorHex[0], 100f, 40f);
                    if (text13 != config.ReleasedTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text13, out color5))
                    {
                        config.ReleasedTextColor = new VertexGradient(color5);
                        keyManager.UpdateLayout();
                    }
                    if (text14 != config.ReleasedCountTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text14, out color6))
                    {
                        config.ReleasedCountTextColor = new VertexGradient(color6);
                        keyManager.UpdateLayout();
                    }
                }
                MoreGUILayout.EndIndent();
            }
            else
            {
                GUILayout.BeginHorizontal();
                config.ShrinkFactor = MoreGUILayout.NamedSliderContent("Shrink Factor", config.ShrinkFactor, 0f, 10f, 600f);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                config.EaseDuration = MoreGUILayout.NamedSliderContent("Ease Duration", config.EaseDuration, 0f, 10f, 600f);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Ease:");
                DrawEase(config.Ease, delegate (Ease ease)
                {
                    config.Ease = ease;
                });
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(lang.GetString("KEY_COUNT"));
                if (uint.TryParse(GUILayout.TextField(config.Count.ToString()), out var result))
                {
                    config.Count = result;
                    CountText.text = config.Count.ToString();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("KeyCode:");
                DrawKeyCode(config.Code, delegate (KeyCode code)
                {
                    config.Code = code;
                });
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Spare KeyCode:");
                DrawSpareKeyCode(config.SpareCode, delegate (KeyCode code)
                {
                    config.SpareCode = code;
                });
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (config.ChangeBgColorJudge = GUILayout.Toggle(config.ChangeBgColorJudge, lang.GetString("CHANGE_BG_COLOR_FOLLOWING_HITMARGIN")))
                {
                    MoreGUILayout.BeginIndent();
                    GUILayout.BeginHorizontal();
                    var (text15, text16) = MoreGUILayout.NamedTextFieldPair("Too Early Hex:", "Very Early Hex:", config.HitMarginColorHex[0], config.HitMarginColorHex[1], 100f, 120f);
                    GUILayout.EndHorizontal();
                    if (text15 != config.HitMarginColorHex[0] && ColorUtility.TryParseHtmlString("#" + text15, out var color7))
                    {
                        config.TooEarlyColor = color7;
                        keyManager.UpdateLayout();
                    }
                    if (text16 != config.HitMarginColorHex[1] && ColorUtility.TryParseHtmlString("#" + text16, out color7))
                    {
                        config.VeryEarlyColor = color7;
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text17, text18) = MoreGUILayout.NamedTextFieldPair("Early Perfect Hex:", "Perfect Hex:", config.HitMarginColorHex[2], config.HitMarginColorHex[3], 100f, 120f);
                    GUILayout.EndHorizontal();
                    if (text17 != config.HitMarginColorHex[2] && ColorUtility.TryParseHtmlString("#" + text17, out color7))
                    {
                        config.EarlyPerfectColor = color7;
                        keyManager.UpdateLayout();
                    }
                    if (text18 != config.HitMarginColorHex[3] && ColorUtility.TryParseHtmlString("#" + text18, out color7))
                    {
                        config.PerfectColor = color7;
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text19, text20) = MoreGUILayout.NamedTextFieldPair("Late Perfect Hex:", "Very Late Hex:", config.HitMarginColorHex[4], config.HitMarginColorHex[5], 100f, 120f);
                    GUILayout.EndHorizontal();
                    if (text19 != config.HitMarginColorHex[4] && ColorUtility.TryParseHtmlString("#" + text19, out color7))
                    {
                        config.LatePerfectColor = color7;
                        keyManager.UpdateLayout();
                    }
                    if (text20 != config.HitMarginColorHex[5] && ColorUtility.TryParseHtmlString("#" + text20, out color7))
                    {
                        config.VeryLateColor = color7;
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text21, text22) = MoreGUILayout.NamedTextFieldPair("Too Late Hex:", "Multipress Hex:", config.HitMarginColorHex[6], config.HitMarginColorHex[7], 100f, 120f);
                    GUILayout.EndHorizontal();
                    if (text21 != config.HitMarginColorHex[6] && ColorUtility.TryParseHtmlString("#" + text21, out color7))
                    {
                        config.TooLateColor = color7;
                        keyManager.UpdateLayout();
                    }
                    if (text22 != config.HitMarginColorHex[7] && ColorUtility.TryParseHtmlString("#" + text22, out color7))
                    {
                        config.MultipressColor = color7;
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text23, text24) = MoreGUILayout.NamedTextFieldPair("Fail Miss Hex:", "Fail Overload Hex:", config.HitMarginColorHex[8], config.HitMarginColorHex[9], 100f, 120f);
                    GUILayout.EndHorizontal();
                    if (text23 != config.HitMarginColorHex[8] && ColorUtility.TryParseHtmlString("#" + text23, out color7))
                    {
                        config.FailMissColor = color7;
                        keyManager.UpdateLayout();
                    }
                    if (text24 != config.HitMarginColorHex[9] && ColorUtility.TryParseHtmlString("#" + text24, out color7))
                    {
                        config.FailOverloadColor = color7;
                        keyManager.UpdateLayout();
                    }
                    MoreGUILayout.EndIndent();
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label(lang.GetString("PRESSED_OUTLINE_COLOR"), GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
                GUILayout.Space(8f);
                GUILayout.Label(lang.GetString("RELEASED_OUTLINE_COLOR"), GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
                GUILayout.Space(20f);
                GUILayout.EndHorizontal();
                MoreGUILayout.BeginIndent();
                Color color8;
                Color color9;
                (color8, color9) = MoreGUILayout.ColorRgbaSlidersPair(config.PressedOutlineColor, config.ReleasedOutlineColor);
                if (color8 != config.PressedOutlineColor)
                {
                    config.PressedOutlineColor = color8;
                    keyManager.UpdateLayout();
                }
                if (color9 != config.ReleasedOutlineColor)
                {
                    config.ReleasedOutlineColor = color9;
                    keyManager.UpdateLayout();
                }
                var (text25, text26) = MoreGUILayout.NamedTextFieldPair("Hex:", "Hex:", config.PressedOutlineColorHex, config.ReleasedOutlineColorHex, 100f, 40f);
                if (text25 != config.PressedOutlineColorHex && ColorUtility.TryParseHtmlString("#" + text25, out color8))
                {
                    config.PressedOutlineColor = color8;
                    keyManager.UpdateLayout();
                }
                if (text26 != config.ReleasedOutlineColorHex && ColorUtility.TryParseHtmlString("#" + text26, out color9))
                {
                    config.ReleasedOutlineColor = color9;
                    keyManager.UpdateLayout();
                }
                MoreGUILayout.EndIndent();
                GUILayout.Space(8f);
                GUILayout.BeginHorizontal();
                GUILayout.Label(lang.GetString("PRESSED_BACKGROUND_COLOR"), GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
                GUILayout.Space(8f);
                GUILayout.Label(lang.GetString("RELEASED_BACKGROUND_COLOR"), GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
                GUILayout.Space(20f);
                GUILayout.EndHorizontal();
                MoreGUILayout.BeginIndent();
                (color8, color9) = MoreGUILayout.ColorRgbaSlidersPair(config.PressedBackgroundColor, config.ReleasedBackgroundColor);
                if (color8 != config.PressedBackgroundColor)
                {
                    config.PressedBackgroundColor = color8;
                    keyManager.UpdateLayout();
                }
                if (color9 != config.ReleasedBackgroundColor)
                {
                    config.ReleasedBackgroundColor = color9;
                    keyManager.UpdateLayout();
                }
                var (text27, text28) = MoreGUILayout.NamedTextFieldPair("Hex:", "Hex:", config.PressedBackgroundColorHex, config.ReleasedBackgroundColorHex, 100f, 40f);
                if (text27 != config.PressedBackgroundColorHex && ColorUtility.TryParseHtmlString("#" + text27, out color8))
                {
                    config.PressedBackgroundColor = color8;
                    keyManager.UpdateLayout();
                }
                if (text28 != config.ReleasedBackgroundColorHex && ColorUtility.TryParseHtmlString("#" + text28, out color9))
                {
                    config.ReleasedBackgroundColor = color9;
                    keyManager.UpdateLayout();
                }
                MoreGUILayout.EndIndent();
                GUILayout.Space(8f);
                config.Gradient = GUILayout.Toggle(config.Gradient, "Gradient");
                GUILayout.BeginHorizontal();
                GUILayout.Label(lang.GetString("PRESSED_TEXT_COLOR"), GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
                GUILayout.Space(8f);
                GUILayout.Label(lang.GetString("RELEASED_TEXT_COLOR"), GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
                GUILayout.Space(20f);
                GUILayout.EndHorizontal();
                MoreGUILayout.BeginIndent();
                if (config.Gradient)
                {
                    var (vertexGradient3, vertexGradient4) = MoreGUILayout.VertexGradientSlidersPair(config.PressedTextColor, config.ReleasedTextColor);
                    if (vertexGradient3.Inequals(config.PressedTextColor))
                    {
                        config.PressedTextColor = vertexGradient3;
                        keyManager.UpdateLayout();
                    }
                    if (vertexGradient4.Inequals(config.ReleasedTextColor))
                    {
                        config.ReleasedTextColor = vertexGradient4;
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text29, text30) = MoreGUILayout.NamedTextFieldPair("Top Left Hex:", "Top Left Hex:", config.PressedTextColorHex[0], config.ReleasedTextColorHex[0], 100f, 100f);
                    GUILayout.EndHorizontal();
                    if (text29 != config.PressedTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text29, out color8))
                    {
                        VertexGradient pressedTextColor = config.PressedTextColor;
                        config.PressedTextColor = new VertexGradient(color8, pressedTextColor.topRight, pressedTextColor.bottomLeft, pressedTextColor.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    if (text30 != config.ReleasedTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text30, out color9))
                    {
                        VertexGradient releasedTextColor5 = config.ReleasedTextColor;
                        config.ReleasedTextColor = new VertexGradient(color9, releasedTextColor5.topRight, releasedTextColor5.bottomLeft, releasedTextColor5.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text31, text32) = MoreGUILayout.NamedTextFieldPair("Top Right Hex:", "Top Right Hex:", config.PressedTextColorHex[1], config.ReleasedTextColorHex[1], 100f, 100f);
                    GUILayout.EndHorizontal();
                    if (text31 != config.PressedTextColorHex[1] && ColorUtility.TryParseHtmlString("#" + text31, out color8))
                    {
                        VertexGradient pressedTextColor2 = config.PressedTextColor;
                        config.PressedTextColor = new VertexGradient(pressedTextColor2.topLeft, color8, pressedTextColor2.bottomLeft, pressedTextColor2.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    if (text32 != config.ReleasedTextColorHex[1] && ColorUtility.TryParseHtmlString("#" + text32, out color9))
                    {
                        VertexGradient releasedTextColor6 = config.ReleasedTextColor;
                        config.ReleasedTextColor = new VertexGradient(releasedTextColor6.topLeft, color9, releasedTextColor6.bottomLeft, releasedTextColor6.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text33, text34) = MoreGUILayout.NamedTextFieldPair("Bottom Left Hex:", "Bottom Left Hex:", config.PressedTextColorHex[2], config.ReleasedTextColorHex[2], 100f, 100f);
                    GUILayout.EndHorizontal();
                    if (text33 != config.PressedTextColorHex[2] && ColorUtility.TryParseHtmlString("#" + text33, out color8))
                    {
                        VertexGradient pressedTextColor3 = config.PressedTextColor;
                        config.PressedTextColor = new VertexGradient(pressedTextColor3.topLeft, pressedTextColor3.topRight, color8, pressedTextColor3.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    if (text34 != config.ReleasedTextColorHex[2] && ColorUtility.TryParseHtmlString("#" + text34, out color9))
                    {
                        VertexGradient releasedTextColor7 = config.ReleasedTextColor;
                        config.ReleasedTextColor = new VertexGradient(releasedTextColor7.topLeft, releasedTextColor7.topRight, color9, releasedTextColor7.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text35, text36) = MoreGUILayout.NamedTextFieldPair("Bottom Right Hex:", "Bottom Right Hex:", config.PressedTextColorHex[3], config.ReleasedTextColorHex[3], 100f, 110f);
                    GUILayout.EndHorizontal();
                    if (text35 != config.PressedTextColorHex[3] && ColorUtility.TryParseHtmlString("#" + text35, out color8))
                    {
                        VertexGradient pressedTextColor4 = config.PressedTextColor;
                        config.PressedTextColor = new VertexGradient(pressedTextColor4.topLeft, pressedTextColor4.topRight, pressedTextColor4.bottomLeft, color8);
                        keyManager.UpdateLayout();
                    }
                    if (text36 != config.ReleasedTextColorHex[3] && ColorUtility.TryParseHtmlString("#" + text36, out color9))
                    {
                        VertexGradient releasedTextColor8 = config.ReleasedTextColor;
                        config.ReleasedTextColor = new VertexGradient(releasedTextColor8.topLeft, releasedTextColor8.topRight, releasedTextColor8.bottomLeft, color9);
                        keyManager.UpdateLayout();
                    }
                }
                else
                {
                    (color8, color9) = MoreGUILayout.ColorRgbaSlidersPair(config.PressedTextColor.topLeft, config.ReleasedTextColor.topLeft);
                    if (color8 != config.PressedTextColor.topLeft)
                    {
                        config.PressedTextColor = new VertexGradient(color8);
                        keyManager.UpdateLayout();
                    }
                    if (color9 != config.ReleasedTextColor.topLeft)
                    {
                        config.ReleasedTextColor = new VertexGradient(color9);
                        keyManager.UpdateLayout();
                    }
                    var (text37, text38) = MoreGUILayout.NamedTextFieldPair("Hex:", "Hex:", config.PressedTextColorHex[0], config.ReleasedTextColorHex[0], 100f, 40f);
                    if (text37 != config.PressedTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text37, out color8))
                    {
                        config.PressedTextColor = new VertexGradient(color8);
                        keyManager.UpdateLayout();
                    }
                    if (text38 != config.ReleasedTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text38, out color9))
                    {
                        config.ReleasedTextColor = new VertexGradient(color9);
                        keyManager.UpdateLayout();
                    }
                }
                MoreGUILayout.EndIndent();
                GUILayout.BeginHorizontal();
                GUILayout.Label(lang.GetString("PRESSED_COUNT_TEXT_COLOR"), GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
                GUILayout.Space(8f);
                GUILayout.Label(lang.GetString("RELEASED_COUNT_TEXT_COLOR"), GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
                GUILayout.Space(20f);
                GUILayout.EndHorizontal();
                MoreGUILayout.BeginIndent();
                if (config.Gradient)
                {
                    var (vertexGradient5, vertexGradient6) = MoreGUILayout.VertexGradientSlidersPair(config.PressedCountTextColor, config.ReleasedCountTextColor);
                    if (vertexGradient5.Inequals(config.PressedCountTextColor))
                    {
                        config.PressedCountTextColor = vertexGradient5;
                        keyManager.UpdateLayout();
                    }
                    if (vertexGradient6.Inequals(config.ReleasedCountTextColor))
                    {
                        config.ReleasedCountTextColor = vertexGradient6;
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text39, text40) = MoreGUILayout.NamedTextFieldPair("Top Left Hex:", "Top Left Hex:", config.PressedCountTextColorHex[0], config.ReleasedCountTextColorHex[0], 100f, 100f);
                    GUILayout.EndHorizontal();
                    if (text39 != config.PressedCountTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text39, out color8))
                    {
                        VertexGradient pressedTextColor5 = config.PressedTextColor;
                        config.PressedTextColor = new VertexGradient(color8, pressedTextColor5.topRight, pressedTextColor5.bottomLeft, pressedTextColor5.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    if (text40 != config.ReleasedCountTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text40, out color9))
                    {
                        VertexGradient releasedTextColor9 = config.ReleasedTextColor;
                        config.ReleasedTextColor = new VertexGradient(color9, releasedTextColor9.topRight, releasedTextColor9.bottomLeft, releasedTextColor9.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text41, text42) = MoreGUILayout.NamedTextFieldPair("Top Right Hex:", "Top Right Hex:", config.PressedCountTextColorHex[1], config.ReleasedCountTextColorHex[1], 100f, 100f);
                    GUILayout.EndHorizontal();
                    if (text41 != config.PressedCountTextColorHex[1] && ColorUtility.TryParseHtmlString("#" + text41, out color8))
                    {
                        VertexGradient pressedTextColor6 = config.PressedTextColor;
                        config.PressedTextColor = new VertexGradient(pressedTextColor6.topLeft, color8, pressedTextColor6.bottomLeft, pressedTextColor6.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    if (text42 != config.ReleasedCountTextColorHex[1] && ColorUtility.TryParseHtmlString("#" + text42, out color9))
                    {
                        VertexGradient releasedTextColor10 = config.ReleasedTextColor;
                        config.ReleasedTextColor = new VertexGradient(releasedTextColor10.topLeft, color9, releasedTextColor10.bottomLeft, releasedTextColor10.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text43, text44) = MoreGUILayout.NamedTextFieldPair("Bottom Left Hex:", "Bottom Left Hex:", config.PressedCountTextColorHex[2], config.ReleasedCountTextColorHex[2], 100f, 100f);
                    GUILayout.EndHorizontal();
                    if (text43 != config.PressedCountTextColorHex[2] && ColorUtility.TryParseHtmlString("#" + text43, out color8))
                    {
                        VertexGradient pressedTextColor7 = config.PressedTextColor;
                        config.PressedTextColor = new VertexGradient(pressedTextColor7.topLeft, pressedTextColor7.topRight, color8, pressedTextColor7.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    if (text44 != config.ReleasedCountTextColorHex[2] && ColorUtility.TryParseHtmlString("#" + text44, out color9))
                    {
                        VertexGradient releasedTextColor11 = config.ReleasedTextColor;
                        config.ReleasedTextColor = new VertexGradient(releasedTextColor11.topLeft, releasedTextColor11.topRight, color9, releasedTextColor11.bottomRight);
                        keyManager.UpdateLayout();
                    }
                    GUILayout.BeginHorizontal();
                    var (text45, text46) = MoreGUILayout.NamedTextFieldPair("Bottom Right Hex:", "Bottom Right Hex:", config.PressedCountTextColorHex[3], config.ReleasedCountTextColorHex[3], 100f, 110f);
                    GUILayout.EndHorizontal();
                    if (text45 != config.PressedCountTextColorHex[3] && ColorUtility.TryParseHtmlString("#" + text45, out color8))
                    {
                        VertexGradient pressedTextColor8 = config.PressedTextColor;
                        config.PressedTextColor = new VertexGradient(pressedTextColor8.topLeft, pressedTextColor8.topRight, pressedTextColor8.bottomLeft, color8);
                        keyManager.UpdateLayout();
                    }
                    if (text46 != config.ReleasedCountTextColorHex[3] && ColorUtility.TryParseHtmlString("#" + text46, out color9))
                    {
                        VertexGradient releasedTextColor12 = config.ReleasedTextColor;
                        config.ReleasedTextColor = new VertexGradient(releasedTextColor12.topLeft, releasedTextColor12.topRight, releasedTextColor12.bottomLeft, color9);
                        keyManager.UpdateLayout();
                    }
                }
                else
                {
                    (color8, color9) = MoreGUILayout.ColorRgbaSlidersPair(config.PressedCountTextColor.topLeft, config.ReleasedCountTextColor.topLeft);
                    if (color8 != config.PressedCountTextColor.topLeft)
                    {
                        config.PressedCountTextColor = new VertexGradient(color8);
                        keyManager.UpdateLayout();
                    }
                    if (color9 != config.ReleasedCountTextColor.topLeft)
                    {
                        config.ReleasedCountTextColor = new VertexGradient(color9);
                        keyManager.UpdateLayout();
                    }
                    var (text47, text48) = MoreGUILayout.NamedTextFieldPair("Hex:", "Hex:", config.PressedCountTextColorHex[0], config.ReleasedCountTextColorHex[0], 100f, 40f);
                    if (text47 != config.PressedCountTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text47, out color8))
                    {
                        config.PressedCountTextColor = new VertexGradient(color8);
                        keyManager.UpdateLayout();
                    }
                    if (text48 != config.ReleasedCountTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text48, out color9))
                    {
                        config.ReleasedCountTextColor = new VertexGradient(color9);
                        keyManager.UpdateLayout();
                    }
                }
                MoreGUILayout.EndIndent();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(lang.GetString("RESET")))
            {
                config.Reset();
                keyManager.UpdateLayout();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            MoreGUILayout.EndIndent();
        }

        public void ChangeHitMarginColor(HitMargin hit)
        {
            if (config.ChangeBgColorJudge)
            {
                forceBgColor = GetHitMarginColor(hit);
            }
        }

        public Color GetHitMarginColor(HitMargin hit)
        {
            return hit switch
            {
                HitMargin.TooEarly => config.TooEarlyColor,
                HitMargin.VeryEarly => config.VeryEarlyColor,
                HitMargin.EarlyPerfect => config.EarlyPerfectColor,
                HitMargin.Perfect => config.PerfectColor,
                HitMargin.LatePerfect => config.LatePerfectColor,
                HitMargin.VeryLate => config.VeryLateColor,
                HitMargin.TooLate => config.TooLateColor,
                HitMargin.Multipress => config.MultipressColor,
                HitMargin.FailMiss => config.FailMissColor,
                HitMargin.FailOverload => config.FailOverloadColor,
                HitMargin.Auto => config.PerfectColor,
                _ => config.PressedBackgroundColor,
            };
        }

        internal Vector2 GetMaskPosition(Direction dir)
        {
            Vector2 vector = position + offsetVec;
            float width = config.Width;
            float num = config.Height + (Profile.ShowKeyPressTotal ? 50f : 0f);
            return dir switch
            {
                Direction.U => new Vector2(vector.x + config.RainConfig.OffsetX, (float)((double)vector.y + ((double)num / 2.0 - (double)config.RainConfig.Softness) + 10.0) + config.RainConfig.OffsetY),
                Direction.D => new Vector2(vector.x + config.RainConfig.OffsetX, (float)((double)vector.y - ((double)num / 2.0 - (double)config.RainConfig.Softness) - 10.0) + config.RainConfig.OffsetY),
                Direction.L => new Vector2((float)((double)vector.x + (double)config.RainConfig.OffsetX - ((double)width / 2.0 - (double)config.RainConfig.Softness) - 10.0), vector.y + config.RainConfig.OffsetY),
                Direction.R => new Vector2((float)((double)vector.x + (double)config.RainConfig.OffsetX + ((double)width / 2.0 - (double)config.RainConfig.Softness) + 10.0), vector.y + config.RainConfig.OffsetY),
                _ => Vector2.zero,
            };
        }

        internal Vector2 GetSizeDelta(Direction dir)
        {
            KeyRain.RainConfig rainConfig = config.RainConfig;
            switch (dir)
            {
                case Direction.U:
                case Direction.D:
                    return ((double)rainConfig.RainWidth <= 0.0) ? new Vector2(config.Width, (float)rainConfig.Softness + rainConfig.RainLength) : new Vector2(rainConfig.RainWidth, (float)rainConfig.Softness + rainConfig.RainLength);
                case Direction.L:
                case Direction.R:
                    {
                        int num = (Profile.ShowKeyPressTotal ? 50 : 0) + 5;
                        return ((double)rainConfig.RainHeight <= 0.0) ? new Vector2((float)rainConfig.Softness + rainConfig.RainLength, config.Height + (float)num) : new Vector2((float)rainConfig.Softness + rainConfig.RainLength, rainConfig.RainHeight);
                    }
                default:
                    return Vector2.zero;
            }
        }

        internal void SetAnchor(Direction dir)
        {
            switch (dir)
            {
                case Direction.U:
                    rainMaskRt.pivot = new Vector2(0.5f, 0f);
                    rainMaskRt.anchorMin = new Vector2(0.5f, 0f);
                    rainMaskRt.anchorMax = new Vector2(0.5f, 0f);
                    break;
                case Direction.D:
                    rainMaskRt.pivot = new Vector2(0.5f, 1f);
                    rainMaskRt.anchorMin = new Vector2(0.5f, 1f);
                    rainMaskRt.anchorMax = new Vector2(0.5f, 1f);
                    break;
                case Direction.L:
                    rainMaskRt.pivot = new Vector2(1f, 0.5f);
                    rainMaskRt.anchorMin = new Vector2(1f, 0.5f);
                    rainMaskRt.anchorMax = new Vector2(1f, 0.5f);
                    break;
                case Direction.R:
                    rainMaskRt.pivot = new Vector2(0f, 0.5f);
                    rainMaskRt.anchorMin = new Vector2(0f, 0.5f);
                    rainMaskRt.anchorMax = new Vector2(0f, 0.5f);
                    break;
            }
        }

        static Key()
        {
            forceBgColor = null;
            eases = (Ease[])Enum.GetValues(typeof(Ease));
            easeNames = eases.Select((Ease e) => e.ToString()).ToArray();
            keyNames = Main.KeyCodes.Select((KeyCode c) => c.ToString()).ToArray();
            KeyString = new Dictionary<KeyCode, string>
        {
            {
                KeyCode.Alpha0,
                "0"
            },
            {
                KeyCode.Alpha1,
                "1"
            },
            {
                KeyCode.Alpha2,
                "2"
            },
            {
                KeyCode.Alpha3,
                "3"
            },
            {
                KeyCode.Alpha4,
                "4"
            },
            {
                KeyCode.Alpha5,
                "5"
            },
            {
                KeyCode.Alpha6,
                "6"
            },
            {
                KeyCode.Alpha7,
                "7"
            },
            {
                KeyCode.Alpha8,
                "8"
            },
            {
                KeyCode.Alpha9,
                "9"
            },
            {
                KeyCode.Keypad0,
                "0"
            },
            {
                KeyCode.Keypad1,
                "1"
            },
            {
                KeyCode.Keypad2,
                "2"
            },
            {
                KeyCode.Keypad3,
                "3"
            },
            {
                KeyCode.Keypad4,
                "4"
            },
            {
                KeyCode.Keypad5,
                "5"
            },
            {
                KeyCode.Keypad6,
                "6"
            },
            {
                KeyCode.Keypad7,
                "7"
            },
            {
                KeyCode.Keypad8,
                "8"
            },
            {
                KeyCode.Keypad9,
                "9"
            },
            {
                KeyCode.KeypadPlus,
                "+"
            },
            {
                KeyCode.KeypadMinus,
                "-"
            },
            {
                KeyCode.KeypadMultiply,
                "*"
            },
            {
                KeyCode.KeypadDivide,
                "/"
            },
            {
                KeyCode.KeypadEnter,
                "↵"
            },
            {
                KeyCode.KeypadEquals,
                "="
            },
            {
                KeyCode.KeypadPeriod,
                "."
            },
            {
                KeyCode.Return,
                "↵"
            },
            {
                KeyCode.None,
                " "
            },
            {
                KeyCode.Tab,
                "⇥"
            },
            {
                KeyCode.Backslash,
                "\\"
            },
            {
                KeyCode.Slash,
                "/"
            },
            {
                KeyCode.Minus,
                "-"
            },
            {
                KeyCode.Equals,
                "="
            },
            {
                KeyCode.LeftBracket,
                "["
            },
            {
                KeyCode.RightBracket,
                "]"
            },
            {
                KeyCode.Semicolon,
                ";"
            },
            {
                KeyCode.Comma,
                ","
            },
            {
                KeyCode.Period,
                "."
            },
            {
                KeyCode.Quote,
                "'"
            },
            {
                KeyCode.UpArrow,
                "↑"
            },
            {
                KeyCode.DownArrow,
                "↓"
            },
            {
                KeyCode.LeftArrow,
                "←"
            },
            {
                KeyCode.RightArrow,
                "→"
            },
            {
                KeyCode.Space,
                "␣"
            },
            {
                KeyCode.BackQuote,
                "`"
            },
            {
                KeyCode.LeftShift,
                "L⇧"
            },
            {
                KeyCode.RightShift,
                "R⇧"
            },
            {
                KeyCode.LeftControl,
                "LCtrl"
            },
            {
                KeyCode.RightControl,
                "RCtrl"
            },
            {
                KeyCode.LeftAlt,
                "LAlt"
            },
            {
                KeyCode.RightAlt,
                "RAlt"
            },
            {
                KeyCode.Delete,
                "Del"
            },
            {
                KeyCode.PageDown,
                "Pg↓"
            },
            {
                KeyCode.PageUp,
                "Pg↑"
            },
            {
                KeyCode.CapsLock,
                "⇪"
            },
            {
                KeyCode.Insert,
                "Ins"
            },
            {
                KeyCode.Mouse0,
                "M0"
            },
            {
                KeyCode.Mouse1,
                "M1"
            },
            {
                KeyCode.Mouse2,
                "M2"
            },
            {
                KeyCode.Mouse3,
                "M3"
            },
            {
                KeyCode.Mouse4,
                "M4"
            },
            {
                KeyCode.Mouse5,
                "M5"
            },
            {
                KeyCode.Mouse6,
                "M6"
            }
        };
            codeIndex = new Dictionary<KeyCode, int>();
            for (int i = 0; i < Main.KeyCodes.Length; i++)
            {
                codeIndex[Main.KeyCodes[i]] = i;
            }
            KeyCodeToNative = new Dictionary<KeyCode, ushort>();
            KeyCodeToNative[KeyCode.None] = 0;
            KeyCodeToNative[KeyCode.Escape] = 1;
            KeyCodeToNative[KeyCode.Alpha1] = 2;
            KeyCodeToNative[KeyCode.Alpha2] = 3;
            KeyCodeToNative[KeyCode.Alpha3] = 4;
            KeyCodeToNative[KeyCode.Alpha4] = 5;
            KeyCodeToNative[KeyCode.Alpha5] = 6;
            KeyCodeToNative[KeyCode.Alpha6] = 7;
            KeyCodeToNative[KeyCode.Alpha7] = 8;
            KeyCodeToNative[KeyCode.Alpha8] = 9;
            KeyCodeToNative[KeyCode.Alpha9] = 10;
            KeyCodeToNative[KeyCode.Alpha0] = 11;
            KeyCodeToNative[KeyCode.Minus] = 12;
            KeyCodeToNative[KeyCode.Equals] = 13;
            KeyCodeToNative[KeyCode.Backspace] = 14;
            KeyCodeToNative[KeyCode.Tab] = 15;
            KeyCodeToNative[KeyCode.Q] = 16;
            KeyCodeToNative[KeyCode.W] = 17;
            KeyCodeToNative[KeyCode.E] = 18;
            KeyCodeToNative[KeyCode.R] = 19;
            KeyCodeToNative[KeyCode.T] = 20;
            KeyCodeToNative[KeyCode.Y] = 21;
            KeyCodeToNative[KeyCode.U] = 22;
            KeyCodeToNative[KeyCode.I] = 23;
            KeyCodeToNative[KeyCode.O] = 24;
            KeyCodeToNative[KeyCode.P] = 25;
            KeyCodeToNative[KeyCode.LeftBracket] = 26;
            KeyCodeToNative[KeyCode.RightBracket] = 27;
            KeyCodeToNative[KeyCode.Return] = 28;
            KeyCodeToNative[KeyCode.LeftControl] = 29;
            KeyCodeToNative[KeyCode.A] = 30;
            KeyCodeToNative[KeyCode.S] = 31;
            KeyCodeToNative[KeyCode.D] = 32;
            KeyCodeToNative[KeyCode.F] = 33;
            KeyCodeToNative[KeyCode.G] = 34;
            KeyCodeToNative[KeyCode.H] = 35;
            KeyCodeToNative[KeyCode.J] = 36;
            KeyCodeToNative[KeyCode.K] = 37;
            KeyCodeToNative[KeyCode.L] = 38;
            KeyCodeToNative[KeyCode.Semicolon] = 39;
            KeyCodeToNative[KeyCode.Quote] = 40;
            KeyCodeToNative[KeyCode.BackQuote] = 41;
            KeyCodeToNative[KeyCode.LeftShift] = 42;
            KeyCodeToNative[KeyCode.Backslash] = 43;
            KeyCodeToNative[KeyCode.Z] = 44;
            KeyCodeToNative[KeyCode.X] = 45;
            KeyCodeToNative[KeyCode.C] = 46;
            KeyCodeToNative[KeyCode.V] = 47;
            KeyCodeToNative[KeyCode.B] = 48;
            KeyCodeToNative[KeyCode.N] = 49;
            KeyCodeToNative[KeyCode.M] = 50;
            KeyCodeToNative[KeyCode.Comma] = 51;
            KeyCodeToNative[KeyCode.Period] = 52;
            KeyCodeToNative[KeyCode.Slash] = 53;
            KeyCodeToNative[KeyCode.RightShift] = 54;
            KeyCodeToNative[KeyCode.KeypadMultiply] = 55;
            KeyCodeToNative[KeyCode.LeftAlt] = 56;
            KeyCodeToNative[KeyCode.Space] = 57;
            KeyCodeToNative[KeyCode.CapsLock] = 58;
            KeyCodeToNative[KeyCode.F1] = 59;
            KeyCodeToNative[KeyCode.F2] = 60;
            KeyCodeToNative[KeyCode.F3] = 61;
            KeyCodeToNative[KeyCode.F4] = 62;
            KeyCodeToNative[KeyCode.F5] = 63;
            KeyCodeToNative[KeyCode.F6] = 64;
            KeyCodeToNative[KeyCode.F7] = 65;
            KeyCodeToNative[KeyCode.F8] = 66;
            KeyCodeToNative[KeyCode.F9] = 67;
            KeyCodeToNative[KeyCode.F10] = 68;
            KeyCodeToNative[KeyCode.Numlock] = 69;
            KeyCodeToNative[KeyCode.ScrollLock] = 70;
            KeyCodeToNative[KeyCode.Keypad7] = 71;
            KeyCodeToNative[KeyCode.Keypad8] = 72;
            KeyCodeToNative[KeyCode.Keypad9] = 73;
            KeyCodeToNative[KeyCode.KeypadMinus] = 74;
            KeyCodeToNative[KeyCode.Keypad4] = 75;
            KeyCodeToNative[KeyCode.Keypad5] = 76;
            KeyCodeToNative[KeyCode.Keypad6] = 77;
            KeyCodeToNative[KeyCode.KeypadPlus] = 78;
            KeyCodeToNative[KeyCode.Keypad1] = 79;
            KeyCodeToNative[KeyCode.Keypad2] = 80;
            KeyCodeToNative[KeyCode.Keypad3] = 81;
            KeyCodeToNative[KeyCode.Keypad0] = 82;
            KeyCodeToNative[KeyCode.KeypadPeriod] = 83;
            KeyCodeToNative[KeyCode.F11] = 87;
            KeyCodeToNative[KeyCode.F12] = 88;
            KeyCodeToNative[KeyCode.F13] = 91;
            KeyCodeToNative[KeyCode.F14] = 92;
            KeyCodeToNative[KeyCode.F15] = 93;
            KeyCodeToNative[KeyCode.Underscore] = 115;
            KeyCodeToNative[KeyCode.Comma] = 126;
            KeyCodeToNative[KeyCode.KeypadEquals] = 3597;
            KeyCodeToNative[KeyCode.KeypadEnter] = 3612;
            KeyCodeToNative[KeyCode.RightControl] = 3613;
            KeyCodeToNative[KeyCode.KeypadDivide] = 3637;
            KeyCodeToNative[KeyCode.Print] = 3639;
            KeyCodeToNative[KeyCode.RightAlt] = 3640;
            KeyCodeToNative[KeyCode.Pause] = 3653;
            KeyCodeToNative[KeyCode.Home] = 3655;
            KeyCodeToNative[KeyCode.PageUp] = 3657;
            KeyCodeToNative[KeyCode.End] = 3663;
            KeyCodeToNative[KeyCode.PageDown] = 3665;
            KeyCodeToNative[KeyCode.Insert] = 3666;
            KeyCodeToNative[KeyCode.Delete] = 3667;
            KeyCodeToNative[KeyCode.LeftMeta] = 3675;
            KeyCodeToNative[KeyCode.RightMeta] = 3676;
            KeyCodeToNative[KeyCode.Menu] = 3677;
            KeyCodeToNative[KeyCode.UpArrow] = 57416;
            KeyCodeToNative[KeyCode.LeftArrow] = 57419;
            KeyCodeToNative[KeyCode.Clear] = 57420;
            KeyCodeToNative[KeyCode.RightArrow] = 57421;
            KeyCodeToNative[KeyCode.DownArrow] = 57424;
            KeyCodeToNative[KeyCode.Home] = 60999;
            KeyCodeToNative[KeyCode.UpArrow] = 61000;
            KeyCodeToNative[KeyCode.PageUp] = 61001;
            KeyCodeToNative[KeyCode.LeftArrow] = 61003;
            KeyCodeToNative[KeyCode.Clear] = 61004;
            KeyCodeToNative[KeyCode.RightArrow] = 61005;
            KeyCodeToNative[KeyCode.End] = 61007;
            KeyCodeToNative[KeyCode.DownArrow] = 61008;
            KeyCodeToNative[KeyCode.PageDown] = 61009;
            KeyCodeToNative[KeyCode.Insert] = 61010;
            KeyCodeToNative[KeyCode.Delete] = 61011;
            KeyCodeToNative[KeyCode.Mouse0] = 1001;
            KeyCodeToNative[KeyCode.Mouse1] = 1002;
            KeyCodeToNative[KeyCode.Mouse2] = 1003;
            KeyCodeToNative[KeyCode.Mouse3] = 1004;
            KeyCodeToNative[KeyCode.Mouse4] = 1005;
        }

        internal static bool DrawEase(Ease ease, Action<Ease> setter)
        {
            int selected = (int)ease;
            int num = (UnityModManager.UI.PopupToggleGroup(ref selected, easeNames, "Ease", null) ? 1 : 0);
            setter(eases[selected]);
            return num != 0;
        }

        internal static bool DrawKeyCode(KeyCode code, Action<KeyCode> setter)
        {
            int selected = codeIndex[code];
            int num = (UnityModManager.UI.PopupToggleGroup(ref selected, keyNames, $"{code} KeyCode", null) ? 1 : 0);
            setter(Main.KeyCodes[selected]);
            return num != 0;
        }

        internal static bool DrawSpareKeyCode(KeyCode code, Action<KeyCode> setter)
        {
            int selected = codeIndex[code];
            int num = (UnityModManager.UI.PopupToggleGroup(ref selected, keyNames, $"{code} Spare KeyCode", null) ? 1 : 0);
            setter(Main.KeyCodes[selected]);
            return num != 0;
        }

        internal static void DrawGlobalConfig(Config config, Action<Config> onChange)
        {
            MoreGUILayout.BeginIndent();
            GUILayout.BeginHorizontal();
            string text = MoreGUILayout.NamedTextField(Main.Lang.GetString("FONT"), config.Font, 300f);
            if (text != config.Font)
            {
                config.Font = text;
                onChange(config);
            }
            GUILayout.EndHorizontal();
            bool flag = GUILayout.Toggle(config.RainEnabled, "Raining Key");
            if (flag)
            {
                MoreGUILayout.BeginIndent();
                if (KeyRain.DrawConfigGUI(config.Code, config.RainConfig))
                {
                    config.keyManager.UpdateLayout();
                }
                MoreGUILayout.EndIndent();
            }
            if (config.RainEnabled != flag)
            {
                config.RainEnabled = flag;
                config.keyManager.UpdateKeys();
                onChange(config);
            }
            GUILayout.BeginHorizontal();
            float num = MoreGUILayout.NamedSliderContent(Main.Lang.GetString("WIDTH"), config.Width, -Screen.width, Screen.width, 300f);
            float num2 = MoreGUILayout.NamedSliderContent(Main.Lang.GetString("HEIGHT"), config.Height, -Screen.height, Screen.height, 300f);
            if ((double)num != (double)config.Width)
            {
                config.Width = num;
                onChange(config);
            }
            if ((double)num2 != (double)config.Height)
            {
                config.Height = num2;
                onChange(config);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            float num3 = MoreGUILayout.NamedSliderContent(Main.Lang.GetString("TEXT_OFFSET_X"), config.TextOffsetX, -300f, 300f, 200f);
            float num4 = MoreGUILayout.NamedSliderContent(Main.Lang.GetString("TEXT_OFFSET_Y"), config.TextOffsetY, -300f, 300f, 200f);
            if ((double)num3 != (double)config.TextOffsetX)
            {
                config.TextOffsetX = num3;
                onChange(config);
            }
            if ((double)num4 != (double)config.TextOffsetY)
            {
                config.TextOffsetY = num4;
                onChange(config);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            float num5 = MoreGUILayout.NamedSliderContent(Main.Lang.GetString("COUNT_TEXT_OFFSET_X"), config.CountTextOffsetX, -300f, 300f, 200f);
            float num6 = MoreGUILayout.NamedSliderContent(Main.Lang.GetString("COUNT_TEXT_OFFSET_Y"), config.CountTextOffsetY, -300f, 300f, 200f);
            if ((double)num5 != (double)config.CountTextOffsetX)
            {
                config.CountTextOffsetX = num5;
                onChange(config);
            }
            if ((double)num6 != (double)config.CountTextOffsetY)
            {
                config.CountTextOffsetY = num6;
                onChange(config);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            float num7 = MoreGUILayout.NamedSliderContent(Main.Lang.GetString("TEXT_FONT_SIZE"), config.TextFontSize, 0f, 300f, 200f);
            float num8 = MoreGUILayout.NamedSliderContent(Main.Lang.GetString("COUNT_TEXT_FONT_SIZE"), config.CountTextFontSize, 0f, 300f, 200f);
            if ((double)num7 != (double)config.TextFontSize)
            {
                config.TextFontSize = num7;
                onChange(config);
            }
            if ((double)num8 != (double)config.CountTextFontSize)
            {
                config.CountTextFontSize = num8;
                onChange(config);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            float num9 = MoreGUILayout.NamedSliderContent("Shrink Factor", config.ShrinkFactor, 0f, 10f, 600f);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            float num10 = MoreGUILayout.NamedSliderContent("Ease Duration", config.EaseDuration, 0f, 10f, 600f);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if ((double)num9 != (double)config.ShrinkFactor)
            {
                config.ShrinkFactor = num9;
                onChange(config);
            }
            if ((double)num10 != (double)config.EaseDuration)
            {
                config.EaseDuration = num10;
                onChange(config);
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Ease:");
            if (DrawEase(config.Ease, delegate (Ease ease)
            {
                config.Ease = ease;
            }))
            {
                onChange(config);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            bool flag2;
            if (flag2 = GUILayout.Toggle(config.ChangeBgColorJudge, Main.Lang.GetString("CHANGE_BG_COLOR_FOLLOWING_HITMARGIN")))
            {
                MoreGUILayout.BeginIndent();
                GUILayout.BeginHorizontal();
                var (text2, text3) = MoreGUILayout.NamedTextFieldPair("Too Early Hex:", "Very Early Hex:", config.HitMarginColorHex[0], config.HitMarginColorHex[1], 100f, 120f);
                GUILayout.EndHorizontal();
                if (text2 != config.HitMarginColorHex[0] && ColorUtility.TryParseHtmlString("#" + text2, out var color))
                {
                    config.TooEarlyColor = color;
                    onChange(config);
                }
                if (text3 != config.HitMarginColorHex[1] && ColorUtility.TryParseHtmlString("#" + text3, out color))
                {
                    config.VeryEarlyColor = color;
                    onChange(config);
                }
                GUILayout.BeginHorizontal();
                var (text4, text5) = MoreGUILayout.NamedTextFieldPair("Early Perfect Hex:", "Perfect Hex:", config.HitMarginColorHex[2], config.HitMarginColorHex[3], 100f, 120f);
                GUILayout.EndHorizontal();
                if (text4 != config.HitMarginColorHex[2] && ColorUtility.TryParseHtmlString("#" + text4, out color))
                {
                    config.EarlyPerfectColor = color;
                    onChange(config);
                }
                if (text5 != config.HitMarginColorHex[3] && ColorUtility.TryParseHtmlString("#" + text5, out color))
                {
                    config.PerfectColor = color;
                    onChange(config);
                }
                GUILayout.BeginHorizontal();
                var (text6, text7) = MoreGUILayout.NamedTextFieldPair("Late Perfect Hex:", "Very Late Hex:", config.HitMarginColorHex[4], config.HitMarginColorHex[5], 100f, 120f);
                GUILayout.EndHorizontal();
                if (text6 != config.HitMarginColorHex[4] && ColorUtility.TryParseHtmlString("#" + text6, out color))
                {
                    config.LatePerfectColor = color;
                    onChange(config);
                }
                if (text7 != config.HitMarginColorHex[5] && ColorUtility.TryParseHtmlString("#" + text7, out color))
                {
                    config.VeryLateColor = color;
                    onChange(config);
                }
                GUILayout.BeginHorizontal();
                var (text8, text9) = MoreGUILayout.NamedTextFieldPair("Too Late Hex:", "Multipress Hex:", config.HitMarginColorHex[6], config.HitMarginColorHex[7], 100f, 120f);
                GUILayout.EndHorizontal();
                if (text8 != config.HitMarginColorHex[6] && ColorUtility.TryParseHtmlString("#" + text8, out color))
                {
                    config.TooLateColor = color;
                    onChange(config);
                }
                if (text9 != config.HitMarginColorHex[7] && ColorUtility.TryParseHtmlString("#" + text9, out color))
                {
                    config.MultipressColor = color;
                    onChange(config);
                }
                GUILayout.BeginHorizontal();
                var (text10, text11) = MoreGUILayout.NamedTextFieldPair("Fail Miss Hex:", "Fail Overload Hex:", config.HitMarginColorHex[8], config.HitMarginColorHex[9], 100f, 120f);
                GUILayout.EndHorizontal();
                if (text10 != config.HitMarginColorHex[8] && ColorUtility.TryParseHtmlString("#" + text10, out color))
                {
                    config.FailMissColor = color;
                    onChange(config);
                }
                if (text11 != config.HitMarginColorHex[9] && ColorUtility.TryParseHtmlString("#" + text11, out color))
                {
                    config.FailOverloadColor = color;
                    onChange(config);
                }
                MoreGUILayout.EndIndent();
            }
            if (flag2 != config.ChangeBgColorJudge)
            {
                config.ChangeBgColorJudge = flag2;
                onChange(config);
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label(Main.Lang.GetString("PRESSED_OUTLINE_COLOR"), GUILayout.Width(200f));
            GUILayout.FlexibleSpace();
            GUILayout.Space(8f);
            GUILayout.Label(Main.Lang.GetString("RELEASED_OUTLINE_COLOR"), GUILayout.Width(200f));
            GUILayout.FlexibleSpace();
            GUILayout.Space(20f);
            GUILayout.EndHorizontal();
            MoreGUILayout.BeginIndent();
            var (color2, color3) = MoreGUILayout.ColorRgbaSlidersPair(config.PressedOutlineColor, config.ReleasedOutlineColor);
            if (color2 != config.PressedOutlineColor)
            {
                config.PressedOutlineColor = color2;
                onChange(config);
            }
            if (color3 != config.ReleasedOutlineColor)
            {
                config.ReleasedOutlineColor = color3;
                onChange(config);
            }
            var (text12, text13) = MoreGUILayout.NamedTextFieldPair("Hex:", "Hex:", config.PressedOutlineColorHex, config.ReleasedOutlineColorHex, 100f, 40f);
            if (text12 != config.PressedOutlineColorHex && ColorUtility.TryParseHtmlString("#" + text12, out color2))
            {
                config.PressedOutlineColor = color2;
                onChange(config);
            }
            if (text13 != config.ReleasedOutlineColorHex && ColorUtility.TryParseHtmlString("#" + text13, out color3))
            {
                config.ReleasedOutlineColor = color3;
                onChange(config);
            }
            MoreGUILayout.EndIndent();
            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Main.Lang.GetString("PRESSED_BACKGROUND_COLOR"), GUILayout.Width(200f));
            GUILayout.FlexibleSpace();
            GUILayout.Space(8f);
            GUILayout.Label(Main.Lang.GetString("RELEASED_BACKGROUND_COLOR"), GUILayout.Width(200f));
            GUILayout.FlexibleSpace();
            GUILayout.Space(20f);
            GUILayout.EndHorizontal();
            MoreGUILayout.BeginIndent();
            var (color4, color5) = MoreGUILayout.ColorRgbaSlidersPair(config.PressedBackgroundColor, config.ReleasedBackgroundColor);
            if (color4 != config.PressedBackgroundColor)
            {
                config.PressedBackgroundColor = color4;
                onChange(config);
            }
            if (color5 != config.ReleasedBackgroundColor)
            {
                config.ReleasedBackgroundColor = color5;
                onChange(config);
            }
            var (text14, text15) = MoreGUILayout.NamedTextFieldPair("Hex:", "Hex:", config.PressedBackgroundColorHex, config.ReleasedBackgroundColorHex, 100f, 40f);
            if (text14 != config.PressedBackgroundColorHex && ColorUtility.TryParseHtmlString("#" + text14, out color4))
            {
                config.PressedBackgroundColor = color4;
                onChange(config);
            }
            if (text15 != config.ReleasedBackgroundColorHex && ColorUtility.TryParseHtmlString("#" + text15, out color5))
            {
                config.ReleasedBackgroundColor = color5;
                onChange(config);
            }
            MoreGUILayout.EndIndent();
            GUILayout.Space(8f);
            config.Gradient = GUILayout.Toggle(config.Gradient, "Gradient");
            GUILayout.BeginHorizontal();
            GUILayout.Label(Main.Lang.GetString("PRESSED_TEXT_COLOR"), GUILayout.Width(200f));
            GUILayout.FlexibleSpace();
            GUILayout.Space(8f);
            GUILayout.Label(Main.Lang.GetString("RELEASED_TEXT_COLOR"), GUILayout.Width(200f));
            GUILayout.FlexibleSpace();
            GUILayout.Space(20f);
            GUILayout.EndHorizontal();
            MoreGUILayout.BeginIndent();
            if (config.Gradient)
            {
                var (vertexGradient, vertexGradient2) = MoreGUILayout.VertexGradientSlidersPair(config.PressedTextColor, config.ReleasedTextColor);
                if (vertexGradient.Inequals(config.PressedTextColor))
                {
                    config.PressedTextColor = vertexGradient;
                    onChange(config);
                }
                if (vertexGradient2.Inequals(config.ReleasedTextColor))
                {
                    config.ReleasedTextColor = vertexGradient2;
                    onChange(config);
                }
                GUILayout.BeginHorizontal();
                var (text16, text17) = MoreGUILayout.NamedTextFieldPair("Top Left Hex:", "Top Left Hex:", config.PressedTextColorHex[0], config.ReleasedTextColorHex[0], 100f, 100f);
                GUILayout.EndHorizontal();
                if (text16 != config.PressedTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text16, out color4))
                {
                    VertexGradient pressedTextColor = config.PressedTextColor;
                    config.PressedTextColor = new VertexGradient(color4, pressedTextColor.topRight, pressedTextColor.bottomLeft, pressedTextColor.bottomRight);
                    onChange(config);
                }
                if (text17 != config.ReleasedTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text17, out color5))
                {
                    VertexGradient releasedTextColor = config.ReleasedTextColor;
                    config.ReleasedTextColor = new VertexGradient(color5, releasedTextColor.topRight, releasedTextColor.bottomLeft, releasedTextColor.bottomRight);
                    onChange(config);
                }
                GUILayout.BeginHorizontal();
                var (text18, text19) = MoreGUILayout.NamedTextFieldPair("Top Right Hex:", "Top Right Hex:", config.PressedTextColorHex[1], config.ReleasedTextColorHex[1], 100f, 100f);
                GUILayout.EndHorizontal();
                if (text18 != config.PressedTextColorHex[1] && ColorUtility.TryParseHtmlString("#" + text18, out color4))
                {
                    VertexGradient pressedTextColor2 = config.PressedTextColor;
                    config.PressedTextColor = new VertexGradient(pressedTextColor2.topLeft, color4, pressedTextColor2.bottomLeft, pressedTextColor2.bottomRight);
                    onChange(config);
                }
                if (text19 != config.ReleasedTextColorHex[1] && ColorUtility.TryParseHtmlString("#" + text19, out color5))
                {
                    VertexGradient releasedTextColor2 = config.ReleasedTextColor;
                    config.ReleasedTextColor = new VertexGradient(releasedTextColor2.topLeft, color5, releasedTextColor2.bottomLeft, releasedTextColor2.bottomRight);
                    onChange(config);
                }
                GUILayout.BeginHorizontal();
                var (text20, text21) = MoreGUILayout.NamedTextFieldPair("Bottom Left Hex:", "Bottom Left Hex:", config.PressedTextColorHex[2], config.ReleasedTextColorHex[2], 100f, 100f);
                GUILayout.EndHorizontal();
                if (text20 != config.PressedTextColorHex[2] && ColorUtility.TryParseHtmlString("#" + text20, out color4))
                {
                    VertexGradient pressedTextColor3 = config.PressedTextColor;
                    config.PressedTextColor = new VertexGradient(pressedTextColor3.topLeft, pressedTextColor3.topRight, color4, pressedTextColor3.bottomRight);
                    onChange(config);
                }
                if (text21 != config.ReleasedTextColorHex[2] && ColorUtility.TryParseHtmlString("#" + text21, out color5))
                {
                    VertexGradient releasedTextColor3 = config.ReleasedTextColor;
                    config.ReleasedTextColor = new VertexGradient(releasedTextColor3.topLeft, releasedTextColor3.topRight, color5, releasedTextColor3.bottomRight);
                    onChange(config);
                }
                GUILayout.BeginHorizontal();
                var (text22, text23) = MoreGUILayout.NamedTextFieldPair("Bottom Right Hex:", "Bottom Right Hex:", config.PressedTextColorHex[3], config.ReleasedTextColorHex[3], 100f, 110f);
                GUILayout.EndHorizontal();
                if (text22 != config.PressedTextColorHex[3] && ColorUtility.TryParseHtmlString("#" + text22, out color4))
                {
                    VertexGradient pressedTextColor4 = config.PressedTextColor;
                    config.PressedTextColor = new VertexGradient(pressedTextColor4.topLeft, pressedTextColor4.topRight, pressedTextColor4.bottomLeft, color4);
                    onChange(config);
                }
                if (text23 != config.ReleasedTextColorHex[3] && ColorUtility.TryParseHtmlString("#" + text23, out color5))
                {
                    VertexGradient releasedTextColor4 = config.ReleasedTextColor;
                    config.ReleasedTextColor = new VertexGradient(releasedTextColor4.topLeft, releasedTextColor4.topRight, releasedTextColor4.bottomLeft, color5);
                    onChange(config);
                }
            }
            else
            {
                (color4, color5) = MoreGUILayout.ColorRgbaSlidersPair(config.PressedTextColor.topLeft, config.ReleasedTextColor.topLeft);
                if (color4 != config.PressedTextColor.topLeft)
                {
                    config.PressedTextColor = new VertexGradient(color4);
                    onChange(config);
                }
                if (color5 != config.ReleasedTextColor.topLeft)
                {
                    config.ReleasedTextColor = new VertexGradient(color5);
                    onChange(config);
                }
                var (text24, text25) = MoreGUILayout.NamedTextFieldPair("Hex:", "Hex:", config.PressedTextColorHex[0], config.ReleasedTextColorHex[0], 100f, 40f);
                if (text24 != config.PressedTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text24, out color4))
                {
                    config.PressedTextColor = new VertexGradient(color4);
                    onChange(config);
                }
                if (text25 != config.ReleasedTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text25, out color5))
                {
                    config.ReleasedTextColor = new VertexGradient(color5);
                    onChange(config);
                }
            }
            MoreGUILayout.EndIndent();
            GUILayout.BeginHorizontal();
            GUILayout.Label(Main.Lang.GetString("PRESSED_COUNT_TEXT_COLOR"), GUILayout.Width(200f));
            GUILayout.FlexibleSpace();
            GUILayout.Space(8f);
            GUILayout.Label(Main.Lang.GetString("RELEASED_COUNT_TEXT_COLOR"), GUILayout.Width(200f));
            GUILayout.FlexibleSpace();
            GUILayout.Space(20f);
            GUILayout.EndHorizontal();
            MoreGUILayout.BeginIndent();
            if (config.Gradient)
            {
                var (vertexGradient3, vertexGradient4) = MoreGUILayout.VertexGradientSlidersPair(config.PressedCountTextColor, config.ReleasedCountTextColor);
                if (vertexGradient3.Inequals(config.PressedCountTextColor))
                {
                    config.PressedCountTextColor = vertexGradient3;
                    onChange(config);
                }
                if (vertexGradient4.Inequals(config.ReleasedCountTextColor))
                {
                    config.ReleasedCountTextColor = vertexGradient4;
                    onChange(config);
                }
                GUILayout.BeginHorizontal();
                var (text26, text27) = MoreGUILayout.NamedTextFieldPair("Top Left Hex:", "Top Left Hex:", config.PressedCountTextColorHex[0], config.ReleasedCountTextColorHex[0], 100f, 100f);
                GUILayout.EndHorizontal();
                if (text26 != config.PressedCountTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text26, out color4))
                {
                    VertexGradient pressedTextColor5 = config.PressedTextColor;
                    config.PressedTextColor = new VertexGradient(color4, pressedTextColor5.topRight, pressedTextColor5.bottomLeft, pressedTextColor5.bottomRight);
                    onChange(config);
                }
                if (text27 != config.ReleasedCountTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text27, out color5))
                {
                    VertexGradient releasedTextColor5 = config.ReleasedTextColor;
                    config.ReleasedTextColor = new VertexGradient(color5, releasedTextColor5.topRight, releasedTextColor5.bottomLeft, releasedTextColor5.bottomRight);
                    onChange(config);
                }
                GUILayout.BeginHorizontal();
                var (text28, text29) = MoreGUILayout.NamedTextFieldPair("Top Right Hex:", "Top Right Hex:", config.PressedCountTextColorHex[1], config.ReleasedCountTextColorHex[1], 100f, 100f);
                GUILayout.EndHorizontal();
                if (text28 != config.PressedCountTextColorHex[1] && ColorUtility.TryParseHtmlString("#" + text28, out color4))
                {
                    VertexGradient pressedTextColor6 = config.PressedTextColor;
                    config.PressedTextColor = new VertexGradient(pressedTextColor6.topLeft, color4, pressedTextColor6.bottomLeft, pressedTextColor6.bottomRight);
                    onChange(config);
                }
                if (text29 != config.ReleasedCountTextColorHex[1] && ColorUtility.TryParseHtmlString("#" + text29, out color5))
                {
                    VertexGradient releasedTextColor6 = config.ReleasedTextColor;
                    config.ReleasedTextColor = new VertexGradient(releasedTextColor6.topLeft, color5, releasedTextColor6.bottomLeft, releasedTextColor6.bottomRight);
                    onChange(config);
                }
                GUILayout.BeginHorizontal();
                var (text30, text31) = MoreGUILayout.NamedTextFieldPair("Bottom Left Hex:", "Bottom Left Hex:", config.PressedCountTextColorHex[2], config.ReleasedCountTextColorHex[2], 100f, 100f);
                GUILayout.EndHorizontal();
                if (text30 != config.PressedCountTextColorHex[2] && ColorUtility.TryParseHtmlString("#" + text30, out color4))
                {
                    VertexGradient pressedTextColor7 = config.PressedTextColor;
                    config.PressedTextColor = new VertexGradient(pressedTextColor7.topLeft, pressedTextColor7.topRight, color4, pressedTextColor7.bottomRight);
                    onChange(config);
                }
                if (text31 != config.ReleasedCountTextColorHex[2] && ColorUtility.TryParseHtmlString("#" + text31, out color5))
                {
                    VertexGradient releasedTextColor7 = config.ReleasedTextColor;
                    config.ReleasedTextColor = new VertexGradient(releasedTextColor7.topLeft, releasedTextColor7.topRight, color5, releasedTextColor7.bottomRight);
                    onChange(config);
                }
                GUILayout.BeginHorizontal();
                var (text32, text33) = MoreGUILayout.NamedTextFieldPair("Bottom Right Hex:", "Bottom Right Hex:", config.PressedCountTextColorHex[3], config.ReleasedCountTextColorHex[3], 100f, 110f);
                GUILayout.EndHorizontal();
                if (text32 != config.PressedCountTextColorHex[3] && ColorUtility.TryParseHtmlString("#" + text32, out color4))
                {
                    VertexGradient pressedTextColor8 = config.PressedTextColor;
                    config.PressedTextColor = new VertexGradient(pressedTextColor8.topLeft, pressedTextColor8.topRight, pressedTextColor8.bottomLeft, color4);
                    onChange(config);
                }
                if (text33 != config.ReleasedCountTextColorHex[3] && ColorUtility.TryParseHtmlString("#" + text33, out color5))
                {
                    VertexGradient releasedTextColor8 = config.ReleasedTextColor;
                    config.ReleasedTextColor = new VertexGradient(releasedTextColor8.topLeft, releasedTextColor8.topRight, releasedTextColor8.bottomLeft, color5);
                    onChange(config);
                }
            }
            else
            {
                var (color6, color7) = MoreGUILayout.ColorRgbaSlidersPair(config.PressedCountTextColor.topLeft, config.ReleasedCountTextColor.topLeft);
                if (color6 != config.PressedCountTextColor.topLeft)
                {
                    config.PressedCountTextColor = new VertexGradient(color6);
                    onChange(config);
                }
                if (color7 != config.ReleasedCountTextColor.topLeft)
                {
                    config.ReleasedCountTextColor = new VertexGradient(color7);
                    onChange(config);
                }
                var (text34, text35) = MoreGUILayout.NamedTextFieldPair("Hex:", "Hex:", config.PressedCountTextColorHex[0], config.ReleasedCountTextColorHex[0], 100f, 40f);
                if (text34 != config.PressedCountTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text34, out color6))
                {
                    config.PressedCountTextColor = new VertexGradient(color6);
                    onChange(config);
                }
                if (text35 != config.ReleasedCountTextColorHex[0] && ColorUtility.TryParseHtmlString("#" + text35, out color7))
                {
                    config.ReleasedCountTextColor = new VertexGradient(color7);
                    onChange(config);
                }
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Main.Lang.GetString("RESET")))
            {
                config.Reset();
                onChange(config);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            MoreGUILayout.EndIndent();
            MoreGUILayout.EndIndent();
        }
    }
}
