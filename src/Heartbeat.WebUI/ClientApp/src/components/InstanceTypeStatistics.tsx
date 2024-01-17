import React, { Component, useEffect } from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import { DataGrid, GridColDef, GridRenderCellParams, GridValueFormatterParams, GridValueGetterParams } from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import { TraversingHeapModeSelect } from './TraversingHeapModeSelect'

import getClient from '../lib/getClient'
import { ObjectTypeStatistics, TraversingHeapModes, TraversingHeapModesObject } from '../client/models';
import prettyBytes from 'pretty-bytes';

const columns: GridColDef[] = [
    { field: 'instanceCount', headerName: 'Count', type: 'number', width: 130 },
    {
        field: 'totalSize',
        headerName: 'Total size',
        type: 'number',
        width: 130,
        valueGetter: (params: GridValueGetterParams) => params.row.totalSize,
        valueFormatter: (params: GridValueFormatterParams<number>) => {
            if (params.value == null) {
                return '';
            }
            return prettyBytes(params.value);
        }
    },
    {
        field: 'typeName',
        headerName: 'Type',
        minWidth: 200,
        flex: 1,
        renderCell: (params: GridRenderCellParams<any, any>) => {
            const mt = params.row.methodTable;
            const mtHex = mt.toString(16)
            return (
                <a href={'/object-instances?mt=' + mtHex}>{params.value}</a>
            )
        }
    }
];

export const InstanceTypeStatistics = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [mode, setMode] = React.useState<TraversingHeapModes>(TraversingHeapModesObject.All)
    const [statistics, setStatistics] = React.useState<ObjectTypeStatistics[]>([])

    useEffect(() => {
        populateStatistics(mode);
    }, [mode]);

    const populateStatistics = async (mode: TraversingHeapModes) => {
        const client = getClient();

        try {
            var stats = await client.api.dump.typeStatistics.get({ queryParameters: { traversingMode: mode } });
            setStatistics(stats!)
            setLoading(false)
        } catch (error) {
            console.log('ERROR!!!!!!!!!!!')
            console.log(error)
        }
    }

    const renderStatisticsTable = (statistics: ObjectTypeStatistics[]) => {
        return (
            <div style={{ flexGrow: 1, height: 700, width: '100%' }}>

                <DataGrid
                    rows={statistics}
                    getRowId={(row) => row.typeName}
                    columns={columns}
                    rowHeight={25}
                    rowsPerPageOptions={[20, 50, 100]}
                    pagination
                    pageSize={20}
                    initialState={{
                        sorting: {
                            sortModel: [{ field: 'totalSize', sort: 'desc' }],
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
        <div style={{ display: 'flex', flexFlow: 'column' }}>
            <h4 id="tableLabel" style={{ flexGrow: 1 }}>Heap dump</h4>
            <div style={{ flexGrow: 1 }}>
                <TraversingHeapModeSelect mode={mode} onChange={(mode) => setMode(mode)} />
                {/* TODO filter by generation */}
            </div>
            {contents}
        </div>
    );
}