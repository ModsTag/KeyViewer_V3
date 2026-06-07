using DG.Tweening;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

namespace KeyViewer
{
    public partial class Key
    {
        public class Config
        {
            internal KeyManager keyManager;

            public KeyRain.RainConfig RainConfig = new KeyRain.RainConfig();

            public bool RainEnabled;

            public string Font = "Default";

            public KeyCode Code;

            public KeyCode SpareCode;

            public SpecialKeyType SpecialType;

            public uint Count;

            public bool Gradient;

            public bool Editing;

            public string KeyTitle;

            public bool ChangeBgColorJudge;

            public float Width = 100f;

            public float Height = 100f;

            [XmlIgnore]
            public float offsetX;

            [XmlIgnore]
            public float offsetY;

            public float TextOffsetX;

            public float TextOffsetY;

            public float CountTextOffsetX;

            public float CountTextOffsetY;

            public float ShrinkFactor = 0.9f;

            public float EaseDuration = 0.1f;

            public float TextFontSize = 75f;

            public float CountTextFontSize = 50f;

            public Ease Ease = Ease.OutExpo;

            private Color pressedOutlineColor = Color.white;

            private Color releasedOutlineColor = Color.white;

            private Color pressedBackgroundColor = Color.white;

            private Color releasedBackgroundColor = Color.black.WithAlpha(0.4f);

            private Color tooEarlyColor = new Color(1f, 0f, 0f, 1f);

            private Color veryEarlyColor = new Color(1f, 0.436f, 0.306f, 1f);

            private Color earlyPerfectColor = new Color(0.627f, 1f, 0.306f, 1f);

            private Color perfectColor = new Color(0.376f, 1f, 0.307f, 1f);

            private Color latePerfectColor = new Color(0.627f, 1f, 0.306f, 1f);

            private Color veryLateColor = new Color(1f, 0.435f, 0.306f, 1f);

            private Color tooLateColor = new Color(1f, 0f, 0f, 1f);

            private Color multipressColor = new Color(0f, 1f, 0.93f, 1f);

            private Color failMissColor = new Color(0.851f, 0.346f, 1f, 1f);

            private Color failOverloadColor = new Color(0.851f, 0.346f, 1f, 1f);

            private VertexGradient pressedTextColor = new VertexGradient(Color.black);

            private VertexGradient releasedTextColor = new VertexGradient(Color.white);

            private VertexGradient pressedCountTextColor = new VertexGradient(Color.black);

            private VertexGradient releasedCountTextColor = new VertexGradient(Color.white);

            [XmlIgnore]
            public string PressedOutlineColorHex;

            [XmlIgnore]
            public string ReleasedOutlineColorHex;

            [XmlIgnore]
            public string PressedBackgroundColorHex;

            [XmlIgnore]
            public string ReleasedBackgroundColorHex;

            [XmlIgnore]
            public string[] PressedTextColorHex = new string[4];

            [XmlIgnore]
            public string[] ReleasedTextColorHex = new string[4];

            [XmlIgnore]
            public string[] PressedCountTextColorHex = new string[4];

            [XmlIgnore]
            public string[] ReleasedCountTextColorHex = new string[4];

            [XmlIgnore]
            public string[] HitMarginColorHex = new string[10];

            [XmlIgnore]
            public bool RelativeOffsetApplied;

            [XmlIgnore]
            public float RelativeOffsetX;

            [XmlIgnore]
            public float RelativeOffsetY;

            public float OffsetX
            {
                get
                {
                    return RelativeOffsetApplied ? (offsetX + RelativeOffsetX) : offsetX;
                }
                set
                {
                    offsetX = value;
                }
            }

            public float OffsetY
            {
                get
                {
                    return RelativeOffsetApplied ? (offsetY + RelativeOffsetY) : offsetY;
                }
                set
                {
                    offsetY = value;
                }
            }

