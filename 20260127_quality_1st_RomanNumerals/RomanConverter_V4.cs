
namespace _20260127_quality_1st_RomanNumerals;

using System;
using System.Linq;


public class RomanConverter_V4
{
    private static readonly Dictionary<char, int> romanToInt = new()
    {
        ['I'] = 1, ['V'] = 5, ['X'] = 10, ['L'] = 50, ['C'] = 100, ['D'] = 500, ['M'] = 1000
    };

    private static int getRomanOrThrow(char c)
    {
        if (!romanToInt.TryGetValue(c, out var value))
            throw new ArgumentException("only I, V, X, L, C, D, M are allowed");
        return value;
    }
    
    public static int ToInteger(string roman)
    {
        if (roman is null) throw new ArgumentNullException(nameof(roman),
            "null is not allowed as an input, only I, V, X, L, C, D, M are allowed");
        if (roman is "") throw new ArgumentException(nameof(roman),
            "an empty string is not allowed");
        List<int?> romanints = roman.Select(x => (int?)getRomanOrThrow(x)).ToList();
        List<(int?,int?)> romanpairs = romanints.Prepend(null).Zip(romanints.Append(null), (a, b) => (a, b)).ToList();
        
        int result = ToIntegerHelper(new List<int>(),romanpairs, 0, roman);
        if (result > 3999) throw new ArgumentException(roman,
                "Numeric value is above maximum allowed: 3999");
        return result;
    }

    private static int ToIntegerHelper(
        List<int> result,
        List<(int?,int?)> romanpairs,
        int repetition,
        string originalRoman)
    {
        int? firstHeadDigit = romanpairs.First().Item1.HasValue ? 
            int.Parse(romanpairs.First().Item1.Value.ToString().First().ToString()) : null;
        int? secondHeadDigit = romanpairs.First().Item2.HasValue ? 
            int.Parse(romanpairs.First().Item2.Value.ToString().First().ToString()) : null;
        int firstGreaterThanSecond = (romanpairs.First().Item1 ?? 0) - (romanpairs.First().Item2 ?? 0);
        // Single source of truth: classify using Decide(...), then act on the decision
        var decision = Decide(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond);

        switch (decision)
        {
            case Decision.End:
                // Last item is null - recursion ends
                return result.Sum();

            case Decision.FirstIter:
                // First item is null - first iteration
                return ToIntegerHelper(
                    [.. result, romanpairs.First().Item2!.Value],
                    romanpairs.Skip(1).ToList(), repetition + 1, originalRoman);

            case Decision.LargerPrecedesSmaller:
                // Larger value precedes smaller: push current and reset repetition
                return ToIntegerHelper(
                    [.. result, romanpairs.First().Item2!.Value],
                    romanpairs.Skip(1).ToList(), 1, originalRoman);

            case Decision.AddRepeat:
                // Equal 1s under repeat limit: fold into last and increment repetition
                return ToIntegerHelper(
                    [.. result[..^1], romanpairs.First().Item2!.Value + result[^1]],
                    romanpairs.Skip(1).ToList(), repetition + 1, originalRoman);

            case Decision.TooManyRepeats:
                // More than three repeats of I/X/C/M are not allowed
                throw new ArgumentException(originalRoman,
                    "Roman numerals cannot repeat more than three times");

            case Decision.RepeatVLD:
                // V, L, D cannot repeat
                throw new ArgumentException(originalRoman,
                    "Roman numerals V, L, and D can not be repeated");

            case Decision.Subtract:
                // Subtractive pair with head 1: compute difference and reset repetition
                return ToIntegerHelper(
                    [.. result[..^1], romanpairs.First().Item2!.Value - result[^1]],
                    romanpairs.Skip(1).ToList(), 1, originalRoman);

            case Decision.IllegalSubtract:
                // Illegal subtract (e.g., V before X)
                throw new ArgumentException(originalRoman,
                    "Invalid Roman numeral substraction");

            default:
                // DefaultUnexpected → our internal invariant
                throw new InternalInvariantViolationException(
                    $"Default arm reached in ToIntegerHelper for tuple " +
                    $"(rep={repetition}, " +
                    $"a={firstHeadDigit?.ToString() ?? "null"}, " +
                    $"b={secondHeadDigit?.ToString() ?? "null"}, " +
                    $"cmp={firstGreaterThanSecond}) while processing '{originalRoman}'");
        }
    }
    
