using Orderstamp.Enums;

namespace Orderstamp.Models
{
    public class Operation
    {
        public char Type { get; }
        public int TID { get; }
        public string? Value { get; }

        private Operation(char type, int tid, string? value)
        {
            Type = type;
            TID = tid;
            Value = value;

            Validate();
        }

        public static Operation Create(char type, int tid, string? value = null)
            => new(type, tid, value);

        public override string ToString()
            => (Type == 'r' || Type == 'w') ? $"{Type}{TID}({Value})" : $"{Type}{TID}";

        public void Begin(State state, int tid)
        {
            if (!state.TransactionTS.ContainsKey(tid) ||
                state.Status.GetValueOrDefault(tid) == TxStatus.Aborted)
            {
                state.TransactionTS[tid] = state.NextTS();
                state.Status[tid] = TxStatus.Active;
                state.TransactionsToRetry.Remove(tid);
            }
        }

        public void Commit(State state, int tid)
        {
            if (state.Status.GetValueOrDefault(tid) != TxStatus.Active)
                return;

            state.Status[tid] = TxStatus.Committed;
            state.FinalOperations.Add($"c{tid}");
        }

        public void Abort(State state, int tid, string reason)
        {
            if (state.Status.GetValueOrDefault(tid) == TxStatus.Aborted)
                return;

            state.Status[tid] = TxStatus.Aborted;

            state.FinalOperations.RemoveAll(o =>
                o.StartsWith($"r{tid}(") ||
                o.StartsWith($"w{tid}(") ||
                o == $"c{tid}");

            if (reason != "explicit abort")
            {
                state.AutoAborts.Add($"a{tid}, abort by {reason}");
                state.TransactionsToRetry.Add(tid);
            }
        }

        public string Write(State state, Operation op)
        {
            Begin(state, op.TID);

            if (state.Status[op.TID] != TxStatus.Active)
                return $"{op} ignored (TX not active)";

            var ts = state.TransactionTS[op.TID];
            var vt = EnsureValue(state, op.Value);

            if (ts < vt.RTS)
            {
                Abort(state, op.TID, $"TS = {ts} < RTS({op.Value}) = {vt.RTS}");
                return $"{op} VIOLATION: TS = {ts} < RTS({op.Value}) = {vt.RTS} -> ABORT T{op.TID}";
            }

            if (ts < vt.WTS)
            {
                Abort(state, op.TID, $"TS = {ts} < WTS({op.Value}) = {vt.WTS}");
                return $"{op} VIOLATION: TS = {ts} < WTS({op.Value}) = {vt.WTS} -> ABORT T{op.TID}";
            }

            vt.WTS = ts;
            state.FinalOperations.Add(op.ToString());

            return $"{op} OK";
        }

        public string Read(State state, Operation op)
        {
            Begin(state, op.TID);

            if (state.Status[op.TID] != TxStatus.Active)
                return $"{op} ignored (TX not active)";

            var vt = EnsureValue(state, op.Value);
            var ts = state.TransactionTS[op.TID];

            if (ts < vt.WTS)
            {
                Abort(state, op.TID, $"TS = {ts} < WTS({op.Value}) = {vt.WTS}");
                return $"{op} VIOLATION: TS = {ts} < WTS({op.Value}) = {vt.WTS} -> ABORT T{op.TID}";
            }

            vt.RTS = Math.Max(vt.RTS, ts);
            state.FinalOperations.Add(op.ToString());

            return $"{op} OK";
        }

        private static ValueTimestamp EnsureValue(State state, string? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (!state.Values.ContainsKey(value))
                state.Values[value] = ValueTimestamp.Create();

            return state.Values[value];
        }

        private void Validate()
        {
            if (!"brwca".Contains(char.ToLower(Type)))
                throw new ArgumentException($"Invalid op '{Type}'.");

            if ((Type is 'r' or 'w') && string.IsNullOrWhiteSpace(Value))
                throw new ArgumentException($"'{Type}{TID}' requires an item.");

            if ((Type is 'b' or 'c' or 'a') && Value is not null)
                throw new ArgumentException($"'{Type}{TID}' must not have a value.");
        }
    }
}
