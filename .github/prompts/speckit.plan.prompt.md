---
description: Execute the implementation planning workflow using the plan template to generate design artifacts.
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

1. **Setup**: Run `.specify/scripts/powershell/setup-plan.ps1 -Json` from repo root and parse JSON for FEATURE_SPEC, IMPL_PLAN, SPECS_DIR, BRANCH. For single quotes in args like "I'm Groot", use escape syntax: e.g 'I'\''m Groot' (or double-quote if possible: "I'm Groot").

2. **Load context**: Read FEATURE_SPEC, `.specify/memory/constitution.md`, `.github/prompts/architecture.prompt.md` for architecture standards, `.github/prompts/backend.prompt.md` for backend patterns, `.github/prompts/frontend.prompt.md` for frontend patterns, `.github/prompts/database.prompt.md` for database design, and `.github/prompts/security.prompt.md` for security requirements. Load IMPL_PLAN template (already copied).

3. **Execute plan workflow**: Follow the structure in IMPL_PLAN template to:
   - Fill Technical Context based on WahadiniCryptoQuest tech stack (.NET 8, React 18, PostgreSQL, etc.)
   - Apply Clean Architecture principles from architecture.prompt.md (Domain, Application, Infrastructure, Presentation layers)
   - Fill Constitution Check section from constitution
   - Evaluate gates (ERROR if violations unjustified)
   - Phase 0: Generate research.md (resolve all NEEDS CLARIFICATION using architecture patterns)
   - Phase 1: Generate data-model.md (following domain entity patterns), contracts/ (RESTful API), quickstart.md
   - Phase 1: Update agent context by running the agent script
   - Re-evaluate Constitution Check post-design

4. **Stop and report**: Command ends after Phase 2 planning. Report branch, IMPL_PLAN path, and generated artifacts.

## Phases

### Phase 0: Outline & Research

1. **Extract unknowns from Technical Context** above:
   - For each NEEDS CLARIFICATION → research task
   - For each dependency → best practices task
   - For each integration → patterns task

2. **Generate and dispatch research agents**:

   ```text
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

### Phase 1: Design & Contracts

**Prerequisites:** `research.md` complete

1. **Extract entities from feature spec** → `data-model.md`:
   - Follow Domain Entity Design patterns from architecture.prompt.md
   - Use factory methods for entity creation, private setters for encapsulation
   - Entity name, fields (with proper types), relationships (navigation properties)
   - Validation rules from requirements (domain validations in entity methods)
   - State transitions if applicable (using domain events)
   - Include base entity pattern (Id, CreatedAt, UpdatedAt, IsDeleted for soft delete)

2. **Generate API contracts** from functional requirements:
   - Follow RESTful API patterns and controller design from architecture.prompt.md
   - For each user action → endpoint with proper HTTP verb (GET, POST, PUT, DELETE)
   - Use standard REST patterns with proper status codes
   - Include authorization attributes ([Authorize], role-based)
   - Output OpenAPI/GraphQL schema to `/contracts/`
   - Define request/response DTOs with validation attributes
   - Document success/error response codes

3. **Agent context update**:
   - Run `.specify/scripts/powershell/update-agent-context.ps1 -AgentType copilot`
   - These scripts detect which AI agent is in use
   - Update the appropriate agent-specific context file
   - Add only new technology from current plan
   - Preserve manual additions between markers

**Output**: data-model.md, /contracts/*, quickstart.md, agent-specific file

## Key rules

- Use absolute paths
- ERROR on gate failures or unresolved clarifications
