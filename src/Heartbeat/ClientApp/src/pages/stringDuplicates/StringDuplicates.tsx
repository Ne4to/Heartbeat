import React, {useEffect} from 'react';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';
import {Stack} from "@mui/material";

import {Generation, ObjectGCStatus, StringDuplicate,} from '../../client/models';

import {useNotifyError} from "../../hooks/useNotifyError";
import {useStateWithLoading} from "../../hooks/useStateWithLoading";

import getClient from '../../lib/getClient'
import {sizeColumn} from "../../lib/gridColumns";
import toSizeString from "../../lib/toSizeString";
import handleFetchData from "../../lib/handleFetchData";

import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {ObjectGCStatusSelect} from "../../components/ObjectGCStatusSelect";
import {GenerationSelect} from "../../components/GenerationSelect";
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
    const {notify, notifyError} = useNotifyError();

    const [gcStatus, setGcStatus] = React.useState<ObjectGCStatus>()
    const [generation, setGeneration] = React.useState<Generation>()
    const [strings, setStrings, isLoading, setIsLoading] = useStateWithLoading<StringDuplicate[]>()

    useEffect(() => {
        const fetchStrings = async () => {
            const client = getClient();
            return await client.api.dump.stringDuplicates.get(
                {queryParameters: {gcStatus: gcStatus, generation: generation}}
            )
        }

        handleFetchData(fetchStrings, setStrings, setIsLoading, notifyError);
    }, [gcStatus, generation, notify]);

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

    const getChildrenContent = (duplicates?: StringDuplicate[]) => {
        if (!duplicates || duplicates.length === 0)
            return undefined;

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
            <ProgressContainer isLoading={isLoading}>
                {getChildrenContent(strings)}
            </ProgressContainer>
        </Stack>
    );
}