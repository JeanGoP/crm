import { useCallback, useEffect, useRef, useState } from 'react';
import { Alert, Badge, Box, Button, Divider, IconButton, Paper, Popover, Snackbar, Stack, Switch, Tooltip, Typography } from '@mui/material';
import NotificationsNone from '@mui/icons-material/NotificationsNone';
import { useNavigate } from 'react-router-dom';
import { api } from './api';
import { useAuthStore } from './store';

type InternalNotification = {
  id: string;
  type: string;
  title: string;
  message: string;
  creditApplicationId: string;
  createdAt: string;
  readAt?: string | null;
};

const soundKey = 'crm-credit-notification-sound';

function playChime() {
  try {
    const context = new AudioContext();
    const oscillator = context.createOscillator();
    const volume = context.createGain();
    oscillator.type = 'sine';
    oscillator.frequency.setValueAtTime(740, context.currentTime);
    oscillator.frequency.setValueAtTime(980, context.currentTime + 0.12);
    volume.gain.setValueAtTime(0.0001, context.currentTime);
    volume.gain.exponentialRampToValueAtTime(0.07, context.currentTime + 0.025);
    volume.gain.exponentialRampToValueAtTime(0.0001, context.currentTime + 0.3);
    oscillator.connect(volume).connect(context.destination);
    oscillator.start();
    oscillator.stop(context.currentTime + 0.31);
    oscillator.onended = () => { void context.close(); };
  } catch { /* El navegador puede bloquear el sonido hasta que el usuario interactúe. */ }
}

