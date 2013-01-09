using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parsel
{
    public class IndexedString
    {
        public IndexedString()
        {
        }

        public IndexedString(string str)
        {
            StartAt = 0;
            String = str;
        }

        public string String { get; set; }

        public int StartAt { get; set; }

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
            return new IndexedString
            {
                String = String,
                StartAt = StartAt + n
            };
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
    }
}

