import React, {useEffect} from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import {formatAddress, formatSize} from '../lib/gridFormatter';
import {renderClrObjectAddress} from '../lib/gridRenderCell';
import prettyBytes from 'pretty-bytes';
import {
    Generation,
    StringDuplicate,
    StringInfo,
    TraversingHeapModes,
    TraversingHeapModesObject
} from '../client/models';
import {PropertiesTable, PropertyRow} from "../components/PropertiesTable";
import {TraversingHeapModeSelect} from "../components/TraversingHeapModeSelect";
import {GenerationSelect} from "../components/GenerationSelect";

const columns: GridColDef[] = [
    {
        field: 'count',
        headerName: 'Count',
        align: 'right',
    },
    {
        field: 'fullLength',
        headerName: 'Length',
        align: 'right',
    },
    {
        field: 'wastedMemory',
        headerName: 'Wasted',
        align: 'right',
        valueFormatter: formatSize
    },
    {
        field: 'value',
        headerName: 'Value',
        minWidth: 200,
        flex: 1,
    }
];

export const StringDuplicates = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [mode, setMode] = React.useState<TraversingHeapModes>(TraversingHeapModesObject.All)
    const [generation, setGeneration] = React.useState<Generation>()
    const [duplicates, setDuplicates] = React.useState<StringDuplicate[]>([])

    useEffect(() => {
        loadData(mode, generation).catch(console.error);
    }, [mode, generation]);

    const loadData = async (mode: TraversingHeapModes, generation?: Generation) => {
        const client = getClient();
        const result = await client.api.dump.stringDuplicates.get(
            {queryParameters: {traversingMode: mode, generation: generation}}
        )
        setDuplicates(result!)
        setLoading(false)
    }

    const renderTable = (duplicates: StringDuplicate[]) => {
        return (
            <div style={{flexGrow: 1, width: '100%'}}>

                <DataGrid
                    rows={duplicates}
                    getRowId={(row) => row.value}
                    columns={columns}
                    rowHeight={25}
                    pageSizeOptions={[20, 50, 100]}
                    density='compact'
                    initialState={{
                        sorting: {
                            sortModel: [{field: 'wastedMemory', sort: 'desc'}],
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
        : renderTable(duplicates);

    const totalWasted = duplicates.map(m => m.wastedMemory!).reduce((sum, current) => sum + current, 0)

    const propertyRows: PropertyRow[] = [
        {title: 'Count', value: String(duplicates.length)},
        {title: 'Total wasted', value: prettyBytes(totalWasted)},
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