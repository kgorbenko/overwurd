module internal Overwurd.Domain.Common.Utils

open System

let ensureUtc (dateTime: DateTime): DateTime =
    match dateTime.Kind with
    | DateTimeKind.Utc -> dateTime
    | _ -> raise (InvalidOperationException "Only UTC dates are allowed.")

let tryParseInt (value: string): int Option =
    match Int32.TryParse value with
    | true, value -> Some value
    | false, _ -> None

let parseInt (value: string): int =
    match tryParseInt value with
    | Some parsed -> parsed
    | None -> raise (InvalidOperationException $"Cannot parse '{value}' to integer")

let toUpperCase (value: string): string =
    value.ToUpperInvariant()