using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// A string together with a starting position. Using immutable strings can be costly in terms of memory, so it is 
    /// better to keep the same string around, and also keep track of where the parser is up to.
    /// </summary>
    public struct IndexedString
    {
        public string String;
        public int StartAt;

        public IndexedString(string str, int startAt = 0)
        {
            StartAt = startAt;
            String = str;
        }

        public bool IsEmpty
        {
            get
            {
                return StartAt >= String.Length;
            }
        }

        public int Length
        {
            get
            {
                return String.Length - StartAt;
            }
        }

        public IndexedString Shift(int n)
        {
            return new IndexedString(String, StartAt + n);
        }

        public char this[int index]
        {
            get
            {
                return String[StartAt + index];
            }
        }

        public string Substring(int n, int count)
        {
            return String.Substring(StartAt + n, count);
        }

        public override string ToString()
        {
            return String.Substring(StartAt);
        }
    }
}

