import React from 'react';
import {useParams} from 'react-router-dom';
import {
    DataGrid,
    GridColDef
} from '@mui/x-data-grid';

import { ObjectGCStatusSelect } from '../../components/ObjectGCStatusSelect'

import getClient from '../../lib/getClient'
import toHexAddress from '../../lib/toHexAddress'
import {
    Generation,
    GetObjectInstancesResult,
    ObjectInstance,
    ObjectGCStatus,
} from '../../client/models';
import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {GenerationSelect} from "../../components/GenerationSelect";
import {objectAddressColumn, sizeColumn} from "../../lib/gridColumns";
import toSizeString from "../../lib/toSizeString";
import {Stack} from "@mui/material";
import {ProgressContainer} from "../../components/ProgressContainer";

const columns: GridColDef[] = [
    objectAddressColumn,
    sizeColumn,
];

// TODO add type select (search by name, get list from backend)
export const ObjectInstances = () => {
    const { id } = useParams();
    const [gcStatus, setGcStatus] = React.useState<ObjectGCStatus>()
    const [generation, setGeneration] = React.useState<Generation>()
    const mt = Number('0x' + id)

    const getData = async() => {
        const client = getClient();
        const result = await client.api.dump.objectInstances.byMt(mt).get(
            { queryParameters: { gcStatus: gcStatus, generation: generation } }
        );
        return result!
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

    const getChildrenContent = (data: GetObjectInstancesResult) => {
        const instances = data.instances!;

        const totalSize = instances.map(m => m.size!)
            .reduce((sum, current) => sum + current, 0)

        const propertyRows: PropertyRow[] = [
            {title: 'MT', value: toHexAddress(data!.methodTable)},
            {title: 'Type', value: data!.typeName},
            {title: 'Count', value: String(instances.length)},
            {title: 'Total size', value: toSizeString(totalSize || 0)},
        ]

        return <Stack>
            <PropertiesTable rows={propertyRows}/>
            {renderTable(instances)}
        </Stack>
    }

    return (
        <Stack>
            <Stack direction="row">
                <ObjectGCStatusSelect gcStatus={gcStatus} onChange={(status) => setGcStatus(status)}/>
                <GenerationSelect generation={generation} onChange={(generation) => setGeneration(generation)}/>
            </Stack>
            <ProgressContainer loadData={getData} getChildren={getChildrenContent}/>
        </Stack>
    );
}