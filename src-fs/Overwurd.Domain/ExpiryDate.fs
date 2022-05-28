namespace Overwurd.Domain

open System
open Overwurd.Domain.Common.Utils

type ExpiryDate =
    private ExpiryDate of DateTime

module ExpiryDate =

    let create (date: DateTime): ExpiryDate =
        date |> ensureUtc |> ExpiryDate

    let unwrap (date: ExpiryDate): DateTime =
        match date with
        | ExpiryDate d -> d