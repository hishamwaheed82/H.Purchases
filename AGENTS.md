# H.Purchases

Soft Plan Purchases API (ABP Modular). Domains: `purchases-api` / `purchases`.

## Local
- API: https://localhost:44450
- UI: http://localhost:5180 (repo `purchases`)
- DB: `H_Purchases`
- Auth: https://localhost:44380 · client `H_Purchases_App` · audience `H.Purchases`

## Commands
```
dotnet run --project src/H.Purchases.DbMigrator
dotnet run --project src/H.Purchases.HttpApi.Host
```

## Notes
- SoftPlan catalog owns permission names; AppServices use `[Authorize]` only.
- GL: `PurchasesGl` → H.Accounting via `H_Ops_Machine` (Invoice post).
- Post also creates Inventory Receipt (`pi:{id}`).
- DbMigrator seeds demo data for every tenant (not only host).
