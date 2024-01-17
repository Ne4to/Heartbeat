import React, { useEffect } from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import { DataGrid, GridColDef, GridRenderCellParams, GridValueFormatterParams, GridValueGetterParams } from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import toHexAddress from '../lib/toHexAddress'
import toHexString from '../lib/toHexString'
import prettyBytes from 'pretty-bytes';
import { GetClrObjectResult, ClrObjectField, Module } from '../client/models';

const columns: GridColDef[] = [
    {
        field: 'address',
        headerName: 'Address',
        type: 'number',
        width: 200,
        valueFormatter: (params: GridValueFormatterParams<number>) => {
            if (params.value == null) {
                return '';
            }
            return toHexAddress(params.value);
        }
    },
    {
        field: 'size',
        headerName: 'Size',
        valueFormatter: (params: GridValueFormatterParams<number>) => {
            if (params.value == null) {
                return '';
            }
            return prettyBytes(params.value);
        }
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
            <div style={{ flexGrow: 1, height: 700, width: '100%' }}>

                <DataGrid
                    rows={modules}
                    getRowId={(row) => row.address}
                    columns={columns}
                    rowHeight={25}
                    initialState={{
                        sorting: {
                            sortModel: [{ field: 'size', sort: 'desc' }],
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