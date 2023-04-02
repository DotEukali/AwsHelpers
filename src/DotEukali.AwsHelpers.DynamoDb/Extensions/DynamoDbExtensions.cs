using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon.Auth.AccessControlPolicy;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using DotEukali.AwsHelpers.DynamoDb.Models;

namespace DotEukali.AwsHelpers.DynamoDb.Extensions
{
    public static class DynamoDbExtensions
    {
        public static string? GetDynamoDbTableName(this object obj) =>
            (Attribute.GetCustomAttributes(obj.GetType())
                .FirstOrDefault(x => x is DynamoDBTableAttribute) as DynamoDBTableAttribute)?.TableName;

        public static TransactWriteItem ToTransactWriteItem(this object item) =>
            item switch
            {
                ConditionCheck conditionCheck => new TransactWriteItem() { ConditionCheck = conditionCheck },
                Delete delete => new TransactWriteItem() { Delete = delete },
                Put put => new TransactWriteItem() { Put = put },
                Update update => new TransactWriteItem() { Update = update },
                _ => throw new Exception($"Unknown TransactionWriteItem type: {item.GetType()}")
            };

        public static Put ToPut(this object obj) =>
            new Put()
            {
                TableName = obj.GetDynamoDbTableName() ?? throw new ArgumentNullException("TableName"),
                Item = BuildPropertiesAttributeDictionary(obj)
            };

        public static Update? ToUpdate<T>(this T existing, T newVersion)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            if (newVersion == null) throw new ArgumentNullException(nameof(newVersion));

            UpdateModel model = GetUpdateModel(existing, newVersion);

            if (!model.HasChanges())
            {
                return null;
            }

            ValidateUpdateKeys(existing, newVersion);

            return new Update
            {
                TableName = existing.GetDynamoDbTableName() ?? throw new ArgumentNullException("TableName"),
                Key = BuildPrimaryKeyAttributeDictionary(existing),
                UpdateExpression = model.GetUpdateExpression(),
                ExpressionAttributeNames = model.GetExpressionAttributeNames(),
                ExpressionAttributeValues = model.GetExpressionAttributeValues()
            };
        }

        public static Delete ToDelete(this object obj) =>
            new Delete()
            {
                TableName = obj.GetDynamoDbTableName() ?? throw new ArgumentNullException("TableName"),
                Key = BuildPrimaryKeyAttributeDictionary(obj)
            };

        private static UpdateModel GetUpdateModel<T>(T original, T newVersion)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (newVersion == null) throw new ArgumentNullException(nameof(newVersion));

            UpdateModel model = new UpdateModel();
            int varId = 0;
            
            foreach (var property in typeof(T).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                         .Where(prop => prop.GetCustomAttributes<DynamoDBPropertyAttribute>().Any() 
                                                && !prop.GetCustomAttributes<DynamoDBHashKeyAttribute>().Any()
                                                && !prop.GetCustomAttributes<DynamoDBRangeKeyAttribute>().Any()))
            {
                object? originalValue = original.GetPropertyValue(property.Name);
                object? newValue = newVersion.GetPropertyValue(property.Name);

                if (originalValue == null && newValue != null)
                {
                    model.Adds.Add(property.Name, BuildAttributeValue(newVersion, property));
                }
                else if (originalValue != null && newValue == null)
                {
                    model.Removes.Add(property.Name, BuildAttributeValue(newVersion, property));
                }
                else if (originalValue != newValue)
                {
                    model.Sets.Add(property.Name, BuildAttributeValue(newVersion, property));
                }
            }

            return model;
        }

        private static void ValidateUpdateKeys<T>(this T existing, T newVersion)
        {
            Dictionary<string, object?> existingKeys = existing!.GetType()
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .Where(prop => prop.GetCustomAttributes<DynamoDBHashKeyAttribute>().Any()
                               || prop.GetCustomAttributes<DynamoDBRangeKeyAttribute>().Any())
                .ToDictionary(prop => prop.Name, prop => GetPropertyValue(existing, prop.Name));

            Dictionary<string, object?> newKeys = newVersion!.GetType()
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .Where(prop => prop.GetCustomAttributes<DynamoDBHashKeyAttribute>().Any()
                               || prop.GetCustomAttributes<DynamoDBRangeKeyAttribute>().Any())
                .ToDictionary(prop => prop.Name, prop => GetPropertyValue(existing, prop.Name));

            //foreach (var key in existingKeys)
            //{
            //    if(!(newKeys.TryGetValue(key.Key, out object? nk) && key.Value == nk))
            //}

            if (existingKeys.Values.Any(x => x == null)
                || newKeys.Values.Any(x => x == null)
                || !existingKeys.Any(key => newKeys.TryGetValue(key.Key, out object? newKeyValue) && key.Value!.Equals(newKeyValue))
                || !newKeys.Any(key => existingKeys.TryGetValue(key.Key, out object? existingKeyValue) && key.Value!.Equals(existingKeyValue)))
            {
                throw new Exception("Key values do not match.");
            }
        }

        private static object? GetPropertyValue(this object source, string propertyName) =>
            source.GetType().GetProperty(propertyName)?.GetValue(source, null);

        private static Dictionary<string, AttributeValue> BuildPrimaryKeyAttributeDictionary(this object source) =>
            source.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .Where(prop => prop.GetCustomAttributes<DynamoDBHashKeyAttribute>().Any()
                               || prop.GetCustomAttributes<DynamoDBRangeKeyAttribute>().Any())
                .ToDictionary(prop => prop.Name, prop => BuildAttributeValue(source, prop));

        private static Dictionary<string, AttributeValue> BuildPropertiesAttributeDictionary(this object source) =>
            source.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .Where(prop => prop.GetCustomAttributes<DynamoDBPropertyAttribute>().Any())
                .ToDictionary(prop => prop.Name, prop => BuildAttributeValue(source, prop));

        private static AttributeValue BuildAttributeValue(object source, PropertyInfo propertyInfo)
        {
            object? value = source.GetPropertyValue(propertyInfo.Name);

            if (value == null)
            {
                return new AttributeValue()
                {
                    NULL = true
                };
            }

            if (propertyInfo.PropertyType == typeof(bool))
            {
                return new AttributeValue()
                {
                    BOOL = (bool)value
                };
            }

            if (NumberTypes.Any(x => propertyInfo.PropertyType == x))
            {
                return new AttributeValue()
                {
                    N = value.ToString()
                };
            }

            if (propertyInfo.PropertyType == typeof(Enum))
            {
                return new AttributeValue()
                {
                    N = ((int)value).ToString()
                };
            }

            return new AttributeValue()
            {
                S = value.ToString()
            };
        }

        private static IEnumerable<Type> NumberTypes => new[]
        {
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong)
        };
    }
}
