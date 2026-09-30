using System.Text.RegularExpressions;
using Orderstamp.Models;

namespace Orderstamp.Helpers
{
    public static partial class HistoryHelper
    {
        public static List<Operation> ParseHistory(string input)
        {
            input = input.TrimStart('\uFEFF');

            var tokens = TokensRegex()
                .Split(input)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            var rw = ReadWriteRegex();
            var bca = BeginCommitAbortRegex();

            var operations = new List<Operation>();

            foreach (var token in tokens)
            {
                var match = rw.Match(token);

                if (match.Success)
                {
                    var type = char.ToLower(match.Groups[1].Value[0]);
                    var transactionID = int.Parse(match.Groups[2].Value);
                    var value = match.Groups[3].Value;

                    operations.Add(Operation.Create(type, transactionID, value));

                    continue;
                }

                match = bca.Match(token);

                if (match.Success)
                {
                    var type = char.ToLower(match.Groups[1].Value[0]);
                    var transactionID = int.Parse(match.Groups[2].Value);

                    operations.Add(Operation.Create(type, transactionID));
                    continue;
                }

                throw new Exception($"Invalid token: {token}");
            }

            return operations;
        }

        [GeneratedRegex(@"[;,\s]+")]
        private static partial Regex TokensRegex();

        [GeneratedRegex(@"^([rw])(\d+)\(([A-Za-z]\w*)\)$", RegexOptions.IgnoreCase)]
        private static partial Regex ReadWriteRegex();

        [GeneratedRegex(@"^([bca])(\d+)$", RegexOptions.IgnoreCase)]
        private static partial Regex BeginCommitAbortRegex();
    }
}
