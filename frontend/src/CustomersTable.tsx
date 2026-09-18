import { useState } from 'react';
import { Box, Chip, IconButton, Paper, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow, Tooltip } from '@mui/material';
import Visibility from '@mui/icons-material/Visibility';
import Edit from '@mui/icons-material/Edit';
import Delete from '@mui/icons-material/Delete';
import type { Customer } from './types';

const border = '#d9e2ec';
const headers = [
  ['Acciones', 112], ['Estado', 115], ['Identificación', 145], ['Primer nombre', 180],
  ['Segundo nombre', 155], ['Primer apellido', 165], ['Segundo apellido', 165],
  ['Teléfono', 155], ['Ciudad', 170], ['Etiquetas', 200],
] as const;

function CellText({ value }: { value?: string }) {
  return <Tooltip title={value || ''}><Box component="span" sx={{ display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{value || '-'}</Box></Tooltip>;
}

export function CustomersTable({ rows, total, filtered, onView, onEdit, onDelete }: {
  rows: Customer[]; total: number; filtered: boolean;
  onView: (customer: Customer) => void; onEdit: (customer: Customer) => void; onDelete?: (customer: Customer) => void;
}) {
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const safePage = Math.min(page, Math.max(0, Math.ceil(rows.length / pageSize) - 1));
  const visible = rows.slice(safePage * pageSize, (safePage + 1) * pageSize);
  return <Paper variant="outlined" sx={{ overflow: 'hidden', borderColor: border, borderRadius: 1, boxShadow: 'none' }}>
    <TableContainer sx={{ overflowX: 'auto', maxHeight: '65vh' }} tabIndex={0} aria-label="Tabla de clientes; desplace horizontalmente para ver todas las columnas">
      <Table size="small" stickyHeader aria-label="Clientes" sx={{ tableLayout: 'fixed', minWidth: 1662,
        '& th, & td': { borderRight: `1px solid ${border}`, borderBottom: `1px solid ${border}`, px: 1, py: .8, fontSize: 12, lineHeight: 1.4, verticalAlign: 'middle' },
        '& th': { bgcolor: '#eef6f8', color: 'primary.main', fontWeight: 700, whiteSpace: 'nowrap' },
        '& .customer-row:nth-of-type(odd)': { bgcolor: '#fbfdff' },
        '& .customer-row:hover': { bgcolor: '#eef9fb' },
        '& .MuiIconButton-root': { border: `1px solid ${border}`, borderRadius: .5, p: .35, bgcolor: '#fff', color: 'primary.main' },
        '& .MuiSvgIcon-root': { fontSize: 17 },
      }}>
        <TableHead><TableRow>{headers.map(([label, width]) => <TableCell key={label} sx={{ width }}>{label}</TableCell>)}</TableRow></TableHead>
        <TableBody>
          {visible.map(customer => {
            const label = customer.name || [customer.firstNames, customer.lastNames].filter(Boolean).join(' ');
            const status = ['-', 'Activo', 'Inactivo', 'Suspendido'][customer.status] || '-';
            const color = customer.status === 1 ? '#0f766e' : customer.status === 3 ? '#b45309' : '#5b6472';
            return <TableRow key={customer.id} className="customer-row">
              <TableCell><Stack direction="row" spacing={.5}>
                <Tooltip title="Ver cliente"><IconButton size="small" aria-label={`Ver cliente ${label}`} onClick={() => onView(customer)}><Visibility /></IconButton></Tooltip>
                <Tooltip title="Editar cliente"><IconButton size="small" aria-label={`Editar cliente ${label}`} onClick={() => onEdit(customer)}><Edit /></IconButton></Tooltip>
                {onDelete && <Tooltip title="Eliminar cliente"><IconButton size="small" aria-label={`Eliminar cliente ${label}`} onClick={() => onDelete(customer)}><Delete /></IconButton></Tooltip>}
              </Stack></TableCell>
              <TableCell><Chip size="small" label={status} sx={{ height: 22, borderRadius: .5, fontSize: 11, fontWeight: 700,
                bgcolor: customer.status === 1 ? '#e6f3ee' : customer.status === 3 ? '#fff4e5' : '#f1f5f9', color, borderLeft: `4px solid ${color}` }} /></TableCell>
              <TableCell><CellText value={customer.identificationNumber} /></TableCell>
              <TableCell><CellText value={customer.firstName || customer.firstNames || customer.name} /></TableCell>
              <TableCell><CellText value={customer.middleName} /></TableCell>
              <TableCell><CellText value={customer.lastName || customer.lastNames} /></TableCell>
              <TableCell><CellText value={customer.secondLastName} /></TableCell>
              <TableCell><CellText value={customer.phone} /></TableCell>
              <TableCell><CellText value={customer.city} /></TableCell>
              <TableCell><CellText value={customer.tags} /></TableCell>
            </TableRow>;
          })}
          {!visible.length && <TableRow><TableCell colSpan={headers.length} align="center" sx={{ py: '28px !important', color: 'text.secondary' }}>{filtered ? 'No se encontraron clientes para estos filtros.' : 'No hay clientes registrados.'}</TableCell></TableRow>}
        </TableBody>
      </Table>
    </TableContainer>
    <TablePagination component="div" count={rows.length} page={safePage} rowsPerPage={pageSize} rowsPerPageOptions={[10, 25, 50]}
      onPageChange={(_, next) => setPage(next)} onRowsPerPageChange={event => { setPageSize(Number(event.target.value)); setPage(0); }}
      labelRowsPerPage="Filas:" labelDisplayedRows={({ from, to, count }) => `${from}–${to} de ${count}${filtered ? ` (${total} en total)` : ''}`}
      showFirstButton showLastButton getItemAriaLabel={type => ({ first: 'Primera página', last: 'Última página', next: 'Página siguiente', previous: 'Página anterior' }[type])}
      sx={{ '& .MuiTablePagination-toolbar': { minHeight: 48, flexWrap: 'wrap', pl: 1 }, '& .MuiTablePagination-selectLabel, & .MuiTablePagination-displayedRows': { fontSize: 12 } }} />
  </Paper>;
}
