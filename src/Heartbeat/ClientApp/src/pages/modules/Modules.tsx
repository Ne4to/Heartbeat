import React, {useEffect} from 'react';
import {DataGrid, GridColDef} from '@mui/x-data-grid';
import {Stack} from "@mui/material";

import getClient from '../../lib/getClient'
import {Module} from '../../client/models';

import {useNotifyError} from "../../hooks/useNotifyError";
import {useStateWithLoading} from "../../hooks/useStateWithLoading";

import {addressColumn, sizeColumn} from "../../lib/gridColumns";
import toSizeString from "../../lib/toSizeString";
import fetchData from "../../lib/handleFetchData";

import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {ProgressContainer} from "../../components/ProgressContainer";

const columns: GridColDef[] = [
    addressColumn,
    sizeColumn,
    {
        field: 'name',
        headerName: 'Name',
        minWidth: 200,
        flex: 1,
    }
];

export const Modules = () => {
    const {notify, notifyError} = useNotifyError();

    const [modules, setModules, isLoading, setIsLoading] = useStateWithLoading<Module[]>()

    useEffect(() => {
        const fetchModules = async () => {
            const client = getClient();
            return await client.api.dump.modules.get()
        }

        fetchData(fetchModules, setModules, setIsLoading, notifyError);
    }, [notify]);

    const renderTable = (modules: Module[]) => {
        return (
            <DataGrid
                rows={modules}
                getRowId={(row) => row.address}
                columns={columns}
                rowHeight={25}
                pageSizeOptions={[20, 50, 100]}
                density='compact'
                initialState={{
                    sorting: {
                        sortModel: [{field: 'size', sort: 'desc'}],
                    },
                    pagination: {paginationModel: {pageSize: 50}},
                }}
            />
        )
    }

    const getChildrenContent = (modules?: Module[]) => {
        if (!modules || modules.length === 0)
            return undefined;

        const totalSize = modules.map(m => m.size!).reduce((sum, current) => sum + current, 0)

        const propertyRows: PropertyRow[] = [
            {title: 'Count', value: String(modules.length)},
            {title: 'Total size', value: toSizeString(totalSize)},
        ]

        return <Stack>
            <PropertiesTable rows={propertyRows}/>
            {renderTable(modules)}
        </Stack>
    }

    return (
        <ProgressContainer isLoading={isLoading}>
            {getChildrenContent(modules)}
        </ProgressContainer>
    );
}