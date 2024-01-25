import React, { useEffect } from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import { HeapSegment } from '../client/models';
import {PropertiesTable, PropertyRow} from "../components/PropertiesTable";
import {addressColumn, sizeColumn} from "../lib/gridColumns";
import toSizeString from "../lib/toSizeString";

const columns: GridColDef[] = [
    {
        ...addressColumn,
        field: 'start',
        headerName: 'Start',
    },
    {
        ...addressColumn,
        field: 'end',
        headerName: 'End',
    },
    sizeColumn,
    {
        field: 'kind',
        headerName: 'Kind',
        minWidth: 200,
        flex: 1,
    }
];

export const SegmentsGrid = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [segments, setSegments] = React.useState<HeapSegment[]>([])

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        const client = getClient();
        const result = await client.api.dump.segments.get()
        setSegments(result!)
        setLoading(false)
    }

    const renderTable = (segments: HeapSegment[]) => {
        return (
            <div style={{ flexGrow: 1, width: '100%' }}>

                <DataGrid
                    rows={segments}
                    getRowId={(row) => row.start}
                    columns={columns}
                    rowHeight={25}
                    density='compact'
                    pageSizeOptions={[20, 50, 100]}
                    pagination
                    // pageSize={10}
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
        : renderTable(segments);

    const totalSize = segments.map(m => m.size!).reduce((sum, current) => sum + current, 0)

    const propertyRows: PropertyRow[] = [
        {title: 'Total size', value: toSizeString(totalSize)},
    ]

    return (
        <div style={{ display: 'flex', flexFlow: 'column' }}>
            <PropertiesTable rows={propertyRows}/>
            {contents}
        </div>
    );
}