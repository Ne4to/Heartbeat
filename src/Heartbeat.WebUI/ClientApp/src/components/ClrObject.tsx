import React, { useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import LinearProgress from '@mui/material/LinearProgress';
import { DataGrid, GridColDef, GridRenderCellParams, GridToolbar, GridValueFormatterParams, GridValueGetterParams } from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import { formatAddress } from '../lib/gridFormatter';
import toHexAddress from '../lib/toHexAddress'
import toHexString from '../lib/toHexString'
import prettyBytes from 'pretty-bytes';
import { GetClrObjectResult, ClrObjectField } from '../client/models';

const columns: GridColDef[] = [
    {
        field: 'mt',
        headerName: 'MT',
        type: 'number',
        width: 200,
        valueGetter: (params: GridValueGetterParams) => params.row.methodTable,
        valueFormatter: formatAddress
    },
    {
        field: 'offset',
        headerName: 'Offset',
        // valueFormatter: (params: GridValueFormatterParams<number>) => {
        //     if (params.value == null) {
        //         return '';
        //     }
        //     return toHexString(params.value);
        // }
    },
    {
        field: 'typeName',
        headerName: 'Type',
        minWidth: 200,
        flex: 0.5,
    },
    {
        field: 'isValueType',
        headerName: 'VT',
    },
    {
        field: 'value',
        headerName: 'Value',
        minWidth: 200,
        flex: 1,
        renderCell: (params: GridRenderCellParams<any, any>) => {
            if (params.value == null) {
                return '';
            }

            const objectAddress = params.row.objectAddress;

            return objectAddress
                ? (
                    <a href={'/clr-object?address=' + toHexAddress(objectAddress)}>{params.value}</a>
                )
                : (
                    params.value
                )
        }
    },
    {
        field: 'name',
        headerName: 'Name',
        minWidth: 200,
        flex: 0.5,
    }
];

export const ClrObject = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [objectResult, setObjectResult] = React.useState<GetClrObjectResult>()
    const [searchParams] = useSearchParams();
    const [address, setAddress] = React.useState(Number('0x' + searchParams.get('address')))

    console.log('Address = ' + address)

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        const client = getClient();

        const result = await client.api.dump.object.byAddress(address).get()
        setObjectResult(result!)
        setLoading(false)
    }

    const renderTable = (fields: ClrObjectField[]) => {
        return (
            <div style={{ flexGrow: 1, height: 700, width: '100%' }}>

                <DataGrid
                    rows={fields}
                    getRowId={(row) => row.name}
                    columns={columns}
                    rowHeight={25}
                    density='compact'
                    slots={{ toolbar: GridToolbar }}
                />

            </div>
        );
    }

    let contents = loading
        ? <Box sx={{ width: '100%' }}>
            <LinearProgress />
        </Box>
        : renderTable(objectResult!.fields!);

    return (
        <div style={{ display: 'flex', flexFlow: 'column' }}>
            <h4 id="tableLabel" style={{ flexGrow: 1 }}>Clr Object {toHexAddress(address)}</h4>
            <ul>
                <li>Name {objectResult?.typeName}</li>
                <li>MethodTable {toHexAddress(objectResult?.methodTable)}</li>
                <li>Size {prettyBytes(objectResult?.size || 0)}</li>
            </ul>
            {contents}
        </div>
    );
}