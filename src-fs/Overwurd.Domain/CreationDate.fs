namespace Overwurd.Domain

open System

type CreationDate =
    private CreationDate of UtcDateTime

module CreationDate =

    let create (date: UtcDateTime): CreationDate =
        date |> CreationDate

    let unwrap (date: CreationDate): DateTime =
        match date with
        | CreationDate utcValue -> UtcDateTime.unwrap utcValue