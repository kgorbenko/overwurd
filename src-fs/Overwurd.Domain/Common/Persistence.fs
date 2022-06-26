module Overwurd.Domain.Common.Persistence

open System.Data
open System.Threading
open System.Threading.Tasks

type DbSession =
    { Connection: IDbConnection
      Transaction: IDbTransaction
      CancellationToken: CancellationToken }

let inTransactionAsync (queryAsync: DbSession -> 'a Task)
                       (cancellationToken: CancellationToken)
                       (connection: IDbConnection): 'a Task =
    task {
        use transaction = connection.BeginTransaction()
        let session =
            { Connection = connection
              Transaction = transaction
              CancellationToken = cancellationToken }

        let! result = queryAsync session
        transaction.Commit()

        return result
    }