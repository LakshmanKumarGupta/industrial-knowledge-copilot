# Industrial Knowledge Intelligence — Unified Asset & Operations Brain

> ET AI Hackathon 2.0 — Phase 2 Submission
> Problem Statement #8: AI for Industrial Knowledge Intelligence

## Problem

Indian heavy industry plants operate across 7-12 disconnected document systems —
engineering drawings, maintenance records, SOPs, inspection reports — scattered across
formats and folders. Engineers spend ~35% of their time searching for information instead
of acting on it. This project builds an AI layer that makes that knowledge instantly
queryable, with full source traceability.

## What This Project Does

1. **Document Ingestion Pipeline** — Upload PDFs (manuals, SOPs, inspection reports) →
   automatic chunking → embedding → indexed in a vector store.
2. **Expert Knowledge Copilot** — A chat interface where engineers ask natural-language
   questions and get answers **grounded in the actual documents**, with:
   - Source citations (which document, which page/section)
   - Confidence indication
   - Honest "I don't know" when no document supports the answer

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular |
| Backend / Orchestration | .NET 8 Web API |
| Agent Framework | Microsoft Semantic Kernel |
| LLM (Generation) | Groq (Llama 3.x) — free tier *(swap-in ready for Azure OpenAI)* |
| Embeddings | Gemini Embedding API — free tier *(swap-in ready for Azure OpenAI)* |
| Vector Store | Qdrant Cloud — free tier *(swap-in ready for Azure AI Search)* |
| Document Parsing | PdfPig / iText (.NET) |

> **Design note:** This prototype uses free-tier equivalents of Azure OpenAI, Azure AI
> Search due to hackathon resource constraints. The architecture is provider-agnostic by
> design (via Semantic Kernel connectors) — switching to Azure OpenAI / Azure AI Search
> for production deployment is a configuration change, not a redesign.

## Project Structure

```
industrial-knowledge-copilot/
├── backend/              # .NET 8 Web API + Semantic Kernel
├── frontend/             # Angular chat UI
├── sample-documents/     # Sample PDFs used for the demo corpus
├── architecture/         # Architecture diagrams
└── docs/                 # Detailed document, deck notes
```

## Status

🚧 Built for ET AI Hackathon 2.0 (Phase 2) — Submission deadline: 22 July 2026

## Author

Lakshman Kumar Gupta — Senior Full Stack Engineer | .NET · Angular · Azure AI
