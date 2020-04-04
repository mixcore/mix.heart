using System;
using System.Collections.Generic;

namespace Mix.Heart.Enums
{
    public class MixEnums
    {
        #region Common

        public enum ExpressionMethod
        {
            Eq = 1,
            Lt = 2,
            Gt = 3,
            Lte = 4,
            Gte = 5,
            And = 6,
            Or = 7
        }

        #endregion Common

        public static List<object> EnumToObject(Type enumType)
        {
            List<object> result = new List<object>();
            var values = Enum.GetValues(enumType);
            foreach (var item in values)
            {
                result.Add(new { name = Enum.GetName(enumType, item), value = Enum.ToObject(enumType, item) });
            }
            return result;
        }
    }
}