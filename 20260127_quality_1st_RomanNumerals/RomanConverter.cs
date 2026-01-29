namespace _20260127_quality_1st_RomanNumerals;  // ← adjust if your namespace is different

using System.Linq;

public static class RomanConverter
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
        List<int> romanints = roman.Select(getRomanOrThrow).ToList();
        return ToIntegerHelper(0, new List<int>(),  romanints, roman);
    }
    
    private static int ToIntegerHelper(
        int runningtotal,
        List<int> unresolved,
        List<int> romanints,
        string originalRoman)
    {
        return (runningtotal, unresolved, romanints) switch
        {
            // Largest_value_is_3999
            (var runningVar, var unresVar, {Count: 0}) 
                when runningVar + unresVar.Sum() > 3999 
                => throw new ArgumentException(originalRoman,
                    "Numeric value is above maximum allowed: 3999"),
            
            (_, _, {Count: 0}) 
                => runningtotal + unresolved.Sum(),
            
            // IXCM_can_be_repeated_3_times:
            (_, _, var romanTemp) 
                when romanTemp.First().ToString().First() == '1' && 
                     unresolved.Sum() / (double)romanTemp.First() == 3 
                => throw new ArgumentException(originalRoman,
                    "Roman numerals cannot repeat more than three times"),
            
            // VLD_can_not_be_repeated
            (_, _, var romanTemp) 
                when romanTemp.First().ToString().First() == '5' && 
                     unresolved.Sum() / (double)romanTemp.First() == 1 
                => throw new ArgumentException(originalRoman,
                    "Roman numerals V, L, and D can not be repeated"),
            
            // Smaller_value_precedes_larger_Negative
            (var runningVar, var unresVar, var romanVar)
                when unresVar.Count > 0 && unresVar.First() < romanVar.First() 
                                        && unresVar.First().ToString().First() == '5'
                => throw new ArgumentException(originalRoman,
                    "Invalid Roman numeral substraction"),
            
            // Smaller_value_precedes_larger_Positive
            (var runningVar, var unresVar, var romanVar)
                when unresVar.Count > 0 && unresVar.First() < romanVar.First() 
                => ToIntegerHelper(runningVar + romanVar.First() - unresVar.Sum(),
                    new List<int>(),  romanints.Skip(1).ToList(), originalRoman),
            
            // Larger_value_precedes_smaller
            (var runningVar, var unresVar, var romanVar)
                when unresVar.Count > 0 && unresVar.First() > romanVar.First()   
                => ToIntegerHelper(runningVar + unresVar.Sum(),
                    new List<int>(){romanVar.First()},  romanints.Skip(1).ToList(), originalRoman),
            
            _ => ToIntegerHelper(
                runningtotal,
                unresolved.Append(romanints.First()).ToList(),
                romanints.Skip(1).ToList(),
                originalRoman),
        };
    }
}