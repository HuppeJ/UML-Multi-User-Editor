﻿using PolyPaint.Templates;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PolyPaint.CustomInk
{
    class ClassTextBox : StackPanel
    {
        public CustomTextBox tb1;
        public CustomTextBox tb2;
        public CustomTextBox tb3;

        public ClassTextBox(ClassStroke stroke) : base()
        {
            ShapeStyle shapeStyle = stroke.shapeStyle;
            tb1 = new CustomTextBox(stroke.name, shapeStyle.width, shapeStyle.height / 3);
            tb1.TextAlignment = TextAlignment.Center;

            tb2 = new CustomTextBox(getString(stroke.attributes), shapeStyle.width, shapeStyle.height / 3);
            tb2.MinLines = 3;

            tb3 = new CustomTextBox(getString(stroke.methods), shapeStyle.width, shapeStyle.height / 3);
            tb3.MinLines = 3;

            Orientation = Orientation.Vertical;

            Children.Add(tb1);
            Children.Add(tb2);
            Children.Add(tb3);
        }

        private string getString(List<string> list)
        {
            string completeString = "";

            foreach(string str in list)
            {
                completeString += str;
                completeString += Environment.NewLine;
            }

            return completeString.Trim();
        }
    }
}