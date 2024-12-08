using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngine
{
    public class Font
    {
        // Font properties
        public string? Family { get; set; }
        public int? Size { get; set; }
        public string? Style { get; set; }

        public string? Color { get; set; }

        public Font()
        {
        }

        public Font(string? family, int? size, string? style, string? color)
        {
            Family = family;
            Size = size;
            Style = style;
            Color = color;
        }

        public override string ToString()
        {
            return $"Family: {Family}, Size: {Size}, Style: {Style}, Color: {Color}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Font font = (Font)obj;
            return Family == font.Family && Size == font.Size && Style == font.Style && Color == font.Color;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
