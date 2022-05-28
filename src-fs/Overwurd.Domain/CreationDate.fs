namespace Overwurd.Domain

open System
open Overwurd.Domain.Common.Utils

type CreationDate =
    private CreationDate of DateTime

module CreationDate =

    let create (date: DateTime): CreationDate =
        date |> ensureUtc |> CreationDate

    let unwrap (date: CreationDate): DateTime =
        match date with
        | CreationDate d -> d