﻿using System.Windows.Controls.Primitives;

namespace PolyPaint.CustomInk
{
    public class StrokeAnchorPointThumb: Thumb
    {
        public ShapeStroke stroke;
        public CustomInkCanvas canvas;
        public int number;

        public StrokeAnchorPointThumb(ShapeStroke stroke, CustomInkCanvas canvas, int number) : base()
        {
            this.stroke = stroke;
            this.canvas = canvas;
            this.number = number;
        }


    }

    public class StrokeResizePointThumb : Thumb
    {
        public CustomStroke stroke;
        public CustomInkCanvas canvas;
        public int number;

        public StrokeResizePointThumb(CustomStroke stroke, CustomInkCanvas canvas, int number) : base()
        {
            this.stroke = stroke;
            this.canvas = canvas;
            this.number = number;
        }


    }
}
