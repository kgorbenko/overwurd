namespace Overwurd.Domain.Features.Status

open System

type ApplicationStatus =
    { ApplicationVersion: Version
      DatabaseVersion: Version }