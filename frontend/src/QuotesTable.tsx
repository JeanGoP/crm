import { Fragment, useMemo, useState } from 'react';
import { Box, Chip, Collapse, IconButton, InputAdornment, Paper, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow, TextField, Tooltip, Typography } from '@mui/material';
import Search from '@mui/icons-material/Search';
import Visibility from '@mui/icons-material/Visibility';
import AutoAwesome from '@mui/icons-material/AutoAwesome';
import ChevronRight from '@mui/icons-material/ChevronRight';
import type { Quote } from './types';

const money = (amount: number) => new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(amount);
const date = (value: string) => {
  const parsed = new Date(/^\d{4}-\d{2}-\d{2}$/.test(value) ? `${value}T12:00:00` : value);
  return Number.isNaN(parsed.getTime()) ? '-' : parsed.toLocaleDateString('es-CO');
};
const customerName = (q: Quote) => [q.customerFirstName ? [q.customerFirstName, q.customerMiddleName].filter(Boolean).join(' ') : q.customerFirstNames,
  q.customerLastName ? [q.customerLastName, q.customerSecondLastName].filter(Boolean).join(' ') : q.customerLastNames].filter(Boolean).join(' ');
const normalize = (value: string) => value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
const border = '#d9e2ec';

function CellText({ value }: { value: string }) {
  return <Tooltip title={value}><Box component="span" sx={{ display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{value || '-'}</Box></Tooltip>;
}

export function QuotesTable({ rows, onPreview, onAnalyze }: {
  rows: Quote[]; onPreview: (quote: Quote) => void; onAnalyze: (quote: Quote) => void;
}) {
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [expanded, setExpanded] = useState<string>();
  const filtered = useMemo(() => {
    const terms = normalize(search).trim().split(/\s+/).filter(Boolean);
    return rows.filter(q => {
      const text = normalize([q.number, customerName(q), q.identificationNumber, q.salesPointName,
        q.productName, q.salesPointRateName, q.promotionName, ...(q.items ?? []).map(i => i.productName)].join(' '));
      return terms.every(term => text.includes(term));
    });
  }, [rows, search]);
  const safePage = Math.min(page, Math.max(0, Math.ceil(filtered.length / pageSize) - 1));
  const visible = filtered.slice(safePage * pageSize, (safePage + 1) * pageSize);
  const headers = [
    ['Acciones', 112], ['Modalidad', 105], ['Número', 175], ['Fecha', 105], ['Identificación', 130],
    ['Cliente', 245], ['Sede', 190], ['Productos', 220], ['Valor contado / financiado', 185], ['Plazos', 145], ['Válida hasta', 110],
  ] as const;
  return <Paper variant="outlined" sx={{ overflow: 'hidden', borderColor: border, borderRadius: 1, boxShadow: 'none' }}>
    <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" gap={1} sx={{ p: 1.25 }}>
      <TextField size="small" label="Buscar cotización" placeholder="Cliente, cédula, número, sede o producto" value={search}
        onChange={event => { setSearch(event.target.value); setPage(0); setExpanded(undefined); }}
        InputProps={{ startAdornment: <InputAdornment position="start"><Search fontSize="small" /></InputAdornment> }}
        sx={{ width: { xs: '100%', sm: 420 } }} />
      <Typography variant="caption" color="text.secondary" sx={{ alignSelf: { sm: 'center' } }}>Abra la flecha para ver productos, cuotas y promociones.</Typography>
    </Stack>
    <TableContainer sx={{ overflowX: 'auto', maxHeight: '65vh' }} tabIndex={0} aria-label="Tabla de cotizaciones; desplace horizontalmente para ver todas las columnas">
      <Table size="small" stickyHeader aria-label="Cotizaciones" sx={{ tableLayout: 'fixed', minWidth: 1722,
        '& th, & td': { borderRight: `1px solid ${border}`, borderBottom: `1px solid ${border}`, px: 1, py: .8, fontSize: 12, lineHeight: 1.4, verticalAlign: 'middle' },
        '& th': { bgcolor: '#eef6f8', color: 'primary.main', fontWeight: 700, whiteSpace: 'nowrap' },
        '& .quote-row:nth-of-type(4n + 1)': { bgcolor: '#fbfdff' },
        '& .quote-row:hover': { bgcolor: '#eef9fb' },
        '& .MuiIconButton-root': { border: `1px solid ${border}`, borderRadius: .5, p: .35, bgcolor: '#fff', color: 'primary.main' },
        '& .MuiSvgIcon-root': { fontSize: 17 },
      }}>
        <TableHead><TableRow>{headers.map(([label, width]) => <TableCell key={label} sx={{ width, textAlign: label === 'Valor contado / financiado' ? 'right' : 'left' }}>{label}</TableCell>)}</TableRow></TableHead>
        <TableBody>
          {visible.map(q => {
            const cash = q.creditType === 'Contado';
            const open = expanded === q.id;
            const options = q.financingOptions ?? [];
            const terms = Array.from(new Set((!q.isBundle && (q.items?.length ?? 0) > 1
              ? q.items.flatMap(i => i.financingOptions?.map(o => o.termMonths) ?? [i.termMonths])
              : options.length ? options.map(o => o.termMonths) : [q.termMonths]).filter(t => t > 0))).sort((a, b) => a - b);
            return <Fragment key={q.id}>
              <TableRow className="quote-row" sx={{ bgcolor: open ? '#eef9fb !important' : undefined }}>
                <TableCell><Stack direction="row" spacing={.5}>
                  <Tooltip title={open ? 'Ocultar detalle' : 'Ver detalle'}><IconButton size="small" aria-label={`${open ? 'Ocultar' : 'Ver'} detalle ${q.number}`} aria-expanded={open} aria-controls={`quote-detail-${q.id}`} onClick={() => setExpanded(open ? undefined : q.id)}><ChevronRight sx={{ transform: open ? 'rotate(90deg)' : undefined }} /></IconButton></Tooltip>
                  <Tooltip title="Ver / descargar PDF"><IconButton size="small" aria-label={`Ver PDF ${q.number}`} onClick={() => onPreview(q)}><Visibility /></IconButton></Tooltip>
                  <Tooltip title="Análisis del cliente"><IconButton size="small" aria-label={`Analizar cliente de ${q.number}`} onClick={() => onAnalyze(q)}><AutoAwesome /></IconButton></Tooltip>
                </Stack></TableCell>
                <TableCell><Chip size="small" label={cash ? 'Contado' : 'Crédito'} sx={{ height: 22, borderRadius: .5, fontSize: 11, fontWeight: 700,
                  bgcolor: cash ? '#e6f3ee' : '#eef6f8', color: cash ? '#0f766e' : 'primary.main', borderLeft: '4px solid', borderLeftColor: cash ? '#0f766e' : 'primary.main' }} /></TableCell>
                <TableCell><CellText value={q.number} /></TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>{date(q.quoteDate)}</TableCell>
                <TableCell><CellText value={q.identificationNumber || '-'} /></TableCell>
                <TableCell><CellText value={customerName(q)} /></TableCell>
                <TableCell><CellText value={q.salesPointName || '-'} /></TableCell>
                <TableCell><CellText value={(q.items?.length ?? 0) > 1 ? `${q.items.length} artículos · ${q.isBundle ? 'Financiación conjunta' : 'Comparativo'}` : q.productName} /></TableCell>
                <TableCell align="right" sx={{ whiteSpace: 'nowrap', fontVariantNumeric: 'tabular-nums' }}>{money(cash ? q.estimatedTotalPayment : q.financedAmount)}</TableCell>
                <TableCell><CellText value={cash ? 'No aplica' : terms.length ? `${terms.join(' / ')} cuotas` : 'Sin simulación'} /></TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>{date(q.validUntil)}</TableCell>
              </TableRow>
              <TableRow><TableCell colSpan={headers.length} sx={{ p: '0 !important', borderBottom: open ? undefined : '0 !important' }}>
                <Collapse in={open} timeout="auto" unmountOnExit>
                  <Box id={`quote-detail-${q.id}`} sx={{ p: 2, bgcolor: '#f4f7fb' }}>
                    <Typography fontWeight={700} fontSize={13}>{q.number} · {customerName(q)}</Typography>
                    <Typography variant="body2" sx={{ mt: .5, mb: 1 }}>{q.salesPointName || 'Sin sede'} · {cash ? 'Contado' : `Tasa: ${q.salesPointRateName || 'Tasa general'}`}</Typography>
                    <Stack direction="row" flexWrap="wrap" gap={1.5}>
                      {(!q.isBundle && q.items?.length > 1 ? q.items : [q]).map((item, index) => <Box key={index} sx={{ p: 1.5, minWidth: 250, maxWidth: 480, bgcolor: '#fff', border: `1px solid ${border}`, borderRadius: 1 }}>
                        <Typography fontSize={13} fontWeight={700}>{q.isBundle ? 'Condiciones de todos los artículos' : item.productName}</Typography>
                        {item.promotionDiscount > 0 && <Typography fontSize={12}>Promoción: {item.promotionName || 'Descuento'} · -{money(item.promotionDiscount)}</Typography>}
                        {!cash && <>
                          <Typography fontSize={12}>Inicial completa: {money(item.downPayment)} · Financiado: {money(item.financedAmount)}</Typography>
                          {(item.financingOptions?.length ? item.financingOptions : [{ termMonths: item.termMonths, monthlyPayment: item.estimatedMonthlyPayment }]).map(option =>
                            <Stack key={option.termMonths} direction="row" justifyContent="space-between" gap={3} sx={{ mt: .5 }}><Typography fontSize={12}>{option.termMonths} cuotas</Typography><Typography fontSize={12} fontWeight={700}>{money(option.monthlyPayment)}</Typography></Stack>)}
                        </>}
                        {cash && <Typography fontSize={12}>Precio contado: {money(item.estimatedTotalPayment)}</Typography>}
                      </Box>)}
                    </Stack>
                    {q.isBundle && <Box sx={{ mt: 1 }}>{q.items.map(item => <Typography key={item.id} fontSize={12}>{item.productName} · {money(item.discountedProductPrice)}{item.promotionDiscount > 0 ? ` · ${item.promotionName || 'Descuento'}: -${money(item.promotionDiscount)}` : ''}</Typography>)}</Box>}
                    {q.notes && <Typography variant="body2" sx={{ mt: 1 }}>Observaciones: {q.notes}</Typography>}
                  </Box>
                </Collapse>
              </TableCell></TableRow>
            </Fragment>;
          })}
          {!visible.length && <TableRow><TableCell colSpan={headers.length} align="center" sx={{ py: '28px !important', color: 'text.secondary' }}>{search ? 'No se encontraron cotizaciones para esta búsqueda.' : 'No hay cotizaciones registradas.'}</TableCell></TableRow>}
        </TableBody>
      </Table>
    </TableContainer>
    <TablePagination component="div" count={filtered.length} page={safePage} rowsPerPage={pageSize} rowsPerPageOptions={[10,25,50]}
      onPageChange={(_, next) => { setPage(next); setExpanded(undefined); }}
      onRowsPerPageChange={event => { setPageSize(Number(event.target.value)); setPage(0); setExpanded(undefined); }}
      labelRowsPerPage="Filas:" labelDisplayedRows={({ from, to, count }) => `${from}–${to} de ${count}${search ? ` (${rows.length} en total)` : ''}`}
      showFirstButton showLastButton getItemAriaLabel={type => ({ first: 'Primera página', last: 'Última página', next: 'Página siguiente', previous: 'Página anterior' }[type])}
      sx={{ '& .MuiTablePagination-toolbar': { minHeight: 48, flexWrap: 'wrap', pl: 1 }, '& .MuiTablePagination-selectLabel, & .MuiTablePagination-displayedRows': { fontSize: 12 } }} />
  </Paper>;
}
