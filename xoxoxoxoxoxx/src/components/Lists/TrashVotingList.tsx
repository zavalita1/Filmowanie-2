import * as React from 'react';
import Box from '@mui/material/Box';
import Collapse from '@mui/material/Collapse';
import IconButton from '@mui/material/IconButton';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell ,  { tableCellClasses } from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import { TrashVotingResultRowDTO, VotingResultDTO } from '../../DTO/Incoming/VotingResultDTO';
import { styled } from '@mui/material/styles';

function Row(props: { row: TrashVotingResultRowDTO, place: number | "" }) {
  const { row } = props;
  const [open, setOpen] = React.useState(false);

  let rowSx: {} = { '& > *': { borderBottom: 'unset' }};

  if (row.isAwarded) {
    rowSx = {...rowSx, backgroundColor: "#a74141" }
  }

  return (
    <React.Fragment>
      <TableRow sx={rowSx}>
        <TableCell>
          <IconButton
            aria-label="expand row"
            size="small"
            onClick={() => setOpen(!open)}
          >
            {open ? <KeyboardArrowUpIcon /> : <KeyboardArrowDownIcon />}
          </IconButton>
        </TableCell>
        <TableCell component="th" scope="row">
            {props.place}
        </TableCell>
        <TableCell align="right">{row.movieName}</TableCell>
        <TableCell align="right">{row.voters.length}</TableCell>
      </TableRow>
      <TableRow>
        <TableCell style={{ paddingBottom: 0, paddingTop: 0 }} colSpan={6}>
          <Collapse in={open} timeout="auto" unmountOnExit>
            <Box sx={{ margin: 1 }}>
              <Table size="small" aria-label="purchases">
                <TableHead>
                  <TableRow>
                    <TableCell><Typography variant='body2'><b>Śmierciarze</b></Typography></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {row.voters.map((voter) => (
                    <TableRow key={voter}>
                      <TableCell component="th" scope="row">
                        {voter}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Box>
          </Collapse>
        </TableCell>
      </TableRow>
    </React.Fragment>
  );
}

export default function TrashVotingList(votes: VotingResultDTO) {
    let counter = 1;
    let previousValue = 0;
    return (
    <TableContainer component={Paper}>
      <div >
      <Table aria-label="collapsible table">
        <TableHead>
          <TableRow sx={{backgroundColor: "#000000", color: "#ffffff"}}>
            <StyledTableCell />
            <StyledTableCell>Miejsce</StyledTableCell>
            <StyledTableCell align="right">Tytuł Filmu</StyledTableCell>
            <StyledTableCell align="right">Liczba głosów</StyledTableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {votes.trashVotingResults.map((row) => {
            let place: "" | number = row.voters.length === previousValue ? "" : counter;
            counter++;
            previousValue = row.voters.length;
            return <Row key={row.movieName} row={row} place={place} />
          })}
        </TableBody>
      </Table>
      </div>
    </TableContainer>
  );
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