import React from 'react';
import {DataGrid, GridColDef} from '@mui/x-data-grid';

import getClient from '../../lib/getClient'
import {Module} from '../../client/models';
import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {addressColumn, sizeColumn} from "../../lib/gridColumns";
import toSizeString from "../../lib/toSizeString";
import {Stack} from "@mui/material";
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
    const getData = async() => {
        const client = getClient();
        const result = await client.api.dump.modules.get()
        return result!
    }

    const renderTable = (modules: Module[]) => {
        return (
            <div style={{flexGrow: 1, width: '100%'}}>

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

            </div>
        );
    }

    const getChildrenContent = (modules: Module[]) => {
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
        <ProgressContainer loadData={getData} getChildren={getChildrenContent}/>
    );
}