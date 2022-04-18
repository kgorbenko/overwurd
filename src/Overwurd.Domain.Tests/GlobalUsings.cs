global using System;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Linq;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Diagnostics.CodeAnalysis;

global using Overwurd.Domain;
global using Overwurd.Domain.Entities;
global using Overwurd.Domain.Entities.Validation;
global using Overwurd.Domain.Services;
global using Overwurd.Domain.Services.Authentication;

global using Overwurd.Domain.Tests.Comparers;
global using Overwurd.Domain.Tests.Helpers;

global using FluentValidation;
global using Nito.Comparers;
global using NUnit.Framework;
global using NSubstitute;
