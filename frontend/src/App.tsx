import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { Navigate, NavLink, Route, Routes, useNavigate, useParams } from 'react-router-dom';
import {
  Alert, AppBar, Box, Button, Card, CardContent, Checkbox, Chip, CssBaseline, Dialog, DialogActions,
  DialogContent, DialogTitle, Divider, Drawer, Grid, IconButton, LinearProgress, MenuItem,
  FormControlLabel, Paper, Snackbar, Stack, Tab, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Tabs, TextField, InputAdornment,
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
import Search from '@mui/icons-material/Search';
import { AxiosError } from 'axios';
import { api } from './api';
import { useAuthStore } from './store';
import { Activity, ColombianIdentityLookup, CollectionOrder, CommercialInventory, CommercialInventorySummary, CommercialReports, Company, CreditApplication, CreditDocument, Customer, Customer360, CustomerAiAnalysis, CustomerTimelineItem, Dashboard, Deal, DealStage, FinancialSettings, Lead, MotorcycleDelivery, Procedure, Product, ProductPhoto, Promotion, Quote, QuoteSimulationResult, RequirementProfile, SalesPoint, User } from './types';

const drawerWidth = 248;
const today = new Date().toISOString().slice(0, 10);
const currentMonthStart = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10);
const simitUrl = 'https://www.fcm.org.co/simit/#/home-public';
const runtUrl = 'https://portalpublico.runt.gov.co/#/consulta-ciudadano-documento/consulta/consulta-ciudadano-documento';
const companyLogoWidth = 320;
const companyLogoHeight = 160;
const companyLogoMaxBytes = 1_000_000;
const deliveryPhotoMaxBytes = 1_000_000;
const uiBorder = '#eaecf0';
const uiSurface = '#ffffff';
const uiMuted = '#667085';
const uiSidebar = '#101828';
const uiSidebarSoft = '#1d2939';
const uiPrimary = '#465fff';

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: uiPrimary, dark: '#3641d8' },
    secondary: { main: '#0e9384' },
    background: { default: '#f9fafb', paper: uiSurface },
    text: { primary: '#101828', secondary: uiMuted },
    divider: uiBorder,
    success: { main: '#15803d' },
    warning: { main: '#b45309' }
  },
  shape: { borderRadius: 12 },
  typography: { fontFamily: '"Inter", "Segoe UI", Arial, sans-serif', button: { textTransform: 'none', fontWeight: 800 } },
  components: {
    MuiCard: {
      styleOverrides: {
        root: {
          border: `1px solid ${uiBorder}`,
          boxShadow: '0 1px 3px rgba(16, 24, 40, .08)',
          backgroundImage: 'none'
        }
      }
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none'
        }
      }
    },
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 10,
          minHeight: 38
        },
        containedPrimary: {
          boxShadow: '0 8px 18px rgba(70, 95, 255, .22)'
        }
      }
    },
    MuiTextField: {
      defaultProps: {
        size: 'small'
      }
    },
    MuiTableCell: {
      styleOverrides: {
        root: {
          borderBottom: `1px solid ${uiBorder}`
        },
        head: {
          color: '#344054',
          backgroundColor: '#f9fafb',
          fontSize: 13
        }
      }
    },
    MuiDialog: {
      styleOverrides: {
        paper: {
          borderRadius: 16
        }
      }
    },
    MuiChip: {
      styleOverrides: {
        root: {
          fontWeight: 700
        }
      }
    }
  }
});

type NavItem = { to: string; label: string; icon: ReactNode; locked?: boolean };

