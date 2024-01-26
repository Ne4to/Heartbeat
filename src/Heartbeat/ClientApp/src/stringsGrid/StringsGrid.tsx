import React, {useEffect} from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import {Generation, ObjectGCStatus, StringInfo} from '../client/models';
import {PropertiesTable, PropertyRow} from "../components/PropertiesTable";
import {ObjectGCStatusSelect} from "../components/ObjectGCStatusSelect";
import {GenerationSelect} from "../components/GenerationSelect";
import {objectAddressColumn, sizeColumn} from "../lib/gridColumns";
import toSizeString from "../lib/toSizeString";

const columns: GridColDef[] = [
    objectAddressColumn,
    {
        field: 'length',
        headerName: 'Length',
        type: 'number',
        align: 'right'
    },
    sizeColumn,
    {
        field: 'value',
        headerName: 'Value',
        minWidth: 200,
        flex: 1,
    }
];

export const StringsGrid = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [gcStatus, setGcStatus] = React.useState<ObjectGCStatus>()
    const [generation, setGeneration] = React.useState<Generation>()
    const [strings, setStrings] = React.useState<StringInfo[]>([])

    useEffect(() => {
        loadData(gcStatus, generation).catch(console.error);
    }, [gcStatus, generation]);

    const loadData = async (gcStatus?: ObjectGCStatus, generation?: Generation) => {
        const client = getClient();
        const result = await client.api.dump.strings.get(
            {queryParameters: {gcStatus: gcStatus, generation: generation}}
        )
        setStrings(result!)
        setLoading(false)
    }

    const renderTable = (strings: StringInfo[]) => {
        return (
            <div style={{flexGrow: 1, width: '100%'}}>

                <DataGrid
                    rows={strings}
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

            </div>
        );
    }

    let contents = loading
        ? <Box sx={{width: '100%'}}>
            <LinearProgress/>
        </Box>
        : renderTable(strings);

    const totalSize = strings.map(m => m.size!).reduce((sum, current) => sum + current, 0)

    const propertyRows: PropertyRow[] = [
        {title: 'Count', value: String(strings.length)},
        {title: 'Total size', value: toSizeString(totalSize)},
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