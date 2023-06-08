using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TiledCS;

namespace PERSIST
{
    public class TiledData
    {
        public Rectangle location
        { get; private set; }

        public TiledMap map
        { get; private set; }

        public TiledTileset tst
        { get; private set; }

        public TiledData(Rectangle location, TiledMap map, TiledTileset tst)
        {
            this.location = location;
            this.map = map;
            this.tst = tst;
        }
    }
}
