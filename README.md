# saudi-plus-ai-lab
Enterprise .NET application with modern engineering practices and AI integrations

## Local PostgreSQL

Copy `.env.example` to `.env`, then replace `POSTGRES_PASSWORD` with a unique local password. The `.env` file is ignored by Git. Set the same password in the API project's .NET User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=127.0.0.1;Port=5433;Database=saudi_plus;Username=saudi_plus;Password=<your-local-password>" --project SaudiPlus/SaudiPlus.Api/SaudiPlus.Api.csproj
```

PostgreSQL is published on `127.0.0.1:5433`. Start PostgreSQL with `docker compose up -d`. Check health with `docker compose ps` and stop it with `docker compose down`. The named volume retains data between stops; **`docker compose down -v` deletes the database data**.


## EF Core migrations

From the repository root, restore the local EF tool with `dotnet tool restore`. With `DefaultConnection` set in API User Secrets to the Docker database on port 5433, generate a migration with:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet tool run dotnet-ef migrations add <Name> --project SaudiPlus/SaudiPlus.Infrastructure --startup-project SaudiPlus/SaudiPlus.Api --context SaudiPlusDbContext --output-dir Persistence/Migrations
```

Apply pending migrations with `dotnet tool run dotnet-ef database update --project SaudiPlus/SaudiPlus.Infrastructure --startup-project SaudiPlus/SaudiPlus.Api --context SaudiPlusDbContext`.

## Offer query index benchmark

On a separate temporary PostgreSQL database with 100,000 synthetic offers, one warm-up and five measured `EXPLAIN (ANALYZE, BUFFERS)` runs gave these median execution times for `discount_percentage >= 30`:

| Query | Before index | With `(discount_percentage DESC, id ASC)` |
| --- | ---: | ---: |
| Count | 12.652 ms | 12.074 ms |
| First page, discount descending | 13.962 ms | 0.106 ms |

The count retained a sequential scan and saw no meaningful benefit. The page query changed from a sequential scan plus sort to an index scan; shared buffer hits fell from 1,397 to 23. Measurements used warm cached data on one local machine, a synthetic distribution, and only the first page. Execution times exclude API and network overhead; other filters, deeper pages, and production-sized data may behave differently.

## Identity roles

After applying migrations, initialize the `Customer`, `Merchant`, and `Administrator` roles with `ASPNETCORE_ENVIRONMENT=Development` and:

```powershell
dotnet run --project SaudiPlus/SaudiPlus.Api/SaudiPlus.Api.csproj --no-launch-profile -- --initialize-roles
```

This command is repeatable and exits without starting the API. It does not create users or credentials.

## Authentication

`POST /api/auth/register` accepts email and password and assigns Customer on the server. `POST /api/auth/login` returns a signed access token and its UTC expiry. Access tokens last 15 minutes. Five failed password attempts trigger Identity's five-minute lockout.

Configure `Jwt:SigningKey` in API User Secrets using a cryptographically random key of at least 32 bytes. Never place it in appsettings or source control. JWT issuer defaults to `SaudiPlus` and audience to `SaudiPlus.Api`; both can be configured under `Jwt`. Swagger provides Bearer authentication. Existing offer endpoints remain public.

## Offer authorization and ownership

`POST /api/offers` requires a Bearer token with the Merchant or Administrator role. Offer reads remain public. A Merchant may create offers only for a merchant they belong to; an Administrator may create for any existing merchant. Ownership is checked again in the Application service, so direct service calls follow the same rule. Merchant and membership records are provisioned through trusted internal code; there is no public provisioning endpoint. Existing offers have no merchant foreign key because their historical merchant IDs were not provisioned as merchant records.
