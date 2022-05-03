namespace Overwurd.Infrastructure

open System.Data

module Common =

    type Session = {
        Connection: IDbConnection
        Transaction: IDbTransaction
    }

