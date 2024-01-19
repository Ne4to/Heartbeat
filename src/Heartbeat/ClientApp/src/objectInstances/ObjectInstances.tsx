import React, { useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import LinearProgress from '@mui/material/LinearProgress';
import { DataGrid, GridColDef, GridRenderCellParams, GridValueGetterParams } from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import { TraversingHeapModeSelect } from '../components/TraversingHeapModeSelect'

import getClient from '../lib/getClient'
import { formatAddress, formatSize } from '../lib/gridFormatter';
import { renderClrObjectAddress } from '../lib/gridRenderCell';
import toHexAddress from '../lib/toHexAddress'
import { GetObjectInstancesResult, ObjectInstance, TraversingHeapModes, TraversingHeapModesObject } from '../client/models';

const columns: GridColDef[] = [
    {
        field: 'address',
        headerName: 'Address',
        type: 'number',
        width: 200,
        valueGetter: (params: GridValueGetterParams) => params.row.address,
        valueFormatter: formatAddress,
        renderCell: renderClrObjectAddress
    },
    {
        field: 'size',
        headerName: 'Size',
        type: 'number',
        width: 130,
        valueFormatter: formatSize
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
            <div style={{ flexGrow: 1, width: '100%' }}>

                <DataGrid
                    rows={instances}
                    getRowId={(row) => row.address}
                    columns={columns}
                    rowHeight={25}
                    density='compact'
                    pageSizeOptions={[20, 50, 100]}
                    pagination
                    initialState={{
                        pagination: { paginationModel: { pageSize: 20 } },
                    }}
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
            <div style={{ flexGrow: 1 }}>
                <TraversingHeapModeSelect mode={mode} onChange={(mode) => setMode(mode)} />
            </div>
            {contents}
        </div>
    );
}