import test from 'node:test';
import assert from 'node:assert/strict';
import { normalizeTenant, enrichTenantsWithLookup } from './tenantUtils.js';

test('normalizeTenant fills property and unit details from context when the API payload is sparse', () => {
  const tenant = normalizeTenant(
    { id: 7, firstName: 'Jane', lastName: 'Doe' },
    {
      property: { id: 3, propertyName: 'Palm House' },
      unit: { id: 9, unitNumber: 'A2', unitType: '2 Bedroom' },
      rentAmount: 14000,
      moveInDate: '2024-01-01T00:00:00.000Z',
    }
  );

  assert.equal(tenant.propertyName, 'Palm House');
  assert.equal(tenant.unitNumber, 'A2');
  assert.equal(tenant.unitType, '2 Bedroom');
  assert.equal(tenant.rentAmount, 14000);
});

test('enrichTenantsWithLookup resolves property and unit names from lookup arrays', () => {
  const enriched = enrichTenantsWithLookup(
    [{ id: 1, propertyId: 2, unitId: 5, rentAmount: 12000 }],
    [{ id: 2, propertyName: 'Green Towers' }],
    [{ id: 5, unitNumber: 'B12', unitType: 'Studio' }]
  );

  assert.equal(enriched[0].propertyName, 'Green Towers');
  assert.equal(enriched[0].unitNumber, 'B12');
  assert.equal(enriched[0].unitType, 'Studio');
});
