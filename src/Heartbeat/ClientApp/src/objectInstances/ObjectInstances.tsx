import React, { useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import LinearProgress from '@mui/material/LinearProgress';
import {
    DataGrid,
    GridColDef,
    gridPageSizeSelector,
    GridRenderCellParams,
    GridValueGetterParams
} from '@mui/x-data-grid';
import Box from '@mui/material/Box';

import { TraversingHeapModeSelect } from '../components/TraversingHeapModeSelect'

import getClient from '../lib/getClient'
import { formatAddress, formatSize } from '../lib/gridFormatter';
import { renderClrObjectAddress } from '../lib/gridRenderCell';
import toHexAddress from '../lib/toHexAddress'
import {
    Generation,
    GetObjectInstancesResult,
    ObjectInstance,
    TraversingHeapModes,
    TraversingHeapModesObject
} from '../client/models';
import {PropertiesTable, PropertyRow} from "../components/PropertiesTable";
import prettyBytes from "pretty-bytes";
import {GenerationSelect} from "../components/GenerationSelect";
import {addressColumn, objectAddressColumn, sizeColumn} from "../lib/gridColumns";

const columns: GridColDef[] = [
    objectAddressColumn,
    sizeColumn,
];

export const ObjectInstances = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [mode, setMode] = React.useState<TraversingHeapModes>(TraversingHeapModesObject.All)
    const [generation, setGeneration] = React.useState<Generation>()
    const [objectInstancesResult, setObjectInstancesResult] = React.useState<GetObjectInstancesResult>()
    const [searchParams] = useSearchParams();
    const [mt, setMt] = React.useState(Number('0x' + searchParams.get('mt')))

    console.log('MT = ' + mt)

    useEffect(() => {
        loadData(mode, generation);
    }, [mode, generation]);

    const loadData = async (mode: TraversingHeapModes, generation?: Generation) => {
        const client = getClient();

        const result = await client.api.dump.objectInstances.byMt(mt).get(
            { queryParameters: { traversingMode: mode, generation: generation } }
        );
        setObjectInstancesResult(result!)
        setLoading(false)
    }

    const renderTable = (instances: ObjectInstance[]) => {
        return (
            <div style={{ flexGrow: 1, width: '100%' }}>

                <DataGrid
                    rows={instances}
                    getRowId={(row) => row.address}
                    columns={columns}
                    rowHeight={25}
                    density='compact'
                    pageSizeOptions={[20, 50, 100]}
                    pagination
                    initialState={{
                        pagination: { paginationModel: { pageSize: 20 } },
                    }}
                />

            </div>
        );
    }

    let contents = loading
        ? <Box sx={{ width: '100%' }}>
            <LinearProgress />
        </Box>
        : renderTable(objectInstancesResult!.instances!);

    const totalSize = objectInstancesResult?.instances!.map(m => m.size!)
        .reduce((sum, current) => sum + current, 0)

    const propertyRows: PropertyRow[] = [
        {title: 'Count', value: String(objectInstancesResult?.instances!.length)},
        {title: 'Total size', value: prettyBytes(totalSize || 0)},
    ]

    return (
        <div style={{ display: 'flex', flexFlow: 'column' }}>
            <h4 id="tableLabel" style={{ flexGrow: 1 }}>MT {toHexAddress(objectInstancesResult?.methodTable)} - {objectInstancesResult?.typeName}</h4>
            <div style={{ flexGrow: 1 }}>
                <TraversingHeapModeSelect mode={mode} onChange={(mode) => setMode(mode)} />
                <GenerationSelect generation={generation} onChange={(generation) => setGeneration(generation)}/>
            </div>
            <PropertiesTable rows={propertyRows}/>
            {contents}
        </div>
    );
}