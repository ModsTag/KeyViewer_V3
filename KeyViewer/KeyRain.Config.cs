using System.Xml.Serialization;
using UnityEngine;

namespace KeyViewer
{
    public partial class KeyRain
    {
        public class RainConfig
        {
            public float OffsetX;

            public float OffsetY;

            public float RainSpeed = 400f;

            public float RainWidth = -1f;

            public float RainHeight = -1f;

            public float RainLength = 400f;

            public int RainPoolSize = 25;

            public int Softness = 100;

            public Color RainColor = Color.white;

            public string RainImage;

            public Direction Direction;

            [XmlIgnore]
            public bool ColorExpanded;

            public RainConfig Copy()
            {
                return new RainConfig
                {
                    OffsetX = OffsetX,
                    OffsetY = OffsetY,
                    RainSpeed = RainSpeed,
                    RainWidth = RainWidth,
                    RainHeight = RainHeight,
                    RainLength = RainLength,
                    RainColor = RainColor,
                    RainImage = RainImage,
                    Softness = Softness,
                    Direction = Direction
                };
            }
        }
    }
}
