using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngine
{
    public class Display
    {
        public int? X { get; set; }
        public int? Y { get; set; }

        public int? Height { get; set; }

        public int? Width { get; set; }

        public Display()
        {
        }

        public Display(int? x, int? y, int? height, int? width)
        {
            X = x;
            Y = y;
            Height = height;
            Width = width;
        }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Height: {Height}, Width: {Width}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Display display = (Display)obj;
            return X == display.X && Y == display.Y && Height == display.Height && Width == display.Width;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Height, Width);
        }


    }
}
