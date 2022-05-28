namespace Overwurd.Domain

open System
open Overwurd.Domain.Common.Utils

type RefreshDate =
    private RefreshDate of DateTime

module RefreshDate =

    let create (date: DateTime): RefreshDate =
        date |> ensureUtc |> RefreshDate

    let unwrap (date: RefreshDate): DateTime =
        match date with
        | RefreshDate d -> d

