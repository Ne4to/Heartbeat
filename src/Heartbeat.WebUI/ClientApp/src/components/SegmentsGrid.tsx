import React, { useEffect } from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import { DataGrid, GridColDef, GridValueFormatterParams } from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import toHexAddress from '../lib/toHexAddress'
import prettyBytes from 'pretty-bytes';
import { HeapSegment } from '../client/models';

const columns: GridColDef[] = [
    {
        field: 'start',
        headerName: 'Start',
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
        field: 'end',
        headerName: 'End',
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
            <div style={{ flexGrow: 1, height: 380, width: '100%' }}>

                <DataGrid
                    rows={segments}
                    getRowId={(row) => row.start}
                    columns={columns}
                    rowHeight={25}
                    rowsPerPageOptions={[10, 20, 50, 100]}
                    pagination
                    pageSize={10}
                    initialState={{
                        sorting: {
                            sortModel: [{ field: 'start', sort: 'asc' }],
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