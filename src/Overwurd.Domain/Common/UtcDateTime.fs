namespace Overwurd.Domain.Common

open System

type UtcDateTime =
    private UtcDateTime of DateTime

module UtcDateTime =

    let now (): UtcDateTime =
        DateTime.UtcNow
        |> UtcDateTime
    
    let create (date: DateTime): UtcDateTime =

        let ensureUtc (dateTime: DateTime): DateTime =
            match dateTime.Kind with
            | DateTimeKind.Utc -> dateTime
            | _ -> raise (InvalidOperationException "Only UTC dates are allowed.")

        date |> ensureUtc |> UtcDateTime

    let unwrap (date: UtcDateTime): DateTime =
        match date with
        | UtcDateTime value -> value