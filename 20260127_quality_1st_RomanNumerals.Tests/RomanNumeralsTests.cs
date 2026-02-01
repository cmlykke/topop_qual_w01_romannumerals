namespace _20260127_quality_1st_RomanNumerals.Tests;

using Xunit;
using System.Collections.Generic;

public class RomanNumeralsTests
{
 
    private static Func<string, int> GenericRomanToInt = RomanConverter_V3.ToInteger;
    private static int bruteForceGenerator_maxLength = 6;
    static readonly char[] bruteForceGeneratorAlphabet = { 'I', 'V', 'X', 'L', 'C', 'D', 'M' };
    
    // brute-force generator test

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
            catch (ArgumentException ex) when (ex.Message.Contains("case should never be reached"))
            {
                Assert.Fail($"Default arm hit for input '{s}'");
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
    
    //Non-roman input tests:
    [Theory]
    [InlineData(null)]           
    public void Input_with_null_NegativeTest(string roman)
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => GenericRomanToInt(roman)
        );

        Assert.Contains("null is not allowed as an input, only I, V, X, L, C, D, M are allowed", 
            ex.Message, 
            StringComparison.Ordinal);
    }
    
    [Theory]
    [InlineData("")]           
    public void Input_with_empty_NegativeTest(string roman)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => GenericRomanToInt(roman)
        );

        Assert.Contains("an empty string is not allowed", 
            ex.Message, 
            StringComparison.Ordinal);
    }
    
    [Theory]
    [InlineData("V1")]          // digit
    [InlineData("1234567890")]  // only digits
    [InlineData("mcmxciv")]     // lowercase letters
    [InlineData("abcIVxyz")]    // mixed valid + lowercase
    [InlineData("XVHLM")]       // Illegal capital letter
    [InlineData("MCMXCIV!")]    // punctuation
    [InlineData("M CM")]        // space
    [InlineData("Ⅴ")]          // unicode "roman" look-alike (U+2174)
    [InlineData("M∞")]          // other symbol
    public void Input_with_non_roman_characters_NegativeTests(string roman)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => GenericRomanToInt(roman)
        );

        Assert.Contains("only I, V, X, L, C, D, M are allowed", 
            ex.Message, 
            StringComparison.Ordinal);
    }
    
    // input that breaks real-world roman numeral rules, regardless of the specification
    
    [Theory]
    // substracting I from C is not illegal according to the specification, 
    // but real roman numerals dont allow it
    [InlineData("IC")]    
    // occilating strings like, IVIVIVIV is allowed by the specification,
    // but real roman numerals dont allow it
    [InlineData("XXXIVIVIVIVIV")]  
    public void Strings_that_break_real_roman_numerals_NegativeTests(string roman)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => GenericRomanToInt(roman)
        );

        Assert.Contains("Input allowed by specification, but not by real world roman numerals.", 
            ex.Message, 
            StringComparison.Ordinal);
    }
    
    //Specification tests:
    
    [Theory]
    [InlineData("I",      1)]
    [InlineData("V",    5)]
    [InlineData("X",     10)]
    [InlineData("L",      50)]
    [InlineData("C",     100)]
    [InlineData("D",      500)]
    [InlineData("M",   1000)]
    public void All_possible_Roman_Letters_PositiveTests(string roman, int expected)
    {
        int result = GenericRomanToInt(roman);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("VI",      6)]
    [InlineData("MI",    1001)]
    [InlineData("XV",     15)]
    [InlineData("MV",      1005)]
    [InlineData("LX",     60)]
    [InlineData("MX",      1010)]
    [InlineData("CL",   150)]
    [InlineData("ML",      1050)]
    [InlineData("DC",     600)]
    [InlineData("MC",      1100)]
    [InlineData("MD",   1500)]
    public void Larger_value_precedes_smaller_PositiveTests(string roman, int expected)
    {
        int result = GenericRomanToInt(roman);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("IV",      4)]
    [InlineData("IM",    999)]
    [InlineData("XL",     40)]
    [InlineData("XM",      990)]
    [InlineData("CD",     400)]
    [InlineData("CM",      900)]
    public void Smaller_value_precedes_larger_PositiveTests(string roman, int expected)
    {
        int result = GenericRomanToInt(roman);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("VX")]
    [InlineData("VM")]
    [InlineData("LC")]
    [InlineData("LM")]
    [InlineData("DM")]
    [InlineData("IVX")]
    public void Smaller_value_precedes_larger_NegativeTests(string roman)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => GenericRomanToInt(roman)
        );

        Assert.Contains("Invalid Roman numeral substraction", 
            exception.Message, StringComparison.Ordinal);
    }
    
    [Theory]
    [InlineData("II",      2)]
    [InlineData("III",    3)]
    [InlineData("MM",     2000)]
    [InlineData("MMM",      3000)]
    public void IXCM_can_be_repeated_3_times_PositiveTests(string roman, int expected)
    {
        int result = GenericRomanToInt(roman);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("IIII")]
    [InlineData("MMMM")]
    [InlineData("IIIIIIIIII")]
    [InlineData("MMMMMMMMMM")]
    public void IXCM_can_be_repeated_3_times_NegativeTests(string roman)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => GenericRomanToInt(roman)
        );

        Assert.Contains("Roman numerals cannot repeat more than three times", 
            exception.Message, StringComparison.Ordinal);
    }
    
    [Theory]
    [InlineData("VV")]
    [InlineData("DD")]
    [InlineData("VVVVVVVVVV")]
    [InlineData("DDDDDDDDDD")]
    [InlineData("IVV")]
    public void VLD_can_not_be_repeated_NegativeTests(string roman)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => GenericRomanToInt(roman)
        );

        Assert.Contains("Roman numerals V, L, and D can not be repeated", 
            exception.Message, StringComparison.Ordinal);
    }
    
    [Theory]
    [InlineData("MMMCMXCIX",      3999)]
    public void Largest_value_is_3999_PositiveTest(string roman, int expected)
    {
        int result = GenericRomanToInt(roman);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("MMMCMXCIXI")]
    [InlineData("MMMCMXCIXIII")]
    [InlineData("MMMCMXCIXV")]
    public void Largest_value_is_3999_NegativeTests(string roman)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => GenericRomanToInt(roman)
        );

        Assert.Contains("Numeric value is above maximum allowed: 3999", 
            exception.Message, StringComparison.Ordinal);
    }
}