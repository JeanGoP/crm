import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { Navigate, NavLink, Route, Routes, useNavigate, useParams } from 'react-router-dom';
import {
  Alert, AppBar, Box, Button, Card, CardContent, Chip, CssBaseline, Dialog, DialogActions,
  DialogContent, DialogTitle, Divider, Drawer, Grid, IconButton, LinearProgress, MenuItem,
  Paper, Snackbar, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField,
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
import Visibility from '@mui/icons-material/Visibility';
import AddTask from '@mui/icons-material/AddTask';
import WhatsApp from '@mui/icons-material/WhatsApp';
import { AxiosError } from 'axios';
import { api } from './api';
import { useAuthStore } from './store';
import { Activity, Company, CreditApplication, CreditDocument, Customer, Customer360, CustomerTimelineItem, Dashboard, Deal, DealStage, Lead, Product, Quote, User } from './types';

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
const emptyProduct = { name: '', category: 'Moto', brand: '', model: '', reference: '', description: '', engineCc: '', year: '', color: '', price: 0, active: true };
const emptyQuote = { identificationType: 1, identificationNumber: '', customerFirstNames: '', customerLastNames: '', productId: '', downPayment: 0, termMonths: 24, monthlyInterestRate: 2.2, notes: '' };
const emptyCreditApplication = { customerId: '', productId: '', quoteId: '', dealId: '', identificationType: 1, identificationNumber: '', birthDate: '', mobile: '', address: '', city: '', occupation: '', monthlyIncome: 0, downPayment: 0, termMonths: 24, motorcycleValue: 0, status: 1, notes: '' };

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
      {nav.map((item) => (
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
          <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 1 }}>
            <Typography variant="h6" fontWeight={900}>Alertas comerciales</Typography>
            <Chip size="small" label={`${data?.alerts?.length ?? 0} pendientes`} variant="outlined" />
          </Stack>
          {data?.alerts?.length ? data.alerts.map((alert) => <Stack key={`${alert.type}${alert.title}${alert.createdAt}`} direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" gap={1.5} sx={{ py: 1.25, borderBottom: '1px solid #edf1f5' }}>
            <Stack spacing={.5}>
              <Stack direction="row" gap={1} alignItems="center" flexWrap="wrap">
                <StatusChip label={alert.type} tone={alertSeverityTone(alert.severity)} />
                <Typography fontWeight={900}>{alert.title}</Typography>
              </Stack>
              <Typography color="text.secondary">{alert.description}</Typography>
              <Typography variant="caption" color="text.secondary">{new Date(alert.createdAt).toLocaleString()}</Typography>
            </Stack>
            {alert.actionUrl && <Button variant="outlined" onClick={() => navigate(alert.actionUrl!)}>Abrir</Button>}
          </Stack>) : <EmptyState text="Sin alertas comerciales" />}
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
  const [notice, setNotice] = useState<Notice>();

  const saveActivity = async (payload: typeof emptyActivity) => {
    await api.post<Activity>('/api/activities', toActivityPayload(payload));
    setNotice({ type: 'success', text: 'Seguimiento registrado.' });
    setActivityForm({ open: false });
    reload();
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
    {customer && <Grid container spacing={2}>
      <Grid item xs={12} md={3}><Metric label="Telefono / WhatsApp" value={customer.phone ? <Button size="small" startIcon={<WhatsApp />} href={whatsappUrl(customer.phone)} target="_blank" rel="noreferrer">{customer.phone}</Button> : '-'} /></Grid>
      <Grid item xs={12} md={3}><Metric label="Email" value={customer.email || '-'} /></Grid>
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
          {data?.quotes.length ? data.quotes.map((q) => <Row key={q.id} primary={`${q.number} - ${q.productName}`} secondary={`${money(q.productPrice)} - cuota ${money(q.estimatedMonthlyPayment)} x ${q.termMonths} - ${new Date(q.quoteDate).toLocaleDateString()}`} />) : <EmptyState text="Sin cotizaciones" />}
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
      headers={['Producto', 'Categoria', 'Marca', 'Referencia', 'Caracteristicas', 'Precio', 'Estado', 'Acciones']}
      empty="No hay productos registrados"
      rows={rows.map((r) => [
        productName(r),
        r.category,
        r.brand,
        r.reference,
        [r.model, r.engineCc ? `${r.engineCc} cc` : undefined, r.year, r.color].filter(Boolean).join(' / ') || r.description,
        money(r.price),
        <StatusChip label={r.active ? 'Activa' : 'Inactiva'} tone={r.active ? 'success' : 'default'} />,
        <Actions onEdit={canManage ? () => setForm({ open: true, item: r }) : undefined} onDelete={canManage && r.active ? () => setConfirm(r) : undefined} />
      ])}
    />
    <ProductDialog form={form} onClose={() => setForm({ open: false })} onSave={save} />
    <ConfirmDialog title="Inactivar producto" text={`Se inactivara ${confirm ? productName(confirm) : ''}. Las cotizaciones existentes conservaran el historial.`} open={!!confirm} onClose={() => setConfirm(undefined)} onConfirm={remove} confirmLabel="Inactivar" />
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
      downPayment: Number(payload.downPayment),
      termMonths: Number(payload.termMonths),
      monthlyInterestRate: Number(payload.monthlyInterestRate),
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
      headers={['Numero', 'Cliente', 'Identificacion', 'Producto', 'Valor', 'Cuota estimada', 'Valida hasta', 'PDF']}
      empty="No hay cotizaciones registradas"
      rows={rows.map((r) => [
        r.number,
        `${r.customerFirstNames} ${r.customerLastNames}`.trim(),
        `${identificationLabel(r.identificationType)} ${r.identificationNumber ?? ''}`.trim(),
        r.productName,
        money(r.productPrice),
        r.estimatedMonthlyPayment > 0 ? `${money(r.estimatedMonthlyPayment)} x ${r.termMonths}` : 'Sin simulacion',
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

  return <Stack spacing={3}>
    <Header title="Solicitudes de credito" action="Nueva solicitud" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Numero', 'Cliente', 'Producto', 'Estado', 'Ingresos', 'Cuota inicial', 'Plazo', 'Documentos', 'Aprobacion', 'Acciones']}
      empty="No hay solicitudes de credito"
      rows={rows.map((r) => [
        r.number,
        r.customerName,
        r.productName,
        <StatusChip label={creditStatus(r.status)} tone={r.status === 5 || r.status === 7 ? 'success' : r.status === 6 ? 'error' : r.status === 4 ? 'warning' : 'default'} />,
        money(r.monthlyIncome),
        money(r.downPayment),
        `${r.termMonths} meses`,
        <DocumentSummary application={r} onUpdate={updateDocument} onUpload={uploadDocument} onDownload={downloadDocument} />,
        <ApprovalSummary application={r} onDecision={decide} />,
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

function ApprovalSummary({ application, onDecision }: { application: CreditApplication; onDecision: (application: CreditApplication, status: number, notes?: string) => Promise<void> }) {
  const actions = [
    { status: 2, label: 'Enviar', show: application.status === 1 },
    { status: 4, label: 'Estudio', show: application.status === 3 },
    { status: 5, label: 'Aprobar', show: application.status === 4 },
    { status: 6, label: 'Rechazar', show: application.status === 4 || application.status === 5 },
    { status: 7, label: 'Desembolsar', show: application.status === 5 }
  ].filter((x) => x.show);
  const lastDate = application.disbursedAt ?? application.approvedAt ?? application.rejectedAt ?? application.reviewStartedAt ?? application.submittedAt;
  return <Stack spacing={.75} sx={{ minWidth: 190 }}>
    <Typography variant="caption" color="text.secondary">
      {lastDate ? `${application.decisionUser ?? 'Sistema'} - ${new Date(lastDate).toLocaleDateString()}` : 'Sin decision registrada'}
    </Typography>
    {application.decisionNotes && <Typography variant="caption" color="text.secondary" noWrap>{application.decisionNotes}</Typography>}
    <Stack direction="row" gap={.5} flexWrap="wrap">
      {actions.length ? actions.map((action) => <Button key={action.status} size="small" variant="outlined" onClick={() => onDecision(application, action.status)}>{action.label}</Button>) : <Chip size="small" label="Sin acciones" variant="outlined" />}
    </Stack>
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
  const [activityForm, setActivityForm] = useState<FormMode<Activity>>({ open: false });
  const [confirm, setConfirm] = useState<Deal>();
  const [notice, setNotice] = useState<Notice>();
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

  const defaultStageId = stages[0]?.id ?? '';
  return <Stack spacing={3}>
    <Header title="Pipeline de ventas a credito" action="Nueva venta" onAction={() => setForm({ open: true })} onRefresh={() => { reloadStages(); reloadDeals(); }} secondaryAction={canManage ? { label: 'Nueva etapa', onClick: () => setStageForm({ open: true }) } : undefined} />
    <StatusBar loading={loadingStages || loadingDeals} error={stagesError || dealsError} />
    <Box className="kanban">{stages.map((stage) => <Paper className="kanbanColumn" key={stage.id}>
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
        return <Card key={deal.id} sx={{ mt: 1 }}>
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
    <DealDialog form={form} stages={stages} customers={customers} defaultStageId={defaultStageId} onClose={() => setForm({ open: false })} onSave={saveDeal} />
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
    {(v, set) => {
      const selectedProduct = products.find((product) => product.id === v.productId);
      const productPrice = selectedProduct?.price ?? 0;
      const downPayment = Math.min(Number(v.downPayment) || 0, productPrice);
      const termMonths = Math.max(Number(v.termMonths) || 1, 1);
      const monthlyInterestRate = Math.max(Number(v.monthlyInterestRate) || 0, 0);
      const financedAmount = Math.max(productPrice - downPayment, 0);
      const monthlyPayment = estimateMonthlyPayment(financedAmount, termMonths, monthlyInterestRate);
      const totalPayment = downPayment + monthlyPayment * termMonths;
      return <>
        <TextField required select label="Tipo de identificacion" value={v.identificationType} onChange={(e) => set({ identificationType: Number(e.target.value) })}>
          {identificationOptions.map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}
        </TextField>
        <TextField label="Numero de identificacion" value={v.identificationNumber} onChange={(e) => set({ identificationNumber: e.target.value })} />
        <TextField required label="Nombres" value={v.customerFirstNames} onChange={(e) => set({ customerFirstNames: e.target.value })} />
        <TextField required label="Apellidos" value={v.customerLastNames} onChange={(e) => set({ customerLastNames: e.target.value })} />
        <TextField required select label="Producto" value={v.productId} onChange={(e) => set({ productId: e.target.value })}>
          {products.length ? products.map((product) => <MenuItem key={product.id} value={product.id}>{productName(product)} ({product.category}) - {money(product.price)}</MenuItem>) : <MenuItem value="">No hay productos activos</MenuItem>}
        </TextField>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={4}><TextField fullWidth label="Cuota inicial" type="number" value={v.downPayment} onChange={(e) => set({ downPayment: Number(e.target.value) })} /></Grid>
          <Grid item xs={12} sm={4}><TextField fullWidth label="Plazo meses" type="number" value={v.termMonths} onChange={(e) => set({ termMonths: Number(e.target.value) })} /></Grid>
          <Grid item xs={12} sm={4}><TextField fullWidth label="Tasa mensual %" type="number" value={v.monthlyInterestRate} onChange={(e) => set({ monthlyInterestRate: Number(e.target.value) })} /></Grid>
        </Grid>
        <Paper variant="outlined" sx={{ p: 2, bgcolor: '#f8fafc' }}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={4}><Typography variant="caption" color="text.secondary">Valor producto</Typography><Typography fontWeight={700}>{money(productPrice)}</Typography></Grid>
            <Grid item xs={12} sm={4}><Typography variant="caption" color="text.secondary">Valor financiado</Typography><Typography fontWeight={700}>{money(financedAmount)}</Typography></Grid>
            <Grid item xs={12} sm={4}><Typography variant="caption" color="text.secondary">Cuota estimada</Typography><Typography fontWeight={700}>{money(monthlyPayment)}</Typography></Grid>
            <Grid item xs={12}><Typography variant="caption" color="text.secondary">Total estimado a pagar: {money(totalPayment)}</Typography></Grid>
          </Grid>
        </Paper>
        <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} multiline minRows={2} />
      </>;
    }}
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
            motorcycleValue: selected?.productPrice ?? v.motorcycleValue,
            downPayment: selected?.downPayment ?? v.downPayment,
            termMonths: selected?.termMonths ?? v.termMonths
          });
        }}>
          <MenuItem value="">Sin cotizacion</MenuItem>
          {quotes.map((x) => <MenuItem key={x.id} value={x.id}>{x.number} - {x.customerFirstNames} {x.customerLastNames}</MenuItem>)}
        </TextField>
        <TextField required select label="Cliente" value={v.customerId} onChange={(e) => set({ customerId: e.target.value })}>{customers.map((x) => <MenuItem key={x.id} value={x.id}>{x.firstNames || x.name} {x.lastNames}</MenuItem>)}</TextField>
        <TextField required select label="Producto principal" value={v.productId} onChange={(e) => {
          const product = products.find((x) => x.id === e.target.value);
          set({ productId: e.target.value, motorcycleValue: product?.price ?? v.motorcycleValue });
        }}>{products.map((x) => <MenuItem key={x.id} value={x.id}>{productName(x)} ({x.category}) - {money(x.price)}</MenuItem>)}</TextField>
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
          <Grid item xs={12} sm={6}><TextField fullWidth label="Valor producto" type="number" value={v.motorcycleValue || selectedQuote?.productPrice || selectedProduct?.price || 0} onChange={(e) => set({ motorcycleValue: Number(e.target.value) })} /></Grid>
        </Grid>
        <TextField select label="Negocio pipeline" value={v.dealId} onChange={(e) => set({ dealId: e.target.value })}><MenuItem value="">Sin negocio</MenuItem>{deals.map((x) => <MenuItem key={x.id} value={x.id}>{x.title}</MenuItem>)}</TextField>
        <TextField select label="Estado" value={v.status} onChange={(e) => set({ status: Number(e.target.value) })}>{[1, 2, 3, 4, 5, 6, 7].map((x) => <MenuItem key={x} value={x}>{creditStatus(x)}</MenuItem>)}</TextField>
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
  return <Stack spacing={.75}>
    {application.documents.map((document) => <Stack key={document.id} direction="row" alignItems="center" justifyContent="space-between" gap={1}>
      <Stack spacing={.25} sx={{ minWidth: 180 }}>
        <Chip size="small" label={`${document.name}: ${documentStatus(document.status)}`} color={document.status === 3 ? 'success' : document.status === 4 ? 'error' : undefined} variant={document.status === 1 ? 'outlined' : 'filled'} />
        {document.hasFile && <Typography variant="caption" color="text.secondary" noWrap>{document.fileName}</Typography>}
      </Stack>
      <Stack direction="row" alignItems="center" gap={.5}>
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
        <TextField select size="small" value={document.status} onChange={(e) => onUpdate(application, document, Number(e.target.value))} sx={{ width: 122 }}>
          {[1, 2, 3, 4].map((status) => <MenuItem key={status} value={status}>{documentStatus(status)}</MenuItem>)}
        </TextField>
      </Stack>
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
      <Table size="small" sx={{ minWidth: 760 }}>
        <TableHead><TableRow>{headers.map((h) => <TableCell key={h} sx={{ whiteSpace: 'nowrap', fontWeight: 800 }}>{h}</TableCell>)}</TableRow></TableHead>
        <TableBody>{rows.length ? rows.map((row, i) => <TableRow key={i}>{row.map((c, j) => <TableCell key={j} sx={{ verticalAlign: 'top', maxWidth: 260 }}>{c ?? '-'}</TableCell>)}</TableRow>) : <TableRow><TableCell colSpan={headers.length}><EmptyState text={empty} /></TableCell></TableRow>}</TableBody>
      </Table>
    </TableContainer>
  </Card>;
}

function Actions({ onView, onEdit, onDelete, onConvert, onDownload, onActivity, onWhatsapp, onStart, onComplete, onReschedule, onCancel, compact }: { onView?: () => void; onEdit?: () => void; onDelete?: () => void; onConvert?: () => void; onDownload?: () => void; onActivity?: () => void; onWhatsapp?: () => void; onStart?: () => void; onComplete?: () => void; onReschedule?: () => void; onCancel?: () => void; compact?: boolean }) {
  return <Stack direction="row" gap={compact ? .5 : 1} sx={{ mt: compact ? 1 : 0, flexWrap: 'wrap' }}>
    {onView && <Tooltip title="Ver cliente 360"><IconButton size="small" onClick={onView}><Visibility fontSize="small" /></IconButton></Tooltip>}
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

