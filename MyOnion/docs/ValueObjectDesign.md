# Value Object Design Plan

## Purpose
Establish value objects for identity-like strings (Position Title, Department Name, Employee Name) to ensure consistent normalization, validation, and display across the domain and read models.

## Use Cases
- Enforce global uniqueness for Position Title and Department Name (case/whitespace insensitive).
- Avoid duplicate variants such as "Sales", " sales", and "Sales ".
- Provide consistent employee full-name formatting for APIs, reports, and UI.
- Centralize validation rules (required, max length, allowed characters).

## Proposed Value Objects
- `PositionTitle`
  - Normalizes by trimming and collapsing whitespace.
  - Validates required and max length (aligned with existing EF limits).
  - Equality compares normalized value.
- `DepartmentName`
  - Same normalization and validation behavior as `PositionTitle`.
  - Supports global uniqueness.
- `PersonName`
  - Normalizes `First`, `Middle`, and `Last` names.
  - Provides `FullName` formatting with middle-name handling.

## Domain Model Changes
- Replace `Position.PositionTitle` with `Position.Title : PositionTitle`.
- Replace `Department.Name` with `Department.Name : DepartmentName`.
- Replace `Employee.FirstName/MiddleName/LastName` with `Employee.Name : PersonName`.
- Keep DTOs as strings to minimize API changes; map from value object `.Value` or `.FullName`.

## Persistence & Uniqueness
- Add EF Core value converters for each value object.
- Create unique indexes on normalized `PositionTitle` and `DepartmentName` columns.
- Ensure normalization is applied before persistence to prevent duplicates.

## Validation & Mapping
- Update validators to accept value-object inputs or normalize before mapping.
- Update mapping code to use `.Value` and `.FullName` for DTOs.
- Keep query filters using normalized strings for consistent search.

## Incremental Rollout Plan
1. Introduce value objects + converters without changing API contracts.
2. Update entities and mappings, then adjust validators.
3. Add unique indexes and data migration/cleanup if needed.
4. Verify search/filter behavior remains consistent.

## Risks & Mitigations
- Data conflicts during migration: run a pre-check to detect duplicates.
- Client-side expectations: keep DTO shapes unchanged.
- Query changes: ensure EF translations still work with value converters.
