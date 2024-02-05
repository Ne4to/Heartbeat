import React, {useEffect} from 'react';
import {useParams} from 'react-router-dom';
import {DataGrid, GridColDef, GridRenderCellParams, GridToolbar} from '@mui/x-data-grid';
import {Stack} from "@mui/material";
import {TabbedShowLayout} from "react-admin";

import {ClrObjectArrayItem, ClrObjectField, ClrObjectRootPath, DictionaryInfo, GetClrObjectResult, JwtInfo} from '../../client/models';

import {useStateWithLoading} from "../../hooks/useStateWithLoading";
import {useNotifyError} from "../../hooks/useNotifyError";

import fetchData from "../../lib/handleFetchData";
import getClient from '../../lib/getClient'
import toHexAddress from '../../lib/toHexAddress'
import {renderClrObjectLink, renderMethodTableLink} from "../../lib/gridRenderCell";
import {methodTableColumn, objectAddressColumn} from "../../lib/gridColumns";
import toSizeString from "../../lib/toSizeString";

import {PropertiesTable, PropertyRow} from '../../components/PropertiesTable'
import {ClrObjectRoot} from "../../components/ClrObjectRoot";
import {ProgressContainer} from "../../components/ProgressContainer";
import Box from "@mui/material/Box";

// TODO add refs to current object
// TODO add Dictionary, Queue, Stack and other collections view to a new tab
// TODO add ConcurrentDictionary view to a new tab (dcd, dumpconcurrentdictionary <address>    Displays concurrent dictionary content.)
// TODO add ConcurrentQueue view to a new tab (dcq, dumpconcurrentqueue <address>         Displays concurrent queue content.)
// TODO add base64 decode tab (base64 string -> utf8 string) | devenv dump - https://localhost:44443/#/clr-object/0000021d1b6452b8/show
// TODO add gzip string decode tab | devenv dump - https://localhost:44443/#/clr-object/0000021d1b6f6f80/show
// TODO add certificate decode tab
// TODO find other debugger visualizers

