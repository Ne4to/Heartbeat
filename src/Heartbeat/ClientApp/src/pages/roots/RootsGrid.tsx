import React, {useEffect} from 'react';
import {DataGrid, GridColDef} from '@mui/x-data-grid';
import {Stack} from "@mui/material";

import {useNotifyError} from "../../hooks/useNotifyError";
import {useStateWithLoading} from "../../hooks/useStateWithLoading";

import {ClrRootKind, RootInfo,} from '../../client/models';

import getClient from '../../lib/getClient'
import fetchData from "../../lib/handleFetchData";
import {methodTableColumn, objectAddressColumn, sizeColumn} from "../../lib/gridColumns";

import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {RootKindSelect} from "../../components/RootKindSelect";
import {ProgressContainer} from "../../components/ProgressContainer";

const columns: GridColDef[] = [
    objectAddressColumn,
    {
        field: 'isPinned',
        headerName: 'Pinned'
    },
    {
        field: 'kind',
        headerName: 'Kind',
        width: 150
    },
    sizeColumn,
    methodTableColumn,
    {
        field: 'typeName',
        headerName: 'Type',
        minWidth: 200,
        flex: 1,
    }
];

export const RootsGrid = () => {
    const {notify, notifyError} = useNotifyError();

    const [rootKind, setRootKind] = React.useState<ClrRootKind>()
    const [roots, setRoots, isLoading, setIsLoading] = useStateWithLoading<RootInfo[]>()

    useEffect(() => {
        const fetchRoots = async () => {
            const client = getClient();
            return await client.api.dump.roots.get(
                {queryParameters: {kind: rootKind}}
            )
        }

        fetchData(fetchRoots, setRoots, setIsLoading, notifyError);
    }, [rootKind, notify]);

    const renderTable = (roots: RootInfo[]) => {
        return (
            <DataGrid
                rows={roots}
                getRowId={(row) => row.address}
                columns={columns}
                rowHeight={25}
                pageSizeOptions={[20, 50, 100]}
                density='compact'
                initialState={{
                    pagination: {paginationModel: {pageSize: 20}},
                }}
            />
        );
    }

    const getChildrenContent = (roots?: RootInfo[]) => {
        if (!roots || roots.length === 0)
            return undefined;

        const propertyRows: PropertyRow[] = [
            {title: 'Count', value: String(roots.length)},
        ]

        return <Stack>
            <PropertiesTable rows={propertyRows}/>
            {renderTable(roots)}
        </Stack>
    }

    return (
        <Stack>
            <RootKindSelect kind={rootKind} onChange={(kind) => setRootKind(kind)}/>
            <ProgressContainer isLoading={isLoading}>
                {getChildrenContent(roots)}
            </ProgressContainer>
        </Stack>
    );
}