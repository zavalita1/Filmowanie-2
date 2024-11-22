import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store';
import { VotingState } from '../../store/votingState';
import { UserState } from '../../store/User';
import { Button } from 'reactstrap';
import SubPageWrapper from '../SubPageWrapper';
import TableContainer from '@mui/material/TableContainer/TableContainer';
import Table from '@mui/material/Table/Table';
import TableHead from '@mui/material/TableHead/TableHead';
import TableRow from '@mui/material/TableRow/TableRow';
import { styled } from '@mui/material/styles';
import TableCell, { tableCellClasses }  from '@mui/material/TableCell';
import TableBody from '@mui/material/TableBody';
import Paper from '@mui/material/Paper';
import TextField from '@mui/material/TextField';
import Info from '../NotificationPopups/Info';
import { actionCreators }  from '../../store/App/actionCreators';

type AdminProps = UserState & ApplicationState  
& typeof actionCreators &  {
    state?: VotingState
}

  const AdminWrapper = (props: AdminProps) => {
    const subWrapperProps = {
        child: Admin,
        childProps: props
    };
  
    return (<SubPageWrapper {...subWrapperProps} ></SubPageWrapper>);
  }

function Admin(props: AdminProps): any {
    if (props.user?.isAdmin !== true)    {
        return <></>
    }

    const [users, setUsers] = React.useState<UserDTO[]>([]);
    const [draftUserName, setDraftUserName] = React.useState("");
    const [reloadUsers, setReloadUsers] = React.useState(true);

    React.useEffect(() => {
        loadUsers().then(loadedUsers => setUsers(loadedUsers));
    }, [reloadUsers]);

    return (
        <React.Fragment>
            <div> Status: {VotingState[props.state!]}</div>
            <br />
            <div>
                <Button onClick={startVoting} disabled={props.state == VotingState.Voting}> Resume voting. </Button>
            </div>
            <br />
            <div>
                <Button onClick={endVoting} disabled={props.state == VotingState.Results}> End voting. </Button>
            </div>
            <br />
            <div>
                <Button onClick={newVoting} disabled={props.state != VotingState.Results}> New Vote</Button>
            </div>

            <br/>
            ---- <br/>
            Users <br/>
            ----

            <div>
            <TableContainer component={Paper}>
        <Table aria-label="simple table">
          <TableHead>
            <TableRow>
              <StyledTableCell>Login</StyledTableCell>
              <StyledTableCell align="right"></StyledTableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {users.map(mapRow)}
          </TableBody>
        </Table>
      </TableContainer>
    
    <br/><br/>---<br/>Create new users<br/> ---<br/>
      <TextField id="standard-basic" label="Login" variant="standard" onChange={e => setDraftUserName(e.target.value)}/>
      <Button onClick={() => createUser(draftUserName)}>Create user!</Button>
            </div>

            <br />
            <div>
                <Button onClick={invalidateUserCache}> Invalidate user cache </Button>
            </div>
        </React.Fragment>
    );

    function createUser(name: string) {
        const fetchOptions = { 
            method: "POST", 
            headers: { 'content-type': 'application/json;charset=UTF-8', },
            body: JSON.stringify({ Id: name, DisplayName: name }) // TODO
        };
    
        fetch(`api/user`, fetchOptions)
        .then(() => props.setError(`user ${name} created!`))
        .catch(() => props.setError("failed to create user :("))
        .finally(() => setReloadUsers(v => !v));
    }
};


function mapRow(row: UserDTO) {
    return <TableRow
        key={row.username}
    >
      <TableCell component="th" scope="row">
            {row.username}
      </TableCell>
        <StyledTableCell align="right"><Button><a href={`api/user/${row.username}`} className='button-link'>View details</a></Button></StyledTableCell>
    </TableRow>
  }

function startVoting() {
    const fetchOptions = { 
        method: "POST", 
        headers: { 'content-type': 'application/json;charset=UTF-8', } 
    };

    fetch("state/startVote", fetchOptions);
}


function endVoting() {
    const fetchOptions = { 
        method: "POST", 
        headers: { 'content-type': 'application/json;charset=UTF-8', } 
    };

    fetch("state/endVote", fetchOptions);
}

function invalidateUserCache() {
    const fetchOptions = { 
        method: "POST", 
        headers: { 'content-type': 'application/json;charset=UTF-8', } 
    };

    fetch("state/invalidateUserCache", fetchOptions);
}

function newVoting() {
    const fetchOptions = { 
        method: "POST", 
        headers: { 'content-type': 'application/json;charset=UTF-8', } 
    };

    fetch("state/newVote", fetchOptions).catch(() => alert('cos nie tak'));
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

function loadUsers() {
    const fetchOptions = { 
        method: "GET", 
        headers: { 'content-type': 'application/json;charset=UTF-8', } 
    };

    return fetch("api/user/all", fetchOptions)
    .catch(() => alert('cos nie tak'))
    .then(response => response?.json())
    .then(data => data as UserDTO[]);
}

type UserDTO = {
    username: string,
    code: string
};

export default connect(
    (state: ApplicationState) => ({  ...state.user, state: state.state?.state, app: state.app  }),
    { ...actionCreators}
)(AdminWrapper as any);
