using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NDFSerialization.NDFDataTypes.Primitive
{
    /// <summary>
    /// Represents data type for NDF reference. Property name should match at least partially to reference value.
    /// </summary>
    /// <example>$/Path/To/OtherObject</example>
    /// <example>~/Path/To/OtherObject</example>
    /// <example>./Path/To/OtherObject</example>
    /// <example>~/Descriptor_Deck_Division_FR_11e_Para_multi,</example>
    public partial class NDFReference
    {
        private string _initialData;
        
        public string Value { get; set; }

        public NDFReference(string value)
        {
            _initialData = value;

            if (NDFReferenceValueRegex.IsMatch(value))
            {
                var groups = NDFReferenceValueRegex.Match(value).Groups;

                if (groups.ContainsKey("ReferenceValue"))
                {
                    Value = groups["ReferenceValue"].Value;

                    if (string.IsNullOrEmpty(Value))
                    {
                        Debug.WriteLine($"{Value} was is incorrect format");
                    }
                }
            }
        }

        public readonly static Regex NDFReferenceValueRegex = NDFReferenceRegex();

        [GeneratedRegex("[~$.]/(?<ReferenceValue>[\\w\\//\\d]+),?", RegexOptions.IgnoreCase | RegexOptions.Compiled, "ru-RU")]
        public static partial Regex NDFReferenceRegex();
    }
}
