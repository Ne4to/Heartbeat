import React from 'react';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';

import {ClrObjectRootPath, RootPathItem} from '../client/models';
import {PropertiesTable, PropertyRow} from './PropertiesTable'
import {methodTableColumn, objectAddressColumn, sizeColumn} from "../lib/gridColumns";

const columns: GridColDef[] = [
    methodTableColumn,
    objectAddressColumn,
    {
        field: 'generation',
        headerName: 'Generation'
    },
    sizeColumn,
    {
        field: 'typeName',
        headerName: 'Type',
        minWidth: 200,
        flex: 1,
    }
];

export type ClrObjectRootProps = {
    rootPath: ClrObjectRootPath
}

export const ClrObjectRoot = (props: ClrObjectRootProps) => {
    const rootPath = props.rootPath;

    const renderTable = (pathItems: RootPathItem[]) => {
        return (
            <DataGrid
                rows={pathItems}
                getRowId={(row) => row.address}
                columns={columns}
                rowHeight={25}
                pageSizeOptions={[20, 50, 100]}
                density='compact'
                slots={{toolbar: GridToolbar}}
                initialState={{
                    pagination: {paginationModel: {pageSize: 20}},
                }}
            />
        );
    }

    const grid = renderTable(rootPath.pathItems!);
    const root = rootPath.root;
    const propertyRows: PropertyRow[] = [
        // {title: 'Address', value: toHexAddress(root?.address)},
        // {title: 'Size', value: toSizeString(root?.size || 0)},
        {title: 'Kind', value: root?.kind},
        {title: 'Pinned', value: String(root?.isPinned)},
        // {title: 'MethodTable', value: renderMethodTableLink(root?.methodTable)},
        // {title: 'Type', value: root?.typeName},
    ]

    return (
        <div style={{display: 'flex', flexFlow: 'column'}}>
            <h5 id="tableLabel" style={{flexGrow: 1}}>Clr Object root</h5>
            <PropertiesTable rows={propertyRows}/>
            {grid}
        </div>
    );
}