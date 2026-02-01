namespace _20260127_quality_1st_RomanNumerals.Tests;

using Xunit;
using System.Collections.Generic;
using System.Linq;
using _20260127_quality_1st_RomanNumerals; // to reach RomanConverter_V3


// this class is for white-box structural testing
public class RomanNumerals_Whitebox_Tests
{
    
    private static Func<string, int> GenericRomanToInt = RomanConverter_V3.ToInteger;
    private static int bruteForceGenerator_maxLength = 5;
    static readonly char[] bruteForceGeneratorAlphabet = { 'I', 'V', 'X', 'L', 'C', 'D', 'M' };
    
    // Prove that the ordering of cases in ToIntegerHelper's switch is irrelevant
    // by showing the predicates are mutually exclusive over the reachable state space.
    // We mirror each predicate as a boolean, then check a bunch of random permutations
    // always lead to the same first-match classification as the canonical Decide.
    [Fact]
    public void Switch_Order_Is_Irrelevant_For_Reachable_Tuples()
    {
        var digits = new int?[] { null, 1, 5 };

        // Local predicate list mirroring ToIntegerHelper's switch arms
        var preds = new List<(string name, Func<int,int?,int?,int,bool> pred, RomanConverter_V3.Decision decision)>
        {
            ("End",               (rep,a,b,cmp) => a is int && b is null,                                      RomanConverter_V3.Decision.End),
            ("FirstIter",         (rep,a,b,cmp) => a is null && b is int,                                      RomanConverter_V3.Decision.FirstIter),
            ("LargerPrecedes",    (rep,a,b,cmp) => a is int && b is int && cmp > 0,                            RomanConverter_V3.Decision.LargerPrecedesSmaller),
            ("AddRepeat",         (rep,a,b,cmp) => rep < 3 && a == 1 && b == 1 && cmp == 0,                    RomanConverter_V3.Decision.AddRepeat),
            ("TooManyRepeats",    (rep,a,b,cmp) => rep >= 3 && a == 1 && b == 1 && cmp == 0,                   RomanConverter_V3.Decision.TooManyRepeats),
            ("RepeatVLD",         (rep,a,b,cmp) => a == 5 && b == 5 && cmp == 0,                               RomanConverter_V3.Decision.RepeatVLD),
            ("Subtract",          (rep,a,b,cmp) => a == 1 && b is int && cmp < 0,                              RomanConverter_V3.Decision.Subtract),
            ("IllegalSubtract",   (rep,a,b,cmp) => a == 5 && b is int && cmp < 0,                              RomanConverter_V3.Decision.IllegalSubtract),
        };

        // Generate a stable list of reachable tuples
        var tuples = new List<(int rep, int? a, int? b, int cmp)>();
        for (int repetition = 0; repetition <= 5; repetition++)
        {
            foreach (var a in digits)
            foreach (var b in digits)
            {
                // Filter to only tuples the helper can actually produce
                bool reachable =
                    (a is null && b is int) ||
                    (a is int && b is int) ||
                    (a is int && b is null);
                if (!reachable) continue;

                int cmp = (a ?? 0) - (b ?? 0);
                tuples.Add((repetition, a, b, cmp));
            }
        }

        // Canonical decisions via the implementation
        var canonical = tuples.Select(t => (t, RomanConverter_V3.Decide(t.rep, t.a, t.b, t.cmp))).ToList();

        // Assert mutual exclusivity of mirrored predicates independent of Decide
        // For every reachable tuple, exactly one mirrored predicate must match.
        foreach (var t in tuples)
        {
            int matches = preds.Count(p => p.pred(t.rep, t.a, t.b, t.cmp));
            Assert.Equal(1, matches);
        }

        // Check multiple random permutations of predicate order
        var rng = new Random(12345);
        for (int run = 0; run < 32; run++)
        {
            var permuted = preds.OrderBy(_ => rng.Next()).ToList();

            foreach (var (t, expected) in canonical)
            {
                // Find the first matching predicate in this permutation
                var first = permuted.FirstOrDefault(p => p.pred(t.rep, t.a, t.b, t.cmp));
                var actual = first == default ? RomanConverter_V3.Decision.DefaultUnexpected : first.decision;

                // expected comes from Decide, which mirrors the helper. If multiple predicates
                // overlapped and mapped to different decisions, different orders would change `actual`.
                Assert.Equal(expected, actual);
            }
        }
    }
    
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

                var decision = RomanConverter_V3.Decide(repetition, a, b, cmp);
                if (decision == RomanConverter_V3.Decision.DefaultUnexpected)
                {
                    failures.Add($"Unclassified tuple a={a?.ToString() ?? "null"}, b={b?.ToString() ?? "null"}, cmp={cmp}, rep={repetition}");
                }
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
                _ = GenericRomanToInt(s);
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