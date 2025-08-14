import * as React from 'react';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import Paper from '@mui/material/Paper';
import { HistoryDTO, HistoryEntryDTO } from '../../DTO/Incoming/HistoryDTO';
import { styled } from '@mui/material/styles';
import TableCell,  { tableCellClasses } from '@mui/material/TableCell';
import TableRow from '@mui/material/TableRow';

export function HistoryList(history: HistoryDTO) {
  return (
      <TableContainer component={Paper}>
        <Table aria-label="simple table">
          <TableHead>
            <TableRow>
              <StyledTableCell>Tytuł filmu</StyledTableCell>
              <StyledTableCell>Oryginalny tytuł</StyledTableCell>
              <StyledTableCell>Stworzony w</StyledTableCell>
              <StyledTableCell align="right">Obejrzany</StyledTableCell>
              <StyledTableCell align="right">Nominowany przez</StyledTableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {history.entries.map(mapRow)}
          </TableBody>
        </Table>
      </TableContainer>
);

  function mapRow(row: HistoryEntryDTO) {
    return <TableRow
      key={row.originalTitle}
    >
      <TableCell component="th" scope="row">
        {row.title}
      </TableCell>
      <TableCell component="th" scope="row">
        {row.originalTitle}
      </TableCell>
      <TableCell component="th" scope="row">
        {row.createdYear}
      </TableCell>
      <StyledTableCell align="right">{row.watched}</StyledTableCell>
      <StyledTableCell align="right">{row.nominatedBy}</StyledTableCell>
    </TableRow>
  }
}


const StyledTableCell = styled(TableCell)(({ theme }) => ({
    [`&.${tableCellClasses.head}`]: {
      backgroundColor: "#121212",
      color: theme.palette.common.white,
    },
    [`&.${tableCellClasses.body}`]: {
      fontSize: 14,
    },
  }));