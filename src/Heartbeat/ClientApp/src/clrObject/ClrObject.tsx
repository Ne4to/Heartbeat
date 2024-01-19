import React, {useEffect} from 'react';
import {useSearchParams} from 'react-router-dom';
import LinearProgress from '@mui/material/LinearProgress';
import {DataGrid, GridColDef, GridRenderCellParams, GridToolbar, GridValueGetterParams} from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import {formatAddress} from '../lib/gridFormatter';
import toHexAddress from '../lib/toHexAddress'
import prettyBytes from 'pretty-bytes';
import {GetClrObjectResult, ClrObjectField} from '../client/models';
import {PropertiesTable, PropertyRow} from '../components/PropertiesTable'
import {renderMethodTable, renderMethodTableLink} from "../lib/gridRenderCell";

const columns: GridColDef[] = [
    {
        field: 'methodTable',
        headerName: 'MT',
        type: 'number',
        width: 200,
        valueFormatter: formatAddress,
        renderCell: renderMethodTable,
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
        field: 'name',
        headerName: 'Name',
        minWidth: 200,
        flex: 0.5,
    },
    {
        field: 'value',
        headerName: 'Value',
        minWidth: 200,
        flex: 1,
        renderCell: (params: GridRenderCellParams) => {
            if (params.value == null) {
                return '';
            }

            const objectAddress = params.row.objectAddress;

            return objectAddress
                ? (
                    <a href={'#/clr-object?address=' + toHexAddress(objectAddress)}>{params.value}</a>
                )
                : (
                    params.value
                )
        }
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
            <div style={{flexGrow: 1, width: '100%'}}>

                <DataGrid
                    rows={fields}
                    getRowId={(row) => row.name}
                    columns={columns}
                    rowHeight={25}
                    pageSizeOptions={[20, 50, 100]}
                    density='compact'
                    slots={{toolbar: GridToolbar}}
                    initialState={{
                        pagination: {paginationModel: {pageSize: 20}},
                    }}
                />

            </div>
        );
    }

    let contents = loading
        ? <Box sx={{width: '100%'}}>
            <LinearProgress/>
        </Box>
        : renderTable(objectResult!.fields!);

// TODO add refs        
//         <p>
//             Resf to: @RefsToCount
//         </p>
//         
//         <p>
//             Resf from: @RefsFromCount
//         </p>

    const propertyRows: PropertyRow[] = [
        {title: 'Address', value: toHexAddress(objectResult?.address)},
        {title: 'Size', value: prettyBytes(objectResult?.size || 0)},
        {title: 'Generation', value: objectResult?.generation},
        {title: 'MethodTable', value: renderMethodTableLink(objectResult?.methodTable)},
        {title: 'Type', value: objectResult?.typeName},
        {title: 'Module', value: objectResult?.moduleName},
    ]

    if (objectResult?.value) {
        propertyRows.push(
            {title: 'Value', value: objectResult?.value},
        )
    }

    return (
        <div style={{display: 'flex', flexFlow: 'column'}}>
            <h4 id="tableLabel" style={{flexGrow: 1}}>Clr Object</h4>
            <PropertiesTable rows={propertyRows}/>
            {contents}
        </div>
    );
}