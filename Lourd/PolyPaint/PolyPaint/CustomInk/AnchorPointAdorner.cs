﻿using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace PolyPaint.CustomInk
{
    class AnchorPointAdorner : Adorner
    {
        // Be sure to call the base class constructor.
        public AnchorPointAdorner(UIElement adornedElement)
          : base(adornedElement)
        {
        }

        // A common way to implement an adorner's rendering behavior is to override the OnRender
        // method, which is called by the layout system as part of a rendering pass.
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

            // Some arbitrary drawing implements.
            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Green);
            renderBrush.Opacity = 0.2;
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);
            double renderRadius = 5.0;

            // Draw a circle at each corner.
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var result = base.MeasureOverride(constraint);
            // ... add custom measure code here if desired ...
            InvalidateVisual();
            return result;
        }
    }
}