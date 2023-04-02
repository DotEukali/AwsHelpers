using System.Collections;
using Amazon.DynamoDBv2.Model;
using DotEukali.AwsHelpers.DynamoDb.Models;
using System.Linq;

namespace DotEukali.AwsHelpers.DynamoDb.Extensions
{
    public static class TransactionModelExtensions
    {
        public static TransactWriteItemsRequest ToTransactWriteItemsRequest(this TransactionModel transactionModel) =>
            new TransactWriteItemsRequest()
            {
                TransactItems = transactionModel.GetAllActions().Select(x => x.ToTransactWriteItem()).ToList()
            };

        public static TransactionModel AddPuts(this TransactionModel transactionModel, params object[] putObjects)
        {
            foreach (var obj in putObjects)
            {
                if (obj is IEnumerable putList)
                {
                    foreach (var putItem in putList)
                    {
                        transactionModel.Put.Add(putItem.ToPut());
                    }
                }
                else
                {
                    transactionModel.Put.Add(obj.ToPut());
                }
            }

            return transactionModel;
        }

        public static TransactionModel AddUpdate<T>(this TransactionModel transactionModel, T oldValue, T newValue)
            where T : class

        {
            Update? update = oldValue.ToUpdate<T>(newValue);
            
            if (update != null)
                transactionModel.Update.Add(update);

            return transactionModel;
        }

        public static TransactionModel AddDeletes(this TransactionModel transactionModel, params object[] deleteObjects)
        {
            foreach (var obj in deleteObjects)
            {
                if (obj is IEnumerable deleteList)
                {
                    foreach (var deleteItem in deleteList)
                    {
                        transactionModel.Delete.Add(deleteItem.ToDelete());
                    }
                }
                else
                {
                    transactionModel.Delete.Add(obj.ToDelete());
                }
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
