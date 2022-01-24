namespace RecShark.Algorithm
{
    using System.Linq;

    public static class Combiner
    {
        private static string[][] Build(params string[][] values)
        {
            var count = new int[values.Length + 1];
            count[0] = 1;
            for (var i = 0; i < values.Length; i++)
                count[i + 1] = count[i] * values[i].Length;

            return Enumerable.Range(1, count[^1])
                             .Select(i => values.Select((x, k) => x[(i / count[k]) % x.Length]).ToArray())
                             .ToArray();
        }

        // less efficient ?
        // public static List<string> GetAllPossibleCombos(List<List<string>> strings)
        // {
        //     IEnumerable<string> combos = new[] { "" };
        //
        //     foreach (var inner in strings)
        //     {
        //         combos = combos.SelectMany(r => inner.Select(x => r + x));
        //     }
        //
        //     return combos.ToList();
        // }
    }
}
