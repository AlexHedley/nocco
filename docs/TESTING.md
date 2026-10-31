# Testing

Tests live in `src/Nocco.Tests` (xUnit).

## Unit tests

`UnitTests.cs` covers comment/code parsing, Markdown mapping, language lookup and output path calculation.

## Integration tests

`IntegrationTests.cs` runs `Nocco.Generate` against temporary directories and checks the generated HTML and assets.

```bash
cd src
dotnet test
```

## Mutation tests

Uses [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/).

```bash
dotnet tool install --global dotnet-stryker
cd src/Nocco.Tests
dotnet stryker
```

Configuration is in `src/Nocco.Tests/stryker-config.json`; the HTML report is written to `StrykerOutput/`.
