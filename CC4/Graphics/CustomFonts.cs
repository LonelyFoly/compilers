using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Fonts;

namespace Graphics
{
    public class CustomFontResolver : IFontResolver
    {
        private static readonly byte[] _fontData = File.ReadAllBytes("fonts/verdana.ttf");

        public byte[] GetFont(string faceName)
        {
            return _fontData;
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo("MyVerdana");
        }
    }
}
