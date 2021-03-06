﻿using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using System;
using PolyPaint.Enums;
using PolyPaint.Templates;
using System.Globalization;

namespace PolyPaint.CustomInk
{
    public class PhaseStroke : ShapeStroke
    {
        private Point topLeft;
        private Point topRight;
        private Point middleLeft;
        private Point middleRight;
        private Point bottomRight;
        private Point bottomLeft;

        public PhaseStroke(StylusPointCollection pts) : base(pts)
        {
            name = "Phase";
            shapeStyle.width = 90;
            shapeStyle.height = 90;
            shapeStyle.coordinates = new Coordinates(pts[pts.Count - 1].ToPoint());

            UpdateShapePoints();

            strokeType = (int)StrokeTypes.PHASE;
        }

        public PhaseStroke(BasicShape basicShape, StylusPointCollection pts) : base(pts, basicShape)
        {

        }

        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            base.DrawCore(drawingContext, drawingAttributes);

            UpdateShapePoints();

            LineSegment topRightSeg = new LineSegment(topRight, true);
            LineSegment middleRightSeg = new LineSegment(middleRight, true);
            LineSegment middleLeftSeg = new LineSegment(middleLeft, true);
            PathSegmentCollection segments = new PathSegmentCollection { topRightSeg, middleRightSeg, middleLeftSeg };
            PathFigure figure = new PathFigure();
            figure.Segments = segments;
            figure.IsClosed = true;
            figure.StartPoint = topLeft;
            PathFigureCollection figures = new PathFigureCollection { figure };
            PathGeometry geometry = new PathGeometry();
            geometry.Figures = figures;

            drawingContext.DrawGeometry(fillColor, pen, geometry);
            
            LineSegment bottomRightSeg = new LineSegment(bottomRight, true);
            LineSegment bottomLeftSeg = new LineSegment(bottomLeft, true);
            PathSegmentCollection segments2 = new PathSegmentCollection { middleRightSeg, bottomRightSeg, bottomLeftSeg };
            PathFigure figure2 = new PathFigure();
            figure2.Segments = segments2;
            figure2.IsClosed = true;
            figure2.StartPoint = middleLeft;
            PathFigureCollection figures2 = new PathFigureCollection { figure2 };
            PathGeometry geometry2 = new PathGeometry();
            geometry2.Figures = figures2;

            drawingContext.DrawGeometry(Brushes.Transparent, pen, geometry2);

            FormattedText formattedText = new FormattedText(name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface("Arial"), 20, Brushes.Black);

            formattedText.MaxTextWidth = shapeStyle.width;
            formattedText.MaxLineCount = 1;
            formattedText.Trimming = TextTrimming.CharacterEllipsis;

            drawingContext.DrawText(formattedText, new Point(GetCustomBound().TopLeft.X + 2, GetCustomBound().TopLeft.Y + 4));
        }

        public override Rect GetBounds()
        {
            return Rect.Empty;
        }

        public override Rect GetCustomBound()
        {
            double width = shapeStyle.width;
            double height = shapeStyle.height;

            Rect rect = new Rect(shapeStyle.coordinates.x, shapeStyle.coordinates.y,
                width, height);

            return rect;
        }

        internal override bool HitTestPoint(Point point)
        {
            RotateTransform rotationTransform = new RotateTransform(shapeStyle.rotation, GetCenter().X, GetCenter().Y);
            return GetCustomBound().Contains(rotationTransform.Inverse.Transform(point));
        }

        internal override bool HitTestPointIncludingEdition(Point point)
        {
            Rect bounds = GetEditingBounds();
            return bounds.Contains(point);
        }

        public override Rect GetEditingBounds()
        {
            Rect bounds = GetCustomBound();
            RotateTransform rotationTransform = new RotateTransform(shapeStyle.rotation, GetCenter().X, GetCenter().Y);
            Point topLeft = rotationTransform.Transform(bounds.TopLeft);
            Point topRight = rotationTransform.Transform(bounds.TopRight);
            Point bottomLeft = rotationTransform.Transform(bounds.BottomLeft);
            Point bottomRight = rotationTransform.Transform(bounds.BottomRight);
            double minX = Math.Min(Math.Min(Math.Min(topLeft.X, topRight.X), bottomLeft.X), bottomRight.X);
            double maxX = Math.Max(Math.Max(Math.Max(topLeft.X, topRight.X), bottomLeft.X), bottomRight.X);
            double minY = Math.Min(Math.Min(Math.Min(topLeft.Y, topRight.Y), bottomLeft.Y), bottomRight.Y);
            double maxY = Math.Max(Math.Max(Math.Max(topLeft.Y, topRight.Y), bottomLeft.Y), bottomRight.Y);

            bounds = new Rect(new Point(minX - 15, minY - 15), new Point(maxX + 15, maxY + 15));
            return bounds;
        }


        private void UpdateShapePoints()
        {
            if (shapeStyle.height < 0.2)
                shapeStyle.height = 0.2;

            double width = shapeStyle.width;
            double height = shapeStyle.height;

            topLeft = shapeStyle.coordinates.ToPoint();

            topRight = new Point(topLeft.X + width, topLeft.Y);

            middleLeft = new Point(topLeft.X, topLeft.Y + 30);

            middleRight = new Point(topLeft.X + width, topLeft.Y + 30);

            bottomLeft = new Point(topLeft.X, topLeft.Y + height);

            bottomRight = new Point(topLeft.X + width, topLeft.Y + height);
        }

        public override Point GetCenter()
        {
            Rect rect = GetCustomBound();
            return new Point(rect.X + shapeStyle.width / 2, rect.Y + shapeStyle.height / 2);
        }
    }
}