import React, {useEffect} from 'react';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';

import {Stack} from "@mui/material";

import {Generation, ObjectGCStatus, SparseArrayStatistics,} from '../../client/models';

import {useStateWithLoading} from "../../hooks/useStateWithLoading";
import {useNotifyError} from "../../hooks/useNotifyError";

import getClient from '../../lib/getClient'
import toSizeString from "../../lib/toSizeString";
import {methodTableColumn, sizeColumn} from "../../lib/gridColumns";
import fetchData from "../../lib/handleFetchData";

import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {ObjectGCStatusSelect} from "../../components/ObjectGCStatusSelect";
import {GenerationSelect} from "../../components/GenerationSelect";
import {ProgressContainer} from "../../components/ProgressContainer";

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
    const {notify, notifyError} = useNotifyError();

    const [gcStatus, setGcStatus] = React.useState<ObjectGCStatus>()
    const [generation, setGeneration] = React.useState<Generation>()
    const [arrays, setArrays, isLoading, setIsLoading] = useStateWithLoading<SparseArrayStatistics[]>()

    useEffect(() => {
        const fetchArrays = async () => {
            const client = getClient();
            return await client.api.dump.arrays.sparse.stat.get(
                {queryParameters: {gcStatus: gcStatus, generation: generation}}
            )
        }

        fetchData(fetchArrays, setArrays, setIsLoading, notifyError);
    }, [gcStatus, generation, notify]);

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

    const getChildrenContent = (arrays?: SparseArrayStatistics[]) => {
        if (!arrays || arrays.length === 0)
            return undefined;

        const totalWasted = arrays.map(m => m.totalWasted!).reduce((sum, current) => sum + current, 0)

        const propertyRows: PropertyRow[] = [
            {title: 'Count', value: String(arrays.length)},
            {title: 'Total wasted', value: toSizeString(totalWasted)},
        ]

        return <Stack>
            <PropertiesTable rows={propertyRows}/>
            {renderTable(arrays)}
        </Stack>
    }

    return (
        <Stack>
            <Stack direction="row">
                <ObjectGCStatusSelect gcStatus={gcStatus} onChange={(status) => setGcStatus(status)}/>
                <GenerationSelect generation={generation} onChange={(generation) => setGeneration(generation)}/>
            </Stack>
            <ProgressContainer isLoading={isLoading}>
                {getChildrenContent(arrays)}
            </ProgressContainer>
        </Stack>
    );
}