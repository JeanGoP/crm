import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { Navigate, NavLink, Route, Routes, useNavigate } from 'react-router-dom';
import {
  Alert, AppBar, Box, Button, Card, CardContent, Chip, CssBaseline, Dialog, DialogActions,
  DialogContent, DialogTitle, Divider, Drawer, Grid, IconButton, LinearProgress, MenuItem,
  Paper, Snackbar, Stack, Table, TableBody, TableCell, TableHead, TableRow, TextField,
  ThemeProvider, Toolbar, Tooltip, Typography, createTheme
} from '@mui/material';
import DashboardIcon from '@mui/icons-material/Dashboard';
import Groups from '@mui/icons-material/Groups';
import Handshake from '@mui/icons-material/Handshake';
import ViewKanban from '@mui/icons-material/ViewKanban';
import EventNote from '@mui/icons-material/EventNote';
import Settings from '@mui/icons-material/Settings';
import Logout from '@mui/icons-material/Logout';
import Add from '@mui/icons-material/Add';
import CheckCircle from '@mui/icons-material/CheckCircle';
import Edit from '@mui/icons-material/Edit';
import Delete from '@mui/icons-material/Delete';
import SyncAlt from '@mui/icons-material/SyncAlt';
import Close from '@mui/icons-material/Close';
import Inventory2 from '@mui/icons-material/Inventory2';
import ReceiptLong from '@mui/icons-material/ReceiptLong';
import Download from '@mui/icons-material/Download';
import Assignment from '@mui/icons-material/Assignment';
import { AxiosError } from 'axios';
import { api } from './api';
import { useAuthStore } from './store';
import { Activity, Company, CreditApplication, CreditDocument, Customer, Dashboard, Deal, DealStage, Lead, Product, Quote, User } from './types';

const drawerWidth = 248;
const today = new Date().toISOString().slice(0, 10);

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

const nav = [
  { to: '/', label: 'Dashboard', icon: <DashboardIcon /> },
  { to: '/clientes', label: 'Clientes', icon: <Groups /> },
  { to: '/productos', label: 'Productos', icon: <Inventory2 /> },
  { to: '/cotizaciones', label: 'Cotizaciones', icon: <ReceiptLong /> },
  { to: '/solicitudes-credito', label: 'Solicitudes credito', icon: <Assignment /> },
  { to: '/prospectos', label: 'Prospectos', icon: <Handshake /> },
  { to: '/pipeline', label: 'Pipeline', icon: <ViewKanban /> },
  { to: '/actividades', label: 'Actividades', icon: <EventNote /> },
  { to: '/configuracion', label: 'Configuracion', icon: <Settings /> }
];

type Notice = { type: 'success' | 'error' | 'info'; text: string };
type FormMode<T> = { open: boolean; item?: T };

const emptyCustomer = { firstNames: '', lastNames: '', companyName: '', email: '', phone: '', status: 1, tags: '' };
const emptyLead = { firstNames: '', lastNames: '', email: '', phone: '', source: 'Web', rating: 1 };
const emptyDeal = { title: '', customerId: '', stageId: '', value: 0, closeProbability: 10, estimatedCloseDate: today, status: 1 };
const emptyActivity = { title: '', description: '', type: 1, status: 1, scheduledAt: `${today}T09:00`, reminderAt: '', customerId: '', dealId: '', assignedUserId: '' };
const emptyCompany = { name: '', subdomain: '', customDomain: '', active: true };
const emptyUser = { fullName: '', email: '', password: '', companyId: '', roles: ['Vendedor'] };
const emptyProduct = { brand: '', model: '', reference: '', engineCc: '', year: '', color: '', price: 0, active: true };
const emptyQuote = { identificationType: 1, identificationNumber: '', customerFirstNames: '', customerLastNames: '', productId: '', notes: '' };
const emptyCreditApplication = { customerId: '', productId: '', quoteId: '', dealId: '', identificationType: 1, identificationNumber: '', birthDate: '', mobile: '', address: '', city: '', occupation: '', monthlyIncome: 0, downPayment: 0, termMonths: 24, motorcycleValue: 0, status: 1, notes: '' };

