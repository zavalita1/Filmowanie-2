import { useEffect, useState } from "react";
import { useNavigate } from "react-router";
import { Button, Input, Table, TableBody, TableCaption, TableCell, TableHead, TableHeader, TableRow } from "../components/ui";
import { VotingStatus } from "../consts/votingStatus";
import { AppComponentProps, Layout } from "./Layout";
import { useResumeVotingMutation, useEndVotingMutation, useStartVotingMutation, useGetAllUsersQuery, useCreateUserMutation } from "../store/apis/3-Admin/api";
import ky from 'ky';

const Admin: React.FC<AppComponentProps> = props => {
    const navigate = useNavigate();
    const [startVote] = useStartVotingMutation();
    const [endVote] = useEndVotingMutation();
    const [resumeVote] = useResumeVotingMutation();
    const [createUser] = useCreateUserMutation();

    useEffect(() => {
        if (!props.userData?.isAdmin)
            navigate('/');
    }, [props.userData]);
    const [draftUserName, setDraftUserName] = useState("");
    const [pushNotification, setPushNotification] = useState("");
    const {data, isLoading, error} = useGetAllUsersQuery();

    if (isLoading) {
        return <div>loading...</div>;
    } else if (error) {
        return <div>JAPA DUPA ALE FAZA</div>
    }
    
    return (
    <div>
        <h1 className="scroll-m-20 text-center text-4xl font-extrabold tracking-tight text-balance mb-20">
                Voting status: {VotingStatus[props.votingStatus]}
        </h1>
        <Button className="mb-10" onClick={async () => {await endVote().unwrap(); window.location.reload() }} disabled={props.votingStatus == VotingStatus.Results}>End voting!</Button>
        <br/>
        <Button className="mb-10" onClick={async () => {await resumeVote().unwrap(); window.location.reload()}} disabled={props.votingStatus != VotingStatus.Results}>Resume voting!</Button>
        <br/>
        <Button className="mb-10" onClick={async () => {await startVote().unwrap(); window.location.reload()}} disabled={props.votingStatus != VotingStatus.Results}>New vote!</Button>
        <h1 className="scroll-m-20 text-center text-4xl font-extrabold tracking-tight text-balance mb-20">
                Users:
        </h1>
        <Table>
            <TableCaption>All users</TableCaption>
            <TableHeader>
                <TableRow>
                    <TableHead className="w-[100px]">Username</TableHead>
                    <TableHead>Details</TableHead>
                </TableRow>
            </TableHeader>
            <TableBody>
                { data!.map(x => 
                    <TableRow>
                    <TableCell>
                    {x.name}
                    </TableCell>
                    <TableCell>
                        <Button onClick={() => document.location = `/api/user/${x.id}`}>Details</Button>
                    </TableCell>
                    </TableRow>
                    )}
            </TableBody>
        </Table>

         <h1 className="scroll-m-20 text-center text-4xl font-extrabold tracking-tight text-balance mb-20">
                New user creation:
        </h1>
         <Input id="standard-basic" onChange={e => setDraftUserName(e.target.value)}/>
            <Button onClick={() => createUser(draftUserName)}>Create user!</Button>

            <h1 className="scroll-m-20 text-center text-4xl font-extrabold tracking-tight text-balance mb-20">
                Notify with push:
        </h1>
         <Input id="standard-basic" onChange={e => setPushNotification(e.target.value)}/>
            <Button onClick={() => ky('api/pushNotification/notify', {json: { message: pushNotification }, method: 'post'})}>Push!</Button>
        </div>
);
}

const wrappedAdmin: React.FC<AppComponentProps> = (props) => { return <Layout><Admin {...props}/></Layout>}

export default wrappedAdmin;