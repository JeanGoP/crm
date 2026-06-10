import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { Navigate, NavLink, Route, Routes, useNavigate, useParams } from 'react-router-dom';
import {
  Alert, AppBar, Box, Button, Card, CardContent, Checkbox, Chip, CssBaseline, Dialog, DialogActions,
  DialogContent, DialogTitle, Divider, Drawer, Grid, IconButton, LinearProgress, MenuItem,
  FormControlLabel, Paper, Snackbar, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, InputAdornment,
  ThemeProvider, Toolbar, Tooltip, Typography, createTheme, useMediaQuery, useTheme
} from '@mui/material';
import DashboardIcon from '@mui/icons-material/Dashboard';
import Groups from '@mui/icons-material/Groups';
import Handshake from '@mui/icons-material/Handshake';
import ViewKanban from '@mui/icons-material/ViewKanban';
import EventNote from '@mui/icons-material/EventNote';
import Settings from '@mui/icons-material/Settings';
import Logout from '@mui/icons-material/Logout';
import Menu from '@mui/icons-material/Menu';
import Add from '@mui/icons-material/Add';
import CheckCircle from '@mui/icons-material/CheckCircle';
import Edit from '@mui/icons-material/Edit';
import Delete from '@mui/icons-material/Delete';
import SyncAlt from '@mui/icons-material/SyncAlt';
import Close from '@mui/icons-material/Close';
import Inventory2 from '@mui/icons-material/Inventory2';
import ReceiptLong from '@mui/icons-material/ReceiptLong';
import Download from '@mui/icons-material/Download';
import UploadFile from '@mui/icons-material/UploadFile';
import Assignment from '@mui/icons-material/Assignment';
import LocalShipping from '@mui/icons-material/LocalShipping';
import Visibility from '@mui/icons-material/Visibility';
import AddTask from '@mui/icons-material/AddTask';
import WhatsApp from '@mui/icons-material/WhatsApp';
import Assessment from '@mui/icons-material/Assessment';
import AutoAwesome from '@mui/icons-material/AutoAwesome';
import { AxiosError } from 'axios';
import { api } from './api';
import { useAuthStore } from './store';
import { Activity, ColombianIdentityLookup, CommercialReports, Company, CreditApplication, CreditDocument, Customer, Customer360, CustomerAiAnalysis, CustomerTimelineItem, Dashboard, Deal, DealStage, FinancialSettings, Lead, MotorcycleDelivery, Product, ProductPhoto, Quote, QuoteSimulationResult, User } from './types';

const drawerWidth = 248;
const today = new Date().toISOString().slice(0, 10);
const currentMonthStart = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10);
const simitUrl = 'https://www.fcm.org.co/simit/#/home-public';
const runtUrl = 'https://portalpublico.runt.gov.co/#/consulta-ciudadano-documento/consulta/consulta-ciudadano-documento';
const companyLogoWidth = 320;
const companyLogoHeight = 160;
const companyLogoMaxBytes = 1_000_000;

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#155e75' },
    secondary: { main: '#7c2d12' },
    background: { default: '#f6f8fb' },
    success: { main: '#15803d' },
    warning: { main: '#b45309' }
  },
  shape: { borderRadius: 8 },
  typography: { fontFamily: '"Inter", "Segoe UI", Arial, sans-serif', button: { textTransform: 'none', fontWeight: 700 } }
});

type NavItem = { to: string; label: string; icon: ReactNode; locked?: boolean };

const nav: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: <DashboardIcon /> },
  { to: '/cotizaciones', label: 'Cotizaciones', icon: <ReceiptLong /> },
  { to: '/clientes', label: 'Clientes', icon: <Groups /> },
  { to: '/solicitudes-credito', label: 'Solicitudes credito', icon: <Assignment /> },
  { to: '/entregas', label: 'Entregas', icon: <LocalShipping /> },
  { to: '/pipeline', label: 'Pipeline', icon: <ViewKanban /> },
  { to: '/actividades', label: 'Actividades', icon: <EventNote /> },
  { to: '/productos', label: 'Productos', icon: <Inventory2 /> },
  { to: '/prospectos', label: 'Prospectos', icon: <Handshake /> },
  { to: '/reportes', label: 'Reportes', icon: <Assessment /> },
  { to: '/configuracion', label: 'Configuracion', icon: <Settings /> }
];

type Notice = { type: 'success' | 'error' | 'info'; text: string };
type FormMode<T> = { open: boolean; item?: T };

const emptyCustomer = {
  identificationType: 1,
  identificationNumber: '',
  firstNames: '',
  lastNames: '',
  firstName: '',
  middleName: '',
  lastName: '',
  secondLastName: '',
  companyName: '',
  email: '',
  phoneCountryCode: '+57',
  phone: '',
  address: '',
  city: '',
  birthDate: '',
  occupation: '',
  status: 1,
  tags: '',
  notes: ''
};
const emptyLead = { firstNames: '', lastNames: '', firstName: '', middleName: '', lastName: '', secondLastName: '', email: '', phone: '', source: 'Web', rating: 1 };
const emptyDeal = { title: '', customerId: '', stageId: '', value: 0, closeProbability: 10, estimatedCloseDate: today, status: 1 };
const emptyActivity = { title: '', description: '', type: 1, status: 1, scheduledAt: `${today}T09:00`, reminderAt: '', customerId: '', dealId: '', assignedUserId: '' };
const emptyCompany = { name: '', subdomain: '', customDomain: '', logoDataUrl: '', active: true };
const emptyUser = { fullName: '', email: '', password: '', companyId: '', roles: ['Vendedor'] };
const emptyProduct = { name: '', category: 'Moto', brand: '', model: '', reference: '', description: '', engineCc: '', year: '', color: '', price: 0, active: true };
const emptyFinancialSettings = { minimumWage: 1400000, consumerAnnualRate: 29.72, lowAmountAnnualRate: 56.33, factorMonthlyRate: 4.5, maxTermMonths: 30, paymentRounding: 1000, useMontelibanoTable: true, active: true };
const emptyQuote = { identificationType: 1, identificationNumber: '', customerFirstNames: '', customerLastNames: '', customerFirstName: '', customerMiddleName: '', customerLastName: '', customerSecondLastName: '', phoneCountryCode: '+57', phoneNumber: '', productId: '', downPayment: 0, insurance: 0, administrativeFees: 0, termMonths: 24, monthlyInterestRate: 2.2, notes: '' };
const emptyCreditApplication = {
  customerId: '', productId: '', quoteId: '', dealId: '', identificationType: 1, identificationNumber: '', birthDate: '', mobile: '', address: '', city: '', occupation: '',
  monthlyIncome: 0, downPayment: 0, termMonths: 24, motorcycleValue: 0,
  coDebtorName: '', coDebtorIdentification: '', coDebtorMobile: '', coDebtorRelationship: '', coDebtorMonthlyIncome: 0,
  reference1Name: '', reference1Mobile: '', reference1Relationship: '', reference2Name: '', reference2Mobile: '', reference2Relationship: '',
  status: 1, notes: ''
};
const emptyMotorcycleDelivery = {
  creditApplicationId: '',
  deliveryDate: `${today}T09:00`,
  responsibleAdvisor: '',
  vin: '',
  chassisNumber: '',
  engineNumber: '',
  plate: '',
  deliveryMileage: '',
  helmetDelivered: false,
  soatDelivered: false,
  registrationDelivered: false,
  warrantyManualDelivered: false,
  deliveryCertificateSigned: false,
  status: 1,
  notes: ''
};
const creditStatusOptions = [1, 8, 2, 4, 5, 6, 7, 9];

function fullFirstNames(firstName?: string, middleName?: string, fallback?: string) {
  const value = [firstName, middleName].filter(Boolean).join(' ').trim();
  return value || fallback || '';
}

function fullLastNames(lastName?: string, secondLastName?: string, fallback?: string) {
  const value = [lastName, secondLastName].filter(Boolean).join(' ').trim();
  return value || fallback || '';
}

function Layout() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();
  const muiTheme = useTheme();
  const isDesktop = useMediaQuery(muiTheme.breakpoints.up('md'));
  const [mobileOpen, setMobileOpen] = useState(false);
  const closeMobileNav = () => setMobileOpen(false);
  const drawerContent = <>
    <Toolbar sx={{ px: 3 }}>
      <Typography variant="h6" fontWeight={800}>CRM SaaS</Typography>
    </Toolbar>
    <Divider />
    <Stack sx={{ p: 1 }}>
      {nav.map((item) => item.locked ? (
        <Tooltip key={item.to} title="Disponible en la siguiente fase de la demostracion" placement="right">
          <span>
            <Button disabled startIcon={item.icon} sx={{ justifyContent: 'flex-start', my: .25, width: '100%', opacity: .48 }}>
              {item.label}
            </Button>
          </span>
        </Tooltip>
      ) : (
        <Button key={item.to} component={NavLink} to={item.to} startIcon={item.icon} onClick={closeMobileNav} sx={{ justifyContent: 'flex-start', my: .25 }}>
          {item.label}
        </Button>
      ))}
    </Stack>
  </>;
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', width: '100%', overflowX: 'hidden' }}>
      <Drawer
        variant={isDesktop ? 'permanent' : 'temporary'}
        open={isDesktop || mobileOpen}
        onClose={closeMobileNav}
        ModalProps={{ keepMounted: true }}
        PaperProps={{ sx: { width: drawerWidth, borderRight: '1px solid #dde5ee' } }}
      >
        {drawerContent}
      </Drawer>
      <Box sx={{ flex: 1, minWidth: 0, ml: { xs: 0, md: `${drawerWidth}px` } }}>
        <AppBar position="sticky" color="inherit" elevation={0} sx={{ borderBottom: '1px solid #dde5ee' }}>
          <Toolbar sx={{ justifyContent: 'space-between', gap: 1 }}>
            <Stack direction="row" alignItems="center" gap={1.25} sx={{ minWidth: 0 }}>
              {!isDesktop && <IconButton aria-label="Abrir menu" edge="start" onClick={() => setMobileOpen(true)}><Menu /></IconButton>}
              <Box sx={{ minWidth: 0 }}>
                <Typography fontWeight={800} noWrap>{user?.fullName ?? 'Equipo comercial'}</Typography>
                <Typography color="text.secondary" fontSize={13} noWrap>{user?.roles.join(', ')}</Typography>
              </Box>
            </Stack>
            <Tooltip title="Salir">
              <IconButton aria-label="Salir" onClick={() => { logout(); navigate('/login'); }}><Logout /></IconButton>
            </Tooltip>
          </Toolbar>
        </AppBar>
        <Box component="main" sx={{ p: { xs: 1.5, sm: 2, md: 3 }, width: '100%', maxWidth: '100vw', boxSizing: 'border-box' }}>
          <Routes>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/clientes" element={<CustomersPage />} />
            <Route path="/clientes/:id" element={<Customer360Page />} />
            <Route path="/productos" element={<ProductsPage />} />
            <Route path="/cotizaciones" element={<QuotesPage />} />
            <Route path="/solicitudes-credito" element={<CreditApplicationsPage />} />
            <Route path="/entregas" element={<MotorcycleDeliveriesPage />} />
            <Route path="/prospectos" element={<LeadsPage />} />
            <Route path="/pipeline" element={<PipelinePage />} />
            <Route path="/actividades" element={<ActivitiesPage />} />
            <Route path="/reportes" element={<CommercialReportsPage />} />
            <Route path="/configuracion" element={<SettingsPage />} />
          </Routes>
        </Box>
      </Box>
    </Box>
  );
}

function LockedModulePage() {
  return <Card><CardContent>
    <Stack spacing={1.5} alignItems="flex-start">
      <Chip label="Siguiente fase" color="primary" variant="outlined" />
      <Typography variant="h5" fontWeight={900}>Modulo reservado para la siguiente demostracion</Typography>
      <Typography color="text.secondary">
        Esta opcion esta preparada en el CRM, pero por ahora esta bloqueada para mostrar primero el flujo inicial: dashboard, clientes, productos y cotizaciones.
      </Typography>
    </Stack>
  </CardContent></Card>;
}

