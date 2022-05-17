namespace Overwurd.Domain.Common.Consistency

open System

type CreationDate =
    private CreationDate of DateTime

module CreationDate =

    let create (date: DateTime): CreationDate =
        match date.Kind with
        | DateTimeKind.Utc -> CreationDate date
        | _ -> raise (InvalidOperationException "Only UTC dates are allowed for CreatedAt fields.")

    let unwrap (date: CreationDate): DateTime =
        match date with
        | CreationDate date -> date
