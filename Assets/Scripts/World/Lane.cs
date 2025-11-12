using System;

namespace World
{
    public enum LaneNumber {
        Left = -1,
        Center = 0,
        Right = 1
    }
    
    public struct Lane {
        public LaneNumber Number;
        public float Width;
        public float Left;
        public float Right;
        public float Center;

        public Lane(LaneNumber number, float left, float right) 
        {
            Number = number;
            Left = left;
            Right = right;
            Width = right - left;
            Center = left + Width / 2;
        }
    }
}