function Layout() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <Drawer variant="permanent" PaperProps={{ sx: { width: drawerWidth, borderRight: '1px solid #dde5ee' } }}>
        <Toolbar sx={{ px: 3 }}>
          <Typography variant="h6" fontWeight={800}>CRM SaaS</Typography>
        </Toolbar>
        <Divider />
        <Stack sx={{ p: 1 }}>
          {nav.map((item) => (
            <Button key={item.to} component={NavLink} to={item.to} startIcon={item.icon} sx={{ justifyContent: 'flex-start', my: .25 }}>
              {item.label}
            </Button>
          ))}
        </Stack>
      </Drawer>
      <Box sx={{ flex: 1, ml: `${drawerWidth}px` }}>
        <AppBar position="sticky" color="inherit" elevation={0} sx={{ borderBottom: '1px solid #dde5ee' }}>
          <Toolbar sx={{ justifyContent: 'space-between' }}>
            <Stack>
              <Typography fontWeight={800}>{user?.fullName ?? 'Equipo comercial'}</Typography>
              <Typography color="text.secondary" fontSize={13}>{user?.roles.join(', ')}</Typography>
            </Stack>
            <Tooltip title="Salir">
              <IconButton aria-label="Salir" onClick={() => { logout(); navigate('/login'); }}><Logout /></IconButton>
            </Tooltip>
          </Toolbar>
        </AppBar>
        <Box component="main" sx={{ p: 3 }}>
          <Routes>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/clientes" element={<CustomersPage />} />
            <Route path="/productos" element={<ProductsPage />} />
            <Route path="/cotizaciones" element={<QuotesPage />} />
            <Route path="/solicitudes-credito" element={<CreditApplicationsPage />} />
            <Route path="/prospectos" element={<LeadsPage />} />
            <Route path="/pipeline" element={<PipelinePage />} />
            <Route path="/actividades" element={<ActivitiesPage />} />
            <Route path="/configuracion" element={<SettingsPage />} />
          </Routes>
        </Box>
      </Box>
    </Box>
  );
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
  const cards: { label: string; value: ReactNode }[] = [
    { label: 'Pipeline abierto', value: money(data?.openPipelineValue) },
    { label: 'Pipeline ponderado', value: money(data?.weightedPipelineValue) },
    { label: 'Clientes activos', value: data?.activeCustomers ?? 0 },
    { label: 'Prospectos abiertos', value: data?.openLeads ?? 0 },
    { label: 'Actividades pendientes', value: data?.pendingActivities ?? 0 }
  ];
  return <Stack spacing={3}>
    <Header title="Dashboard" onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <Grid container spacing={2}>{cards.map((card) => <Grid item xs={12} md={2.4} key={card.label}><Metric label={card.label} value={card.value} /></Grid>)}</Grid>
    <Card><CardContent><Typography variant="h6">Actividad reciente</Typography>{data?.recentActivities?.length ? data.recentActivities.map((a) => <Row key={`${a.title}${a.scheduledAt}`} primary={a.title} secondary={new Date(a.scheduledAt).toLocaleString()} />) : <EmptyState text="Sin actividad reciente" />}</CardContent></Card>
  </Stack>;
}

function CustomersPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<Customer[]>('/api/customers', []);
  const [form, setForm] = useState<FormMode<Customer>>({ open: false });
  const [confirm, setConfirm] = useState<Customer>();
  const [notice, setNotice] = useState<Notice>();
  const canDelete = useCanManage();

  const save = async (payload: typeof emptyCustomer) => {
    const body = { ...payload, status: Number(payload.status) };
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
      headers={['Nombres', 'Apellidos', 'Email', 'Telefono', 'Estado', 'Etiquetas', 'Acciones']}
      empty="No hay clientes registrados"
      rows={rows.map((r) => [
        r.firstNames || r.name,
        r.lastNames,
        r.email,
        r.phone,
        <StatusChip label={statusLabel(r.status)} tone={r.status === 1 ? 'success' : 'default'} />,
        r.tags,
        <Actions onEdit={() => setForm({ open: true, item: r })} onDelete={canDelete ? () => setConfirm(r) : undefined} />
      ])}
    />
    <CustomerDialog form={form} onClose={() => setForm({ open: false })} onSave={save} />
    <ConfirmDialog title="Eliminar cliente" text={`Se eliminara ${confirm?.name}.`} open={!!confirm} onClose={() => setConfirm(undefined)} onConfirm={remove} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
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
      engineCc: payload.engineCc === '' ? null : Number(payload.engineCc),
      year: payload.year === '' ? null : Number(payload.year),
      price: Number(payload.price),
      active: Boolean(payload.active)
    };
    const { data } = form.item
      ? await api.put<Product>(`/api/products/${form.item.id}`, body)
      : await api.post<Product>('/api/products', body);
    setData(form.item ? rows.map((x) => x.id === data.id ? data : x) : [data, ...rows]);
    setNotice({ type: 'success', text: form.item ? 'Moto actualizada.' : 'Moto creada.' });
    setForm({ open: false });
  };

  const remove = async () => {
    if (!confirm) return;
    const { data } = await api.delete<Product>(`/api/products/${confirm.id}`);
    setData(rows.map((x) => x.id === data.id ? data : x));
    setNotice({ type: 'success', text: 'Moto inactivada.' });
    setConfirm(undefined);
  };

  return <Stack spacing={3}>
    <Header title="Productos / motos" action={canManage ? 'Nueva moto' : undefined} onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Marca', 'Modelo', 'Referencia', 'Cilindraje', 'Ano', 'Color', 'Precio', 'Estado', 'Acciones']}
      empty="No hay motos registradas"
      rows={rows.map((r) => [
        r.brand,
        r.model,
        r.reference,
        r.engineCc ? `${r.engineCc} cc` : undefined,
        r.year,
        r.color,
        money(r.price),
        <StatusChip label={r.active ? 'Activa' : 'Inactiva'} tone={r.active ? 'success' : 'default'} />,
        <Actions onEdit={canManage ? () => setForm({ open: true, item: r }) : undefined} onDelete={canManage && r.active ? () => setConfirm(r) : undefined} />
      ])}
    />
    <ProductDialog form={form} onClose={() => setForm({ open: false })} onSave={save} />
    <ConfirmDialog title="Inactivar moto" text={`Se inactivara ${confirm?.brand} ${confirm?.model}. Las cotizaciones existentes conservaran el historial.`} open={!!confirm} onClose={() => setConfirm(undefined)} onConfirm={remove} confirmLabel="Inactivar" />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function QuotesPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<Quote[]>('/api/quotes', []);
  const { data: products = [] } = useResource<Product[]>('/api/products', []);
  const [form, setForm] = useState<FormMode<Quote>>({ open: false });
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
      identificationType: Number(payload.identificationType),
      identificationNumber: payload.identificationNumber || null,
      notes: payload.notes || null
    };
    const { data } = await api.post<Quote>('/api/quotes', body);
    setData([data, ...rows]);
    setNotice({ type: 'success', text: 'Cotizacion creada. El cliente quedo registrado para completar sus datos.' });
    setForm({ open: false });
    await downloadPdf(data);
  };

  return <Stack spacing={3}>
    <Header title="Cotizaciones" action="Nueva cotizacion" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Numero', 'Cliente', 'Identificacion', 'Moto', 'Valor', 'Valida hasta', 'PDF']}
      empty="No hay cotizaciones registradas"
      rows={rows.map((r) => [
        r.number,
        `${r.customerFirstNames} ${r.customerLastNames}`.trim(),
        `${identificationLabel(r.identificationType)} ${r.identificationNumber ?? ''}`.trim(),
        r.productName,
        money(r.productPrice),
        new Date(r.validUntil).toLocaleDateString(),
        <Actions onDownload={() => downloadPdf(r)} />
      ])}
    />
    <QuoteDialog form={form} products={products.filter((x) => x.active)} onClose={() => setForm({ open: false })} onSave={save} />
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

  return <Stack spacing={3}>
    <Header title="Solicitudes de credito" action="Nueva solicitud" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Numero', 'Cliente', 'Moto', 'Estado', 'Ingresos', 'Cuota inicial', 'Plazo', 'Documentos', 'Acciones']}
      empty="No hay solicitudes de credito"
      rows={rows.map((r) => [
        r.number,
        r.customerName,
        r.productName,
        <StatusChip label={creditStatus(r.status)} tone={r.status === 5 || r.status === 7 ? 'success' : r.status === 6 ? 'error' : r.status === 4 ? 'warning' : 'default'} />,
        money(r.monthlyIncome),
        money(r.downPayment),
        `${r.termMonths} meses`,
        <DocumentSummary application={r} onUpdate={updateDocument} />,
        <Stack direction="row" gap={1} alignItems="center">
          <Actions onEdit={() => setForm({ open: true, item: r })} />
          <TextField select size="small" label="Estado" value={r.status} onChange={(e) => changeStatus(r, Number(e.target.value))} sx={{ minWidth: 160 }}>
            {[1, 2, 3, 4, 5, 6, 7].map((x) => <MenuItem key={x} value={x}>{creditStatus(x)}</MenuItem>)}
          </TextField>
        </Stack>
      ])}
    />
    <CreditApplicationDialog form={form} customers={customers} products={products.filter((x) => x.active)} quotes={quotes} deals={deals} onClose={() => setForm({ open: false })} onSave={save} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function LeadsPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<Lead[]>('/api/leads', []);
  const [form, setForm] = useState<FormMode<Lead>>({ open: false });
  const [confirm, setConfirm] = useState<Lead>();
  const [notice, setNotice] = useState<Notice>();
  const canDelete = useCanManage();

  const save = async (payload: typeof emptyLead) => {
    const body = { ...payload, rating: Number(payload.rating) };
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
      headers={['Nombres', 'Apellidos', 'Email', 'Telefono', 'Fuente', 'Calificacion', 'Estado', 'Acciones']}
      empty="No hay prospectos registrados"
      rows={rows.map((r) => [
        r.firstNames || r.name,
        r.lastNames,
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
  const [confirm, setConfirm] = useState<Deal>();
  const [notice, setNotice] = useState<Notice>();
  const canManage = useCanManage();

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

  const defaultStageId = stages[0]?.id ?? '';
  return <Stack spacing={3}>
    <Header title="Pipeline de motos a credito" action="Nueva venta" onAction={() => setForm({ open: true })} onRefresh={() => { reloadStages(); reloadDeals(); }} secondaryAction={canManage ? { label: 'Nueva etapa', onClick: () => setStageForm({ open: true }) } : undefined} />
    <StatusBar loading={loadingStages || loadingDeals} error={stagesError || dealsError} />
    <Box className="kanban">{stages.map((stage) => <Paper className="kanbanColumn" key={stage.id}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Stack>
          <Typography fontWeight={800}>{stage.name}</Typography>
          <Typography color="text.secondary" fontSize={12}>{stage.defaultProbability}% pred.</Typography>
        </Stack>
        {canManage && <Tooltip title="Editar etapa"><IconButton size="small" onClick={() => setStageForm({ open: true, item: stage })}><Edit fontSize="small" /></IconButton></Tooltip>}
      </Stack>
      {deals.filter((d) => d.stageId === stage.id).map((deal) => <Card key={deal.id} sx={{ mt: 1 }}>
        <CardContent>
          <Stack direction="row" justifyContent="space-between" gap={1}>
            <Typography fontWeight={800}>{deal.title}</Typography>
            <StatusChip label={dealStatus(deal.status)} tone={deal.status === 2 ? 'success' : deal.status === 3 ? 'error' : 'default'} />
          </Stack>
          <Typography color="text.secondary">{money(deal.value)}</Typography>
          <LinearProgress variant="determinate" value={deal.closeProbability} sx={{ mt: 1 }} />
          <Actions onEdit={() => setForm({ open: true, item: deal })} onDelete={canManage ? () => setConfirm(deal) : undefined} compact />
        </CardContent>
      </Card>)}
    </Paper>)}</Box>
    <DealDialog form={form} stages={stages} customers={customers} defaultStageId={defaultStageId} onClose={() => setForm({ open: false })} onSave={saveDeal} />
    <StageDialog form={stageForm} onClose={() => setStageForm({ open: false })} onSave={saveStage} />
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
  const canDelete = useCanManage();

  const save = async (payload: typeof emptyActivity) => {
    const body = {
      ...payload,
      type: Number(payload.type),
      status: Number(payload.status),
      customerId: payload.customerId || null,
      dealId: payload.dealId || null,
      assignedUserId: payload.assignedUserId || null,
      scheduledAt: new Date(payload.scheduledAt).toISOString(),
      reminderAt: payload.reminderAt ? new Date(payload.reminderAt).toISOString() : null
    };
    const { data } = form.item
      ? await api.put<Activity>(`/api/activities/${form.item.id}`, body)
      : await api.post<Activity>('/api/activities', body);
    setData(form.item ? rows.map((x) => x.id === data.id ? data : x) : [data, ...rows]);
    setNotice({ type: 'success', text: form.item ? 'Actividad actualizada.' : 'Actividad creada.' });
    setForm({ open: false });
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
    <EntityTable
      headers={['Titulo', 'Tipo', 'Estado', 'Fecha', 'Recordatorio', 'Acciones']}
      empty="No hay actividades registradas"
      rows={rows.map((r) => [
        r.title,
        typeLabel(r.type),
        <StatusChip label={activityStatus(r.status)} tone={r.status === 3 ? 'success' : r.status === 1 ? 'warning' : 'default'} />,
        new Date(r.scheduledAt).toLocaleString(),
        r.reminderAt ? new Date(r.reminderAt).toLocaleString() : undefined,
        <Actions onEdit={() => setForm({ open: true, item: r })} onDelete={canDelete ? () => setConfirm(r) : undefined} />
      ])}
    />
    <ActivityDialog form={form} customers={customers} deals={deals} onClose={() => setForm({ open: false })} onSave={save} />
    <ConfirmDialog title="Eliminar actividad" text={`Se eliminara ${confirm?.title}.`} open={!!confirm} onClose={() => setConfirm(undefined)} onConfirm={remove} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function SettingsPage() {
  const user = useAuthStore((s) => s.user);
  const canManage = useCanManage();
  const { data: companies = [], loading: loadingCompanies, error: companiesError, reload: reloadCompanies, setData: setCompanies } = useResource<Company[]>('/api/companies', []);
  const { data: users = [], loading: loadingUsers, error: usersError, reload: reloadUsers, setData: setUsers } = useResource<User[]>('/api/users', []);
  const [companyForm, setCompanyForm] = useState<FormMode<Company>>({ open: false });
  const [userForm, setUserForm] = useState<FormMode<User>>({ open: false });
  const [notice, setNotice] = useState<Notice>();

  const saveCompany = async (payload: typeof emptyCompany) => {
    const body = {
      name: payload.name,
      subdomain: payload.subdomain,
      customDomain: payload.customDomain || null,
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

  return <Stack spacing={3}>
    <Header title="Configuracion" onRefresh={() => { reloadCompanies(); reloadUsers(); }} />
    <Card><CardContent><Grid container spacing={2}>
      <Grid item xs={12} md={6}><TextField fullWidth label="API URL" value={import.meta.env.VITE_API_URL ?? ''} InputProps={{ readOnly: true }} /></Grid>
      <Grid item xs={12} md={6}><TextField fullWidth label="Tenant" value={import.meta.env.VITE_TENANT ?? 'demo'} InputProps={{ readOnly: true }} /></Grid>
      <Grid item xs={12}><Chip icon={<CheckCircle />} label={`Sesion activa: ${user?.email} (${user?.roles.join(', ')})`} /></Grid>
    </Grid></CardContent></Card>
    {canManage && <>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h5" fontWeight={900}>Empresas</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => setCompanyForm({ open: true })}>Nueva empresa</Button>
      </Stack>
      <StatusBar loading={loadingCompanies} error={companiesError} />
      <EntityTable
        headers={['Nombre', 'Subdominio', 'Dominio', 'Estado', 'Acciones']}
        empty="No hay empresas registradas"
        rows={companies.map((c) => [
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
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function CompanyDialog({ form, onClose, onSave }: DialogProps<Company, typeof emptyCompany>) {
  const initial = form.item ? { name: form.item.name, subdomain: form.item.subdomain, customDomain: form.item.customDomain ?? '', active: form.item.active } : emptyCompany;
  return <FormDialog title={form.item ? 'Editar empresa' : 'Nueva empresa'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <TextField required label="Nombre" value={v.name} onChange={(e) => set({ name: e.target.value })} />
      <TextField required label="Subdominio" value={v.subdomain} onChange={(e) => set({ subdomain: e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, '') })} />
      <TextField label="Dominio personalizado" value={v.customDomain} onChange={(e) => set({ customDomain: e.target.value })} />
      <TextField select label="Estado" value={String(v.active)} onChange={(e) => set({ active: e.target.value === 'true' })}><MenuItem value="true">Activa</MenuItem><MenuItem value="false">Inactiva</MenuItem></TextField>
    </>}
  </FormDialog>;
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

function CustomerDialog({ form, onClose, onSave }: DialogProps<Customer, typeof emptyCustomer>) {
  const initial = form.item ? { firstNames: form.item.firstNames || form.item.name, lastNames: form.item.lastNames ?? '', companyName: form.item.companyName ?? '', email: form.item.email, phone: form.item.phone ?? '', status: form.item.status, tags: form.item.tags ?? '' } : emptyCustomer;
  return <FormDialog title={form.item ? 'Editar cliente' : 'Nuevo cliente'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <TextField required label="Nombres" value={v.firstNames} onChange={(e) => set({ firstNames: e.target.value })} />
      <TextField required label="Apellidos" value={v.lastNames} onChange={(e) => set({ lastNames: e.target.value })} />
      <TextField required label="Email" value={v.email} onChange={(e) => set({ email: e.target.value })} />
      <TextField label="Telefono" value={v.phone} onChange={(e) => set({ phone: e.target.value })} />
      <TextField select label="Estado" value={v.status} onChange={(e) => set({ status: Number(e.target.value) })}>{[1, 2, 3].map((x) => <MenuItem key={x} value={x}>{statusLabel(x)}</MenuItem>)}</TextField>
      <TextField label="Etiquetas" value={v.tags} onChange={(e) => set({ tags: e.target.value })} />
    </>}
  </FormDialog>;
}

function ProductDialog({ form, onClose, onSave }: DialogProps<Product, typeof emptyProduct>) {
  const initial = form.item ? {
    brand: form.item.brand,
    model: form.item.model,
    reference: form.item.reference,
    engineCc: form.item.engineCc?.toString() ?? '',
    year: form.item.year?.toString() ?? '',
    color: form.item.color ?? '',
    price: form.item.price,
    active: form.item.active
  } : emptyProduct;
  return <FormDialog title={form.item ? 'Editar moto' : 'Nueva moto'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <TextField required label="Marca" value={v.brand} onChange={(e) => set({ brand: e.target.value })} />
      <TextField required label="Modelo" value={v.model} onChange={(e) => set({ model: e.target.value })} />
      <TextField required label="Referencia" value={v.reference} onChange={(e) => set({ reference: e.target.value })} />
      <Grid container spacing={2}>
        <Grid item xs={12} sm={6}><TextField fullWidth label="Cilindraje" type="number" value={v.engineCc} onChange={(e) => set({ engineCc: e.target.value })} /></Grid>
        <Grid item xs={12} sm={6}><TextField fullWidth label="Ano" type="number" value={v.year} onChange={(e) => set({ year: e.target.value })} /></Grid>
      </Grid>
      <TextField label="Color" value={v.color} onChange={(e) => set({ color: e.target.value })} />
      <TextField required label="Precio" type="number" value={v.price} onChange={(e) => set({ price: Number(e.target.value) })} />
      <TextField select label="Estado" value={String(v.active)} onChange={(e) => set({ active: e.target.value === 'true' })}><MenuItem value="true">Activa</MenuItem><MenuItem value="false">Inactiva</MenuItem></TextField>
    </>}
  </FormDialog>;
}

function QuoteDialog({ form, products, onClose, onSave }: DialogProps<Quote, typeof emptyQuote> & { products: Product[] }) {
  const initial = { ...emptyQuote, productId: products[0]?.id ?? '' };
  return <FormDialog title="Nueva cotizacion" open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <TextField required select label="Tipo de identificacion" value={v.identificationType} onChange={(e) => set({ identificationType: Number(e.target.value) })}>
        {identificationOptions.map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}
      </TextField>
      <TextField label="Numero de identificacion" value={v.identificationNumber} onChange={(e) => set({ identificationNumber: e.target.value })} />
      <TextField required label="Nombres" value={v.customerFirstNames} onChange={(e) => set({ customerFirstNames: e.target.value })} />
      <TextField required label="Apellidos" value={v.customerLastNames} onChange={(e) => set({ customerLastNames: e.target.value })} />
      <TextField required select label="Moto" value={v.productId} onChange={(e) => set({ productId: e.target.value })}>
        {products.length ? products.map((product) => <MenuItem key={product.id} value={product.id}>{product.brand} {product.model} {product.reference} - {money(product.price)}</MenuItem>) : <MenuItem value="">No hay motos activas</MenuItem>}
      </TextField>
      <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} multiline minRows={2} />
    </>}
  </FormDialog>;
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
            motorcycleValue: selected?.productPrice ?? v.motorcycleValue
          });
        }}>
          <MenuItem value="">Sin cotizacion</MenuItem>
          {quotes.map((x) => <MenuItem key={x.id} value={x.id}>{x.number} - {x.customerFirstNames} {x.customerLastNames}</MenuItem>)}
        </TextField>
        <TextField required select label="Cliente" value={v.customerId} onChange={(e) => set({ customerId: e.target.value })}>{customers.map((x) => <MenuItem key={x.id} value={x.id}>{x.firstNames || x.name} {x.lastNames}</MenuItem>)}</TextField>
        <TextField required select label="Moto" value={v.productId} onChange={(e) => {
          const product = products.find((x) => x.id === e.target.value);
          set({ productId: e.target.value, motorcycleValue: product?.price ?? v.motorcycleValue });
        }}>{products.map((x) => <MenuItem key={x.id} value={x.id}>{x.brand} {x.model} {x.reference} - {money(x.price)}</MenuItem>)}</TextField>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}><TextField fullWidth required select label="Tipo identificacion" value={v.identificationType} onChange={(e) => set({ identificationType: Number(e.target.value) })}>{identificationOptions.map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}</TextField></Grid>
          <Grid item xs={12} sm={6}><TextField fullWidth required label="Numero identificacion" value={v.identificationNumber} onChange={(e) => set({ identificationNumber: e.target.value })} /></Grid>
        </Grid>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}><TextField fullWidth label="Fecha nacimiento" type="date" value={v.birthDate} onChange={(e) => set({ birthDate: e.target.value })} InputLabelProps={{ shrink: true }} /></Grid>
          <Grid item xs={12} sm={6}><TextField fullWidth required label="Celular / WhatsApp" value={v.mobile} onChange={(e) => set({ mobile: e.target.value })} /></Grid>
        </Grid>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={7}><TextField fullWidth label="Direccion" value={v.address} onChange={(e) => set({ address: e.target.value })} /></Grid>
          <Grid item xs={12} sm={5}><TextField fullWidth label="Ciudad" value={v.city} onChange={(e) => set({ city: e.target.value })} /></Grid>
        </Grid>
        <TextField label="Ocupacion" value={v.occupation} onChange={(e) => set({ occupation: e.target.value })} />
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}><TextField fullWidth label="Ingresos mensuales" type="number" value={v.monthlyIncome} onChange={(e) => set({ monthlyIncome: Number(e.target.value) })} /></Grid>
          <Grid item xs={12} sm={6}><TextField fullWidth label="Cuota inicial" type="number" value={v.downPayment} onChange={(e) => set({ downPayment: Number(e.target.value) })} /></Grid>
        </Grid>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}><TextField fullWidth label="Plazo meses" type="number" value={v.termMonths} onChange={(e) => set({ termMonths: Number(e.target.value) })} /></Grid>
          <Grid item xs={12} sm={6}><TextField fullWidth label="Valor moto" type="number" value={v.motorcycleValue || selectedQuote?.productPrice || selectedProduct?.price || 0} onChange={(e) => set({ motorcycleValue: Number(e.target.value) })} /></Grid>
        </Grid>
        <TextField select label="Negocio pipeline" value={v.dealId} onChange={(e) => set({ dealId: e.target.value })}><MenuItem value="">Sin negocio</MenuItem>{deals.map((x) => <MenuItem key={x.id} value={x.id}>{x.title}</MenuItem>)}</TextField>
        <TextField select label="Estado" value={v.status} onChange={(e) => set({ status: Number(e.target.value) })}>{[1, 2, 3, 4, 5, 6, 7].map((x) => <MenuItem key={x} value={x}>{creditStatus(x)}</MenuItem>)}</TextField>
        <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} multiline minRows={2} />
        {selectedCustomer && <Alert severity="info">Cliente seleccionado: {selectedCustomer.firstNames || selectedCustomer.name} {selectedCustomer.lastNames}</Alert>}
      </>;
    }}
  </FormDialog>;
}

