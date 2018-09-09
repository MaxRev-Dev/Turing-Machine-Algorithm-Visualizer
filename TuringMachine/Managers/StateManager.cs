using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TuringMachine.Elements;

namespace TuringMachine.Managers
{
    public class StateManager
    {
        public StateManager(char blank)
        {
            BlankMarker = blank;
            History.CollectionChanged += OnHistoryChanged;
        }

        public event EventHandler HistoryChanged;

        public ObservableCollection<RuleState> History { get; private set; } = new ObservableCollection<RuleState>();
        public List<RuleState> Table { get; set; }
        public string Word { get; set; }
        public Strip GetStrip => strip;
        public int Selected => SelectedCell;

        private readonly char BlankMarker;
        private int SelectedCell;
        private RuleState execState;
        private Strip strip;

        protected virtual void OnHistoryChanged(object sender, EventArgs e)
        {
            HistoryChanged?.Invoke(this, e);
        }
        public string GetResult => strip.GetResult;

        public StateMachine.RuleStateEventArgs InitStrip()
        {
            if (string.IsNullOrEmpty(Word)) {
                MessageBox.Show("Can't start with null word");
                return null;
            }
            strip = new Strip(Word, BlankMarker);
            History.Clear();
            SelectedCell = strip.ZeroOffset;
            return InitState();
        }
        private StateMachine.RuleStateEventArgs InitState()
        {
            try
            {
                return new StateMachine.RuleStateEventArgs(execState = Table.Where(x =>
                x.Q == 0 && x.Marker == strip[SelectedCell]).First())
                { State = StateMachine.RuleStateEventArgs.OnState.NewState };
            }
            catch (InvalidOperationException) {  
                throw;
            }
        }
        private StateMachine.RuleStateEventArgs Undo(RuleState state)
        {
            var t = Table.Where(x =>
                           x.Q == state.Q &&
                           x.Marker == state.Marker).First();
            if (t.Previous != null)
                t.ReferenceState = t.GetUndo();
            return new StateMachine.RuleStateEventArgs(t) { State = StateMachine.RuleStateEventArgs.OnState.Undo };
        }

        public StateMachine.RuleStateEventArgs ReverseToState()
        {
            if (History.Count == 0) return null;
            var ex = History.Last();
            var undo = ex.GetUndo();
            undo.GetNextSelected(ref SelectedCell);
            strip[SelectedCell] = undo.SetMarker;
            // by index only - bug => equals overrided
            History.RemoveAt(History.Count - 1);
            return new StateMachine.RuleStateEventArgs(History.Count > 0 ? History.Last() : null)
            {
                State = StateMachine.RuleStateEventArgs.OnState.Undo
            };
        }
        public StateMachine.RuleStateEventArgs GoToState()
        {
            if (strip == null) return InitStrip();
            bool init = false;
            strip.CheckBounds(ref SelectedCell);
            try
            {

                if (History.Count == 0 || execState == null)
                {
                    //init q0 state
                    InitState();
                    init = true;
                }
                else
                    //get next
                    execState = Table.Where(x =>
                           x.Q == execState.ReferenceState.NextQ &&
                           x.Marker == strip[SelectedCell]).First();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Unknown letter under carret");
                return null;
            } //q0 stop 
            if (execState != null &&
                execState.Q == 0 &&
                !init)
                return
                   new StateMachine.RuleStateEventArgs(
                    new FinishedState(execState))
                   {
                       State = StateMachine.RuleStateEventArgs.OnState.Finished |
                                 StateMachine.RuleStateEventArgs.OnState.NewState
                   };

            //setting history state for current exec
            var hi = execState.ReferenceState.Clone();
            hi.SetMarker = strip[SelectedCell];
            execState.SetPrevState(hi);
            //changing cell
            strip[SelectedCell] = execState.ReferenceState.SetMarker;
            //moving marker
            execState.ReferenceState.GetNextSelected(ref SelectedCell);

            //adding to history
            History.Add(execState);
            return new StateMachine.RuleStateEventArgs(execState)
            { State = StateMachine.RuleStateEventArgs.OnState.NewState };
        }


        public void StopFinalize()
        {
            this.execState = null;
            if (string.IsNullOrEmpty(Word)) return;
            strip = new Strip(Word, BlankMarker);
            //History.Clear();
        }

        protected void DebugTest(string word)
        {
            strip = new Strip(word, BlankMarker);
            int s = 0, max = 50;
            SelectedCell = 0;
            execState = null;
            while (s++ < max)
                GoToState();
            while (--s > 0)
                ReverseToState();
            while (s++ < max)
                GoToState();
            while (--s > 0)
                ReverseToState();
            while (s++ < max)
                GoToState();
            while (--s > 0)
                ReverseToState();
            while (s++ < max)
                GoToState();
            while (--s > 0)
                ReverseToState();
        }
    }
}
