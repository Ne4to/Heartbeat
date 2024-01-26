import React from 'react';
import {DataGrid, GridColDef} from '@mui/x-data-grid';

import getClient from '../../lib/getClient'
import {
    ClrRootKind,
    RootInfo,
} from '../../client/models';
import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {RootKindSelect} from "../../components/RootKindSelect";
import {methodTableColumn, objectAddressColumn, sizeColumn} from "../../lib/gridColumns";
import {Stack} from "@mui/material";
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
    const [rootKind, setRootKind] = React.useState<ClrRootKind>()

    const getData = async () => {
        const client = getClient();
        const result = await client.api.dump.roots.get(
            {queryParameters: {kind: rootKind}}
        )
        return result!
    }

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

    const getChildrenContent = (roots: RootInfo[]) => {
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
            <ProgressContainer loadData={getData} getChildren={getChildrenContent}/>
        </Stack>
    );
}