const nav: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: <DashboardIcon /> },
  { to: '/cotizaciones', label: 'Cotizaciones', icon: <ReceiptLong /> },
  { to: '/clientes', label: 'Clientes', icon: <Groups /> },
  { to: '/solicitudes-credito', label: 'Solicitudes credito', icon: <Assignment /> },
  { to: '/ordenes-recaudo', label: 'Ordenes recaudo', icon: <ReceiptLong /> },
  { to: '/tramites', label: 'Tramites', icon: <Assignment /> },
  { to: '/entregas', label: 'Entregas', icon: <LocalShipping /> },
  { to: '/pipeline', label: 'Pipeline', icon: <ViewKanban /> },
  { to: '/actividades', label: 'Actividades', icon: <EventNote /> },
  { to: '/productos', label: 'Productos', icon: <Inventory2 /> },
  { to: '/inventario', label: 'Inventario', icon: <Inventory2 /> },
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
const emptyUser = { fullName: '', email: '', password: '', companyId: '', salesPointId: '', roles: ['Vendedor'] };
const emptyProduct = { name: '', category: 'Moto', brand: '', model: '', line: '', version: '', reference: '', description: '', engineCc: '', year: '', color: '', price: 0, soat: 0, registrationFee: 0, taxes: 0, technicalSheet: '', priceValidFrom: today, active: true };
const emptyCommercialInventory = { productId: '', salesPointId: '', vin: '', chassisNumber: '', engineNumber: '', plate: '', color: '', isUsed: false, mileage: '', status: 1, notes: '' };
const emptyInventoryReservation = { customerId: '', quoteId: '', creditApplicationId: '', reservationExpiresAt: new Date(Date.now() + 3 * 86400000).toISOString().slice(0, 10), notes: '' };
const emptyFinancialSettings = { minimumWage: 1400000, consumerAnnualRate: 29.72, lowAmountAnnualRate: 56.33, factorMonthlyRate: 4.5, maxTermMonths: 30, paymentRounding: 1000, useMontelibanoTable: true, active: true };
const emptySalesPoint = { name: '', code: '', city: '', address: '', phone: '', mainBrand: 'Honda', brandLogoDataUrl: '', factorMonthlyRate: 4.5, maxTermMonths: 30, quoteValidityDays: 7, deliveryMode: 'ConSoat', soatDays: 14, registrationDays: 20, soatProvider: '', registrationAgent: '', commercialTerms: 'Cotizacion sujeta a disponibilidad del producto, validacion comercial y aprobacion final.', active: true };
const emptyRequirementDocument = { type: 5, name: '', description: '', required: true, order: 1 };
const emptyRequirementProfile = { name: '', code: '', description: '', isCash: false, active: true, documents: [emptyRequirementDocument] };
const emptyPromotion = { name: '', code: '', discountType: 'Valor', discountValue: 0, productId: '', brand: '', color: '', salesPointId: '', validFrom: today, validUntil: today, active: true };
const emptyQuoteItem = { productId: '', downPayment: 0, insurance: 0, administrativeFees: 0, termMonths: 24, monthlyInterestRate: 2.2 };
const emptyQuote = { identificationType: 1, identificationNumber: '', customerFirstNames: '', customerLastNames: '', customerFirstName: '', customerMiddleName: '', customerLastName: '', customerSecondLastName: '', phoneCountryCode: '+57', phoneNumber: '', requirementProfileId: '', productId: '', downPayment: 0, insurance: 0, administrativeFees: 0, termMonths: 24, monthlyInterestRate: 2.2, items: [emptyQuoteItem], notes: '' };
const emptyCreditApplication = {
  customerId: '', productId: '', quoteId: '', dealId: '', requirementProfileId: '', identificationType: 1, identificationNumber: '', birthDate: '', mobile: '', address: '', city: '', occupation: '',
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
  preDeliveryChecklistCompleted: false,
  deliveryProtocol: '',
  deliveryPhotoDataUrl: '',
  deliveryPhotoFileName: '',
  firstServiceScheduledAt: '',
  status: 1,
  notes: ''
};
const emptyCollectionOrder = {
  creditApplicationId: '',
  dueDate: today,
  vehicleAmount: 0,
  documentsAmount: 0,
  advanceAmount: 0,
  paidAmount: 0,
  status: 1,
  notes: ''
};
const emptyProcedure = {
  creditApplicationId: '',
  salesPointId: '',
  type: 1,
  status: 1,
  startDate: today,
  estimatedDate: '',
  completedAt: '',
  responsible: '',
  thirdParty: '',
  notifyCustomer: true,
  customerNotifiedAt: '',
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

function normalizeSearch(value?: string | null) {
  return (value ?? '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().trim();
}

function Layout() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();
  const muiTheme = useTheme();
  const isDesktop = useMediaQuery(muiTheme.breakpoints.up('md'));
  const [mobileOpen, setMobileOpen] = useState(false);
  const closeMobileNav = () => setMobileOpen(false);
  const navButtonSx = {
    justifyContent: 'flex-start',
    my: .25,
    width: '100%',
    px: 1.5,
    py: 1.05,
    color: '#d0d5dd',
    '& .MuiButton-startIcon': { color: 'inherit' },
    '&.active': {
      bgcolor: uiPrimary,
      color: '#fff',
      boxShadow: '0 10px 24px rgba(70, 95, 255, .28)'
    },
    '&:hover': {
      bgcolor: 'rgba(255, 255, 255, .08)',
      color: '#fff'
    }
  };
  const drawerContent = <>
    <Toolbar sx={{ px: 2.5, minHeight: 76 }}>
      <Stack direction="row" alignItems="center" gap={1.25} sx={{ minWidth: 0 }}>
        <Box sx={{
          width: 38,
          height: 38,
          borderRadius: 2,
          display: 'grid',
          placeItems: 'center',
          bgcolor: uiPrimary,
          color: '#fff',
          fontWeight: 900,
          boxShadow: '0 12px 28px rgba(70, 95, 255, .32)'
        }}>E</Box>
        <Box sx={{ minWidth: 0 }}>
          <Typography variant="h6" fontWeight={900} color="#fff" noWrap>EnMarcha CRM</Typography>
          <Typography variant="caption" color="#98a2b3" noWrap>Gestion comercial SaaS</Typography>
        </Box>
      </Stack>
    </Toolbar>
    <Divider sx={{ borderColor: 'rgba(255, 255, 255, .08)' }} />
    <Stack sx={{ p: 1.25, flex: 1 }}>
      {nav.map((item) => item.locked ? (
        <Tooltip key={item.to} title="Disponible en la siguiente fase de la demostracion" placement="right">
          <span>
            <Button disabled startIcon={item.icon} sx={{ ...navButtonSx, opacity: .36, color: '#98a2b3' }}>
              {item.label}
            </Button>
          </span>
        </Tooltip>
      ) : (
        <Button key={item.to} component={NavLink} to={item.to} startIcon={item.icon} onClick={closeMobileNav} sx={navButtonSx}>
          {item.label}
        </Button>
      ))}
    </Stack>
    <Box sx={{ m: 1.5, p: 1.5, borderRadius: 3, bgcolor: uiSidebarSoft, border: '1px solid rgba(255, 255, 255, .08)' }}>
      <Typography variant="caption" color="#98a2b3">Sesion activa</Typography>
      <Typography color="#fff" fontWeight={800} fontSize={13} noWrap>{user?.email ?? 'Usuario'}</Typography>
    </Box>
  </>;
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', width: '100%', overflowX: 'hidden', bgcolor: 'background.default' }}>
      <Drawer
        variant={isDesktop ? 'permanent' : 'temporary'}
        open={isDesktop || mobileOpen}
        onClose={closeMobileNav}
        ModalProps={{ keepMounted: true }}
        PaperProps={{ sx: { width: drawerWidth, borderRight: 0, bgcolor: uiSidebar, color: '#fff' } }}
      >
        {drawerContent}
      </Drawer>
      <Box sx={{ flex: 1, minWidth: 0, ml: { xs: 0, md: `${drawerWidth}px` } }}>
        <AppBar position="sticky" color="inherit" elevation={0} sx={{ borderBottom: `1px solid ${uiBorder}`, bgcolor: 'rgba(255, 255, 255, .86)', backdropFilter: 'blur(14px)' }}>
          <Toolbar sx={{ justifyContent: 'space-between', gap: 1.5, minHeight: 70 }}>
            <Stack direction="row" alignItems="center" gap={1.25} sx={{ minWidth: 0 }}>
              {!isDesktop && <IconButton aria-label="Abrir menu" edge="start" onClick={() => setMobileOpen(true)}><Menu /></IconButton>}
              <Box sx={{ minWidth: 0 }}>
                <Typography fontWeight={900} noWrap>{user?.fullName ?? 'Equipo comercial'}</Typography>
                <Typography color="text.secondary" fontSize={13} noWrap>{user?.roles.join(', ')}</Typography>
              </Box>
            </Stack>
            <Stack direction="row" alignItems="center" gap={1}>
              <Box sx={{ display: { xs: 'none', sm: 'flex' }, alignItems: 'center', gap: 1, px: 1.5, py: .75, border: `1px solid ${uiBorder}`, borderRadius: 999, bgcolor: '#fff' }}>
                <Search fontSize="small" sx={{ color: uiMuted }} />
                <Typography variant="body2" color="text.secondary">CRM comercial</Typography>
              </Box>
              <Tooltip title="Salir">
                <IconButton aria-label="Salir" onClick={() => { logout(); navigate('/login'); }} sx={{ border: `1px solid ${uiBorder}`, bgcolor: '#fff' }}><Logout /></IconButton>
              </Tooltip>
            </Stack>
          </Toolbar>
        </AppBar>
        <Box component="main" sx={{ p: { xs: 1.5, sm: 2, md: 3 }, width: '100%', maxWidth: '100vw', boxSizing: 'border-box' }}>
          <Routes>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/clientes" element={<CustomersPage />} />
            <Route path="/clientes/:id" element={<Customer360Page />} />
            <Route path="/productos" element={<ProductsPage />} />
            <Route path="/inventario" element={<CommercialInventoryPage />} />
            <Route path="/cotizaciones" element={<QuotesPage />} />
            <Route path="/solicitudes-credito" element={<CreditApplicationsPage />} />
            <Route path="/ordenes-recaudo" element={<CollectionOrdersPage />} />
            <Route path="/tramites" element={<ProceduresPage />} />
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
        <Stack direction="row" alignItems="center" gap={1.25} sx={{ mb: 3 }}>
          <Box sx={{
            width: 44,
            height: 44,
            borderRadius: 2.5,
            display: 'grid',
            placeItems: 'center',
            bgcolor: uiPrimary,
            color: '#fff',
            fontWeight: 900,
            fontSize: 22,
            boxShadow: '0 14px 28px rgba(70, 95, 255, .28)'
          }}>E</Box>
          <Box>
            <Typography variant="h4" fontWeight={900} sx={{ lineHeight: 1 }}>EnMarcha CRM</Typography>
            <Typography color="text.secondary">Gestion comercial multiempresa</Typography>
          </Box>
        </Stack>
        <Stack spacing={2}>
          <TextField label="Empresa" value={tenant} onChange={(e) => setTenant(e.target.value)} />
          <TextField label="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
          <TextField label="Contrasena" type="password" value={password} onChange={(e) => setPassword(e.target.value)} onKeyDown={(e) => e.key === 'Enter' && login()} />
          {error && <Alert severity="error">{error}</Alert>}
          {loading && <LinearProgress />}
          <Button variant="contained" size="large" onClick={login} disabled={loading}>Ingresar al CRM</Button>
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
  const [search, setSearch] = useState('');
  const [customerView, setCustomerView] = useState('all');
  const canDelete = useCanManage();
  const navigate = useNavigate();
  const customerViewOptions = useMemo(() => [
    { key: 'all', label: 'Todos', count: rows.length },
    { key: 'active', label: 'Activos', count: rows.filter((customer) => customer.status === 1).length },
    { key: 'inactive', label: 'Inactivos', count: rows.filter((customer) => customer.status === 2).length },
    { key: 'suspended', label: 'Suspendidos', count: rows.filter((customer) => customer.status === 3).length },
    { key: 'missingPhone', label: 'Sin telefono', count: rows.filter((customer) => !normalizeSearch(customer.phone)).length }
  ], [rows]);
  const filteredRows = useMemo(() => {
    const term = normalizeSearch(search);
    return rows.filter((customer) => {
      const matchesView =
        customerView === 'all'
        || (customerView === 'active' && customer.status === 1)
        || (customerView === 'inactive' && customer.status === 2)
        || (customerView === 'suspended' && customer.status === 3)
        || (customerView === 'missingPhone' && !normalizeSearch(customer.phone));
      if (!matchesView) return false;
      if (!term) return true;
      const searchable = [
        customer.firstName,
        customer.middleName,
        customer.lastName,
        customer.secondLastName,
        customer.firstNames,
        customer.lastNames,
        customer.name,
        `${customer.firstName ?? ''} ${customer.middleName ?? ''} ${customer.lastName ?? ''} ${customer.secondLastName ?? ''}`,
        `${customer.firstNames ?? ''} ${customer.lastNames ?? ''}`,
        customer.phone,
        customer.phoneCountryCode,
        `${customer.phoneCountryCode ?? ''}${customer.phone ?? ''}`
      ].map(normalizeSearch).join(' ');
      return searchable.includes(term);
    });
  }, [rows, search, customerView]);

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
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={1.5}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={1.5} alignItems={{ xs: 'stretch', md: 'center' }} justifyContent="space-between">
          <TextField
            fullWidth
            label="Buscar cliente"
            placeholder="Nombre, apellido o telefono"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            InputProps={{
              startAdornment: <InputAdornment position="start"><Search fontSize="small" /></InputAdornment>,
              endAdornment: search ? <InputAdornment position="end"><IconButton size="small" onClick={() => setSearch('')}><Close fontSize="small" /></IconButton></InputAdornment> : undefined
            }}
          />
          <Chip
            variant="outlined"
            label={`${filteredRows.length} de ${rows.length} clientes`}
            sx={{ alignSelf: { xs: 'flex-start', md: 'center' }, flexShrink: 0 }}
          />
        </Stack>
        <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap" useFlexGap>
          <Typography variant="body2" color="text.secondary" fontWeight={800}>Vistas rapidas</Typography>
          {customerViewOptions.map((option) => <Chip
            key={option.key}
            clickable
            color={customerView === option.key ? 'primary' : 'default'}
            variant={customerView === option.key ? 'filled' : 'outlined'}
            label={`${option.label} (${option.count})`}
            onClick={() => setCustomerView(option.key)}
          />)}
        </Stack>
      </Stack>
    </Paper>
    <EntityTable
      headers={['Identificacion', 'Primer nombre', 'Segundo nombre', 'Primer apellido', 'Segundo apellido', 'Telefono', 'Ciudad', 'Estado', 'Etiquetas', 'Acciones']}
      empty={search ? 'No hay clientes que coincidan con la busqueda' : 'No hay clientes registrados'}
      rows={filteredRows.map((r) => [
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
  const [notice, setNotice] = useState<Notice>();

  const saveActivity = async (payload: typeof emptyActivity) => {
    await api.post<Activity>('/api/activities', toActivityPayload(payload));
    setNotice({ type: 'success', text: 'Seguimiento registrado.' });
    setActivityForm({ open: false });
    reload();
  };

  const customerName = customer ? `${customer.firstNames || customer.name} ${customer.lastNames ?? ''}`.trim() : '';
  const quotes = data?.quotes ?? [];
  const creditApplications = data?.creditApplications ?? [];
  const deals = data?.deals ?? [];
  const activities = data?.activities ?? [];
  const timeline = data?.timeline ?? [];
  const totalQuoted = quotes.reduce((sum, quote) => sum + Number(quote.productPrice || 0), 0);
  const openDeals = deals.filter((deal) => deal.status === 1);
  const openPipelineValue = openDeals.reduce((sum, deal) => sum + Number(deal.value || 0), 0);
  const pendingActivities = activities.filter((activity) => activity.status !== 3 && activity.status !== 4);
  const overdueActivities = pendingActivities.filter((activity) => new Date(activity.scheduledAt).getTime() < Date.now());
  const sortedPendingActivities = [...pendingActivities].sort((a, b) => new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime());
  const nextActivity = sortedPendingActivities.find((activity) => new Date(activity.scheduledAt).getTime() >= Date.now()) ?? sortedPendingActivities[0];
  const latestQuote = [...quotes].sort((a, b) => new Date(b.quoteDate).getTime() - new Date(a.quoteDate).getTime())[0];
  const activeCredit = [...creditApplications].sort((a, b) => new Date(b.submittedAt ?? '').getTime() - new Date(a.submittedAt ?? '').getTime())[0];
  const pendingDocuments = creditApplications.reduce((sum, application) => sum + (application.documents?.filter((document) => document.status === 1).length ?? 0), 0);
  const latestTimeline = timeline[0];

  return <Stack spacing={3}>
    <Header
      title={customer ? customerName : 'Cliente 360'}
      onRefresh={reload}
      secondaryAction={{ label: 'Volver', onClick: () => navigate('/clientes') }}
    />
    <StatusBar loading={loading} error={error} />
    {customer && <Paper variant="outlined" sx={{ p: { xs: 2, md: 3 }, bgcolor: '#fbfdff' }}>
      <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ xs: 'stretch', md: 'center' }} gap={2}>
        <Stack direction="row" spacing={2} alignItems="center">
          <Box sx={{ width: 58, height: 58, borderRadius: '50%', bgcolor: '#155e75', color: '#fff', display: 'grid', placeItems: 'center', fontSize: 22, fontWeight: 900, flexShrink: 0 }}>
            {(customer.firstName?.[0] || customer.firstNames?.[0] || customer.name?.[0] || 'C').toUpperCase()}
          </Box>
          <Box sx={{ minWidth: 0 }}>
            <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap">
              <Typography variant="h5" fontWeight={900}>{customerName}</Typography>
              <StatusChip label={statusLabel(customer.status)} tone={customer.status === 1 ? 'success' : 'default'} />
            </Stack>
            <Typography color="text.secondary">{identificationLabel(customer.identificationType ?? 1)} {customer.identificationNumber || 'sin identificacion'}</Typography>
            {customer.tags && <Typography variant="body2" color="text.secondary">{customer.tags}</Typography>}
          </Box>
        </Stack>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} alignItems={{ xs: 'stretch', sm: 'center' }}>
          {customer.phone && <Button variant="contained" startIcon={<WhatsApp />} href={whatsappUrl(customer.phone)} target="_blank" rel="noreferrer">WhatsApp</Button>}
          {customer.email && <Button variant="outlined" href={`mailto:${customer.email}`}>Email</Button>}
          <Button variant="outlined" startIcon={<AddTask />} onClick={() => setActivityForm({ open: true, item: { ...emptyActivity, title: `Seguimiento: ${customer.name}`, customerId: customer.id } as Activity })}>Seguimiento</Button>
        </Stack>
      </Stack>
    </Paper>}
    {customer && <Grid container spacing={2}>
      <Grid item xs={12} sm={6} md={2.4}><Metric label="Cotizaciones" value={quotes.length} /></Grid>
      <Grid item xs={12} sm={6} md={2.4}><Metric label="Valor cotizado" value={money(totalQuoted)} /></Grid>
      <Grid item xs={12} sm={6} md={2.4}><Metric label="Solicitudes" value={creditApplications.length} /></Grid>
      <Grid item xs={12} sm={6} md={2.4}><Metric label="Pipeline abierto" value={money(openPipelineValue)} /></Grid>
      <Grid item xs={12} sm={6} md={2.4}><Metric label="Pendientes" value={pendingActivities.length + pendingDocuments} /></Grid>
    </Grid>}
    {customer && <Grid container spacing={2}>
      <Grid item xs={12} lg={4}>
        <Card sx={{ height: '100%' }}><CardContent>
          <Typography variant="h6" fontWeight={900} sx={{ mb: 1.5 }}>Datos del cliente</Typography>
          <Stack spacing={1}>
            <InfoLine label="Telefono" value={customer.phone ? `${customer.phoneCountryCode ?? ''} ${customer.phone}` : '-'} />
            <InfoLine label="Email" value={customer.email || '-'} />
            <InfoLine label="Ciudad" value={customer.city || '-'} />
            <InfoLine label="Direccion" value={customer.address || '-'} />
            <InfoLine label="Ocupacion" value={customer.occupation || '-'} />
            <InfoLine label="Empresa / razon comercial" value={customer.companyName || '-'} />
          </Stack>
        </CardContent></Card>
      </Grid>
      <Grid item xs={12} lg={4}>
        <Card sx={{ height: '100%' }}><CardContent>
          <Typography variant="h6" fontWeight={900} sx={{ mb: 1.5 }}>Resumen comercial</Typography>
          <Stack spacing={1}>
            <InfoLine label="Ultima cotizacion" value={latestQuote ? `${latestQuote.number} - ${money(latestQuote.productPrice)}` : 'Sin cotizaciones'} />
            <InfoLine label="Credito actual" value={activeCredit ? `${activeCredit.number} - ${creditStatus(activeCredit.status)}` : 'Sin solicitud'} />
            <InfoLine label="Negocios abiertos" value={`${openDeals.length} por ${money(openPipelineValue)}`} />
            <InfoLine label="Documentos pendientes" value={pendingDocuments} />
            <InfoLine label="Actividades vencidas" value={overdueActivities.length} />
          </Stack>
        </CardContent></Card>
      </Grid>
      <Grid item xs={12} lg={4}>
        <Card sx={{ height: '100%' }}><CardContent>
          <Typography variant="h6" fontWeight={900} sx={{ mb: 1.5 }}>Siguiente paso</Typography>
          {nextActivity ? <Stack spacing={1}>
            <StatusChip label={activityStatus(nextActivity.status)} tone={new Date(nextActivity.scheduledAt).getTime() < Date.now() ? 'warning' : 'default'} />
            <Typography fontWeight={900}>{nextActivity.title}</Typography>
            <Typography color="text.secondary">{new Date(nextActivity.scheduledAt).toLocaleString()}</Typography>
            {nextActivity.description && <Typography color="text.secondary">{nextActivity.description}</Typography>}
          </Stack> : <EmptyState text="Sin seguimiento programado" />}
          {latestTimeline && <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid #edf1f5' }}>
            <Typography variant="caption" color="text.secondary">Ultimo movimiento</Typography>
            <Typography fontWeight={800}>{latestTimeline.title}</Typography>
            <Typography variant="body2" color="text.secondary">{new Date(latestTimeline.occurredAt).toLocaleString()}</Typography>
          </Box>}
        </CardContent></Card>
      </Grid>
    </Grid>}
    <Grid container spacing={2}>
      <Grid item xs={12} lg={7}>
        <Card><CardContent>
          <Stack direction="row" alignItems="center" justifyContent="space-between" gap={2} sx={{ mb: 2 }}>
            <Typography variant="h6" fontWeight={900}>Timeline comercial</Typography>
            <Chip size="small" label={`${timeline.length} eventos`} variant="outlined" />
          </Stack>
          <CustomerTimeline items={timeline} />
        </CardContent></Card>
      </Grid>
      <Grid item xs={12} lg={5}>
        <Stack spacing={2}>
          <Card><CardContent>
            <Typography variant="h6" fontWeight={900}>Cotizaciones</Typography>
            {quotes.length ? quotes.slice(0, 4).map((q) => <Row key={q.id} primary={`${q.number} - ${q.productName}`} secondary={`Financiado ${money(q.financedAmount)} - cuota aprox. ${money(q.estimatedMonthlyPayment)} x ${q.termMonths} - ${new Date(q.quoteDate).toLocaleDateString()}`} />) : <EmptyState text="Sin cotizaciones" />}
          </CardContent></Card>
          <Card><CardContent>
            <Typography variant="h6" fontWeight={900}>Solicitudes de credito</Typography>
            {creditApplications.length ? creditApplications.slice(0, 4).map((s) => <Row key={s.id} primary={`${s.number} - ${s.productName}`} secondary={`${creditStatus(s.status)} - ${money(s.motorcycleValue)}`} />) : <EmptyState text="Sin solicitudes" />}
          </CardContent></Card>
        </Stack>
      </Grid>
      <Grid item xs={12} lg={6}>
        <Card><CardContent>
          <Typography variant="h6" fontWeight={900}>Pipeline</Typography>
          {deals.length ? deals.map((d) => <Row key={d.id} primary={d.title} secondary={`${dealStatus(d.status)} - ${money(d.value)} - ${d.closeProbability}%`} />) : <EmptyState text="Sin negocios" />}
        </CardContent></Card>
      </Grid>
      <Grid item xs={12} lg={6}>
        <Card><CardContent>
          <Typography variant="h6" fontWeight={900}>Actividades</Typography>
          {activities.length ? activities.map((a) => <Row key={a.id} primary={a.title} secondary={`${activityStatus(a.status)} - ${new Date(a.scheduledAt).toLocaleString()}`} />) : <EmptyState text="Sin actividades" />}
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
      line: payload.line || null,
      version: payload.version || null,
      engineCc: payload.engineCc === '' ? null : Number(payload.engineCc),
      year: payload.year === '' ? null : Number(payload.year),
      color: payload.color || null,
      price: Number(payload.price),
      soat: Number(payload.soat),
      registrationFee: Number(payload.registrationFee),
      taxes: Number(payload.taxes),
      technicalSheet: payload.technicalSheet || null,
      priceValidFrom: payload.priceValidFrom || null,
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
      headers={['Producto', 'Fotos', 'Categoria', 'Marca', 'Referencia', 'Caracteristicas', 'Cargos', 'Precio', 'Estado', 'Acciones']}
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
        [r.model, r.line, r.version, r.engineCc ? `${r.engineCc} cc` : undefined, r.year, r.color].filter(Boolean).join(' / ') || r.description,
        <Stack spacing={0.25}>
          <Typography variant="body2">{money((r.soat ?? 0) + (r.registrationFee ?? 0) + (r.taxes ?? 0))}</Typography>
          <Typography variant="caption" color="text.secondary">SOAT {money(r.soat ?? 0)} · Mat. {money(r.registrationFee ?? 0)}</Typography>
        </Stack>,
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

function CommercialInventoryPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<CommercialInventory[]>('/api/commercial-inventory', []);
  const { data: summary = [], reload: reloadSummary } = useResource<CommercialInventorySummary[]>('/api/commercial-inventory/summary', []);
  const { data: products = [] } = useResource<Product[]>('/api/products', []);
  const { data: salesPoints = [] } = useResource<SalesPoint[]>('/api/sales-points', []);
  const { data: customers = [] } = useResource<Customer[]>('/api/customers', []);
  const { data: quotes = [] } = useResource<Quote[]>('/api/quotes', []);
  const { data: applications = [] } = useResource<CreditApplication[]>('/api/credit-applications', []);
  const [form, setForm] = useState<FormMode<CommercialInventory>>({ open: false });
  const [reserveForm, setReserveForm] = useState<FormMode<CommercialInventory>>({ open: false });
  const [notice, setNotice] = useState<Notice>();
  const canManage = useCanManage();

  const refreshAll = async () => {
    await Promise.all([reload(), reloadSummary()]);
  };

  const save = async (payload: typeof emptyCommercialInventory) => {
    const body = {
      ...payload,
      vin: payload.vin || null,
      chassisNumber: payload.chassisNumber || null,
      engineNumber: payload.engineNumber || null,
      plate: payload.plate || null,
      color: payload.color || null,
      mileage: payload.mileage === '' ? null : Number(payload.mileage),
      status: Number(payload.status),
      notes: payload.notes || null
    };
    const { data } = form.item
      ? await api.put<CommercialInventory>(`/api/commercial-inventory/${form.item.id}`, body)
      : await api.post<CommercialInventory>('/api/commercial-inventory', body);
    setData(form.item ? rows.map((x) => x.id === data.id ? data : x) : [data, ...rows]);
    await reloadSummary();
    setNotice({ type: 'success', text: form.item ? 'Unidad actualizada.' : 'Unidad registrada.' });
    setForm({ open: false });
  };

  const reserve = async (payload: typeof emptyInventoryReservation) => {
    if (!reserveForm.item) return;
    const body = {
      customerId: payload.customerId || null,
      quoteId: payload.quoteId || null,
      creditApplicationId: payload.creditApplicationId || null,
      reservationExpiresAt: payload.reservationExpiresAt || null,
      notes: payload.notes || null
    };
    const { data } = await api.post<CommercialInventory>(`/api/commercial-inventory/${reserveForm.item.id}/reserve`, body);
    setData(rows.map((x) => x.id === data.id ? data : x));
    await reloadSummary();
    setNotice({ type: 'success', text: 'Unidad separada contra disponibilidad.' });
    setReserveForm({ open: false });
  };

  const quickAction = async (item: CommercialInventory, action: 'release' | 'sell') => {
    const { data } = await api.post<CommercialInventory>(`/api/commercial-inventory/${item.id}/${action}`);
    setData(rows.map((x) => x.id === data.id ? data : x));
    await reloadSummary();
    setNotice({ type: 'success', text: action === 'release' ? 'Separacion liberada.' : 'Unidad marcada como vendida.' });
  };

  return <Stack spacing={3}>
    <Header title="Inventario comercial" action={canManage ? 'Nueva unidad' : undefined} onAction={() => setForm({ open: true })} onRefresh={refreshAll} />
    <StatusBar loading={loading} error={error} />
    <Grid container spacing={2}>
      {summary.slice(0, 6).map((item) => <Grid item xs={12} md={6} lg={4} key={`${item.productId}-${item.salesPointId}`}>
        <Paper variant="outlined" sx={{ p: 2, height: '100%' }}>
          <Typography fontWeight={900}>{item.productName}</Typography>
          <Typography variant="body2" color="text.secondary">{item.salesPointName}</Typography>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap sx={{ mt: 1.5 }}>
            <Chip size="small" color="success" label={`Disp. ${item.available}`} />
            <Chip size="small" color="warning" label={`Sep. ${item.reserved}`} />
            <Chip size="small" variant="outlined" label={`Usadas ${item.used}`} />
            <Chip size="small" variant="outlined" label={`Vend. ${item.sold}`} />
          </Stack>
        </Paper>
      </Grid>)}
      {summary.length === 0 && <Grid item xs={12}><Alert severity="info">Aun no hay inventario registrado por sede.</Alert></Grid>}
    </Grid>
    <EntityTable
      headers={['Producto', 'Sede', 'Seriales', 'Tipo', 'Estado', 'Reserva', 'Acciones']}
      empty="No hay unidades de inventario registradas"
      rows={rows.map((r) => [
        <Row primary={r.productName} secondary={r.color || 'Sin color'} />,
        r.salesPointName,
        <Stack spacing={.4}>
          <Typography variant="body2">Chasis: {r.chassisNumber || '-'}</Typography>
          <Typography variant="body2">Motor: {r.engineNumber || '-'}</Typography>
          <Typography variant="body2">Placa: {r.plate || '-'}</Typography>
        </Stack>,
        <Row primary={r.isUsed ? 'Usada' : 'Nueva'} secondary={r.mileage != null ? `${r.mileage} km` : r.vin || 'Sin VIN'} />,
        <StatusChip label={inventoryStatus(r.status)} tone={inventoryTone(r.status, r.reservationExpired)} />,
        <Row
          primary={r.reservedCustomerName || r.reservedQuoteNumber || r.reservedCreditApplicationNumber || 'Sin separacion'}
          secondary={r.reservationExpiresAt ? `Vence ${new Date(r.reservationExpiresAt).toLocaleDateString()}` : r.notes || ''}
        />,
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
          <Actions onEdit={canManage ? () => setForm({ open: true, item: r }) : undefined} />
          {(r.status === 1 || r.status === 4) && <Button size="small" variant="outlined" onClick={() => setReserveForm({ open: true, item: r })}>Separar</Button>}
          {r.status === 2 && <Button size="small" variant="outlined" color="inherit" onClick={() => void quickAction(r, 'release')}>Liberar</Button>}
          {(r.status === 1 || r.status === 2 || r.status === 4) && <Button size="small" variant="contained" onClick={() => void quickAction(r, 'sell')}>Vendida</Button>}
        </Stack>
      ])}
    />
    <CommercialInventoryDialog form={form} products={products.filter((x) => x.active)} salesPoints={salesPoints.filter((x) => x.active)} onClose={() => setForm({ open: false })} onSave={save} />
    <InventoryReservationDialog form={reserveForm} customers={customers} quotes={quotes} applications={applications} onClose={() => setReserveForm({ open: false })} onSave={reserve} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function QuotesPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<Quote[]>('/api/quotes', []);
  const { data: products = [] } = useResource<Product[]>('/api/products', []);
  const { data: customers = [] } = useResource<Customer[]>('/api/customers', []);
  const { data: requirementProfiles = [] } = useResource<RequirementProfile[]>('/api/requirement-profiles', []);
  const [form, setForm] = useState<FormMode<Quote>>({ open: false });
  const [analysis, setAnalysis] = useState<CustomerAiAnalysis>();
  const [analysisPhone, setAnalysisPhone] = useState<string>();
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
    const quoteItems = (payload.items?.length ? payload.items : [{ ...emptyQuoteItem, productId: payload.productId, downPayment: payload.downPayment, insurance: payload.insurance, administrativeFees: payload.administrativeFees, termMonths: payload.termMonths, monthlyInterestRate: payload.monthlyInterestRate }])
      .filter((item) => item.productId)
      .map((item) => ({
        productId: item.productId,
        downPayment: Number(item.downPayment),
        insurance: Number(item.insurance),
        administrativeFees: Number(item.administrativeFees),
        termMonths: Number(item.termMonths),
        monthlyInterestRate: Number(item.monthlyInterestRate)
      }));
    if (!quoteItems.length) throw new Error('Debe agregar al menos un producto.');
    const firstItem = quoteItems[0];
    const body = {
      ...payload,
      customerFirstNames: fullFirstNames(payload.customerFirstName, payload.customerMiddleName, payload.customerFirstNames),
      customerLastNames: fullLastNames(payload.customerLastName, payload.customerSecondLastName, payload.customerLastNames),
      identificationType: Number(payload.identificationType),
      identificationNumber: payload.identificationNumber || null,
      phoneCountryCode: payload.phoneCountryCode || '+57',
      phoneNumber: payload.phoneNumber || null,
      requirementProfileId: payload.requirementProfileId || null,
      productId: firstItem.productId,
      items: quoteItems,
      downPayment: firstItem.downPayment,
      insurance: firstItem.insurance,
      administrativeFees: firstItem.administrativeFees,
      termMonths: firstItem.termMonths,
      monthlyInterestRate: firstItem.monthlyInterestRate,
      notes: payload.notes || null
    };
    const { data } = await api.post<Quote>('/api/quotes', body);
    setData([data, ...rows]);
    setNotice({ type: 'success', text: 'Cotizacion creada. Revise la vista previa antes de descargar o imprimir.' });
    setForm({ open: false });
    setPreviewQuote(data);
  };

  const analyzeCustomer = async (customerId: string, phone?: string) => {
    try {
      const { data } = await api.get<CustomerAiAnalysis>(`/api/customers/${customerId}/ai-analysis`);
      setAnalysis(data);
      setAnalysisPhone(phone);
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  return <Stack spacing={3}>
    <Header title="Cotizaciones" action="Nueva cotizacion" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Numero', 'Cliente', 'Sede', 'Perfil', 'Promocion', 'Productos', 'Total financiado', 'Cuota aprox.', 'Valida hasta', 'Acciones']}
      empty="No hay cotizaciones registradas"
      rows={rows.map((r) => [
        r.number,
        `${fullFirstNames(r.customerFirstName, r.customerMiddleName, r.customerFirstNames)} ${fullLastNames(r.customerLastName, r.customerSecondLastName, r.customerLastNames)}`.trim(),
        r.salesPointName || '-',
        r.requirementProfileName || '-',
        r.promotionDiscount > 0 ? <Row primary={r.promotionName ?? 'Promocion'} secondary={`-${money(r.promotionDiscount)}`} /> : '-',
        (r.items?.length ?? 0) > 1 ? `${r.items.length} productos` : r.productName,
        money(r.financedAmount),
        r.estimatedMonthlyPayment > 0 ? `${money(r.estimatedMonthlyPayment)} x ${r.termMonths}` : 'Sin simulacion',
        new Date(r.validUntil).toLocaleDateString(),
        <Actions onAi={() => analyzeCustomer(r.customerId, customers.find((x) => x.id === r.customerId)?.phone)} onDownload={() => setPreviewQuote(r)} />
      ])}
    />
    <QuoteDialog form={form} products={products.filter((x) => x.active)} requirementProfiles={requirementProfiles.filter((x) => x.active)} onClose={() => setForm({ open: false })} onSave={save} />
    <QuotePdfPreviewDialog quote={previewQuote} onClose={() => setPreviewQuote(undefined)} onDownload={downloadPdf} />
    <AiAnalysisDialog analysis={analysis} phone={analysisPhone} onClose={() => { setAnalysis(undefined); setAnalysisPhone(undefined); }} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function CreditApplicationsPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<CreditApplication[]>('/api/credit-applications', []);
  const { data: customers = [] } = useResource<Customer[]>('/api/customers', []);
  const { data: products = [] } = useResource<Product[]>('/api/products', []);
  const { data: quotes = [] } = useResource<Quote[]>('/api/quotes', []);
  const { data: deals = [] } = useResource<Deal[]>('/api/pipeline/deals', []);
  const { data: requirementProfiles = [] } = useResource<RequirementProfile[]>('/api/requirement-profiles', []);
  const [form, setForm] = useState<FormMode<CreditApplication>>({ open: false });
  const [management, setManagement] = useState<CreditApplication>();
  const [analysis, setAnalysis] = useState<CustomerAiAnalysis>();
  const [analysisPhone, setAnalysisPhone] = useState<string>();
  const [notice, setNotice] = useState<Notice>();
  const managementApplication = management ? rows.find((x) => x.id === management.id) ?? management : undefined;

  const save = async (payload: typeof emptyCreditApplication) => {
    const body = {
      ...payload,
      quoteId: payload.quoteId || null,
      dealId: payload.dealId || null,
      requirementProfileId: payload.requirementProfileId || null,
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
    try {
      const { data } = await api.post<CreditApplication>(`/api/credit-applications/${application.id}/status`, { status });
      setData(rows.map((x) => x.id === data.id ? data : x));
      setNotice({ type: 'success', text: `Solicitud marcada como ${creditStatus(status)}.` });
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  const decide = async (application: CreditApplication, status: number, notes?: string, study?: Partial<CreditApplication> & { result?: string }) => {
    try {
      const { data } = await api.post<CreditApplication>(`/api/credit-applications/${application.id}/decision`, {
        status,
        notes: notes ?? null,
        result: study?.result ?? null,
        approvedAmount: study?.analystApprovedAmount ?? null,
        approvedDownPayment: study?.approvedDownPayment ?? null,
        approvedTermMonths: study?.approvedTermMonths ?? null,
        approvedMonthlyPayment: study?.approvedMonthlyPayment ?? null,
        requiresCoDebtor: study?.requiresCoDebtorForApproval ?? false,
        finalConditions: study?.finalConditions ?? null
      });
      setData(rows.map((x) => x.id === data.id ? data : x));
      setNotice({ type: 'success', text: `Solicitud marcada como ${creditStatus(status)}.` });
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  const saveStep0 = async (application: CreditApplication, patch?: Partial<CreditApplication>) => {
    try {
      const { data } = await api.post<CreditApplication>(`/api/credit-applications/${application.id}/study/step0`, {
        runtChecked: patch?.runtChecked ?? application.runtChecked,
        simitChecked: patch?.simitChecked ?? application.simitChecked,
        identityValidated: patch?.identityValidated ?? application.identityValidated,
        notes: patch?.step0Notes ?? application.step0Notes ?? null
      });
      setData(rows.map((x) => x.id === data.id ? data : x));
      setNotice({ type: 'success', text: 'Validacion inicial actualizada.' });
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  const saveRecalculation = async (application: CreditApplication, patch: Partial<CreditApplication>) => {
    try {
      const { data } = await api.post<CreditApplication>(`/api/credit-applications/${application.id}/study/recalculation`, {
        approvedAmount: patch.analystApprovedAmount ?? application.analystApprovedAmount ?? application.motorcycleValue,
        approvedDownPayment: patch.approvedDownPayment ?? application.approvedDownPayment ?? application.downPayment,
        approvedTermMonths: patch.approvedTermMonths ?? application.approvedTermMonths ?? application.termMonths,
        approvedMonthlyPayment: patch.approvedMonthlyPayment ?? application.approvedMonthlyPayment ?? 0,
        notes: patch.decisionNotes ?? application.decisionNotes ?? null
      });
      setData(rows.map((x) => x.id === data.id ? data : x));
      setNotice({ type: 'success', text: 'Recalculo del analista guardado.' });
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  const updateDocument = async (application: CreditApplication, document: CreditDocument, status: number, patch?: Partial<Pick<CreditDocument, 'expiresAt' | 'notes' | 'rejectionReason'>>) => {
    try {
      const { data } = await api.put<CreditApplication>(`/api/credit-applications/${application.id}/documents/${document.id}`, {
        type: document.type,
        name: document.name,
        status,
        receivedAt: status === 2 || status === 3 ? new Date().toISOString() : document.receivedAt ?? null,
        expiresAt: patch?.expiresAt ?? document.expiresAt ?? null,
        notes: patch?.notes ?? document.notes ?? null,
        rejectionReason: patch?.rejectionReason ?? document.rejectionReason ?? null
      });
      setData(rows.map((x) => x.id === data.id ? data : x));
      setNotice({ type: 'success', text: `${document.name} actualizado.` });
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
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

  const analyzeCustomer = async (customerId: string, phone?: string) => {
    try {
      const { data } = await api.get<CustomerAiAnalysis>(`/api/customers/${customerId}/ai-analysis`);
      setAnalysis(data);
      setAnalysisPhone(phone);
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  return <Stack spacing={3}>
    <Header title="Solicitudes de credito" action="Nueva solicitud" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Solicitud', 'Cliente', 'Credito', 'Estado', 'Pendientes', 'Acciones']}
      empty="No hay solicitudes de credito"
      rows={rows.map((r) => [
        <Row primary={r.number} secondary={r.requirementProfileName || 'Sin perfil'} />,
        <Stack spacing={.5}>
          <Typography fontWeight={800}>{r.customerName}</Typography>
          <Typography variant="caption" color="text.secondary">{r.identificationNumber || 'Sin identificacion'} · {r.mobile || 'Sin telefono'}</Typography>
        </Stack>,
        <Stack spacing={.5}>
          <Typography fontWeight={800}>{r.productName}</Typography>
          <Typography variant="caption" color="text.secondary">Ingresos {money(r.monthlyIncome)} · Inicial {money(r.downPayment)}</Typography>
          <Typography variant="caption" color="text.secondary">{r.coDebtorName ? `Codeudor: ${r.coDebtorName}` : 'Sin codeudor'} · {[r.reference1Name, r.reference2Name].filter(Boolean).length || 0} ref.</Typography>
        </Stack>,
        <StatusChip label={creditStatus(r.status)} tone={creditTone(r.status)} />,
        <CreditApplicationPendingSummary application={r} />,
        <Stack direction="row" gap={1} alignItems="center" flexWrap="wrap" useFlexGap>
          <Button size="small" variant="contained" onClick={() => setManagement(r)}>Gestionar</Button>
          <Actions onAi={() => analyzeCustomer(r.customerId, r.mobile)} onEdit={() => setForm({ open: true, item: r })} />
        </Stack>
      ])}
    />
    <CreditApplicationDialog form={form} customers={customers} products={products.filter((x) => x.active)} quotes={quotes} deals={deals} requirementProfiles={requirementProfiles.filter((x) => x.active)} onClose={() => setForm({ open: false })} onSave={save} />
    <CreditApplicationManagementDialog
      application={managementApplication}
      onClose={() => setManagement(undefined)}
      onUpdateDocument={updateDocument}
      onUploadDocument={uploadDocument}
      onDownloadDocument={downloadDocument}
      onStep0={saveStep0}
      onRecalculate={saveRecalculation}
      onDecision={decide}
      onDownloadTemplate={downloadTemplate}
      onAnalyze={(application) => analyzeCustomer(application.customerId, application.mobile)}
      onEdit={(application) => setForm({ open: true, item: application })}
      onChangeStatus={(application, status) => changeStatus(application, status)}
    />
    <AiAnalysisDialog analysis={analysis} phone={analysisPhone} onClose={() => { setAnalysis(undefined); setAnalysisPhone(undefined); }} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function CreditApplicationPendingSummary({ application }: { application: CreditApplication }) {
  const validDocuments = application.documents.filter((x) => x.status === 3).length;
  const pendingDocuments = application.documents.filter((x) => x.status === 1 || x.status === 4 || x.isExpired).length;
  const step0Ready = application.runtChecked && application.simitChecked && application.identityValidated;
  const items = [
    pendingDocuments > 0 ? `${pendingDocuments} doc. pendientes` : 'Docs ok',
    step0Ready ? 'Validacion inicial lista' : 'Validacion inicial pendiente',
    application.studyResult || (application.status === 4 ? 'En estudio' : undefined),
    application.status === 5 && 'Aprobada',
    application.status === 6 && 'Negada'
  ].filter(Boolean);

  return <Stack direction="row" gap={.5} flexWrap="wrap" useFlexGap>
    <Chip size="small" color={pendingDocuments ? 'warning' : 'success'} label={`${validDocuments}/${application.documents.length} docs`} />
    <Chip size="small" color={step0Ready ? 'success' : 'warning'} variant={step0Ready ? 'filled' : 'outlined'} label={step0Ready ? 'Validacion inicial lista' : 'Validacion inicial pendiente'} />
    {application.studyResult && <Chip size="small" label={application.studyResult} variant="outlined" color={application.status === 6 ? 'error' : application.status === 5 ? 'success' : 'default'} />}
    {!application.studyResult && items.length === 0 && <Chip size="small" variant="outlined" label="Sin pendientes" />}
  </Stack>;
}

function CreditApplicationManagementDialog({
  application,
  onClose,
  onUpdateDocument,
  onUploadDocument,
  onDownloadDocument,
  onStep0,
  onRecalculate,
  onDecision,
  onDownloadTemplate,
  onAnalyze,
  onEdit,
  onChangeStatus
}: {
  application?: CreditApplication;
  onClose: () => void;
  onUpdateDocument: (application: CreditApplication, document: CreditDocument, status: number, patch?: Partial<Pick<CreditDocument, 'expiresAt' | 'notes' | 'rejectionReason'>>) => Promise<void>;
  onUploadDocument: (application: CreditApplication, document: CreditDocument, file: File) => Promise<void>;
  onDownloadDocument: (application: CreditApplication, document: CreditDocument) => Promise<void>;
  onStep0: (application: CreditApplication, patch?: Partial<CreditApplication>) => Promise<void>;
  onRecalculate: (application: CreditApplication, patch: Partial<CreditApplication>) => Promise<void>;
  onDecision: (application: CreditApplication, status: number, notes?: string, study?: Partial<CreditApplication> & { result?: string }) => Promise<void>;
  onDownloadTemplate: (application: CreditApplication, template: CreditTemplate) => Promise<void>;
  onAnalyze: (application: CreditApplication) => void;
  onEdit: (application: CreditApplication) => void;
  onChangeStatus: (application: CreditApplication, status: number) => void;
}) {
  const [tab, setTab] = useState(0);
  useEffect(() => { if (application) setTab(0); }, [application?.id]);

  if (!application) return null;

  return <Dialog open={!!application} onClose={onClose} fullWidth maxWidth="lg">
    <DialogTitle>
      <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" gap={1.5} alignItems={{ xs: 'flex-start', sm: 'center' }}>
        <Box>
          <Typography variant="h6" fontWeight={900}>Gestionar solicitud</Typography>
          <Typography variant="body2" color="text.secondary">{application.number} · {application.customerName} · {application.productName}</Typography>
        </Box>
        <StatusChip label={creditStatus(application.status)} tone={creditTone(application.status)} />
      </Stack>
    </DialogTitle>
    <DialogContent dividers sx={{ p: 0 }}>
      <Tabs value={tab} onChange={(_, value) => setTab(value)} variant="scrollable" scrollButtons="auto" sx={{ px: 2, borderBottom: '1px solid #e2e8f0' }}>
        <Tab label="Documentos" />
        <Tab label="Estudio" />
        <Tab label="Plantillas" />
        <Tab label="Acciones" />
      </Tabs>
      <Box sx={{ p: 2.5 }}>
        {tab === 0 && <Stack spacing={2}>
          <Stack direction="row" gap={.75} flexWrap="wrap" useFlexGap>
            <CreditApplicationPendingSummary application={application} />
          </Stack>
          <DocumentSummary application={application} onUpdate={onUpdateDocument} onUpload={onUploadDocument} onDownload={onDownloadDocument} />
        </Stack>}
        {tab === 1 && <CreditStudySummary application={application} onStep0={onStep0} onRecalculate={onRecalculate} onDecision={onDecision} />}
        {tab === 2 && <Stack spacing={2}>
          <Typography variant="subtitle2" fontWeight={900}>Descargar documentos</Typography>
          <CreditTemplateDownloads application={application} onDownload={onDownloadTemplate} />
        </Stack>}
        {tab === 3 && <Stack spacing={2}>
          <FieldGrid>
            <TextField select size="small" label="Estado" value={application.status} onChange={(e) => onChangeStatus(application, Number(e.target.value))}>
              {creditStatusOptions.map((x) => <MenuItem key={x} value={x}>{creditStatus(x)}</MenuItem>)}
            </TextField>
            <Box>
              <Typography variant="subtitle2" fontWeight={900} sx={{ mb: .75 }}>Acciones rapidas</Typography>
              <Stack direction="row" gap={1} flexWrap="wrap" useFlexGap>
                <Button variant="outlined" startIcon={<AutoAwesome />} onClick={() => onAnalyze(application)}>Analizar IA</Button>
                <Button variant="outlined" startIcon={<Edit />} onClick={() => onEdit(application)}>Editar solicitud</Button>
              </Stack>
            </Box>
          </FieldGrid>
          <Paper variant="outlined" sx={{ p: 1.5 }}>
            <Typography variant="subtitle2" fontWeight={900}>Resumen</Typography>
            <InfoLine label="Ingresos" value={money(application.monthlyIncome)} />
            <InfoLine label="Cuota inicial" value={money(application.downPayment)} />
            <InfoLine label="Codeudor" value={application.coDebtorName || 'Sin codeudor'} />
            <InfoLine label="Referencias" value={[application.reference1Name, application.reference2Name].filter(Boolean).join(', ') || 'Sin referencias'} />
          </Paper>
        </Stack>
        }
      </Box>
    </DialogContent>
    <DialogActions>
      <Button onClick={onClose}>Cerrar</Button>
    </DialogActions>
  </Dialog>;
}

function CreditStudySummary({ application, onStep0, onRecalculate, onDecision }: {
  application: CreditApplication;
  onStep0: (application: CreditApplication, patch?: Partial<CreditApplication>) => Promise<void>;
  onRecalculate: (application: CreditApplication, patch: Partial<CreditApplication>) => Promise<void>;
  onDecision: (application: CreditApplication, status: number, notes?: string, study?: Partial<CreditApplication> & { result?: string }) => Promise<void>;
}) {
  const actions = [
    { status: 8, label: 'Interesado', show: application.status === 1 },
    { status: 2, label: 'Documentos', show: application.status === 1 || application.status === 8 },
    { status: 4, label: 'Estudio', show: application.status === 2 || application.status === 3 },
    { status: 7, label: 'Entregar', show: application.status === 5 },
    { status: 9, label: 'Desistir', show: ![6, 7, 9].includes(application.status) }
  ].filter((x) => x.show);
  const lastDate = application.disbursedAt ?? application.approvedAt ?? application.rejectedAt ?? application.reviewStartedAt ?? application.submittedAt;
  const step0Ready = application.runtChecked && application.simitChecked && application.identityValidated;
  const approvedAmount = application.analystApprovedAmount ?? application.motorcycleValue;
  const approvedDownPayment = application.approvedDownPayment ?? application.downPayment;
  const approvedTerm = application.approvedTermMonths ?? application.termMonths;
  const approvedPayment = application.approvedMonthlyPayment ?? 0;
  const [initialValidationOpen, setInitialValidationOpen] = useState(false);
  const [initialValidation, setInitialValidation] = useState({
    runtChecked: application.runtChecked,
    simitChecked: application.simitChecked,
    identityValidated: application.identityValidated,
    notes: application.step0Notes ?? ''
  });

  const requestNumber = (label: string, current: number) => {
    const value = window.prompt(label, String(current || 0));
    if (value === null) return undefined;
    const number = Number(value.replace(/\D/g, ''));
    return Number.isFinite(number) ? number : undefined;
  };

  const openInitialValidation = () => {
    setInitialValidation({
      runtChecked: application.runtChecked,
      simitChecked: application.simitChecked,
      identityValidated: application.identityValidated,
      notes: application.step0Notes ?? ''
    });
    setInitialValidationOpen(true);
  };

  const saveInitialValidation = () => {
    onStep0(application, {
      runtChecked: initialValidation.runtChecked,
      simitChecked: initialValidation.simitChecked,
      identityValidated: initialValidation.identityValidated,
      step0Notes: initialValidation.notes
    });
    setInitialValidationOpen(false);
  };

  const recalculate = () => {
    const amount = requestNumber('Valor aprobado por analista', approvedAmount);
    if (amount === undefined) return;
    const downPayment = requestNumber('Cuota inicial aprobada', approvedDownPayment);
    if (downPayment === undefined) return;
    const term = requestNumber('Plazo aprobado en meses', approvedTerm);
    if (term === undefined) return;
    const payment = requestNumber('Cuota mensual aprobada', approvedPayment);
    if (payment === undefined) return;
    const notes = window.prompt('Observaciones del recalculo', application.decisionNotes ?? '');
    if (notes === null) return;
    onRecalculate(application, { analystApprovedAmount: amount, approvedDownPayment: downPayment, approvedTermMonths: term, approvedMonthlyPayment: payment, decisionNotes: notes });
  };

  const approve = (requiresCoDebtor: boolean, withAdjustment: boolean) => {
    const finalConditions = window.prompt('Condiciones finales para la carta', application.finalConditions ?? '');
    if (finalConditions === null) return;
    const result = withAdjustment ? 'Aprobado con ajuste' : 'Aprobado';
    onDecision(application, 5, finalConditions, {
      result,
      analystApprovedAmount: approvedAmount,
      approvedDownPayment,
      approvedTermMonths: approvedTerm,
      approvedMonthlyPayment: approvedPayment,
      requiresCoDebtorForApproval: requiresCoDebtor,
      finalConditions
    });
  };

  const reject = () => {
    const reason = window.prompt('Motivo de negacion del credito', application.decisionNotes ?? '');
    if (!reason?.trim()) return;
    onDecision(application, 6, reason.trim(), { result: 'Negado', finalConditions: reason.trim() });
  };

  return <>
  <Stack spacing={.75} sx={{ minWidth: 0 }}>
    <Stack direction="row" gap={.5} flexWrap="wrap">
      <Chip size="small" label={step0Ready ? 'Validacion inicial lista' : 'Validacion inicial pendiente'} color={step0Ready ? 'success' : 'warning'} variant={step0Ready ? 'filled' : 'outlined'} />
      {application.studyResult && <Chip size="small" label={application.studyResult} color={application.status === 6 ? 'error' : application.status === 5 ? 'success' : 'default'} variant="outlined" />}
    </Stack>
    <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.25 }}>
      {lastDate ? `${application.decisionUser ?? 'Sistema'} - ${new Date(lastDate).toLocaleDateString()}` : 'Sin decision registrada'}
    </Typography>
    <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.25 }}>
      Aprobado: {money(approvedAmount)} · Inicial: {money(approvedDownPayment)} · {approvedTerm} meses
    </Typography>
    {approvedPayment > 0 && <Typography variant="caption" color="text.secondary">Cuota analista: {money(approvedPayment)}</Typography>}
    {application.decisionNotes && <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.25 }}>{application.decisionNotes}</Typography>}
    <Stack direction="row" gap={.5} flexWrap="wrap">
      <Button size="small" variant="outlined" onClick={() => void openExternalLookup(runtUrl, application.identificationNumber)}>RUNT</Button>
      <Button size="small" variant="outlined" onClick={() => void openExternalLookup(simitUrl, application.identificationNumber)}>SIMIT</Button>
      <Button size="small" variant="outlined" onClick={openInitialValidation}>Validacion inicial</Button>
      <Button size="small" variant="outlined" onClick={recalculate}>Recalcular</Button>
    </Stack>
    <Stack direction="row" gap={.5} flexWrap="wrap">
      {actions.length ? actions.map((action) => <Button key={action.status} size="small" variant="outlined" onClick={() => onDecision(application, action.status)}>{action.label}</Button>) : <Chip size="small" label="Sin acciones" variant="outlined" />}
      {application.status === 4 && <>
        <Button size="small" variant="contained" onClick={() => approve(false, false)}>Aprobar</Button>
        <Button size="small" variant="outlined" onClick={() => approve(false, true)}>Con ajuste</Button>
        <Button size="small" variant="outlined" onClick={() => approve(true, false)}>Con codeudor</Button>
        <Button size="small" color="error" variant="outlined" onClick={reject}>Negar</Button>
      </>}
    </Stack>
  </Stack>
  <Dialog open={initialValidationOpen} onClose={() => setInitialValidationOpen(false)} fullWidth maxWidth="sm">
    <DialogTitle>Validacion inicial</DialogTitle>
    <DialogContent dividers>
      <Stack spacing={2}>
        <Alert severity="info">Confirma las consultas basicas antes de avanzar el credito a estudio.</Alert>
        <FormControlLabel
          control={<Checkbox checked={initialValidation.runtChecked} onChange={(e) => setInitialValidation((current) => ({ ...current, runtChecked: e.target.checked }))} />}
          label="RUNT consultado"
        />
        <FormControlLabel
          control={<Checkbox checked={initialValidation.simitChecked} onChange={(e) => setInitialValidation((current) => ({ ...current, simitChecked: e.target.checked }))} />}
          label="SIMIT consultado"
        />
        <FormControlLabel
          control={<Checkbox checked={initialValidation.identityValidated} onChange={(e) => setInitialValidation((current) => ({ ...current, identityValidated: e.target.checked }))} />}
          label="Identidad validada"
        />
        <TextField
          label="Observaciones"
          value={initialValidation.notes}
          onChange={(e) => setInitialValidation((current) => ({ ...current, notes: e.target.value }))}
          placeholder="Ej: RUNT y SIMIT consultados sin novedades. Identidad validada con cedula del cliente."
          multiline
          minRows={3}
          fullWidth
        />
      </Stack>
    </DialogContent>
    <DialogActions>
      <Button onClick={() => setInitialValidationOpen(false)}>Cancelar</Button>
      <Button variant="contained" onClick={saveInitialValidation}>Guardar validacion</Button>
    </DialogActions>
  </Dialog>
  </>;
}

function CollectionOrdersPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<CollectionOrder[]>('/api/collection-orders', []);
  const { data: applications = [] } = useResource<CreditApplication[]>('/api/credit-applications', []);
  const [form, setForm] = useState<FormMode<CollectionOrder>>({ open: false });
  const [notice, setNotice] = useState<Notice>();
  const eligibleApplications = applications.filter((x) => ![6, 9].includes(x.status));

  const save = async (payload: typeof emptyCollectionOrder) => {
    try {
      const body = {
        creditApplicationId: payload.creditApplicationId,
        dueDate: new Date(`${payload.dueDate}T00:00:00`).toISOString(),
        vehicleAmount: Number(payload.vehicleAmount),
        documentsAmount: Number(payload.documentsAmount),
        advanceAmount: Number(payload.advanceAmount),
        paidAmount: Number(payload.paidAmount),
        status: Number(payload.status),
        notes: payload.notes || null
      };
      const { data } = form.item
        ? await api.put<CollectionOrder>(`/api/collection-orders/${form.item.id}`, body)
        : await api.post<CollectionOrder>('/api/collection-orders', body);
      setData(form.item ? rows.map((x) => x.id === data.id ? data : x) : [data, ...rows]);
      setNotice({ type: 'success', text: form.item ? 'Orden actualizada.' : 'Orden de recaudo emitida.' });
      setForm({ open: false });
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  return <Stack spacing={3}>
    <Header title="Ordenes de recaudo" action="Nueva orden" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Numero', 'Cliente', 'Solicitud', 'Conceptos', 'Total', 'Pagado', 'Saldo', 'Vence', 'Estado', 'Acciones']}
      empty="No hay ordenes de recaudo"
      rows={rows.map((r) => [
        r.number,
        r.customerName,
        r.creditApplicationNumber,
        <Stack spacing={.25}>
          <Typography variant="body2">Vehiculo: {money(r.vehicleAmount)}</Typography>
          <Typography variant="body2">Documentos: {money(r.documentsAmount)}</Typography>
          <Typography variant="body2">Anticipo: {money(r.advanceAmount)}</Typography>
        </Stack>,
        money(r.total),
        money(r.paidAmount),
        money(r.balance),
        new Date(r.dueDate).toLocaleDateString(),
        <StatusChip label={collectionOrderStatus(r.status)} tone={collectionOrderTone(r.status)} />,
        <Actions onEdit={() => setForm({ open: true, item: r })} />
      ])}
    />
    <CollectionOrderDialog form={form} applications={eligibleApplications} onClose={() => setForm({ open: false })} onSave={save} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function CollectionOrderDialog({ form, applications, onClose, onSave }: DialogProps<CollectionOrder, typeof emptyCollectionOrder> & { applications: CreditApplication[] }) {
  const initial = form.item ? {
    creditApplicationId: form.item.creditApplicationId,
    dueDate: form.item.dueDate.slice(0, 10),
    vehicleAmount: form.item.vehicleAmount,
    documentsAmount: form.item.documentsAmount,
    advanceAmount: form.item.advanceAmount,
    paidAmount: form.item.paidAmount,
    status: form.item.status,
    notes: form.item.notes ?? ''
  } : { ...emptyCollectionOrder, creditApplicationId: applications[0]?.id ?? '' };

  return <FormDialog title={form.item ? 'Editar orden de recaudo' : 'Nueva orden de recaudo'} open={form.open} initial={initial} onClose={onClose} onSave={onSave} maxWidth="md">
    {(v, set) => {
      const application = applications.find((x) => x.id === v.creditApplicationId);
      const total = Number(v.vehicleAmount) + Number(v.documentsAmount) + Number(v.advanceAmount);
      return <Stack spacing={2}>
        <FieldGrid>
          <TextField required select label="Solicitud" value={v.creditApplicationId} onChange={(e) => {
            const selected = applications.find((x) => x.id === e.target.value);
            set({
              creditApplicationId: e.target.value,
              vehicleAmount: selected?.analystApprovedAmount ?? selected?.motorcycleValue ?? v.vehicleAmount,
              advanceAmount: selected?.approvedDownPayment ?? selected?.downPayment ?? v.advanceAmount
            });
          }}>
            {applications.map((x) => <MenuItem key={x.id} value={x.id}>{x.number} - {x.customerName}</MenuItem>)}
          </TextField>
          <TextField required label="Fecha vencimiento" type="date" value={v.dueDate} onChange={(e) => set({ dueDate: e.target.value })} InputLabelProps={{ shrink: true }} />
        </FieldGrid>
        {application && <Alert severity="info">Solicitud {application.number}: {application.customerName} · {creditStatus(application.status)}</Alert>}
        <FieldGrid columns={3}>
          <TextField label="Vehiculo" type="number" value={v.vehicleAmount} onChange={(e) => set({ vehicleAmount: Number(e.target.value) })} />
          <TextField label="Documentos" type="number" value={v.documentsAmount} onChange={(e) => set({ documentsAmount: Number(e.target.value) })} />
          <TextField label="Anticipo" type="number" value={v.advanceAmount} onChange={(e) => set({ advanceAmount: Number(e.target.value) })} />
        </FieldGrid>
        <FieldGrid columns={3}>
          <TextField label="Total" value={money(total)} InputProps={{ readOnly: true }} />
          <TextField label="Valor pagado" type="number" value={v.paidAmount} onChange={(e) => set({ paidAmount: Number(e.target.value) })} />
          <TextField select label="Estado" value={v.status} onChange={(e) => set({ status: Number(e.target.value) })}>
            {[1, 2, 3, 4, 5].map((x) => <MenuItem key={x} value={x}>{collectionOrderStatus(x)}</MenuItem>)}
          </TextField>
        </FieldGrid>
        <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} multiline minRows={2} />
      </Stack>;
    }}
  </FormDialog>;
}

function ProceduresPage() {
  const { data: rows = [], loading, error, reload, setData } = useResource<Procedure[]>('/api/procedures', []);
  const { data: applications = [] } = useResource<CreditApplication[]>('/api/credit-applications', []);
  const { data: salesPoints = [] } = useResource<SalesPoint[]>('/api/sales-points', []);
  const [form, setForm] = useState<FormMode<Procedure>>({ open: false });
  const [notice, setNotice] = useState<Notice>();
  const eligibleApplications = applications.filter((x) => ![6, 9].includes(x.status));

  const save = async (payload: typeof emptyProcedure) => {
    try {
      const body = {
        creditApplicationId: payload.creditApplicationId,
        salesPointId: payload.salesPointId || null,
        type: Number(payload.type),
        status: Number(payload.status),
        startDate: new Date(`${payload.startDate}T00:00:00`).toISOString(),
        estimatedDate: payload.estimatedDate ? new Date(`${payload.estimatedDate}T00:00:00`).toISOString() : null,
        completedAt: payload.completedAt ? new Date(`${payload.completedAt}T00:00:00`).toISOString() : null,
        responsible: payload.responsible || null,
        thirdParty: payload.thirdParty || null,
        notifyCustomer: !!payload.notifyCustomer,
        customerNotifiedAt: payload.customerNotifiedAt ? new Date(`${payload.customerNotifiedAt}T00:00:00`).toISOString() : null,
        notes: payload.notes || null
      };
      const { data } = form.item
        ? await api.put<Procedure>(`/api/procedures/${form.item.id}`, body)
        : await api.post<Procedure>('/api/procedures', body);
      setData(form.item ? rows.map((x) => x.id === data.id ? data : x) : [data, ...rows]);
      setNotice({ type: 'success', text: form.item ? 'Tramite actualizado.' : 'Tramite creado.' });
      setForm({ open: false });
    } catch (err) {
      setNotice({ type: 'error', text: apiError(err) });
    }
  };

  const notify = (row: Procedure) => {
    window.open(whatsappUrl(row.customerMobile, row.whatsappMessage), '_blank', 'noopener,noreferrer');
  };

  return <Stack spacing={3}>
    <Header title="Tramites" action="Nuevo tramite" onAction={() => setForm({ open: true })} onRefresh={reload} />
    <StatusBar loading={loading} error={error} />
    <EntityTable
      headers={['Numero', 'Cliente', 'Solicitud', 'Tipo', 'Sede', 'Fechas', 'Responsable', 'Estado', 'Notificacion', 'Acciones']}
      empty="No hay tramites registrados"
      rows={rows.map((r) => [
        r.number,
        <Row primary={r.customerName} secondary={r.productName} />,
        r.creditApplicationNumber,
        procedureType(r.type),
        r.salesPointName ?? '-',
        <Stack spacing={.25}>
          <Typography variant="body2">Inicio: {new Date(r.startDate).toLocaleDateString()}</Typography>
          <Typography variant="body2" color={r.isOverdue ? 'error' : 'text.secondary'}>Estimada: {new Date(r.estimatedDate).toLocaleDateString()}</Typography>
        </Stack>,
        <Row primary={r.responsible ?? 'Sin responsable'} secondary={r.thirdParty ?? 'Sin tercero'} />,
        <StatusChip label={procedureStatus(r.status)} tone={procedureTone(r.status)} />,
        r.notifyCustomer ? <Button size="small" startIcon={<WhatsApp />} onClick={() => notify(r)}>WhatsApp</Button> : 'No notificar',
        <Actions onEdit={() => setForm({ open: true, item: r })} />
      ])}
    />
    <ProcedureDialog form={form} applications={eligibleApplications} salesPoints={salesPoints.filter((x) => x.active)} onClose={() => setForm({ open: false })} onSave={save} />
    <Notice notice={notice} onClose={() => setNotice(undefined)} />
  </Stack>;
}

function ProcedureDialog({ form, applications, salesPoints, onClose, onSave }: DialogProps<Procedure, typeof emptyProcedure> & { applications: CreditApplication[]; salesPoints: SalesPoint[] }) {
  const initial = form.item ? {
    creditApplicationId: form.item.creditApplicationId,
    salesPointId: form.item.salesPointId ?? '',
    type: form.item.type,
    status: form.item.status,
    startDate: form.item.startDate.slice(0, 10),
    estimatedDate: form.item.estimatedDate.slice(0, 10),
    completedAt: form.item.completedAt?.slice(0, 10) ?? '',
    responsible: form.item.responsible ?? '',
    thirdParty: form.item.thirdParty ?? '',
    notifyCustomer: form.item.notifyCustomer,
    customerNotifiedAt: form.item.customerNotifiedAt?.slice(0, 10) ?? '',
    notes: form.item.notes ?? ''
  } : { ...emptyProcedure, creditApplicationId: applications[0]?.id ?? '', salesPointId: salesPoints[0]?.id ?? '' };

  return <FormDialog title={form.item ? 'Editar tramite' : 'Nuevo tramite'} open={form.open} initial={initial} onClose={onClose} onSave={onSave} maxWidth="md">
    {(v, set) => {
      const selectedSalesPoint = salesPoints.find((x) => x.id === v.salesPointId);
      return <Stack spacing={2}>
        <FieldGrid>
          <TextField required select label="Solicitud" value={v.creditApplicationId} onChange={(e) => set({ creditApplicationId: e.target.value })}>
            {applications.map((x) => <MenuItem key={x.id} value={x.id}>{x.number} - {x.customerName}</MenuItem>)}
          </TextField>
          <TextField select label="Sede / punto de venta" value={v.salesPointId} onChange={(e) => set({ salesPointId: e.target.value })}>
            <MenuItem value="">Sede por defecto</MenuItem>
            {salesPoints.map((x) => <MenuItem key={x.id} value={x.id}>{x.name} - {x.city}</MenuItem>)}
          </TextField>
        </FieldGrid>
        {selectedSalesPoint && <Alert severity="info">Tiempos de sede: SOAT {selectedSalesPoint.soatDays} dia(s), matricula {selectedSalesPoint.registrationDays} dia(s).</Alert>}
        <FieldGrid columns={3}>
          <TextField select label="Tipo" value={v.type} onChange={(e) => set({ type: Number(e.target.value) })}>
            {[1, 2, 3, 4].map((x) => <MenuItem key={x} value={x}>{procedureType(x)}</MenuItem>)}
          </TextField>
          <TextField select label="Estado" value={v.status} onChange={(e) => set({ status: Number(e.target.value) })}>
            {[1, 2, 3, 4, 5].map((x) => <MenuItem key={x} value={x}>{procedureStatus(x)}</MenuItem>)}
          </TextField>
          <TextField label="Responsable interno" value={v.responsible} onChange={(e) => set({ responsible: e.target.value })} />
        </FieldGrid>
        <FieldGrid columns={3}>
          <TextField label="Fecha inicio" type="date" value={v.startDate} onChange={(e) => set({ startDate: e.target.value })} InputLabelProps={{ shrink: true }} />
          <TextField label="Fecha estimada" type="date" value={v.estimatedDate} onChange={(e) => set({ estimatedDate: e.target.value })} helperText="Dejalo vacio para calcular por sede." InputLabelProps={{ shrink: true }} />
          <TextField label="Fecha finalizacion" type="date" value={v.completedAt} onChange={(e) => set({ completedAt: e.target.value })} InputLabelProps={{ shrink: true }} />
        </FieldGrid>
        <FieldGrid>
          <TextField label="Tercero / proveedor" value={v.thirdParty} onChange={(e) => set({ thirdParty: e.target.value })} />
          <TextField label="Fecha notificacion cliente" type="date" value={v.customerNotifiedAt} onChange={(e) => set({ customerNotifiedAt: e.target.value })} InputLabelProps={{ shrink: true }} />
        </FieldGrid>
        <FormControlLabel control={<Checkbox checked={v.notifyCustomer} onChange={(e) => set({ notifyCustomer: e.target.checked })} />} label="Notificar cliente por WhatsApp" />
        <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} multiline minRows={2} />
      </Stack>;
    }}
  </FormDialog>;
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
      deliveryProtocol: payload.deliveryProtocol || null,
      deliveryPhotoDataUrl: payload.deliveryPhotoDataUrl || null,
      deliveryPhotoFileName: payload.deliveryPhotoFileName || null,
      firstServiceScheduledAt: payload.firstServiceScheduledAt ? new Date(payload.firstServiceScheduledAt).toISOString() : null,
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
      headers={['Numero', 'Cliente', 'Producto', 'Estado', 'Fecha', 'Tecnicos', 'Protocolo', 'Acciones']}
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
        <Stack spacing={1}>
          <DeliveryChecklist delivery={r} />
          <Row primary={r.deliveryPhotoDataUrl ? 'Foto registrada' : 'Sin foto'} secondary={r.firstServiceScheduledAt ? `Revision: ${new Date(r.firstServiceScheduledAt).toLocaleString()}` : 'Revision pendiente'} />
        </Stack>,
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
    ['Acta', delivery.deliveryCertificateSigned],
    ['Checklist', delivery.preDeliveryChecklistCompleted],
    ['Foto', !!delivery.deliveryPhotoDataUrl]
  ];
  return <Stack direction="row" gap={.5} flexWrap="wrap">
    {items.map(([label, ok]) => <Chip key={String(label)} size="small" label={label} color={ok ? 'success' : undefined} variant={ok ? 'filled' : 'outlined'} />)}
  </Stack>;
}

function DeliveryPhotoPicker({ value, fileName, onChange }: { value?: string; fileName?: string; onChange: (value: string, fileName: string) => void }) {
  const [error, setError] = useState('');
  const handlePhoto = async (file?: File) => {
    if (!file) return;
    try {
      setError('');
      onChange(await normalizeDeliveryPhoto(file), file.name);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'No se pudo procesar la foto.');
    }
  };

  return <Stack spacing={1}>
    <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ xs: 'stretch', sm: 'center' }}>
      <Box sx={{
        width: 150,
        height: 96,
        border: '1px dashed',
        borderColor: 'divider',
        borderRadius: 1,
        bgcolor: 'background.default',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        overflow: 'hidden'
      }}>
        {value ? <Box component="img" src={value} alt="Foto de entrega" sx={{ width: '100%', height: '100%', objectFit: 'cover' }} /> : <Typography color="text.secondary" fontSize={13}>Foto entrega</Typography>}
      </Box>
      <Stack spacing={1} sx={{ minWidth: 0 }}>
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
          <Button variant="outlined" component="label" startIcon={<UploadFile />}>
            Adjuntar foto
            <input hidden type="file" accept="image/png,image/jpeg,image/webp" onChange={(e) => void handlePhoto(e.target.files?.[0])} />
          </Button>
          {value && <Button color="inherit" onClick={() => onChange('', '')}>Quitar</Button>}
        </Stack>
        <Typography variant="caption" color="text.secondary">{fileName || 'PNG, JPG o WebP hasta 1 MB.'}</Typography>
      </Stack>
    </Stack>
    {error && <Alert severity="error">{error}</Alert>}
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
    preDeliveryChecklistCompleted: form.item.preDeliveryChecklistCompleted,
    deliveryProtocol: form.item.deliveryProtocol ?? '',
    deliveryPhotoDataUrl: form.item.deliveryPhotoDataUrl ?? '',
    deliveryPhotoFileName: form.item.deliveryPhotoFileName ?? '',
    firstServiceScheduledAt: form.item.firstServiceScheduledAt ? toInputDateTime(form.item.firstServiceScheduledAt) : '',
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
      <Divider />
      <Typography variant="subtitle2" fontWeight={900} color="primary">Protocolo digital y evidencia</Typography>
      <TextField label="Protocolo por marca" value={v.deliveryProtocol} onChange={(e) => set({ deliveryProtocol: e.target.value })} fullWidth multiline minRows={2} helperText="Si se deja vacio, el sistema aplica un protocolo base segun la marca del producto." />
      <DeliveryPhotoPicker
        value={v.deliveryPhotoDataUrl}
        fileName={v.deliveryPhotoFileName}
        onChange={(deliveryPhotoDataUrl, deliveryPhotoFileName) => set({ deliveryPhotoDataUrl, deliveryPhotoFileName })}
      />
      <Divider />
      <Typography variant="subtitle2" fontWeight={900} color="primary">Checklist obligatorio</Typography>
      <Grid container spacing={1}>
        <Grid item xs={12} sm={6}><FormControlLabel control={<Checkbox checked={v.helmetDelivered} onChange={(e) => set({ helmetDelivered: e.target.checked })} />} label="Casco entregado" /></Grid>
        <Grid item xs={12} sm={6}><FormControlLabel control={<Checkbox checked={v.soatDelivered} onChange={(e) => set({ soatDelivered: e.target.checked })} />} label="SOAT entregado" /></Grid>
        <Grid item xs={12} sm={6}><FormControlLabel control={<Checkbox checked={v.registrationDelivered} onChange={(e) => set({ registrationDelivered: e.target.checked })} />} label="Matricula entregada" /></Grid>
        <Grid item xs={12} sm={6}><FormControlLabel control={<Checkbox checked={v.warrantyManualDelivered} onChange={(e) => set({ warrantyManualDelivered: e.target.checked })} />} label="Manual/garantia" /></Grid>
        <Grid item xs={12}><FormControlLabel control={<Checkbox checked={v.deliveryCertificateSigned} onChange={(e) => set({ deliveryCertificateSigned: e.target.checked })} />} label="Acta de entrega firmada" /></Grid>
        <Grid item xs={12}><FormControlLabel control={<Checkbox checked={v.preDeliveryChecklistCompleted} onChange={(e) => set({ preDeliveryChecklistCompleted: e.target.checked })} />} label="Checklist preentrega completado" /></Grid>
      </Grid>
      <TextField type="datetime-local" label="Primera revision" value={v.firstServiceScheduledAt} onChange={(e) => set({ firstServiceScheduledAt: e.target.value })} fullWidth InputLabelProps={{ shrink: true }} helperText="Si queda vacia y la entrega se marca como entregada, se agenda automaticamente a 30 dias." />
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
  return <Stack direction="row" gap={.5} flexWrap="wrap">
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
  const [reschedule, setReschedule] = useState<Activity>();
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

  const saveReschedule = async (scheduledAt: string) => {
    if (!reschedule) return;
    const reminderAt = reschedule.reminderAt
      ? moveReminderKeepingOffset(reschedule.scheduledAt, reschedule.reminderAt, scheduledAt)
      : undefined;
    await updateActivity(reschedule, { scheduledAt, reminderAt }, 'Actividad reprogramada.');
    setReschedule(undefined);
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
          onReschedule={r.status === 1 || r.status === 2 ? () => setReschedule(r) : undefined}
          onCancel={r.status !== 4 ? () => updateActivity(r, { status: 4 }, 'Actividad cancelada.') : undefined}
          onEdit={() => setForm({ open: true, item: r })}
          onDelete={canDelete ? () => setConfirm(r) : undefined}
        />
      ])}
    />
    <ActivityDialog form={form} customers={customers} deals={deals} onClose={() => setForm({ open: false })} onSave={save} />
    <RescheduleActivityDialog activity={reschedule} onClose={() => setReschedule(undefined)} onSave={saveReschedule} />
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
  const { data: products = [] } = useResource<Product[]>('/api/products', []);
  const { data: financialSettings, loading: loadingFinancialSettings, error: financialSettingsError, reload: reloadFinancialSettings, setData: setFinancialSettings } = useResource<FinancialSettings>('/api/financial-settings');
  const { data: salesPoints = [], loading: loadingSalesPoints, error: salesPointsError, reload: reloadSalesPoints, setData: setSalesPoints } = useResource<SalesPoint[]>('/api/sales-points', []);
  const { data: requirementProfiles = [], loading: loadingRequirementProfiles, error: requirementProfilesError, reload: reloadRequirementProfiles, setData: setRequirementProfiles } = useResource<RequirementProfile[]>('/api/requirement-profiles', []);
  const { data: promotions = [], loading: loadingPromotions, error: promotionsError, reload: reloadPromotions, setData: setPromotions } = useResource<Promotion[]>('/api/promotions', []);
  const [companyForm, setCompanyForm] = useState<FormMode<Company>>({ open: false });
  const [userForm, setUserForm] = useState<FormMode<User>>({ open: false });
  const [financialForm, setFinancialForm] = useState<FormMode<FinancialSettings>>({ open: false });
  const [salesPointForm, setSalesPointForm] = useState<FormMode<SalesPoint>>({ open: false });
  const [requirementProfileForm, setRequirementProfileForm] = useState<FormMode<RequirementProfile>>({ open: false });
  const [promotionForm, setPromotionForm] = useState<FormMode<Promotion>>({ open: false });
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
    const { data } = await api.post<User>('/api/users', { ...payload, salesPointId: payload.salesPointId || null });
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

  const saveSalesPoint = async (payload: typeof emptySalesPoint) => {
    const body = {
      name: payload.name,
      code: payload.code,
      city: payload.city,
      address: payload.address || null,
      phone: payload.phone || null,
      mainBrand: payload.mainBrand,
      brandLogoDataUrl: payload.brandLogoDataUrl || null,
      factorMonthlyRate: Number(payload.factorMonthlyRate),
      maxTermMonths: Number(payload.maxTermMonths),
      quoteValidityDays: Number(payload.quoteValidityDays),
      deliveryMode: payload.deliveryMode,
      soatDays: Number(payload.soatDays),
      registrationDays: Number(payload.registrationDays),
      soatProvider: payload.soatProvider || null,
      registrationAgent: payload.registrationAgent || null,
      commercialTerms: payload.commercialTerms || null,
      active: Boolean(payload.active)
    };
    const { data } = salesPointForm.item
      ? await api.put<SalesPoint>(`/api/sales-points/${salesPointForm.item.id}`, body)
      : await api.post<SalesPoint>('/api/sales-points', body);
    setSalesPoints(salesPointForm.item ? salesPoints.map((x) => x.id === data.id ? data : x) : [...salesPoints, data].sort((a, b) => a.city.localeCompare(b.city) || a.name.localeCompare(b.name)));
    setNotice({ type: 'success', text: salesPointForm.item ? 'Sede actualizada.' : 'Sede creada.' });
    setSalesPointForm({ open: false });
  };

  const saveRequirementProfile = async (payload: typeof emptyRequirementProfile) => {
    const documents = payload.documents
      .filter((document) => document.name.trim())
      .map((document, index) => ({
        type: Number(document.type),
        name: document.name,
        description: document.description || null,
        required: Boolean(document.required),
        order: Number(document.order) > 0 ? Number(document.order) : index + 1
      }));
    const body = {
      name: payload.name,
      code: payload.code,
      description: payload.description || null,
      isCash: Boolean(payload.isCash),
      active: Boolean(payload.active),
      documents
    };
    const { data } = requirementProfileForm.item
      ? await api.put<RequirementProfile>(`/api/requirement-profiles/${requirementProfileForm.item.id}`, body)
      : await api.post<RequirementProfile>('/api/requirement-profiles', body);
    setRequirementProfiles(requirementProfileForm.item ? requirementProfiles.map((x) => x.id === data.id ? data : x) : [...requirementProfiles, data].sort((a, b) => a.name.localeCompare(b.name)));
    setNotice({ type: 'success', text: requirementProfileForm.item ? 'Perfil actualizado.' : 'Perfil creado.' });
    setRequirementProfileForm({ open: false });
  };

  const savePromotion = async (payload: typeof emptyPromotion) => {
    const body = {
      name: payload.name,
      code: payload.code,
      discountType: payload.discountType,
      discountValue: Number(payload.discountValue),
      productId: payload.productId || null,
      brand: payload.brand || null,
      color: payload.color || null,
      salesPointId: payload.salesPointId || null,
      validFrom: payload.validFrom,
      validUntil: payload.validUntil,
      active: Boolean(payload.active)
    };
    const { data } = promotionForm.item
      ? await api.put<Promotion>(`/api/promotions/${promotionForm.item.id}`, body)
      : await api.post<Promotion>('/api/promotions', body);
    setPromotions(promotionForm.item ? promotions.map((x) => x.id === data.id ? data : x) : [data, ...promotions]);
    setNotice({ type: 'success', text: promotionForm.item ? 'Promocion actualizada.' : 'Promocion creada.' });
    setPromotionForm({ open: false });
  };

  return <Stack spacing={3}>
    <Header title="Configuracion" onRefresh={() => { reloadCompanies(); reloadUsers(); reloadFinancialSettings(); reloadSalesPoints(); reloadRequirementProfiles(); reloadPromotions(); }} />
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
            <Typography color="text.secondary" fontSize={14}>Condiciones usadas por la empresa al crear cotizaciones.</Typography>
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
          <Grid item xs={6} md={3}><Metric label="Tabla financiera" value={financialSettings.useMontelibanoTable ? 'Activa' : 'Manual'} /></Grid>
          <Grid item xs={6} md={3}><Metric label="Estado" value={financialSettings.active ? 'Activa' : 'Inactiva'} /></Grid>
        </Grid>}
      </Stack>
    </CardContent></Card>
    <Card><CardContent>
      <Stack spacing={2}>
        <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'stretch', sm: 'center' }} gap={1}>
          <Box>
            <Typography variant="h5" fontWeight={900}>Sedes / puntos de venta</Typography>
            <Typography color="text.secondary" fontSize={14}>Base para tasas por sede, logos de marca, tramites, promociones y entregas.</Typography>
          </Box>
          {canManage && <Button variant="contained" startIcon={<Add />} onClick={() => setSalesPointForm({ open: true })}>Nueva sede</Button>}
        </Stack>
        <StatusBar loading={loadingSalesPoints} error={salesPointsError} />
        <EntityTable
          headers={['Marca', 'Sede', 'Ciudad', 'Entrega', 'Tasa', 'Tramites', 'Estado', 'Acciones']}
          empty="No hay sedes registradas"
          rows={salesPoints.map((point) => [
            <Stack direction="row" spacing={1} alignItems="center">
              {point.brandLogoDataUrl && <Box component="img" src={point.brandLogoDataUrl} alt={point.mainBrand} sx={{ width: 42, height: 28, objectFit: 'contain' }} />}
              <Typography fontWeight={800}>{point.mainBrand}</Typography>
            </Stack>,
            <Box><Typography fontWeight={800}>{point.name}</Typography><Typography color="text.secondary" fontSize={12}>{point.code}</Typography></Box>,
            point.city,
            point.deliveryMode === 'Completa' ? 'Completa' : 'Con SOAT',
            `${point.factorMonthlyRate}% / ${point.maxTermMonths} meses`,
            `SOAT ${point.soatDays}d · Matricula ${point.registrationDays}d`,
            <StatusChip label={point.active ? 'Activa' : 'Inactiva'} tone={point.active ? 'success' : 'default'} />,
            <Actions onEdit={canManage ? () => setSalesPointForm({ open: true, item: point }) : undefined} />
          ])}
        />
      </Stack>
    </CardContent></Card>
    <Card><CardContent>
      <Stack spacing={2}>
        <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'stretch', sm: 'center' }} gap={1}>
          <Box>
            <Typography variant="h5" fontWeight={900}>Perfiles de requisitos</Typography>
            <Typography color="text.secondary" fontSize={14}>Checklist documental por tipo de cliente o forma de pago.</Typography>
          </Box>
          {canManage && <Button variant="contained" startIcon={<Add />} onClick={() => setRequirementProfileForm({ open: true })}>Nuevo perfil</Button>}
        </Stack>
        <StatusBar loading={loadingRequirementProfiles} error={requirementProfilesError} />
        <EntityTable
          headers={['Perfil', 'Tipo', 'Documentos', 'Estado', 'Acciones']}
          empty="No hay perfiles de requisitos registrados"
          rows={requirementProfiles.map((profile) => [
            <Box><Typography fontWeight={800}>{profile.name}</Typography><Typography color="text.secondary" fontSize={12}>{profile.code}</Typography></Box>,
            profile.isCash ? 'Contado' : 'Credito',
            <Stack direction="row" gap={.5} flexWrap="wrap">{profile.documents.slice(0, 4).map((document) => <Chip key={document.id} size="small" label={document.name} variant="outlined" />)}{profile.documents.length > 4 && <Chip size="small" label={`+${profile.documents.length - 4}`} />}</Stack>,
            <StatusChip label={profile.active ? 'Activo' : 'Inactivo'} tone={profile.active ? 'success' : 'default'} />,
            <Actions onEdit={canManage ? () => setRequirementProfileForm({ open: true, item: profile }) : undefined} />
          ])}
        />
      </Stack>
    </CardContent></Card>
    <Card><CardContent>
      <Stack spacing={2}>
        <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'stretch', sm: 'center' }} gap={1}>
          <Box>
            <Typography variant="h5" fontWeight={900}>Promociones / planes tacticos</Typography>
            <Typography color="text.secondary" fontSize={14}>Descuentos automaticos por producto, marca, color, sede y vigencia.</Typography>
          </Box>
          {canManage && <Button variant="contained" startIcon={<Add />} onClick={() => setPromotionForm({ open: true })}>Nueva promocion</Button>}
        </Stack>
        <StatusBar loading={loadingPromotions} error={promotionsError} />
        <EntityTable
          headers={['Promocion', 'Descuento', 'Alcance', 'Vigencia', 'Estado', 'Acciones']}
          empty="No hay promociones registradas"
          rows={promotions.map((promotion) => [
            <Box><Typography fontWeight={800}>{promotion.name}</Typography><Typography color="text.secondary" fontSize={12}>{promotion.code}</Typography></Box>,
            promotion.discountType === 'Porcentaje' ? `${promotion.discountValue}%` : money(promotion.discountValue),
            [promotion.productName, promotion.brand, promotion.color, promotion.salesPointName].filter(Boolean).join(' / ') || 'General',
            `${new Date(promotion.validFrom).toLocaleDateString()} - ${new Date(promotion.validUntil).toLocaleDateString()}`,
            <StatusChip label={promotion.active ? 'Activa' : 'Inactiva'} tone={promotion.active ? 'success' : 'default'} />,
            <Actions onEdit={canManage ? () => setPromotionForm({ open: true, item: promotion }) : undefined} />
          ])}
        />
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
        headers={['Nombre', 'Email', 'Empresa', 'Sede', 'Roles']}
        empty="No hay usuarios registrados"
        rows={users.map((u) => [
          u.fullName,
          u.email,
          companies.find((c) => c.id === u.companyId)?.name ?? u.companyId,
          u.salesPointName ?? salesPoints.find((p) => p.id === u.salesPointId)?.name ?? '-',
          u.roles.join(', ')
        ])}
      />
    </>}
    <CompanyDialog form={companyForm} onClose={() => setCompanyForm({ open: false })} onSave={saveCompany} />
    <SalesPointDialog form={salesPointForm} onClose={() => setSalesPointForm({ open: false })} onSave={saveSalesPoint} />
    <RequirementProfileDialog form={requirementProfileForm} onClose={() => setRequirementProfileForm({ open: false })} onSave={saveRequirementProfile} />
    <PromotionDialog form={promotionForm} products={products.filter((x) => x.active)} salesPoints={salesPoints.filter((x) => x.active)} onClose={() => setPromotionForm({ open: false })} onSave={savePromotion} />
    <UserDialog form={userForm} companies={companies.filter((x) => x.active)} salesPoints={salesPoints.filter((x) => x.active)} onClose={() => setUserForm({ open: false })} onSave={saveUser} />
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

