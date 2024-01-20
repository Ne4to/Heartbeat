import React, {useEffect} from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import {DataGrid, GridColDef} from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import {formatAddress, formatSize} from '../lib/gridFormatter';
import prettyBytes from 'pretty-bytes';
import {Module} from '../client/models';
import {PropertiesTable, PropertyRow} from "../components/PropertiesTable";
import {renderAddress} from "../lib/gridRenderCell";
import {addressColumn, sizeColumn} from "../lib/gridColumns";

const columns: GridColDef[] = [
    addressColumn,
    sizeColumn,
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
            <div style={{flexGrow: 1, width: '100%'}}>

                <DataGrid
                    rows={modules}
                    getRowId={(row) => row.address}
                    columns={columns}
                    rowHeight={25}
                    pageSizeOptions={[20, 50, 100]}
                    density='compact'
                    initialState={{
                        sorting: {
                            sortModel: [{field: 'size', sort: 'desc'}],
                        },
                        pagination: {paginationModel: {pageSize: 20}},
                    }}
                />

            </div>
        );
    }

    let contents = loading
        ? <Box sx={{width: '100%'}}>
            <LinearProgress/>
        </Box>
        : renderTable(modules);

    const totalSize = modules.map(m => m.size!).reduce((sum, current) => sum + current, 0)

    const propertyRows: PropertyRow[] = [
        {title: 'Count', value: String(modules.length)},
        {title: 'Total size', value: prettyBytes(totalSize)},
    ]

    return (
        <div style={{display: 'flex', flexFlow: 'column'}}>
            <PropertiesTable rows={propertyRows}/>
            {contents}
        </div>
    );
}