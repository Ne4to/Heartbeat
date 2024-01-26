import React, {useEffect} from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import {
    Generation,
    ObjectGCStatus,
    StringDuplicate,
} from '../client/models';
import {PropertiesTable, PropertyRow} from "../components/PropertiesTable";
import {ObjectGCStatusSelect} from "../components/ObjectGCStatusSelect";
import {GenerationSelect} from "../components/GenerationSelect";
import {sizeColumn} from "../lib/gridColumns";
import toSizeString from "../lib/toSizeString";

const columns: GridColDef[] = [
    {
        field: 'count',
        headerName: 'Count',
        type: 'number',
        align: 'right',
    },
    {
        field: 'fullLength',
        headerName: 'Length',
        type: 'number',
        align: 'right',
    },
    {
        ...sizeColumn,
        field: 'wastedMemory',
        headerName: 'Wasted',
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
    const [gcStatus, setGcStatus] = React.useState<ObjectGCStatus>()
    const [generation, setGeneration] = React.useState<Generation>()
    const [duplicates, setDuplicates] = React.useState<StringDuplicate[]>([])

    useEffect(() => {
        loadData(gcStatus, generation).catch(console.error);
    }, [gcStatus, generation]);

    const loadData = async (gcStatus?: ObjectGCStatus, generation?: Generation) => {
        const client = getClient();
        const result = await client.api.dump.stringDuplicates.get(
            {queryParameters: {gcStatus: gcStatus, generation: generation}}
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