import React, { Component } from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import { DataGrid, GridColDef, GridRowIdGetter, GridValueGetterParams } from '@mui/x-data-grid';
import { TraversingHeapModeSelect } from './TraversingHeapModeSelect'
import Box from '@mui/material/Box';

type Size = {
    bytes: number;
}

type TypeStatistics = {
    instanceCount: number;
    totalSize: Size;
    typeName: string;
}

type InstanceTypeStatisticsState = {
    loading: boolean;
    statistics: TypeStatistics[];
}

const columns: GridColDef[] = [
    { field: 'instanceCount', headerName: 'Count', type: 'number', width: 130 },
    {
        field: 'totalSize.bytes',
        headerName: 'Total size',
        type: 'number',
        width: 130,
        valueGetter: (params: GridValueGetterParams) => params.row.totalSize.bytes
    },
    { field: 'typeName', headerName: 'Type', minWidth: 200, flex: 1 }
];

export class InstanceTypeStatistics extends Component<{}, InstanceTypeStatisticsState> {
    constructor(props: {}) {
        super(props);
        this.state = {
            loading: true,
            statistics: []
        };
    }

    async componentDidMount() {
        await this.populateStatistics();
    }

    static renderStatisticsTable(statistics: TypeStatistics[]) {
        return (
            <div style={{ flexGrow: 1, height: 700, width: '100%' }}>

                <DataGrid
                    rows={statistics}
                    getRowId={(row) => row.typeName}
                    columns={columns}
                    rowsPerPageOptions={[10, 20, 50, 100]}
                    pagination
                    pageSize={10}
                />

            </div>
        );
    }

    render() {
        let contents = this.state.loading
            ? <Box sx={{ width: '100%' }}>
                <LinearProgress />
            </Box>
            : InstanceTypeStatistics.renderStatisticsTable(this.state.statistics);

        return (
            <div style={{display: 'flex', flexFlow: 'column'}}>
                <h1 id="tableLabel" style={{flexGrow: 1}}>Instance type statistics</h1>
                <div style={{flexGrow: 1}}>
                    <TraversingHeapModeSelect />
                </div>
                {contents}
            </div>
        );
    }

    async populateStatistics() {
        const response = await fetch('api/dump/type-statistics');
        const data: TypeStatistics[] = await response.json();
        this.setState(
            {
                statistics: data,
                loading: false
            });
    }
}