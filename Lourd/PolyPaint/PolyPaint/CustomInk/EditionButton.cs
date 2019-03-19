﻿using PolyPaint.Vues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;

namespace PolyPaint.CustomInk
{
    public class EditionButton : Button
    {
        public CustomStroke stroke;
        public CustomInkCanvas canvas;
        public int number;

        public EditionButton(CustomStroke stroke, CustomInkCanvas canvas) : base()
        {
            this.stroke = stroke;
            this.canvas = canvas;
        }

        protected override void OnClick()
        {
            var parent = canvas.Parent;
            while (!(parent is WindowDrawing))
            {
                parent = LogicalTreeHelper.GetParent(parent);
            }

            WindowDrawing windowDrawing = (WindowDrawing)parent;
            if (windowDrawing != null)
            {
                windowDrawing.RenameSelection();
            }
            
        }

    }
}