function SalesPointDialog({ form, onClose, onSave }: DialogProps<SalesPoint, typeof emptySalesPoint>) {
  const initial = form.item ? {
    name: form.item.name,
    code: form.item.code,
    city: form.item.city,
    address: form.item.address ?? '',
    phone: form.item.phone ?? '',
    mainBrand: form.item.mainBrand,
    brandLogoDataUrl: form.item.brandLogoDataUrl ?? '',
    factorMonthlyRate: form.item.factorMonthlyRate,
    maxTermMonths: form.item.maxTermMonths,
    quoteValidityDays: form.item.quoteValidityDays,
    deliveryMode: form.item.deliveryMode,
    soatDays: form.item.soatDays,
    registrationDays: form.item.registrationDays,
    soatProvider: form.item.soatProvider ?? '',
    registrationAgent: form.item.registrationAgent ?? '',
    commercialTerms: form.item.commercialTerms ?? '',
    active: form.item.active
  } : emptySalesPoint;

  return <FormDialog title={form.item ? 'Editar sede' : 'Nueva sede'} open={form.open} initial={initial} onClose={onClose} onSave={onSave} maxWidth="md">
    {(v, set) => <>
      <CompanyLogoPicker title="Logo de marca" helper="PNG, JPG o WebP. Se usara luego en cotizaciones y documentos por sede." value={v.brandLogoDataUrl} onChange={(brandLogoDataUrl) => set({ brandLogoDataUrl })} />
      <Grid container spacing={1.5}>
        <Grid item xs={12} md={6}><TextField fullWidth required label="Nombre de la sede" value={v.name} onChange={(e) => set({ name: e.target.value })} /></Grid>
        <Grid item xs={12} md={3}><TextField fullWidth required label="Codigo" value={v.code} onChange={(e) => set({ code: e.target.value.toUpperCase().replace(/[^A-Z0-9-]/g, '') })} /></Grid>
        <Grid item xs={12} md={3}><TextField fullWidth required label="Ciudad" value={v.city} onChange={(e) => set({ city: e.target.value })} /></Grid>
        <Grid item xs={12} md={8}><TextField fullWidth label="Direccion" value={v.address} onChange={(e) => set({ address: e.target.value })} /></Grid>
        <Grid item xs={12} md={4}><TextField fullWidth label="Telefono" value={v.phone} onChange={(e) => set({ phone: e.target.value })} /></Grid>
      </Grid>
      <Grid container spacing={1.5}>
        <Grid item xs={12} md={4}><TextField fullWidth required label="Marca principal" value={v.mainBrand} onChange={(e) => set({ mainBrand: e.target.value })} /></Grid>
        <Grid item xs={12} md={4}><TextField fullWidth required type="number" label="Tasa factor mensual (%)" value={v.factorMonthlyRate} onChange={(e) => set({ factorMonthlyRate: Number(e.target.value) })} /></Grid>
        <Grid item xs={12} md={4}><TextField fullWidth required type="number" label="Plazo maximo" value={v.maxTermMonths} onChange={(e) => set({ maxTermMonths: Number(e.target.value) })} /></Grid>
      </Grid>
      <Grid container spacing={1.5}>
        <Grid item xs={12} md={4}>
          <TextField fullWidth select label="Modalidad de entrega" value={v.deliveryMode} onChange={(e) => set({ deliveryMode: e.target.value })}>
            <MenuItem value="ConSoat">Entrega con SOAT</MenuItem>
            <MenuItem value="Completa">Entrega completa</MenuItem>
          </TextField>
        </Grid>
        <Grid item xs={12} md={4}><TextField fullWidth required type="number" label="Vigencia cotizacion" value={v.quoteValidityDays} onChange={(e) => set({ quoteValidityDays: Number(e.target.value) })} /></Grid>
        <Grid item xs={12} md={2}><TextField fullWidth required type="number" label="Dias SOAT" value={v.soatDays} onChange={(e) => set({ soatDays: Number(e.target.value) })} /></Grid>
        <Grid item xs={12} md={2}><TextField fullWidth required type="number" label="Dias matricula" value={v.registrationDays} onChange={(e) => set({ registrationDays: Number(e.target.value) })} /></Grid>
        <Grid item xs={12} md={6}><TextField fullWidth label="Proveedor SOAT" value={v.soatProvider} onChange={(e) => set({ soatProvider: e.target.value })} /></Grid>
        <Grid item xs={12} md={6}><TextField fullWidth label="Tramitador matricula" value={v.registrationAgent} onChange={(e) => set({ registrationAgent: e.target.value })} /></Grid>
        <Grid item xs={12}><TextField fullWidth multiline minRows={2} label="Condiciones comerciales para cotizacion" value={v.commercialTerms} onChange={(e) => set({ commercialTerms: e.target.value })} /></Grid>
      </Grid>
      <FormControlLabel control={<Checkbox checked={v.active} onChange={(e) => set({ active: e.target.checked })} />} label="Sede activa" />
    </>}
  </FormDialog>;
}