function DocumentSummary({ application, onUpdate }: { application: CreditApplication; onUpdate: (application: CreditApplication, document: CreditDocument, status: number) => Promise<void> }) {
  return <Stack spacing={.75}>
    {application.documents.map((document) => <Stack key={document.id} direction="row" alignItems="center" justifyContent="space-between" gap={1}>
      <Chip size="small" label={`${document.name}: ${documentStatus(document.status)}`} color={document.status === 3 ? 'success' : document.status === 4 ? 'error' : undefined} variant={document.status === 1 ? 'outlined' : 'filled'} />
      <TextField select size="small" value={document.status} onChange={(e) => onUpdate(application, document, Number(e.target.value))} sx={{ width: 122 }}>
        {[1, 2, 3, 4].map((status) => <MenuItem key={status} value={status}>{documentStatus(status)}</MenuItem>)}
      </TextField>
    </Stack>)}
  </Stack>;
}

function LeadDialog({ form, onClose, onSave }: DialogProps<Lead, typeof emptyLead>) {
  const initial = form.item ? { firstNames: form.item.firstNames || form.item.name, lastNames: form.item.lastNames ?? '', email: form.item.email, phone: form.item.phone ?? '', source: form.item.source, rating: form.item.rating } : emptyLead;
  return <FormDialog title={form.item ? 'Editar prospecto' : 'Nuevo prospecto'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <TextField required label="Nombres" value={v.firstNames} onChange={(e) => set({ firstNames: e.target.value })} />
      <TextField required label="Apellidos" value={v.lastNames} onChange={(e) => set({ lastNames: e.target.value })} />
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
  return <FormDialog title={form.item ? 'Editar venta de moto' : 'Nueva venta de moto'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <TextField required label="Cliente y moto" placeholder="Juan Perez - AKT NKD 125 a credito" value={v.title} onChange={(e) => set({ title: e.target.value })} />
      <TextField select label="Cliente" value={v.customerId} onChange={(e) => set({ customerId: e.target.value })}><MenuItem value="">Sin cliente</MenuItem>{customers.map((x) => <MenuItem key={x.id} value={x.id}>{x.name}</MenuItem>)}</TextField>
      <TextField required select label="Etapa" value={v.stageId} onChange={(e) => set({ stageId: e.target.value })}>{stages.map((x) => <MenuItem key={x.id} value={x.id}>{x.name}</MenuItem>)}</TextField>
      <TextField label="Valor de la moto / credito" type="number" value={v.value} onChange={(e) => set({ value: Number(e.target.value) })} />
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

function FormDialog<T extends Record<string, unknown>>({ title, open, initial, children, onClose, onSave }: { title: string; open: boolean; initial: T; children: (value: T, set: (patch: Partial<T>) => void) => ReactNode; onClose: () => void; onSave: (payload: T) => Promise<void> }) {
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

  return <Dialog open={open} onClose={saving ? undefined : onClose} fullWidth maxWidth="sm">
    <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>{title}<IconButton onClick={onClose}><Close /></IconButton></DialogTitle>
    <DialogContent><Stack spacing={2} sx={{ pt: 1 }}>{error && <Alert severity="error">{error}</Alert>}{children(value, (patch) => setValue((prev) => ({ ...prev, ...patch })))}</Stack></DialogContent>
    <DialogActions><Button onClick={onClose} disabled={saving}>Cancelar</Button><Button variant="contained" onClick={save} disabled={saving}>{saving ? 'Guardando...' : 'Guardar'}</Button></DialogActions>
  </Dialog>;
}

function ConfirmDialog({ open, title, text, onClose, onConfirm, confirmLabel = 'Eliminar' }: { open: boolean; title: string; text: string; onClose: () => void; onConfirm: () => Promise<void>; confirmLabel?: string }) {
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
  return <Dialog open={open} onClose={loading ? undefined : onClose} fullWidth maxWidth="xs">
    <DialogTitle>{title}</DialogTitle>
    <DialogContent><Stack spacing={2}>{error && <Alert severity="error">{error}</Alert>}<Typography>{text}</Typography></Stack></DialogContent>
    <DialogActions><Button onClick={onClose} disabled={loading}>Cancelar</Button><Button color="error" variant="contained" onClick={confirm} disabled={loading}>{confirmLabel}</Button></DialogActions>
  </Dialog>;
}

function Header({ title, action, onAction, onRefresh, secondaryAction }: { title: string; action?: string; onAction?: () => void; onRefresh?: () => void; secondaryAction?: { label: string; onClick: () => void } }) {
  return <Stack direction="row" alignItems="center" justifyContent="space-between" gap={2}>
    <Typography variant="h4" fontWeight={900}>{title}</Typography>
    <Stack direction="row" gap={1}>{onRefresh && <Button onClick={onRefresh}>Actualizar</Button>}{secondaryAction && <Button variant="outlined" onClick={secondaryAction.onClick}>{secondaryAction.label}</Button>}{action && <Button variant="contained" startIcon={<Add />} onClick={onAction}>{action}</Button>}</Stack>
  </Stack>;
}

function EntityTable({ headers, rows, empty }: { headers: string[]; rows: ReactNode[][]; empty: string }) {
  return <Card><Table><TableHead><TableRow>{headers.map((h) => <TableCell key={h}>{h}</TableCell>)}</TableRow></TableHead><TableBody>{rows.length ? rows.map((row, i) => <TableRow key={i}>{row.map((c, j) => <TableCell key={j}>{c ?? '-'}</TableCell>)}</TableRow>) : <TableRow><TableCell colSpan={headers.length}><EmptyState text={empty} /></TableCell></TableRow>}</TableBody></Table></Card>;
}

function Actions({ onEdit, onDelete, onConvert, onDownload, compact }: { onEdit?: () => void; onDelete?: () => void; onConvert?: () => void; onDownload?: () => void; compact?: boolean }) {
  return <Stack direction="row" gap={compact ? .5 : 1} sx={{ mt: compact ? 1 : 0 }}>
    {onEdit && <Tooltip title="Editar"><IconButton size="small" onClick={onEdit}><Edit fontSize="small" /></IconButton></Tooltip>}
    {onConvert && <Tooltip title="Convertir a cliente"><IconButton size="small" onClick={onConvert}><SyncAlt fontSize="small" /></IconButton></Tooltip>}
    {onDownload && <Tooltip title="Descargar PDF"><IconButton size="small" onClick={onDownload}><Download fontSize="small" /></IconButton></Tooltip>}
    {onDelete && <Tooltip title="Eliminar"><IconButton size="small" color="error" onClick={onDelete}><Delete fontSize="small" /></IconButton></Tooltip>}
  </Stack>;
}

function Metric({ label, value }: { label: string; value: ReactNode }) {
  return <Card><CardContent><Typography color="text.secondary" fontSize={13}>{label}</Typography><Typography variant="h5" fontWeight={900}>{value}</Typography></CardContent></Card>;
}

function Row({ primary, secondary }: { primary: string; secondary: string }) {
  return <Stack direction="row" justifyContent="space-between" sx={{ py: 1, borderBottom: '1px solid #edf1f5' }}><Typography>{primary}</Typography><Typography color="text.secondary">{secondary}</Typography></Stack>;
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

function money(value?: number) { return new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(value ?? 0); }
function statusLabel(value: number) { return ['-', 'Activo', 'Inactivo', 'Suspendido'][value] ?? 'Activo'; }
function ratingLabel(value: number) { return ['-', 'Frio', 'Tibio', 'Caliente'][value] ?? 'Frio'; }
function typeLabel(value: number) { return ['-', 'Tarea', 'Llamada', 'Reunion'][value] ?? 'Tarea'; }
function activityStatus(value: number) { return ['-', 'Pendiente', 'En proceso', 'Completada', 'Cancelada'][value] ?? 'Pendiente'; }
function dealStatus(value: number) { return ['-', 'Abierto', 'Ganado', 'Perdido'][value] ?? 'Abierto'; }
function creditStatus(value: number) { return ['-', 'Borrador', 'Documentos pendientes', 'Documentos recibidos', 'En estudio', 'Aprobada', 'Rechazada', 'Desembolsada'][value] ?? 'Borrador'; }
function documentStatus(value: number) { return ['-', 'Pendiente', 'Recibido', 'Validado', 'Rechazado'][value] ?? 'Pendiente'; }
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
