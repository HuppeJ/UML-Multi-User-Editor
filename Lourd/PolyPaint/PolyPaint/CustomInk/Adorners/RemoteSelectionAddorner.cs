﻿using PolyPaint.CustomInk.Adorners;
using PolyPaint.CustomInk.Strokes;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PolyPaint.CustomInk
{
    class RemoteSelectionAdorner : CustomAdorner
    {
        private CustomStroke stroke;
        private CustomInkCanvas canvas;

        private RectangleGeometry fill;
        private Path border;

        VisualCollection visualChildren;

        // Be sure to call the base class constructor.
        public RemoteSelectionAdorner(UIElement adornedElement, CustomStroke stroke, CustomInkCanvas canvas)
          : base(adornedElement)
        {
            adornedStroke = stroke;

            this.stroke = stroke;
            this.canvas = canvas;
            visualChildren = new VisualCollection(this);

            border = new Path();
            if (stroke is ShapeStroke)
            {
                Point center = stroke.GetCenter();
                RotateTransform rotation = new RotateTransform((stroke as ShapeStroke).shapeStyle.rotation, center.X, center.Y);

                fill = new RectangleGeometry(stroke.GetCustomBound(), 0, 0, rotation);
                border.Data = fill;
                border.StrokeThickness = 2;
            }
            else
            {
                border.Data = stroke.GetGeometry();
                border.StrokeThickness = (stroke as LinkStroke).getThickness();
            }

            border.Stroke = (Brush)new BrushConverter().ConvertFromString("#CC7F7F");

            visualChildren.Add(border);
        }

        /// <summary>
        /// Draw the rotation handle and the outline of
        /// the element.
        /// </summary>
        /// <param name="finalSize">The final area within the 
        /// parent that this element should use to arrange 
        /// itself and its children.</param>
        /// <returns>The actual size used. </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (stroke.GetCustomBound().IsEmpty)
            {
                return finalSize;
            }
            
            border.Arrange(new Rect(new Size(canvas.ActualWidth, canvas.ActualHeight)));
            return finalSize;
        }

        // Override the VisualChildrenCount and 
        // GetVisualChild properties to interface with 
        // the adorner's visual collection.
        protected override int VisualChildrenCount
        {
            get { return visualChildren.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visualChildren[index];
        }
    }
}