function CompanyLogoPicker({ value, onChange, title = 'Logo de la empresa', helper = 'PNG, JPG o WebP. Se ajusta automaticamente a 320 x 160 px.' }: { value?: string; onChange: (value: string) => void; title?: string; helper?: string }) {
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
    <Typography variant="subtitle2" fontWeight={900} color="primary">{title}</Typography>
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
        <Typography variant="caption" color="text.secondary">{helper}</Typography>
      </Stack>
    </Stack>
    {error && <Alert severity="error">{error}</Alert>}
  </Stack>;
}

function RequirementProfileDialog({ form, onClose, onSave }: DialogProps<RequirementProfile, typeof emptyRequirementProfile>) {
  const initial = form.item ? {
    name: form.item.name,
    code: form.item.code,
    description: form.item.description ?? '',
    isCash: form.item.isCash,
    active: form.item.active,
    documents: form.item.documents.length ? form.item.documents.map((document) => ({
      type: document.type,
      name: document.name,
      description: document.description ?? '',
      required: document.required,
      order: document.order
    })) : [emptyRequirementDocument]
  } : emptyRequirementProfile;

  return <FormDialog title={form.item ? 'Editar perfil de requisitos' : 'Nuevo perfil de requisitos'} open={form.open} initial={initial} onClose={onClose} onSave={onSave} maxWidth="md">
    {(v, set) => {
      const documents = v.documents.length ? v.documents : [emptyRequirementDocument];
      const updateDocument = (index: number, patch: Partial<typeof emptyRequirementDocument>) => {
        set({ documents: documents.map((document, documentIndex) => documentIndex === index ? { ...document, ...patch } : document) });
      };
      const addDocument = () => set({ documents: [...documents, { ...emptyRequirementDocument, order: documents.length + 1 }] });
      const removeDocument = (index: number) => set({ documents: documents.filter((_, documentIndex) => documentIndex !== index).map((document, documentIndex) => ({ ...document, order: documentIndex + 1 })) });

      return <>
        <SectionTitle title="Perfil" />
        <FieldGrid columns={2}>
          <TextField fullWidth required label="Nombre" value={v.name} onChange={(e) => set({ name: e.target.value })} />
          <TextField fullWidth required label="Codigo" value={v.code} onChange={(e) => set({ code: e.target.value.toUpperCase().replace(/[^A-Z0-9_ -]/g, '').replace(/\s+/g, '_') })} />
        </FieldGrid>
        <TextField label="Descripcion" value={v.description} onChange={(e) => set({ description: e.target.value })} multiline minRows={2} />
        <Stack direction={{ xs: 'column', sm: 'row' }} gap={1}>
          <FormControlLabel control={<Checkbox checked={v.isCash} onChange={(e) => set({ isCash: e.target.checked })} />} label="Perfil para venta de contado" />
          <FormControlLabel control={<Checkbox checked={v.active} onChange={(e) => set({ active: e.target.checked })} />} label="Perfil activo" />
        </Stack>
        <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'stretch', sm: 'center' }} gap={1}>
          <SectionTitle title="Documentos requeridos" />
          <Button type="button" variant="outlined" startIcon={<Add />} onClick={addDocument}>Agregar documento</Button>
        </Stack>
        <Stack spacing={1.25}>
          {documents.map((document, index) => <Paper key={index} variant="outlined" sx={{ p: 1.5, bgcolor: '#f8fafc' }}>
            <Box sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', md: '80px minmax(160px, 1fr) minmax(220px, 1.3fr) 120px 48px' },
              gap: 1,
              alignItems: 'center'
            }}>
              <TextField fullWidth label="Orden" type="number" value={document.order} onChange={(e) => updateDocument(index, { order: Number(e.target.value) })} />
              <TextField fullWidth select label="Tipo" value={document.type} onChange={(e) => updateDocument(index, { type: Number(e.target.value) })}>
                {[1, 2, 3, 4, 5].map((type) => <MenuItem key={type} value={type}>{documentType(type)}</MenuItem>)}
              </TextField>
              <TextField fullWidth required label="Documento" value={document.name} onChange={(e) => updateDocument(index, { name: e.target.value })} />
              <FormControlLabel control={<Checkbox checked={document.required} onChange={(e) => updateDocument(index, { required: e.target.checked })} />} label="Obligatorio" />
              <IconButton color="error" disabled={documents.length === 1} onClick={() => removeDocument(index)}><Delete fontSize="small" /></IconButton>
            </Box>
            <TextField fullWidth sx={{ mt: 1 }} label="Nota para el asesor" value={document.description} onChange={(e) => updateDocument(index, { description: e.target.value })} />
          </Paper>)}
        </Stack>
      </>;
    }}
  </FormDialog>;
}

