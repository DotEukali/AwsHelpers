using Amazon.DynamoDBv2.DataModel;

namespace DotEukali.AwsHelpers.DynamoDb.Tests.Models
{

    [DynamoDBTable("Test_Table_Name")]
    public class TestDynamoDbTableClass
    {
        [DynamoDBHashKey]
        [DynamoDBGlobalSecondaryIndexRangeKey]
        public Guid HashKey { get; set; }

        //[DynamoDBRangeKey]
        [DynamoDBGlobalSecondaryIndexHashKey]
        public Guid RangeKey { get; set; }

        [DynamoDBProperty]
        public string Property1 { get; set; } = default!;

        [DynamoDBProperty]
        public string Property2 { get; set; } = default!;
        
    }
}
