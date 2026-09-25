type PhoneOwner = { mobile?: string | null; reference1Mobile?: string | null; reference2Mobile?: string | null; formDetails?: { workPhone?: string | null } };
type ApplicationPhones = PhoneOwner & {
  coDebtors?: (PhoneOwner & { name?: string })[] | null;
  coDebtorName?: string | null;
  coDebtorMobile?: string | null;
  coDebtorReference1Mobile?: string | null;
  coDebtorReference2Mobile?: string | null;
};
export type DuplicatePhone = { phone: string; fields: { path: string; label: string }[] };

// Same comparison rules as CreditPhoneValidation.Normalize on the server.
export function normalizeCreditPhone(value?: string | null): string {
  let digits = (value ?? '').replace(/[^0-9]/g, '');
  if (digits.startsWith('00')) digits = digits.slice(2);
  if (digits.length === 12 && digits.startsWith('57')) digits = digits.slice(2);
  return digits;
}

export function findDuplicateCreditPhones(value: ApplicationPhones): DuplicatePhone[] {
  const fields: { path: string; label: string; value?: string | null }[] = [
    { path: 'mobile', label: 'Cliente - celular', value: value.mobile },
    { path: 'formDetails.workPhone', label: 'Cliente - teléfono laboral', value: value.formDetails?.workPhone },
    { path: 'reference1Mobile', label: 'Cliente - referencia 1', value: value.reference1Mobile },
    { path: 'reference2Mobile', label: 'Cliente - referencia 2', value: value.reference2Mobile },
  ];
  if (value.coDebtors != null) {
    value.coDebtors.forEach((person, index) => {
      const label = `Codeudor ${index + 1}${person.name ? ` (${person.name})` : ''}`;
      fields.push(
        { path: `coDebtors.${index}.mobile`, label: `${label} - celular`, value: person.mobile },
        { path: `coDebtors.${index}.formDetails.workPhone`, label: `${label} - teléfono laboral`, value: person.formDetails?.workPhone },
        { path: `coDebtors.${index}.reference1Mobile`, label: `${label} - referencia 1`, value: person.reference1Mobile },
        { path: `coDebtors.${index}.reference2Mobile`, label: `${label} - referencia 2`, value: person.reference2Mobile },
      );
    });
  } else if (value.coDebtorName?.trim()) {
    fields.push(
      { path: 'coDebtorMobile', label: 'Codeudor 1 - celular', value: value.coDebtorMobile },
      { path: 'coDebtorReference1Mobile', label: 'Codeudor 1 - referencia 1', value: value.coDebtorReference1Mobile },
      { path: 'coDebtorReference2Mobile', label: 'Codeudor 1 - referencia 2', value: value.coDebtorReference2Mobile },
    );
  }
  const groups = new Map<string, DuplicatePhone>();
  fields.forEach(field => {
    const phone = normalizeCreditPhone(field.value);
    if (!phone) return;
    const group = groups.get(phone) ?? { phone, fields: [] };
    group.fields.push({ path: field.path, label: field.label });
    groups.set(phone, group);
  });
  return Array.from(groups.values()).filter(group => group.fields.length > 1);
}

export function duplicateCreditPhoneMessage(groups: DuplicatePhone[]): string {
  return 'No se puede guardar: hay teléfonos repetidos en la solicitud. ' +
    groups.map(group => `${group.phone}: ${group.fields.map(field => field.label).join(', ')}`).join('; ') + '. Corrija los campos indicados.';
}

export function creditPhoneFieldError(groups: DuplicatePhone[], path: string): string | undefined {
  const group = groups.find(item => item.fields.some(field => field.path === path));
  return group ? `Repetido con: ${group.fields.filter(field => field.path !== path).map(field => field.label).join(', ')}.` : undefined;
}
