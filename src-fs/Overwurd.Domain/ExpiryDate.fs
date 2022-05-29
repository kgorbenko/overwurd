namespace Overwurd.Domain

open System

type ExpiryDate =
    private ExpiryDate of UtcDateTime

module ExpiryDate =

    let create (date: DateTime): ExpiryDate =
        date |> UtcDateTime.create |> ExpiryDate

    let unwrap (date: ExpiryDate): DateTime =
        match date with
        | ExpiryDate utcValue -> UtcDateTime.unwrap utcValue