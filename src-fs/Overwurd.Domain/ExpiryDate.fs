namespace Overwurd.Domain

open System

type ExpiryDate =
    private ExpiryDate of UtcDateTime

module ExpiryDate =

    let create (date: UtcDateTime): ExpiryDate =
        date |> ExpiryDate

    let unwrap (date: ExpiryDate): DateTime =
        match date with
        | ExpiryDate utcValue -> UtcDateTime.unwrap utcValue