function LoginPage() {
  const [email, setEmail] = useState('admin@demo.com');
  const [password, setPassword] = useState('');
  const [tenant, setTenant] = useState(import.meta.env.VITE_TENANT ?? 'demo');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const setSession = useAuthStore((s) => s.setSession);
  const navigate = useNavigate();

  const login = async () => {
    setLoading(true);
    setError('');
    try {
      const { data } = await api.post('/api/auth/login', { email, password, tenant });
      setSession(data.accessToken, data.refreshToken, data.user);
      navigate('/');
    } catch (err) {
      setError(apiError(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box className="loginShell">
      <Paper className="loginPanel">
        <Typography variant="h4" fontWeight={900}>CRM SaaS</Typography>
        <Typography color="text.secondary" sx={{ mb: 3 }}>Gestion comercial multiempresa</Typography>
        <Stack spacing={2}>
          <TextField label="Empresa" value={tenant} onChange={(e) => setTenant(e.target.value)} />
          <TextField label="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
          <TextField label="Contrasena" type="password" value={password} onChange={(e) => setPassword(e.target.value)} onKeyDown={(e) => e.key === 'Enter' && login()} />
          {error && <Alert severity="error">{error}</Alert>}
          {loading && <LinearProgress />}
          <Button variant="contained" size="large" onClick={login} disabled={loading}>Ingresar</Button>
        </Stack>
      </Paper>
    </Box>
  );
}

function DashboardPage() {
  const { data, loading, error, reload } = useResource<Dashboard>('/api/dashboard');
  const navigate = useNavigate();
  const cards: { label: string; value: ReactNode }[] = [
    { label: 'Pipeline abierto', value: money(data?.openPipelineValue) },
    { label: 'Pipeline ponderado', value: money(data?.weightedPipelineValue) },
    { label: 'Clientes activos', value: data?.activeCustomers ?? 0 },
    { label: 'Prospectos abiertos', value: data?.openLeads ?? 0 },
    { label: 'Actividades pendientes', value: data?.pendingActivities ?? 0 },
    { label: 'Vencidas', value: data?.overdueActivities ?? 0 },
    { label: 'Para hoy', value: data?.todayActivities ?? 0 }
  ];
  return <Stack spacing={3}>
    <Header title="Dashboard" onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <Grid container spacing={2}>{cards.map((card) => <Grid item xs={12} md={card.label.length > 12 ? 2.4 : 1.7} key={card.label}><Metric label={card.label} value={card.value} /></Grid>)}</Grid>
    <Grid container spacing={2}>
      <Grid item xs={12} md={7}>
        <Card><CardContent>
          <Stack direction={{ xs: 'column', sm: 'row' }} alignItems={{ xs: 'flex-start', sm: 'center' }} justifyContent="space-between" gap={1} sx={{ mb: 1 }}>
            <Box>
              <Typography variant="h6" fontWeight={900}>Notificaciones internas</Typography>
              <Typography variant="body2" color="text.secondary">Documentos, creditos, actividades y clientes que necesitan accion.</Typography>
            </Box>
            <Chip size="small" label={`${data?.alerts?.length ?? 0} pendientes`} color={data?.alerts?.some((alert) => alert.severity === 'error') ? 'error' : 'default'} variant="outlined" />
          </Stack>
          {data?.alerts?.length ? data.alerts.map((alert) => <Stack key={`${alert.type}${alert.title}${alert.createdAt}`} direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" gap={1.5} sx={{ py: 1.25, borderBottom: '1px solid #edf1f5' }}>
            <Stack spacing={.5} sx={{ minWidth: 0 }}>
              <Stack direction="row" gap={1} alignItems="center" flexWrap="wrap">
                <StatusChip label={alert.type} tone={alertSeverityTone(alert.severity)} />
                <Typography fontWeight={900}>{alert.title}</Typography>
              </Stack>
              <Typography color="text.secondary">{alert.description}</Typography>
              <Typography variant="caption" color="text.secondary">{new Date(alert.createdAt).toLocaleString()}</Typography>
            </Stack>
            {alert.actionUrl && <Button variant="outlined" sx={{ alignSelf: { xs: 'stretch', md: 'center' } }} onClick={() => navigate(alert.actionUrl!)}>Abrir</Button>}
          </Stack>) : <EmptyState text="Sin notificaciones internas" />}
        </CardContent></Card>
      </Grid>
      <Grid item xs={12} md={5}>
        <Card><CardContent><Typography variant="h6" fontWeight={900}>Actividad reciente</Typography>{data?.recentActivities?.length ? data.recentActivities.map((a) => <Row key={`${a.title}${a.scheduledAt}`} primary={a.title} secondary={new Date(a.scheduledAt).toLocaleString()} />) : <EmptyState text="Sin actividad reciente" />}</CardContent></Card>
      </Grid>
    </Grid>
  </Stack>;
}

function CustomersPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<Customer[]>('/api/customers', []);
  const [form, setForm] = useState<FormMode<Customer>>({ open: false });
  const [confirm, setConfirm] = useState<Customer>();
  const [notice, setNotice] = useState<Notice>();
  const canDelete = useCanManage();
  const navigate = useNavigate();

  const save = async (payload: typeof emptyCustomer) => {
    const body = {
      ...payload,
      firstNames: fullFirstNames(payload.firstName, payload.middleName, payload.firstNames),
      lastNames: fullLastNames(payload.lastName, payload.secondLastName, payload.lastNames),
      identificationType: payload.identificationType ? Number(payload.identificationType) : null,
      identificationNumber: payload.identificationNumber || null,
      companyName: payload.companyName || null,
      email: payload.email || null,
      phoneCountryCode: payload.phoneCountryCode || '+57',
      phone: payload.phone || null,
      address: payload.address || null,
      city: payload.city || null,
      birthDate: payload.birthDate ? new Date(payload.birthDate).toISOString() : null,
      occupation: payload.occupation || null,
      status: Number(payload.status),
      tags: payload.tags || null,
      notes: payload.notes || null
    };
    const { data } = form.item
      ? await api.put<Customer>(`/api/customers/${form.item.id}`, body)
      : await api.post<Customer>('/api/customers', body);
    setData(form.item ? rows.map((x) => x.id === data.id ? data : x) : [data, ...rows]);
    setNotice({ type: 'success', text: form.item ? 'Cliente actualizado.' : 'Cliente creado.' });
    setForm({ open: false });
  };

  const remove = async () => {
    if (!confirm) return;
    await api.delete(`/api/customers/${confirm.id}`);
    setData(rows.filter((x) => x.id !== confirm.id));
    setNotice({ type: 'success', text: 'Cliente eliminado.' });
    setConfirm(undefined);
  };

  return <Stack spacing={3}>
    <Header title="Clientes" action="Nuevo cliente" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Identificacion', 'Primer nombre', 'Segundo nombre', 'Primer apellido', 'Segundo apellido', 'Telefono', 'Ciudad', 'Estado', 'Etiquetas', 'Acciones']}
      empty="No hay clientes registrados"
      rows={rows.map((r) => [
        r.identificationNumber || '-',
        r.firstName || r.firstNames || r.name,
        r.middleName || '-',
        r.lastName || r.lastNames,
        r.secondLastName || '-',
        r.phone,
        r.city,
        <StatusChip label={statusLabel(r.status)} tone={r.status === 1 ? 'success' : 'default'} />,
        r.tags,
        <Actions onView={() => navigate(`/clientes/${r.id}`)} onEdit={() => setForm({ open: true, item: r })} onDelete={canDelete ? () => setConfirm(r) : undefined} />
      ])}
    />
    <CustomerDialog form={form} onClose={() => setForm({ open: false })} onSave={save} />
    <ConfirmDialog title="Eliminar cliente" text={`Se eliminara ${confirm?.name}.`} open={!!confirm} onClose={() => setConfirm(undefined)} onConfirm={remove} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function Customer360Page() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data, loading, error, reload } = useResource<Customer360>(`/api/customers/${id}/summary`);
  const customer = data?.customer;
  const [activityForm, setActivityForm] = useState<FormMode<Activity>>({ open: false });
  const [analysis, setAnalysis] = useState<CustomerAiAnalysis>();
  const [analyzing, setAnalyzing] = useState(false);
  const [notice, setNotice] = useState<Notice>();

  const saveActivity = async (payload: typeof emptyActivity) => {
    await api.post<Activity>('/api/activities', toActivityPayload(payload));
    setNotice({ type: 'success', text: 'Seguimiento registrado.' });
    setActivityForm({ open: false });
    reload();
  };

  const analyzeCustomer = async () => {
    if (!customer) return;
    setAnalyzing(true);
    try {
      const { data } = await api.get<CustomerAiAnalysis>(`/api/customers/${customer.id}/ai-analysis`);
      setAnalysis(data);
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    } finally {
      setAnalyzing(false);
    }
  };

  return <Stack spacing={3}>
    <Header
      title={customer ? `${customer.firstNames || customer.name} ${customer.lastNames}`.trim() : 'Cliente 360'}
      action={customer ? 'Nuevo seguimiento' : undefined}
      onAction={() => customer && setActivityForm({ open: true, item: { ...emptyActivity, title: `Seguimiento: ${customer.name}`, customerId: customer.id } as Activity })}
      onRefresh={reload}
      secondaryAction={{ label: 'Volver', onClick: () => navigate('/clientes') }}
    />
    <StatusBar loading={loading} error={error} />
    {customer && <Card><CardContent>
      <Stack direction={{ xs: 'column', md: 'row' }} alignItems={{ xs: 'stretch', md: 'center' }} justifyContent="space-between" gap={2}>
        <Box>
          <Typography variant="h6" fontWeight={900}>Asistente comercial del cliente</Typography>
          <Typography color="text.secondary">Resume el caso, detecta pendientes y sugiere la siguiente accion comercial.</Typography>
        </Box>
        <Button variant="contained" startIcon={<AutoAwesome />} onClick={analyzeCustomer} disabled={analyzing}>
          {analyzing ? 'Analizando...' : 'Analizar con IA'}
        </Button>
      </Stack>
    </CardContent></Card>}
    {customer && <Grid container spacing={2}>
      <Grid item xs={12} md={3}><Metric label="Identificacion" value={customer.identificationNumber || '-'} /></Grid>
      <Grid item xs={12} md={3}><Metric label="Telefono / WhatsApp" value={customer.phone ? <Button size="small" startIcon={<WhatsApp />} href={whatsappUrl(customer.phone)} target="_blank" rel="noreferrer">{customer.phone}</Button> : '-'} /></Grid>
      <Grid item xs={12} md={3}><Metric label="Email" value={customer.email || '-'} /></Grid>
      <Grid item xs={12} md={3}><Metric label="Ciudad" value={customer.city || '-'} /></Grid>
      <Grid item xs={12} md={3}><Metric label="Direccion" value={customer.address || '-'} /></Grid>
      <Grid item xs={12} md={3}><Metric label="Ocupacion" value={customer.occupation || '-'} /></Grid>
      <Grid item xs={12} md={3}><Metric label="Estado" value={statusLabel(customer.status)} /></Grid>
      <Grid item xs={12} md={3}><Metric label="Etiquetas" value={customer.tags || '-'} /></Grid>
    </Grid>}
    <Card><CardContent>
      <Stack direction="row" alignItems="center" justifyContent="space-between" gap={2} sx={{ mb: 2 }}>
        <Typography variant="h6" fontWeight={900}>Historial del cliente</Typography>
        <Chip size="small" label={`${data?.timeline.length ?? 0} eventos`} variant="outlined" />
      </Stack>
      <CustomerTimeline items={data?.timeline ?? []} />
    </CardContent></Card>
    <Grid container spacing={2}>
      <Grid item xs={12} md={6}>
        <Card><CardContent>
          <Typography variant="h6" fontWeight={900}>Cotizaciones</Typography>
          {data?.quotes.length ? data.quotes.map((q) => <Row key={q.id} primary={`${q.number} - ${q.productName}`} secondary={`Financiado ${money(q.financedAmount)} - cuota aprox. ${money(q.estimatedMonthlyPayment)} x ${q.termMonths} - ${new Date(q.quoteDate).toLocaleDateString()}`} />) : <EmptyState text="Sin cotizaciones" />}
        </CardContent></Card>
      </Grid>
      <Grid item xs={12} md={6}>
        <Card><CardContent>
          <Typography variant="h6" fontWeight={900}>Solicitudes de credito</Typography>
          {data?.creditApplications.length ? data.creditApplications.map((s) => <Row key={s.id} primary={`${s.number} - ${s.productName}`} secondary={`${creditStatus(s.status)} - ${money(s.motorcycleValue)}`} />) : <EmptyState text="Sin solicitudes" />}
        </CardContent></Card>
      </Grid>
      <Grid item xs={12} md={6}>
        <Card><CardContent>
          <Typography variant="h6" fontWeight={900}>Pipeline</Typography>
          {data?.deals.length ? data.deals.map((d) => <Row key={d.id} primary={d.title} secondary={`${dealStatus(d.status)} - ${money(d.value)} - ${d.closeProbability}%`} />) : <EmptyState text="Sin negocios" />}
        </CardContent></Card>
      </Grid>
      <Grid item xs={12} md={6}>
        <Card><CardContent>
          <Typography variant="h6" fontWeight={900}>Actividades</Typography>
          {data?.activities.length ? data.activities.map((a) => <Row key={a.id} primary={a.title} secondary={`${activityStatus(a.status)} - ${new Date(a.scheduledAt).toLocaleString()}`} />) : <EmptyState text="Sin actividades" />}
        </CardContent></Card>
      </Grid>
    </Grid>
    <ActivityDialog form={activityForm} customers={customer ? [customer] : []} deals={data?.deals ?? []} onClose={() => setActivityForm({ open: false })} onSave={saveActivity} />
    <AiAnalysisDialog analysis={analysis} onClose={() => setAnalysis(undefined)} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function CustomerTimeline({ items }: { items: CustomerTimelineItem[] }) {
  if (!items.length) return <EmptyState text="Sin historial registrado" />;
  return <Stack spacing={0}>
    {items.map((item, index) => <Stack key={`${item.type}-${item.relatedId ?? index}-${item.occurredAt}`} direction="row" gap={2} sx={{ position: 'relative', pb: 2 }}>
      <Box sx={{ width: 16, display: 'flex', justifyContent: 'center', position: 'relative' }}>
        <Box sx={{ width: 10, height: 10, borderRadius: '50%', bgcolor: timelineColor(item.tone), mt: .9, zIndex: 1 }} />
        {index < items.length - 1 && <Box sx={{ position: 'absolute', top: 20, bottom: 0, width: 2, bgcolor: '#e5eaf0' }} />}
      </Box>
      <Box sx={{ flex: 1, minWidth: 0, borderBottom: index < items.length - 1 ? '1px solid #edf1f5' : 'none', pb: 1.5 }}>
        <Stack direction="row" alignItems="center" gap={1} flexWrap="wrap">
          <Chip size="small" label={item.type} color={timelineChipColor(item.tone)} variant={item.tone === 'default' || item.tone === 'info' ? 'outlined' : 'filled'} />
          <Typography fontWeight={800}>{item.title}</Typography>
          <Typography variant="caption" color="text.secondary">{new Date(item.occurredAt).toLocaleString()}</Typography>
        </Stack>
        <Typography color="text.secondary" sx={{ mt: .5 }}>{item.description}</Typography>
      </Box>
    </Stack>)}
  </Stack>;
}

function ProductsPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<Product[]>('/api/products', []);
  const [form, setForm] = useState<FormMode<Product>>({ open: false });
  const [confirm, setConfirm] = useState<Product>();
  const [notice, setNotice] = useState<Notice>();
  const canManage = useCanManage();

  const save = async (payload: typeof emptyProduct) => {
    const body = {
      ...payload,
      description: payload.description || null,
      engineCc: payload.engineCc === '' ? null : Number(payload.engineCc),
      year: payload.year === '' ? null : Number(payload.year),
      price: Number(payload.price),
      active: Boolean(payload.active)
    };
    const { data } = form.item
      ? await api.put<Product>(`/api/products/${form.item.id}`, body)
      : await api.post<Product>('/api/products', body);
    setData(form.item ? rows.map((x) => x.id === data.id ? data : x) : [data, ...rows]);
    setNotice({ type: 'success', text: form.item ? 'Producto actualizado.' : 'Producto creado.' });
    setForm({ open: false });
  };

  const remove = async () => {
    if (!confirm) return;
    const { data } = await api.delete<Product>(`/api/products/${confirm.id}`);
    setData(rows.map((x) => x.id === data.id ? data : x));
    setNotice({ type: 'success', text: 'Producto inactivado.' });
    setConfirm(undefined);
  };

  return <Stack spacing={3}>
    <Header title="Productos" action={canManage ? 'Nuevo producto' : undefined} onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Producto', 'Fotos', 'Categoria', 'Marca', 'Referencia', 'Caracteristicas', 'Precio', 'Estado', 'Acciones']}
      empty="No hay productos registrados"
      rows={rows.map((r) => [
        <Stack direction="row" spacing={1.5} alignItems="center">
          <ProductPhotoThumb photo={(r.photos ?? []).find((photo) => photo.isQuoteDefault) ?? (r.photos ?? [])[0]} />
          <Box>
            <Typography fontWeight={800}>{productName(r)}</Typography>
            <Typography variant="caption" color="text.secondary">{r.description || 'Sin descripcion'}</Typography>
          </Box>
        </Stack>,
        <Stack spacing={.5}>
          <Chip size="small" label={`${r.photos?.length ?? 0} foto${(r.photos?.length ?? 0) === 1 ? '' : 's'}`} variant="outlined" />
          {(r.photos ?? []).some((photo) => photo.isQuoteDefault) && <Typography variant="caption" color="text.secondary">Principal PDF lista</Typography>}
        </Stack>,
        r.category,
        r.brand,
        r.reference,
        [r.model, r.engineCc ? `${r.engineCc} cc` : undefined, r.year, r.color].filter(Boolean).join(' / ') || r.description,
        money(r.price),
        <StatusChip label={r.active ? 'Activa' : 'Inactiva'} tone={r.active ? 'success' : 'default'} />,
        <Actions onEdit={canManage ? () => setForm({ open: true, item: r }) : undefined} onDelete={canManage && r.active ? () => setConfirm(r) : undefined} />
      ])}
    />
    <ProductDialog form={form} onClose={() => setForm({ open: false })} onSave={save} onChanged={reload} />
    <ConfirmDialog title="Inactivar producto" text={`Se inactivara ${confirm ? productName(confirm) : ''}. Las cotizaciones existentes conservaran el historial.`} open={!!confirm} onClose={() => setConfirm(undefined)} onConfirm={remove} confirmLabel="Inactivar" />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function QuotesPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<Quote[]>('/api/quotes', []);
  const { data: products = [] } = useResource<Product[]>('/api/products', []);
  const [form, setForm] = useState<FormMode<Quote>>({ open: false });
  const [analysis, setAnalysis] = useState<CustomerAiAnalysis>();
  const [previewQuote, setPreviewQuote] = useState<Quote>();
  const [notice, setNotice] = useState<Notice>();

  const downloadPdf = async (quote: Quote) => {
    const { data } = await api.get<Blob>(`/api/quotes/${quote.id}/pdf`, { responseType: 'blob' });
    const url = URL.createObjectURL(data);
    const link = document.createElement('a');
    link.href = url;
    link.download = `${quote.number}.pdf`;
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(url);
  };

  const save = async (payload: typeof emptyQuote) => {
    const body = {
      ...payload,
      customerFirstNames: fullFirstNames(payload.customerFirstName, payload.customerMiddleName, payload.customerFirstNames),
      customerLastNames: fullLastNames(payload.customerLastName, payload.customerSecondLastName, payload.customerLastNames),
      identificationType: Number(payload.identificationType),
      identificationNumber: payload.identificationNumber || null,
      phoneCountryCode: payload.phoneCountryCode || '+57',
      phoneNumber: payload.phoneNumber || null,
      downPayment: Number(payload.downPayment),
      insurance: Number(payload.insurance),
      administrativeFees: Number(payload.administrativeFees),
      termMonths: Number(payload.termMonths),
      monthlyInterestRate: Number(payload.monthlyInterestRate),
      notes: payload.notes || null
    };
    const { data } = await api.post<Quote>('/api/quotes', body);
    setData([data, ...rows]);
    setNotice({ type: 'success', text: 'Cotizacion creada. Revise la vista previa antes de descargar o imprimir.' });
    setForm({ open: false });
    setPreviewQuote(data);
  };

  const analyzeCustomer = async (customerId: string) => {
    try {
      const { data } = await api.get<CustomerAiAnalysis>(`/api/customers/${customerId}/ai-analysis`);
      setAnalysis(data);
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  return <Stack spacing={3}>
    <Header title="Cotizaciones" action="Nueva cotizacion" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Numero', 'Cliente', 'Identificacion', 'Producto', 'Total financiado', 'Cuota aprox.', 'Valida hasta', 'Acciones']}
      empty="No hay cotizaciones registradas"
      rows={rows.map((r) => [
        r.number,
        `${fullFirstNames(r.customerFirstName, r.customerMiddleName, r.customerFirstNames)} ${fullLastNames(r.customerLastName, r.customerSecondLastName, r.customerLastNames)}`.trim(),
        `${identificationLabel(r.identificationType)} ${r.identificationNumber ?? ''}`.trim(),
        r.productName,
        money(r.financedAmount),
        r.estimatedMonthlyPayment > 0 ? `${money(r.estimatedMonthlyPayment)} x ${r.termMonths}` : 'Sin simulacion',
        new Date(r.validUntil).toLocaleDateString(),
        <Actions onAi={() => analyzeCustomer(r.customerId)} onDownload={() => setPreviewQuote(r)} />
      ])}
    />
    <QuoteDialog form={form} products={products.filter((x) => x.active)} onClose={() => setForm({ open: false })} onSave={save} />
    <QuotePdfPreviewDialog quote={previewQuote} onClose={() => setPreviewQuote(undefined)} onDownload={downloadPdf} />
    <AiAnalysisDialog analysis={analysis} onClose={() => setAnalysis(undefined)} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function CreditApplicationsPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<CreditApplication[]>('/api/credit-applications', []);
  const { data: customers = [] } = useResource<Customer[]>('/api/customers', []);
  const { data: products = [] } = useResource<Product[]>('/api/products', []);
  const { data: quotes = [] } = useResource<Quote[]>('/api/quotes', []);
  const { data: deals = [] } = useResource<Deal[]>('/api/pipeline/deals', []);
  const [form, setForm] = useState<FormMode<CreditApplication>>({ open: false });
  const [analysis, setAnalysis] = useState<CustomerAiAnalysis>();
  const [notice, setNotice] = useState<Notice>();

  const save = async (payload: typeof emptyCreditApplication) => {
    const body = {
      ...payload,
      quoteId: payload.quoteId || null,
      dealId: payload.dealId || null,
      identificationType: Number(payload.identificationType),
      birthDate: payload.birthDate ? new Date(payload.birthDate).toISOString() : null,
      monthlyIncome: Number(payload.monthlyIncome),
      downPayment: Number(payload.downPayment),
      termMonths: Number(payload.termMonths),
      motorcycleValue: Number(payload.motorcycleValue),
      coDebtorName: payload.coDebtorName || null,
      coDebtorIdentification: payload.coDebtorIdentification || null,
      coDebtorMobile: payload.coDebtorMobile || null,
      coDebtorRelationship: payload.coDebtorRelationship || null,
      coDebtorMonthlyIncome: Number(payload.coDebtorMonthlyIncome) > 0 ? Number(payload.coDebtorMonthlyIncome) : null,
      reference1Name: payload.reference1Name || null,
      reference1Mobile: payload.reference1Mobile || null,
      reference1Relationship: payload.reference1Relationship || null,
      reference2Name: payload.reference2Name || null,
      reference2Mobile: payload.reference2Mobile || null,
      reference2Relationship: payload.reference2Relationship || null,
      status: Number(payload.status),
      notes: payload.notes || null
    };
    const { data } = form.item
      ? await api.put<CreditApplication>(`/api/credit-applications/${form.item.id}`, body)
      : await api.post<CreditApplication>('/api/credit-applications', body);
    setData(form.item ? rows.map((x) => x.id === data.id ? data : x) : [data, ...rows]);
    setNotice({ type: 'success', text: form.item ? 'Solicitud actualizada.' : 'Solicitud de credito creada.' });
    setForm({ open: false });
  };

  const changeStatus = async (application: CreditApplication, status: number) => {
    const { data } = await api.post<CreditApplication>(`/api/credit-applications/${application.id}/status`, { status });
    setData(rows.map((x) => x.id === data.id ? data : x));
    setNotice({ type: 'success', text: `Solicitud marcada como ${creditStatus(status)}.` });
  };

  const decide = async (application: CreditApplication, status: number, notes?: string) => {
    try {
      const { data } = await api.post<CreditApplication>(`/api/credit-applications/${application.id}/decision`, { status, notes: notes ?? null });
      setData(rows.map((x) => x.id === data.id ? data : x));
      setNotice({ type: 'success', text: `Solicitud marcada como ${creditStatus(status)}.` });
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  const updateDocument = async (application: CreditApplication, document: CreditDocument, status: number) => {
    const { data } = await api.put<CreditApplication>(`/api/credit-applications/${application.id}/documents/${document.id}`, {
      type: document.type,
      name: document.name,
      status,
      receivedAt: status === 2 || status === 3 ? new Date().toISOString() : document.receivedAt ?? null,
      notes: document.notes ?? null
    });
    setData(rows.map((x) => x.id === data.id ? data : x));
  };

  const uploadDocument = async (application: CreditApplication, document: CreditDocument, file: File) => {
    try {
      const formData = new FormData();
      formData.append('file', file);
      const { data } = await api.post<CreditApplication>(`/api/credit-applications/${application.id}/documents/${document.id}/file`, formData);
      setData(rows.map((x) => x.id === data.id ? data : x));
      setNotice({ type: 'success', text: `${document.name} cargado correctamente.` });
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  const downloadDocument = async (application: CreditApplication, document: CreditDocument) => {
    try {
      const response = await api.get<Blob>(`/api/credit-applications/${application.id}/documents/${document.id}/file`, { responseType: 'blob' });
      const url = URL.createObjectURL(response.data);
      const anchor = window.document.createElement('a');
      anchor.href = url;
      anchor.download = document.fileName || document.name;
      anchor.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  const downloadTemplate = async (application: CreditApplication, template: CreditTemplate) => {
    try {
      const response = await api.get<Blob>(`/api/credit-applications/${application.id}/pdf/${template.id}`, { responseType: 'blob' });
      const url = URL.createObjectURL(response.data);
      const anchor = window.document.createElement('a');
      anchor.href = url;
      anchor.download = `${application.number}-${template.id}.pdf`;
      anchor.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  const analyzeCustomer = async (customerId: string) => {
    try {
      const { data } = await api.get<CustomerAiAnalysis>(`/api/customers/${customerId}/ai-analysis`);
      setAnalysis(data);
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  return <Stack spacing={3}>
    <Header title="Solicitudes de credito" action="Nueva solicitud" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Numero', 'Cliente', 'Producto', 'Estado', 'Ingresos', 'Codeudor', 'Referencias', 'Documentos', 'Aprobacion', 'Plantillas', 'Acciones']}
      empty="No hay solicitudes de credito"
      rows={rows.map((r) => [
        r.number,
        r.customerName,
        r.productName,
        <StatusChip label={creditStatus(r.status)} tone={creditTone(r.status)} />,
        money(r.monthlyIncome),
        r.coDebtorName ? <Row primary={r.coDebtorName} secondary={r.coDebtorMobile ?? 'Sin celular'} /> : '-',
        [r.reference1Name, r.reference2Name].filter(Boolean).length ? <Stack spacing={.5}>
          {r.reference1Name && <Typography variant="body2">{r.reference1Name} - {r.reference1Mobile ?? 'Sin celular'}</Typography>}
          {r.reference2Name && <Typography variant="body2">{r.reference2Name} - {r.reference2Mobile ?? 'Sin celular'}</Typography>}
        </Stack> : '-',
        <DocumentSummary application={r} onUpdate={updateDocument} onUpload={uploadDocument} onDownload={downloadDocument} />,
        <ApprovalSummary application={r} onDecision={decide} />,
        <CreditTemplateDownloads application={r} onDownload={downloadTemplate} />,
        <Stack direction="row" gap={1} alignItems="center">
          <Actions onAi={() => analyzeCustomer(r.customerId)} onEdit={() => setForm({ open: true, item: r })} />
          <TextField select size="small" label="Estado" value={r.status} onChange={(e) => changeStatus(r, Number(e.target.value))} sx={{ minWidth: 160 }}>
            {creditStatusOptions.map((x) => <MenuItem key={x} value={x}>{creditStatus(x)}</MenuItem>)}
          </TextField>
        </Stack>
      ])}
    />
    <CreditApplicationDialog form={form} customers={customers} products={products.filter((x) => x.active)} quotes={quotes} deals={deals} onClose={() => setForm({ open: false })} onSave={save} />
    <AiAnalysisDialog analysis={analysis} onClose={() => setAnalysis(undefined)} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function ApprovalSummary({ application, onDecision }: { application: CreditApplication; onDecision: (application: CreditApplication, status: number, notes?: string) => Promise<void> }) {
  const actions = [
    { status: 8, label: 'Interesado', show: application.status === 1 },
    { status: 2, label: 'Documentos', show: application.status === 1 || application.status === 8 },
    { status: 4, label: 'Estudio', show: application.status === 2 || application.status === 3 },
    { status: 5, label: 'Aprobar', show: application.status === 4 },
    { status: 6, label: 'Rechazar', show: application.status === 4 || application.status === 5 },
    { status: 7, label: 'Entregar', show: application.status === 5 },
    { status: 9, label: 'Desistir', show: ![6, 7, 9].includes(application.status) }
  ].filter((x) => x.show);
  const lastDate = application.disbursedAt ?? application.approvedAt ?? application.rejectedAt ?? application.reviewStartedAt ?? application.submittedAt;
  return <Stack spacing={.75} sx={{ minWidth: 180, maxWidth: 210 }}>
    <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.25 }}>
      {lastDate ? `${application.decisionUser ?? 'Sistema'} - ${new Date(lastDate).toLocaleDateString()}` : 'Sin decision registrada'}
    </Typography>
    {application.decisionNotes && <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.25 }}>{application.decisionNotes}</Typography>}
    <Stack direction="row" gap={.5} flexWrap="wrap">
      {actions.length ? actions.map((action) => <Button key={action.status} size="small" variant="outlined" onClick={() => onDecision(application, action.status)}>{action.label}</Button>) : <Chip size="small" label="Sin acciones" variant="outlined" />}
    </Stack>
  </Stack>;
}

function MotorcycleDeliveriesPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<MotorcycleDelivery[]>('/api/motorcycle-deliveries', []);
  const { data: applications = [] } = useResource<CreditApplication[]>('/api/credit-applications', []);
  const [form, setForm] = useState<FormMode<MotorcycleDelivery>>({ open: false });
  const [notice, setNotice] = useState<Notice>();
  const eligibleApplications = applications.filter((x) => x.status === 5 || x.status === 7);

  const save = async (payload: typeof emptyMotorcycleDelivery) => {
    const body = {
      ...payload,
      deliveryDate: new Date(payload.deliveryDate).toISOString(),
      responsibleAdvisor: payload.responsibleAdvisor || null,
      vin: payload.vin || null,
      chassisNumber: payload.chassisNumber || null,
      engineNumber: payload.engineNumber || null,
      plate: payload.plate || null,
      deliveryMileage: payload.deliveryMileage === '' ? null : Number(payload.deliveryMileage),
      status: Number(payload.status),
      notes: payload.notes || null
    };
    const { data } = form.item
      ? await api.put<MotorcycleDelivery>(`/api/motorcycle-deliveries/${form.item.id}`, body)
      : await api.post<MotorcycleDelivery>('/api/motorcycle-deliveries', body);
    setData(form.item ? rows.map((x) => x.id === data.id ? data : x) : [data, ...rows]);
    setNotice({ type: 'success', text: form.item ? 'Entrega actualizada.' : 'Entrega registrada.' });
    setForm({ open: false });
  };

  return <Stack spacing={3}>
    <Header title="Entregas de motos" action="Nueva entrega" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Numero', 'Cliente', 'Producto', 'Estado', 'Fecha', 'Tecnicos', 'Documentos', 'Acciones']}
      empty="No hay entregas registradas"
      rows={rows.map((r) => [
        r.number,
        <Row primary={r.customerName} secondary={r.creditApplicationNumber} />,
        r.productName,
        <StatusChip label={deliveryStatus(r.status)} tone={r.status === 2 ? 'success' : r.status === 3 ? 'error' : 'warning'} />,
        <Row primary={new Date(r.deliveryDate).toLocaleString()} secondary={r.responsibleAdvisor ?? 'Sin asesor'} />,
        <Stack spacing={.5}>
          <Typography variant="body2">Chasis: {r.chassisNumber || '-'}</Typography>
          <Typography variant="body2">Motor: {r.engineNumber || '-'}</Typography>
          <Typography variant="body2">Placa: {r.plate || '-'}</Typography>
        </Stack>,
        <DeliveryChecklist delivery={r} />,
        <Actions onEdit={() => setForm({ open: true, item: r })} />
      ])}
    />
    <MotorcycleDeliveryDialog form={form} applications={eligibleApplications} onClose={() => setForm({ open: false })} onSave={save} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function DeliveryChecklist({ delivery }: { delivery: MotorcycleDelivery }) {
  const items = [
    ['Casco', delivery.helmetDelivered],
    ['SOAT', delivery.soatDelivered],
    ['Matricula', delivery.registrationDelivered],
    ['Garantia', delivery.warrantyManualDelivered],
    ['Acta', delivery.deliveryCertificateSigned]
  ];
  return <Stack direction="row" gap={.5} flexWrap="wrap">
    {items.map(([label, ok]) => <Chip key={String(label)} size="small" label={label} color={ok ? 'success' : undefined} variant={ok ? 'filled' : 'outlined'} />)}
  </Stack>;
}

function MotorcycleDeliveryDialog({ form, applications, onClose, onSave }: DialogProps<MotorcycleDelivery, typeof emptyMotorcycleDelivery> & { applications: CreditApplication[] }) {
  const initial = form.item ? {
    creditApplicationId: form.item.creditApplicationId,
    deliveryDate: toInputDateTime(form.item.deliveryDate),
    responsibleAdvisor: form.item.responsibleAdvisor ?? '',
    vin: form.item.vin ?? '',
    chassisNumber: form.item.chassisNumber ?? '',
    engineNumber: form.item.engineNumber ?? '',
    plate: form.item.plate ?? '',
    deliveryMileage: form.item.deliveryMileage?.toString() ?? '',
    helmetDelivered: form.item.helmetDelivered,
    soatDelivered: form.item.soatDelivered,
    registrationDelivered: form.item.registrationDelivered,
    warrantyManualDelivered: form.item.warrantyManualDelivered,
    deliveryCertificateSigned: form.item.deliveryCertificateSigned,
    status: form.item.status,
    notes: form.item.notes ?? ''
  } : { ...emptyMotorcycleDelivery, creditApplicationId: applications[0]?.id ?? '' };

  return <FormDialog open={form.open} title={form.item ? 'Editar entrega' : 'Nueva entrega'} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <TextField select label="Solicitud aprobada" value={v.creditApplicationId} onChange={(e) => set({ creditApplicationId: e.target.value })} disabled={!!form.item} fullWidth>
        {applications.map((x) => <MenuItem key={x.id} value={x.id}>{x.number} - {x.customerName} - {x.productName}</MenuItem>)}
      </TextField>
      <Grid container spacing={2}>
        <Grid item xs={12} md={6}><TextField type="datetime-local" label="Fecha entrega" value={v.deliveryDate} onChange={(e) => set({ deliveryDate: e.target.value })} fullWidth InputLabelProps={{ shrink: true }} /></Grid>
        <Grid item xs={12} md={6}><TextField label="Asesor responsable" value={v.responsibleAdvisor} onChange={(e) => set({ responsibleAdvisor: e.target.value })} fullWidth /></Grid>
        <Grid item xs={12} md={6}><TextField label="VIN" value={v.vin} onChange={(e) => set({ vin: e.target.value })} fullWidth /></Grid>
        <Grid item xs={12} md={6}><TextField label="Numero chasis" value={v.chassisNumber} onChange={(e) => set({ chassisNumber: e.target.value })} fullWidth /></Grid>
        <Grid item xs={12} md={6}><TextField label="Numero motor" value={v.engineNumber} onChange={(e) => set({ engineNumber: e.target.value })} fullWidth /></Grid>
        <Grid item xs={12} md={3}><TextField label="Placa" value={v.plate} onChange={(e) => set({ plate: e.target.value })} fullWidth /></Grid>
        <Grid item xs={12} md={3}><TextField type="number" label="Kilometraje" value={v.deliveryMileage} onChange={(e) => set({ deliveryMileage: e.target.value })} fullWidth /></Grid>
      </Grid>
      <TextField select label="Estado" value={v.status} onChange={(e) => set({ status: Number(e.target.value) })} fullWidth>
        {[1, 2, 3].map((x) => <MenuItem key={x} value={x}>{deliveryStatus(x)}</MenuItem>)}
      </TextField>
      <Grid container spacing={1}>
        <Grid item xs={12} sm={6}><FormControlLabel control={<Checkbox checked={v.helmetDelivered} onChange={(e) => set({ helmetDelivered: e.target.checked })} />} label="Casco entregado" /></Grid>
        <Grid item xs={12} sm={6}><FormControlLabel control={<Checkbox checked={v.soatDelivered} onChange={(e) => set({ soatDelivered: e.target.checked })} />} label="SOAT entregado" /></Grid>
        <Grid item xs={12} sm={6}><FormControlLabel control={<Checkbox checked={v.registrationDelivered} onChange={(e) => set({ registrationDelivered: e.target.checked })} />} label="Matricula entregada" /></Grid>
        <Grid item xs={12} sm={6}><FormControlLabel control={<Checkbox checked={v.warrantyManualDelivered} onChange={(e) => set({ warrantyManualDelivered: e.target.checked })} />} label="Manual/garantia" /></Grid>
        <Grid item xs={12}><FormControlLabel control={<Checkbox checked={v.deliveryCertificateSigned} onChange={(e) => set({ deliveryCertificateSigned: e.target.checked })} />} label="Acta de entrega firmada" /></Grid>
      </Grid>
      <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} fullWidth multiline minRows={2} />
    </>}
  </FormDialog>;
}

type CreditTemplate = { id: 'solicitud-credito' | 'autorizacion-datos' | 'carta-aprobacion' | 'orden-entrega'; label: string; disabled?: (application: CreditApplication) => boolean; reason?: string };

const creditTemplates: CreditTemplate[] = [
  { id: 'solicitud-credito', label: 'Solicitud' },
  { id: 'autorizacion-datos', label: 'Datos' },
  { id: 'carta-aprobacion', label: 'Aprobacion', disabled: (x) => x.status !== 5 && x.status !== 7, reason: 'Disponible cuando la solicitud este aprobada.' },
  { id: 'orden-entrega', label: 'Entrega', disabled: (x) => x.status !== 5 && x.status !== 7, reason: 'Disponible cuando la solicitud este aprobada.' }
];

function CreditTemplateDownloads({ application, onDownload }: { application: CreditApplication; onDownload: (application: CreditApplication, template: CreditTemplate) => Promise<void> }) {
  return <Stack direction="row" gap={.5} flexWrap="wrap" sx={{ minWidth: 142, maxWidth: 150 }}>
    {creditTemplates.map((template) => {
      const disabled = template.disabled?.(application) ?? false;
      return <Tooltip key={template.id} title={disabled ? template.reason ?? '' : `Descargar ${template.label}`}>
        <span>
          <IconButton
            size="small"
            color="primary"
            disabled={disabled}
            onClick={() => onDownload(application, template)}
            aria-label={`Descargar ${template.label}`}
          >
            <Download fontSize="small" />
          </IconButton>
        </span>
      </Tooltip>;
    })}
  </Stack>;
}

function LeadsPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<Lead[]>('/api/leads', []);
  const [form, setForm] = useState<FormMode<Lead>>({ open: false });
  const [confirm, setConfirm] = useState<Lead>();
  const [notice, setNotice] = useState<Notice>();
  const canDelete = useCanManage();

  const save = async (payload: typeof emptyLead) => {
    const body = {
      ...payload,
      firstNames: fullFirstNames(payload.firstName, payload.middleName, payload.firstNames),
      lastNames: fullLastNames(payload.lastName, payload.secondLastName, payload.lastNames),
      rating: Number(payload.rating)
    };
    const { data } = form.item
      ? await api.put<Lead>(`/api/leads/${form.item.id}`, body)
      : await api.post<Lead>('/api/leads', body);
    setData(form.item ? rows.map((x) => x.id === data.id ? data : x) : [data, ...rows]);
    setNotice({ type: 'success', text: form.item ? 'Prospecto actualizado.' : 'Prospecto creado.' });
    setForm({ open: false });
  };

  const convert = async (lead: Lead) => {
    const { data } = await api.post<Lead>(`/api/leads/${lead.id}/convert`);
    setData(rows.map((x) => x.id === data.id ? data : x));
    setNotice({ type: 'success', text: 'Prospecto convertido a cliente.' });
  };

  const remove = async () => {
    if (!confirm) return;
    await api.delete(`/api/leads/${confirm.id}`);
    setData(rows.filter((x) => x.id !== confirm.id));
    setNotice({ type: 'success', text: 'Prospecto eliminado.' });
    setConfirm(undefined);
  };

  return <Stack spacing={3}>
    <Header title="Prospectos" action="Nuevo prospecto" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Primer nombre', 'Segundo nombre', 'Primer apellido', 'Segundo apellido', 'Email', 'Telefono', 'Fuente', 'Calificacion', 'Estado', 'Acciones']}
      empty="No hay prospectos registrados"
      rows={rows.map((r) => [
        r.firstName || r.firstNames || r.name,
        r.middleName || '-',
        r.lastName || r.lastNames,
        r.secondLastName || '-',
        r.email,
        r.phone,
        r.source,
        <StatusChip label={ratingLabel(r.rating)} tone={r.rating === 3 ? 'warning' : 'default'} />,
        r.converted ? <StatusChip label="Convertido" tone="success" /> : 'Abierto',
        <Actions onEdit={() => setForm({ open: true, item: r })} onConvert={!r.converted ? () => convert(r) : undefined} onDelete={canDelete ? () => setConfirm(r) : undefined} />
      ])}
    />
    <LeadDialog form={form} onClose={() => setForm({ open: false })} onSave={save} />
    <ConfirmDialog title="Eliminar prospecto" text={`Se eliminara ${confirm?.name}.`} open={!!confirm} onClose={() => setConfirm(undefined)} onConfirm={remove} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function PipelinePage() {
  const { data: stages = [], loading: loadingStages, error: stagesError, reload: reloadStages, setData: setStages } = useResource<DealStage[]>('/api/pipeline/stages', []);
  const { data: deals = [], loading: loadingDeals, error: dealsError, reload: reloadDeals, setData: setDeals } = useResource<Deal[]>('/api/pipeline/deals', []);
  const { data: customers = [] } = useResource<Customer[]>('/api/customers', []);
  const [form, setForm] = useState<FormMode<Deal>>({ open: false });
  const [stageForm, setStageForm] = useState<FormMode<DealStage>>({ open: false });
  const [activityForm, setActivityForm] = useState<FormMode<Activity>>({ open: false });
  const [confirm, setConfirm] = useState<Deal>();
  const [notice, setNotice] = useState<Notice>();
  const [draggingDealId, setDraggingDealId] = useState<string>();
  const [dragOverStageId, setDragOverStageId] = useState<string>();
  const canManage = useCanManage();
  const navigate = useNavigate();

  const saveDeal = async (payload: typeof emptyDeal) => {
    const body = {
      ...payload,
      customerId: payload.customerId || null,
      stageId: payload.stageId,
      value: Number(payload.value),
      closeProbability: Number(payload.closeProbability),
      status: Number(payload.status),
      estimatedCloseDate: new Date(payload.estimatedCloseDate).toISOString()
    };
    const { data } = form.item
      ? await api.put<Deal>(`/api/pipeline/deals/${form.item.id}`, body)
      : await api.post<Deal>('/api/pipeline/deals', body);
    setDeals(form.item ? deals.map((x) => x.id === data.id ? data : x) : [data, ...deals]);
    setNotice({ type: 'success', text: form.item ? 'Negocio actualizado.' : 'Negocio creado.' });
    setForm({ open: false });
  };

  const saveStage = async (payload: { name: string; order: number; defaultProbability: number; active: boolean }) => {
    const body = { ...payload, order: Number(payload.order), defaultProbability: Number(payload.defaultProbability), active: Boolean(payload.active) };
    const { data } = stageForm.item
      ? await api.put<DealStage>(`/api/pipeline/stages/${stageForm.item.id}`, body)
      : await api.post<DealStage>('/api/pipeline/stages', body);
    setStages(stageForm.item ? stages.map((x) => x.id === data.id ? data : x) : [...stages, data].sort((a, b) => a.order - b.order));
    setNotice({ type: 'success', text: stageForm.item ? 'Etapa actualizada.' : 'Etapa creada.' });
    setStageForm({ open: false });
  };

  const remove = async () => {
    if (!confirm) return;
    await api.delete(`/api/pipeline/deals/${confirm.id}`);
    setDeals(deals.filter((x) => x.id !== confirm.id));
    setNotice({ type: 'success', text: 'Negocio eliminado.' });
    setConfirm(undefined);
  };

  const saveActivity = async (payload: typeof emptyActivity) => {
    await api.post<Activity>('/api/activities', toActivityPayload(payload));
    setNotice({ type: 'success', text: 'Actividad registrada.' });
    setActivityForm({ open: false });
  };

  const visibleStages = stages.filter((stage) => stage.active);
  const defaultStageId = visibleStages[0]?.id ?? '';
  const moveDeal = async (deal: Deal, stage: DealStage) => {
    if (deal.stageId === stage.id) return;
    const previousDeals = deals;
    const movedDeal: Deal = {
      ...deal,
      stageId: stage.id,
      closeProbability: Number(stage.defaultProbability),
      status: dealStatusForStage(stage.name)
    };
    setDeals(deals.map((x) => x.id === deal.id ? movedDeal : x));
    setNotice({ type: 'info', text: `Moviendo a ${stage.name}...` });
    try {
      const { data } = await api.put<Deal>(`/api/pipeline/deals/${deal.id}`, {
        title: movedDeal.title,
        customerId: movedDeal.customerId || null,
        stageId: movedDeal.stageId,
        value: Number(movedDeal.value),
        closeProbability: Number(movedDeal.closeProbability),
        status: Number(movedDeal.status),
        estimatedCloseDate: new Date(movedDeal.estimatedCloseDate).toISOString()
      });
      setDeals((current) => (current ?? []).map((x) => x.id === data.id ? data : x));
      setNotice({ type: 'success', text: `Negocio movido a ${stage.name}.` });
    } catch (err) {
      setDeals(previousDeals);
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  return <Stack spacing={3}>
    <Header title="Pipeline de ventas a credito" action="Nueva venta" onAction={() => setForm({ open: true })} onRefresh={() => { reloadStages(); reloadDeals(); }} secondaryAction={canManage ? { label: 'Nueva etapa', onClick: () => setStageForm({ open: true }) } : undefined} />
    <StatusBar loading={loadingStages || loadingDeals} error={stagesError || dealsError} />
    <Box className="kanban">{visibleStages.map((stage) => <Paper
      className={`kanbanColumn${dragOverStageId === stage.id ? ' kanbanColumnOver' : ''}`}
      key={stage.id}
      onDragOver={(event) => {
        event.preventDefault();
        event.dataTransfer.dropEffect = 'move';
        setDragOverStageId(stage.id);
      }}
      onDragLeave={() => setDragOverStageId((current) => current === stage.id ? undefined : current)}
      onDrop={(event) => {
        event.preventDefault();
        const dealId = event.dataTransfer.getData('text/plain') || draggingDealId;
        const deal = deals.find((x) => x.id === dealId);
        setDraggingDealId(undefined);
        setDragOverStageId(undefined);
        if (deal) void moveDeal(deal, stage);
      }}
    >
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Stack>
          <Typography fontWeight={800}>{stage.name}</Typography>
          <Typography color="text.secondary" fontSize={12}>{stage.defaultProbability}% pred.</Typography>
        </Stack>
        {canManage && <Tooltip title="Editar etapa"><IconButton size="small" onClick={() => setStageForm({ open: true, item: stage })}><Edit fontSize="small" /></IconButton></Tooltip>}
      </Stack>
      {deals.filter((d) => d.stageId === stage.id).map((deal) => {
        const dealCustomer = customers.find((x) => x.id === deal.customerId);
        const dealCustomerPhone = dealCustomer?.phone;
        return <Card
          key={deal.id}
          className={`kanbanCard${draggingDealId === deal.id ? ' kanbanCardDragging' : ''}`}
          draggable
          onDragStart={(event) => {
            setDraggingDealId(deal.id);
            event.dataTransfer.effectAllowed = 'move';
            event.dataTransfer.setData('text/plain', deal.id);
          }}
          onDragEnd={() => {
            setDraggingDealId(undefined);
            setDragOverStageId(undefined);
          }}
          sx={{ mt: 1 }}
        >
        <CardContent>
          <Stack direction="row" justifyContent="space-between" gap={1}>
            <Typography fontWeight={800}>{deal.title}</Typography>
            <StatusChip label={dealStatus(deal.status)} tone={deal.status === 2 ? 'success' : deal.status === 3 ? 'error' : 'default'} />
          </Stack>
          <Typography color="text.secondary">{money(deal.value)}</Typography>
          <LinearProgress variant="determinate" value={deal.closeProbability} sx={{ mt: 1 }} />
          <Actions
            onView={deal.customerId ? () => navigate(`/clientes/${deal.customerId}`) : undefined}
            onWhatsapp={dealCustomerPhone ? () => window.open(whatsappUrl(dealCustomerPhone), '_blank', 'noopener,noreferrer') : undefined}
            onActivity={() => setActivityForm({ open: true, item: { ...emptyActivity, title: `Seguimiento: ${deal.title}`, customerId: deal.customerId ?? '', dealId: deal.id } as Activity })}
            onEdit={() => setForm({ open: true, item: deal })}
            onDelete={canManage ? () => setConfirm(deal) : undefined}
            compact
          />
        </CardContent>
      </Card>;
      })}
    </Paper>)}</Box>
    <DealDialog form={form} stages={visibleStages} customers={customers} defaultStageId={defaultStageId} onClose={() => setForm({ open: false })} onSave={saveDeal} />
    <StageDialog form={stageForm} onClose={() => setStageForm({ open: false })} onSave={saveStage} />
    <ActivityDialog form={activityForm} customers={customers} deals={deals} onClose={() => setActivityForm({ open: false })} onSave={saveActivity} />
    <ConfirmDialog title="Eliminar negocio" text={`Se eliminara ${confirm?.title}.`} open={!!confirm} onClose={() => setConfirm(undefined)} onConfirm={remove} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function ActivitiesPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<Activity[]>('/api/activities', []);
  const { data: customers = [] } = useResource<Customer[]>('/api/customers', []);
  const { data: deals = [] } = useResource<Deal[]>('/api/pipeline/deals', []);
  const [form, setForm] = useState<FormMode<Activity>>({ open: false });
  const [confirm, setConfirm] = useState<Activity>();
  const [notice, setNotice] = useState<Notice>();
  const [statusFilter, setStatusFilter] = useState<'open' | 'all' | 'done'>('open');
  const [dueFilter, setDueFilter] = useState<'all' | 'overdue' | 'today' | 'upcoming'>('all');
  const canDelete = useCanManage();
  const overdueCount = rows.filter((x) => activityDueState(x) === 'overdue').length;
  const todayCount = rows.filter((x) => activityDueState(x) === 'today').length;
  const upcomingCount = rows.filter((x) => activityDueState(x) === 'upcoming').length;
  const completedCount = rows.filter((x) => x.status === 3).length;
  const visibleRows = rows.filter((row) => {
    const statusMatch = statusFilter === 'all' || (statusFilter === 'open' ? row.status === 1 || row.status === 2 : row.status === 3 || row.status === 4);
    const dueMatch = dueFilter === 'all' || activityDueState(row) === dueFilter;
    return statusMatch && dueMatch;
  });

  const save = async (payload: typeof emptyActivity) => {
    const { data } = form.item
      ? await api.put<Activity>(`/api/activities/${form.item.id}`, toActivityPayload(payload))
      : await api.post<Activity>('/api/activities', toActivityPayload(payload));
    setData(form.item ? rows.map((x) => x.id === data.id ? data : x) : [data, ...rows]);
    setNotice({ type: 'success', text: form.item ? 'Actividad actualizada.' : 'Actividad creada.' });
    setForm({ open: false });
  };

  const updateActivity = async (activity: Activity, patch: Partial<Activity>, message: string) => {
    const { data } = await api.put<Activity>(`/api/activities/${activity.id}`, toActivityPayload({ ...activity, ...patch }));
    setData(rows.map((x) => x.id === data.id ? data : x));
    setNotice({ type: 'success', text: message });
  };

  const remove = async () => {
    if (!confirm) return;
    await api.delete(`/api/activities/${confirm.id}`);
    setData(rows.filter((x) => x.id !== confirm.id));
    setNotice({ type: 'success', text: 'Actividad eliminada.' });
    setConfirm(undefined);
  };

  return <Stack spacing={3}>
    <Header title="Actividades" action="Nueva actividad" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <Grid container spacing={2}>
      <Grid item xs={12} md={3}><Metric label="Vencidas" value={overdueCount} /></Grid>
      <Grid item xs={12} md={3}><Metric label="Para hoy" value={todayCount} /></Grid>
      <Grid item xs={12} md={3}><Metric label="Proximas" value={upcomingCount} /></Grid>
      <Grid item xs={12} md={3}><Metric label="Completadas" value={completedCount} /></Grid>
    </Grid>
    <Card><CardContent>
      <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" gap={2}>
        <Stack direction="row" gap={1} flexWrap="wrap">
          {[
            ['open', 'Abiertas'],
            ['all', 'Todas'],
            ['done', 'Cerradas']
          ].map(([value, label]) => <Button key={value} variant={statusFilter === value ? 'contained' : 'outlined'} onClick={() => setStatusFilter(value as typeof statusFilter)}>{label}</Button>)}
        </Stack>
        <Stack direction="row" gap={1} flexWrap="wrap">
          {[
            ['all', 'Todas las fechas'],
            ['overdue', 'Vencidas'],
            ['today', 'Hoy'],
            ['upcoming', 'Proximas']
          ].map(([value, label]) => <Button key={value} variant={dueFilter === value ? 'contained' : 'outlined'} onClick={() => setDueFilter(value as typeof dueFilter)}>{label}</Button>)}
        </Stack>
      </Stack>
    </CardContent></Card>
    <EntityTable
      headers={['Seguimiento', 'Cliente', 'Negocio', 'Estado', 'Vence', 'Acciones']}
      empty="No hay actividades registradas"
      rows={visibleRows.map((r) => [
        <Stack><Typography fontWeight={800}>{r.title}</Typography><Typography color="text.secondary" fontSize={13}>{typeLabel(r.type)}{r.description ? ` - ${r.description}` : ''}</Typography></Stack>,
        r.customerName || customers.find((x) => x.id === r.customerId)?.name,
        r.dealTitle || deals.find((x) => x.id === r.dealId)?.title,
        <StatusChip label={activityStatus(r.status)} tone={activityTone(r)} />,
        <Stack><Typography>{new Date(r.scheduledAt).toLocaleString()}</Typography><Typography color={activityDueState(r) === 'overdue' ? 'error.main' : 'text.secondary'} fontSize={13}>{activityDueLabel(r)}</Typography></Stack>,
        <Actions
          onStart={r.status === 1 ? () => updateActivity(r, { status: 2 }, 'Actividad marcada en proceso.') : undefined}
          onComplete={r.status !== 3 ? () => updateActivity(r, { status: 3 }, 'Actividad completada.') : undefined}
          onReschedule={r.status === 1 || r.status === 2 ? () => updateActivity(r, { scheduledAt: addDaysIso(r.scheduledAt, 1), reminderAt: r.reminderAt ? addDaysIso(r.reminderAt, 1) : undefined }, 'Actividad reprogramada para manana.') : undefined}
          onCancel={r.status !== 4 ? () => updateActivity(r, { status: 4 }, 'Actividad cancelada.') : undefined}
          onEdit={() => setForm({ open: true, item: r })}
          onDelete={canDelete ? () => setConfirm(r) : undefined}
        />
      ])}
    />
    <ActivityDialog form={form} customers={customers} deals={deals} onClose={() => setForm({ open: false })} onSave={save} />
    <ConfirmDialog title="Eliminar actividad" text={`Se eliminara ${confirm?.title}.`} open={!!confirm} onClose={() => setConfirm(undefined)} onConfirm={remove} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function CommercialReportsPage() {
  const [from, setFrom] = useState(currentMonthStart);
  const [to, setTo] = useState(today);
  const { data, loading, error, reload } = useResource<CommercialReports>(`/api/commercial-reports?from=${from}&to=${to}`);
  const topQuoteCount = Math.max(1, ...(data?.topQuotedProducts.map((x) => x.quoteCount) ?? [1]));
  const cards: { label: string; value: ReactNode }[] = [
    { label: 'Cotizaciones', value: data?.summary.totalQuotes ?? 0 },
    { label: 'Convertidas a credito', value: data?.summary.quotesConvertedToCredit ?? 0 },
    { label: 'Conversion cotizacion', value: percent(data?.summary.quoteToCreditConversionRate) },
    { label: 'Creditos aprobados', value: data?.summary.approvedCredits ?? 0 },
    { label: 'Creditos rechazados', value: data?.summary.rejectedCredits ?? 0 },
    { label: 'Tasa aprobacion', value: percent(data?.summary.creditApprovalRate) },
    { label: 'Valor aprobado', value: money(data?.summary.approvedCreditAmount) }
  ];

  return <Stack spacing={3}>
    <Header title="Reportes comerciales" onRefresh={reload} />
    <Card><CardContent>
      <Stack direction={{ xs: 'column', md: 'row' }} gap={2} alignItems={{ xs: 'stretch', md: 'center' }}>
        <TextField label="Desde" type="date" value={from} onChange={(e) => setFrom(e.target.value)} InputLabelProps={{ shrink: true }} />
        <TextField label="Hasta" type="date" value={to} onChange={(e) => setTo(e.target.value)} InputLabelProps={{ shrink: true }} />
        <Button variant="outlined" onClick={reload}>Aplicar</Button>
      </Stack>
    </CardContent></Card>
    <StatusBar loading={loading} error={error} />
    <Grid container spacing={2}>{cards.map((card) => <Grid item xs={12} sm={6} md={card.label === 'Valor aprobado' ? 3 : 1.8} key={card.label}><Metric label={card.label} value={card.value} /></Grid>)}</Grid>
    <Grid container spacing={2}>
      <Grid item xs={12} lg={6}>
        <Card><CardContent>
          <Typography variant="h6" fontWeight={900} sx={{ mb: 1 }}>Ventas por vendedor</Typography>
          <ReportTable
            headers={['Vendedor', 'Cotizaciones', 'Aprobados', 'Valor']}
            empty="Sin ventas aprobadas en el periodo"
            rows={(data?.salesBySeller ?? []).map((row) => [
              row.seller,
              row.quotes,
              row.approvedCredits,
              money(row.approvedAmount)
            ])}
          />
        </CardContent></Card>
      </Grid>
      <Grid item xs={12} lg={6}>
        <Card><CardContent>
          <Typography variant="h6" fontWeight={900} sx={{ mb: 1 }}>Cotizaciones por estado</Typography>
          <ReportTable
            headers={['Estado', 'Cantidad', 'Valor']}
            empty="Sin cotizaciones en el periodo"
            rows={(data?.quotesByStatus ?? []).map((row) => [
              <StatusChip label={row.status} tone={row.status.includes('Vencida') ? 'warning' : row.status.includes('Convertida') ? 'success' : 'default'} />,
              row.count,
              money(row.amount)
            ])}
          />
        </CardContent></Card>
      </Grid>
      <Grid item xs={12} lg={6}>
        <Card><CardContent>
          <Typography variant="h6" fontWeight={900} sx={{ mb: 1 }}>Creditos aprobados/rechazados</Typography>
          <ReportTable
            headers={['Estado', 'Cantidad', 'Valor']}
            empty="Sin solicitudes de credito en el periodo"
            rows={(data?.creditsByStatus ?? []).map((row) => [
              <StatusChip label={row.status} tone={row.status === 'Aprobado' || row.status === 'Entregado' ? 'success' : row.status === 'Rechazado' || row.status === 'Desistido' ? 'error' : row.status === 'Credito en estudio' || row.status === 'Documentos pendientes' ? 'warning' : 'default'} />,
              row.count,
              money(row.amount)
            ])}
          />
        </CardContent></Card>
      </Grid>
      <Grid item xs={12} lg={6}>
        <Card><CardContent>
          <Typography variant="h6" fontWeight={900} sx={{ mb: 1 }}>Motos mas cotizadas</Typography>
          {(data?.topQuotedProducts?.length ?? 0) ? <Stack spacing={1.25}>
            {data!.topQuotedProducts.map((product) => <Box key={product.productId}>
              <Stack direction="row" justifyContent="space-between" gap={1}>
                <Box sx={{ minWidth: 0 }}>
                  <Typography fontWeight={900} noWrap>{product.productName}</Typography>
                  <Typography color="text.secondary" fontSize={13} noWrap>{product.brand} {product.model} - {money(product.quotedAmount)}</Typography>
                </Box>
                <Chip size="small" label={`${product.quoteCount} cot.`} />
              </Stack>
              <LinearProgress variant="determinate" value={Math.min(100, product.quoteCount / topQuoteCount * 100)} sx={{ mt: .75 }} />
            </Box>)}
          </Stack> : <EmptyState text="Sin productos cotizados en el periodo" />}
        </CardContent></Card>
      </Grid>
    </Grid>
  </Stack>;
}

function SettingsPage() {
  const user = useAuthStore((s) => s.user);
  const canManage = useCanManage();
  const { data: companies = [], loading: loadingCompanies, error: companiesError, reload: reloadCompanies, setData: setCompanies } = useResource<Company[]>('/api/companies', []);
  const { data: users = [], loading: loadingUsers, error: usersError, reload: reloadUsers, setData: setUsers } = useResource<User[]>('/api/users', []);
  const { data: financialSettings, loading: loadingFinancialSettings, error: financialSettingsError, reload: reloadFinancialSettings, setData: setFinancialSettings } = useResource<FinancialSettings>('/api/financial-settings');
  const [companyForm, setCompanyForm] = useState<FormMode<Company>>({ open: false });
  const [userForm, setUserForm] = useState<FormMode<User>>({ open: false });
  const [financialForm, setFinancialForm] = useState<FormMode<FinancialSettings>>({ open: false });
  const [notice, setNotice] = useState<Notice>();

  const saveCompany = async (payload: typeof emptyCompany) => {
  const body = {
      name: payload.name,
      subdomain: payload.subdomain,
      customDomain: payload.customDomain || null,
      logoDataUrl: payload.logoDataUrl || null,
      active: Boolean(payload.active)
    };
    const { data } = companyForm.item
      ? await api.put<Company>(`/api/companies/${companyForm.item.id}`, body)
      : await api.post<Company>('/api/companies', body);
    setCompanies(companyForm.item ? companies.map((x) => x.id === data.id ? data : x) : [...companies, data].sort((a, b) => a.name.localeCompare(b.name)));
    setNotice({ type: 'success', text: companyForm.item ? 'Empresa actualizada.' : 'Empresa creada con roles y etapas iniciales.' });
    setCompanyForm({ open: false });
  };

  const saveUser = async (payload: typeof emptyUser) => {
    const { data } = await api.post<User>('/api/users', payload);
    setUsers([...users, data].sort((a, b) => a.fullName.localeCompare(b.fullName)));
    setNotice({ type: 'success', text: 'Usuario creado.' });
    setUserForm({ open: false });
  };

  const saveFinancialSettings = async (payload: typeof emptyFinancialSettings) => {
    const body = {
      minimumWage: Number(payload.minimumWage),
      consumerAnnualRate: Number(payload.consumerAnnualRate),
      lowAmountAnnualRate: Number(payload.lowAmountAnnualRate),
      factorMonthlyRate: Number(payload.factorMonthlyRate),
      maxTermMonths: Number(payload.maxTermMonths),
      paymentRounding: Number(payload.paymentRounding),
      useMontelibanoTable: Boolean(payload.useMontelibanoTable),
      active: Boolean(payload.active)
    };
    const { data } = await api.put<FinancialSettings>('/api/financial-settings', body);
    setFinancialSettings(data);
    setNotice({ type: 'success', text: 'Configuracion financiera actualizada.' });
    setFinancialForm({ open: false });
  };

  return <Stack spacing={3}>
    <Header title="Configuracion" onRefresh={() => { reloadCompanies(); reloadUsers(); reloadFinancialSettings(); }} />
    <Card><CardContent><Grid container spacing={2}>
      <Grid item xs={12} md={6}><TextField fullWidth label="API URL" value={import.meta.env.VITE_API_URL ?? ''} InputProps={{ readOnly: true }} /></Grid>
      <Grid item xs={12} md={6}><TextField fullWidth label="Tenant" value={import.meta.env.VITE_TENANT ?? 'demo'} InputProps={{ readOnly: true }} /></Grid>
      <Grid item xs={12}><Chip icon={<CheckCircle />} label={`Sesion activa: ${user?.email} (${user?.roles.join(', ')})`} /></Grid>
    </Grid></CardContent></Card>
    <Card><CardContent>
      <Stack spacing={2}>
        <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'stretch', sm: 'center' }} gap={1}>
          <Box>
            <Typography variant="h5" fontWeight={900}>Configuracion financiera</Typography>
            <Typography color="text.secondary" fontSize={14}>Tabla de financiacion usada por la empresa al crear cotizaciones.</Typography>
          </Box>
          {canManage && <Button variant="outlined" startIcon={<Edit />} onClick={() => setFinancialForm({ open: true, item: financialSettings })}>Editar tabla</Button>}
        </Stack>
        <StatusBar loading={loadingFinancialSettings} error={financialSettingsError} />
        {financialSettings && <Grid container spacing={1.5}>
          <Grid item xs={6} md={3}><Metric label="Salario minimo" value={money(financialSettings.minimumWage)} /></Grid>
          <Grid item xs={6} md={3}><Metric label="Consumo EA" value={`${financialSettings.consumerAnnualRate}%`} /></Grid>
          <Grid item xs={6} md={3}><Metric label="Bajo monto EA" value={`${financialSettings.lowAmountAnnualRate}%`} /></Grid>
          <Grid item xs={6} md={3}><Metric label="Factor mensual" value={`${financialSettings.factorMonthlyRate}%`} /></Grid>
          <Grid item xs={6} md={3}><Metric label="Plazo maximo" value={`${financialSettings.maxTermMonths} meses`} /></Grid>
          <Grid item xs={6} md={3}><Metric label="Redondeo cuota" value={money(financialSettings.paymentRounding)} /></Grid>
          <Grid item xs={6} md={3}><Metric label="Tabla Montelibano" value={financialSettings.useMontelibanoTable ? 'Activa' : 'Manual'} /></Grid>
          <Grid item xs={6} md={3}><Metric label="Estado" value={financialSettings.active ? 'Activa' : 'Inactiva'} /></Grid>
        </Grid>}
      </Stack>
    </CardContent></Card>
    {canManage && <>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h5" fontWeight={900}>Empresas</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => setCompanyForm({ open: true })}>Nueva empresa</Button>
      </Stack>
      <StatusBar loading={loadingCompanies} error={companiesError} />
      <EntityTable
        headers={['Logo', 'Nombre', 'Subdominio', 'Dominio', 'Estado', 'Acciones']}
        empty="No hay empresas registradas"
        rows={companies.map((c) => [
          c.logoDataUrl ? <Box component="img" src={c.logoDataUrl} alt={`Logo ${c.name}`} sx={{ width: 72, height: 36, objectFit: 'contain', display: 'block' }} /> : <Typography color="text.secondary" fontSize={13}>Sin logo</Typography>,
          c.name,
          c.subdomain,
          c.customDomain,
          <StatusChip label={c.active ? 'Activa' : 'Inactiva'} tone={c.active ? 'success' : 'default'} />,
          <Actions onEdit={() => setCompanyForm({ open: true, item: c })} />
        ])}
      />
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h5" fontWeight={900}>Usuarios</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => setUserForm({ open: true })}>Nuevo usuario</Button>
      </Stack>
      <StatusBar loading={loadingUsers} error={usersError} />
      <EntityTable
        headers={['Nombre', 'Email', 'Empresa', 'Roles']}
        empty="No hay usuarios registrados"
        rows={users.map((u) => [
          u.fullName,
          u.email,
          companies.find((c) => c.id === u.companyId)?.name ?? u.companyId,
          u.roles.join(', ')
        ])}
      />
    </>}
    <CompanyDialog form={companyForm} onClose={() => setCompanyForm({ open: false })} onSave={saveCompany} />
    <UserDialog form={userForm} companies={companies.filter((x) => x.active)} onClose={() => setUserForm({ open: false })} onSave={saveUser} />
    <FinancialSettingsDialog form={financialForm} onClose={() => setFinancialForm({ open: false })} onSave={saveFinancialSettings} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function CompanyDialog({ form, onClose, onSave }: DialogProps<Company, typeof emptyCompany>) {
  const initial = form.item ? { name: form.item.name, subdomain: form.item.subdomain, customDomain: form.item.customDomain ?? '', logoDataUrl: form.item.logoDataUrl ?? '', active: form.item.active } : emptyCompany;
  return <FormDialog title={form.item ? 'Editar empresa' : 'Nueva empresa'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <CompanyLogoPicker value={v.logoDataUrl} onChange={(logoDataUrl) => set({ logoDataUrl })} />
      <TextField required label="Nombre" value={v.name} onChange={(e) => set({ name: e.target.value })} />
      <TextField required label="Subdominio" value={v.subdomain} onChange={(e) => set({ subdomain: e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, '') })} />
      <TextField label="Dominio personalizado" value={v.customDomain} onChange={(e) => set({ customDomain: e.target.value })} />
      <TextField select label="Estado" value={String(v.active)} onChange={(e) => set({ active: e.target.value === 'true' })}><MenuItem value="true">Activa</MenuItem><MenuItem value="false">Inactiva</MenuItem></TextField>
    </>}
  </FormDialog>;
}

function CompanyLogoPicker({ value, onChange }: { value?: string; onChange: (value: string) => void }) {
  const [error, setError] = useState('');
  const handleLogo = async (file?: File) => {
    if (!file) return;
    try {
      setError('');
      onChange(await normalizeCompanyLogo(file));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'No se pudo procesar el logo.');
    }
  };

  return <Stack spacing={1}>
    <Typography variant="subtitle2" fontWeight={900} color="primary">Logo de la empresa</Typography>
    <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ xs: 'stretch', sm: 'center' }}>
      <Box sx={{
        width: 160,
        height: 80,
        border: '1px dashed',
        borderColor: 'divider',
        borderRadius: 1,
        bgcolor: 'background.default',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        overflow: 'hidden'
      }}>
        {value ? <Box component="img" src={value} alt="Logo de empresa" sx={{ width: '100%', height: '100%', objectFit: 'contain' }} /> : <Typography color="text.secondary" fontSize={13}>320 x 160 px</Typography>}
      </Box>
      <Stack spacing={1} sx={{ minWidth: 0 }}>
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
          <Button variant="outlined" component="label" startIcon={<UploadFile />}>
            Cargar logo
            <input hidden type="file" accept="image/png,image/jpeg,image/webp" onChange={(e) => void handleLogo(e.target.files?.[0])} />
          </Button>
          {value && <Button color="inherit" onClick={() => onChange('')}>Quitar</Button>}
        </Stack>
        <Typography variant="caption" color="text.secondary">PNG, JPG o WebP. Se ajusta automaticamente a 320 x 160 px.</Typography>
      </Stack>
    </Stack>
    {error && <Alert severity="error">{error}</Alert>}
  </Stack>;
}

function UserDialog({ form, companies, onClose, onSave }: DialogProps<User, typeof emptyUser> & { companies: Company[] }) {
  const initial = { ...emptyUser, companyId: companies[0]?.id ?? '' };
  return <FormDialog title="Nuevo usuario" open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <TextField required label="Nombre completo" value={v.fullName} onChange={(e) => set({ fullName: e.target.value })} />
      <TextField required label="Email" value={v.email} onChange={(e) => set({ email: e.target.value })} />
      <TextField required label="Contrasena temporal" type="password" value={v.password} onChange={(e) => set({ password: e.target.value })} />
      <TextField required select label="Empresa" value={v.companyId} onChange={(e) => set({ companyId: e.target.value })}>{companies.map((c) => <MenuItem key={c.id} value={c.id}>{c.name} ({c.subdomain})</MenuItem>)}</TextField>
      <TextField select label="Rol" value={v.roles[0] ?? 'Vendedor'} onChange={(e) => set({ roles: [e.target.value] })}>{['Administrador', 'Supervisor', 'Vendedor'].map((role) => <MenuItem key={role} value={role}>{role}</MenuItem>)}</TextField>
    </>}
  </FormDialog>;
}

function FinancialSettingsDialog({ form, onClose, onSave }: DialogProps<FinancialSettings, typeof emptyFinancialSettings>) {
  const initial = form.item ? {
    minimumWage: form.item.minimumWage,
    consumerAnnualRate: form.item.consumerAnnualRate,
    lowAmountAnnualRate: form.item.lowAmountAnnualRate,
    factorMonthlyRate: form.item.factorMonthlyRate,
    maxTermMonths: form.item.maxTermMonths,
    paymentRounding: form.item.paymentRounding,
    useMontelibanoTable: form.item.useMontelibanoTable,
    active: form.item.active
  } : emptyFinancialSettings;

  return <FormDialog title="Configuracion financiera" open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <FormControlLabel control={<Checkbox checked={v.active} onChange={(e) => set({ active: e.target.checked })} />} label="Configuracion activa" />
      <FormControlLabel control={<Checkbox checked={v.useMontelibanoTable} onChange={(e) => set({ useMontelibanoTable: e.target.checked })} />} label="Usar tabla Montelibano en cotizaciones" />
      <TextField required label="Salario minimo vigente" type="number" value={v.minimumWage} onChange={(e) => set({ minimumWage: Number(e.target.value) })} />
      <Grid container spacing={1.5}>
        <Grid item xs={12} sm={6}><TextField fullWidth required label="Tasa consumo EA (%)" type="number" value={v.consumerAnnualRate} onChange={(e) => set({ consumerAnnualRate: Number(e.target.value) })} /></Grid>
        <Grid item xs={12} sm={6}><TextField fullWidth required label="Tasa bajo monto EA (%)" type="number" value={v.lowAmountAnnualRate} onChange={(e) => set({ lowAmountAnnualRate: Number(e.target.value) })} /></Grid>
      </Grid>
      <Grid container spacing={1.5}>
        <Grid item xs={12} sm={4}><TextField fullWidth required label="Factor mensual (%)" type="number" value={v.factorMonthlyRate} onChange={(e) => set({ factorMonthlyRate: Number(e.target.value) })} /></Grid>
        <Grid item xs={12} sm={4}><TextField fullWidth required label="Plazo maximo" type="number" value={v.maxTermMonths} onChange={(e) => set({ maxTermMonths: Number(e.target.value) })} /></Grid>
        <Grid item xs={12} sm={4}><TextField fullWidth required label="Redondeo cuota" type="number" value={v.paymentRounding} onChange={(e) => set({ paymentRounding: Number(e.target.value) })} /></Grid>
      </Grid>
    </>}
  </FormDialog>;
}

function CustomerDialog({ form, onClose, onSave }: DialogProps<Customer, typeof emptyCustomer>) {
  const initial = form.item ? {
    identificationType: form.item.identificationType ?? 1,
    identificationNumber: form.item.identificationNumber ?? '',
    firstNames: form.item.firstNames || form.item.name,
    lastNames: form.item.lastNames ?? '',
    firstName: form.item.firstName || (form.item.firstNames || form.item.name).split(' ')[0] || '',
    middleName: form.item.middleName ?? (form.item.firstNames || '').split(' ').slice(1).join(' '),
    lastName: form.item.lastName || (form.item.lastNames || '').split(' ')[0] || '',
    secondLastName: form.item.secondLastName ?? (form.item.lastNames || '').split(' ').slice(1).join(' '),
    companyName: form.item.companyName ?? '',
    email: form.item.email ?? '',
    phoneCountryCode: form.item.phoneCountryCode ?? '+57',
    phone: form.item.phone ?? '',
    address: form.item.address ?? '',
    city: form.item.city ?? '',
    birthDate: form.item.birthDate?.slice(0, 10) ?? '',
    occupation: form.item.occupation ?? '',
    status: form.item.status,
    tags: form.item.tags ?? '',
    notes: form.item.notes ?? ''
  } : emptyCustomer;
  return <FormDialog title={form.item ? 'Editar cliente' : 'Nuevo cliente'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <SectionTitle title="Identificacion" />
      <FieldGrid>
        <TextField fullWidth select label="Tipo identificacion" value={v.identificationType} onChange={(e) => set({ identificationType: Number(e.target.value) })}>{identificationOptions.map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}</TextField>
        <TextField fullWidth label="Numero identificacion" value={v.identificationNumber} onChange={(e) => set({ identificationNumber: e.target.value })} />
      </FieldGrid>
      <SectionTitle title="Datos personales" />
      <FieldGrid>
        <TextField fullWidth required label="Primer nombre" value={v.firstName} onChange={(e) => set({ firstName: e.target.value, firstNames: fullFirstNames(e.target.value, v.middleName) })} />
        <TextField fullWidth label="Segundo nombre" value={v.middleName} onChange={(e) => set({ middleName: e.target.value, firstNames: fullFirstNames(v.firstName, e.target.value) })} />
      </FieldGrid>
      <FieldGrid>
        <TextField fullWidth required label="Primer apellido" value={v.lastName} onChange={(e) => set({ lastName: e.target.value, lastNames: fullLastNames(e.target.value, v.secondLastName) })} />
        <TextField fullWidth label="Segundo apellido" value={v.secondLastName} onChange={(e) => set({ secondLastName: e.target.value, lastNames: fullLastNames(v.lastName, e.target.value) })} />
      </FieldGrid>
      <FieldGrid>
        <TextField fullWidth label="Fecha nacimiento" type="date" value={v.birthDate} onChange={(e) => set({ birthDate: e.target.value })} InputLabelProps={{ shrink: true }} />
        <TextField fullWidth label="Ocupacion" value={v.occupation} onChange={(e) => set({ occupation: e.target.value })} />
      </FieldGrid>
      <SectionTitle title="Contacto" />
      <FieldGrid columns={3}>
        <TextField fullWidth label="Indicativo" value={v.phoneCountryCode} onChange={(e) => set({ phoneCountryCode: e.target.value })} />
        <TextField fullWidth label="Telefono / WhatsApp" value={v.phone} onChange={(e) => set({ phone: e.target.value })} sx={{ gridColumn: { sm: 'span 2' } }} />
      </FieldGrid>
      <TextField label="Email" value={v.email} onChange={(e) => set({ email: e.target.value })} />
      <FieldGrid>
        <TextField fullWidth label="Direccion" value={v.address} onChange={(e) => set({ address: e.target.value })} />
        <TextField fullWidth label="Ciudad" value={v.city} onChange={(e) => set({ city: e.target.value })} />
      </FieldGrid>
      <SectionTitle title="Gestion comercial" />
      <TextField label="Empresa o razon comercial" value={v.companyName} onChange={(e) => set({ companyName: e.target.value })} />
      <TextField select label="Estado" value={v.status} onChange={(e) => set({ status: Number(e.target.value) })}>{[1, 2, 3].map((x) => <MenuItem key={x} value={x}>{statusLabel(x)}</MenuItem>)}</TextField>
      <TextField label="Etiquetas" value={v.tags} onChange={(e) => set({ tags: e.target.value })} />
      <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} multiline minRows={2} />
    </>}
  </FormDialog>;
}

function ProductDialog({ form, onClose, onSave, onChanged }: DialogProps<Product, typeof emptyProduct> & { onChanged: () => void }) {
  const initial = form.item ? {
    name: form.item.name,
    category: form.item.category,
    brand: form.item.brand,
    model: form.item.model,
    reference: form.item.reference,
    description: form.item.description ?? '',
    engineCc: form.item.engineCc?.toString() ?? '',
    year: form.item.year?.toString() ?? '',
    color: form.item.color ?? '',
    price: form.item.price,
    active: form.item.active
  } : emptyProduct;
  return <FormDialog title={form.item ? 'Editar producto' : 'Nuevo producto'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <TextField required label="Nombre del producto" value={v.name} onChange={(e) => set({ name: e.target.value })} />
      <TextField required select label="Categoria" value={v.category} onChange={(e) => set({ category: e.target.value })}>
        {['Moto', 'Accesorio', 'Seguro', 'Tramite', 'Repuesto', 'Servicio', 'Garantia', 'Otro'].map((category) => <MenuItem key={category} value={category}>{category}</MenuItem>)}
      </TextField>
      <TextField label="Marca" value={v.brand} onChange={(e) => set({ brand: e.target.value })} />
      <TextField label="Modelo" value={v.model} onChange={(e) => set({ model: e.target.value })} />
      <TextField required label="Referencia" value={v.reference} onChange={(e) => set({ reference: e.target.value })} />
      <TextField label="Descripcion" value={v.description} onChange={(e) => set({ description: e.target.value })} multiline minRows={2} />
      <FieldGrid>
        <TextField fullWidth label="Cilindraje" type="number" value={v.engineCc} onChange={(e) => set({ engineCc: e.target.value })} />
        <TextField fullWidth label="Ano" type="number" value={v.year} onChange={(e) => set({ year: e.target.value })} />
      </FieldGrid>
      <TextField label="Color" value={v.color} onChange={(e) => set({ color: e.target.value })} />
      <TextField required label="Precio" type="number" value={v.price} onChange={(e) => set({ price: Number(e.target.value) })} />
      <TextField select label="Estado" value={String(v.active)} onChange={(e) => set({ active: e.target.value === 'true' })}><MenuItem value="true">Activa</MenuItem><MenuItem value="false">Inactiva</MenuItem></TextField>
      {form.item && <ProductPhotosManager product={form.item} onChanged={onChanged} />}
      {!form.item && <Alert severity="info">Guarde el producto primero. Luego podra editarlo para adjuntar una o varias fotos y elegir la foto principal del PDF.</Alert>}
    </>}
  </FormDialog>;
}

function ProductPhotoThumb({ photo, size = 54 }: { photo?: ProductPhoto; size?: number }) {
  return <Box
    sx={{
      width: size,
      height: size,
      borderRadius: 1,
      overflow: 'hidden',
      bgcolor: '#e2e8f0',
      border: '1px solid #d8e0e8',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      flex: '0 0 auto'
    }}
  >
    {photo
      ? <Box component="img" src={photo.dataUrl} alt={photo.fileName} sx={{ width: '100%', height: '100%', objectFit: 'cover' }} />
      : <Inventory2 fontSize="small" color="disabled" />}
  </Box>;
}

function ProductPhotosManager({ product, onChanged }: { product: Product; onChanged: () => void }) {
  const [photos, setPhotos] = useState<ProductPhoto[]>(product.photos ?? []);
  const [uploading, setUploading] = useState(false);
  const [notice, setNotice] = useState<Notice>();

  useEffect(() => {
    setPhotos(product.photos ?? []);
    setNotice(undefined);
    setUploading(false);
  }, [product.id, product.photos]);

  const replaceProduct = (updated: Product) => {
    setPhotos(updated.photos ?? []);
    onChanged();
  };

  const upload = async (files: FileList | null) => {
    if (!files?.length) return;
    const formData = new FormData();
    Array.from(files).forEach((file) => formData.append('files', file));
    setUploading(true);
    setNotice(undefined);
    try {
      const { data } = await api.post<Product>(`/api/products/${product.id}/photos`, formData, { headers: { 'Content-Type': 'multipart/form-data' } });
      replaceProduct(data);
      setNotice({ type: 'success', text: 'Fotos cargadas correctamente.' });
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    } finally {
      setUploading(false);
    }
  };

  const setDefault = async (photo: ProductPhoto) => {
    try {
      const { data } = await api.put<Product>(`/api/products/${product.id}/photos/${photo.id}/quote-default`);
      replaceProduct(data);
      setNotice({ type: 'success', text: 'Foto principal del PDF actualizada.' });
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  const remove = async (photo: ProductPhoto) => {
    try {
      const { data } = await api.delete<Product>(`/api/products/${product.id}/photos/${photo.id}`);
      replaceProduct(data);
      setNotice({ type: 'success', text: 'Foto eliminada.' });
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  return <Paper variant="outlined" sx={{ p: 2, bgcolor: '#f8fafc' }}>
    <Stack spacing={1.5}>
      <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'stretch', sm: 'center' }} gap={1}>
        <Box>
          <Typography fontWeight={800}>Fotos del producto</Typography>
          <Typography variant="caption" color="text.secondary">Puede cargar varias fotos y marcar cual se imprime en la cotizacion. El PDF admite JPG/JPEG y PNG compatibles.</Typography>
        </Box>
        <Button component="label" variant="outlined" startIcon={<UploadFile />} disabled={uploading}>
          {uploading ? 'Subiendo...' : 'Adjuntar fotos'}
          <input hidden multiple type="file" accept="image/jpeg,image/jpg,image/png,image/webp" onChange={(event) => void upload(event.target.files)} />
        </Button>
      </Stack>
      {notice && <Alert severity={notice.type === 'success' ? 'success' : 'error'}>{notice.text}</Alert>}
      {photos.length === 0 && <Alert severity="info">Este producto aun no tiene fotos. La cotizacion se generara sin imagen del producto.</Alert>}
      {photos.length > 0 && <Box className="productPhotoGrid">
        {photos.map((photo) => <Paper key={photo.id} variant="outlined" className="productPhotoCard">
          <Box component="img" src={photo.dataUrl} alt={photo.fileName} className="productPhotoImage" />
          <Stack spacing={1} sx={{ p: 1 }}>
            <Tooltip title={photo.fileName}>
              <Typography variant="caption" noWrap>{photo.fileName}</Typography>
            </Tooltip>
            <Stack direction="row" gap={1} flexWrap="wrap">
              <Chip size="small" color={photo.isQuoteDefault ? 'success' : 'default'} label={photo.isQuoteDefault ? 'Foto PDF' : readableFileSize(photo.sizeBytes)} />
            </Stack>
            <Stack direction="row" gap={1}>
              <Button type="button" size="small" variant={photo.isQuoteDefault ? 'contained' : 'outlined'} onClick={() => void setDefault(photo)}>Usar en PDF</Button>
              <IconButton size="small" color="error" onClick={() => void remove(photo)}><Delete fontSize="small" /></IconButton>
            </Stack>
          </Stack>
        </Paper>)}
      </Box>}
    </Stack>
  </Paper>;
}

function QuoteDialog({ form, products, onClose, onSave }: DialogProps<Quote, typeof emptyQuote> & { products: Product[] }) {
  const initial = { ...emptyQuote, productId: products[0]?.id ?? '' };
  const [identityLoading, setIdentityLoading] = useState(false);
  const [identityNotice, setIdentityNotice] = useState<Notice>();

  useEffect(() => {
    if (!form.open) {
      setIdentityLoading(false);
      setIdentityNotice(undefined);
    }
  }, [form.open]);

  const lookupIdentity = async (value: typeof emptyQuote, set: (patch: Partial<typeof emptyQuote>) => void) => {
    const digits = identificationDigits(value.identificationNumber);
    if (Number(value.identificationType) !== 1) {
      setIdentityNotice({ type: 'error', text: 'La consulta esta disponible para cedula de ciudadania.' });
      return;
    }
    if (digits.length < 5) {
      setIdentityNotice({ type: 'error', text: 'Digite una cedula valida para consultar.' });
      return;
    }

    setIdentityLoading(true);
    setIdentityNotice(undefined);
    try {
      const { data } = await api.get<ColombianIdentityLookup>('/api/identity/colombia/cedula', { params: { documentNumber: digits } });
      set({
        identificationNumber: data.documentNumber || digits,
        customerFirstName: data.firstName ?? value.customerFirstName,
        customerMiddleName: data.middleName ?? value.customerMiddleName,
        customerLastName: data.lastName ?? value.customerLastName,
        customerSecondLastName: data.secondLastName ?? value.customerSecondLastName,
        customerFirstNames: fullFirstNames(data.firstName ?? value.customerFirstName, data.middleName ?? value.customerMiddleName, value.customerFirstNames),
        customerLastNames: fullLastNames(data.lastName ?? value.customerLastName, data.secondLastName ?? value.customerSecondLastName, value.customerLastNames)
      });
      const extra = [data.expeditionCity, data.expeditionDepartment].filter(Boolean).join(', ');
      setIdentityNotice({
        type: 'success',
        text: data.source === 'database'
          ? 'Datos encontrados en la base del CRM.'
          : `Datos encontrados y guardados en clientes${extra ? ` - expedida en ${extra}` : ''}.`
      });
    } catch (err) {
      setIdentityNotice({ type: 'error', text: apiError(err) });
    } finally {
      setIdentityLoading(false);
    }
  };

  return <FormDialog title="Nueva cotizacion" open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => {
      const selectedProduct = products.find((product) => product.id === v.productId);
      return <>
        <TextField required select label="Tipo de identificacion" value={v.identificationType} onChange={(e) => set({ identificationType: Number(e.target.value) })}>
          {identificationOptions.map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}
        </TextField>
        <TextField
          label="Numero de identificacion"
          value={v.identificationNumber}
          onChange={(e) => set({ identificationNumber: e.target.value })}
          InputProps={{ endAdornment: <IdentificationLookupAdornment identification={v.identificationNumber} /> }}
        />
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} alignItems={{ xs: 'stretch', sm: 'center' }}>
          <Button variant="outlined" startIcon={<AutoAwesome />} disabled={identityLoading || !identificationDigits(v.identificationNumber)} onClick={() => void lookupIdentity(v, set)}>
            {identityLoading ? 'Consultando...' : 'Consultar'}
          </Button>
          <Typography variant="caption" color="text.secondary">Primero busca en el CRM; si no existe, consulta el proveedor externo.</Typography>
        </Stack>
        {identityNotice && <Alert severity={identityNotice.type === 'success' ? 'success' : identityNotice.type === 'info' ? 'info' : 'error'}>{identityNotice.text}</Alert>}
        <FieldGrid>
          <TextField fullWidth required label="Primer nombre" value={v.customerFirstName} onChange={(e) => set({ customerFirstName: e.target.value, customerFirstNames: fullFirstNames(e.target.value, v.customerMiddleName) })} />
          <TextField fullWidth label="Segundo nombre" value={v.customerMiddleName} onChange={(e) => set({ customerMiddleName: e.target.value, customerFirstNames: fullFirstNames(v.customerFirstName, e.target.value) })} />
        </FieldGrid>
        <FieldGrid>
          <TextField fullWidth required label="Primer apellido" value={v.customerLastName} onChange={(e) => set({ customerLastName: e.target.value, customerLastNames: fullLastNames(e.target.value, v.customerSecondLastName) })} />
          <TextField fullWidth label="Segundo apellido" value={v.customerSecondLastName} onChange={(e) => set({ customerSecondLastName: e.target.value, customerLastNames: fullLastNames(v.customerLastName, e.target.value) })} />
        </FieldGrid>
        <FieldGrid columns={3}>
          <TextField fullWidth required label="Indicativo" value={v.phoneCountryCode} onChange={(e) => set({ phoneCountryCode: e.target.value })} />
          <TextField fullWidth required label="Telefono / WhatsApp" value={v.phoneNumber} onChange={(e) => set({ phoneNumber: e.target.value })} sx={{ gridColumn: { sm: 'span 2' } }} />
        </FieldGrid>
        <TextField required select label="Producto" value={v.productId} onChange={(e) => set({ productId: e.target.value })}>
          {products.length ? products.map((product) => <MenuItem key={product.id} value={product.id}>{productName(product)} ({product.category}) - {money(product.price)}</MenuItem>) : <MenuItem value="">No hay productos activos</MenuItem>}
        </TextField>
        {selectedProduct && <Paper variant="outlined" sx={{ p: 2, bgcolor: '#f8fafc' }}>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} alignItems={{ xs: 'stretch', sm: 'center' }}>
            <ProductPhotoThumb photo={(selectedProduct.photos ?? []).find((photo) => photo.isQuoteDefault) ?? (selectedProduct.photos ?? [])[0]} size={88} />
            <Box>
              <Typography fontWeight={800}>Foto que saldra en el PDF</Typography>
              <Typography variant="body2" color="text.secondary">
                {(selectedProduct.photos ?? []).some((photo) => photo.isQuoteDefault)
                  ? 'Se usara la foto marcada como Foto PDF en el producto.'
                  : 'Este producto no tiene foto principal. Puede configurarla en Productos.'}
              </Typography>
            </Box>
          </Stack>
        </Paper>}
        <FieldGrid columns={2}>
          <TextField fullWidth label="Cuota inicial" type="number" value={v.downPayment} onChange={(e) => set({ downPayment: Number(e.target.value) })} />
          <TextField fullWidth label="Numero de cuotas" type="number" value={v.termMonths} onChange={(e) => set({ termMonths: Number(e.target.value) })} />
        </FieldGrid>
        <FieldGrid columns={2}>
          <TextField fullWidth label="Seguro" type="number" value={v.insurance} onChange={(e) => set({ insurance: Number(e.target.value) })} />
          <TextField fullWidth label="Gastos administrativos" type="number" value={v.administrativeFees} onChange={(e) => set({ administrativeFees: Number(e.target.value) })} />
        </FieldGrid>
        <QuoteSimulationPreview value={v} selectedProduct={selectedProduct} />
        <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} multiline minRows={2} />
      </>;
    }}
  </FormDialog>;
}

function QuoteSimulationPreview({ value, selectedProduct }: { value: typeof emptyQuote; selectedProduct?: Product }) {
  const [simulation, setSimulation] = useState<QuoteSimulationResult>();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const productPrice = selectedProduct?.price ?? 0;

  useEffect(() => {
    if (!selectedProduct?.id) {
      setSimulation(undefined);
      setError('');
      return undefined;
    }

    const timer = window.setTimeout(() => {
      setLoading(true);
      setError('');
      api.post<QuoteSimulationResult>('/api/quotes/simulate', {
        productId: selectedProduct.id,
        productPrice: selectedProduct.price,
        downPayment: Number(value.downPayment),
        insurance: Number(value.insurance),
        administrativeFees: Number(value.administrativeFees),
        termMonths: Number(value.termMonths),
        monthlyInterestRate: Number(value.monthlyInterestRate)
      })
        .then(({ data }) => setSimulation(data))
        .catch((err) => setError(apiError(err)))
        .finally(() => setLoading(false));
    }, 250);

    return () => window.clearTimeout(timer);
  }, [selectedProduct?.id, selectedProduct?.price, value.downPayment, value.insurance, value.administrativeFees, value.termMonths, value.monthlyInterestRate]);

  const insurance = Math.max(Number(value.insurance) || 0, 0);
  const administrativeFees = Math.max(Number(value.administrativeFees) || 0, 0);
  const totalToFinance = productPrice + insurance + administrativeFees;
  const fallbackDownPayment = Math.min(Number(value.downPayment) || 0, totalToFinance);
  const fallbackTermMonths = Math.max(Number(value.termMonths) || 1, 1);
  const fallbackFinanced = Math.max(totalToFinance - fallbackDownPayment, 0);
  const fallbackPayment = estimateMonthlyPayment(fallbackFinanced, fallbackTermMonths, Number(value.monthlyInterestRate) || 0);
  const preview = simulation ?? {
    downPayment: fallbackDownPayment,
    insurance,
    administrativeFees,
    termMonths: fallbackTermMonths,
    monthlyInterestRate: Number(value.monthlyInterestRate) || 0,
    financedAmount: fallbackFinanced,
    estimatedMonthlyPayment: fallbackPayment,
    estimatedTotalPayment: fallbackDownPayment + fallbackPayment * fallbackTermMonths,
    creditType: 'Vista previa',
    usedCompanyFinancialSettings: false
  };

  return <Paper variant="outlined" sx={{ p: 2, bgcolor: '#f8fafc' }}>
    <Stack spacing={1.5}>
      {loading && <LinearProgress />}
      {error && <Alert severity="warning">{error}</Alert>}
      <FieldGrid columns={3}>
        <Box><Typography variant="caption" color="text.secondary">Valor producto</Typography><Typography fontWeight={700}>{money(productPrice)}</Typography></Box>
        <Box><Typography variant="caption" color="text.secondary">Total financiado</Typography><Typography fontWeight={700}>{money(preview.financedAmount)}</Typography></Box>
        <Box><Typography variant="caption" color="text.secondary">Cuota aproximada</Typography><Typography fontWeight={700}>{money(preview.estimatedMonthlyPayment)}</Typography></Box>
      </FieldGrid>
    </Stack>
  </Paper>;
}

function QuotePdfPreviewDialog({ quote, onClose, onDownload }: { quote?: Quote; onClose: () => void; onDownload: (quote: Quote) => Promise<void> }) {
  const muiTheme = useTheme();
  const fullScreen = useMediaQuery(muiTheme.breakpoints.down('sm'));
  const [url, setUrl] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    let objectUrl = '';
    if (!quote) {
      setUrl('');
      setError('');
      setLoading(false);
      return undefined;
    }

    setLoading(true);
    setError('');
    api.get<Blob>(`/api/quotes/${quote.id}/pdf`, { responseType: 'blob' })
      .then(({ data }) => {
        objectUrl = URL.createObjectURL(data);
        setUrl(objectUrl);
      })
      .catch((err) => setError(apiError(err)))
      .finally(() => setLoading(false));

    return () => {
      if (objectUrl) URL.revokeObjectURL(objectUrl);
    };
  }, [quote?.id]);

  const print = () => {
    if (!url) return;
    const win = window.open(url, '_blank');
    if (!win) {
      setError('El navegador bloqueo la ventana de impresion. Permita ventanas emergentes para imprimir.');
      return;
    }
    win.addEventListener('load', () => {
      win.focus();
      win.print();
    }, { once: true });
  };

  return <Dialog open={!!quote} onClose={onClose} fullWidth maxWidth="lg" fullScreen={fullScreen}>
    <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 1 }}>
      <Box>
        <Typography fontWeight={900}>Vista previa de cotizacion</Typography>
        <Typography variant="caption" color="text.secondary">{quote?.number}</Typography>
      </Box>
      <IconButton onClick={onClose}><Close /></IconButton>
    </DialogTitle>
    <DialogContent sx={{ p: { xs: 1.5, sm: 2 } }}>
      <Stack spacing={2}>
        {loading && <LinearProgress />}
        {error && <Alert severity="error">{error}</Alert>}
        {!loading && !error && url && <Box
          component="iframe"
          title={`Cotizacion ${quote?.number}`}
          src={url}
          sx={{
            width: '100%',
            height: { xs: '70vh', sm: '76vh' },
            border: '1px solid #d8e0e8',
            borderRadius: 1,
            bgcolor: '#f8fafc'
          }}
        />}
      </Stack>
    </DialogContent>
    <DialogActions sx={{ px: { xs: 2, sm: 3 }, pb: 2, flexWrap: 'wrap' }}>
      <Button onClick={onClose}>Cerrar</Button>
      <Button startIcon={<Download />} disabled={!quote || !url} onClick={() => quote && void onDownload(quote)}>Descargar PDF</Button>
      <Button variant="contained" disabled={!url} onClick={print}>Imprimir</Button>
    </DialogActions>
  </Dialog>;
}

