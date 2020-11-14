using System.Collections.Generic;

namespace MsHelper.MapleLib.WzLib.Util
{
    public class WzVector2
    {
        public float X;

        public float Y;

        public Dictionary<string, float> GetV2()
        {
            return new Dictionary<string, float>
            {
                ["x"] = X, ["y"] = Y
            };
        }

        public WzVector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}