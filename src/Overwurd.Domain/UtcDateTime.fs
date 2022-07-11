namespace Overwurd.Domain

open System
open Overwurd.Domain.Common.Utils

type UtcDateTime =
    private UtcDateTime of DateTime

module UtcDateTime =

    let now (): UtcDateTime =
        DateTime.UtcNow
        |> UtcDateTime

    let create (date: DateTime): UtcDateTime =
        date |> ensureUtc |> UtcDateTime

    let unwrap (date: UtcDateTime): DateTime =
        match date with
        | UtcDateTime value -> value