function CreditApplicationDialog({ form, customers, products, quotes, deals, onClose, onSave }: DialogProps<CreditApplication, typeof emptyCreditApplication> & { customers: Customer[]; products: Product[]; quotes: Quote[]; deals: Deal[] }) {
  const quote = quotes.find((x) => x.id === (form.item?.quoteId ?? ''));
  const initial = form.item ? {
    customerId: form.item.customerId,
    productId: form.item.productId,
    quoteId: form.item.quoteId ?? '',
    dealId: form.item.dealId ?? '',
    identificationType: form.item.identificationType,
    identificationNumber: form.item.identificationNumber,
    birthDate: form.item.birthDate?.slice(0, 10) ?? '',
    mobile: form.item.mobile,
    address: form.item.address ?? '',
    city: form.item.city ?? '',
    occupation: form.item.occupation ?? '',
    monthlyIncome: form.item.monthlyIncome,
    downPayment: form.item.downPayment,
    termMonths: form.item.termMonths,
    motorcycleValue: form.item.motorcycleValue,
    coDebtorName: form.item.coDebtorName ?? '',
    coDebtorIdentification: form.item.coDebtorIdentification ?? '',
    coDebtorMobile: form.item.coDebtorMobile ?? '',
    coDebtorRelationship: form.item.coDebtorRelationship ?? '',
    coDebtorMonthlyIncome: form.item.coDebtorMonthlyIncome ?? 0,
    reference1Name: form.item.reference1Name ?? '',
    reference1Mobile: form.item.reference1Mobile ?? '',
    reference1Relationship: form.item.reference1Relationship ?? '',
    reference2Name: form.item.reference2Name ?? '',
    reference2Mobile: form.item.reference2Mobile ?? '',
    reference2Relationship: form.item.reference2Relationship ?? '',
    status: form.item.status,
    notes: form.item.notes ?? ''
  } : { ...emptyCreditApplication, customerId: customers[0]?.id ?? '', productId: products[0]?.id ?? '', motorcycleValue: products[0]?.price ?? 0 };
  return <FormDialog title={form.item ? 'Editar solicitud de credito' : 'Nueva solicitud de credito'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => {
      const selectedQuote = quotes.find((x) => x.id === v.quoteId);
      const selectedProduct = products.find((x) => x.id === v.productId);
      const selectedCustomer = customers.find((x) => x.id === v.customerId);
      return <>
        <TextField select label="Cotizacion" value={v.quoteId} onChange={(e) => {
          const selected = quotes.find((x) => x.id === e.target.value);
          set({
            quoteId: e.target.value,
            customerId: selected?.customerId ?? v.customerId,
            productId: selected?.productId ?? v.productId,
            identificationType: selected?.identificationType ?? v.identificationType,
            identificationNumber: selected?.identificationNumber ?? v.identificationNumber,
            motorcycleValue: selected?.productPrice ?? v.motorcycleValue,
            downPayment: selected?.downPayment ?? v.downPayment,
            termMonths: selected?.termMonths ?? v.termMonths
          });
        }}>
          <MenuItem value="">Sin cotizacion</MenuItem>
          {quotes.map((x) => <MenuItem key={x.id} value={x.id}>{x.number} - {fullFirstNames(x.customerFirstName, x.customerMiddleName, x.customerFirstNames)} {fullLastNames(x.customerLastName, x.customerSecondLastName, x.customerLastNames)}</MenuItem>)}
        </TextField>
        <TextField required select label="Cliente" value={v.customerId} onChange={(e) => set({ customerId: e.target.value })}>{customers.map((x) => <MenuItem key={x.id} value={x.id}>{x.firstNames || x.name} {x.lastNames}</MenuItem>)}</TextField>
        <TextField required select label="Producto principal" value={v.productId} onChange={(e) => {
          const product = products.find((x) => x.id === e.target.value);
          set({ productId: e.target.value, motorcycleValue: product?.price ?? v.motorcycleValue });
        }}>{products.map((x) => <MenuItem key={x.id} value={x.id}>{productName(x)} ({x.category}) - {money(x.price)}</MenuItem>)}</TextField>
        <FieldGrid>
          <TextField fullWidth required select label="Tipo identificacion" value={v.identificationType} onChange={(e) => set({ identificationType: Number(e.target.value) })}>{identificationOptions.map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}</TextField>
          <TextField
            fullWidth
            required
            label="Numero identificacion"
            value={v.identificationNumber}
            onChange={(e) => set({ identificationNumber: e.target.value })}
            InputProps={{ endAdornment: <IdentificationLookupAdornment identification={v.identificationNumber} /> }}
          />
        </FieldGrid>
        <FieldGrid>
          <TextField fullWidth label="Fecha nacimiento" type="date" value={v.birthDate} onChange={(e) => set({ birthDate: e.target.value })} InputLabelProps={{ shrink: true }} />
          <TextField fullWidth required label="Celular / WhatsApp" value={v.mobile} onChange={(e) => set({ mobile: e.target.value })} />
        </FieldGrid>
        <FieldGrid>
          <TextField fullWidth label="Direccion" value={v.address} onChange={(e) => set({ address: e.target.value })} />
          <TextField fullWidth label="Ciudad" value={v.city} onChange={(e) => set({ city: e.target.value })} />
        </FieldGrid>
        <TextField label="Ocupacion" value={v.occupation} onChange={(e) => set({ occupation: e.target.value })} />
        <FieldGrid>
          <TextField fullWidth label="Ingresos mensuales" type="number" value={v.monthlyIncome} onChange={(e) => set({ monthlyIncome: Number(e.target.value) })} />
          <TextField fullWidth label="Cuota inicial" type="number" value={v.downPayment} onChange={(e) => set({ downPayment: Number(e.target.value) })} />
        </FieldGrid>
        <FieldGrid>
          <TextField fullWidth label="Plazo meses" type="number" value={v.termMonths} onChange={(e) => set({ termMonths: Number(e.target.value) })} />
          <TextField fullWidth label="Valor producto" type="number" value={v.motorcycleValue || selectedQuote?.productPrice || selectedProduct?.price || 0} onChange={(e) => set({ motorcycleValue: Number(e.target.value) })} />
        </FieldGrid>
        <SectionTitle title="Codeudor" />
        <FieldGrid>
          <TextField fullWidth label="Nombre codeudor" value={v.coDebtorName} onChange={(e) => set({ coDebtorName: e.target.value })} />
          <TextField fullWidth label="Identificacion codeudor" value={v.coDebtorIdentification} onChange={(e) => set({ coDebtorIdentification: e.target.value })} />
        </FieldGrid>
        <FieldGrid>
          <TextField fullWidth label="Celular codeudor" value={v.coDebtorMobile} onChange={(e) => set({ coDebtorMobile: e.target.value })} />
          <TextField fullWidth label="Parentesco / relacion" value={v.coDebtorRelationship} onChange={(e) => set({ coDebtorRelationship: e.target.value })} />
        </FieldGrid>
        <TextField label="Ingresos mensuales codeudor" type="number" value={v.coDebtorMonthlyIncome} onChange={(e) => set({ coDebtorMonthlyIncome: Number(e.target.value) })} />
        <SectionTitle title="Referencias personales" />
        <FieldGrid columns={3}>
          <TextField fullWidth label="Referencia 1" value={v.reference1Name} onChange={(e) => set({ reference1Name: e.target.value })} />
          <TextField fullWidth label="Celular referencia 1" value={v.reference1Mobile} onChange={(e) => set({ reference1Mobile: e.target.value })} />
          <TextField fullWidth label="Relacion referencia 1" value={v.reference1Relationship} onChange={(e) => set({ reference1Relationship: e.target.value })} />
        </FieldGrid>
        <FieldGrid columns={3}>
          <TextField fullWidth label="Referencia 2" value={v.reference2Name} onChange={(e) => set({ reference2Name: e.target.value })} />
          <TextField fullWidth label="Celular referencia 2" value={v.reference2Mobile} onChange={(e) => set({ reference2Mobile: e.target.value })} />
          <TextField fullWidth label="Relacion referencia 2" value={v.reference2Relationship} onChange={(e) => set({ reference2Relationship: e.target.value })} />
        </FieldGrid>
        <TextField select label="Negocio pipeline" value={v.dealId} onChange={(e) => set({ dealId: e.target.value })}><MenuItem value="">Sin negocio</MenuItem>{deals.map((x) => <MenuItem key={x.id} value={x.id}>{x.title}</MenuItem>)}</TextField>
        <TextField select label="Estado" value={v.status} onChange={(e) => set({ status: Number(e.target.value) })}>{creditStatusOptions.map((x) => <MenuItem key={x} value={x}>{creditStatus(x)}</MenuItem>)}</TextField>
        <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} multiline minRows={2} />
        {selectedCustomer && <Alert severity="info">Cliente seleccionado: {selectedCustomer.firstNames || selectedCustomer.name} {selectedCustomer.lastNames}</Alert>}
      </>;
    }}
  </FormDialog>;
}

