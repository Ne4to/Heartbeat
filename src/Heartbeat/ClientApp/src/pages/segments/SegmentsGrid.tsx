import React, {useEffect} from 'react';
import {DataGrid, GridColDef} from '@mui/x-data-grid';
import {Stack} from "@mui/material";

import {HeapSegment} from '../../client/models';

import {useNotifyError} from "../../hooks/useNotifyError";
import {useStateWithLoading} from "../../hooks/useStateWithLoading";

import getClient from '../../lib/getClient'
import {addressColumn, sizeColumn} from "../../lib/gridColumns";
import toSizeString from "../../lib/toSizeString";
import handleFetchData from "../../lib/handleFetchData";

import {ProgressContainer} from "../../components/ProgressContainer";
import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";

const columns: GridColDef[] = [
    {
        ...addressColumn,
        field: 'start',
        headerName: 'Start',
    },
    {
        ...addressColumn,
        field: 'end',
        headerName: 'End',
    },
    sizeColumn,
    {
        field: 'kind',
        headerName: 'Kind',
        minWidth: 200,
        flex: 1,
    }
];

export const SegmentsGrid = () => {
    const {notify, notifyError} = useNotifyError();

    const [segments, setSegments, isLoading, setIsLoading] = useStateWithLoading<HeapSegment[]>()

    useEffect(() => {
        const fetchSegments = async () => {
            const client = getClient();
            const result = await client.api.dump.segments.get()
            return result!
        }

        handleFetchData(fetchSegments, setSegments, setIsLoading, notifyError);
    }, [notify]);

    const renderTable = (segments: HeapSegment[]) => {
        return (
            <div style={{flexGrow: 1, width: '100%'}}>

                <DataGrid
                    rows={segments}
                    getRowId={(row) => row.start}
                    columns={columns}
                    rowHeight={25}
                    density='compact'
                    pageSizeOptions={[20, 50, 100]}
                    pagination
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

    const getChildrenContent = (segments?: HeapSegment[]) => {
        if (!segments || segments.length === 0)
            return undefined;

        const totalSize = segments.map(m => m.size!).reduce((sum, current) => sum + current, 0)

        const propertyRows: PropertyRow[] = [
            {title: 'Total size', value: toSizeString(totalSize)},
        ]

        return <Stack>
            <PropertiesTable rows={propertyRows}/>
            {renderTable(segments)}
        </Stack>
    }

    // TODO add SegmentKindSelect
    return (
        <ProgressContainer isLoading={isLoading}>
            {getChildrenContent(segments)}
        </ProgressContainer>
    );
}