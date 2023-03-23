using Amazon.DynamoDBv2.Model;
using DotEukali.AwsHelpers.DynamoDb.Models;

namespace DotEukali.AwsHelpers.DynamoDb.Extensions
{
    public static class TransactionModelExtensions
    {
        public static TransactionModel AddPuts(this TransactionModel transactionModel, params object[] putObjects)
        {
            foreach (var obj in putObjects)
            {
                transactionModel.Put.Add(obj.ToPut());
            }

            return transactionModel;
        }

        public static TransactionModel AddUpdate<T>(this TransactionModel transactionModel, T oldValue, T newValue)
            where T : class

        {
            transactionModel.Update.Add(oldValue.ToUpdate<T>(newValue));

            return transactionModel;
        }

        public static TransactionModel AddDeletes(this TransactionModel transactionModel, params object[] deleteObjects)
        {
            foreach (var obj in deleteObjects)
            {
                transactionModel.Delete.Add(obj.ToDelete());
            }

            return transactionModel;
        }

        public static TransactionModel AddConditionChecks(this TransactionModel transactionModel, params ConditionCheck[] conditionChecks)
        {
            foreach (var conditionCheck in conditionChecks)
            {
                transactionModel.ConditionCheck.Add(conditionCheck);
            }

            return transactionModel;
        }
    }
}
