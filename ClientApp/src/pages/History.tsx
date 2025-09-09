import { useGetVotingsListQuery, useGetWatchedMoviesListQuery, useLazyGetVotingQuery } from '../store/apis/5-History/api';
import { Tabs, TabsList, TabsTrigger, TabsContent, Table, TableBody, TableCaption, TableCell, TableHead, TableHeader, TableRow, Select, SelectTrigger, SelectValue, SelectItem, SelectContent } from "../components/ui";
import { AppComponentProps, Layout } from "./Layout";
import { useState } from 'react';
import { ResultsComponent } from './Results';

const History: React.FC<AppComponentProps> = props => {
    return (
         <div className="flex w-full flex-col items-center mb-auto mt-10">
            <Tabs defaultValue="moviesWatched" className="w-full max-w-4/5">
                <TabsList className="w-full ml-10 bg-emerald-100 dark:bg-pink-900">
                    <TabsTrigger value="votingResults">Wyniki ostatnich głosowań</TabsTrigger>
                    <TabsTrigger value="moviesWatched">Lista obejrzanych filmów</TabsTrigger>
                    <TabsTrigger value="charts">Wykresy</TabsTrigger>
                </TabsList>
                <div className="min-h-[300px] relative mt-4">
                    <TabsContent value="votingResults" className="top-0 left-0 w-full">
                        <VotingResults />
                    </TabsContent>
                    <TabsContent value="moviesWatched" className="top-0 left-0 w-full">
                        <WatchedMovies />
                    </TabsContent>
                    <TabsContent value="charts" className="top-0 left-0 w-full">
                        <Charts />
                    </TabsContent>
                </div>
            </Tabs>
        </div>
    )
}

const VotingResults: React.FC = () => {
    const [endedYear, setEndedYear] = useState<number | undefined>();
    const [endedMonth, setEndedMonth] = useState<number | undefined>();
    const [endedDate, setEndedDate] = useState<string | undefined>();
    const { data, error, isLoading } = useGetVotingsListQuery();
    const [trigger, result, lastPromiseInfo] = useLazyGetVotingQuery();

    if (isLoading) {
        return <div>loading...</div>
    }
    else if (error) {
        return <div>Coś się zesrao :(</div>
    }

    const dates = data!.map(x => new Date(x.concluded));
    const yearsToChoseFrom = Array.from(new Set(dates.map(x => x.getFullYear())));
    const monthsToChoseFrom = Array.from(new Set(dates.filter(x => x.getFullYear() === endedYear).map(x => x.getMonth())));
    const daysToChoseFrom = dates.filter(x => x.getFullYear() === endedYear && x.getMonth() == endedMonth);
    
    return (
        <div className="">
        <div className="grid grid-cols-3">
        <Select onValueChange={e => setEndedYear(parseInt(e))} value={endedYear?.toString()}>
            <SelectTrigger className="w-3/5">
                <SelectValue placeholder="Głosowanie z roku" />
            </SelectTrigger>
            <SelectContent>
                {yearsToChoseFrom.map((x, index) => (
                    <SelectItem value={x.toString()} key={index}>{x}</SelectItem>
                ))}
            </SelectContent>
        </Select>
        <Select onValueChange={e => setEndedMonth(parseInt(e))} value={endedMonth?.toString()} disabled={endedYear === undefined}>
            <SelectTrigger className="w-3/5">
                <SelectValue placeholder="Głosowanie z miesiąca" />
            </SelectTrigger>
            <SelectContent>
                {monthsToChoseFrom.map((x, index) => (
                    <SelectItem value={x.toString()} key={index}>{x}</SelectItem>
                ))}
            </SelectContent>
        </Select>
        <Select onValueChange={handleDateChange} value={endedDate} disabled={endedMonth === undefined}>
            <SelectTrigger className="w-3/5">
                <SelectValue placeholder="Głosowanie z dnia" />
            </SelectTrigger>
            <SelectContent>
                {daysToChoseFrom.map((x, index) => (
                    <SelectItem value={x.toISOString()} key={index}>{data!.find(y => y.concluded === x.toISOString())!.concludedLocalized}</SelectItem>
                ))}
            </SelectContent>
        </Select>
        </div>
        <div className='justify-items-center-safe'>
               {!!result.data ? <ResultsComponent fn={() => ({data: result.data, isLoading: false})}/> : <></>}
        </div>
        </div>
    )

    async function handleDateChange(date: string) {
        setEndedDate(date);
        const votingSession = data!.find(x => x.concluded === date);
        await trigger(votingSession!.id, true).unwrap();
    }
}

const WatchedMovies: React.FC = () => {
    const {data, isLoading, error} = useGetWatchedMoviesListQuery();
    
    if (isLoading) {
        return <div>loading...</div>
    }
    else if (error) {
        return <div>Coś się zesrao :(</div>
    }

    return (
        <Table>
            <TableCaption><h1 className="scroll-m-20 text-center text-2xl font-extrabold tracking-tight text-balance mb-10">
                Lista wszystkich filmów oglądanych dotychczas na filmowaniu.
            </h1></TableCaption>
            <TableHeader>
                <TableRow>
                    <TableHead className="w-2/5">Tytuł</TableHead>
                    <TableHead className="w-2/5">Tytuł oryginalny</TableHead>
                    <TableHead>Obejrzany</TableHead>
                    <TableHead className="text-right">Nominowany przez</TableHead>
                </TableRow>
            </TableHeader>
            <TableBody>
                { data!.map((x, index) => (
                <TableRow key={index}>
                    <TableCell className="font-medium">{x.title}</TableCell>
                    <TableCell>{x.originalTitle}</TableCell>
                    <TableCell>{x.watched}</TableCell>
                    <TableCell className="text-right">{x.nominatedBy}</TableCell>
                </TableRow>))
                }
            </TableBody>
        </Table>
    )
}

const Charts: React.FC = () => {
    return (
        <div>TODO, nuttin here yet</div>
    )
}

const wrappedHistory: React.FC<AppComponentProps> = (props) => { return <Layout><History {...props}/></Layout>}

export default wrappedHistory;