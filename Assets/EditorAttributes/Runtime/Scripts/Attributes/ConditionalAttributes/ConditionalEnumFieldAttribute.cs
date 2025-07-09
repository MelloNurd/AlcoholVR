using UnityEngine;

namespace EditorAttributes
{
    /// <summary>
    /// Attribute to show/hide or disable/enable a field based on enum conditions
    /// </summary>
    public class ConditionalEnumFieldAttribute : PropertyAttribute
    {
        public string[] EnumNames { get; private set; }
        public int[] EnumValues { get; private set; }
        public bool[] NegatedValues { get; private set; }

        public ConditionType ConditionType { get; private set; }
        public ConditionResult ConditionResult { get; private set; }

        /// <summary>
        /// Attribute to show/hide or disable/enable a field based on enum conditions
        /// </summary>
        /// <param name="conditionType">How to evaluate the specified enum conditions</param>
        /// <param name="enumName">The name of the enum field to evaluate</param>
        /// <param name="enumValue">The enum value to compare against</param>
        public ConditionalEnumFieldAttribute(ConditionType conditionType, string enumName, object enumValue)
#if UNITY_2023_3_OR_NEWER
        : base(true) 
#endif
        {
            EnumNames = new string[] { enumName };
            EnumValues = new int[] { (int)enumValue };
            ConditionType = conditionType;
            ConditionResult = ConditionResult.ShowHide;
        }

        /// <summary>
        /// Attribute to show/hide or disable/enable a field based on multiple enum conditions
        /// </summary>
        /// <param name="conditionType">How to evaluate the specified enum conditions</param>
        /// <param name="enumNamesAndValues">Pairs of enum names and values (name1, value1, name2, value2, ...)</param>
        public ConditionalEnumFieldAttribute(ConditionType conditionType, params object[] enumNamesAndValues)
#if UNITY_2023_3_OR_NEWER
        : base(true) 
#endif
        {
            int pairCount = enumNamesAndValues.Length / 2;
            EnumNames = new string[pairCount];
            EnumValues = new int[pairCount];
            
            for (int i = 0; i < pairCount; i++)
            {
                EnumNames[i] = (string)enumNamesAndValues[i * 2];
                EnumValues[i] = (int)enumNamesAndValues[i * 2 + 1];
            }
            ConditionType = conditionType;
            ConditionResult = ConditionResult.ShowHide;
        }

        /// <summary>
        /// Attribute to show/hide or disable/enable a field based on multiple enum conditions
        /// </summary>
        /// <param name="conditionType">How to evaluate the specified enum conditions</param>
        /// <param name="conditionResult">What happens to the property when the condition evaluates to true</param>
        /// <param name="enumNamesAndValues">Pairs of enum names and values (name1, value1, name2, value2, ...)</param>
        public ConditionalEnumFieldAttribute(ConditionType conditionType, ConditionResult conditionResult, params object[] enumNamesAndValues)
#if UNITY_2023_3_OR_NEWER
        : base(true) 
#endif
        {
            int pairCount = enumNamesAndValues.Length / 2;
            EnumNames = new string[pairCount];
            EnumValues = new int[pairCount];
            
            for (int i = 0; i < pairCount; i++)
            {
                EnumNames[i] = (string)enumNamesAndValues[i * 2];
                EnumValues[i] = (int)enumNamesAndValues[i * 2 + 1];
            }
            ConditionType = conditionType;
            ConditionResult = conditionResult;
        }

        /// <summary>
        /// Attribute to show/hide or disable/enable a field based on multiple enum conditions with negation
        /// </summary>
        /// <param name="conditionType">How to evaluate the specified enum conditions</param>
        /// <param name="negatedValues">Specify which enum conditions to negate</param>
        /// <param name="enumNamesAndValues">Pairs of enum names and values (name1, value1, name2, value2, ...)</param>
        public ConditionalEnumFieldAttribute(ConditionType conditionType, bool[] negatedValues, params object[] enumNamesAndValues)
#if UNITY_2023_3_OR_NEWER
        : base(true) 
#endif
        {
            int pairCount = enumNamesAndValues.Length / 2;
            EnumNames = new string[pairCount];
            EnumValues = new int[pairCount];
            
            for (int i = 0; i < pairCount; i++)
            {
                EnumNames[i] = (string)enumNamesAndValues[i * 2];
                EnumValues[i] = (int)enumNamesAndValues[i * 2 + 1];
            }
            NegatedValues = negatedValues;
            ConditionType = conditionType;
            ConditionResult = ConditionResult.ShowHide;
        }

        /// <summary>
        /// Attribute to show/hide or disable/enable a field based on multiple enum conditions with negation
        /// </summary>
        /// <param name="conditionType">How to evaluate the specified enum conditions</param>
        /// <param name="conditionResult">What happens to the property when the condition evaluates to true</param>
        /// <param name="negatedValues">Specify which enum conditions to negate</param>
        /// <param name="enumNamesAndValues">Pairs of enum names and values (name1, value1, name2, value2, ...)</param>
        public ConditionalEnumFieldAttribute(ConditionType conditionType, ConditionResult conditionResult, bool[] negatedValues, params object[] enumNamesAndValues)
#if UNITY_2023_3_OR_NEWER
        : base(true) 
#endif
        {
            int pairCount = enumNamesAndValues.Length / 2;
            EnumNames = new string[pairCount];
            EnumValues = new int[pairCount];
            
            for (int i = 0; i < pairCount; i++)
            {
                EnumNames[i] = (string)enumNamesAndValues[i * 2];
                EnumValues[i] = (int)enumNamesAndValues[i * 2 + 1];
            }
            NegatedValues = negatedValues;
            ConditionType = conditionType;
            ConditionResult = conditionResult;
        }
    }
}