function PromotionDialog({ form, products, salesPoints, onClose, onSave }: DialogProps<Promotion, typeof emptyPromotion> & { products: Product[]; salesPoints: SalesPoint[] }) {
  const initial = form.item ? {
    name: form.item.name,
    code: form.item.code,
    discountType: form.item.discountType,
    discountValue: form.item.discountValue,
    productId: form.item.productId ?? '',
    brand: form.item.brand ?? '',
    color: form.item.color ?? '',
    salesPointId: form.item.salesPointId ?? '',
    validFrom: form.item.validFrom?.slice(0, 10) ?? today,
    validUntil: form.item.validUntil?.slice(0, 10) ?? today,
    active: form.item.active
  } : emptyPromotion;

  return <FormDialog title={form.item ? 'Editar promocion' : 'Nueva promocion'} open={form.open} initial={initial} onClose={onClose} onSave={onSave} maxWidth="md">
    {(v, set) => <>
      <SectionTitle title="Datos de la promocion" />
      <FieldGrid columns={2}>
        <TextField fullWidth required label="Nombre" value={v.name} onChange={(e) => set({ name: e.target.value })} />
        <TextField fullWidth required label="Codigo" value={v.code} onChange={(e) => set({ code: e.target.value.toUpperCase().replace(/[^A-Z0-9_ -]/g, '').replace(/\s+/g, '_') })} />
      </FieldGrid>
      <FieldGrid columns={3}>
        <TextField fullWidth select label="Tipo descuento" value={v.discountType} onChange={(e) => set({ discountType: e.target.value })}>
          <MenuItem value="Valor">Valor fijo</MenuItem>
          <MenuItem value="Porcentaje">Porcentaje</MenuItem>
        </TextField>
        <TextField fullWidth required type="number" label={v.discountType === 'Porcentaje' ? 'Porcentaje' : 'Valor descuento'} value={v.discountValue} onChange={(e) => set({ discountValue: Number(e.target.value) })} />
        <FormControlLabel control={<Checkbox checked={v.active} onChange={(e) => set({ active: e.target.checked })} />} label="Promocion activa" />
      </FieldGrid>
      <SectionTitle title="Alcance" />
      <FieldGrid columns={2}>
        <TextField fullWidth select label="Producto especifico" value={v.productId} onChange={(e) => set({ productId: e.target.value })}>
          <MenuItem value="">Cualquier producto</MenuItem>
          {products.map((product) => <MenuItem key={product.id} value={product.id}>{productName(product)} - {money(product.price)}</MenuItem>)}
        </TextField>
        <TextField fullWidth select label="Sede" value={v.salesPointId} onChange={(e) => set({ salesPointId: e.target.value })}>
          <MenuItem value="">Todas las sedes</MenuItem>
          {salesPoints.map((point) => <MenuItem key={point.id} value={point.id}>{point.name} - {point.city}</MenuItem>)}
        </TextField>
      </FieldGrid>
      <FieldGrid columns={2}>
        <TextField fullWidth label="Marca" value={v.brand} onChange={(e) => set({ brand: e.target.value })} helperText="Opcional. Si se llena, solo aplica a esa marca." />
        <TextField fullWidth label="Color" value={v.color} onChange={(e) => set({ color: e.target.value })} helperText="Opcional. Si se llena, solo aplica a ese color." />
      </FieldGrid>
      <SectionTitle title="Vigencia" />
      <FieldGrid columns={2}>
        <TextField fullWidth required type="date" label="Desde" value={v.validFrom} onChange={(e) => set({ validFrom: e.target.value })} InputLabelProps={{ shrink: true }} />
        <TextField fullWidth required type="date" label="Hasta" value={v.validUntil} onChange={(e) => set({ validUntil: e.target.value })} InputLabelProps={{ shrink: true }} />
      </FieldGrid>
      <Alert severity="info">La cotizacion aplicara automaticamente la promocion vigente mas especifica para producto, marca, color y sede.</Alert>
    </>}
  </FormDialog>;
}

