using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// A type with one inhabitant
    /// </summary>
    public class Unit
    {
        public static readonly Unit Value = new Unit();

        private Unit() { }
    }
}
