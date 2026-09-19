# ZWebAPI
ZWebAPI is a library that brings additional functionalities for web API projects.

## Supported frameworks

ZWebAPI ships assets for .NET 8, .NET 9 and .NET 10.

| Target framework | Notes |
| --- | --- |
| `net10.0` | LTS, supported until November 2028 |
| `net9.0` | STS, out of support since May 2026 |
| `net8.0` | LTS, out of support after November 2026 |

ZWebAPI depends on [ZDatabase](https://github.com/RicardoZambon/ZDatabase) and
[ZSecurity](https://github.com/RicardoZambon/ZSecurity), which ship the same three targets.
ZDatabase carries the EF Core dependency for each of them.

The `net9.0` and `net8.0` targets exist for compatibility with applications that have not yet
moved to .NET 10. Both of those runtimes are at or near end of support, so new projects should
target `net10.0`.
