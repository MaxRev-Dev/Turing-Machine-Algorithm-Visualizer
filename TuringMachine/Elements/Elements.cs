using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuringMachine.Other;

namespace TuringMachine.Elements
{
    public class State
    {
        public enum Direction
        {
            Stop,
            Left,
            Right,
            //Up, Down
        }
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("MoveDirection")]
        public Direction Dir { get; set; }
        [JsonProperty("SetupMarker")]
        public Char SetMarker { get; set; }
        public int NextQ { get; set; }
        public void GetNextSelected(ref int X)
        {
            switch (Dir)
            {
                case Direction.Left: X -= 1; break;
                case Direction.Right: X += 1; break;              
                case Direction.Stop:
                //case Direction.Down:
               // case Direction.Up:
                default: return;
            }
        }
        private State() { }
        protected State(State state) : this(state.NextQ, state.Dir, state.SetMarker) { }
        public State(int NextQ, Direction dir, char marker)
        {
            this.NextQ = NextQ;
            this.SetMarker = marker;
            this.Dir = dir;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (!(obj is State)) return false;
            var y = obj as State;
            return this.NextQ == y.NextQ && this.SetMarker == y.SetMarker && this.Dir == y.Dir;
        }

        public string GetAsUserInputString()
        {
            var res = this.NextQ.ToString();
            res += '.';
            res += this.SetMarker;
            res += '.';
            res += convMap.Where(x => x.Value == Dir).First().Key;
            return res;
        }

        static readonly Dictionary<char, Direction> convMap = new Dictionary<char, Direction>()
        {
           // {'D', Direction.Down},
           // {'U', Direction.Up},
            {'L', Direction.Left},
            {'R', Direction.Right},
            {'S', Direction.Stop},
        };
        public static State ConvUserInputString(string input)
        {
            State st = new State();
            var x = input.Split(new char[] { '.' }, StringSplitOptions.None);
            if (x.Count() < 3) throw new FormatException("Invalid state format");
            string q = x[0], mark = x[1], dir = x[2];
            if (int.TryParse(q, out var pq))
                st.NextQ = pq;
            else
                throw new FormatException("Invalid state name");

            if (string.IsNullOrEmpty(mark))
                throw new FormatException("Mark not Found");
            st.SetMarker = mark[0];
            if (string.IsNullOrEmpty(dir))
                throw new FormatException("Dir not Found");
            if (!convMap.ContainsKey(dir[0]))
                throw new FormatException("Invalid direction name");
            st.Dir = convMap[dir[0]];
            return st;
        }

        public override string ToString()
        {
            return $"Q:{NextQ.ToString()[0].GetLowerIndex()} R:{SetMarker} D:{Dir.ToString()}";
        }
        public State Clone()
        {
            return (State)this.MemberwiseClone();
        }
    }
    public class FinishedState : RuleState
    {
        public FinishedState(RuleState state) :
            base(state.Q, state.Marker, state.ReferenceState)
        { }
    }
    public class RuleState
    {
        public RuleState(int Q, char Marker, State state)
        {
            this.ReferenceState = state;
            this.Q = Q;
            this.Marker = Marker;
        }
        public int Q { get; }
        public char Marker { get; }
        public State ReferenceState { set; get; }
        [JsonIgnore]
        public State Previous { set; get; }
        public void SetPrevState(State state)
        {
            Previous = state;
        }
        public State GetUndo()
        {
            if (Previous == null) throw new NullReferenceException("Prev state not set");
            State.Direction nDir = State.Direction.Stop;
            switch (ReferenceState.Dir)
            {
                case State.Direction.Stop:
                    break;
                case State.Direction.Left:
                    nDir = State.Direction.Right;
                    break;
                case State.Direction.Right:
                    nDir = State.Direction.Left;
                    break;
                //case State.Direction.Up:
                //    nDir = State.Direction.Down;
                //    break;
                //case State.Direction.Down:
                //    nDir = State.Direction.Up;
                //    break;
                default:
                    break;
            }
            return new State(Previous.NextQ, nDir, Previous.SetMarker);
        }
        public override string ToString()
        {
            return $"Q:{Q} M:{Marker} ToState: {ReferenceState.ToString()}";
        }
        public RuleState Clone()
        {
            return (RuleState)this.MemberwiseClone();
        }
    }

}
