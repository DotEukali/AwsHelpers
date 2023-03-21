using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace DotEukali.AwsHelpers.DynamoDb.Models
{
    internal class UpdateModel
    {
        public string UpdateExpression { get; set; } = "SET";
        public Dictionary<string, AttributeValue> ExpressionAttributeValues { get; set; } = new Dictionary<string, AttributeValue>();
    }
}
