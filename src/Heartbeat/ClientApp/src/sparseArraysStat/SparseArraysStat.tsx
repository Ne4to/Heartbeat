import React, {useEffect} from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import {
    Generation,
    ObjectGCStatus,
    SparseArrayStatistics,
} from '../client/models';
import {PropertiesTable, PropertyRow} from "../components/PropertiesTable";
import {ObjectGCStatusSelect} from "../components/ObjectGCStatusSelect";
import {GenerationSelect} from "../components/GenerationSelect";
import {methodTableColumn, sizeColumn} from "../lib/gridColumns";
import toSizeString from "../lib/toSizeString";

const columns: GridColDef[] = [
    methodTableColumn,
    {
        field: 'count',
        headerName: 'Count',
        type: 'number',
        align: 'right'
    },
    {
        ...sizeColumn,
        field: 'totalWasted',
        headerName: 'Total wasted'
    },
    {
        field: "typeName",
        headerName: 'Type',
        flex: 1
    },
];

export const SparseArraysStat = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [gcStatus, setGcStatus] = React.useState<ObjectGCStatus>()
    const [generation, setGeneration] = React.useState<Generation>()
    const [arrays, setArrays] = React.useState<SparseArrayStatistics[]>([])

    useEffect(() => {
        loadData(gcStatus, generation).catch(console.error);
    }, [gcStatus, generation]);

    const loadData = async (gcStatus?: ObjectGCStatus, generation?: Generation) => {
        const client = getClient();
        const result = await client.api.dump.arrays.sparse.stat.get(
            {queryParameters: {gcStatus: gcStatus, generation: generation}}
        )
        setArrays(result!)
        setLoading(false)
    }

    const renderTable = (arrays: SparseArrayStatistics[]) => {
        return (
            <div style={{flexGrow: 1, width: '100%'}}>

                <DataGrid
                    rows={arrays}
                    getRowId={(row) => row.methodTable}
                    columns={columns}
                    rowHeight={25}
                    pageSizeOptions={[20, 50, 100]}
                    density='compact'
                    initialState={{
                        sorting: {
                            sortModel: [{field: 'totalWasted', sort: 'desc'}],
                        },
                        pagination: {paginationModel: {pageSize: 20}},
                    }}
                    slots={{toolbar: GridToolbar}}
                    slotProps={{
                        toolbar: {
                            showQuickFilter: true,
                        },
                    }}
                />

            </div>
        );
    }

    let contents = loading
        ? <Box sx={{width: '100%'}}>
            <LinearProgress/>
        </Box>
        : renderTable(arrays);

    const totalWasted = arrays.map(m => m.totalWasted!).reduce((sum, current) => sum + current, 0)

    const propertyRows: PropertyRow[] = [
        {title: 'Count', value: String(arrays.length)},
        {title: 'Total wasted', value: toSizeString(totalWasted)},
    ]

    return (
        <div style={{display: 'flex', flexFlow: 'column'}}>
            <div style={{flexGrow: 1}}>
                <ObjectGCStatusSelect gcStatus={gcStatus} onChange={(status) => setGcStatus(status)}/>
                <GenerationSelect generation={generation} onChange={(generation) => setGeneration(generation)}/>
            </div>
            <PropertiesTable rows={propertyRows}/>
            {contents}
        </div>
    );
}