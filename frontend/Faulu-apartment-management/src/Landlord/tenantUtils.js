export const normalizeTenant = (tenant, context = {}) => {
  if (!tenant) return null;

  const selectedProperty = context.property ?? null;
  const selectedUnit = context.unit ?? null;

  return {
    ...tenant,
    firstName: tenant.firstName ?? context.firstName ?? '',
    lastName: tenant.lastName ?? context.lastName ?? '',
    email: tenant.email ?? context.email ?? '',
    phoneNumber: tenant.phoneNumber ?? context.phoneNumber ?? context.phone ?? '',
    propertyName: tenant.propertyName
      || tenant.property?.name
      || tenant.buildingName
      || tenant.building?.name
      || selectedProperty?.propertyName
      || selectedProperty?.name
      || selectedProperty?.buildingName
      || tenant.property
      || tenant.buildingId
      || tenant.propertyId
      || '',
    propertyId: tenant.propertyId ?? tenant.buildingId ?? selectedProperty?.id ?? context.propertyId ?? null,
    buildingId: tenant.buildingId ?? tenant.propertyId ?? selectedProperty?.id ?? context.propertyId ?? null,
    unitNumber: tenant.unitNumber
      || tenant.unit?.unitNumber
      || selectedUnit?.unitNumber
      || tenant.unitId
      || '',
    unitId: tenant.unitId ?? selectedUnit?.id ?? context.unitId ?? null,
    unitType: tenant.unitType ?? selectedUnit?.unitType ?? '',
    rentAmount: tenant.rentAmount ?? tenant.rent ?? context.rentAmount ?? null,
    moveInDate: tenant.moveInDate ?? context.moveInDate ?? null,
    createdAt: tenant.createdAt ?? context.createdAt ?? new Date().toISOString(),
  };
};

export const enrichTenantsWithLookup = (tenantsList, propertiesList, unitsList) => {
  if (!Array.isArray(tenantsList)) return [];

  return tenantsList.map((tenant) => {
    if (!tenant) return tenant;

    let propertyName = tenant.propertyName || tenant.buildingName;
    let unitNumber = tenant.unitNumber;
    let unitType = tenant.unitType;

    const propId = tenant.propertyId ?? tenant.buildingId;
    const unitId = tenant.unitId;

    if (!propertyName && propId) {
      const prop = propertiesList.find((p) => String(p.id) === String(propId));
      propertyName = prop?.propertyName || prop?.name || prop?.buildingName || propertyName;
    }

    if (!unitNumber && unitId) {
      const unit = unitsList.find((u) => String(u.id) === String(unitId));
      unitNumber = unit?.unitNumber || unit?.name || unitNumber;
      unitType = unit?.unitType || unitType;
    }

    return {
      ...tenant,
      propertyName: propertyName || tenant.propertyName || tenant.buildingId || tenant.propertyId || '',
      unitNumber: unitNumber || tenant.unitNumber || tenant.unitId || '',
      unitType: unitType || tenant.unitType || '',
    };
  });
};
