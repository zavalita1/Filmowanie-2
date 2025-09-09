import React, { memo, useRef, useState } from "react";
import { BsChevronDown, BsChevronUp } from 'react-icons/bs'
import { ColumnDef, flexRender, getCoreRowModel, Row, useReactTable } from "@tanstack/react-table";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "../components/ui"
import { AppComponentProps, Layout } from "./Layout";
import { useGetResultsQuery } from "../store/apis/2-Voting/votingApi";
import { ResultRow, Results } from "../models/Results";

// TODO column widths

const votingColumns: ColumnDef<ResultRow>[] = [
  {
    accessorKey: "rank",
    header: "Miejsce"
  },
  {
    accessorKey: "movieTitle",
    header: "Film",
  },
  {
    accessorKey: "votesCount",
    header: "Głosy",
  },
];

const trashVotingColumns = [ {
    accessorKey: "rank",
    header: "",
  },
  {
    accessorKey: "movieTitle",
    header: "Film",
  },
  {
    accessorKey: "votesCount",
    header: "Głosy",
  }];

const ResultsPageComponent: React.FC<AppComponentProps> = (props) => {
    return (<ResultsComponent fn={() => useGetResultsQuery("")}></ResultsComponent>);
}

export type ResultsComponentProps = {
    fn: () => {data?: Results, error?: any, isLoading: boolean};
};

export const ResultsComponent: React.FC<ResultsComponentProps> = props => {
    const { data, error, isLoading } = props.fn();
    const [expandedRows, setExpandedRows] = useState<number[]>([]);
    const table = useReactTable<ResultRow>({
        data: data?.voting ?? [],
        columns: votingColumns,
        getCoreRowModel: getCoreRowModel()
    });

    const trashVotingTable = useReactTable<ResultRow>({
        data: data?.trashVoting ?? [],
        columns: trashVotingColumns,
        getCoreRowModel: getCoreRowModel()
    });

    if (isLoading) {
        return (<div>Loading..</div>);
    } else if (!!error || !data?.voting) {
        return (<>coś się zjebao :(</>);
    }

    return (
        <div className="w-full md:w-4/5 mt-10 select-none">
            <h1 className="scroll-m-20 text-center text-4xl font-extrabold tracking-tight text-balance mb-10">
                Rezultat głosowania
            </h1>
            <div className="overflow-hidden rounded-md border">
                <Table className="min-w-4/5">
                    <TableHeader>
                        {table.getHeaderGroups().map((headerGroup) => (
                            <TableRow key={headerGroup.id}>
                                {headerGroup.headers.map((header) => {
                                    return (
                                        <TableHead key={header.id} >
                                            {
                                                flexRender(header.column.columnDef.header, header.getContext())
                                            }
                                        </TableHead>
                                    )
                                })}
                            </TableRow>
                        ))}
                    </TableHeader>
                    <TableBody>
                        {
                            table.getRowModel().rows.map(row => (
                                <TableRow
                                    key={row.id}>
                                    {row.getVisibleCells().map((cell) => (
                                        <TableCell key={cell.id} className={row.original.isDecorated ? "bg-emerald-200 dark:bg-amber-400 font-bold" : ""}>
                                            {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                        </TableCell>
                                    ))}
                                </TableRow>
                            ))
                        }
                    </TableBody>
                </Table>
            </div>
            <h1 className="scroll-m-20 text-center text-4xl font-extrabold tracking-tight text-balance mb-10 mt-10">
                Rezultat śmieci
            </h1>
            <div className="overflow-hidden rounded-md border">
                <Table>
                    <TableHeader>
                        {trashVotingTable.getHeaderGroups().map((headerGroup) => (
                            <TableRow key={headerGroup.id}>
                                {headerGroup.headers.map((header) => {
                                    return (
                                        <TableHead key={header.id}>
                                            {flexRender(
                                                header.column.columnDef.header,
                                                header.getContext()
                                            )}
                                        </TableHead>
                                    )
                                })}
                            </TableRow>
                        ))}
                    </TableHeader>
                    <TableBody>
                        {trashVotingTable.getRowModel().rows?.length ? (
                            trashVotingTable.getRowModel().rows.map((row) => (
                                <><TableRow
                                    key={row.id}
                                    onClick={() => onTrashResultRowClick(row)}
                                    className={row.original.votesCount === 0 ? "" : "cursor-pointer"}
                                >
                                            {row.getVisibleCells().map((cell) => (
                                                    <TableCell key={cell.id} className={row.original.isDecorated ? "bg-rose-300 dark:bg-rose-500 font-bold" : ""}>
                                                    { 
                                                        cell.column.columnDef.header === "" 
                                                        ? (!expandedRows.includes(row.original.rank) ? <BsChevronDown />: <BsChevronUp />)
                                                        : flexRender(cell.column.columnDef.cell, cell.getContext())
                                                    }
                                                    </TableCell>
                                            ))}
                                </TableRow>
                                { expandedRows.includes(row.original.rank) 
                                ? <TableRow>
                                    <TableCell></TableCell>
                                    <TableCell>
                                    <h3><b>Śmieciarze:</b></h3>
                                    {row.original.voters!.map(x => (<p>{x}</p>))}
                                    </TableCell>
                                </TableRow> 
                                : <></>}
                                </>
                            ))
                        ) : (
                            <TableRow>
                                <TableCell colSpan={votingColumns.length} className="h-24 text-center">
                                    No results.
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </div>
        </div>);

        
function onTrashResultRowClick(row: Row<ResultRow>) {
    if (row.original.votesCount === 0) {
        return;
    }
    
    const rank = row.original.rank;
    setExpandedRows(expandedRows.includes(rank) ? expandedRows.filter(x => x !== rank) : [...expandedRows, rank])
}
}



const module: React.FC<AppComponentProps> = (props) => { return <Layout disableCenterVertically={true}><ResultsPageComponent {...props}/></Layout>}

export default module;