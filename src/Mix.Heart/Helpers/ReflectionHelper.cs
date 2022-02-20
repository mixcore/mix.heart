using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Extensions;
using Mix.Heart.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mix.Heart.Helpers
{
    public class ReflectionHelper
    {
        static JsonSerializer serializer = new JsonSerializer()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        public static Expression<Func<TEntity, bool>> BuildExpressionByKeys<TEntity, TDbContext>(
            TEntity model, TDbContext context)
            where TDbContext : DbContext
        {

            Expression<Func<TEntity, bool>> predicate = null;

            foreach (var item in context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties)
            {
                var pre = GetExpression<TEntity>(
                        item.Name,
                        GetPropertyValue<TEntity>(model, item.Name),
                        ExpressionMethod.Eq);
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

        public static object GetMembers<TEntity>(TEntity model, string[] selectMembers)
           where TEntity : class
        {
            serializer.Converters.Add(new StringEnumConverter());
            var result = JObject.FromObject(model, serializer).Properties()
                            .Where(p => selectMembers.Any(m => m.ToLower() == p.Name.ToLower()));
            return new JObject() { result };
        }

        public static Type GetPropertyType(Type type, string name)
        {
            Type fieldPropertyType;
            FieldInfo fieldInfo = type.GetField(name);

            if (fieldInfo == null)
            {
                PropertyInfo propertyInfo = type.GetProperty(name);

                // if (propertyInfo == null)
                //{
                //    throw new Exception();
                //}

                fieldPropertyType = propertyInfo?.PropertyType;
            }
            else
            {
                fieldPropertyType = fieldInfo.FieldType;
            }
            return fieldPropertyType;
        }

        public static object GetPropertyValue<T>(T data, string fieldName)
        {
            var prop = data.GetType().GetProperty(fieldName.ToTitleCase());
            if (prop != null)
            {
                // System.ComponentModel.TypeConverter conv =
                // System.ComponentModel.TypeDescriptor.GetConverter(prop.PropertyType);
                // var obj = conv.ConvertFrom();
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

        public static void SetPropertyValue<T>(T data, EntityPropertyModel propety) where T
            : class
        {
            var prop = data.GetType().GetProperty(propety.PropertyName.ToTitleCase());
            if (prop != null)
            {
                prop.SetValue(data, propety.PropertyValue);
            }
        }

        public static Expression<Func<T, bool>>
        GetExpression<T>(string propertyName, object propertyValue,
                         ExpressionMethod kind, string name = "model")
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
                PropertyInfo propertyInfo =
                    type.GetProperty(propertyName.ToTitleCase());

                if (propertyInfo == null)
                {
                    throw new MixException(MixErrorStatus.Badrequest, "Invalid Property Expression");
                }

                fieldPropertyType = propertyInfo.PropertyType;
                fieldPropertyExpression = Expression.Property(par, propertyInfo);
            }
            object data2;
            if (fieldPropertyType.IsGenericType &&
                fieldPropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                System.ComponentModel.TypeConverter conv =
                    System.ComponentModel.TypeDescriptor.GetConverter(
                        fieldPropertyType);
                data2 = conv.ConvertFrom(propertyValue);
                // data2 = Convert.ChangeType(constant.LiteralText,
                // Nullable.GetUnderlyingType());
            }
            else
            {
                data2 = Convert.ChangeType(propertyValue, fieldPropertyType);
            }

            if (fieldPropertyType == typeof(string))
            {
                data2 = data2.ToString().Replace("'", "");
            }
            BinaryExpression eq = null;
            switch (kind)
            {
                case ExpressionMethod.Eq:
                    eq = Expression.Equal(fieldPropertyExpression,
                                          Expression.Constant(data2, fieldPropertyType));
                    break;

                case ExpressionMethod.Lt:
                    eq = Expression.LessThan(fieldPropertyExpression,
                                             Expression.Constant(data2, fieldPropertyType));
                    break;

                case ExpressionMethod.Gt:
                    eq = Expression.GreaterThan(
                        fieldPropertyExpression,
                        Expression.Constant(data2, fieldPropertyType));
                    break;

                case ExpressionMethod.Lte:
                    eq = Expression.LessThanOrEqual(
                        fieldPropertyExpression,
                        Expression.Constant(data2, fieldPropertyType));
                    break;

                case ExpressionMethod.Gte:
                    eq = Expression.GreaterThanOrEqual(
                        fieldPropertyExpression,
                        Expression.Constant(data2, fieldPropertyType));
                    break;

                case ExpressionMethod.And:
                    eq = Expression.And(fieldPropertyExpression,
                                        Expression.Constant(data2, fieldPropertyType));
                    break;

                case ExpressionMethod.Or:
                    eq = Expression.Or(fieldPropertyExpression,
                                       Expression.Constant(data2, fieldPropertyType));
                    break;

                default:
                    break;
            }

            return Expression.Lambda<Func<T, bool>>(eq, par);
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

            public ReplaceExpressionVisitor(Expression oldValue,
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
}