export const ClrObject = () => {
    const {id} = useParams();
    const address = Number('0x' + id);
    const {notify, notifyError} = useNotifyError();

    const [clrObject, setClrObject, isClrObjectLoading, setIsClrObjectLoading] = useStateWithLoading<GetClrObjectResult>()
    const [fields, setFields, isFieldsLoading, setIsFieldsLoading] = useStateWithLoading<ClrObjectField[]>()
    const [roots, setRoots, isRootsLoading, setIsRootsLoading] = useStateWithLoading<ClrObjectRootPath[]>()
    const [arrayItems, setArrayItems, isArrayItemsLoading, setArrayItemsLoading] = useStateWithLoading<ClrObjectArrayItem[]>()
    const [dictionary, setDictionary, isDictionaryLoading, setDictionaryLoading] = useStateWithLoading<DictionaryInfo>()
    const [jwt, setJwt, isJwtLoading, setJwtLoading] = useStateWithLoading<JwtInfo>()

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

    useEffect(() => {
        const fetchArrayItems = async () => {
            const client = getClient();
            return await client.api.dump.object.byAddress(address).asArray.get()
        }

        fetchData(fetchArrayItems, setArrayItems, setArrayItemsLoading, notifyError)
    }, [address, notify]);

    useEffect(() => {
        const fetchDictionary = async () => {
            const client = getClient();
            return await client.api.dump.object.byAddress(address).asDictionary.get()
        }

        fetchData(fetchDictionary, setDictionary, setDictionaryLoading, notifyError)
    }, [address, notify]);

    useEffect(() => {
        const fetchJwt = async () => {
            const client = getClient();
            return await client.api.dump.object.byAddress(address).asJwt.get()
        }

        fetchData(fetchJwt, setJwt, setJwtLoading, notifyError)
    }, [address, notify]);

    const getChildrenContent = (objectResult?: GetClrObjectResult) => {
        if (!objectResult)
            return undefined;

        const propertyRows: PropertyRow[] = [
            {title: 'Address', value: toHexAddress(objectResult.address)},
            {title: 'Size', value: toSizeString(objectResult.size || 0)},
            {title: 'Generation', value: objectResult.generation},
            // TODO add Live / Dead
            // TODO add ValueType / reference type
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

    return (
        <Stack>
            <ProgressContainer isLoading={isClrObjectLoading}>
                {getChildrenContent(clrObject)}
            </ProgressContainer>

            <TabbedShowLayout record={{id: 0}} syncWithLocation={false}>
                <TabbedShowLayout.Tab label="Fields">
                    <FieldsTabContent isLoading={isFieldsLoading} fields={fields}/>
                </TabbedShowLayout.Tab>
                <TabbedShowLayout.Tab label="Roots">
                    <RootsTabContent isLoading={isRootsLoading} roots={roots}/>
                </TabbedShowLayout.Tab>
                <TabbedShowLayout.Tab label="Array" hidden={!isArrayItemsLoading && !arrayItems}>
                    <ArrayTabContent isLoading={isArrayItemsLoading} arrayItems={arrayItems}/>
                </TabbedShowLayout.Tab>
                <TabbedShowLayout.Tab label="Dictionary" hidden={!isDictionaryLoading && !dictionary}>
                    <DictionaryTabContent isLoading={isDictionaryLoading} dictionary={dictionary}/>
                </TabbedShowLayout.Tab>
                <TabbedShowLayout.Tab label="JWT" hidden={!isJwtLoading && !jwt}>
                    <JwtTabContent isLoading={isJwtLoading} jwt={jwt}/>
                </TabbedShowLayout.Tab>
            </TabbedShowLayout>
        </Stack>
    );
}

const FieldsTabContent = (props: { isLoading: boolean, fields?: ClrObjectField[] }) => {
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

    const getFieldsGrid = (fields?: ClrObjectField[]) => {
        if (!fields || fields.length === 0)
            return undefined;

        return (
            <DataGrid
                rows={fields}
                getRowId={(row) => row.name!}
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

    return (<ProgressContainer isLoading={props.isLoading}>
        {getFieldsGrid(props.fields)}
    </ProgressContainer>);
}

const RootsTabContent = (props: { isLoading: boolean, roots?: ClrObjectRootPath[] }) => {
    const getRootsContent = (roots?: ClrObjectRootPath[]) => {
        if (!roots || roots.length === 0)
            return undefined;

        return <ClrObjectRoot rootPath={roots[0]}/>
    }

    return (<ProgressContainer isLoading={props.isLoading}>
        {getRootsContent(props.roots)}
    </ProgressContainer>)
}

const ArrayTabContent = (props: { isLoading: boolean, arrayItems?: ClrObjectArrayItem[] }) => {

    // TODO show char[] as string
    // TODO show byte[] as utf8 string
    const getArrayItemsContent = (arrayItems?: ClrObjectArrayItem[]) => {
        if (!arrayItems || arrayItems.length === 0)
            return undefined;

        const arrayItemsColumns: GridColDef[] = [
            {
                field: 'index',
                headerName: 'Index',
                type: 'number'
            },
            objectAddressColumn,
            {
                field: 'value',
                headerName: 'Value',
                minWidth: 200,
                flex: 1
            }
        ];

        return (
            <DataGrid
                rows={arrayItems}
                getRowId={(row) => row.index}
                columns={arrayItemsColumns}
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

    return (<ProgressContainer isLoading={props.isLoading}>
        {getArrayItemsContent(props.arrayItems)}
    </ProgressContainer>);
}

const DictionaryTabContent = (props: { isLoading: boolean, dictionary?: DictionaryInfo }) => {

    // TODO show char[] as string
    // TODO show byte[] as utf8 string
    const getDictionaryItemsContent = (dictionary?: DictionaryInfo) => {
        if (!dictionary)
            return undefined;

        const GetGrid = () => {
            if (!dictionary || dictionary.items!.length === 0)
                return (<></>);

            const columns: GridColDef[] = [
                {
                    ...objectAddressColumn,
                    field: 'key.address',
                    headerName: 'Key address',
                    valueGetter: params => params.row.key.address
                },
                {
                    ...objectAddressColumn,
                    field: 'value.address',
                    headerName: 'Value address',
                    valueGetter: params => params.row.value.address
                },
                {
                    field: 'key.value',
                    headerName: 'Key',
                    valueGetter: params => params.row.key.value,
                    flex: 0.5
                },
                {
                    field: 'value.value',
                    headerName: 'Value',
                    valueGetter: params => params.row.value.value,
                    flex: 1
                },
            ];

            return (
                <DataGrid
                    rows={dictionary.items || []}
                    getRowId={(row) => row.key!.address!}
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

        const propertyRows: PropertyRow[] = [
            {title: 'Count', value: dictionary.count!},
            // {title: 'Key MT', value: renderMethodTableLink(dictionary.keyMethodTable)},
            // {title: 'Value MT', value: renderMethodTableLink(dictionary.keyMethodTable)},
        ]

        return (
            <Stack>
                <PropertiesTable rows={propertyRows}/>
                {GetGrid()}
            </Stack>
        );
    }

    return (<ProgressContainer isLoading={props.isLoading}>
        {getDictionaryItemsContent(props.dictionary)}
    </ProgressContainer>);
}

const JwtTabContent = (props: { isLoading: boolean, jwt?: JwtInfo }) => {
    const getRootsContent = (jwt?: JwtInfo) => {
        if (!jwt)
            return undefined;

        const columns: GridColDef[] = [
            {
                field: 'key',
                headerName: 'Key',
                minWidth: 200
            },
            {
                field: 'value',
                headerName: 'Value',
                width: 600
            },
            {
                field: 'description',
                headerName: 'Description',
                flex: 1
            }
        ];

        return (
            <Stack>
                <Box>Header</Box>
                <DataGrid
                    rows={props.jwt?.header!}
                    getRowId={(row) => row.key!}
                    columns={columns}
                    rowHeight={25}
                    density='compact'
                />
                <Box>Payload</Box>
                <DataGrid
                    rows={props.jwt?.payload!}
                    getRowId={(row) => row.key!}
                    columns={columns}
                    rowHeight={25}
                    density='compact'
                />
            </Stack>
        );
    }

    return (<ProgressContainer isLoading={props.isLoading}>
        {getRootsContent(props.jwt)}
    </ProgressContainer>)
}