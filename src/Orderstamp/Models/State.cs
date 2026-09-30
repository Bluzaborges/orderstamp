using Orderstamp.Enums;

namespace Orderstamp.Models
{
    public class State
    {
        public Dictionary<int, int> TransactionTS { get; } = new();
        public Dictionary<int, TxStatus> Status { get; } = new();
        public Dictionary<string, ValueTimestamp> Values { get; } = new();
        public List<string> FinalOperations { get; } = new();
        public List<string> AutoAborts { get; } = new();
        public HashSet<int> TransactionsToRetry { get; } = new();

        private int _tsCounter = 0;
        public int NextTS() => ++_tsCounter;

        public static State Create() => new();
    }
}
