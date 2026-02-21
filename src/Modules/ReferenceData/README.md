# ReferenceData Module

Global reference data for the TadHub platform. Contains static/slowly-changing data that is shared across all tenants and modules.

## Entities

### Country
ISO 3166-1 compliant country reference with:
- Alpha-2 and Alpha-3 codes
- Bilingual names (English/Arabic)
- Nationality adjectives
- Dialing codes
- Common Tadbeer nationality flag (for quick filtering)

### JobCategory
19 official MoHRE job categories for domestic workers:
- MoHRE code
- Bilingual names
- Display order

## API Endpoints

### Countries
```
GET  /api/v1/countries                    # List with filtering/pagination
GET  /api/v1/countries/{id}               # Get by ID
GET  /api/v1/countries/by-code/{code}     # Get by ISO alpha-2 code
GET  /api/v1/countries/refs               # Lightweight refs for dropdowns
GET  /api/v1/countries/common-nationalities  # Common Tadbeer nationalities
```

### Job Categories
```
GET  /api/v1/job-categories               # List with filtering/pagination
GET  /api/v1/job-categories/all           # All active categories (no pagination)
GET  /api/v1/job-categories/{id}          # Get by ID
GET  /api/v1/job-categories/by-code/{code}  # Get by MoHRE code
GET  /api/v1/job-categories/refs          # Lightweight refs for dropdowns
```

## Seeding

Reference data is seeded on application startup:
- 19 MoHRE job categories
- 70+ countries with:
  - 10 common Tadbeer nationalities (Philippines, Indonesia, Ethiopia, India, Sri Lanka, Nepal, Bangladesh, Uganda, Kenya, Ghana)
  - 6 GCC countries
  - 50+ other major countries

## Usage

### Frontend Components
Use `/refs` endpoints for lightweight dropdown data:

```typescript
// React example
const { data: countries } = useQuery('countries-refs', () => 
  api.get('/api/v1/countries/refs')
);

// For worker nationality selection, use common nationalities
const { data: nationalities } = useQuery('common-nationalities', () => 
  api.get('/api/v1/countries/common-nationalities')
);
```

### Worker Module Integration
The Worker module references `ReferenceData.Core.Entities.JobCategory` directly for the foreign key relationship.
