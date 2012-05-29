using System;
using System.Collections.Generic;
using System.Text;

namespace TrueTypeText.Drawing
{
    /// <summary>
    /// ’€œﬂ
    /// </summary>
    public class PolylineDrawing : LineDrawing
    {
        public override void Draw(IDrawingContext context)
        {
            context.Graphics.DrawLines(context.Pen, this.Points.ToArray());
        }
    }
}