function DocumentSummary({ application, onUpdate, onUpload, onDownload }: {
  application: CreditApplication;
  onUpdate: (application: CreditApplication, document: CreditDocument, status: number) => Promise<void>;
  onUpload: (application: CreditApplication, document: CreditDocument, file: File) => Promise<void>;
  onDownload: (application: CreditApplication, document: CreditDocument) => Promise<void>;
}) {
  return <Stack spacing={.75} sx={{ minWidth: 360, maxWidth: 390 }}>
    {application.documents.map((document) => <Stack key={document.id} direction="row" alignItems="center" justifyContent="space-between" gap={1} sx={{ width: '100%' }}>
      <Stack spacing={.25} sx={{ minWidth: 170, maxWidth: 190 }}>
        <Chip size="small" label={`${document.name}: ${documentStatus(document.status)}`} color={document.status === 3 ? 'success' : document.status === 4 ? 'error' : undefined} variant={document.status === 1 ? 'outlined' : 'filled'} />
        {document.hasFile && <Typography variant="caption" color="text.secondary" noWrap>{document.fileName}</Typography>}
      </Stack>
      <Stack direction="row" alignItems="center" gap={.5} sx={{ flexShrink: 0 }}>
        <Tooltip title="Subir documento">
          <IconButton component="label" size="small" color={document.hasFile ? 'success' : 'primary'}>
            <UploadFile fontSize="small" />
            <input hidden type="file" accept=".pdf,.jpg,.jpeg,.png,.webp,image/*" onChange={(e) => {
              const file = e.target.files?.[0];
              if (file) onUpload(application, document, file);
              e.currentTarget.value = '';
            }} />
          </IconButton>
        </Tooltip>
        {document.hasFile && <Tooltip title="Descargar documento">
          <IconButton size="small" onClick={() => onDownload(application, document)}>
            <Download fontSize="small" />
          </IconButton>
        </Tooltip>}
        <TextField select size="small" value={document.status} onChange={(e) => onUpdate(application, document, Number(e.target.value))} sx={{ width: 126 }}>
          {[1, 2, 3, 4].map((status) => <MenuItem key={status} value={status}>{documentStatus(status)}</MenuItem>)}
        </TextField>
      </Stack>
    </Stack>)}
  </Stack>;
}

