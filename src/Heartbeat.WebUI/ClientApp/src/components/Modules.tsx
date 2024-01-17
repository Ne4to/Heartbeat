import React, { useEffect } from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import { formatAddress, formatSize } from '../lib/gridFormatter';
import prettyBytes from 'pretty-bytes';
import { Module } from '../client/models';

const columns: GridColDef[] = [
    {
        field: 'address',
        headerName: 'Address',
        type: 'number',
        width: 200,
        valueFormatter: formatAddress
    },
    {
        field: 'size',
        headerName: 'Size',
        valueFormatter: formatSize
    },
    {
        field: 'name',
        headerName: 'Name',
        minWidth: 200,
        flex: 1,
    }
];

export const Modules = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [modules, setModules] = React.useState<Module[]>([])

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        const client = getClient();
        const result = await client.api.dump.modules.get()
        setModules(result!)
        setLoading(false)
    }

    const renderTable = (modules: Module[]) => {
        return (
            <div style={{ flexGrow: 1, width: '100%' }}>

                <DataGrid
                    rows={modules}
                    getRowId={(row) => row.address}
                    columns={columns}
                    rowHeight={25}
                    pageSizeOptions={[20, 50, 100]}
                    density='compact'
                    initialState={{
                        sorting: {
                            sortModel: [{ field: 'size', sort: 'desc' }],
                        },
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
        : renderTable(modules);

    const totalSize = modules.map(m => m.size!).reduce((sum, current) => sum + current, 0)

    return (
        <div style={{ display: 'flex', flexFlow: 'column' }}>
            <h4 id="tableLabel" style={{ flexGrow: 1 }}>Modules</h4>
            <ul>
                <li>Count: {modules.length}</li>
                <li>Total size: {prettyBytes(totalSize)}</li>
            </ul>
            {contents}
        </div>
    );
}