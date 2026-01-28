namespace _20260127_quality_1st_RomanNumerals.Tests;

using Xunit;

public class RomanNumeralsTests
{
 
    //Non-roman input tests:
    
    [Theory]
    [InlineData(null)]            // null check
    public void Input_with_null_NegativeTest(string roman)
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => RomanConverter.ToInteger(roman)
        );

        Assert.Contains("null is not allowed as an input, only I, V, X, L, C, D, M are allowed", 
            ex.Message, 
            StringComparison.OrdinalIgnoreCase);
    }
    
    [Theory]
    [InlineData("")]           
    public void Input_with_empty_NegativeTest(string roman)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => RomanConverter.ToInteger(roman)
        );

        Assert.Contains("an empty string is not allowed", 
            ex.Message, 
            StringComparison.OrdinalIgnoreCase);
    }
    
    [Theory]
    [InlineData("V1")]          // digit
    [InlineData("1234567890")]  // only digits
    [InlineData("mcmxciv")]       // lowercase letters
    [InlineData("abcIVxyz")]    // mixed valid + lowercase
    [InlineData("XVHLM")]       // Illegal capital letter
    [InlineData("MCMXCIV!")]    // punctuation
    [InlineData("M CM")]        // space
    [InlineData("Ⅴ")]          // unicode "roman" look-alike (U+2174)
    [InlineData("M∞")]          // other symbol
    public void Input_with_non_roman_characters_NegativeTests(string roman)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => RomanConverter.ToInteger(roman)
        );

        // Optional: make the assertion more specific
        Assert.Contains("only I, V, X, L, C, D, M are allowed", 
            ex.Message, 
            StringComparison.OrdinalIgnoreCase);
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
        int result = RomanConverter.ToInteger(roman);
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
        int result = RomanConverter.ToInteger(roman);
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
        int result = RomanConverter.ToInteger(roman);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("VX")]
    [InlineData("VM")]
    [InlineData("LC")]
    [InlineData("LM")]
    [InlineData("DM")]
    public void Smaller_value_precedes_larger_NegativeTests(string roman)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => RomanConverter.ToInteger(roman)
        );

        Assert.Contains("Invalid Roman numeral substraction", 
            exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Theory]
    [InlineData("II",      2)]
    [InlineData("III",    3)]
    [InlineData("MM",     2000)]
    [InlineData("MMM",      3000)]
    public void IXCM_can_be_repeated_3_times_PositiveTests(string roman, int expected)
    {
        int result = RomanConverter.ToInteger(roman);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("IIII")]
    [InlineData("MMMM")]
    [InlineData("IIIIIIIIII")]
    [InlineData("MMMMMMMMMM")]
    public void IXCM_can_be_repeated_3_times_NegativeTests(string roman)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => RomanConverter.ToInteger(roman)
        );

        Assert.Contains("Roman numerals cannot repeat more than three times", 
            exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Theory]
    [InlineData("VV")]
    [InlineData("DD")]
    [InlineData("VVVVVVVVVV")]
    [InlineData("DDDDDDDDDD")]
    public void VLD_can_not_be_repeated_NegativeTests(string roman)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => RomanConverter.ToInteger(roman)
        );

        Assert.Contains("Roman numerals V, L, and D can not be repeated", 
            exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Theory]
    [InlineData("MMMCMXCIX",      3999)]
    public void Largest_value_is_3999_PositiveTest(string roman, int expected)
    {
        int result = RomanConverter.ToInteger(roman);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("MMMCMXCIXI")]
    //[InlineData("MMMCMXCIXIII")]
    //[InlineData("MMMCMXCIXV")]
    public void Largest_value_is_3999_NegativeTests(string roman)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => RomanConverter.ToInteger(roman)
        );

        Assert.Contains("Numeric value is above maximum allowed: 3999", 
            exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    
}