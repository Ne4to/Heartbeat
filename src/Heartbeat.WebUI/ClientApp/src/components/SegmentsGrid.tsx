import React, { useEffect } from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import { formatAddress, formatSize } from '../lib/gridFormatter';
import prettyBytes from 'pretty-bytes';
import { HeapSegment } from '../client/models';

const columns: GridColDef[] = [
    {
        field: 'start',
        headerName: 'Start',
        type: 'number',
        width: 200,
        valueFormatter: formatAddress
    },
    {
        field: 'end',
        headerName: 'End',
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
                            sortModel: [{ field: 'start', sort: 'asc' }],
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

    return (
        <div style={{ display: 'flex', flexFlow: 'column' }}>
            <h4 id="tableLabel" style={{ flexGrow: 1 }}>Segments</h4>
            <ul>
                <li>Total size: {prettyBytes(totalSize)}</li>
            </ul>
            {contents}
        </div>
    );
}