﻿using PolyPaint.CustomInk;
using PolyPaint.Enums;
using PolyPaint.Templates;
using System;
using System.Collections.Generic;
using System.Windows.Ink;
using System.Windows.Input;

namespace PolyPaint.Services
{
    class DrawingService: ConnectionService
    {
        public event Action<string> JoinCanvasRoom;
        public event Action<Stroke> AddStroke;
        public event Action<CustomStroke> UpdateStroke;

        public DrawingService()
        {
            
        }

        public void Initialize(object o)
        {
            socket.Emit("joinCanvasTest");

            socket.On("joinCanvasTestResponse", (data) =>
            {
                //string joinCanvas = serializer.Deserialize<string>((string)data);

                //JoinCanvasRoom?.Invoke(joinCanvas);
            });

            socket.On("CanvasUpdateTestResponse", (data) =>
            {
                BasicShape updatedStroke = serializer.Deserialize<BasicShape>((string)data);

                UpdateStroke?.Invoke(createStroke(updatedStroke));
            });
        }

        // TODO: Ajouter le nom de la room quand ça va être mis sur le serveur
        public void JoinCanvas()
        {
            socket.Emit("joinCanvasTest");
        }

        public void UpdateShape(string id, int type, string name, ShapeStyle shapeStyle, List<string> links)
        {
            BasicShape updatedShape = new BasicShape()
            {
                id = id,
                type = type,
                name = name,
                shapeStyle = shapeStyle,
                links = links
            };

            socket.Emit("CanvasUpdateTest", serializer.Serialize(updatedShape));
        }

        private CustomStroke createStroke(BasicShape basicShape)
        {
            StylusPointCollection points = new StylusPointCollection();
            points.Add(new StylusPoint(basicShape.shapeStyle.coordinates.x, basicShape.shapeStyle.coordinates.y));

            CustomStroke customStroke;
            StrokeTypes type = (StrokeTypes) basicShape.type;

            switch (type)
            {
                case StrokeTypes.CLASS_SHAPE:
                    customStroke = new ClassStroke(basicShape, points);
                    break;
                case StrokeTypes.ARTIFACT:
                    customStroke = new ActivityStroke(basicShape, points);
                    break;
                case StrokeTypes.ACTIVITY:
                    customStroke = new ArtifactStroke(basicShape, points);
                    break;
                case StrokeTypes.ROLE:
                    customStroke = new ActorStroke(basicShape, points);
                    break;
                //case StrokeTypes.COMMENT:
                //    customStroke = new CommentStroke(points);
                //    break;
                //case StrokeTypes.PHASE:
                //    customStroke = new PhaseStroke(points);
                //    break;
                default:
                    customStroke = new ClassStroke(basicShape, points);
                    break;

            }
            customStroke.guid = Guid.Parse(basicShape.id);

            return customStroke;
        }

    }
}