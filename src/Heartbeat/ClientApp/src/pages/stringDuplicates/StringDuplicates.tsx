import React from 'react';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';

import getClient from '../../lib/getClient'
import {
    Generation,
    ObjectGCStatus,
    StringDuplicate,
} from '../../client/models';
import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {ObjectGCStatusSelect} from "../../components/ObjectGCStatusSelect";
import {GenerationSelect} from "../../components/GenerationSelect";
import {sizeColumn} from "../../lib/gridColumns";
import toSizeString from "../../lib/toSizeString";
import {Stack} from "@mui/material";
import {ProgressContainer} from "../../components/ProgressContainer";

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
    const [gcStatus, setGcStatus] = React.useState<ObjectGCStatus>()
    const [generation, setGeneration] = React.useState<Generation>()

    const getData = async () => {
        const client = getClient();
        const result = await client.api.dump.stringDuplicates.get(
            {queryParameters: {gcStatus: gcStatus, generation: generation}}
        )
        return result!
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

    const getChildrenContent = (duplicates: StringDuplicate[]) => {
        const totalWasted = duplicates.map(m => m.wastedMemory!).reduce((sum, current) => sum + current, 0)

        const propertyRows: PropertyRow[] = [
            {title: 'Count', value: String(duplicates.length)},
            {title: 'Total wasted', value: toSizeString(totalWasted)},
        ]

        return <Stack>
            <PropertiesTable rows={propertyRows}/>
            {renderTable(duplicates)}
        </Stack>
    }

    return (
        <Stack>
            <Stack direction="row">
                <ObjectGCStatusSelect gcStatus={gcStatus} onChange={(status) => setGcStatus(status)}/>
                <GenerationSelect generation={generation} onChange={(generation) => setGeneration(generation)}/>
            </Stack>
            <ProgressContainer loadData={getData} getChildren={getChildrenContent}/>
        </Stack>
    );
}