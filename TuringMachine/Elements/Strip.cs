using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TuringMachine.Elements
{
    public class Strip
    {
        
        public Strip(string word, char blank)
        {
            BlankCell = blank;
            XStrip = new List<char>();
            ZeroOffset = 49;
            XStrip.AddRange(new char[49].Select(x => ' '));
            XStrip.AddRange( word.Select(x => x).ToList());
            XStrip.AddRange(new char[49 - word.Length].Select(x => ' '));
        }
        public Strip()
        {
            int init = 25;
            XStrip = new char[init].Select(x => x = BlankCell).ToList(); 
        } 
        public List<char> XStrip { get; private set; } = new List<char>();
        public List<int> Indexes { get; private set; } = new List<int>();
        public char this[int index]
        {
            get => XStrip[index];
            set => XStrip[index] = value;
        }
        readonly char BlankCell;
        public int ZeroOffset { get; private set; } = 0;

        public string GetResult
        {
            get => new StringBuilder().Append(XStrip.ToArray()).ToString();
        }
        public int[] GetIndexes
        {
            get {
                Indexes.Clear();
                for(int i = 0, ind=-ZeroOffset; i < XStrip.Count; i++) 
                    Indexes.Add(ind++);
                return Indexes.ToArray();
            }
        }

        public void ExpandRight(ref int selected)
        {
            XStrip.AddRange(new char[5].Select(x => x = BlankCell).ToArray());
        }
        public void CheckBounds(ref int selected)
        {
            if (selected <= 0)
                ExpandLeft(ref selected);
            else if (selected >= XStrip.Count - 1)
                ExpandRight(ref selected);
        }
        public void BlankCheck(ref int selected)
        {
            if (XStrip.Where(x => x == BlankCell).Count() > XStrip.Count - 3) return;
            int rmind = XStrip.FindIndex(x => x != BlankCell);
            XStrip.RemoveRange(0, rmind > 3 ? rmind - 3 : rmind);
            selected -= rmind > 3 ? rmind - 3 : rmind;
            rmind = XStrip.FindLastIndex(x => x != BlankCell);
            XStrip.RemoveRange(rmind, XStrip.Count - 1 - rmind);
        }
        public void ExpandLeft(ref int selected)
        {
            selected += 5;
            ZeroOffset += 5;
            XStrip.InsertRange(0, new char[5].Select(x => x = BlankCell).ToArray());
        }

    }
}
