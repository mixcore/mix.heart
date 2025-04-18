﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Extensions;
using Mix.Heart.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Mix.Heart.Helpers
{
    public class ReflectionHelper
    {
        private static Newtonsoft.Json.JsonSerializer Serializer = InitSerializer();
        private static JsonSerializerOptions SystemSerializer = InitSystemSerializer();

        private static JsonSerializerOptions InitSystemSerializer()
        {
            var serializer = new JsonSerializerOptions
            {
                WriteIndented = false
            };
            serializer.Converters.Add(new JTokenConverter());
            return serializer;
        }

        private static Newtonsoft.Json.JsonSerializer InitSerializer()
        {
            var serializer = new Newtonsoft.Json.JsonSerializer()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            serializer.Converters.Add(new StringEnumConverter());
            return serializer;
        }

        #region Binary
        public static byte[] ToByteArray<T>(T obj)
        {
            if (obj == null)
                return null;

            return
                JsonSerializer.SerializeToUtf8Bytes(obj, SystemSerializer);
        }

        public static T FromByteArray<T>(byte[] data)
        {
            if (data == null)
                return default;
            return
                JsonSerializer.Deserialize<T>(data, SystemSerializer);
        }
        #endregion

        public static JArray ParseArray<T>(T obj)
        {
            return obj != null ? JArray.FromObject(obj, Serializer) : null;
        }

        public static JObject ParseObject<T>(T obj)
        {
            return obj != null ? JObject.FromObject(obj, Serializer) : null;
        }

        public static T ParseStringToObject<T>(string obj)
        {
            try
            {
                var jsonObject = JObject.Parse(obj);
                return jsonObject.ToObject<T>();
            }
            catch (Exception ex)
            {
                throw new MixException(MixErrorStatus.Badrequest, ex);
            }
        }

        public static T ParseStringToArray<T>(string obj)
        {
            try
            {
                var jsonObject = JArray.Parse(obj);
                return jsonObject.ToObject<T>();
            }
            catch (Exception ex)
            {
                throw new MixException(MixErrorStatus.Badrequest, ex);
            }
        }

        public static Newtonsoft.Json.JsonSerializer FormattingData()
        {
            var jsonSerializersettings = new Newtonsoft.Json.JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return jsonSerializersettings;
        }

        public static JObject CamelCaseData(JObject jObject)
        {
            dynamic camelCaseData =
            Newtonsoft.Json.JsonConvert.DeserializeObject(jObject.ToString(), new Newtonsoft.Json.JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            return JObject.FromObject(camelCaseData, FormattingData());
        }

        public static Dictionary<string, string> ConvertObjectToDictionary(object someObject)
        {
            return someObject.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(someObject, null)?.ToString());
        }
        public static bool AreEqual<T>(T obj1, T obj2)
        {
            if (obj1 == null || obj2 == null)
                return obj1 == null && obj2 == null;
            Type type = typeof(T);
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                object value1 = property.GetValue(obj1);
                object value2 = property.GetValue(obj2);
                if (value1 == null || value2 == null)
                {
                    if (value1 != value2) // One is null and the other is not
                        return false;
                }
                else if (property.PropertyType != typeof(string) && property.PropertyType.IsClass)
                {
                    if (!AreEqual(value1, value2))
                    {
                        return false;
                    }
                }
                else if (!value1.Equals(value2))
                {
                    return false;
                }

            }
            return true;
        }
        public static TSource CloneObject<TSource>(TSource sourceObject, TSource destinationObject = default)
            where TSource : class
        {
            destinationObject ??= (TSource)Activator.CreateInstance(typeof(TSource));
            Map(sourceObject, destinationObject);
            return destinationObject;
        }

        public static Expression<Func<TEntity, bool>> BuildExpressionByKeys<TEntity, TDbContext>(
            TEntity model,
            TDbContext context) where TDbContext : DbContext
        {

            Expression<Func<TEntity, bool>> predicate = null;

            foreach (var item in context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties)
            {
                var pre = GetExpression<TEntity>(
                        item.Name,
                        GetPropertyValue(model, item.Name),
                        ExpressionMethod.Equal);
                predicate =
                    predicate == null ? pre
                    : predicate.AndAlso(pre);
            }
            return predicate;
        }

        public static string[] GetKeyMembers<TEntity>(TEntity model)
            where TEntity : IModel
        {
            return model.FindEntityType(typeof(TEntity))
                .FindPrimaryKey().Properties.Select(x => x.Name)
                .ToArray();
        }

        public static JObject GetMembers<TEntity>(
            TEntity model,
            string[] selectMembers,
            Newtonsoft.Json.JsonSerializer serializer = null) where TEntity : class
        {
            var result = JObject.FromObject(model, serializer ?? Serializer)
                .Properties()
                .Where(p => selectMembers.Any(m => m.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase)));

            return new JObject() { result };
        }

        public static Type GetPropertyType(Type type, string name)
        {
            Type fieldPropertyType;
            FieldInfo fieldInfo = type.GetField(name);

            if (fieldInfo == null)
            {
                PropertyInfo propertyInfo = type.GetProperty(name);
                fieldPropertyType = propertyInfo?.PropertyType;
            }
            else
            {
                fieldPropertyType = fieldInfo.FieldType;
            }

            return fieldPropertyType;
        }

        public static bool HasProperty(Type type, string propertyName)
        {
            return type.GetProperty(propertyName.ToTitleCase()) != null;
        }

        public static object GetPropertyValue<T>(T data, string fieldName)
        {
            var prop = data.GetType().GetProperty(fieldName);
            if (prop != null)
            {
                return prop.GetValue(data);
            }
            return default;
        }

        /// <summary>
        /// Gets the lambda.
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        /// <param name="isGetDefault">if set to <c>true</c> [is get
        /// default].</param> <returns></returns>
        public static LambdaExpression GetLambda<TModel>(string propName, bool isGetDefault = true)
        {
            var parameter = Expression.Parameter(typeof(TModel));
            var type = typeof(TModel);
            var prop = Array.Find(type.GetProperties(), p => p.Name == propName);
            if (prop == null && isGetDefault)
            {
                propName = type.GetProperties().FirstOrDefault()?.Name;
            }
            var memberExpression = Expression.Property(parameter, propName);
            return Expression.Lambda(memberExpression, parameter);
        }

        public static void SetPropertyValue<T>(T data, EntityPropertyModel property) where T : class
        {
            var prop = data.GetType().GetProperty(property.PropertyName.ToTitleCase());
            if (prop != null)
            {
                object val = property.PropertyValue;
                if (prop.PropertyType.BaseType == typeof(Enum))
                {
                    val = Enum.Parse(prop.PropertyType, property.PropertyValue.ToString());
                }
                prop.SetValue(data, val);
            }
        }

        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
            where TDestination : class
        {
            if (source is null)
            {
                return default;
            }

            var inPropDict = typeof(TSource)
                .GetProperties()
                .Where(p => p.CanRead)
                .DistinctBy(p => p.Name)
                .ToDictionary(p => p.Name);

            var outProps = typeof(TDestination)
                .GetProperties()
                .Where(p => p.CanWrite);

            foreach (var outProp in outProps)
            {
                if (inPropDict.TryGetValue(outProp.Name, out var inProp))
                {
                    object sourceValue = inProp.GetValue(source);
                    outProp.SetValue(destination, sourceValue);
                }
            }

            return destination;
        }

        public static Expression<Func<T, bool>> GetExpression<T>(
            string propertyName,
            object propertyValue,
            ExpressionMethod expressionMethod,
            string name = "model")
        {
            Type type = typeof(T);
            var par = Expression.Parameter(type, name);

            Type fieldPropertyType;
            Expression fieldPropertyExpression;

            FieldInfo fieldInfo = type.GetField(propertyName.ToTitleCase());

            if (fieldInfo != null)
            {
                fieldPropertyType = fieldInfo.FieldType;
                fieldPropertyExpression = Expression.Field(par, fieldInfo);
            }
            else
            {
                var propertyInfo = type.GetProperty(propertyName.ToTitleCase());
                if (propertyInfo == null)
                {
                    return null;
                }

                fieldPropertyType = propertyInfo.PropertyType;
                fieldPropertyExpression = Expression.Property(par, propertyInfo);
            }

            object parsedValue;
            if (fieldPropertyType.IsGenericType &&
                fieldPropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                TypeConverter typeConverter = TypeDescriptor
                    .GetConverter(fieldPropertyType);

                parsedValue = typeConverter.ConvertFrom(propertyValue);
            }
            else
            {
                if (fieldPropertyType.BaseType == typeof(Enum))
                {
                    parsedValue = Enum.Parse(fieldPropertyType, propertyValue.ToString());
                }
                else
                {
                    parsedValue = expressionMethod != ExpressionMethod.In && expressionMethod != ExpressionMethod.NotIn
                        ? Convert.ChangeType(propertyValue, fieldPropertyType)
                        : propertyValue;
                }
            }

            Expression expression;
            if (IsNumeric(parsedValue))
            {
                expression = GetNumericExpression(expressionMethod, fieldPropertyExpression, fieldPropertyType, parsedValue);

            }
            else if (IsDateTime(parsedValue))
            {
                expression = GetNumericExpression(expressionMethod, fieldPropertyExpression, fieldPropertyType, parsedValue);
            }
            else if (parsedValue is string)
            {
                parsedValue = parsedValue.ToString().Replace("'", "");
                expression = GetStringExpression(expressionMethod, fieldPropertyExpression, fieldPropertyType, parsedValue, propertyName, propertyValue);
            }
            else
            {
                expression = Expression
                    .Equal(fieldPropertyExpression, Expression.Constant(parsedValue, fieldPropertyType));
            }

            return Expression.Lambda<Func<T, bool>>(expression, par);
        }

        private static Expression GetStringExpression(
            ExpressionMethod kind,
            Expression fieldPropertyExpression,
            Type fieldPropertyType,
            object data,
            string propertyName,
            object propertyValue)
        {
            switch (kind)
            {
                case ExpressionMethod.Equal:
                    return Expression.Equal(fieldPropertyExpression, Expression.Constant(data, fieldPropertyType));
                case ExpressionMethod.NotEqual:
                    return Expression.NotEqual(fieldPropertyExpression, Expression.Constant(data, fieldPropertyType));
                case ExpressionMethod.Like:
                    return GetStringContainsExpression(fieldPropertyExpression, propertyValue);
                case ExpressionMethod.ILike:
                    return GetILikeExpression(fieldPropertyExpression, propertyValue);
                case ExpressionMethod.ILikeUnaccent:
                    return GetILikeUnaccentExpression(fieldPropertyExpression, propertyValue);
                case ExpressionMethod.In:
                    string[] arr = data.ToString().Split(',');
                    BinaryExpression binaryExpression = null;
                    foreach (string val in arr)
                    {
                        BinaryExpression newBinaryExpression = Expression
                            .Equal(
                                fieldPropertyExpression,
                                Expression.Constant(Convert.ChangeType(val, fieldPropertyType), fieldPropertyType));

                        if (binaryExpression == null)
                        {
                            binaryExpression = newBinaryExpression;
                        }
                        else
                        {
                            binaryExpression = Expression.OrElse(binaryExpression, newBinaryExpression);
                        }
                    }

                    return binaryExpression;
                case ExpressionMethod.NotIn:
                    string[] notarr = data.ToString().Split(',');
                    BinaryExpression notexp = null;
                    foreach (string val in notarr)
                    {
                        BinaryExpression eq = Expression
                            .NotEqual(
                                fieldPropertyExpression,
                                Expression.Constant(Convert.ChangeType(val, fieldPropertyType), fieldPropertyType));

                        if (notexp == null)
                        {
                            notexp = eq;
                        }
                        else
                        {
                            notexp = Expression.AndAlso(notexp, eq);
                        }
                    }
                    return notexp;
                default:
                    return Expression
                        .Equal(
                            fieldPropertyExpression,
                            Expression.Constant(data, fieldPropertyType));
            }
        }

        private static BinaryExpression GetNumericExpression(
            ExpressionMethod kind,
            Expression fieldPropertyExpression,
            Type fieldPropertyType,
            object data)
        {
            switch (kind)
            {
                case ExpressionMethod.Equal:
                    return Expression.Equal(fieldPropertyExpression,
                                          Expression.Constant(data, fieldPropertyType));

                case ExpressionMethod.LessThan:
                    return Expression.LessThan(fieldPropertyExpression,
                                             Expression.Constant(data, fieldPropertyType));

                case ExpressionMethod.GreaterThan:
                    return Expression.GreaterThan(
                        fieldPropertyExpression,
                        Expression.Constant(data, fieldPropertyType));

                case ExpressionMethod.LessThanOrEqual:
                    return Expression.LessThanOrEqual(
                        fieldPropertyExpression,
                        Expression.Constant(data, fieldPropertyType));

                case ExpressionMethod.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(
                        fieldPropertyExpression,
                        Expression.Constant(data, fieldPropertyType));

                case ExpressionMethod.NotEqual:
                    return Expression.NotEqual(fieldPropertyExpression,
                                      Expression.Constant(data, fieldPropertyType));
                default:
                    return Expression.Equal(fieldPropertyExpression,
                                                  Expression.Constant(data, fieldPropertyType));
            }
        }

        private static bool IsNumeric(object value)
        {
            return value is sbyte ||
                   value is byte ||
                   value is short ||
                   value is ushort ||
                   value is int ||
                   value is uint ||
                   value is long ||
                   value is ulong ||
                   value is float ||
                   value is double ||
                   value is decimal;
        }

        private static bool IsDateTime(object value)
        {
            return value is DateTime;
        }

        private static Expression GetStringContainsExpression(Expression fieldExpression, object propertyValue)
        {
            var likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                [typeof(DbFunctions), typeof(string), typeof(string)],
                null);
            string pattern = $"%{propertyValue}%";
            ConstantExpression likeConstant = Expression.Constant(pattern, typeof(string));
            return Expression.Call(method: likeMethod, arguments: [Expression.Property(null, typeof(EF), nameof(EF.Functions)), fieldExpression, likeConstant]);
        }

        private static Expression GetILikeExpression(Expression fieldExpression, object propertyValue)
        {
            var likeMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod(nameof(NpgsqlDbFunctionsExtensions.ILike),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                [typeof(DbFunctions), typeof(string), typeof(string)],
                null);
            string pattern = $"%{propertyValue}%";
            ConstantExpression likeConstant = Expression.Constant(pattern, typeof(string));
            return Expression.Call(method: likeMethod, arguments: [Expression.Property(null, typeof(EF), nameof(EF.Functions)), fieldExpression, likeConstant]);
        }

        private static Expression GetILikeUnaccentExpression(Expression fieldExpression, object propertyValue)
        {
            var likeMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod(nameof(NpgsqlDbFunctionsExtensions.ILike),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                [typeof(DbFunctions), typeof(string), typeof(string)],
                null);

            var unaccentMethod = typeof(NpgsqlFullTextSearchDbFunctionsExtensions).GetMethod(nameof(NpgsqlFullTextSearchDbFunctionsExtensions.Unaccent),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                [typeof(DbFunctions), typeof(string)],
                null);

            string pattern = $"%{propertyValue}%";
            ConstantExpression likeConstant = Expression.Constant(pattern, typeof(string));

            var unaccentExpression
                = Expression.Call(method: unaccentMethod, arguments: [Expression.Property(null, typeof(EF), nameof(EF.Functions)), likeConstant]);

            var unaccentFieldExpression
                = Expression.Call(method: unaccentMethod, arguments: [Expression.Property(null, typeof(EF), nameof(EF.Functions)), fieldExpression]);

            return Expression.Call(method: likeMethod, arguments: [Expression.Property(null, typeof(EF), nameof(EF.Functions)), unaccentFieldExpression, unaccentExpression]);
        }

        public static T InitModel<T>()
        {
            Type classType = typeof(T);
            ConstructorInfo classConstructor = classType.GetConstructor(new Type[] { });
            T context = (T)classConstructor.Invoke(new object[] { });
            return context;
        }

        public static string[] GetKeyMembers<TDbContext>(TDbContext context, Type entityType)
            where TDbContext : DbContext

        {
            return context.Model.FindEntityType(entityType)
                .FindPrimaryKey().Properties.Select(x => x.Name)
                .ToArray();
        }

        public static string[] FilterSelectedFields<TView, TEntity>()
        {
            var viewProperties = typeof(TView).GetProperties();
            var modelProperties = typeof(TEntity).GetProperties();
            return viewProperties.Where(p => modelProperties.Any(m => m.Name == p.Name)).Select(p => p.Name).ToArray();
        }

        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(
                Expression oldValue,
                Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }
    }
    public class JTokenConverter : JsonConverter<JToken>
    {
        public override bool CanConvert(Type typeToConvert) => typeof(JToken).IsAssignableFrom(typeToConvert);
        public override JToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Use JsonDocument to parse the JSON and create a JToken from it
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            return JToken.Parse(document.RootElement.GetRawText());
        }

        public override void Write(Utf8JsonWriter writer, JToken value, JsonSerializerOptions options)
        {
            // Write the raw JSON from the JToken to the writer
            writer.WriteRawValue(value.ToString());
        }
    }
}
