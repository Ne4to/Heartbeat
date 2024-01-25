import React, {useEffect} from 'react';
import {useSearchParams} from 'react-router-dom';
import LinearProgress from '@mui/material/LinearProgress';
import {DataGrid, GridColDef, GridRenderCellParams, GridToolbar} from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import toHexAddress from '../lib/toHexAddress'
import {GetClrObjectResult, ClrObjectField, ClrObjectRootPath} from '../client/models';
import {PropertiesTable, PropertyRow} from '../components/PropertiesTable'
import {renderMethodTableLink} from "../lib/gridRenderCell";
import {ClrObjectRoot} from "../components/ClrObjectRoot";
import {methodTableColumn} from "../lib/gridColumns";
import toSizeString from "../lib/toSizeString";

const columns: GridColDef[] = [
    methodTableColumn,
    {
        field: 'offset',
        headerName: 'Offset',
        type: 'number',
        width: 80
    },
    {
        field: 'isValueType',
        headerName: 'VT',
    },
    {
        field: 'typeName',
        headerName: 'Type',
        minWidth: 200,
        flex: 0.5,
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
    const [roots, setRoots] = React.useState<ClrObjectRootPath[]>()
    const [searchParams] = useSearchParams();
    const [address, setAddress] = React.useState(Number('0x' + searchParams.get('address')))

    useEffect(() => {
        loadData().catch(console.error)
        loadRoots().catch(console.error)
    });

    const loadData = async () => {
        const client = getClient();

        const result = await client.api.dump.object.byAddress(address).get()
        setObjectResult(result!)
        setLoading(false)
    }

    const loadRoots = async() => {
        const client = getClient();
        const result = await client.api.dump.object.byAddress(address).roots.get()
        setRoots(result!)
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

    const propertyRows: PropertyRow[] = [
        {title: 'Address', value: toHexAddress(objectResult?.address)},
        {title: 'Size', value: toSizeString(objectResult?.size || 0)},
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

    const rootGrid = roots && roots.length !== 0
        ? <ClrObjectRoot rootPath={roots[0]} />
        : <div>root path not found</div>;

    return (
        <div style={{display: 'flex', flexFlow: 'column'}}>
            <h4 id="tableLabel" style={{flexGrow: 1}}>Clr Object</h4>
            <PropertiesTable rows={propertyRows}/>
            {contents}
            {rootGrid}
        </div>
    );
}