function UserDialog({ form, companies, salesPoints, onClose, onSave }: DialogProps<User, typeof emptyUser> & { companies: Company[]; salesPoints: SalesPoint[] }) {
  const initialCompanyId = companies[0]?.id ?? '';
  const initial = { ...emptyUser, companyId: initialCompanyId, salesPointId: salesPoints[0]?.id ?? '' };
  return <FormDialog title="Nuevo usuario" open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => {
      const currentSalesPoints = salesPoints;
      return <>
      <TextField required label="Nombre completo" value={v.fullName} onChange={(e) => set({ fullName: e.target.value })} />
      <TextField required label="Email" value={v.email} onChange={(e) => set({ email: e.target.value })} />
      <TextField required label="Contrasena temporal" type="password" value={v.password} onChange={(e) => set({ password: e.target.value })} />
      <TextField required select label="Empresa" value={v.companyId} onChange={(e) => set({ companyId: e.target.value, salesPointId: currentSalesPoints[0]?.id ?? '' })}>{companies.map((c) => <MenuItem key={c.id} value={c.id}>{c.name} ({c.subdomain})</MenuItem>)}</TextField>
      <TextField select label="Sede principal" value={v.salesPointId} onChange={(e) => set({ salesPointId: e.target.value })} helperText="Se usara en cotizaciones, reportes y tramites por sede.">
        <MenuItem value="">Sin sede asignada</MenuItem>
        {currentSalesPoints.map((point) => <MenuItem key={point.id} value={point.id}>{point.name} - {point.city}</MenuItem>)}
      </TextField>
      <TextField select label="Rol" value={v.roles[0] ?? 'Vendedor'} onChange={(e) => set({ roles: [e.target.value] })}>{['Administrador', 'Supervisor', 'Vendedor'].map((role) => <MenuItem key={role} value={role}>{role}</MenuItem>)}</TextField>
    </>;
    }}
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
      <FormControlLabel control={<Checkbox checked={v.useMontelibanoTable} onChange={(e) => set({ useMontelibanoTable: e.target.checked })} />} label="Usar tabla financiera en cotizaciones" />
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
    line: form.item.line ?? '',
    version: form.item.version ?? '',
    reference: form.item.reference,
    description: form.item.description ?? '',
    engineCc: form.item.engineCc?.toString() ?? '',
    year: form.item.year?.toString() ?? '',
    color: form.item.color ?? '',
    price: form.item.price,
    soat: form.item.soat ?? 0,
    registrationFee: form.item.registrationFee ?? 0,
    taxes: form.item.taxes ?? 0,
    technicalSheet: form.item.technicalSheet ?? '',
    priceValidFrom: form.item.priceValidFrom?.slice(0, 10) ?? today,
    active: form.item.active
  } : emptyProduct;
  return <FormDialog title={form.item ? 'Editar producto' : 'Nuevo producto'} open={form.open} initial={initial} onClose={onClose} onSave={onSave}>
    {(v, set) => <>
      <SectionTitle title="Datos comerciales" />
      <FieldGrid columns={2}>
        <TextField fullWidth required label="Nombre del producto" value={v.name} onChange={(e) => set({ name: e.target.value })} />
        <TextField fullWidth required select label="Categoria" value={v.category} onChange={(e) => set({ category: e.target.value })}>
          {['Moto', 'Accesorio', 'Seguro', 'Tramite', 'Repuesto', 'Servicio', 'Garantia', 'Otro'].map((category) => <MenuItem key={category} value={category}>{category}</MenuItem>)}
        </TextField>
      </FieldGrid>
      <FieldGrid columns={3}>
        <TextField fullWidth label="Marca" value={v.brand} onChange={(e) => set({ brand: e.target.value })} />
        <TextField fullWidth label="Modelo" value={v.model} onChange={(e) => set({ model: e.target.value })} />
        <TextField fullWidth label="Linea" value={v.line} onChange={(e) => set({ line: e.target.value })} />
      </FieldGrid>
      <FieldGrid columns={3}>
        <TextField fullWidth label="Version" value={v.version} onChange={(e) => set({ version: e.target.value })} />
        <TextField fullWidth required label="Referencia" value={v.reference} onChange={(e) => set({ reference: e.target.value })} />
        <TextField fullWidth label="Color" value={v.color} onChange={(e) => set({ color: e.target.value })} />
      </FieldGrid>
      <TextField label="Descripcion comercial" value={v.description} onChange={(e) => set({ description: e.target.value })} multiline minRows={2} />
      <SectionTitle title="Ficha tecnica" />
      <FieldGrid columns={3}>
        <TextField fullWidth label="Cilindraje" type="number" value={v.engineCc} onChange={(e) => set({ engineCc: e.target.value })} />
        <TextField fullWidth label="Ano" type="number" value={v.year} onChange={(e) => set({ year: e.target.value })} />
        <TextField fullWidth label="Vigente desde" type="date" value={v.priceValidFrom} onChange={(e) => set({ priceValidFrom: e.target.value })} InputLabelProps={{ shrink: true }} />
      </FieldGrid>
      <TextField label="Ficha tecnica estructurada" value={v.technicalSheet} onChange={(e) => set({ technicalSheet: e.target.value })} multiline minRows={3} placeholder="Ej: Motor: 125 cc&#10;Transmision: 5 velocidades&#10;Freno delantero: Disco" />
      <SectionTitle title="Precio y cargos" />
      <FieldGrid columns={4}>
        <TextField fullWidth required label="Precio base" type="number" value={v.price} onChange={(e) => set({ price: Number(e.target.value) })} />
        <TextField fullWidth label="SOAT" type="number" value={v.soat} onChange={(e) => set({ soat: Number(e.target.value) })} />
        <TextField fullWidth label="Matricula" type="number" value={v.registrationFee} onChange={(e) => set({ registrationFee: Number(e.target.value) })} />
        <TextField fullWidth label="Impuestos" type="number" value={v.taxes} onChange={(e) => set({ taxes: Number(e.target.value) })} />
      </FieldGrid>
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

function CommercialInventoryDialog({ form, products, salesPoints, onClose, onSave }: DialogProps<CommercialInventory, typeof emptyCommercialInventory> & { products: Product[]; salesPoints: SalesPoint[] }) {
  const initial = form.item ? {
    productId: form.item.productId,
    salesPointId: form.item.salesPointId,
    vin: form.item.vin ?? '',
    chassisNumber: form.item.chassisNumber ?? '',
    engineNumber: form.item.engineNumber ?? '',
    plate: form.item.plate ?? '',
    color: form.item.color ?? '',
    isUsed: form.item.isUsed,
    mileage: form.item.mileage?.toString() ?? '',
    status: form.item.status === 2 ? 1 : form.item.status,
    notes: form.item.notes ?? ''
  } : { ...emptyCommercialInventory, productId: products[0]?.id ?? '', salesPointId: salesPoints[0]?.id ?? '' };

  return <FormDialog open={form.open} title={form.item ? 'Editar unidad de inventario' : 'Nueva unidad de inventario'} initial={initial} onClose={onClose} onSave={onSave} maxWidth="md">
    {(v, set) => <Stack spacing={2}>
      <FieldGrid>
        <TextField select required label="Producto" value={v.productId} onChange={(e) => set({ productId: e.target.value })}>
          {products.map((x) => <MenuItem key={x.id} value={x.id}>{productName(x)}</MenuItem>)}
        </TextField>
        <TextField select required label="Sede" value={v.salesPointId} onChange={(e) => set({ salesPointId: e.target.value })}>
          {salesPoints.map((x) => <MenuItem key={x.id} value={x.id}>{x.name} - {x.city}</MenuItem>)}
        </TextField>
        <TextField label="Color" value={v.color} onChange={(e) => set({ color: e.target.value })} />
        <TextField select label="Estado" value={v.status} onChange={(e) => set({ status: Number(e.target.value) })}>
          {[1, 3, 4, 5].map((x) => <MenuItem key={x} value={x}>{inventoryStatus(x)}</MenuItem>)}
        </TextField>
      </FieldGrid>
      <FieldGrid>
        <TextField label="VIN" value={v.vin} onChange={(e) => set({ vin: e.target.value })} />
        <TextField label="Numero chasis" value={v.chassisNumber} onChange={(e) => set({ chassisNumber: e.target.value })} />
        <TextField label="Numero motor" value={v.engineNumber} onChange={(e) => set({ engineNumber: e.target.value })} />
        <TextField label="Placa" value={v.plate} onChange={(e) => set({ plate: e.target.value.toUpperCase() })} />
      </FieldGrid>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
        <FormControlLabel control={<Checkbox checked={v.isUsed} onChange={(e) => set({ isUsed: e.target.checked, status: e.target.checked && v.status === 1 ? 4 : v.status })} />} label="Moto usada" />
        <TextField type="number" label="Kilometraje" value={v.mileage} onChange={(e) => set({ mileage: e.target.value })} sx={{ minWidth: 220 }} />
      </Stack>
      <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} multiline minRows={2} />
    </Stack>}
  </FormDialog>;
}

function InventoryReservationDialog({ form, customers, quotes, applications, onClose, onSave }: DialogProps<CommercialInventory, typeof emptyInventoryReservation> & { customers: Customer[]; quotes: Quote[]; applications: CreditApplication[] }) {
  const initial = emptyInventoryReservation;
  const quoteOptions = quotes.filter((x) => !form.item || x.productId === form.item.productId || (x.items ?? []).some((item) => item.productId === form.item?.productId));
  const applicationOptions = applications.filter((x) => !form.item || x.productId === form.item.productId);

  return <FormDialog open={form.open} title={`Separar ${form.item?.productName ?? 'unidad'}`} initial={initial} onClose={onClose} onSave={onSave} maxWidth="sm">
    {(v, set) => <Stack spacing={2}>
      <Alert severity="info">La unidad queda bloqueada contra disponibilidad hasta la fecha indicada.</Alert>
      <TextField select label="Cliente" value={v.customerId} onChange={(e) => set({ customerId: e.target.value })}>
        <MenuItem value="">Seleccionar por cotizacion o solicitud</MenuItem>
        {customers.map((x) => <MenuItem key={x.id} value={x.id}>{customerDisplayName(x)}</MenuItem>)}
      </TextField>
      <TextField select label="Cotizacion" value={v.quoteId} onChange={(e) => set({ quoteId: e.target.value })}>
        <MenuItem value="">Sin cotizacion</MenuItem>
        {quoteOptions.map((x) => <MenuItem key={x.id} value={x.id}>{x.number} - {x.customerFirstNames} {x.customerLastNames}</MenuItem>)}
      </TextField>
      <TextField select label="Solicitud de credito" value={v.creditApplicationId} onChange={(e) => set({ creditApplicationId: e.target.value })}>
        <MenuItem value="">Sin solicitud</MenuItem>
        {applicationOptions.map((x) => <MenuItem key={x.id} value={x.id}>{x.number} - {x.customerName}</MenuItem>)}
      </TextField>
      <TextField type="date" label="Vence separacion" value={v.reservationExpiresAt} onChange={(e) => set({ reservationExpiresAt: e.target.value })} InputLabelProps={{ shrink: true }} />
      <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} multiline minRows={2} />
    </Stack>}
  </FormDialog>;
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

