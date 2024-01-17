import React, { Component, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import LinearProgress from '@mui/material/LinearProgress';
import { DataGrid, GridColDef, GridRenderCellParams, GridValueFormatterParams, GridValueGetterParams } from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import { TraversingHeapModeSelect } from './TraversingHeapModeSelect'

import getClient from '../lib/getClient'
import toHexAddress from '../lib/toHexAddress'
import prettyBytes from 'pretty-bytes';
import { GetObjectInstancesResult, ObjectInstance, TraversingHeapModes, TraversingHeapModesObject } from '../client/models';

const columns: GridColDef[] = [
    {
        field: 'address',
        headerName: 'Address',
        type: 'number',
        width: 200,
        valueGetter: (params: GridValueGetterParams) => params.row.address,
        valueFormatter: (params: GridValueFormatterParams<number>) => {
            if (params.value == null) {
                return '';
            }
            return toHexAddress(params.value);
        },
        renderCell: (params: GridRenderCellParams<any, any>) => {
            const address = toHexAddress(params.value)
            return (
                <a href={'/clr-object?address=' + address}>{address}</a>
            )
        }
    },
    {
        field: 'size',
        headerName: 'Size',
        type: 'number',
        width: 130,
        valueGetter: (params: GridValueGetterParams) => params.row.size,
        valueFormatter: (params: GridValueFormatterParams<number>) => {
            if (params.value == null) {
                return '';
            }
            return prettyBytes(params.value);
        }
    }
];

export const ObjectInstances = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [mode, setMode] = React.useState<TraversingHeapModes>(TraversingHeapModesObject.All)
    const [objectInstancesResult, setObjectInstancesResult] = React.useState<GetObjectInstancesResult>()
    const [searchParams] = useSearchParams();
    const [mt, setMt] = React.useState(Number('0x' + searchParams.get('mt')))

    console.log('MT = ' + mt)

    useEffect(() => {
        loadData(mode);
    }, [mode]);

    const loadData = async (mode: TraversingHeapModes) => {
        const client = getClient();

        const result = await client.api.dump.objectInstances.byMt(mt).get({ queryParameters: { traversingMode: mode } });
        setObjectInstancesResult(result!)
        setLoading(false)
    }

    const renderTable = (instances: ObjectInstance[]) => {
        return (
            <div style={{ flexGrow: 1, height: 700, width: '100%' }}>

                <DataGrid
                    rows={instances}
                    getRowId={(row) => row.address}
                    columns={columns}
                    rowHeight={25}
                    rowsPerPageOptions={[10, 20, 50, 100]}
                    pagination
                    pageSize={10}
                />

            </div>
        );
    }

    let contents = loading
        ? <Box sx={{ width: '100%' }}>
            <LinearProgress />
        </Box>
        : renderTable(objectInstancesResult!.instances!);

    return (
        <div style={{ display: 'flex', flexFlow: 'column' }}>
            <h4 id="tableLabel" style={{ flexGrow: 1 }}>MT {toHexAddress(objectInstancesResult?.methodTable)} - {objectInstancesResult?.typeName}</h4>
            {/* <p>
                @MethodTable @ClrType?.Name
            </p> */}
            <div style={{ flexGrow: 1 }}>
                <TraversingHeapModeSelect mode={mode} onChange={(mode) => setMode(mode)} />
            </div>
            {contents}
        </div>
    );
}