    // Internal decision surface for tests only
    internal enum Decision
    {
        End,
        FirstIter,
        LargerPrecedesSmaller,
        AddRepeat,
        TooManyRepeats,
        RepeatVLD,
        Subtract,
        IllegalSubtract,
        DefaultUnexpected
    }

    // Mirrors the predicates used by the ToIntegerHelper switch.
    // It does not perform any state mutation; it only classifies the current tuple.
    internal static class RomanV4Predicates
    {
        internal static bool IsEnd              (int rep, int? a, int? b, int cmp) => a is int && b is null;
        internal static bool IsFirstIter        (int rep, int? a, int? b, int cmp) => a is null && b is int;
        internal static bool IsLargerPrecedesSmaller(int rep, int? a, int? b, int cmp) => a is int && b is int && cmp > 0;
        internal static bool IsAddRepeat        (int rep, int? a, int? b, int cmp) => rep < 3  && a == 1 && b == 1 && cmp == 0;
        internal static bool IsTooManyRepeats   (int rep, int? a, int? b, int cmp) => rep >= 3 && a == 1 && b == 1 && cmp == 0;
        internal static bool IsRepeatVLD        (int rep, int? a, int? b, int cmp) => a == 5 && b == 5 && cmp == 0;
        internal static bool IsSubtract         (int rep, int? a, int? b, int cmp) => a == 1 && b is int && cmp < 0;
        internal static bool IsIllegalSubtract  (int rep, int? a, int? b, int cmp) => a == 5 && b is int && cmp < 0;
    }

    internal static Decision Decide(
        int repetition,
        int? firstHeadDigit,
        int? secondHeadDigit,
        int firstGreaterThanSecond)
    {
        // DEBUG-only overlap guard: ensure mutual exclusivity of predicates
        #if DEBUG
        int matches = 0;
        if (RomanV4Predicates.IsEnd(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) matches++;
        if (RomanV4Predicates.IsFirstIter(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) matches++;
        if (RomanV4Predicates.IsLargerPrecedesSmaller(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) matches++;
        if (RomanV4Predicates.IsAddRepeat(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) matches++;
        if (RomanV4Predicates.IsTooManyRepeats(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) matches++;
        if (RomanV4Predicates.IsRepeatVLD(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) matches++;
        if (RomanV4Predicates.IsSubtract(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) matches++;
        if (RomanV4Predicates.IsIllegalSubtract(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) matches++;
        if (matches > 1)
            throw new InternalInvariantViolationException("Overlapping decision predicates detected");
        #endif

        if (RomanV4Predicates.IsEnd(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) return Decision.End;
        if (RomanV4Predicates.IsFirstIter(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) return Decision.FirstIter;
        if (RomanV4Predicates.IsLargerPrecedesSmaller(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) return Decision.LargerPrecedesSmaller;
        if (RomanV4Predicates.IsAddRepeat(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) return Decision.AddRepeat;
        if (RomanV4Predicates.IsTooManyRepeats(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) return Decision.TooManyRepeats;
        if (RomanV4Predicates.IsRepeatVLD(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) return Decision.RepeatVLD;
        if (RomanV4Predicates.IsSubtract(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) return Decision.Subtract;
        if (RomanV4Predicates.IsIllegalSubtract(repetition, firstHeadDigit, secondHeadDigit, firstGreaterThanSecond)) return Decision.IllegalSubtract;
        return Decision.DefaultUnexpected;
    }
}