function LeadDialog({ form, onClose, onSave }: DialogProps<Lead, typeof emptyLead>) {
  const initial = form.item ? {
    firstNames: form.item.firstNames || form.item.name,
    lastNames: form.item.lastNames ?? '',
    firstName: form.item.firstName || (form.item.firstNames || form.item.name).split(' ')[0] || '',
    middleName: form.item.middleName ?? (form.item.firstNames || '').split(' ').slice(1).join(' '),
    lastName: form.item.lastName || (form.item.lastNames || '').split(' ')[0] || '',
    secondLastName: form.item.secondLastName ?? (form.item.lastNames || '').split(' ').slice(1).join(' '),
    email: form.item.email,
    phone: form.item.phone ?? '',
    source: form.item.source,
    rating: form.item.rating
  } : emptyLead;
  return <FormDialog title={form.item ? 'Editar prospecto' : 'Nuevo prospecto'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <FieldGrid>
        <TextField fullWidth required label="Primer nombre" value={v.firstName} onChange={(e) => set({ firstName: e.target.value, firstNames: fullFirstNames(e.target.value, v.middleName) })} />
        <TextField fullWidth label="Segundo nombre" value={v.middleName} onChange={(e) => set({ middleName: e.target.value, firstNames: fullFirstNames(v.firstName, e.target.value) })} />
      </FieldGrid>
      <FieldGrid>
        <TextField fullWidth required label="Primer apellido" value={v.lastName} onChange={(e) => set({ lastName: e.target.value, lastNames: fullLastNames(e.target.value, v.secondLastName) })} />
        <TextField fullWidth label="Segundo apellido" value={v.secondLastName} onChange={(e) => set({ secondLastName: e.target.value, lastNames: fullLastNames(v.lastName, e.target.value) })} />
      </FieldGrid>
      <TextField required label="Email" value={v.email} onChange={(e) => set({ email: e.target.value })} />
      <TextField label="Telefono" value={v.phone} onChange={(e) => set({ phone: e.target.value })} />
      <TextField required label="Fuente" value={v.source} onChange={(e) => set({ source: e.target.value })} />
      <TextField select label="Calificacion" value={v.rating} onChange={(e) => set({ rating: Number(e.target.value) })}>{[1, 2, 3].map((x) => <MenuItem key={x} value={x}>{ratingLabel(x)}</MenuItem>)}</TextField>
    </>}
  </FormDialog>;
}

