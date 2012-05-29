using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace FontExport
{
    /// <summary>
    /// GDI·½·¨
    /// </summary>
    public class GdiNativeMethods
    {
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);

        [DllImport("gdi32.dll")]
        public static extern int GetGlyphOutline(IntPtr hdc, uint uChar, uint fuFormat, out GLYPHMETRICS lpgm, uint cbBuffer, IntPtr lpBuffer, ref MAT2 lpmat2);

        [DllImport("Kernel32.dll")]
        public static extern IntPtr CloseHandle(IntPtr hObject);
    }
}
