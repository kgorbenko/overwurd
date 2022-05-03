namespace Overwurd.Infrastructure.Tests

module Database =

    open Dapper
    open Overwurd.Infrastructure.Common

    let clear (session: Session) =
        task {
            let sql = """
TRUNCATE "overwurd"."Courses" RESTART IDENTITY
"""

            let command = Utils.makeCommand sql session.Transaction
            let _ = command |> session.Connection.ExecuteAsync

            ()
        }