function DealDialog({ form, stages, customers, defaultStageId, onClose, onSave }: DialogProps<Deal, typeof emptyDeal> & { stages: DealStage[]; customers: Customer[]; defaultStageId: string }) {
  const initial = form.item ? {
    title: form.item.title,
    customerId: form.item.customerId ?? '',
    stageId: form.item.stageId,
    value: form.item.value,
    closeProbability: form.item.closeProbability,
    estimatedCloseDate: form.item.estimatedCloseDate.slice(0, 10),
    status: form.item.status
  } : { ...emptyDeal, stageId: defaultStageId };
  return <FormDialog title={form.item ? 'Editar venta' : 'Nueva venta'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <TextField required label="Cliente y producto" placeholder="Juan Perez - AKT NKD 125 a credito" value={v.title} onChange={(e) => set({ title: e.target.value })} />
      <TextField select label="Cliente" value={v.customerId} onChange={(e) => set({ customerId: e.target.value })}><MenuItem value="">Sin cliente</MenuItem>{customers.map((x) => <MenuItem key={x.id} value={x.id}>{x.name}</MenuItem>)}</TextField>
      <TextField required select label="Etapa" value={v.stageId} onChange={(e) => set({ stageId: e.target.value })}>{stages.map((x) => <MenuItem key={x.id} value={x.id}>{x.name}</MenuItem>)}</TextField>
      <TextField label="Valor del producto / credito" type="number" value={v.value} onChange={(e) => set({ value: Number(e.target.value) })} />
      <TextField label="Probabilidad" type="number" value={v.closeProbability} onChange={(e) => set({ closeProbability: Number(e.target.value) })} />
      <TextField label="Fecha estimada" type="date" value={v.estimatedCloseDate} onChange={(e) => set({ estimatedCloseDate: e.target.value })} InputLabelProps={{ shrink: true }} />
      <TextField select label="Estado" value={v.status} onChange={(e) => set({ status: Number(e.target.value) })}>{[1, 2, 3].map((x) => <MenuItem key={x} value={x}>{dealStatus(x)}</MenuItem>)}</TextField>
    </>}
  </FormDialog>;
}

