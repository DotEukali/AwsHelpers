using Amazon.DynamoDBv2.Model;
using DotEukali.AwsHelpers.DynamoDb.Extensions;
using DotEukali.AwsHelpers.DynamoDb.Models;
using DotEukali.AwsHelpers.DynamoDb.Tests.Models;
using FluentAssertions;

namespace DotEukali.AwsHelpers.DynamoDb.Tests.Extensions
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            TestDynamoDbTableClass testClass = new TestDynamoDbTableClass();

            string? tablename = testClass.GetDynamoDbTableName();

            tablename.Should().Be("Test_Table_Name");
        }

        [Fact]
        public void Test2()
        {
            TestDynamoDbTableClass testClass = new TestDynamoDbTableClass();

            IEnumerable<TestDynamoDbTableClass> classes = new List<TestDynamoDbTableClass>()
            {
                testClass, testClass, testClass, testClass, testClass
            };

            TransactionModel transactionModel = new TransactionModel().AddPuts(classes);

            transactionModel.Put.Count.Should().Be(5);
        }

        [Fact]
        public void TestUpdate()
        {
            Guid hash = Guid.NewGuid();
            Guid range = Guid.NewGuid();

            TestDynamoDbTableClass existing = new TestDynamoDbTableClass()
            {
                HashKey = hash,
                RangeKey = range,
                Property1 = "hi",
                Property2 = "bye"
            }; 
            TestDynamoDbTableClass changes = new TestDynamoDbTableClass()
            {
                HashKey = hash,
                RangeKey = range,
                Property1 = "yo",
                Property2 = "jo"
            };

            Update? update = existing.ToUpdate(changes);
            
            update!.Key.Count.Should().Be(1);
            update!.Key.Single().Key.Should().Be(nameof(TestDynamoDbTableClass.HashKey));
            update.Key.Single().Value.S.Should().Be(hash.ToString());

            update.ExpressionAttributeNames.Count.Should().Be(2);
            update.ExpressionAttributeValues.Count.Should().Be(2);

            update.UpdateExpression.Should().Be("SET #setName0 = :setValue0, #setName1 = :setValue1 ");

        }
    }
}
