import React, {useEffect} from 'react';
import {useParams} from 'react-router-dom';
import {DataGrid, GridColDef, GridRenderCellParams, GridToolbar} from '@mui/x-data-grid';
import {Stack} from "@mui/material";
import {TabbedShowLayout} from "react-admin";

import {ClrObjectField, ClrObjectRootPath, GetClrObjectResult} from '../../client/models';

import {useStateWithLoading} from "../../hooks/useStateWithLoading";
import {useNotifyError} from "../../hooks/useNotifyError";

import fetchData from "../../lib/handleFetchData";
import getClient from '../../lib/getClient'
import toHexAddress from '../../lib/toHexAddress'
import {renderClrObjectLink, renderMethodTableLink} from "../../lib/gridRenderCell";
import {methodTableColumn} from "../../lib/gridColumns";
import toSizeString from "../../lib/toSizeString";

import {PropertiesTable, PropertyRow} from '../../components/PropertiesTable'
import {ClrObjectRoot} from "../../components/ClrObjectRoot";
import {ProgressContainer} from "../../components/ProgressContainer";

// TODO add Dictionary view to a new tab
// TODO add Array view to a new tab
// TODO add JWT decode tab (https://github.com/panva/jose)
// TODO find other debugger visualizers

export const ClrObject = () => {
    const {id} = useParams();
    const address = Number('0x' + id);
    const {notify, notifyError} = useNotifyError();

    const [clrObject, setClrObject, isClrObjectLoading, setIsClrObjectLoading] = useStateWithLoading<GetClrObjectResult>()
    const [fields, setFields, isFieldsLoading, setIsFieldsLoading] = useStateWithLoading<ClrObjectField[]>()
    const [roots, setRoots, isRootsLoading, setIsRootsLoading] = useStateWithLoading<ClrObjectRootPath[]>()

    useEffect(() => {
        const getObject = async () => {
            const client = getClient();
            return await client.api.dump.object.byAddress(address).get()
        }

        fetchData(getObject, setClrObject, setIsClrObjectLoading, notifyError)
    }, [address, notify]);

    useEffect(() => {
        const getFields = async () => {
            const client = getClient();
            return await client.api.dump.object.byAddress(address).fields.get()
        }

        fetchData(getFields, setFields, setIsFieldsLoading, notifyError)
    }, [address, notify]);

    useEffect(() => {
        const getRoots = async () => {
            const client = getClient();
            return await client.api.dump.object.byAddress(address).roots.get()
        }

        fetchData(getRoots, setRoots, setIsRootsLoading, notifyError)
    }, [address, notify]);

    const fieldsGridColumns: GridColDef[] = [
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
                    renderClrObjectLink(objectAddress, params.value)
                    : (
                        params.value
                    )
            }
        }
    ];

    const getChildrenContent = (objectResult?: GetClrObjectResult) => {
        if (!objectResult)
            return undefined;

        const propertyRows: PropertyRow[] = [
            {title: 'Address', value: toHexAddress(objectResult.address)},
            {title: 'Size', value: toSizeString(objectResult.size || 0)},
            {title: 'Generation', value: objectResult.generation},
            // TODO add Live / Dead
            {title: 'MethodTable', value: renderMethodTableLink(objectResult.methodTable)},
            {title: 'Type', value: objectResult.typeName},
            {title: 'Module', value: objectResult.moduleName},
        ]

        if (objectResult.value) {
            propertyRows.push(
                {title: 'Value', value: objectResult.value},
            )
        }

        return <PropertiesTable rows={propertyRows}/>
    }

    const getFieldsGrid = (fields?: ClrObjectField[]) => {
        if (!fields || fields.length === 0)
            return undefined;

        return (
            <DataGrid
                rows={fields}
                getRowId={(row) => row.name}
                columns={fieldsGridColumns}
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

    const getRootsContent = (roots?: ClrObjectRootPath[]) => {
        if (!roots || roots.length === 0)
            return undefined;

        return <ClrObjectRoot rootPath={roots[0]}/>
    }

    return (
        <Stack>
            <ProgressContainer isLoading={isClrObjectLoading}>
                {getChildrenContent(clrObject)}
            </ProgressContainer>

            <TabbedShowLayout record={{id: 0}} syncWithLocation={false}>
                <TabbedShowLayout.Tab label="Fields">
                    <ProgressContainer isLoading={isFieldsLoading}>
                        {getFieldsGrid(fields)}
                    </ProgressContainer>
                </TabbedShowLayout.Tab>
                <TabbedShowLayout.Tab label="Roots">
                    <ProgressContainer isLoading={isRootsLoading}>
                        {getRootsContent(roots)}
                    </ProgressContainer>
                </TabbedShowLayout.Tab>
            </TabbedShowLayout>
        </Stack>
    );
}