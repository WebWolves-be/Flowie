// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Suppressing warnings about internal types and Shared namespace
[assembly: SuppressMessage("Design", "CA1515:Type is internal", Scope = "namespace", Target = "~N:Flowie.Api.Infrastructure.Behaviors")]
[assembly: SuppressMessage("Design", "CA1515:Type is internal", Scope = "namespace", Target = "~N:Flowie.Api.Infrastructure.Middleware")]
[assembly: SuppressMessage("Design", "CA1515:Type is internal", Scope = "namespace", Target = "~N:Flowie.Api.Migrations")]
[assembly: SuppressMessage("Design", "CA1716:Identifiers should not match keywords", Scope = "namespace", Target = "~N:Flowie.Api.Shared.Infrastructure.Behaviors")]
[assembly: SuppressMessage("Design", "CA1716:Identifiers should not match keywords", Scope = "namespace", Target = "~N:Flowie.Api.Shared.Infrastructure.Middleware")]
[assembly: SuppressMessage("Design", "CA1716:Identifiers should not match keywords", Scope = "namespace", Target = "~N:Flowie.Api.Shared.Domain")]
[assembly: SuppressMessage("Design", "CA1716:Identifiers should not match keywords", Scope = "namespace", Target = "~N:Flowie.Api.Shared.Infrastructure.Database")]

// Suppress warnings about ConfigureAwait and null validation in migrations
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Scope = "module")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Scope = "namespaceanddescendants", Target = "~N:Flowie.Api.Migrations")]

// Suppress warnings about LoggerMessage delegates for improved performance
[assembly: SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Scope = "module")]