import React, {useEffect} from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import {DataGrid, GridColDef} from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import {formatAddress, formatSize} from '../lib/gridFormatter';
import {renderClrObjectAddress, renderMethodTable} from '../lib/gridRenderCell';
import {
    ClrRootKind,
    RootInfo,
} from '../client/models';
import {PropertiesTable, PropertyRow} from "../components/PropertiesTable";
import {RootKindSelect} from "../components/RootKindSelect";

const columns: GridColDef[] = [
    {
        field: 'address',
        headerName: 'Address',
        type: 'number',
        width: 200,
        valueFormatter: formatAddress,
        renderCell: renderClrObjectAddress
    },
    {
        field: 'isPinned',
        headerName: 'Pinned'
    },
    {
        field: 'kind',
        headerName: 'Kind',
        width: 150
    },
    {
        field: 'size',
        headerName: 'Size',
        valueFormatter: formatSize
    },
    {
        field: 'methodTable',
        headerName: 'MethodTable',
        type: 'number',
        width: 200,
        valueFormatter: formatAddress,
        renderCell: renderMethodTable
    },
    {
        field: 'typeName',
        headerName: 'Type',
        minWidth: 200,
        flex: 1,
    }
];

export const RootsGrid = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [rootKind, setRootKind] = React.useState<ClrRootKind>()
    const [roots, setRoots] = React.useState<RootInfo[]>([])

    useEffect(() => {
        loadData(rootKind).catch(console.error);
    }, [rootKind]);

    const loadData = async (rootKind?: ClrRootKind) => {
        const client = getClient();
        const result = await client.api.dump.roots.get(
            {queryParameters: {kind: rootKind}}
        )
        setRoots(result!)
        setLoading(false)
    }

    const renderTable = (roots: RootInfo[]) => {
        return (
            <div style={{flexGrow: 1, width: '100%'}}>

                <DataGrid
                    rows={roots}
                    getRowId={(row) => row.address}
                    columns={columns}
                    rowHeight={25}
                    pageSizeOptions={[20, 50, 100]}
                    density='compact'
                    initialState={{
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
        : renderTable(roots);

    const propertyRows: PropertyRow[] = [
        {title: 'Count', value: String(roots.length)},
    ]

    return (
        <div style={{display: 'flex', flexFlow: 'column'}}>
            <div style={{flexGrow: 1}}>
                <RootKindSelect kind={rootKind} onChange={(kind) => setRootKind(kind)}/>
            </div>
            <PropertiesTable rows={propertyRows}/>
            {contents}
        </div>
    );
}