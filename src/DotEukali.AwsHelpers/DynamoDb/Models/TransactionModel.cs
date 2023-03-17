using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace DotEukali.AwsHelpers.DynamoDb.Models
{
    public sealed class TransactionModel
    {
        public IList<Put> Put { get; set; } = new List<Put>();
        public IList<Update> Update { get; set; } = new List<Update>();
        public IList<Delete> Delete { get; } = new List<Delete>();
        public IList<ConditionCheck> ConditionCheck { get; set; } = new List<ConditionCheck>();

        public IList<object> GetAllActions()
        {
            List<object> actions = new List<object>();

            actions.AddRange(ConditionCheck);
            actions.AddRange(Delete);
            actions.AddRange(Put);
            actions.AddRange(Update);

            return actions;
        }
    }
}
