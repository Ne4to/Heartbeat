import React from 'react';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';

import {ObjectGCStatusSelect} from '../../components/ObjectGCStatusSelect'
import {GenerationSelect} from '../../components/GenerationSelect'

import getClient from '../../lib/getClient'
import {
    Generation,
    ObjectGCStatus,
    ObjectTypeStatistics,
} from '../../client/models';
import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {methodTableColumn, sizeColumn} from "../../lib/gridColumns";
import toSizeString from "../../lib/toSizeString";
import {Stack} from "@mui/material";
import {ProgressContainer} from "../../components/ProgressContainer";

const columns: GridColDef[] = [
    methodTableColumn,
    {
        field: 'instanceCount',
        headerName: 'Count',
        type: 'number',
        width: 100
    },
    {
        ...sizeColumn,
        field: 'totalSize',
        headerName: 'Total size',
        width: 120
    },
    {
        field: 'typeName',
        headerName: 'Type',
        minWidth: 200,
        flex: 1
    }
];

export const HeapDumpStat = () => {
    // TODO save selects state - https://mui.com/x/react-data-grid/state/#save-and-restore-the-state-from-external-storage
    const [gcStatus, setGcStatus] = React.useState<ObjectGCStatus>()
    const [generation, setGeneration] = React.useState<Generation>()

    const renderStatisticsTable = (statistics: ObjectTypeStatistics[]) => {
        /* TODO unify grid settings */
        // TODO save quick filter value (per page)
        return (
            <DataGrid
                rows={statistics}
                getRowId={(row) => row.typeName}
                columns={columns}
                rowHeight={25}
                density='compact'
                pageSizeOptions={[20, 50, 100]}
                pagination
                initialState={{
                    sorting: {
                        sortModel: [{field: 'totalSize', sort: 'desc'}],
                    },
                    pagination: {paginationModel: {pageSize: 50}},
                }}
                slots={{toolbar: GridToolbar}}
                slotProps={{
                    toolbar: {
                        showQuickFilter: true,
                    },
                }}
            />

        )
    }

    const getData = async () => {
        const client = getClient();
        const stats = await client.api.dump.heapDumpStatistics.get(
            {queryParameters: {gcStatus: gcStatus, generation: generation}}
        );
        return stats || [];
    }

    const getChildrenContent = (data: ObjectTypeStatistics[]) => {
        const totalCount = data.reduce((sum, current) => sum + (current.instanceCount || 0), 0)
        const totalSize = data.reduce((sum, current) => sum + (current.totalSize || 0), 0)

        const propertyRows: PropertyRow[] = [
            {title: 'Total count', value: String(totalCount)},
            {title: 'Total size', value: toSizeString(totalSize)},
        ]

        return <Stack>
            <PropertiesTable rows={propertyRows}/>
            {renderStatisticsTable(data)}
        </Stack>
    }

    return (
        <Stack>
            <Stack direction="row">
                {/* TODO make disabled while ProgressContainer isLoading. Add onLoading(loading: bool) to ProgressContainer */}
                {/* TODO add spacing between controls */}
                <ObjectGCStatusSelect gcStatus={gcStatus} onChange={(status) => setGcStatus(status)}/>
                <GenerationSelect generation={generation} onChange={(generation) => setGeneration(generation)}/>
            </Stack>
            <ProgressContainer loadData={getData} getChildren={getChildrenContent}/>
        </Stack>
    );
}