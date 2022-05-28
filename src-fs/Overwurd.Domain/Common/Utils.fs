module internal Overwurd.Domain.Common.Utils

open System

let ensureUtc (dateTime: DateTime): DateTime =
    match dateTime.Kind with
    | DateTimeKind.Utc -> dateTime
    | _ -> raise (InvalidOperationException "Only UTC dates are allowed.")