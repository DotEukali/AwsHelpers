using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace DotEukali.AwsHelpers.DynamoDb.Models
{
    internal class UpdateModel
    {
        public IDictionary<string, AttributeValue> Adds = new Dictionary<string, AttributeValue>();
        public IDictionary<string, AttributeValue> Sets = new Dictionary<string, AttributeValue>();
        public IDictionary<string, AttributeValue> Removes = new Dictionary<string, AttributeValue>();
    }
}
