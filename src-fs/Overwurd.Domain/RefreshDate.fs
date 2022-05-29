namespace Overwurd.Domain

open System

type RefreshDate =
    private RefreshDate of UtcDateTime

module RefreshDate =

    let create (date: DateTime): RefreshDate =
        date |> UtcDateTime.create |> RefreshDate

    let unwrap (date: RefreshDate): DateTime =
        match date with
        | RefreshDate utcValue -> UtcDateTime.unwrap utcValue

