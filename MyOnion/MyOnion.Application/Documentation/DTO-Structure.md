<!--
app/views/partials/folders.html
{% for folder in folders %}
  <li>{{ folder.name }}
    {% if folder.subfolders %}
      <ul>
        {% for subfolder in folder.subfolders %}
          <li>{{ subfolder.name }}</li>
        {% endfor %}
      </ul>
    {% endif %}
  </li>
{% endfor %}
-->

# DTO Folder Structure Plan

## Goals
- Surface request/response contracts so they are easy to discover.
- Avoid leaking domain entities through API responses.
- Keep CQRS intent intact (commands/queries stay with their handlers).

## Proposed Structure
- Each feature area gets an optional `DTOs/` folder under `Features/<FeatureName>/`.
- Response view models or shared request shapes live there (e.g., `PositionSummaryDto.cs`).
- Command classes stay under `Commands/`, but may compose DTOs from the shared folder if multiple commands reuse the same payload.
- Mapping profiles reference DTOs from the feature-specific folder; when DTOs are moved/renamed, update the profile + handlers referencing them.

### Example: Positions Feature
```
Features/Positions/
  Commands/
    CreatePosition/…
    UpdatePosition/…
  Queries/
    GetPositionById/…
    GetPositions/…
  DTOs/
    PositionSummaryDto.cs   <-- shared list/detail projection
```

## Next Steps
- Adopt this pattern for other entities (Employees, Departments, etc.) as DTOs emerge.
- Document the expectation in contributor guidelines so new features follow the same layout.