            public Color PressedOutlineColor
            {
                get
                {
                    return pressedOutlineColor;
                }
                set
                {
                    pressedOutlineColor = value;
                    PressedOutlineColorHex = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Color ReleasedOutlineColor
            {
                get
                {
                    return releasedOutlineColor;
                }
                set
                {
                    releasedOutlineColor = value;
                    ReleasedOutlineColorHex = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Color PressedBackgroundColor
            {
                get
                {
                    return pressedBackgroundColor;
                }
                set
                {
                    pressedBackgroundColor = value;
                    PressedBackgroundColorHex = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Color ReleasedBackgroundColor
            {
                get
                {
                    return releasedBackgroundColor;
                }
                set
                {
                    releasedBackgroundColor = value;
                    ReleasedBackgroundColorHex = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public VertexGradient PressedTextColor
            {
                get
                {
                    return pressedTextColor;
                }
                set
                {
                    pressedTextColor = value;
                    PressedTextColorHex[0] = ColorUtility.ToHtmlStringRGBA(value.topLeft);
                    PressedTextColorHex[1] = ColorUtility.ToHtmlStringRGBA(value.topRight);
                    PressedTextColorHex[2] = ColorUtility.ToHtmlStringRGBA(value.bottomLeft);
                    PressedTextColorHex[3] = ColorUtility.ToHtmlStringRGBA(value.bottomRight);
                }
            }

            public VertexGradient ReleasedTextColor
            {
                get
                {
                    return releasedTextColor;
                }
                set
                {
                    releasedTextColor = value;
                    ReleasedTextColorHex[0] = ColorUtility.ToHtmlStringRGBA(value.topLeft);
                    ReleasedTextColorHex[1] = ColorUtility.ToHtmlStringRGBA(value.topRight);
                    ReleasedTextColorHex[2] = ColorUtility.ToHtmlStringRGBA(value.bottomLeft);
                    ReleasedTextColorHex[3] = ColorUtility.ToHtmlStringRGBA(value.bottomRight);
                }
            }

            public VertexGradient PressedCountTextColor
            {
                get
                {
                    return pressedCountTextColor;
                }
                set
                {
                    pressedCountTextColor = value;
                    PressedCountTextColorHex[0] = ColorUtility.ToHtmlStringRGBA(value.topLeft);
                    PressedCountTextColorHex[1] = ColorUtility.ToHtmlStringRGBA(value.topRight);
                    PressedCountTextColorHex[2] = ColorUtility.ToHtmlStringRGBA(value.bottomLeft);
                    PressedCountTextColorHex[3] = ColorUtility.ToHtmlStringRGBA(value.bottomRight);
                }
            }

            public VertexGradient ReleasedCountTextColor
            {
                get
                {
                    return releasedCountTextColor;
                }
                set
                {
                    releasedCountTextColor = value;
                    ReleasedCountTextColorHex[0] = ColorUtility.ToHtmlStringRGBA(value.topLeft);
                    ReleasedCountTextColorHex[1] = ColorUtility.ToHtmlStringRGBA(value.topRight);
                    ReleasedCountTextColorHex[2] = ColorUtility.ToHtmlStringRGBA(value.bottomLeft);
                    ReleasedCountTextColorHex[3] = ColorUtility.ToHtmlStringRGBA(value.bottomRight);
                }
            }

            public Color TooEarlyColor
            {
                get
                {
                    return tooEarlyColor;
                }
                set
                {
                    tooEarlyColor = value;
                    HitMarginColorHex[0] = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Color VeryEarlyColor
            {
                get
                {
                    return veryEarlyColor;
                }
                set
                {
                    veryEarlyColor = value;
                    HitMarginColorHex[1] = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Color EarlyPerfectColor
            {
                get
                {
                    return earlyPerfectColor;
                }
                set
                {
                    earlyPerfectColor = value;
                    HitMarginColorHex[2] = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Color PerfectColor
            {
                get
                {
                    return perfectColor;
                }
                set
                {
                    perfectColor = value;
                    HitMarginColorHex[3] = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Color LatePerfectColor
            {
                get
                {
                    return latePerfectColor;
                }
                set
                {
                    latePerfectColor = value;
                    HitMarginColorHex[4] = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Color VeryLateColor
            {
                get
                {
                    return veryLateColor;
                }
                set
                {
                    veryLateColor = value;
                    HitMarginColorHex[5] = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Color TooLateColor
            {
                get
                {
                    return tooLateColor;
                }
                set
                {
                    tooLateColor = value;
                    HitMarginColorHex[6] = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Color MultipressColor
            {
                get
                {
                    return multipressColor;
                }
                set
                {
                    multipressColor = value;
                    HitMarginColorHex[7] = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Color FailMissColor
            {
                get
                {
                    return failMissColor;
                }
                set
                {
                    failMissColor = value;
                    HitMarginColorHex[8] = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Color FailOverloadColor
            {
                get
                {
                    return failOverloadColor;
                }
                set
                {
                    failOverloadColor = value;
                    HitMarginColorHex[9] = ColorUtility.ToHtmlStringRGBA(value);
                }
            }

            public Config()
            {
                Reset();
            }

            public Config(KeyManager manager)
                : this()
            {
                keyManager = manager;
            }

            public Config(KeyManager manager, KeyCode code)
                : this()
            {
                Code = code;
                keyManager = manager;
            }

            public Config(KeyManager manager, SpecialKeyType type)
                : this()
            {
                SpecialType = type;
                keyManager = manager;
            }

            public void Reset()
            {
                RainEnabled = false;
                RainConfig = new KeyRain.RainConfig();
                Width = 100f;
                Height = 100f;
                OffsetX = 0f;
                OffsetY = 0f;
                ShrinkFactor = 0.9f;
                EaseDuration = 0.1f;
                Ease = Ease.OutExpo;
                SpareCode = Code;
                Count = 0u;
                TextFontSize = 75f;
                CountTextFontSize = 50f;
                TextOffsetX = 0f;
                TextOffsetY = 0f;
                CountTextOffsetX = 0f;
                CountTextOffsetY = 0f;
                PressedOutlineColor = Color.white;
                ReleasedOutlineColor = Color.white;
                PressedBackgroundColor = Color.white;
                ReleasedBackgroundColor = Color.black.WithAlpha(0.4f);
                PressedTextColor = new VertexGradient(Color.black);
                ReleasedTextColor = new VertexGradient(Color.white);
                PressedCountTextColor = new VertexGradient(Color.black);
                ReleasedCountTextColor = new VertexGradient(Color.white);
                ChangeBgColorJudge = false;
                TooEarlyColor = new Color(1f, 0f, 0f, 1f);
                VeryEarlyColor = new Color(1f, 0.436f, 0.306f, 1f);
                EarlyPerfectColor = new Color(0.627f, 1f, 0.306f, 1f);
                PerfectColor = new Color(0.376f, 1f, 0.307f, 1f);
                LatePerfectColor = new Color(0.627f, 1f, 0.306f, 1f);
                VeryLateColor = new Color(1f, 0.435f, 0.306f, 1f);
                TooLateColor = new Color(1f, 0f, 0f, 1f);
                MultipressColor = new Color(0f, 1f, 0.93f, 1f);
                FailMissColor = new Color(0.851f, 0.346f, 1f, 1f);
                FailOverloadColor = new Color(0.851f, 0.346f, 1f, 1f);
            }

            public void ApplyConfig(Config config)
            {
                RainEnabled = config.RainEnabled;
                RainConfig = config.RainConfig.Copy();
                Font = config.Font;
                Width = config.Width;
                Height = config.Height;
                OffsetX = config.OffsetX;
                OffsetY = config.OffsetY;
                ShrinkFactor = config.ShrinkFactor;
                EaseDuration = config.EaseDuration;
                Ease = config.Ease;
                TextOffsetX = config.TextOffsetX;
                TextOffsetY = config.TextOffsetY;
                CountTextOffsetX = config.CountTextOffsetX;
                CountTextOffsetY = config.CountTextOffsetY;
                TextFontSize = config.TextFontSize;
                CountTextFontSize = config.CountTextFontSize;
                PressedOutlineColor = config.PressedOutlineColor;
                ReleasedOutlineColor = config.ReleasedOutlineColor;
                PressedBackgroundColor = config.PressedBackgroundColor;
                ReleasedBackgroundColor = config.ReleasedBackgroundColor;
                PressedTextColor = config.PressedTextColor;
                ReleasedTextColor = config.ReleasedTextColor;
                PressedCountTextColor = config.PressedCountTextColor;
                ReleasedCountTextColor = config.ReleasedCountTextColor;
                ChangeBgColorJudge = config.ChangeBgColorJudge;
                TooEarlyColor = config.TooEarlyColor;
                VeryEarlyColor = config.VeryEarlyColor;
                EarlyPerfectColor = config.EarlyPerfectColor;
                PerfectColor = config.PerfectColor;
                LatePerfectColor = config.LatePerfectColor;
                VeryLateColor = config.VeryLateColor;
                TooLateColor = config.TooLateColor;
                MultipressColor = config.MultipressColor;
                FailMissColor = config.FailMissColor;
                FailOverloadColor = config.FailOverloadColor;
            }

            public void ApplyOffsetRelative(Config config)
            {
                RelativeOffsetApplied = true;
                RelativeOffsetX = config.OffsetX;
                RelativeOffsetY = config.OffsetY;
            }

            public void ApplyConfigWithoutOffset(Config config)
            {
                RainEnabled = config.RainEnabled;
                RainConfig = config.RainConfig.Copy();
                Font = config.Font;
                Width = config.Width;
                Height = config.Height;
                ShrinkFactor = config.ShrinkFactor;
                EaseDuration = config.EaseDuration;
                Ease = config.Ease;
                TextOffsetX = config.TextOffsetX;
                TextOffsetY = config.TextOffsetY;
                CountTextOffsetX = config.CountTextOffsetX;
                CountTextOffsetY = config.CountTextOffsetY;
                TextFontSize = config.TextFontSize;
                CountTextFontSize = config.CountTextFontSize;
                PressedOutlineColor = config.PressedOutlineColor;
                ReleasedOutlineColor = config.ReleasedOutlineColor;
                PressedBackgroundColor = config.PressedBackgroundColor;
                ReleasedBackgroundColor = config.ReleasedBackgroundColor;
                PressedTextColor = config.PressedTextColor;
                ReleasedTextColor = config.ReleasedTextColor;
                PressedCountTextColor = config.PressedCountTextColor;
                ReleasedCountTextColor = config.ReleasedCountTextColor;
                ChangeBgColorJudge = config.ChangeBgColorJudge;
                TooEarlyColor = config.TooEarlyColor;
                VeryEarlyColor = config.VeryEarlyColor;
                EarlyPerfectColor = config.EarlyPerfectColor;
                PerfectColor = config.PerfectColor;
                LatePerfectColor = config.LatePerfectColor;
                VeryLateColor = config.VeryLateColor;
                TooLateColor = config.TooLateColor;
                MultipressColor = config.MultipressColor;
                FailMissColor = config.FailMissColor;
                FailOverloadColor = config.FailOverloadColor;
            }

            public void ApplyConfigAll(Config config)
            {
                keyManager = config.keyManager;
                RainEnabled = config.RainEnabled;
                RainConfig = config.RainConfig.Copy();
                Font = config.Font;
                Code = config.Code;
                SpecialType = config.SpecialType;
                Width = config.Width;
                Height = config.Height;
                OffsetX = config.OffsetX;
                OffsetY = config.OffsetY;
                ShrinkFactor = config.ShrinkFactor;
                EaseDuration = config.EaseDuration;
                Ease = config.Ease;
                SpareCode = config.SpareCode;
                Count = config.Count;
                TextFontSize = config.TextFontSize;
                CountTextFontSize = config.CountTextFontSize;
                TextOffsetX = config.TextOffsetX;
                TextOffsetY = config.TextOffsetY;
                CountTextOffsetX = config.CountTextOffsetX;
                CountTextOffsetY = config.CountTextOffsetY;
                PressedOutlineColor = config.PressedOutlineColor;
                ReleasedOutlineColor = config.ReleasedOutlineColor;
                PressedBackgroundColor = config.PressedBackgroundColor;
                ReleasedBackgroundColor = config.ReleasedBackgroundColor;
                PressedTextColor = config.PressedTextColor;
                ReleasedTextColor = config.ReleasedTextColor;
                PressedCountTextColor = config.PressedCountTextColor;
                ReleasedCountTextColor = config.ReleasedCountTextColor;
                ChangeBgColorJudge = config.ChangeBgColorJudge;
                TooEarlyColor = config.TooEarlyColor;
                VeryEarlyColor = config.VeryEarlyColor;
                EarlyPerfectColor = config.EarlyPerfectColor;
                PerfectColor = config.PerfectColor;
                LatePerfectColor = config.LatePerfectColor;
                VeryLateColor = config.VeryLateColor;
                TooLateColor = config.TooLateColor;
                MultipressColor = config.MultipressColor;
                FailMissColor = config.FailMissColor;
                FailOverloadColor = config.FailOverloadColor;
            }

            public Config Copy()
            {
                return new Config
                {
                    keyManager = keyManager,
                    RainEnabled = RainEnabled,
                    RainConfig = RainConfig.Copy(),
                    Font = Font,
                    Code = Code,
                    SpecialType = SpecialType,
                    Width = Width,
                    Height = Height,
                    OffsetX = OffsetX,
                    OffsetY = OffsetY,
                    ShrinkFactor = ShrinkFactor,
                    EaseDuration = EaseDuration,
                    Ease = Ease,
                    SpareCode = SpareCode,
                    Count = Count,
                    TextFontSize = TextFontSize,
                    CountTextFontSize = CountTextFontSize,
                    TextOffsetX = TextOffsetX,
                    TextOffsetY = TextOffsetY,
                    CountTextOffsetX = CountTextOffsetX,
                    CountTextOffsetY = CountTextOffsetY,
                    PressedOutlineColor = PressedOutlineColor,
                    ReleasedOutlineColor = ReleasedOutlineColor,
                    PressedBackgroundColor = PressedBackgroundColor,
                    ReleasedBackgroundColor = ReleasedBackgroundColor,
                    PressedTextColor = PressedTextColor,
                    ReleasedTextColor = ReleasedTextColor,
                    PressedCountTextColor = PressedCountTextColor,
                    ReleasedCountTextColor = ReleasedCountTextColor,
                    ChangeBgColorJudge = ChangeBgColorJudge,
                    TooEarlyColor = TooEarlyColor,
                    VeryEarlyColor = VeryEarlyColor,
                    EarlyPerfectColor = EarlyPerfectColor,
                    PerfectColor = PerfectColor,
                    LatePerfectColor = LatePerfectColor,
                    VeryLateColor = VeryLateColor,
                    TooLateColor = TooLateColor,
                    MultipressColor = MultipressColor,
                    FailMissColor = FailMissColor,
                    FailOverloadColor = FailOverloadColor
                };
            }
        }
    }
}
