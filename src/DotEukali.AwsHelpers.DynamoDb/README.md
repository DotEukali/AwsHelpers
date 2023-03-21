# Aws Helpers
A growing library of AWS related classes and extensions.

## DynamoDb
Some extension methods to more fluently prepare atomic transactions. Assumes dynamo table classes are built using attributes for the table name and properties, as it uses reflection to build the `TransactWriteItem`s from this.
