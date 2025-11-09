# Refactoring & Technical Debt Notes

## High Priority

### 1. Replace Entity Returns with DTOs
**Issue:** API endpoints currently return Entity Framework entities directly, which causes:
- Circular reference issues (requiring `IgnoreCycles` workaround)
- Exposure of internal navigation properties (`Expedition: null`, etc.)
- Potential performance issues from over-fetching data
- Less control over API response shape

**Affected Endpoints:**
- `GET /api/expeditions/active` - Returns full Expedition entities
- `GET /api/expeditions/{id}` - Returns full Expedition entity
- `GET /api/expeditions/{id}/hexes` - Returns ExpeditionHex entities
- `POST /api/hexmapadmin/expeditions/create` - Returns full Expedition
- All character endpoints
- All user endpoints

**Solution:**
Create proper DTOs for all API responses:
```csharp
// Example structure:
public class ExpeditionDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int LeaderUserId { get; set; }
    public string LeaderUsername { get; set; }  // Flattened from Leader.Username
    public List<ExpeditionMemberDto> Members { get; set; }
    public ExpeditionStatus Status { get; set; }
    // No navigation properties that cause cycles
}
```

**Files to Create:**
- `Westmarchestool.API/DTOs/Expeditions/ExpeditionDto.cs`
- `Westmarchestool.API/DTOs/Expeditions/ExpeditionHexDto.cs`
- `Westmarchestool.API/DTOs/Expeditions/ExpeditionMemberDto.cs`
- `Westmarchestool.API/DTOs/HexMap/TownMapHexDto.cs`
- `Westmarchestool.API/DTOs/HexMap/GMMapHexDto.cs`
- `Westmarchestool.API/DTOs/HexMap/PointOfInterestDto.cs`

**Mapping:**
Consider using AutoMapper or manual mapping in service layer.

---

## Medium Priority

### 2. Remove Temporary Admin Promotion Endpoint
**Location:** `AuthController.cs` - `POST /api/auth/make-me-admin`

**Action:** Delete this endpoint once proper admin users are set up in production.

---

### 3. Add Logging
**Issue:** No structured logging for debugging/monitoring.

**Solution:** 
- Add Serilog or similar
- Log important events (expedition created, disputes detected, etc.)
- Log errors with context

---

### 4. Add Input Validation
**Issue:** Some endpoints lack proper validation.

**Examples:**
- Hex coordinates should have reasonable bounds
- Expedition names should have max length
- Terrain type enums should be validated

---

## Low Priority

### 5. API Versioning
Prepare for future API changes by implementing versioning strategy.

### 6. Add API Documentation
- Swagger/OpenAPI descriptions
- Example requests/responses
- Authentication requirements clearly documented

### 7. Unit Tests
Add tests for:
- TerrainGenerator
- HexMath utilities
- MapMergeService dispute detection
- Coordinate system conversions

---

## Completed
_(Nothing yet)_