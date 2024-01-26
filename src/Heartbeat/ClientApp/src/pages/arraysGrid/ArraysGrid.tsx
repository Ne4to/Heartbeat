import React from 'react';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';

import getClient from '../../lib/getClient'
import {formatPercent} from '../../lib/gridFormatter';
import {
    ArrayInfo,
    Generation,
    ObjectGCStatus,
} from '../../client/models';
import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {ObjectGCStatusSelect} from "../../components/ObjectGCStatusSelect";
import {GenerationSelect} from "../../components/GenerationSelect";
import {methodTableColumn, objectAddressColumn, sizeColumn} from "../../lib/gridColumns";
import toSizeString from "../../lib/toSizeString";
import {Stack} from "@mui/material";
import {ProgressContainer} from "../../components/ProgressContainer";

const columns: GridColDef[] = [
    objectAddressColumn,
    methodTableColumn,
    {
        field: 'length',
        headerName: 'Length',
        type: 'number',
        align: 'right'
    },
    {
        field: 'unusedPercent',
        headerName: 'Unused %',
        align: 'right',
        valueFormatter: formatPercent
    },
    {
        ...sizeColumn,
        field: 'wasted',
        headerName: 'Wasted'
    },
    {
        field: "typeName",
        headerName: 'Type',
        flex: 1
    },
];

export const ArraysGrid = () => {
    const [gcStatus, setGcStatus] = React.useState<ObjectGCStatus>()
    const [generation, setGeneration] = React.useState<Generation>()

    const getData = async() => {
        const client = getClient();
        const result = await client.api.dump.arrays.sparse.get(
            {queryParameters: {gcStatus: gcStatus, generation: generation}}
        )
        return result!
    }

    const renderTable = (arrays: ArrayInfo[]) => {
        return (
            <DataGrid
                rows={arrays}
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
        );
    }

    const getChildrenContent = (arrays: ArrayInfo[]) => {
        const totalWasted = arrays.map(m => m.wasted!).reduce((sum, current) => sum + current, 0)

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
            <ProgressContainer loadData={getData} getChildren={getChildrenContent}/>
        </Stack>
    );
}