namespace Overwurd.Domain.Jwt.Entities

open System

open Overwurd.Domain.Jwt

module JwtRefreshTokenId =

    let unwrap (tokenId: JwtRefreshTokenId): int =
        match tokenId with
        | JwtRefreshTokenId value -> value

module JwtAccessTokenId =

    let tryParse (tokenId: string): JwtAccessTokenId option =
        match Guid.TryParse tokenId with
        | true, value -> Some (JwtAccessTokenId value)
        | false, _ -> None

    let unwrap (tokenId: JwtAccessTokenId): Guid =
        match tokenId with
        | JwtAccessTokenId value -> value