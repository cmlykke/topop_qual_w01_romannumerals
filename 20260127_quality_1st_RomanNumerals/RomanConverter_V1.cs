namespace _20260127_quality_1st_RomanNumerals;  // ← adjust if your namespace is different

using System.Linq;

public static class RomanConverter_V1
{
    private static readonly Dictionary<char, int> romanToInt = new()
    {
        ['I'] = 1, ['V'] = 5, ['X'] = 10, ['L'] = 50, ['C'] = 100, ['D'] = 500, ['M'] = 1000
    };

    private static int GetRomanOrThrow(char c)
    {
        if (!romanToInt.TryGetValue(c, out var value))
            throw new ArgumentException("only I, V, X, L, C, D, M are allowed");
        return value;
    }
    
    public static int ToInteger(string roman)
    {
        if (roman is null)
            throw new ArgumentNullException(nameof(roman),
                "null is not allowed as an input, only I, V, X, L, C, D, M are allowed");
        if (roman is "")
            throw new ArgumentException(nameof(roman),
                "an empty string is not allowed");
        return ToIntegerHelper(roman, new LinkedList<int>(), 0, roman);
    }

    private static int ToIntegerHelper(string roman, LinkedList<int> templist, int runningtotal, string originalRoman)
    {
        if (roman.Length == 0 && templist.Count == 0)
        {
            return runningtotal;
        }

        int headInt = GetRomanOrThrow(roman.First());
        roman = roman.Substring(1);
        
        if (templist.Count == 0 && roman.Length == 0  && runningtotal + headInt > 3999)
        {
            throw new ArgumentException(nameof(originalRoman),
                "Numeric value is above maximum allowed: 3999");
        }
        
        //VLD_can_not_be_repeated_NegativeTests
        if (templist.Count > 0 && templist.Last.Value == headInt && '5'== headInt.ToString().First())
        {
            throw new ArgumentException(nameof(originalRoman),
                "Roman numerals V, L, and D can not be repeated");
        }
        //IXCM_can_be_repeated_3_times_NegativeTests
        if (templist.Count > 2 && templist.Last.Value == headInt)
        {
            throw new ArgumentException(nameof(originalRoman),
                "Roman numerals cannot repeat more than three times");
        }
        
        //Smaller_value_precedes_larger_NegativeTests
        if (templist.Count > 0 && templist.Last.Value < headInt && '5'== templist.Last.Value.ToString().First())
        {
            throw new ArgumentException(nameof(originalRoman),
                "Invalid Roman numeral substraction");
        }
        //Smaller_value_precedes_larger_PositiveTests
        if (templist.Count > 0 && templist.Last.Value < headInt)
        {
            int substract = headInt - templist.Sum();
            return ToIntegerHelper(roman, 
                new LinkedList<int>(), 
                runningtotal + substract, originalRoman);
        }
        
        if (roman.Length == 0) 
            return runningtotal + headInt + templist.Sum();
        
        //Larger_value_precedes_smaller_PositiveTests
        if (templist.Count > 0 && templist.Last.Value > headInt)
        {
            int running = runningtotal + templist.Sum();
            return ToIntegerHelper(roman, 
                new([headInt]), 
                running, originalRoman);
        }
        
        templist.AddLast(headInt);
        return ToIntegerHelper(roman, 
            templist,
            runningtotal, originalRoman); 
    }
}