function StageDialog({ form, onClose, onSave }: DialogProps<DealStage, { name: string; order: number; defaultProbability: number; active: boolean }>) {
  const initial = form.item ? { name: form.item.name, order: form.item.order, defaultProbability: form.item.defaultProbability, active: form.item.active } : { name: '', order: 1, defaultProbability: 10, active: true };
  return <FormDialog title={form.item ? 'Editar etapa' : 'Nueva etapa'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <TextField required label="Nombre" value={v.name} onChange={(e) => set({ name: e.target.value })} />
      <TextField label="Orden" type="number" value={v.order} onChange={(e) => set({ order: Number(e.target.value) })} />
      <TextField label="Probabilidad pred." type="number" value={v.defaultProbability} onChange={(e) => set({ defaultProbability: Number(e.target.value) })} />
      <TextField select label="Activa" value={String(v.active)} onChange={(e) => set({ active: e.target.value === 'true' })}><MenuItem value="true">Si</MenuItem><MenuItem value="false">No</MenuItem></TextField>
    </>}
  </FormDialog>;
}

function ActivityDialog({ form, customers, deals, onClose, onSave }: DialogProps<Activity, typeof emptyActivity> & { customers: Customer[]; deals: Deal[] }) {
  const initial = form.item ? {
    title: form.item.title,
    description: form.item.description ?? '',
    type: form.item.type,
    status: form.item.status,
    scheduledAt: toInputDateTime(form.item.scheduledAt),
    reminderAt: form.item.reminderAt ? toInputDateTime(form.item.reminderAt) : '',
    customerId: form.item.customerId ?? '',
    dealId: form.item.dealId ?? '',
    assignedUserId: form.item.assignedUserId ?? ''
  } : emptyActivity;
  return <FormDialog title={form.item ? 'Editar actividad' : 'Nueva actividad'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <TextField required label="Titulo" value={v.title} onChange={(e) => set({ title: e.target.value })} />
      <TextField label="Descripcion" value={v.description} onChange={(e) => set({ description: e.target.value })} multiline minRows={2} />
      <TextField select label="Tipo" value={v.type} onChange={(e) => set({ type: Number(e.target.value) })}>{[1, 2, 3].map((x) => <MenuItem key={x} value={x}>{typeLabel(x)}</MenuItem>)}</TextField>
      <TextField select label="Estado" value={v.status} onChange={(e) => set({ status: Number(e.target.value) })}>{[1, 2, 3, 4].map((x) => <MenuItem key={x} value={x}>{activityStatus(x)}</MenuItem>)}</TextField>
      <TextField label="Fecha programada" type="datetime-local" value={v.scheduledAt} onChange={(e) => set({ scheduledAt: e.target.value })} InputLabelProps={{ shrink: true }} />
      <TextField label="Recordatorio" type="datetime-local" value={v.reminderAt} onChange={(e) => set({ reminderAt: e.target.value })} InputLabelProps={{ shrink: true }} />
      <TextField select label="Cliente" value={v.customerId} onChange={(e) => set({ customerId: e.target.value })}><MenuItem value="">Sin cliente</MenuItem>{customers.map((x) => <MenuItem key={x.id} value={x.id}>{x.name}</MenuItem>)}</TextField>
      <TextField select label="Negocio" value={v.dealId} onChange={(e) => set({ dealId: e.target.value })}><MenuItem value="">Sin negocio</MenuItem>{deals.map((x) => <MenuItem key={x.id} value={x.id}>{x.title}</MenuItem>)}</TextField>
    </>}
  </FormDialog>;
}

type DialogProps<TItem, TPayload> = { form: FormMode<TItem>; onClose: () => void; onSave: (payload: TPayload) => Promise<void> };

function FieldGrid({ children, columns = 2 }: { children: ReactNode; columns?: 2 | 3 }) {
  return <Box sx={{
    display: 'grid',
    gridTemplateColumns: { xs: '1fr', sm: `repeat(${columns}, minmax(0, 1fr))` },
    gap: 2,
    width: '100%'
  }}>{children}</Box>;
}

function SectionTitle({ title }: { title: string }) {
  return <Typography variant="subtitle2" fontWeight={900} color="primary" sx={{ mt: 1 }}>{title}</Typography>;
}

function FormDialog<T extends Record<string, unknown>>({ title, open, initial, children, onClose, onSave }: { title: string; open: boolean; initial: T; children: (value: T, set: (patch: Partial<T>) => void) => ReactNode; onClose: () => void; onSave: (payload: T) => Promise<void> }) {
  const muiTheme = useTheme();
  const fullScreen = useMediaQuery(muiTheme.breakpoints.down('sm'));
  const [value, setValue] = useState(initial);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (open) {
      setValue(initial);
      setError('');
    }
  }, [open, JSON.stringify(initial)]);

  const save = async () => {
    setSaving(true);
    setError('');
    try {
      await onSave(value);
    } catch (err) {
      setError(apiError(err));
    } finally {
      setSaving(false);
    }
  };

  return <Dialog open={open} onClose={saving ? undefined : onClose} fullWidth maxWidth="sm" fullScreen={fullScreen}>
    <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 1 }}>{title}<IconButton onClick={onClose}><Close /></IconButton></DialogTitle>
    <DialogContent sx={{ px: { xs: 2, sm: 3 } }}><Stack spacing={2} sx={{ pt: 1 }}>{error && <Alert severity="error">{error}</Alert>}{children(value, (patch) => setValue((prev) => ({ ...prev, ...patch })))}</Stack></DialogContent>
    <DialogActions sx={{ px: { xs: 2, sm: 3 }, pb: { xs: 2, sm: 1 }, flexWrap: 'wrap' }}><Button onClick={onClose} disabled={saving}>Cancelar</Button><Button variant="contained" onClick={save} disabled={saving}>{saving ? 'Guardando...' : 'Guardar'}</Button></DialogActions>
  </Dialog>;
}

function ConfirmDialog({ open, title, text, onClose, onConfirm, confirmLabel = 'Eliminar' }: { open: boolean; title: string; text: string; onClose: () => void; onConfirm: () => Promise<void>; confirmLabel?: string }) {
  const muiTheme = useTheme();
  const fullScreen = useMediaQuery(muiTheme.breakpoints.down('sm'));
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const confirm = async () => {
    setLoading(true);
    setError('');
    try {
      await onConfirm();
    } catch (err) {
      setError(apiError(err));
    } finally {
      setLoading(false);
    }
  };
  return <Dialog open={open} onClose={loading ? undefined : onClose} fullWidth maxWidth="xs" fullScreen={fullScreen}>
    <DialogTitle>{title}</DialogTitle>
    <DialogContent><Stack spacing={2}>{error && <Alert severity="error">{error}</Alert>}<Typography>{text}</Typography></Stack></DialogContent>
    <DialogActions sx={{ flexWrap: 'wrap' }}><Button onClick={onClose} disabled={loading}>Cancelar</Button><Button color="error" variant="contained" onClick={confirm} disabled={loading}>{confirmLabel}</Button></DialogActions>
  </Dialog>;
}

function Header({ title, action, onAction, onRefresh, secondaryAction }: { title: string; action?: string; onAction?: () => void; onRefresh?: () => void; secondaryAction?: { label: string; onClick: () => void } }) {
  return <Stack direction={{ xs: 'column', sm: 'row' }} alignItems={{ xs: 'stretch', sm: 'center' }} justifyContent="space-between" gap={1.5}>
    <Typography variant="h4" fontWeight={900} sx={{ fontSize: { xs: 26, sm: 34 }, lineHeight: 1.15 }}>{title}</Typography>
    <Stack direction={{ xs: 'column', sm: 'row' }} gap={1} sx={{ width: { xs: '100%', sm: 'auto' } }}>
      {onRefresh && <Button fullWidth={false} onClick={onRefresh}>Actualizar</Button>}
      {secondaryAction && <Button variant="outlined" onClick={secondaryAction.onClick}>{secondaryAction.label}</Button>}
      {action && <Button variant="contained" startIcon={<Add />} onClick={onAction}>{action}</Button>}
    </Stack>
  </Stack>;
}

