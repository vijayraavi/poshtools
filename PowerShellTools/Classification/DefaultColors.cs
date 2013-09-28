using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PowerShellTools.Classification
{
    interface IDefaultColors
    {
        Color Attribute { get; }
        Color Comment { get; }
        Color Command { get; }
        Color CommandArgument { get; }
        Color CommandParameter { get; }
        Color Number { get; }
        Color Operator { get; }
        Color Keyword { get; }
        Color String { get; }
        Color Type { get; }
        Color Variable { get; }
        Color Member { get; }
        Color GroupStart { get; }
        Color GroupEnd { get; }
    }

    class LightThemeDefaultColors : IDefaultColors
    {
        public Color Attribute
        {
            get
            {
                return Color.FromRgb(0, 0, 255);
            }
        }
        public Color Comment
        {
            get
            {
                return Color.FromRgb(0, 100, 0);
            }
        }
        public Color Command
        {
            get
            {
                return Color.FromRgb(0, 0, 255);
            }
        }
        public Color CommandArgument
        {
            get
            {
                return Color.FromRgb(138, 43, 226);
            }
        }
        public Color CommandParameter
        {
            get
            {
                return Color.FromRgb(0, 0, 128);
            }
        }
        public Color Number
        {
            get
            {
                return Color.FromRgb(128, 0, 128);
            }
        }
        public Color Operator
        {
            get
            {
                return Color.FromRgb(169, 169, 169);
            }
        }
        public Color Keyword
        {
            get
            {
                return Color.FromRgb(0, 0, 139);
            }
        }
        public Color String
        {
            get
            {
                return Color.FromRgb(139, 0, 0);
            }
        }
        public Color Type
        {
            get
            {
                return Color.FromRgb(0, 128, 128);
            }
        }

        public Color Variable
        {
            get
            {
                return Color.FromRgb(255, 69, 0);
            }
        }
        public Color Member
        {
            get
            {
                return Color.FromRgb(0, 0, 0);
            }
        }

        public Color GroupStart
        {
            get
            {
                return Color.FromRgb(0, 0, 0);
            }
        }
        public Color GroupEnd
        {
            get
            {
                return Color.FromRgb(0, 0, 0);
            }
        }
    }

    class BlueThemeDefaultColors : IDefaultColors
    {
        public Color Attribute
        {
            get
            {
                return Color.FromRgb(0, 0, 255);
            }
        }
        public Color Comment
        {
            get
            {
                return Color.FromRgb(0, 100, 0);
            }
        }
        public Color Command
        {
            get
            {
                return Color.FromRgb(0, 0, 255);
            }
        }
        public Color CommandArgument
        {
            get
            {
                return Color.FromRgb(138, 43, 226);
            }
        }
        public Color CommandParameter
        {
            get
            {
                return Color.FromRgb(0, 0, 128);
            }
        }
        public Color Number
        {
            get
            {
                return Color.FromRgb(128, 0, 128);
            }
        }
        public Color Operator
        {
            get
            {
                return Color.FromRgb(169, 169, 169);
            }
        }
        public Color Keyword
        {
            get
            {
                return Color.FromRgb(0, 0, 139);
            }
        }
        public Color String
        {
            get
            {
                return Color.FromRgb(139, 0, 0);
            }
        }
        public Color Type
        {
            get
            {
                return Color.FromRgb(0, 128, 128);
            }
        }

        public Color Variable
        {
            get
            {
                return Color.FromRgb(255, 69, 0);
            }
        }
        public Color Member
        {
            get
            {
                return Color.FromRgb(0, 0, 0);
            }
        }

        public Color GroupStart
        {
            get
            {
                return Color.FromRgb(0, 0, 0);
            }
        }
        public Color GroupEnd
        {
            get
            {
                return Color.FromRgb(0, 0, 0);
            }
        }
    }

    class DarkThemeDefaultColors : IDefaultColors
    {
        public Color Attribute
        {
            get
            {
                return Color.FromRgb(176, 196, 222);
            }
        }
        public Color Comment
        {
            get
            {
                return Color.FromRgb(152, 251, 152);
            }
        }
        public Color Command
        {
            get
            {
                return Color.FromRgb(0, 0, 255);
            }
        }
        public Color CommandArgument
        {
            get
            {
                return Color.FromRgb(138, 43, 226);
            }
        }
        public Color CommandParameter
        {
            get
            {
                return Color.FromRgb(0, 0, 128);
            }
        }
        public Color Number
        {
            get
            {
                return Color.FromRgb(128, 0, 128);
            }
        }
        public Color Operator
        {
            get
            {
                return Color.FromRgb(169, 169, 169);
            }
        }
        public Color Keyword
        {
            get
            {
                return Color.FromRgb(0, 0, 139);
            }
        }
        public Color String
        {
            get
            {
                return Color.FromRgb(139, 0, 0);
            }
        }
        public Color Type
        {
            get
            {
                return Color.FromRgb(0, 128, 128);
            }
        }

        public Color Variable
        {
            get
            {
                return Color.FromRgb(255, 69, 0);
            }
        }
        public Color Member
        {
            get
            {
                return Color.FromRgb(0, 0, 0);
            }
        }

        public Color GroupStart
        {
            get
            {
                return Color.FromRgb(0, 0, 0);
            }
        }
        public Color GroupEnd
        {
            get
            {
                return Color.FromRgb(0, 0, 0);
            }
        }
    }
}
