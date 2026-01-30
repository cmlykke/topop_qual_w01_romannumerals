namespace _20260127_quality_1st_RomanNumerals;  // ← adjust if your namespace is different

using System.Linq;

/*
 
 Feedback - 2026-01-30 - Markus
 
 Bare lige som feedback mens jeg lytter til AAI
(Er ikke øvet i C# så tag det med et gram salt)

Du kunne reducere kode og "Checks" rigtig meget 
ved at samle dem der checker om .Count > 0 eller unresolved.First() < romanints.First() 
(Idk hvor gode switch cases er i C#, i JS og Python ville man nok holde sig fra dem)

I samme stil kunne du gemme romanints.First().ToString().First() i en variable, 
reducerer runtime og kode

Stor fan af rekursive function (´▽`ʃ♡ƪ) (selvom de er ueffektive af)
 
 */

public static class RomanConverter_V2
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
            // Largest_value_is_3999_NegativeTest
            (_, _, {Count: 0}) 
                when runningtotal + unresolved.Sum() > 3999 
                => throw new ArgumentException(originalRoman,
                    "Numeric value is above maximum allowed: 3999"),
            
            (_, _, {Count: 0}) 
                => runningtotal + unresolved.Sum(),
            
            // IXCM_can_be_repeated_3_times_NegativeTest
            _ when romanints.First().ToString().First() == '1' && 
                   unresolved.Sum() / (double)romanints.First() == 3 
                => throw new ArgumentException(originalRoman,
                    "Roman numerals cannot repeat more than three times"),
            
            // VLD_can_not_be_repeated_NetagiveTest
            _ when romanints.First().ToString().First() == '5' && 
                   unresolved.Sum() / (double)romanints.First() == 1 
                => throw new ArgumentException(originalRoman,
                    "Roman numerals V, L, and D can not be repeated"),
            
            // Smaller_value_precedes_larger_NegativeTest
            _ when unresolved.Count > 0 && unresolved.First() < romanints.First() 
                                        && unresolved.First().ToString().First() == '5'
                => throw new ArgumentException(originalRoman,
                    "Invalid Roman numeral substraction"),
            
            // Smaller_value_precedes_larger
            _ when unresolved.Count > 0 && unresolved.First() < romanints.First() 
                => ToIntegerHelper(runningtotal - unresolved.Sum(),
                    new List<int>(),  romanints, originalRoman),
            
            // Larger_value_precedes_smaller
            _ when unresolved.Count > 0 && unresolved.First() > romanints.First()   
                => ToIntegerHelper(runningtotal + unresolved.Sum(),
                    new List<int>(),  romanints, originalRoman),
            
            _ => ToIntegerHelper(
                runningtotal,
                unresolved.Append(romanints.First()).ToList(),
                romanints.Skip(1).ToList(),
                originalRoman),
        };
    }
}