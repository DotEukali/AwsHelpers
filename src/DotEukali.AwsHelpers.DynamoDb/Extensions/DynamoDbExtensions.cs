using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using DotEukali.AwsHelpers.DynamoDb.Models;

namespace DotEukali.AwsHelpers.DynamoDb.Extensions
{
    public static class DynamoDbExtensions
    {
        public static TransactWriteItemsRequest ToTransactWriteItemsRequest(this TransactionModel transactionModel) =>
            new TransactWriteItemsRequest()
            {
                TransactItems = transactionModel.GetAllActions().Select(x => x.ToTransactWriteItem()).ToList()
            };

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

        public static Update ToUpdate<T>(this T existing, T newVersion)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            if (newVersion == null) throw new ArgumentNullException(nameof(newVersion));

            Dictionary<string, AttributeValue> existingKeys = BuildPrimaryKeyAttributeDictionary(existing);
            Dictionary<string, AttributeValue> newKeys = BuildPrimaryKeyAttributeDictionary(newVersion);

            if (existingKeys.Any(key => key.Value != newKeys[key.Key]))
            {
                throw new Exception();
            }

            if (newKeys.Any(key => key.Value != existingKeys[key.Key]))
            {
                throw new Exception();
            }

            UpdateModel model = GetUpdateModel(existing, newVersion);

            return new Update
            {
                TableName = existing.GetDynamoDbTableName() ?? throw new ArgumentNullException("TableName"),
                Key = existingKeys,
                UpdateExpression = model.UpdateExpression,
                ExpressionAttributeValues = model.ExpressionAttributeValues
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
                         .Where(prop => prop.GetCustomAttributes<DynamoDBPropertyAttribute>().Any()))
            {
                var originalValue = original.GetPropertyValue(property.Name);
                var newValue = newVersion.GetPropertyValue(property.Name);

                if (originalValue != newValue)
                {
                    varId++;
                    string key = $":val{varId}";

                    model.UpdateExpression += $" {property.Name} = {key},";
                    model.ExpressionAttributeValues.Add(key, BuildAttributeValue(newVersion, property));
                }
            }

            model.UpdateExpression.Trim(',');

            return model;
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
