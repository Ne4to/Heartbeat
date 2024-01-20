import React, {useEffect, useContext} from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import {DataGrid, GridColDef, GridRenderCellParams, GridToolbar, GridValueGetterParams} from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import {AlertContext} from '../contexts/alertContext';
import {TraversingHeapModeSelect} from '../components/TraversingHeapModeSelect'
import {GenerationSelect} from '../components/GenerationSelect'

import getClient from '../lib/getClient'
import {formatAddress, formatSize} from '../lib/gridFormatter';
import {
    Generation,
    ObjectTypeStatistics,
    ProblemDetails,
    TraversingHeapModes,
    TraversingHeapModesObject
} from '../client/models';
import toHexAddress from "../lib/toHexAddress";
import prettyBytes from "pretty-bytes";
import {PropertiesTable, PropertyRow} from "../components/PropertiesTable";
import {renderMethodTable} from "../lib/gridRenderCell";
import {methodTableColumn, sizeColumn} from "../lib/gridColumns";

const columns: GridColDef[] = [
    methodTableColumn,
    {
        field: 'instanceCount',
        headerName: 'Count',
        type: 'number',
        width: 100
    },
    {
        ...sizeColumn,
        field: 'totalSize',
        headerName: 'Total size',
        width: 120
    },
    {
        field: 'typeName',
        headerName: 'Type',
        minWidth: 200,
        flex: 1
    }
];

// TODO save state - https://mui.com/x/react-data-grid/state/#save-and-restore-the-state-from-external-storage

export const HeapDumpStat = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [mode, setMode] = React.useState<TraversingHeapModes>(TraversingHeapModesObject.All)
    const [generation, setGeneration] = React.useState<Generation>()
    const [statistics, setStatistics] = React.useState<ObjectTypeStatistics[]>([])

    const showMessage = useContext(AlertContext);

    useEffect(() => {
        // TODO fix async errors
        // TODO handle errors
        populateStatistics(mode, generation).catch(console.error);
    }, [mode, generation]);

    const populateStatistics = async (mode: TraversingHeapModes, generation?: Generation) => {
        const client = getClient();

        try {
            setLoading(true)
            const stats = await client.api.dump.heapDumpStatistics.get(
                {queryParameters: {traversingMode: mode, generation: generation}}
            );
            setStatistics(stats!)
            setLoading(false)
        } catch (error: unknown) {
            const problemDetails = error as ProblemDetails
            showMessage(problemDetails.title || 'API call error')
        }
    }

    const renderStatisticsTable = (statistics: ObjectTypeStatistics[]) => {
        return (
            <div style={{flexGrow: 1, width: '100%'}}>

                <DataGrid
                    rows={statistics}
                    getRowId={(row) => row.typeName}
                    columns={columns}
                    rowHeight={25}
                    density='compact'
                    pageSizeOptions={[20, 50, 100]}
                    pagination
                    initialState={{
                        filter: {
                            filterModel: {
                                items: [],
                                quickFilterValues: [],
                            },
                        },
                        sorting: {
                            sortModel: [{field: 'totalSize', sort: 'desc'}],
                        },
                        pagination: {paginationModel: {pageSize: 20}},
                    }}
                    slots={{toolbar: GridToolbar}}
                    slotProps={{
                        toolbar: {
                            showQuickFilter: true,
                        },
                    }}
                />

            </div>
        );
    }

    let contents = loading
        ? <Box sx={{width: '100%'}}>
            <LinearProgress/>
        </Box>
        : renderStatisticsTable(statistics);

    const totalCount = statistics.reduce((sum, current) => sum + (current.instanceCount || 0), 0)
    const totalSize = statistics.reduce((sum, current) => sum + (current.totalSize || 0), 0)

    const propertyRows: PropertyRow[] = [
        {title: 'Total count', value: String(totalCount)},
        {title: 'Total size', value: prettyBytes(totalSize)},
    ]

    return (
        // <Box display="flex">
        <div style={{display: 'flex', flexFlow: 'column'}}>
            <div style={{flexGrow: 1}}>
                <TraversingHeapModeSelect mode={mode} onChange={(mode) => setMode(mode)}/>
                <GenerationSelect generation={generation} onChange={(generation) => setGeneration(generation)}/>
            </div>
            <PropertiesTable rows={propertyRows}/>
            {contents}
        </div>
        // </Box>
    );
}