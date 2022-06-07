module Overwurd.Infrastructure.Database.StatusStore

open System
open System.Threading
open System.Threading.Tasks

open Overwurd.Infrastructure.Database.Dapper

let getDatabaseVersionAsync (cancellationToken: CancellationToken)
                            (session: Session)
                            : Version Task =
    task {
        let sql = """
select version
  from public.changelog
 order by Id desc
 limit 1
"""

        let command = makeSqlCommand sql session.Transaction cancellationToken
        let! versionString = session.Connection |> querySingleAsync<string> command
        return Version.Parse versionString
    }