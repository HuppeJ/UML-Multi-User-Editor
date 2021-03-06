using System.Collections.Generic;

namespace PolyPaint.Templates
{
    public class Canvas
    {
        public string id { get; set; }
        public string name { get; set; }
        public string author { get; set; }
        public string owner { get; set; }
        public int accessibility { get; set; }
        public string password { get; set; }
        public dynamic shapes { get; set; }
        public List<Link> links { get; set; }
        public Coordinates dimensions { get; set; }
        public string thumbnail { get; set; }

        public Canvas()
        {
        }

        public Canvas(string id, string name, string author, string owner, int accessibility, string password, List<BasicShape> shapes, List<Link> links, Coordinates dimensions, string thumbnail)
        {
            this.id = id;
            this.name = name;
            this.author = author;
            this.owner = owner;
            this.accessibility = accessibility;
            this.password = password;
            this.shapes = shapes;
            this.links = links;
            this.dimensions = dimensions;
            this.thumbnail = thumbnail;
        }
    }
}