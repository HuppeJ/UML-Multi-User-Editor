﻿using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using System;
using PolyPaint.Enums;
using PolyPaint.Templates;

namespace PolyPaint.CustomInk
{
    public class ActorStroke : ShapeStroke
    {
        private double headRadiusX;
        private double headRadiusY;
        private Point headCenter;
        private Point neck;
        private Point torso;
        private Point leftHand;
        private Point rightHand;
        private Point hip;
        private Point leftFoot;
        private Point rightFoot;

        private const int WIDTH = 30;
        private const int HEIGHT = 70;

        public ActorStroke(StylusPointCollection pts) : base(pts)
        {
            shapeStyle.coordinates = new Coordinates(pts[pts.Count - 1].ToPoint());

            UpdateShapePoints();

            strokeType = (int)StrokeTypes.ROLE;
        }

        public ActorStroke(BasicShape basicShape, StylusPointCollection pts) : base(pts, basicShape)
        {
            
        }
        
        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            base.DrawCore(drawingContext, drawingAttributes);
            UpdateShapePoints();

            /*
            Rect editionBorder = new Rect(shapeStyle.coordinates.x - 15, shapeStyle.coordinates.y - 15,
                WIDTH * shapeStyle.width + 30, HEIGHT * shapeStyle.height + 30);
            */

            // drawingContext.DrawRectangle(null, pen2, editionBorder);

            drawingContext.DrawEllipse(fillColor, pen, headCenter, headRadiusX, headRadiusY);
            drawingContext.DrawLine(pen, neck, hip);
            drawingContext.DrawLine(pen, torso, leftHand);
            drawingContext.DrawLine(pen, torso, rightHand);
            drawingContext.DrawLine(pen, hip, leftFoot);
            drawingContext.DrawLine(pen, hip, rightFoot);
        }

        public override Rect GetBounds()
        {
            double width = shapeStyle.width * WIDTH;
            double height = shapeStyle.height * HEIGHT;

            Rect rect = new Rect(shapeStyle.coordinates.x, shapeStyle.coordinates.y, 
                width, height);

            return rect;
        }

        internal override bool HitTestPoint(Point point)
        {
            RotateTransform rotationTransform = new RotateTransform(shapeStyle.rotation, GetCenter().X, GetCenter().Y);
            return GetBounds().Contains(rotationTransform.Inverse.Transform(point));
        }

        internal override bool HitTestPointIncludingEdition(Point point)
        {
            Rect editionBorder = new Rect(shapeStyle.coordinates.x - 15, shapeStyle.coordinates.y - 15,
                WIDTH * shapeStyle.width + 30, HEIGHT * shapeStyle.height + 30);
            RotateTransform rotationTransform = new RotateTransform(shapeStyle.rotation, GetCenter().X, GetCenter().Y);
            return editionBorder.Contains(rotationTransform.Inverse.Transform(point));
        }


        private void UpdateShapePoints()
        {
            double width = shapeStyle.width * WIDTH;
            double height = shapeStyle.height * HEIGHT;

            headRadiusX = width / 2;
            headRadiusY = height / 6;

            neck = shapeStyle.coordinates.ToPoint();
            neck.Offset(width / 2, headRadiusY * 2);

            torso = new Point(neck.X, neck.Y + headRadiusY);

            leftHand = new Point(neck.X - width / 2, neck.Y + headRadiusY / 2);

            rightHand = new Point(neck.X + width / 2, neck.Y + headRadiusY / 2);

            hip = new Point(neck.X, neck.Y + headRadiusY * 3);

            leftFoot = new Point(neck.X - width / 2, neck.Y + headRadiusY * 4);

            rightFoot = new Point(neck.X + width / 2, neck.Y + headRadiusY * 4);

            headCenter = new Point(neck.X, neck.Y - headRadiusY);
        }

        public override Point GetCenter()
        {
            Rect rect = GetBounds();
            return new Point (rect.X + shapeStyle.width * WIDTH / 2, rect.Y + shapeStyle.height * HEIGHT / 2);
        }
    }
}