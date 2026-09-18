import { useMemo, useState, type ReactNode } from 'react';
import { Box, InputAdornment, Paper, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow, TextField, Tooltip } from '@mui/material';
import Search from '@mui/icons-material/Search';

const border = '#d9e2ec';
const normalize = (value: string) => value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();

export function CompactRecordsTable({ headers, rows, label, searchLabel, placeholder, empty }: {
  headers: readonly (readonly [string, number, ('left' | 'right' | 'center')?])[];
  rows: { id: string; cells: ReactNode[]; searchText: string }[];
  label: string; searchLabel: string; placeholder: string; empty: string;
}) {
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const filtered = useMemo(() => {
    const terms = normalize(search).trim().split(/\s+/).filter(Boolean);
    return rows.filter(row => terms.every(term => normalize(row.searchText).includes(term)));
  }, [rows, search]);
  const safePage = Math.min(page, Math.max(0, Math.ceil(filtered.length / pageSize) - 1));
  return <Paper variant="outlined" sx={{ overflow: 'hidden', borderColor: border, borderRadius: 1, boxShadow: 'none' }}>
    <Box sx={{ p: 1.25 }}><TextField size="small" label={searchLabel} placeholder={placeholder} value={search}
      onChange={event => { setSearch(event.target.value); setPage(0); }}
      InputProps={{ startAdornment: <InputAdornment position="start"><Search fontSize="small" /></InputAdornment> }}
      sx={{ width: { xs: '100%', sm: 420 } }} /></Box>
    <TableContainer sx={{ overflowX: 'auto', maxHeight: '65vh' }} tabIndex={0} aria-label={`Tabla de ${label}; desplace horizontalmente para ver todas las columnas`}>
      <Table size="small" stickyHeader aria-label={label} sx={{ tableLayout: 'fixed', minWidth: headers.reduce((sum, [, width]) => sum + width, 0),
        '& th, & td': { borderRight: `1px solid ${border}`, borderBottom: `1px solid ${border}`, px: 1, py: .8, fontSize: 12, lineHeight: 1.4, verticalAlign: 'middle' },
        '& th': { bgcolor: '#eef6f8', color: 'primary.main', fontWeight: 700, whiteSpace: 'nowrap' },
        '& tbody tr:nth-of-type(odd)': { bgcolor: '#fbfdff' },
        '& tbody tr:hover': { bgcolor: '#eef9fb' },
        '& .MuiIconButton-root': { border: `1px solid ${border}`, borderRadius: .5, p: .35, width: 25, height: 25, boxShadow: 'none', bgcolor: '#fff', color: 'primary.main' },
        '& .MuiIconButton-colorSuccess': { color: 'success.main' },
        '& .MuiIconButton-colorWarning': { color: 'warning.main' },
        '& .MuiIconButton-colorError': { color: 'error.main' },
        '& .MuiSvgIcon-root': { fontSize: 17 },
        '& .MuiChip-root': { height: 22, borderRadius: .5, fontSize: 11, fontWeight: 700 },
      }}>
        <TableHead><TableRow>{headers.map(([title, width, align]) => <TableCell key={title} align={align} sx={{ width }}>{title}</TableCell>)}</TableRow></TableHead>
        <TableBody>
          {filtered.slice(safePage * pageSize, (safePage + 1) * pageSize).map(row => <TableRow key={row.id}>
            {row.cells.map((cell, index) => <TableCell key={headers[index][0]} align={headers[index][2]}>{typeof cell === 'string' || cell == null
              ? <Tooltip title={cell || ''}><Box component="span" sx={{ display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{cell || '-'}</Box></Tooltip>
              : index === 0 ? <Stack sx={{ '& > .MuiStack-root': { flexWrap: 'nowrap', mt: 0 } }}>{cell}</Stack> : cell}</TableCell>)}
          </TableRow>)}
          {!filtered.length && <TableRow><TableCell colSpan={headers.length} align="center" sx={{ py: '28px !important', color: 'text.secondary' }}>{search ? 'No se encontraron resultados para esta búsqueda.' : empty}</TableCell></TableRow>}
        </TableBody>
      </Table>
    </TableContainer>
    <TablePagination component="div" count={filtered.length} page={safePage} rowsPerPage={pageSize} rowsPerPageOptions={[10, 25, 50]}
      onPageChange={(_, next) => setPage(next)} onRowsPerPageChange={event => { setPageSize(Number(event.target.value)); setPage(0); }}
      labelRowsPerPage="Filas:" labelDisplayedRows={({ from, to, count }) => `${from}–${to} de ${count}${search ? ` (${rows.length} en total)` : ''}`}
      showFirstButton showLastButton getItemAriaLabel={type => ({ first: 'Primera página', last: 'Última página', next: 'Página siguiente', previous: 'Página anterior' }[type])}
      sx={{ '& .MuiTablePagination-toolbar': { minHeight: 48, flexWrap: 'wrap', pl: 1 }, '& .MuiTablePagination-selectLabel, & .MuiTablePagination-displayedRows': { fontSize: 12 } }} />
  </Paper>;
}
