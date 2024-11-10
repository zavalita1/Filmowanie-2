import * as React from 'react';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import Paper from '@mui/material/Paper';
import { VotingResultDTO, VotingResultRowDTO } from '../../DTO/Incoming/VotingResultDTO';
import { styled } from '@mui/material/styles';
import TableCell,  { tableCellClasses } from '@mui/material/TableCell';
import TableRow from '@mui/material/TableRow';
import { Grid } from '@mui/material';

export function VotingList(votes: VotingResultDTO) {
  let counter = 1;
  let previousValue = 0;
  
  return (
      <TableContainer component={Paper}>
        <Table aria-label="simple table">
          <TableHead>
            <TableRow>
              <StyledTableCell>Miejsce</StyledTableCell>
              <StyledTableCell align="right">Tytuł filmu</StyledTableCell>
              <StyledTableCell align="right">Liczba głosów</StyledTableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {votes.votingResults.map(mapRow)}
          </TableBody>
        </Table>
      </TableContainer>
);

  function mapRow(row: VotingResultRowDTO) {
    let place = row.votersCount === previousValue ? "" : counter;
    counter++;
    previousValue = row.votersCount;

    let rowSx: {} = { '&:last-child td, &:last-child th': { border: 0 } };

  if (row.isWinner === true) {
    rowSx = {...rowSx, backgroundColor: "#368a52" }
  }

    return <TableRow
      key={row.movieName}
      sx={rowSx}
    >
      <TableCell component="th" scope="row">
        {place}
      </TableCell>
      <StyledTableCell align="right">{row.movieName}</StyledTableCell>
      <StyledTableCell align="right">{row.votersCount}</StyledTableCell>
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

  const StyledTableRow = styled(TableRow)(({ theme }) => ({
    '&:nth-of-type(odd)': {
      backgroundColor: theme.palette.action.hover,
    },
    // hide last border
    '&:last-child td, &:last-child th': {
      border: 0,
    },
  }));