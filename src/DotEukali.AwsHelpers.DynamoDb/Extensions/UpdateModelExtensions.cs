using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DynamoDBv2.Model;
using DotEukali.AwsHelpers.DynamoDb.Models;

namespace DotEukali.AwsHelpers.DynamoDb.Extensions
{
    internal static class UpdateModelExtensions
    {
        internal static string GetUpdateExpression(this UpdateModel updateModel)
        {
            StringBuilder sb = new StringBuilder();

            if (updateModel.Adds.Any())
            {
                sb.Append("ADD");

                for (var i = 0; i < updateModel.Adds.Count; i++)
                {
                    sb.Append($"{(i > 0 ? "," : "")} #addName{i} = :addValue{i}");
                }

                sb.Append(" ");
            }

            if (updateModel.Sets.Any())
            {
                sb.Append("SET");

                for (var i = 0; i < updateModel.Sets.Count; i++)
                {
                    sb.Append($"{(i > 0 ? "," : "")} #setName{i} = :setValue{i}");
                }

                sb.Append(" ");
            }

            if (updateModel.Removes.Any())
            {
                sb.Append("REMOVE");

                for (var i = 0; i < updateModel.Removes.Count; i++)
                {
                    sb.Append($"{(i > 0 ? "," : "")} #removeName{i} = :removeValue{i}");
                }
            }

            return sb.ToString();
        }

        internal static Dictionary<string, string> GetExpressionAttributeNames(this UpdateModel updateModel)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (updateModel.Adds.Any())
            {
                int i = 0;

                foreach (var item in updateModel.Adds)
                {
                    result.Add($"#addName{i}", item.Key);
                    i++;
                }
            }

            if (updateModel.Sets.Any())
            {
                int i = 0;

                foreach (var item in updateModel.Sets)
                {
                    result.Add($"#setName{i}", item.Key);
                    i++;
                }
            }

            if (updateModel.Removes.Any())
            {
                int i = 0;

                foreach (var item in updateModel.Removes)
                {
                    result.Add($"#removeName{i}", item.Key);
                    i++;
                }
            }

            return result;
        }

        internal static Dictionary<string, AttributeValue> GetExpressionAttributeValues(this UpdateModel updateModel)
        {
            Dictionary<string, AttributeValue> result = new Dictionary<string, AttributeValue>();

            if (updateModel.Adds.Any())
            {
                int i = 0;

                foreach (var item in updateModel.Adds)
                {
                    result.Add($":addValue{i}", item.Value);
                    i++;
                }
            }

            if (updateModel.Sets.Any())
            {
                int i = 0;

                foreach (var item in updateModel.Sets)
                {
                    result.Add($":setValue{i}", item.Value);
                    i++;
                }
            }
            
            return result;
        }

        internal static bool HasChanges(this UpdateModel updateModel) => updateModel.Sets.Any() || updateModel.Adds.Any() || updateModel.Removes.Any();
    }
}