function EntityTable({ headers, rows, empty }: { headers: string[]; rows: ReactNode[][]; empty: string }) {
  return <Card sx={{ width: '100%', overflow: 'hidden' }}>
    <TableContainer sx={{ width: '100%', overflowX: 'auto' }}>
      <Table size="small" sx={{ minWidth: tableMinWidth(headers), tableLayout: 'fixed' }}>
        <TableHead><TableRow>{headers.map((h) => <TableCell key={h} sx={{ ...tableColumnSx(h), whiteSpace: 'nowrap', fontWeight: 800 }}>{h}</TableCell>)}</TableRow></TableHead>
        <TableBody>{rows.length ? rows.map((row, i) => <TableRow key={i}>{row.map((c, j) => <TableCell key={j} sx={{ ...tableColumnSx(headers[j]), verticalAlign: 'top' }}>{c ?? '-'}</TableCell>)}</TableRow>) : <TableRow><TableCell colSpan={headers.length}><EmptyState text={empty} /></TableCell></TableRow>}</TableBody>
      </Table>
    </TableContainer>
  </Card>;
}

function ReportTable({ headers, rows, empty }: { headers: string[]; rows: ReactNode[][]; empty: string }) {
  return <TableContainer sx={{ width: '100%', overflowX: 'auto' }}>
    <Table size="small" sx={{ minWidth: 520 }}>
      <TableHead><TableRow>{headers.map((h) => <TableCell key={h} sx={{ whiteSpace: 'nowrap', fontWeight: 800 }}>{h}</TableCell>)}</TableRow></TableHead>
      <TableBody>{rows.length ? rows.map((row, i) => <TableRow key={i}>{row.map((c, j) => <TableCell key={j} sx={{ verticalAlign: 'top' }}>{c ?? '-'}</TableCell>)}</TableRow>) : <TableRow><TableCell colSpan={headers.length}><EmptyState text={empty} /></TableCell></TableRow>}</TableBody>
    </Table>
  </TableContainer>;
}

function tableMinWidth(headers: string[]) {
  return headers.includes('Plantillas') ? 1760 : 760;
}

function tableColumnSx(header: string) {
  const widths: Record<string, number> = {
    Identificacion: 150,
    Numero: 150,
    Cliente: 170,
    Producto: 220,
    Ciudad: 150,
    Estado: 150,
    Ingresos: 130,
    Codeudor: 170,
    Referencias: 210,
    Documentos: 410,
    Aprobacion: 220,
    Plantillas: 170,
    Acciones: 230
  };
  return {
    width: widths[header] ?? 180,
    minWidth: widths[header] ?? 180,
    maxWidth: widths[header] ?? 260,
    overflow: 'hidden',
    textOverflow: 'ellipsis'
  };
}

function AiAnalysisDialog({ analysis, onClose }: { analysis?: CustomerAiAnalysis; onClose: () => void }) {
  const [copied, setCopied] = useState(false);
  const copyMessage = async () => {
    if (!analysis?.whatsappMessage) return;
    await navigator.clipboard?.writeText(analysis.whatsappMessage).catch(() => undefined);
    setCopied(true);
    setTimeout(() => setCopied(false), 1800);
  };
  return <Dialog open={!!analysis} onClose={onClose} fullWidth maxWidth="md">
    <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 1 }}>
      Analisis comercial con IA
      <IconButton onClick={onClose}><Close /></IconButton>
    </DialogTitle>
    <DialogContent>
      {analysis && <Stack spacing={2} sx={{ pt: 1 }}>
        <Alert severity={analysis.riskLevel === 'Alto' ? 'error' : analysis.riskLevel === 'Medio' ? 'warning' : 'success'}>
          Prioridad {analysis.priority} - Riesgo {analysis.riskLevel}
        </Alert>
        <Box>
          <SectionTitle title="Resumen del caso" />
          <Typography>{analysis.summary}</Typography>
        </Box>
        <Box>
          <SectionTitle title="Pendientes" />
          <Stack component="ul" sx={{ pl: 2, my: .5 }}>{analysis.pendingItems.map((item) => <Typography component="li" key={item}>{item}</Typography>)}</Stack>
        </Box>
        <Box>
          <SectionTitle title="Siguiente mejor accion" />
          <Typography>{analysis.nextBestAction}</Typography>
        </Box>
        <Box>
          <SectionTitle title="Mensaje sugerido para WhatsApp" />
          <Paper variant="outlined" sx={{ p: 2, bgcolor: '#f8fafc' }}>
            <Typography>{analysis.whatsappMessage}</Typography>
          </Paper>
        </Box>
        <Box>
          <SectionTitle title="Senales usadas" />
          <Stack component="ul" sx={{ pl: 2, my: .5 }}>{analysis.signals.map((item) => <Typography component="li" key={item}>{item}</Typography>)}</Stack>
        </Box>
      </Stack>}
    </DialogContent>
    <DialogActions sx={{ flexWrap: 'wrap' }}>
      <Button onClick={onClose}>Cerrar</Button>
      <Button variant="contained" startIcon={<WhatsApp />} onClick={copyMessage}>{copied ? 'Copiado' : 'Copiar mensaje'}</Button>
    </DialogActions>
  </Dialog>;
}

function Actions({ onView, onEdit, onDelete, onConvert, onDownload, onActivity, onWhatsapp, onAi, onStart, onComplete, onReschedule, onCancel, compact }: { onView?: () => void; onEdit?: () => void; onDelete?: () => void; onConvert?: () => void; onDownload?: () => void; onActivity?: () => void; onWhatsapp?: () => void; onAi?: () => void; onStart?: () => void; onComplete?: () => void; onReschedule?: () => void; onCancel?: () => void; compact?: boolean }) {
  return <Stack direction="row" gap={compact ? .5 : 1} sx={{ mt: compact ? 1 : 0, flexWrap: 'wrap' }}>
    {onView && <Tooltip title="Ver cliente 360"><IconButton size="small" onClick={onView}><Visibility fontSize="small" /></IconButton></Tooltip>}
    {onAi && <Tooltip title="Analizar con IA"><IconButton size="small" color="primary" onClick={onAi}><AutoAwesome fontSize="small" /></IconButton></Tooltip>}
    {onWhatsapp && <Tooltip title="Abrir WhatsApp"><IconButton size="small" onClick={onWhatsapp}><WhatsApp fontSize="small" /></IconButton></Tooltip>}
    {onActivity && <Tooltip title="Registrar actividad"><IconButton size="small" onClick={onActivity}><AddTask fontSize="small" /></IconButton></Tooltip>}
    {onStart && <Tooltip title="Marcar en proceso"><IconButton size="small" onClick={onStart}><SyncAlt fontSize="small" /></IconButton></Tooltip>}
    {onComplete && <Tooltip title="Completar"><IconButton size="small" color="success" onClick={onComplete}><CheckCircle fontSize="small" /></IconButton></Tooltip>}
    {onReschedule && <Tooltip title="Reprogramar para manana"><IconButton size="small" onClick={onReschedule}><EventNote fontSize="small" /></IconButton></Tooltip>}
    {onCancel && <Tooltip title="Cancelar"><IconButton size="small" color="warning" onClick={onCancel}><Close fontSize="small" /></IconButton></Tooltip>}
    {onEdit && <Tooltip title="Editar"><IconButton size="small" onClick={onEdit}><Edit fontSize="small" /></IconButton></Tooltip>}
    {onConvert && <Tooltip title="Convertir a cliente"><IconButton size="small" onClick={onConvert}><SyncAlt fontSize="small" /></IconButton></Tooltip>}
    {onDownload && <Tooltip title="Descargar PDF"><IconButton size="small" onClick={onDownload}><Download fontSize="small" /></IconButton></Tooltip>}
    {onDelete && <Tooltip title="Eliminar"><IconButton size="small" color="error" onClick={onDelete}><Delete fontSize="small" /></IconButton></Tooltip>}
  </Stack>;
}

function Metric({ label, value }: { label: string; value: ReactNode }) {
  return <Card sx={{ height: '100%' }}><CardContent><Typography color="text.secondary" fontSize={13}>{label}</Typography><Typography variant="h5" fontWeight={900} sx={{ overflowWrap: 'anywhere' }}>{value}</Typography></CardContent></Card>;
}

function Row({ primary, secondary }: { primary: string; secondary: string }) {
  return <Stack direction={{ xs: 'column', sm: 'row' }} gap={.5} justifyContent="space-between" sx={{ py: 1, borderBottom: '1px solid #edf1f5' }}><Typography sx={{ overflowWrap: 'anywhere' }}>{primary}</Typography><Typography color="text.secondary" sx={{ flexShrink: 0 }}>{secondary}</Typography></Stack>;
}

function EmptyState({ text }: { text: string }) {
  return <Typography color="text.secondary" sx={{ py: 3, textAlign: 'center' }}>{text}</Typography>;
}

function StatusBar({ loading, error }: { loading?: boolean; error?: string }) {
  return <>{loading && <LinearProgress />}{error && <Alert severity="error">{error}</Alert>}</>;
}

function StatusChip({ label, tone }: { label: string; tone: 'success' | 'warning' | 'error' | 'default' }) {
  return <Chip size="small" label={label} color={tone === 'default' ? undefined : tone} variant={tone === 'default' ? 'outlined' : 'filled'} />;
}

function Notice({ notice, onClose }: { notice?: Notice; onClose: () => void }) {
  return <Snackbar open={!!notice} autoHideDuration={3600} onClose={onClose} anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}>{notice ? <Alert severity={notice.type} onClose={onClose}>{notice.text}</Alert> : undefined}</Snackbar>;
}

function useResource<T>(url: string, fallback?: T) {
  const [data, setData] = useState<T | undefined>(fallback);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const reload = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await api.get<T>(url);
      setData(response.data);
    } catch (err) {
      setError(apiError(err));
    } finally {
      setLoading(false);
    }
  }, [url]);
  useEffect(() => { reload(); }, [reload]);
  return { data, setData, loading, error, reload };
}

function useCanManage() {
  const roles = useAuthStore((s) => s.user?.roles ?? []);
  return useMemo(() => roles.includes('Administrador') || roles.includes('Supervisor'), [roles]);
}

function apiError(err: unknown) {
  const axiosError = err as AxiosError<{ detail?: string; title?: string }>;
  if (axiosError.response?.status === 403) return 'No tienes permisos para realizar esta accion.';
  if (axiosError.response?.data?.detail) return axiosError.response.data.detail;
  if (axiosError.response?.data?.title) return axiosError.response.data.title;
  if (axiosError.message) return axiosError.message;
  return 'Ocurrio un error inesperado.';
}

function toInputDateTime(value: string) {
  const date = new Date(value);
  const offset = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 16);
}

function toActivityPayload(payload: typeof emptyActivity | Activity) {
  return {
    ...payload,
    type: Number(payload.type),
    status: Number(payload.status),
    customerId: payload.customerId || null,
    dealId: payload.dealId || null,
    assignedUserId: payload.assignedUserId || null,
    scheduledAt: new Date(payload.scheduledAt).toISOString(),
    reminderAt: payload.reminderAt ? new Date(payload.reminderAt).toISOString() : null
  };
}

function money(value?: number) { return new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(value ?? 0); }
function percent(value?: number) { return `${new Intl.NumberFormat('es-CO', { maximumFractionDigits: 2 }).format(value ?? 0)}%`; }
function identificationDigits(value?: string) { return (value ?? '').replace(/\D/g, ''); }
async function openExternalLookup(url: string, identification?: string) {
  const digits = identificationDigits(identification);
  if (digits && navigator.clipboard?.writeText) await navigator.clipboard.writeText(digits).catch(() => undefined);
  window.open(url, '_blank', 'noopener,noreferrer');
}
function readFileAsDataUrl(file: File) {
  return new Promise<string>((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(String(reader.result ?? ''));
    reader.onerror = () => reject(new Error('No se pudo leer el archivo.'));
    reader.readAsDataURL(file);
  });
}
function loadImage(dataUrl: string) {
  return new Promise<HTMLImageElement>((resolve, reject) => {
    const image = new Image();
    image.onload = () => resolve(image);
    image.onerror = () => reject(new Error('El archivo no parece ser una imagen valida.'));
    image.src = dataUrl;
  });
}
async function normalizeCompanyLogo(file: File) {
  if (!['image/png', 'image/jpeg', 'image/webp'].includes(file.type)) {
    throw new Error('El logo debe estar en formato PNG, JPG o WebP.');
  }
  if (file.size > companyLogoMaxBytes) {
    throw new Error('El logo no puede superar 1 MB.');
  }

  const image = await loadImage(await readFileAsDataUrl(file));
  const canvas = document.createElement('canvas');
  canvas.width = companyLogoWidth;
  canvas.height = companyLogoHeight;
  const context = canvas.getContext('2d');
  if (!context) throw new Error('No se pudo preparar el logo.');

  context.clearRect(0, 0, companyLogoWidth, companyLogoHeight);
  const scale = Math.min(companyLogoWidth / image.width, companyLogoHeight / image.height);
  const width = Math.round(image.width * scale);
  const height = Math.round(image.height * scale);
  const x = Math.round((companyLogoWidth - width) / 2);
  const y = Math.round((companyLogoHeight - height) / 2);
  context.drawImage(image, x, y, width, height);
  return canvas.toDataURL('image/png');
}
function IdentificationLookupAdornment({ identification }: { identification?: string }) {
  const digits = identificationDigits(identification);
  return <InputAdornment position="end">
    <Stack direction="row" spacing={.5}>
      <Button size="small" variant="outlined" disabled={!digits} onClick={() => void openExternalLookup(simitUrl, digits)}>Simit</Button>
      <Button size="small" variant="outlined" disabled={!digits} onClick={() => void openExternalLookup(runtUrl, digits)}>Runt</Button>
    </Stack>
  </InputAdornment>;
}
function timelineColor(tone: CustomerTimelineItem['tone']) {
  if (tone === 'success') return '#15803d';
  if (tone === 'warning') return '#b45309';
  if (tone === 'error') return '#b91c1c';
  if (tone === 'info') return '#155e75';
  return '#64748b';
}
function timelineChipColor(tone: CustomerTimelineItem['tone']) {
  return tone === 'success' || tone === 'warning' || tone === 'error' ? tone : undefined;
}
function estimateMonthlyPayment(financedAmount: number, termMonths: number, monthlyInterestRate: number) {
  if (financedAmount <= 0) return 0;
  const rate = monthlyInterestRate / 100;
  const payment = rate === 0
    ? financedAmount / termMonths
    : financedAmount * rate / (1 - Math.pow(1 + rate, -termMonths));
  return Math.round(payment);
}
function whatsappUrl(value: string) {
  const digits = value.replace(/\D/g, '');
  const withCountry = digits.startsWith('57') ? digits : `57${digits}`;
  return `https://wa.me/${withCountry}`;
}
function activityDueState(activity: Activity) {
  if (activity.status === 3 || activity.status === 4) return 'done';
  const scheduled = new Date(activity.scheduledAt);
  const start = new Date();
  start.setHours(0, 0, 0, 0);
  const end = new Date(start);
  end.setDate(end.getDate() + 1);
  if (scheduled < start) return 'overdue';
  if (scheduled < end) return 'today';
  return 'upcoming';
}
function activityDueLabel(activity: Activity) {
  const state = activityDueState(activity);
  if (state === 'overdue') return 'Vencida';
  if (state === 'today') return 'Para hoy';
  if (state === 'done') return activity.status === 3 ? 'Completada' : 'Cancelada';
  return 'Proxima';
}
function activityTone(activity: Activity): 'success' | 'warning' | 'error' | 'default' {
  const state = activityDueState(activity);
  if (activity.status === 3) return 'success';
  if (activity.status === 4 || state === 'overdue') return 'error';
  if (state === 'today' || activity.status === 2) return 'warning';
  return 'default';
}
function alertSeverityTone(severity?: string): 'success' | 'warning' | 'error' | 'default' {
  if (severity === 'error') return 'error';
  if (severity === 'warning') return 'warning';
  if (severity === 'success') return 'success';
  return 'default';
}
function productName(product: Product) {
  return product.name?.trim() || [product.brand, product.model, product.reference].filter(Boolean).join(' ').trim() || 'Producto';
}
function readableFileSize(bytes: number) {
  if (!Number.isFinite(bytes) || bytes <= 0) return '0 KB';
  if (bytes < 1024 * 1024) return `${Math.ceil(bytes / 1024)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}
function addDaysIso(value: string, days: number) {
  const date = new Date(value);
  date.setDate(date.getDate() + days);
  return date.toISOString();
}
function statusLabel(value: number) { return ['-', 'Activo', 'Inactivo', 'Suspendido'][value] ?? 'Activo'; }
function ratingLabel(value: number) { return ['-', 'Frio', 'Tibio', 'Caliente'][value] ?? 'Frio'; }
function typeLabel(value: number) { return ['-', 'Tarea', 'Llamada', 'Reunion'][value] ?? 'Tarea'; }
function activityStatus(value: number) { return ['-', 'Pendiente', 'En proceso', 'Completada', 'Cancelada'][value] ?? 'Pendiente'; }
function dealStatus(value: number) { return ['-', 'Abierto', 'Ganado', 'Perdido'][value] ?? 'Abierto'; }
function dealStatusForStage(stageName: string) {
  const normalized = stageName.trim().toLowerCase();
  if (normalized.includes('entregado')) return 2;
  if (normalized.includes('rechazado') || normalized.includes('desistido') || normalized.includes('perdido')) return 3;
  return 1;
}
function creditStatus(value: number) {
  return ({
    1: 'Cotizado',
    2: 'Documentos pendientes',
    3: 'Credito en estudio',
    4: 'Credito en estudio',
    5: 'Aprobado',
    6: 'Rechazado',
    7: 'Entregado',
    8: 'Interesado',
    9: 'Desistido'
  } as Record<number, string>)[value] ?? 'Cotizado';
}
function creditTone(value: number): 'success' | 'warning' | 'error' | 'default' {
  if (value === 5 || value === 7) return 'success';
  if (value === 6 || value === 9) return 'error';
  if (value === 2 || value === 3 || value === 4) return 'warning';
  return 'default';
}
function documentStatus(value: number) { return ['-', 'Pendiente', 'Recibido', 'Validado', 'Rechazado'][value] ?? 'Pendiente'; }
function deliveryStatus(value: number) { return ['-', 'Programada', 'Entregada', 'Cancelada'][value] ?? 'Programada'; }
const identificationOptions = [
  { value: 1, label: 'Cedula de ciudadania' },
  { value: 2, label: 'Cedula de extranjeria' },
  { value: 3, label: 'NIT' },
  { value: 4, label: 'Pasaporte' },
  { value: 5, label: 'Tarjeta de identidad' },
  { value: 6, label: 'Permiso por proteccion temporal' }
];
function identificationLabel(value: number) { return identificationOptions.find((x) => x.value === value)?.label ?? 'Identificacion'; }

export default function App() {
  const token = useAuthStore((s) => s.accessToken);
  return <ThemeProvider theme={theme}><CssBaseline /><Routes><Route path="/login" element={token ? <Navigate to="/" /> : <LoginPage />} /><Route path="/*" element={token ? <Layout /> : <Navigate to="/login" />} /></Routes></ThemeProvider>;
}

