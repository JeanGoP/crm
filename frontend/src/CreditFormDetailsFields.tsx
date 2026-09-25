import { Accordion, AccordionDetails, AccordionSummary, Box, TextField, Typography } from '@mui/material';
import ExpandMore from '@mui/icons-material/ExpandMore';
import { currencyInputValue } from './quotePayments';

export interface CreditFormDetails {
  identificationType?: string | null;
  housingType?: string | null;
  maritalStatus?: string | null;
  email?: string | null;
  address?: string | null;
  city?: string | null;
  employer?: string | null;
  jobTitle?: string | null;
  occupation?: string | null;
  workPhone?: string | null;
  workAddress?: string | null;
  workEmail?: string | null;
  locationReference?: string | null;
  reference1Address?: string | null;
  reference2Address?: string | null;
  zone?: string | null;
  advisor?: string | null;
  salesPoint?: string | null;
  businessType?: string | null;
  vehicleType?: string | null;
  vehicleLine?: string | null;
  vehicleVariant?: string | null;
  vehicleModel?: string | null;
  vehicleColor?: string | null;
  vehicleEngineCc?: string | null;
  vehiclePlate?: string | null;
  vehicleChassis?: string | null;
  vehicleBrand?: string | null;
  vehicleEngine?: string | null;
  vehicleNotes?: string | null;
  extraPayment?: number | null;
  extraPaymentCount?: number | null;
  monthlyPayment?: number | null;
  soat?: number | null;
  registration?: number | null;
  totalCredit?: number | null;
}

const labels = {
  identificationType: 'Tipo de documento',
  housingType: 'Tipo de vivienda',
  maritalStatus: 'Estado civil',
  email: 'Correo electrónico',
  address: 'Dirección',
  city: 'Ciudad',
  employer: 'Empresa donde labora',
  jobTitle: 'Cargo',
  occupation: 'Profesión / oficio',
  workPhone: 'Teléfono laboral',
  workAddress: 'Dirección laboral',
  workEmail: 'Correo laboral',
  locationReference: 'Puntos de referencia',
  reference1Address: 'Dirección referencia 1',
  reference2Address: 'Dirección referencia 2',
  zone: 'Zona',
  advisor: 'Asesor',
  salesPoint: 'Punto de venta',
  businessType: 'Tipo de negocio',
  vehicleType: 'Tipo de vehículo / artículo',
  vehicleLine: 'Línea',
  vehicleVariant: 'Cuál / versión',
  vehicleModel: 'Modelo',
  vehicleColor: 'Color',
  vehicleEngineCc: 'Cilindraje',
  vehiclePlate: 'Placa',
  vehicleChassis: 'Chasis',
  vehicleBrand: 'Marca',
  vehicleEngine: 'Número de motor',
  vehicleNotes: 'Observaciones del vehículo',
  extraPayment: 'Cuota extra',
  extraPaymentCount: 'Cantidad cuotas extras',
  monthlyPayment: 'Valor cuota',
  soat: 'SOAT',
  registration: 'Matrícula',
  totalCredit: 'Total crédito',
};
const numberFields = new Set<keyof CreditFormDetails>(['extraPayment', 'extraPaymentCount', 'monthlyPayment', 'soat', 'registration', 'totalCredit']);
export function CreditFormDetailsFields({ value: suppliedValue, onChange, group, coDebtor = false, phoneError }: {
  value?: CreditFormDetails | null; onChange: (value: CreditFormDetails) => void;
  group: 'person' | 'references' | 'sale'; coDebtor?: boolean; phoneError?: string;
}) {
  const value = suppliedValue ?? {};
  const keys: (keyof CreditFormDetails)[] = group === 'references' ? ['reference1Address', 'reference2Address']
    : group === 'sale' ? ['zone', 'advisor', 'salesPoint', 'businessType', 'extraPayment', 'extraPaymentCount', 'monthlyPayment', 'soat', 'registration', 'totalCredit', 'vehicleType', 'vehicleLine', 'vehicleVariant', 'vehicleModel', 'vehicleColor', 'vehicleEngineCc', 'vehiclePlate', 'vehicleChassis', 'vehicleBrand', 'vehicleEngine', 'vehicleNotes']
    : [...(coDebtor ? ['identificationType', 'address', 'city', 'occupation'] as const : []), 'housingType', 'maritalStatus', 'email', 'employer', 'jobTitle', 'workPhone', 'workAddress', 'workEmail', 'locationReference'];
  const fields = <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, minmax(0, 1fr))', md: 'repeat(3, minmax(0, 1fr))' }, gap: 2, pt: 1 }}>
    {keys.map(key => <TextField key={key} fullWidth label={labels[key]} value={numberFields.has(key) && key !== 'extraPaymentCount' ? currencyInputValue(value[key] as number | undefined) : value[key] ?? ''}
      type={key === 'extraPaymentCount' ? 'number' : key === 'email' || key === 'workEmail' ? 'email' : 'text'}
      inputProps={numberFields.has(key) ? { min: 0, step: key === 'extraPaymentCount' ? 1 : 'any' } : { maxLength: 500 }}
      error={key === 'workPhone' && !!phoneError} helperText={key === 'workPhone' ? phoneError : undefined}
      onChange={event => onChange({ ...value, [key]: numberFields.has(key) ? event.target.value === '' ? null : Number(key === 'extraPaymentCount' ? event.target.value : event.target.value.replace(/\D/g, '')) : event.target.value })} />)}
  </Box>;
  return group === 'references' ? fields : <Accordion disableGutters variant="outlined" defaultExpanded={!!phoneError} sx={{ boxShadow: 'none', '&:before': { display: 'none' } }}>
    <AccordionSummary expandIcon={<ExpandMore />}><Typography fontWeight={800}>{group === 'sale' ? 'Información de venta y características del vehículo / artículo' : 'Datos personales y actividad económica'}{phoneError ? ' · Teléfono repetido' : ''}</Typography></AccordionSummary>
    <AccordionDetails>{fields}</AccordionDetails>
  </Accordion>;
}
