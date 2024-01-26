import React, {useEffect} from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import {formatPercent} from '../lib/gridFormatter';
import {ArrayInfo, Generation, TraversingHeapModes, TraversingHeapModesObject} from '../client/models';
import {PropertiesTable, PropertyRow} from "../components/PropertiesTable";
import {TraversingHeapModeSelect} from "../components/TraversingHeapModeSelect";
import {GenerationSelect} from "../components/GenerationSelect";
import {methodTableColumn, objectAddressColumn, sizeColumn} from "../lib/gridColumns";
import toSizeString from "../lib/toSizeString";

const columns: GridColDef[] = [
    objectAddressColumn,
    methodTableColumn,
    {
        field: 'length',
        headerName: 'Length',
        type: 'number',
        align: 'right'
    },
    {
        field: 'unusedPercent',
        headerName: 'Unused %',
        align: 'right',
        valueFormatter: formatPercent
    },
    {
        ...sizeColumn,
        field: 'wasted',
        headerName: 'Wasted'
    },
    {
        field: "typeName",
        headerName: 'Type',
        flex: 1
    },
];

export const ArraysGrid = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [mode, setMode] = React.useState<TraversingHeapModes>(TraversingHeapModesObject.All)
    const [generation, setGeneration] = React.useState<Generation>()
    const [arrays, setArrays] = React.useState<ArrayInfo[]>([])

    useEffect(() => {
        loadData(mode, generation).catch(console.error);
    }, [mode, generation]);

    const loadData = async (mode: TraversingHeapModes, generation?: Generation) => {
        const client = getClient();
        const result = await client.api.dump.arrays.sparse.get(
            {queryParameters: {traversingMode: mode, generation: generation}}
        )
        setArrays(result!)
        setLoading(false)
    }

    const renderTable = (arrays: ArrayInfo[]) => {
        return (
            <DataGrid
                rows={arrays}
                getRowId={(row) => row.address}
                columns={columns}
                rowHeight={25}
                pageSizeOptions={[20, 50, 100]}
                density='compact'
                initialState={{
                    sorting: {
                        sortModel: [{field: 'length', sort: 'desc'}],
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
        );
    }

    let contents = loading
        ? <Box sx={{width: '100%'}}>
            <LinearProgress/>
        </Box>
        : renderTable(arrays);

    const totalWasted = arrays.map(m => m.wasted!).reduce((sum, current) => sum + current, 0)

    const propertyRows: PropertyRow[] = [
        {title: 'Count', value: String(arrays.length)},
        {title: 'Total wasted', value: toSizeString(totalWasted)},
    ]

    return (
        <div style={{display: 'flex', flexFlow: 'column'}}>
            <div style={{flexGrow: 1}}>
                <TraversingHeapModeSelect mode={mode} onChange={(mode) => setMode(mode)}/>
                <GenerationSelect generation={generation} onChange={(generation) => setGeneration(generation)}/>
            </div>
            <PropertiesTable rows={propertyRows}/>
            {contents}
        </div>
    );
}