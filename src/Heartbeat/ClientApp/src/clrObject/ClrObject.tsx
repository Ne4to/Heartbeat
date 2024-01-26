import React, {useEffect} from 'react';
import {useParams} from 'react-router-dom';
import LinearProgress from '@mui/material/LinearProgress';
import {DataGrid, GridColDef, GridRenderCellParams, GridToolbar} from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import getClient from '../lib/getClient'
import toHexAddress from '../lib/toHexAddress'
import {GetClrObjectResult, ClrObjectField, ClrObjectRootPath} from '../client/models';
import {PropertiesTable, PropertyRow} from '../components/PropertiesTable'
import {renderClrObjectLink, renderMethodTableLink} from "../lib/gridRenderCell";
import {ClrObjectRoot} from "../components/ClrObjectRoot";
import {methodTableColumn} from "../lib/gridColumns";
import toSizeString from "../lib/toSizeString";
import {Button, Link} from "react-admin";

export const ClrObject = () => {
    const { id } = useParams();
    const [loading, setLoading] = React.useState<boolean>(true)
    const [objectResult, setObjectResult] = React.useState<GetClrObjectResult>()
    const [roots, setRoots] = React.useState<ClrObjectRootPath[]>()
    const address = Number('0x' + id);

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
                    ?
                        renderClrObjectLink(objectAddress)
                        // <Button component={Link} to={`/clr-object/${toHexAddress(objectAddress)}/show`}>{params.value}</Button>
                    : (
                        params.value
                    )
            }
        }
    ];

    useEffect(() => {
        loadData().catch(console.error)
        loadRoots().catch(console.error)
    }, [address]);

    const loadData = async () => {
        const client = getClient();

        const result = await client.api.dump.object.byAddress(address).get()
        setObjectResult(result!)
        setLoading(false)
    }

    // TODO move root to a separate tab
    // TODO add Dictionary view to a new tab
    // TODO add Array view to a new tab
    // TODO add JWT decode tab (https://github.com/panva/jose)
    // TODO find other debugger visualizers
    const loadRoots = async() => {
        const client = getClient();
        const result = await client.api.dump.object.byAddress(address).roots.get()
        setRoots(result!)
    }

    const renderTable = (fields: ClrObjectField[]) => {
        return (


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
        // TODO add Live / Dead
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
        <div >
            <h4 id="tableLabel" style={{flexGrow: 1}}>Clr Object</h4>
            <PropertiesTable rows={propertyRows}/>
            {contents}
            {rootGrid}
        </div>
    );
}