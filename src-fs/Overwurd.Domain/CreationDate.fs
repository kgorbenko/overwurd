namespace Overwurd.Domain

open System

type CreationDate =
    private CreationDate of UtcDateTime

module CreationDate =

    let create (date: DateTime): CreationDate =
        date |> UtcDateTime.create |> CreationDate

    let unwrap (date: CreationDate): DateTime =
        match date with
        | CreationDate utcValue -> UtcDateTime.unwrap utcValue