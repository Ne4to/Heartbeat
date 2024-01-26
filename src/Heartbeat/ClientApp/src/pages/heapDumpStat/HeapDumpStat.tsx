import React from 'react';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';

import {TraversingHeapModeSelect} from '../../components/TraversingHeapModeSelect'
import {GenerationSelect} from '../../components/GenerationSelect'

import getClient from '../../lib/getClient'
import {Generation, ObjectTypeStatistics, TraversingHeapModes, TraversingHeapModesObject} from '../../client/models';
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
    const [mode, setMode] = React.useState<TraversingHeapModes>(TraversingHeapModesObject.All)
    const [generation, setGeneration] = React.useState<Generation>()

    const renderStatisticsTable = (statistics: ObjectTypeStatistics[]) => {
        return (
            <div style={{flexGrow: 1, width: '100%'}}>

                <DataGrid
                    rows={statistics}
                    getRowId={(row) => row.typeName}
                    columns={columns}
                    rowHeight={25}
                    density='compact'
                    pageSizeOptions={[20, 50, 100]}
                    pagination
                    initialState={{
                        filter: {
                            filterModel: {
                                items: [],
                                quickFilterValues: [],
                            },
                        },
                        sorting: {
                            sortModel: [{field: 'totalSize', sort: 'desc'}],
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

    const getData = async () => {
        const client = getClient();
        const stats = await client.api.dump.heapDumpStatistics.get(
            {queryParameters: {traversingMode: mode, generation: generation}}
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
                <TraversingHeapModeSelect mode={mode} onChange={(mode) => setMode(mode)}/>
                <GenerationSelect generation={generation} onChange={(generation) => setGeneration(generation)}/>
            </Stack>
            <ProgressContainer loadData={getData} getChildren={getChildrenContent}/>
        </Stack>
    );
}