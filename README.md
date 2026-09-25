# FieldOps Copilot

A secure AI assistant for field and warehouse teams: ask questions over equipment manuals and SOPs with cited answers, and let an agent check stock or raise maintenance requests, with approval.

**Status:** building in public, one milestone at a time.

## Stack
.NET 10 · ASP.NET Core minimal APIs · EF Core · Azure OpenAI · Azure AI Search · Microsoft Agent Framework · .NET MAUI

## Run locally
    dotnet run --project src/FieldOps.Api
    open http://localhost:<port>/health

## Roadmap
- [x] API skeleton: health, OpenAPI, JSON logging
- [ ] Persistence + JWT auth
- [ ] RAG v0 with citations
- [ ] Azure deployment
- [ ] Agent with tools + human approval
- [ ] MAUI client