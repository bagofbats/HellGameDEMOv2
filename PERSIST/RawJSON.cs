using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PERSIST
{
    public class RawJSON
    {
        public string ogmoVersion { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int offsetX { get; set; }
        public int offsetY { get; set; }
        public List<Layer> layers { get; set; }
    }

    public class Layer
    {
        public string name { get; set; }
        public string _eid { get; set; }
        public int offsetX { get; set; }
        public int offsetY { get; set; }
        public int gridCellWidth { get; set; }
        public int gridCellHeight { get; set; }
        public int gridCellsX { get; set; }
        public int gridCellsY { get; set; }
        public string tileset { get; set; }
        public List<int> data { get; set; }
        public int exportMode { get; set; }
        public int arrayMode { get; set; }
        public List<Entity> entities { get; set; }
    }

    public class Entity
    {
        public string name { get; set; }
        public int id { get; set; }
        public string _eid { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int originX { get; set; }
        public int originY { get; set; }
    }

    public static class JsonFileReader
    {
        public static T Read<T>(string filePath)
        {
            string text = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(text);
        }
    }

    public class JSON
    {
        public Rectangle location
        { get; private set; }

        public RawJSON raw
        { get; private set; }

        public JSON(Rectangle location, RawJSON raw)
        {
            this.location = location;
            this.raw = raw;
        }
    }
}
