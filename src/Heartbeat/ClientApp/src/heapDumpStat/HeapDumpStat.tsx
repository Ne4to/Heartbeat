import React, { useEffect } from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import { DataGrid, GridColDef, GridRenderCellParams, GridToolbar, GridValueGetterParams } from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import { TraversingHeapModeSelect } from '../components/TraversingHeapModeSelect'
import { GenerationSelect } from '../components/GenerationSelect'

import getClient from '../lib/getClient'
import { formatSize } from '../lib/gridFormatter';
import {
    GCSegmentKind,
    Generation,
    ObjectTypeStatistics,
    TraversingHeapModes,
    TraversingHeapModesObject
} from '../client/models';

const columns: GridColDef[] = [
    { field: 'instanceCount', headerName: 'Count', type: 'number', width: 130 },
    {
        field: 'totalSize',
        headerName: 'Total size',
        type: 'number',
        width: 130,
        valueGetter: (params: GridValueGetterParams) => params.row.totalSize,
        valueFormatter: formatSize
    },
    {
        field: 'typeName',
        headerName: 'Type',
        minWidth: 200,
        flex: 1,
        renderCell: (params: GridRenderCellParams) => {
            const mt = params.row.methodTable;
            const mtHex = mt.toString(16)
            return (
                <a href={'#/object-instances?mt=' + mtHex}>{params.value}</a>
            )
        }
    }
];

// TODO save state - https://mui.com/x/react-data-grid/state/#save-and-restore-the-state-from-external-storage

export const HeapDumpStat = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [mode, setMode] = React.useState<TraversingHeapModes>(TraversingHeapModesObject.All)
    const [generation, setGeneration] = React.useState<Generation>()
    const [statistics, setStatistics] = React.useState<ObjectTypeStatistics[]>([])

    useEffect(() => {
        // TODO fix async errors
        // TODO handle errors
        populateStatistics(mode, generation);
    }, [mode, generation]);

    const populateStatistics = async (mode: TraversingHeapModes, generation?: Generation) => {
        const client = getClient();

        try {
            const stats = await client.api.dump.heapDumpStatistics.get(
                {queryParameters: {traversingMode: mode, generation: generation}}
            );
            setStatistics(stats!)
            setLoading(false)
        } catch (error) {
            console.log('ERROR!!!!!!!!!!!')
            console.log(error)
        }
    }

    const renderStatisticsTable = (statistics: ObjectTypeStatistics[]) => {
        return (
            <div style={{ flexGrow: 1, width: '100%' }}>

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
                            sortModel: [{ field: 'totalSize', sort: 'desc' }],
                        },
                        pagination: { paginationModel: { pageSize: 20 } },
                    }}
                    slots={{ toolbar: GridToolbar }}
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
        ? <Box sx={{ width: '100%' }}>
            <LinearProgress />
        </Box>
        : renderStatisticsTable(statistics);

    return (
        // <Box display="flex">
        <div style={{ display: 'flex', flexFlow: 'column' }}>
            <div style={{ flexGrow: 1 }}>
                <TraversingHeapModeSelect mode={mode} onChange={(mode) => setMode(mode)} />
                <GenerationSelect generation={generation} onChange={(generation) => setGeneration(generation)} />
            </div>
            {contents}
        </div >
        // </Box>
    );
}