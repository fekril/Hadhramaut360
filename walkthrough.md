# Discover Content — Implementation Walkthrough

## Build Result
✅ **Build succeeded** — 0 errors, 2 warnings (pre-existing in `AppDbContext.cs`, unrelated to this feature)

---

## Files Added

| File | Purpose |
|------|---------|
| [DiscoverContentQueryDto.cs](file:///c:/Users/Asiana/source/repos/API/Had360back/Dtos/DiscoverContent/DiscoverContentQueryDto.cs) | Pagination + search query params |
| [DiscoverContentResponseDto.cs](file:///c:/Users/Asiana/source/repos/API/Had360back/Dtos/DiscoverContent/DiscoverContentResponseDto.cs) | Lightweight card DTO (list view) |
| [DiscoverContentDetailsDto.cs](file:///c:/Users/Asiana/source/repos/API/Had360back/Dtos/DiscoverContent/DiscoverContentDetailsDto.cs) | Full details DTO (single item view) |
| [CreateDiscoverContentDto.cs](file:///c:/Users/Asiana/source/repos/API/Had360back/Dtos/DiscoverContent/CreateDiscoverContentDto.cs) | Create input DTO |
| [UpdateDiscoverContentDto.cs](file:///c:/Users/Asiana/source/repos/API/Had360back/Dtos/DiscoverContent/UpdateDiscoverContentDto.cs) | Update input DTO |
| [IDiscoverContentService.cs](file:///c:/Users/Asiana/source/repos/API/Had360back/Services/Interfaces/IDiscoverContentService.cs) | Service interface |
| [DiscoverContentService.cs](file:///c:/Users/Asiana/source/repos/API/Had360back/Services/Implementations/DiscoverContentService.cs) | Service implementation |
| [DiscoverContentController.cs](file:///c:/Users/Asiana/source/repos/API/Had360back/Controllers/DiscoverContentController.cs) | API controller |

## Files Modified

| File | Change |
|------|--------|
| [ApiRoutes.cs](file:///c:/Users/Asiana/source/repos/API/Had360back/Common/ApiRoutes.cs) | Added `DiscoverContent` static class with all route constants |
| [Program.cs](file:///c:/Users/Asiana/source/repos/API/Had360back/Program.cs) | Registered `IDiscoverContentService` → `DiscoverContentService` in DI |

---

## Endpoint Structure

### Public Endpoints (no authentication required)

| Method | URL | Description |
|--------|-----|-------------|
| GET | `/api/discover-content` | Paginated list with optional search |
| GET | `/api/discover-content/{id}` | Full details for a single item |
| GET | `/api/discover-content/culture` | Culture section (keyword-filtered) |
| GET | `/api/discover-content/food` | Food section (keyword-filtered) |
| GET | `/api/discover-content/games` | Games section (keyword-filtered) |

All list endpoints support `?Search=&PageNumber=1&PageSize=10` query parameters.

### Management Endpoints (Admin or ContentManager only)

| Method | URL | Description |
|--------|-----|-------------|
| POST | `/api/discover-content` | Create a new item |
| PUT | `/api/discover-content/{id}` | Update an existing item |
| DELETE | `/api/discover-content/{id}` | Delete an item |

---

## Authorization

Management endpoints use `[Authorize(Policy = AuthorizationPolicies.ManageContent)]` — exactly matching the `LandmarksController` convention. This policy covers the `Admin` and `ContentManager` roles as defined in `Program.cs`.

---

## Keyword-Based Section Design

Section endpoints call a shared `GetBySectionAsync(query, keywords)` method in the service. The only difference between sections is the keyword list passed in.

**Adding a new section** (e.g., `/api/discover-content/art`) requires:
1. Add a new keyword list to `SectionKeywords` inside the controller
2. Add one new `[HttpGet]` method that calls `GetBySectionAsync` with that list

No changes to the service, interface, or any other file needed.

### Keyword Lists

**Culture**: ثقافة، تراث، عادات، حضارة، موروث، culture، heritage، tradition، history

**Food**: أكل، أكلات، لذيذ، طبخ، كبسة، حلويات، food، cuisine، dish، meal، recipe

**Games**: لعبة، ألعاب، ألعاب شعبية، تراثية، game، games، traditional game، play

---

## Search Behavior

Search applies across all 4 text fields: `title_ar`, `title_en`, `body_ar`, `body_en`.

For section endpoints, keyword pre-filtering is applied first (OR logic across all keywords and fields), then the optional user search is applied on top of that result set.

---

## What Was Intentionally Excluded

- `IsPublished` — model field exists but is completely ignored
- File upload — `CoverImageUrl` is a plain string URL, not a file upload
- Categories — no category table dependency
- Many-to-many relationships — not needed
