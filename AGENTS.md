# Saudi Plus AI Lab

Production-oriented learning project that progressively integrates LLMs, tool calling, MCP, RAG, AI agents, evaluations, and observability.

## Technology and architecture

- .NET 10, ASP.NET Core, EF Core, PostgreSQL 17. PostgreSQL runs in Docker on localhost:5433; the API runs locally through Visual Studio. Configure `DefaultConnection` through User Secrets. Keep EF migrations in `Infrastructure/Persistence/Migrations`.
- Preserve Clean Architecture boundaries: Domain owns business rules without infrastructure dependencies; Application owns use cases and repository abstractions; Infrastructure implements persistence and external integrations; API owns HTTP transport and dependency registration.
- Keep application services reusable by future AI tools and agents. Follow existing naming and coding patterns. Add abstractions, dependencies, and optimizations only when needed.

## Development

- Preserve existing functionality and API contracts unless explicitly requested. Keep domain invariants and database constraints consistent.
- Use async operations, propagate cancellation tokens, and validate inputs before accessing external resources.
- Keep credentials outside source control and output. Obtain explicit approval before destructive database operations. Commit, push, or merge only when explicitly requested.
- Add focused tests for new or changed behavior; preserve existing assertions.

## Working context and verification

- Read only task-relevant files and immediate dependencies; reuse current context. Avoid full scans, repeated reads, unrelated refactoring, and speculative implementation.
- Run relevant verification once; repeat only after a relevant change or failure. Run the full test suite at milestone completion or when requested. Keep plans and explanations brief. Ask only when ambiguity materially affects implementation. Preserve correctness and security regardless of token cost.
- Read [docs/PROGRESS.md](docs/PROGRESS.md) when milestone context is needed. Update it after each course goal as a compact current-state summary, without repeating this file or README.
- Report files changed, implementation summary, verification results, and unresolved issues or important decisions concisely.