export function NotificationCenter() {
  const navigate = useNavigate();
  const token = useAuthStore((state) => state.accessToken);
  const activeCompanyId = useAuthStore((state) => state.activeCompanyId);
  const email = useAuthStore((state) => state.user?.email);
  const userId = useAuthStore((state) => state.user?.id);
  const canManage = useAuthStore((state) => state.user?.roles.some((role) => role === 'Administrador' || role === 'Supervisor') ?? false);
  const [rows, setRows] = useState<InternalNotification[]>([]);
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const [toast, setToast] = useState<InternalNotification | null>(null);
  const [sound, setSound] = useState(() => localStorage.getItem(soundKey) === 'true');
  const knownIds = useRef(new Set<string>());
  const loaded = useRef(false);
  const soundRef = useRef(sound);
  const unreadCount = rows.filter((row) => !row.readAt).length;

  useEffect(() => { soundRef.current = sound; localStorage.setItem(soundKey, String(sound)); }, [sound]);

  const refresh = useCallback(async () => {
    if (!canManage) return;
    try {
      const { data } = await api.get<InternalNotification[]>('/api/notifications');
      if (loaded.current) {
        const latest = data.find((row) => !knownIds.current.has(row.id));
        if (latest) {
          setToast(latest);
          if (soundRef.current) playChime();
        }
      }
      data.forEach((row) => knownIds.current.add(row.id));
      loaded.current = true;
      setRows(data);
    } catch { /* La bandeja se recuperará en el siguiente intento. */ }
  }, [canManage]);

  useEffect(() => {
    knownIds.current.clear();
    loaded.current = false;
    setRows([]);
    if (!token) return;
    void refresh();
    const controller = new AbortController();
    let retryTimer: number | undefined;
    let active = true;
    const connect = async () => {
      try {
        const url = api.getUri({ url: '/api/notifications/stream' });
        const headers: Record<string, string> = { Authorization: `Bearer ${token}` };
        if (email?.toLowerCase() === 'admin@demo.com' && activeCompanyId) headers['X-Company-Id'] = activeCompanyId;
        const response = await fetch(url, { headers, signal: controller.signal, cache: 'no-store' });
        if (!response.ok || !response.body) throw new Error('Sin conexión de avisos');
        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let pending = '';
        while (active) {
          const { value, done } = await reader.read();
          if (done) break;
          pending = (pending + decoder.decode(value, { stream: true })).replace(/\r\n/g, '\n');
          const chunks = pending.split('\n\n');
          pending = chunks.pop() ?? '';
          for (const chunk of chunks) {
            if (!chunk.includes('event: refresh')) continue;
            const dataLine = chunk.split('\n').find((line) => line.startsWith('data: '));
            if (dataLine) {
              try {
                const { applicationId } = JSON.parse(dataLine.slice(6)) as { applicationId?: string };
                if (applicationId) window.dispatchEvent(new CustomEvent('credit-application-changed', { detail: applicationId }));
              } catch { /* Se ignora un evento incompleto. */ }
            }
            void refresh();
          }
        }
      } catch { /* Se reintenta automáticamente. */ }
      if (active) retryTimer = window.setTimeout(() => { void connect(); }, 3000);
    };
    void connect();
    return () => {
      active = false;
      controller.abort();
      if (retryTimer) window.clearTimeout(retryTimer);
    };
  }, [token, activeCompanyId, email, userId, refresh]);

  const openApplication = async (row: InternalNotification) => {
    if (!row.readAt) {
      await api.put(`/api/notifications/${row.id}/read`).catch(() => undefined);
      setRows((current) => current.map((item) => item.id === row.id ? { ...item, readAt: new Date().toISOString() } : item));
    }
    setAnchor(null);
    navigate(`/solicitudes-credito?solicitud=${row.creditApplicationId}&tab=${row.type === 'ListaParaDecision' ? 'estudio' : 'pendiente'}`);
  };

  const markAllRead = async () => {
    await api.put('/api/notifications/read-all');
    setRows((current) => current.map((row) => ({ ...row, readAt: row.readAt ?? new Date().toISOString() })));
  };

  return canManage ? <>
    <Tooltip title="Avisos de solicitudes">
      <IconButton aria-label={`Avisos de solicitudes: ${unreadCount} sin leer`} onClick={(event) => setAnchor(event.currentTarget)}
        sx={{ border: '1px solid #d9e2ec', bgcolor: '#fff' }}>
        <Badge badgeContent={unreadCount} color="error" max={99}><NotificationsNone /></Badge>
      </IconButton>
    </Tooltip>
    <Popover open={Boolean(anchor)} anchorEl={anchor} onClose={() => setAnchor(null)}
      anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }} transformOrigin={{ vertical: 'top', horizontal: 'right' }}
      slotProps={{ paper: { sx: { width: { xs: 'min(94vw, 420px)', sm: 420 }, maxHeight: 'min(75vh, 620px)', mt: 1 } } }}>
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ p: 2, pb: 1 }}>
        <Box><Typography fontWeight={900}>Avisos</Typography><Typography variant="caption" color="text.secondary">{unreadCount} sin leer</Typography></Box>
        <Button size="small" disabled={!unreadCount} onClick={() => { void markAllRead(); }}>Marcar leídos</Button>
      </Stack>
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ px: 2, pb: 1 }}>
        <Typography variant="body2">Sonido de avisos</Typography>
        <Switch size="small" checked={sound} onChange={(_, checked) => { setSound(checked); if (checked) playChime(); }} inputProps={{ 'aria-label': 'Activar sonido de avisos' }} />
      </Stack>
      <Divider />
      <Box sx={{ maxHeight: '55vh', overflowY: 'auto' }}>
        {!rows.length && <Typography color="text.secondary" sx={{ p: 2 }}>No hay avisos todavía.</Typography>}
        {rows.map((row) => <Paper key={row.id} square elevation={0} onClick={() => { void openApplication(row); }}
          sx={{ px: 2, py: 1.4, cursor: 'pointer', bgcolor: row.readAt ? '#fff' : '#eaf7fb', borderBottom: '1px solid #e2e8f0', '&:hover': { bgcolor: '#f1f5f9' } }}>
          <Stack direction="row" justifyContent="space-between" gap={1}>
            <Typography fontSize={13} fontWeight={row.readAt ? 700 : 900}>{row.title}</Typography>
            {!row.readAt && <Box sx={{ width: 8, height: 8, bgcolor: '#155e75', borderRadius: '50%', mt: .5, flexShrink: 0 }} />}
          </Stack>
          <Typography fontSize={12.5} color="text.secondary">{row.message}</Typography>
          <Typography fontSize={11} color="text.secondary" sx={{ mt: .5 }}>{new Date(row.createdAt).toLocaleString('es-CO')}</Typography>
        </Paper>)}
      </Box>
    </Popover>
    <Snackbar open={!!toast} autoHideDuration={7000} onClose={() => setToast(null)} anchorOrigin={{ vertical: 'top', horizontal: 'right' }}>
      <Alert severity="info" onClose={() => setToast(null)} action={toast ? <Button color="inherit" size="small" onClick={() => { void openApplication(toast); setToast(null); }}>Ver</Button> : undefined}>
        <strong>{toast?.title}</strong><br />{toast?.message}
      </Alert>
    </Snackbar>
  </> : null;
}
