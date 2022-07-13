module Overwurd.DataAccess.Database.StatusStorage

open System

open Overwurd.Domain.Common.Persistence
open Overwurd.Domain.Features.Status.Workflows
open Overwurd.DataAccess.Database.Dapper

let getDatabaseVersionAsync: GetDatabaseVersionAsync =
    fun (session: DbSession) ->
        task {
            let sql = """
select version
  from public.changelog
 order by Id desc
 limit 1
"""

            let command = makeSqlCommand sql session
            let! versionString = session.Connection |> querySingleAsync<string> command
            return Version.Parse versionString
        }