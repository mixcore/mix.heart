using System;
using System.Collections.Generic;

namespace Mix.Heart.Enums
{
    public class MixHeartEnums
    {
        #region Common

        public enum DisplayDirection
        {
            Asc,
            Desc
        }

        public enum ExpressionMethod
        {
            Eq,
            Lt,
            Gt,
            Lte,
            Gte,
            And,
            Or
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