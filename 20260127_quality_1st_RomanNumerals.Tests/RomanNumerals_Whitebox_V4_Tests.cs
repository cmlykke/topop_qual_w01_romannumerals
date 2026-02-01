namespace _20260127_quality_1st_RomanNumerals.Tests;

using Xunit;
using System.Collections.Generic;
using System.Linq;
using _20260127_quality_1st_RomanNumerals;

using RomanConverter = _20260127_quality_1st_RomanNumerals.RomanConverter_V4;

// this class is for white-box structural testing
public class RomanNumerals_Whitebox_V4_Tests
{
    private static int bruteForceGenerator_maxLength = 5;
    static readonly char[] bruteForceGeneratorAlphabet = { 'I', 'V', 'X', 'L', 'C', 'D', 'M' };
    
    // In V4, Decide(...) is the single source of truth for classification.
    // We verify both:
    //  - Exhaustiveness: no reachable tuple maps to DefaultUnexpected.
    //  - Non-overlap/order-independence: exactly one internal predicate holds per reachable tuple,
    //    and Decide(...) aligns with that unique predicate.
    
    // test that the helper is exhaustive for all possible combinations of digits
    // (effectively, it does the same as the test ToInteger_DoesNotHit_DefaultArm_On_Reasonable_Lengths)
    // but by using internals instead of public API
    [Fact]
    public void Decide_Is_Exhaustive_For_Reachable_Tuples()
    {
        var digits = new int?[] { null, 1, 5 };
        var failures = new List<string>();

        // repetition matters only for the 1==1 equal case; scan a small but covering range
        for (int repetition = 0; repetition <= 5; repetition++)
        {
            foreach (var a in digits)
            foreach (var b in digits)
            {
                int cmp = (a ?? 0) - (b ?? 0);

                // Only combinations that can actually appear in the helper’s stream
                bool reachable =
                    (a is null && b is int) ||     // first iteration
                    (a is int && b is int) ||      // middle
                    (a is int && b is null);       // end
                if (!reachable) continue;

                var decision = RomanConverter.Decide(repetition, a, b, cmp);
                if (decision == RomanConverter.Decision.DefaultUnexpected)
                {
                    failures.Add($"Unclassified tuple a={a?.ToString() ?? "null"}, b={b?.ToString() ?? "null"}, cmp={cmp}, rep={repetition}");
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }
    
    // Prove non-overlap: for every reachable (rep, a, b, cmp) exactly one predicate is true,
    // and ensure Decide(...) maps to that same unique predicate.
    [Fact]
    public void Predicates_Are_Mutually_Exclusive_And_Aligned_With_Decide()
    {
        var digits = new int?[] { null, 1, 5 };
        var failures = new List<string>();

        for (int rep = 0; rep <= 5; rep++)
        foreach (var a in digits)
        foreach (var b in digits)
        {
            int cmp = (a ?? 0) - (b ?? 0);

            bool reachable =
                (a is null && b is int) ||     // first iteration
                (a is int && b is int) ||      // middle
                (a is int && b is null);       // end
            if (!reachable) continue;

            var preds = new[]
            {
                RomanConverter.RomanV4Predicates.IsEnd(rep, a, b, cmp),
                RomanConverter.RomanV4Predicates.IsFirstIter(rep, a, b, cmp),
                RomanConverter.RomanV4Predicates.IsLargerPrecedesSmaller(rep, a, b, cmp),
                RomanConverter.RomanV4Predicates.IsAddRepeat(rep, a, b, cmp),
                RomanConverter.RomanV4Predicates.IsTooManyRepeats(rep, a, b, cmp),
                RomanConverter.RomanV4Predicates.IsRepeatVLD(rep, a, b, cmp),
                RomanConverter.RomanV4Predicates.IsSubtract(rep, a, b, cmp),
                RomanConverter.RomanV4Predicates.IsIllegalSubtract(rep, a, b, cmp)
            };

            int count = preds.Count(p => p);
            if (count != 1)
            {
                failures.Add($"Expected exactly one predicate true, got {count} for a={a?.ToString() ?? "null"}, b={b?.ToString() ?? "null"}, rep={rep}, cmp={cmp}");
                continue; // No need to also check Decide mapping when exclusivity already failed
            }

            // Verify Decide alignment with the single true predicate
            var decision = RomanConverter.Decide(rep, a, b, cmp);
            var expectedIndex = Array.FindIndex(preds, p => p);
            var actualIndex = decision switch
            {
                RomanConverter.Decision.End => 0,
                RomanConverter.Decision.FirstIter => 1,
                RomanConverter.Decision.LargerPrecedesSmaller => 2,
                RomanConverter.Decision.AddRepeat => 3,
                RomanConverter.Decision.TooManyRepeats => 4,
                RomanConverter.Decision.RepeatVLD => 5,
                RomanConverter.Decision.Subtract => 6,
                RomanConverter.Decision.IllegalSubtract => 7,
                _ => -1
            };
            if (expectedIndex != actualIndex)
            {
                failures.Add($"Decide mismatch for a={a?.ToString() ?? "null"}, b={b?.ToString() ?? "null"}, rep={rep}, cmp={cmp}: expected index {expectedIndex}, actual index {actualIndex}");
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }
    
    // brute-force generator test that ensure that
    // the default arm is never hit in the ToInteger helper 
    // (effectively, it does the same as the test ToInteger_DoesNotHit_DefaultArm_On_Reasonable_Lengths,
    // but by using public API (a roman numeral) instead of internals)
    [Fact]
    public void ToInteger_DoesNotHit_DefaultArm_On_Reasonable_Lengths()
    {
        var samples = GenerateSamples(bruteForceGenerator_maxLength);
        foreach (var s in samples)
        {
            try
            {
                _ = RomanConverter.ToInteger(s);
            }
            catch (_20260127_quality_1st_RomanNumerals.InternalInvariantViolationException)
            {
                // The default arm in the helper was reached — this is a test failure.
                Assert.True(false, $"Default arm hit for input '{s}'");
            }
            catch (ArgumentException)
            {
                // Expected for invalid strings (e.g., VV, LL, DM, bad subtractives, etc.). Ignore.
            }
        }
    }

    private static IEnumerable<string> GenerateSamples(int maxLen)
    {
        var list = new List<string>();
        void Gen(string prefix, int len)
        {
            if (len == 0) { list.Add(prefix); return; }
            foreach (var c in bruteForceGeneratorAlphabet)
                Gen(prefix + c, len - 1);
        }
        for (int l = 1; l <= maxLen; l++) Gen(string.Empty, l);
        return list;
    }
}