function QuoteDialog({ form, products, requirementProfiles, onClose, onSave }: DialogProps<Quote, typeof emptyQuote> & { products: Product[]; requirementProfiles: RequirementProfile[] }) {
  const firstProduct = products[0];
  const initialItem = {
    ...emptyQuoteItem,
    productId: firstProduct?.id ?? '',
    insurance: firstProduct?.soat ?? 0,
    administrativeFees: (firstProduct?.registrationFee ?? 0) + (firstProduct?.taxes ?? 0)
  };
  const initial = { ...emptyQuote, requirementProfileId: requirementProfiles[0]?.id ?? '', productId: initialItem.productId, items: [initialItem] };
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

  return <FormDialog title="Nueva cotizacion" open={form.open} initial={initial} onClose={onClose} onSave={onSave} maxWidth="lg">
    {(v, set) => {
      const quoteItems = v.items?.length ? v.items : [{ ...emptyQuoteItem, productId: v.productId }];
      const updateItem = (index: number, patch: Partial<typeof emptyQuoteItem>) => {
        const items = quoteItems.map((item, itemIndex) => itemIndex === index ? { ...item, ...patch } : item);
        set({ items, productId: items[0]?.productId ?? '', downPayment: Number(items[0]?.downPayment ?? 0), insurance: Number(items[0]?.insurance ?? 0), administrativeFees: Number(items[0]?.administrativeFees ?? 0), termMonths: Number(items[0]?.termMonths ?? 24), monthlyInterestRate: Number(items[0]?.monthlyInterestRate ?? 2.2) });
      };
      const updateItemProduct = (index: number, productId: string) => {
        const selected = products.find((product) => product.id === productId);
        updateItem(index, {
          productId,
          insurance: selected?.soat ?? 0,
          administrativeFees: (selected?.registrationFee ?? 0) + (selected?.taxes ?? 0)
        });
      };
      const addItem = () => {
        if (quoteItems.length >= 4) return;
        const selected = products[0];
        const items = [...quoteItems, {
          ...emptyQuoteItem,
          productId: selected?.id ?? '',
          insurance: selected?.soat ?? 0,
          administrativeFees: (selected?.registrationFee ?? 0) + (selected?.taxes ?? 0)
        }];
        set({ items });
      };
      const removeItem = (index: number) => {
        const items = quoteItems.filter((_, itemIndex) => itemIndex !== index);
        const normalized = items.length ? items : [{ ...emptyQuoteItem, productId: products[0]?.id ?? '' }];
        set({ items: normalized, productId: normalized[0]?.productId ?? '' });
      };
      return <>
        <Paper variant="outlined" sx={{ p: 2, bgcolor: '#fbfdff' }}>
          <Stack spacing={2}>
            <Typography variant="subtitle1" fontWeight={900}>Datos del cliente</Typography>
            {identityNotice && <Alert severity={identityNotice.type === 'success' ? 'success' : identityNotice.type === 'info' ? 'info' : 'error'}>{identityNotice.text}</Alert>}
            <Box sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', md: '260px minmax(280px, 1fr) auto' },
              gap: 1.5,
              alignItems: 'center'
            }}>
              <TextField required select label="Tipo identificacion" value={v.identificationType} onChange={(e) => set({ identificationType: Number(e.target.value) })}>
                {identificationOptions.map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}
              </TextField>
              <TextField
                label="Numero identificacion"
                value={v.identificationNumber}
                onChange={(e) => set({ identificationNumber: e.target.value })}
              />
              <Stack direction="row" spacing={1} alignItems="center" justifyContent={{ xs: 'stretch', md: 'flex-end' }}>
                <Button sx={{ flex: { xs: 1, md: '0 0 auto' } }} variant="contained" startIcon={<AutoAwesome />} disabled={identityLoading || !identificationDigits(v.identificationNumber)} onClick={() => void lookupIdentity(v, set)}>
                  {identityLoading ? 'Consultando...' : 'Consultar'}
                </Button>
                <IdentificationLookupAdornment identification={v.identificationNumber} inline />
              </Stack>
            </Box>
            <FieldGrid columns={4}>
              <TextField fullWidth required label="Primer nombre" value={v.customerFirstName} onChange={(e) => set({ customerFirstName: e.target.value, customerFirstNames: fullFirstNames(e.target.value, v.customerMiddleName) })} />
              <TextField fullWidth label="Segundo nombre" value={v.customerMiddleName} onChange={(e) => set({ customerMiddleName: e.target.value, customerFirstNames: fullFirstNames(v.customerFirstName, e.target.value) })} />
              <TextField fullWidth required label="Primer apellido" value={v.customerLastName} onChange={(e) => set({ customerLastName: e.target.value, customerLastNames: fullLastNames(e.target.value, v.customerSecondLastName) })} />
              <TextField fullWidth label="Segundo apellido" value={v.customerSecondLastName} onChange={(e) => set({ customerSecondLastName: e.target.value, customerLastNames: fullLastNames(v.customerLastName, e.target.value) })} />
            </FieldGrid>
            <Box sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', sm: '140px minmax(260px, 1fr)' },
              gap: 1.5,
              maxWidth: { md: 620 }
            }}>
              <TextField fullWidth required label="Indicativo" value={v.phoneCountryCode} onChange={(e) => set({ phoneCountryCode: e.target.value })} />
              <TextField fullWidth required label="Telefono / WhatsApp" value={v.phoneNumber} onChange={(e) => set({ phoneNumber: e.target.value })} />
            </Box>
            <Box sx={{ maxWidth: { md: 520 } }}>
              <TextField fullWidth select label="Perfil de requisitos" value={v.requirementProfileId} onChange={(e) => set({ requirementProfileId: e.target.value })} helperText="Este perfil generara el checklist de documentos si la cotizacion pasa a solicitud de credito.">
                <MenuItem value="">Empleado por defecto</MenuItem>
                {requirementProfiles.map((profile) => <MenuItem key={profile.id} value={profile.id}>{profile.name}{profile.isCash ? ' - contado' : ''}</MenuItem>)}
              </TextField>
            </Box>
          </Stack>
        </Paper>
        <Stack spacing={1.5}>
          <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'stretch', sm: 'center' }} gap={1}>
            <Box>
              <Typography variant="subtitle1" fontWeight={900}>Articulos a cotizar</Typography>
              <Typography variant="body2" color="text.secondary">Agregue varios productos para imprimir la cotizacion como comparativo.</Typography>
            </Box>
            <Button variant="outlined" startIcon={<Add />} disabled={quoteItems.length >= 4 || !products.length} onClick={addItem}>Agregar articulo</Button>
          </Stack>
          {quoteItems.map((item, index) => {
            const selectedProduct = products.find((product) => product.id === item.productId);
            return <Paper key={index} variant="outlined" sx={{ p: 2, bgcolor: '#f8fafc' }}>
              <Stack spacing={1.5}>
                <Stack direction="row" justifyContent="space-between" alignItems="center" gap={1}>
                  <Typography fontWeight={900}>Articulo {index + 1}</Typography>
                  {quoteItems.length > 1 && <Button color="error" size="small" startIcon={<Delete />} onClick={() => removeItem(index)}>Quitar</Button>}
                </Stack>
                <Box sx={{
                  display: 'grid',
                  gridTemplateColumns: {
                    xs: '1fr',
                    md: 'minmax(300px, 1.7fr) repeat(4, minmax(112px, 1fr)) minmax(170px, 1fr)'
                  },
                  gap: 1.5,
                  alignItems: 'stretch'
                }}>
                  <TextField required select label="Producto" value={item.productId} onChange={(e) => updateItemProduct(index, e.target.value)}>
                    {products.length ? products.map((product) => <MenuItem key={product.id} value={product.id}>{productName(product)} ({product.category}) - {money(product.price)}</MenuItem>) : <MenuItem value="">No hay productos activos</MenuItem>}
                  </TextField>
                  <TextField fullWidth label="Cuota inicial" type="number" value={item.downPayment} onChange={(e) => updateItem(index, { downPayment: Number(e.target.value) })} />
                  <TextField fullWidth label="Cuotas" type="number" value={item.termMonths} onChange={(e) => updateItem(index, { termMonths: Number(e.target.value) })} />
                  <TextField fullWidth label="Seguro" type="number" value={item.insurance} onChange={(e) => updateItem(index, { insurance: Number(e.target.value) })} />
                  <TextField fullWidth label="Gastos adm." type="number" value={item.administrativeFees} onChange={(e) => updateItem(index, { administrativeFees: Number(e.target.value) })} />
                  <QuoteSimulationPreview value={item} selectedProduct={selectedProduct} compact />
                </Box>
              </Stack>
            </Paper>;
          })}
        </Stack>
        <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} multiline minRows={2} />
      </>;
    }}
  </FormDialog>;
}

function QuoteSimulationPreview({ value, selectedProduct, compact = false }: { value: typeof emptyQuoteItem; selectedProduct?: Product; compact?: boolean }) {
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
    promotionId: undefined,
    promotionName: undefined,
    promotionDiscount: 0,
    discountedProductPrice: productPrice,
    financedAmount: fallbackFinanced,
    estimatedMonthlyPayment: fallbackPayment,
    estimatedTotalPayment: fallbackDownPayment + fallbackPayment * fallbackTermMonths,
    creditType: 'Vista previa',
    usedCompanyFinancialSettings: false
  };

  return <Paper variant="outlined" sx={{ p: compact ? 1.25 : 2, bgcolor: compact ? '#ffffff' : '#f8fafc', height: '100%' }}>
    <Stack spacing={compact ? 0.75 : 1.5}>
      {loading && <LinearProgress />}
      {error && <Alert severity="warning">{error}</Alert>}
      {compact
        ? <Stack spacing={0.5}>
          {preview.promotionDiscount > 0 && <Typography variant="caption" color="success.main" fontWeight={800}>{preview.promotionName ?? 'Promocion'}: -{money(preview.promotionDiscount)}</Typography>}
          <Box><Typography variant="caption" color="text.secondary">Financiado</Typography><Typography fontWeight={800}>{money(preview.financedAmount)}</Typography></Box>
          <Box><Typography variant="caption" color="text.secondary">Cuota aprox.</Typography><Typography fontWeight={800}>{money(preview.estimatedMonthlyPayment)}</Typography></Box>
        </Stack>
        : <FieldGrid columns={3}>
          <Box><Typography variant="caption" color="text.secondary">Valor producto</Typography><Typography fontWeight={700}>{money(preview.discountedProductPrice)}</Typography>{preview.promotionDiscount > 0 && <Typography variant="caption" color="success.main">Descuento {money(preview.promotionDiscount)}</Typography>}</Box>
          <Box><Typography variant="caption" color="text.secondary">Total financiado</Typography><Typography fontWeight={700}>{money(preview.financedAmount)}</Typography></Box>
          <Box><Typography variant="caption" color="text.secondary">Cuota aproximada</Typography><Typography fontWeight={700}>{money(preview.estimatedMonthlyPayment)}</Typography></Box>
        </FieldGrid>}
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

function CreditApplicationDialog({ form, customers, products, quotes, deals, requirementProfiles, onClose, onSave }: DialogProps<CreditApplication, typeof emptyCreditApplication> & { customers: Customer[]; products: Product[]; quotes: Quote[]; deals: Deal[]; requirementProfiles: RequirementProfile[] }) {
  const quote = quotes.find((x) => x.id === (form.item?.quoteId ?? ''));
  const initial = form.item ? {
    customerId: form.item.customerId,
    productId: form.item.productId,
    quoteId: form.item.quoteId ?? '',
    dealId: form.item.dealId ?? '',
    requirementProfileId: form.item.requirementProfileId ?? '',
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
  } : { ...emptyCreditApplication, customerId: customers[0]?.id ?? '', productId: products[0]?.id ?? '', requirementProfileId: requirementProfiles[0]?.id ?? '', motorcycleValue: products[0]?.price ?? 0 };
  return <FormDialog title={form.item ? 'Editar solicitud de credito' : 'Nueva solicitud de credito'} open={form.open} initial={initial} onClose={onClose} onSave={onSave} maxWidth="lg">
    {(v, set) => {
      const selectedQuote = quotes.find((x) => x.id === v.quoteId);
      const selectedProduct = products.find((x) => x.id === v.productId);
      const selectedCustomer = customers.find((x) => x.id === v.customerId);
      return <>
        <Paper variant="outlined" sx={{ p: 2, bgcolor: '#fbfdff' }}>
          <Stack spacing={2}>
            <Typography variant="subtitle1" fontWeight={900}>Origen y cliente</Typography>
            <FieldGrid columns={3}>
              <TextField select label="Cotizacion" value={v.quoteId} onChange={(e) => {
                const selected = quotes.find((x) => x.id === e.target.value);
                set({
                  quoteId: e.target.value,
                  customerId: selected?.customerId ?? v.customerId,
                  productId: selected?.productId ?? v.productId,
                  requirementProfileId: selected?.requirementProfileId ?? v.requirementProfileId,
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
              <TextField select label="Negocio pipeline" value={v.dealId} onChange={(e) => set({ dealId: e.target.value })}><MenuItem value="">Sin negocio</MenuItem>{deals.map((x) => <MenuItem key={x.id} value={x.id}>{x.title}</MenuItem>)}</TextField>
            </FieldGrid>
            <Box sx={{ maxWidth: { md: 520 } }}>
              <TextField fullWidth select label="Perfil de requisitos" value={v.requirementProfileId} onChange={(e) => set({ requirementProfileId: e.target.value })} helperText="Define el checklist inicial de documentos de esta solicitud.">
                <MenuItem value="">Empleado por defecto</MenuItem>
                {requirementProfiles.map((profile) => <MenuItem key={profile.id} value={profile.id}>{profile.name}{profile.isCash ? ' - contado' : ''}</MenuItem>)}
              </TextField>
            </Box>
            <Box sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', md: '260px minmax(260px, 1fr) auto' },
              gap: 1.5,
              alignItems: 'center'
            }}>
              <TextField fullWidth required select label="Tipo identificacion" value={v.identificationType} onChange={(e) => set({ identificationType: Number(e.target.value) })}>{identificationOptions.map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}</TextField>
              <TextField fullWidth required label="Numero identificacion" value={v.identificationNumber} onChange={(e) => set({ identificationNumber: e.target.value })} />
              <IdentificationLookupAdornment identification={v.identificationNumber} inline />
            </Box>
            <FieldGrid columns={4}>
              <TextField fullWidth label="Fecha nacimiento" type="date" value={v.birthDate} onChange={(e) => set({ birthDate: e.target.value })} InputLabelProps={{ shrink: true }} />
              <TextField fullWidth required label="Celular / WhatsApp" value={v.mobile} onChange={(e) => set({ mobile: e.target.value })} />
              <TextField fullWidth label="Direccion" value={v.address} onChange={(e) => set({ address: e.target.value })} />
              <TextField fullWidth label="Ciudad" value={v.city} onChange={(e) => set({ city: e.target.value })} />
            </FieldGrid>
            {selectedCustomer && <Alert severity="info">Cliente seleccionado: {selectedCustomer.firstNames || selectedCustomer.name} {selectedCustomer.lastNames}</Alert>}
          </Stack>
        </Paper>

        <Paper variant="outlined" sx={{ p: 2, bgcolor: '#f8fafc' }}>
          <Stack spacing={2}>
            <Typography variant="subtitle1" fontWeight={900}>Producto y credito</Typography>
            <Box sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', md: 'minmax(280px, 1.6fr) repeat(4, minmax(130px, 1fr))' },
              gap: 1.5
            }}>
              <TextField required select label="Producto principal" value={v.productId} onChange={(e) => {
                const product = products.find((x) => x.id === e.target.value);
                set({ productId: e.target.value, motorcycleValue: product?.price ?? v.motorcycleValue });
              }}>{products.map((x) => <MenuItem key={x.id} value={x.id}>{productName(x)} ({x.category}) - {money(x.price)}</MenuItem>)}</TextField>
              <TextField fullWidth label="Ingresos" type="number" value={v.monthlyIncome} onChange={(e) => set({ monthlyIncome: Number(e.target.value) })} />
              <TextField fullWidth label="Cuota inicial" type="number" value={v.downPayment} onChange={(e) => set({ downPayment: Number(e.target.value) })} />
              <TextField fullWidth label="Plazo meses" type="number" value={v.termMonths} onChange={(e) => set({ termMonths: Number(e.target.value) })} />
              <TextField fullWidth label="Valor producto" type="number" value={v.motorcycleValue || selectedQuote?.productPrice || selectedProduct?.price || 0} onChange={(e) => set({ motorcycleValue: Number(e.target.value) })} />
            </Box>
            <TextField label="Ocupacion" value={v.occupation} onChange={(e) => set({ occupation: e.target.value })} />
          </Stack>
        </Paper>

        <Paper variant="outlined" sx={{ p: 2, bgcolor: '#fbfdff' }}>
          <Stack spacing={2}>
            <Typography variant="subtitle1" fontWeight={900}>Codeudor y referencias</Typography>
            <FieldGrid columns={4}>
              <TextField fullWidth label="Nombre codeudor" value={v.coDebtorName} onChange={(e) => set({ coDebtorName: e.target.value })} />
              <TextField fullWidth label="Identificacion codeudor" value={v.coDebtorIdentification} onChange={(e) => set({ coDebtorIdentification: e.target.value })} />
              <TextField fullWidth label="Celular codeudor" value={v.coDebtorMobile} onChange={(e) => set({ coDebtorMobile: e.target.value })} />
              <TextField fullWidth label="Parentesco / relacion" value={v.coDebtorRelationship} onChange={(e) => set({ coDebtorRelationship: e.target.value })} />
            </FieldGrid>
            <Box sx={{ maxWidth: { md: 260 } }}>
              <TextField fullWidth label="Ingresos codeudor" type="number" value={v.coDebtorMonthlyIncome} onChange={(e) => set({ coDebtorMonthlyIncome: Number(e.target.value) })} />
            </Box>
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
          </Stack>
        </Paper>

        <Paper variant="outlined" sx={{ p: 2, bgcolor: '#f8fafc' }}>
          <Stack spacing={2}>
            <Typography variant="subtitle1" fontWeight={900}>Gestion</Typography>
            <FieldGrid>
              <TextField select label="Estado" value={v.status} onChange={(e) => set({ status: Number(e.target.value) })}>{creditStatusOptions.map((x) => <MenuItem key={x} value={x}>{creditStatus(x)}</MenuItem>)}</TextField>
              <TextField label="Observaciones" value={v.notes} onChange={(e) => set({ notes: e.target.value })} multiline minRows={2} />
            </FieldGrid>
          </Stack>
        </Paper>
      </>;
    }}
  </FormDialog>;
}

