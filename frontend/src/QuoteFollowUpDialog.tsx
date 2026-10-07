import { useState } from 'react';
import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Stack, TextField, Typography } from '@mui/material';
import type { Quote } from './types';

export type QuoteFollowUp = { outcome: string; notes: string; nextContactAt: string | null };

export function QuoteFollowUpDialog({ quote, onClose, onSave }: {
  quote?: Quote;
  onClose: () => void;
  onSave: (quote: Quote, payload: QuoteFollowUp) => Promise<void>;
}) {
  const [outcome, setOutcome] = useState('Contactado');
  const [notes, setNotes] = useState('');
  const [nextContactAt, setNextContactAt] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const close = () => {
    if (saving) return;
    setOutcome('Contactado'); setNotes(''); setNextContactAt(''); setError(''); onClose();
  };
  const save = async () => {
    if (!quote) return;
    setSaving(true); setError('');
    try {
      await onSave(quote, { outcome, notes: notes.trim(), nextContactAt: nextContactAt || null });
      setOutcome('Contactado'); setNotes(''); setNextContactAt(''); onClose();
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'No se pudo guardar el seguimiento.');
    } finally {
      setSaving(false);
    }
  };

  return <Dialog open={!!quote} onClose={close} maxWidth="sm" fullWidth>
    <DialogTitle>Registrar seguimiento</DialogTitle>
    <DialogContent><Stack spacing={2} sx={{ pt: 1 }}>
      <Typography variant="body2" color="text.secondary">{quote?.number} · Registre el resultado y, si hace falta, el próximo contacto.</Typography>
      {error && <Alert severity="error">{error}</Alert>}
      <TextField select label="Resultado" value={outcome} onChange={event => {
        setOutcome(event.target.value);
        if (event.target.value === 'No interesado') setNextContactAt('');
      }} fullWidth>
        {['Contactado', 'Sin respuesta', 'Interesado', 'No interesado'].map(value => <MenuItem key={value} value={value}>{value}</MenuItem>)}
      </TextField>
      <TextField label="Nota breve" value={notes} onChange={event => setNotes(event.target.value)} inputProps={{ maxLength: 1000 }} multiline minRows={2} fullWidth />
      <TextField label="Próximo contacto (opcional)" type="datetime-local" value={nextContactAt}
        onChange={event => setNextContactAt(event.target.value)} InputLabelProps={{ shrink: true }}
        disabled={outcome === 'No interesado'} fullWidth />
    </Stack></DialogContent>
    <DialogActions><Button onClick={close} disabled={saving}>Cancelar</Button><Button variant="contained" onClick={save} disabled={saving}>{saving ? 'Guardando...' : 'Guardar seguimiento'}</Button></DialogActions>
  </Dialog>;
}
