import React, {useEffect} from 'react';
import {DataGrid, GridColDef, GridToolbar} from '@mui/x-data-grid';
import {Stack} from "@mui/material";

import {HttpHeader, HttpRequestInfo, HttpRequestStatus, ObjectGCStatus,} from '../../client/models';

import {useStateWithLoading} from "../../hooks/useStateWithLoading";
import {useNotifyError} from "../../hooks/useNotifyError";

import {objectAddressColumn} from "../../lib/gridColumns";
import getClient from '../../lib/getClient'
import handleFetchData from "../../lib/handleFetchData";

import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {ObjectGCStatusSelect} from "../../components/ObjectGCStatusSelect";
import {ProgressContainer} from "../../components/ProgressContainer";
import {HttpRequestStatusSelect} from "../../components/HttpRequestStatusSelect";

const columns: GridColDef[] = [
    {
        ...objectAddressColumn,
        field: "requestAddress"
    },
    {
        field: "statusCode",
        headerName: 'Status Code',
        type: "number"
    },
    {
        field: "url",
        headerName: 'Url',
        flex: 0.75
    },
    {
        field: "traceparent",
        headerName: 'Trace Parent',
        flex: 0.25,
        valueGetter: params => {
            return params.row.requestHeaders.find((h: HttpHeader) => h.name === 'traceparent')?.value
                ?? params.row.responseHeaders.find((h: HttpHeader) => h.name === 'traceparent')?.value;
        },
        renderCell: params => {
            return <div className="monoFont">{params.value}</div>
        }
    },
];

export const HttpRequests = () => {
    const {notify, notifyError} = useNotifyError();

    const [gcStatus, setGcStatus] = React.useState<ObjectGCStatus>()
    const [status, setStatus] = React.useState<HttpRequestStatus>()
    const [httpRequests, setHttpRequests, isLoading, setIsLoading] = useStateWithLoading<HttpRequestInfo[]>()

    useEffect(() => {
        const fetchHttpRequests = async () => {
            const client = getClient();
            return await client.api.dump.httpRequests.get(
                {queryParameters: {gcStatus: gcStatus, status: status}}
            )
        }

        handleFetchData(fetchHttpRequests, setHttpRequests, setIsLoading, notifyError);
    }, [gcStatus, status, notify]);

    const renderTable = (httpRequests: HttpRequestInfo[]) => {
        // TODO master - detail for headers
        // this grid required Pro license: https://mui.com/x/react-data-grid/master-detail/
        return (
            <DataGrid
                rows={httpRequests}
                getRowId={(row) => row.requestAddress}
                columns={columns}
                rowHeight={25}
                pageSizeOptions={[20, 50, 100]}
                density='compact'
                initialState={{
                    // sorting: {
                    //     sortModel: [{field: 'length', sort: 'desc'}],
                    // },
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

    const getChildrenContent = (httpRequests?: HttpRequestInfo[]) => {
        if (!httpRequests || httpRequests.length === 0)
            return undefined;

        const propertyRows: PropertyRow[] = [
            {title: 'Count', value: String(httpRequests.length)},
        ]

        return <Stack>
            <PropertiesTable rows={propertyRows}/>
            {renderTable(httpRequests)}
        </Stack>
    }

    return (
        <Stack>
            <Stack direction="row">
                <ObjectGCStatusSelect gcStatus={gcStatus} onChange={(status) => setGcStatus(status)}/>
                <HttpRequestStatusSelect status={status} onChange={(status) => setStatus(status)}/>
            </Stack>
            <ProgressContainer isLoading={isLoading}>
                {getChildrenContent(httpRequests)}
            </ProgressContainer>
        </Stack>
    );
}