function DocumentSummary({ application, onUpdate, onUpload, onDownload }: {
  application: CreditApplication;
  onUpdate: (application: CreditApplication, document: CreditDocument, status: number, patch?: Partial<Pick<CreditDocument, 'expiresAt' | 'notes' | 'rejectionReason'>>) => Promise<void>;
  onUpload: (application: CreditApplication, document: CreditDocument, file: File) => Promise<void>;
  onDownload: (application: CreditApplication, document: CreditDocument) => Promise<void>;
}) {
  const canValidate = useCanManage();
  const statusOptions = canValidate ? [1, 2, 3, 4] : [1, 2];

  const handleStatus = (document: CreditDocument, status: number) => {
    if (status === 4) {
      const reason = window.prompt('Motivo de rechazo del documento');
      if (!reason?.trim()) return;
      onUpdate(application, document, status, { rejectionReason: reason.trim() });
      return;
    }

    onUpdate(application, document, status);
  };

  return <Stack spacing={1} sx={{ minWidth: 0, width: '100%' }}>
    {application.documents.map((document) => {
      const documentStatusOptions = statusOptions.includes(document.status) ? statusOptions : [...statusOptions, document.status];
      return <Stack key={document.id} spacing={.75} sx={{ p: 1, border: '1px solid #e2e8f0', borderRadius: 1, bgcolor: '#fff' }}>
      <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1}>
        <Stack spacing={.4} sx={{ minWidth: 0 }}>
          <Stack direction="row" gap={.5} flexWrap="wrap">
            <Chip size="small" label={`${document.name}: ${documentStatus(document.status)}`} color={document.status === 3 ? 'success' : document.status === 4 ? 'error' : undefined} variant={document.status === 1 ? 'outlined' : 'filled'} />
            {document.isExpired && <Chip size="small" color="error" variant="outlined" label="Vencido" />}
            {!document.isExpired && document.daysToExpire !== undefined && document.daysToExpire !== null && document.daysToExpire <= 7 && <Chip size="small" color="warning" variant="outlined" label={`Vence en ${document.daysToExpire} dia(s)`} />}
          </Stack>
          {document.hasFile && <Typography variant="caption" color="text.secondary" noWrap>{document.fileName}</Typography>}
          {document.rejectionReason && <Typography variant="caption" color="error">Motivo: {document.rejectionReason}</Typography>}
          {document.validatedBy && <Typography variant="caption" color="success.main">Validado por {document.validatedBy}</Typography>}
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
          <TextField select size="small" value={document.status} onChange={(e) => handleStatus(document, Number(e.target.value))} sx={{ width: 126 }} helperText={!canValidate ? 'Sin validar' : undefined}>
            {documentStatusOptions.map((status) => <MenuItem key={status} value={status}>{documentStatus(status)}</MenuItem>)}
          </TextField>
        </Stack>
      </Stack>
      <TextField
        size="small"
        type="date"
        label="Vigencia"
        value={document.expiresAt?.slice(0, 10) ?? ''}
        onChange={(e) => onUpdate(application, document, document.status, { expiresAt: e.target.value ? new Date(`${e.target.value}T00:00:00`).toISOString() : undefined })}
        InputLabelProps={{ shrink: true }}
      />
    </Stack>;
    })}
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

function RescheduleActivityDialog({ activity, onClose, onSave }: { activity?: Activity; onClose: () => void; onSave: (scheduledAt: string) => Promise<void> }) {
  const muiTheme = useTheme();
  const fullScreen = useMediaQuery(muiTheme.breakpoints.down('sm'));
  const [dateValue, setDateValue] = useState('');
  const [timeValue, setTimeValue] = useState('09:00');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const scheduledAt = dateValue && timeValue ? `${dateValue}T${timeValue}` : '';
  const timeOptions = useMemo(() => commonActivityTimes(timeValue), [timeValue]);

  useEffect(() => {
    if (activity) {
      const parts = splitInputDateTime(toInputDateTime(activity.scheduledAt));
      setDateValue(parts.date);
      setTimeValue(parts.time);
      setError('');
    }
  }, [activity]);

  const setQuickDate = (days: number) => setDateValue(addDaysInputDate(days));

  const save = async () => {
    if (!scheduledAt) {
      setError('Seleccione la nueva fecha y hora.');
      return;
    }

    setSaving(true);
    setError('');
    try {
      await onSave(scheduledAt);
    } catch (err) {
      setError(apiError(err));
    } finally {
      setSaving(false);
    }
  };

  return <Dialog open={!!activity} onClose={saving ? undefined : onClose} fullWidth maxWidth="xs" fullScreen={fullScreen}>
    <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 1 }}>
      Reprogramar actividad
      <IconButton onClick={onClose} disabled={saving}><Close /></IconButton>
    </DialogTitle>
    <DialogContent sx={{ px: { xs: 2, sm: 3 } }}>
      <Stack spacing={2} sx={{ pt: 1 }}>
        {error && <Alert severity="error">{error}</Alert>}
        <Box>
          <Typography fontWeight={800}>{activity?.title}</Typography>
          <Typography color="text.secondary" fontSize={13}>{activity?.customerName ?? 'Sin cliente asociado'}</Typography>
        </Box>
        <Stack direction="row" gap={1} flexWrap="wrap">
          <Button size="small" variant={dateValue === addDaysInputDate(0) ? 'contained' : 'outlined'} onClick={() => setQuickDate(0)}>Hoy</Button>
          <Button size="small" variant={dateValue === addDaysInputDate(1) ? 'contained' : 'outlined'} onClick={() => setQuickDate(1)}>Manana</Button>
          <Button size="small" variant={dateValue === addDaysInputDate(2) ? 'contained' : 'outlined'} onClick={() => setQuickDate(2)}>En 2 dias</Button>
          <Button size="small" variant={dateValue === addDaysInputDate(7) ? 'contained' : 'outlined'} onClick={() => setQuickDate(7)}>Proxima semana</Button>
        </Stack>
        <FieldGrid columns={2}>
          <TextField
            label="Fecha"
            type="date"
            value={dateValue}
            onChange={(event) => setDateValue(event.target.value)}
            InputLabelProps={{ shrink: true }}
            fullWidth
            required
          />
          <TextField select label="Hora" value={timeValue} onChange={(event) => setTimeValue(event.target.value)} fullWidth required>
            {timeOptions.map((time) => <MenuItem key={time} value={time}>{formatTimeLabel(time)}</MenuItem>)}
          </TextField>
        </FieldGrid>
        {scheduledAt && <Paper variant="outlined" sx={{ p: 1.5, bgcolor: '#f8fafc' }}>
          <Typography variant="caption" color="text.secondary">Nueva programacion</Typography>
          <Typography fontWeight={800}>{formatLocalDateTime(scheduledAt)}</Typography>
        </Paper>}
        {activity?.reminderAt && <Alert severity="info">El recordatorio se conservara con la misma anticipacion.</Alert>}
      </Stack>
    </DialogContent>
    <DialogActions sx={{ px: { xs: 2, sm: 3 }, pb: 2, flexWrap: 'wrap' }}>
      <Button onClick={onClose} disabled={saving}>Cancelar</Button>
      <Button variant="contained" onClick={save} disabled={saving}>{saving ? 'Guardando...' : 'Reprogramar'}</Button>
    </DialogActions>
  </Dialog>;
}

type DialogProps<TItem, TPayload> = { form: FormMode<TItem>; onClose: () => void; onSave: (payload: TPayload) => Promise<void> };

function FieldGrid({ children, columns = 2 }: { children: ReactNode; columns?: 2 | 3 | 4 }) {
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

function FormDialog<T extends Record<string, unknown>>({ title, open, initial, children, onClose, onSave, maxWidth = 'sm' }: { title: string; open: boolean; initial: T; children: (value: T, set: (patch: Partial<T>) => void) => ReactNode; onClose: () => void; onSave: (payload: T) => Promise<void>; maxWidth?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' }) {
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

  return <Dialog open={open} onClose={saving ? undefined : onClose} fullWidth maxWidth={maxWidth} fullScreen={fullScreen}>
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
  return <Stack direction={{ xs: 'column', sm: 'row' }} alignItems={{ xs: 'stretch', sm: 'center' }} justifyContent="space-between" gap={1.5} sx={{ mb: .5 }}>
    <Box>
      <Typography variant="h4" fontWeight={900} sx={{ fontSize: { xs: 26, sm: 34 }, lineHeight: 1.15, color: '#101828' }}>{title}</Typography>
      <Typography color="text.secondary" fontSize={14}>Panel de trabajo comercial</Typography>
    </Box>
    <Stack direction={{ xs: 'column', sm: 'row' }} gap={1} sx={{ width: { xs: '100%', sm: 'auto' } }}>
      {onRefresh && <Button fullWidth={false} variant="outlined" onClick={onRefresh}>Actualizar</Button>}
      {secondaryAction && <Button variant="outlined" onClick={secondaryAction.onClick}>{secondaryAction.label}</Button>}
      {action && <Button variant="contained" startIcon={<Add />} onClick={onAction}>{action}</Button>}
    </Stack>
  </Stack>;
}

function EntityTable({ headers, rows, empty }: { headers: string[]; rows: ReactNode[][]; empty: string }) {
  return <Card sx={{ width: '100%', overflow: 'hidden' }}>
    <TableContainer sx={{ width: '100%', overflowX: 'auto' }}>
      <Table size="small" sx={{ minWidth: tableMinWidth(headers), tableLayout: 'fixed' }}>
        <TableHead><TableRow>{headers.map((h) => <TableCell key={h} sx={{ ...tableColumnSx(h), whiteSpace: 'nowrap', fontWeight: 900, py: 1.35 }}>{h}</TableCell>)}</TableRow></TableHead>
        <TableBody>{rows.length ? rows.map((row, i) => <TableRow key={i} sx={{ '&:hover': { bgcolor: '#f9fafb' } }}>{row.map((c, j) => <TableCell key={j} sx={{ ...tableColumnSx(headers[j]), verticalAlign: 'top', py: 1.5 }}>{c ?? '-'}</TableCell>)}</TableRow>) : <TableRow><TableCell colSpan={headers.length}><EmptyState text={empty} /></TableCell></TableRow>}</TableBody>
      </Table>
    </TableContainer>
  </Card>;
}

function ReportTable({ headers, rows, empty }: { headers: string[]; rows: ReactNode[][]; empty: string }) {
  return <TableContainer sx={{ width: '100%', overflowX: 'auto', border: `1px solid ${uiBorder}`, borderRadius: 3 }}>
    <Table size="small" sx={{ minWidth: 520 }}>
      <TableHead><TableRow>{headers.map((h) => <TableCell key={h} sx={{ whiteSpace: 'nowrap', fontWeight: 900 }}>{h}</TableCell>)}</TableRow></TableHead>
      <TableBody>{rows.length ? rows.map((row, i) => <TableRow key={i} sx={{ '&:hover': { bgcolor: '#f9fafb' } }}>{row.map((c, j) => <TableCell key={j} sx={{ verticalAlign: 'top' }}>{c ?? '-'}</TableCell>)}</TableRow>) : <TableRow><TableCell colSpan={headers.length}><EmptyState text={empty} /></TableCell></TableRow>}</TableBody>
    </Table>
  </TableContainer>;
}

function tableMinWidth(headers: string[]) {
  if (headers.includes('Pendientes')) return 1120;
  if (headers.includes('Gestion')) return 1180;
  return headers.includes('Plantillas') ? 1760 : 760;
}

function tableColumnSx(header: string) {
  const widths: Record<string, number> = {
    Solicitud: 170,
    Identificacion: 150,
    Numero: 150,
    Cliente: 220,
    Credito: 260,
    Producto: 220,
    Ciudad: 150,
    Estado: 150,
    Pendientes: 250,
    Gestion: 560,
    Ingresos: 130,
    Codeudor: 170,
    Referencias: 210,
    Documentos: 410,
    Aprobacion: 220,
    Plantillas: 170,
    Acciones: 220
  };
  return {
    width: widths[header] ?? 180,
    minWidth: widths[header] ?? 180,
    maxWidth: widths[header] ?? 260,
    overflow: 'hidden',
    textOverflow: 'ellipsis'
  };
}

function AiAnalysisDialog({ analysis, phone, onClose }: { analysis?: CustomerAiAnalysis; phone?: string; onClose: () => void }) {
  const [copied, setCopied] = useState(false);
  const sendMessage = async () => {
    if (!analysis?.whatsappMessage) return;
    const message = analysis.whatsappMessage;
    if (phone) {
      window.open(whatsappUrl(phone, message), '_blank', 'noopener,noreferrer');
    }
    await navigator.clipboard?.writeText(message).catch(() => undefined);
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
      <Button variant="contained" startIcon={<WhatsApp />} onClick={sendMessage}>
        {phone ? 'Enviar por WhatsApp' : copied ? 'Copiado' : 'Copiar mensaje'}
      </Button>
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
    {onReschedule && <Tooltip title="Reprogramar"><IconButton size="small" onClick={onReschedule}><EventNote fontSize="small" /></IconButton></Tooltip>}
    {onCancel && <Tooltip title="Cancelar"><IconButton size="small" color="warning" onClick={onCancel}><Close fontSize="small" /></IconButton></Tooltip>}
    {onEdit && <Tooltip title="Editar"><IconButton size="small" onClick={onEdit}><Edit fontSize="small" /></IconButton></Tooltip>}
    {onConvert && <Tooltip title="Convertir a cliente"><IconButton size="small" onClick={onConvert}><SyncAlt fontSize="small" /></IconButton></Tooltip>}
    {onDownload && <Tooltip title="Descargar PDF"><IconButton size="small" onClick={onDownload}><Download fontSize="small" /></IconButton></Tooltip>}
    {onDelete && <Tooltip title="Eliminar"><IconButton size="small" color="error" onClick={onDelete}><Delete fontSize="small" /></IconButton></Tooltip>}
  </Stack>;
}

function Metric({ label, value }: { label: string; value: ReactNode }) {
  return <Card sx={{ height: '100%', position: 'relative', overflow: 'hidden' }}>
    <CardContent>
      <Box sx={{ position: 'absolute', top: 16, right: 16, width: 10, height: 10, borderRadius: '50%', bgcolor: uiPrimary, opacity: .24 }} />
      <Typography color="text.secondary" fontSize={13} fontWeight={700}>{label}</Typography>
      <Typography variant="h5" fontWeight={900} sx={{ overflowWrap: 'anywhere', mt: .5 }}>{value}</Typography>
    </CardContent>
  </Card>;
}

function Row({ primary, secondary }: { primary: string; secondary: string }) {
  return <Stack direction={{ xs: 'column', sm: 'row' }} gap={.5} justifyContent="space-between" sx={{ py: 1, borderBottom: `1px solid ${uiBorder}` }}><Typography sx={{ overflowWrap: 'anywhere' }}>{primary}</Typography><Typography color="text.secondary" sx={{ flexShrink: 0 }}>{secondary}</Typography></Stack>;
}

function InfoLine({ label, value }: { label: string; value: ReactNode }) {
  return <Stack direction={{ xs: 'column', sm: 'row' }} gap={1} justifyContent="space-between" sx={{ py: .75, borderBottom: `1px solid ${uiBorder}` }}>
    <Typography variant="body2" color="text.secondary">{label}</Typography>
    <Typography fontWeight={800} textAlign={{ xs: 'left', sm: 'right' }} sx={{ overflowWrap: 'anywhere' }}>{value}</Typography>
  </Stack>;
}

function EmptyState({ text }: { text: string }) {
  return <Typography color="text.secondary" sx={{ py: 3, textAlign: 'center', fontWeight: 700 }}>{text}</Typography>;
}

function StatusBar({ loading, error }: { loading?: boolean; error?: string }) {
  return <>{loading && <LinearProgress sx={{ borderRadius: 999 }} />}{error && <Alert severity="error">{error}</Alert>}</>;
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

function splitInputDateTime(value: string) {
  const [date, time = '09:00'] = value.split('T');
  return { date, time: time.slice(0, 5) };
}

function addDaysInputDate(days: number) {
  const date = new Date();
  date.setDate(date.getDate() + days);
  const offset = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 10);
}

function commonActivityTimes(selected?: string) {
  const times = ['08:00', '09:00', '10:00', '11:00', '14:00', '15:00', '16:00', '17:00'];
  return selected && !times.includes(selected) ? [...times, selected].sort() : times;
}

function formatTimeLabel(value: string) {
  return new Date(`2000-01-01T${value}:00`).toLocaleTimeString('es-CO', { hour: 'numeric', minute: '2-digit' });
}

function formatLocalDateTime(value: string) {
  return new Date(value).toLocaleString('es-CO', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit'
  });
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
function customerDisplayName(customer: Customer) {
  return [customer.firstNames, customer.lastNames].filter(Boolean).join(' ').trim() || customer.name || customer.email || 'Cliente';
}
function inventoryStatus(value: number) { return ['-', 'Disponible', 'Separada', 'Vendida', 'Usada', 'No disponible'][value] ?? 'Disponible'; }
function inventoryTone(value: number, expired?: boolean): 'success' | 'warning' | 'error' | 'default' {
  if (expired) return 'error';
  if (value === 1) return 'success';
  if (value === 2) return 'warning';
  if (value === 5) return 'error';
  return 'default';
}
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
async function normalizeDeliveryPhoto(file: File) {
  if (!['image/png', 'image/jpeg', 'image/webp'].includes(file.type)) {
    throw new Error('La foto debe estar en formato PNG, JPG o WebP.');
  }
  if (file.size > deliveryPhotoMaxBytes) {
    throw new Error('La foto no puede superar 1 MB.');
  }

  const image = await loadImage(await readFileAsDataUrl(file));
  const canvas = document.createElement('canvas');
  const maxSide = 1280;
  const scale = Math.min(1, maxSide / Math.max(image.width, image.height));
  canvas.width = Math.max(1, Math.round(image.width * scale));
  canvas.height = Math.max(1, Math.round(image.height * scale));
  const context = canvas.getContext('2d');
  if (!context) throw new Error('No se pudo preparar la foto.');
  context.drawImage(image, 0, 0, canvas.width, canvas.height);
  return canvas.toDataURL('image/jpeg', 0.82);
}
function IdentificationLookupAdornment({ identification, inline = false }: { identification?: string; inline?: boolean }) {
  const digits = identificationDigits(identification);
  const buttons = <Stack direction="row" spacing={.5}>
    <Button size="small" variant="outlined" disabled={!digits} onClick={() => void openExternalLookup(simitUrl, digits)}>Simit</Button>
    <Button size="small" variant="outlined" disabled={!digits} onClick={() => void openExternalLookup(runtUrl, digits)}>Runt</Button>
  </Stack>;
  return inline ? buttons : <InputAdornment position="end">{buttons}</InputAdornment>;
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
function whatsappUrl(value: string, message?: string) {
  const digits = value.replace(/\D/g, '');
  const withCountry = digits.startsWith('57') ? digits : `57${digits}`;
  const text = message ? `?text=${encodeURIComponent(message)}` : '';
  return `https://wa.me/${withCountry}${text}`;
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
  return product.name?.trim() || [product.brand, product.model, product.line, product.version, product.reference].filter(Boolean).join(' ').trim() || 'Producto';
}
function readableFileSize(bytes: number) {
  if (!Number.isFinite(bytes) || bytes <= 0) return '0 KB';
  if (bytes < 1024 * 1024) return `${Math.ceil(bytes / 1024)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}
function moveReminderKeepingOffset(previousScheduledAt: string, previousReminderAt: string, nextScheduledAt: string) {
  const offset = new Date(previousScheduledAt).getTime() - new Date(previousReminderAt).getTime();
  return new Date(new Date(nextScheduledAt).getTime() - offset).toISOString();
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
function documentType(value: number) { return ['-', 'Cedula', 'Soporte ingresos', 'Recibo servicio', 'Referencias', 'Otro'][value] ?? 'Otro'; }
function deliveryStatus(value: number) { return ['-', 'Programada', 'Entregada', 'Cancelada'][value] ?? 'Programada'; }
function collectionOrderStatus(value: number) { return ['-', 'Emitida', 'Pagada', 'Parcial', 'Vencida', 'Anulada'][value] ?? 'Emitida'; }
function collectionOrderTone(value: number): 'success' | 'warning' | 'error' | 'default' {
  if (value === 2) return 'success';
  if (value === 3 || value === 1) return 'warning';
  if (value === 4 || value === 5) return 'error';
  return 'default';
}
function procedureType(value: number) { return ['-', 'SOAT', 'Matricula', 'Placas', 'Terceros'][value] ?? 'Tramite'; }
function procedureStatus(value: number) { return ['-', 'Pendiente', 'En proceso', 'Completado', 'Atrasado', 'Cancelado'][value] ?? 'Pendiente'; }
function procedureTone(value: number): 'success' | 'warning' | 'error' | 'default' {
  if (value === 3) return 'success';
  if (value === 4 || value === 5) return 'error';
  if (value === 1 || value === 2) return 'warning';
  return 'default';
}
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

