import React from 'react';

import getClient from '../../lib/getClient'
import {DumpInfo} from "../../client/models";
import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {ProgressContainer} from "../../components/ProgressContainer";

const Home = () => {
    const getData = async () => {
        const client = getClient();
        const result = await client.api.dump.info.get()
        return result!
    }

    const getChildrenContent = (data: DumpInfo) => {
        const propertyRows: PropertyRow[] = [
            {title: 'Runtime', value: `${data?.platform} ${data?.architecture} .NET ${data?.runtimeVersion}`},
            {title: 'Dump', value: data?.dumpPath},
            {title: 'Server heap', value: String(data?.isServerHeap)},
            {title: 'Can walk heap', value: String(data?.canWalkHeap)},
            {title: 'Process id', value: String(data?.processId)},
            {title: 'Clr module', value: data?.clrModulePath},
        ]

        return (<PropertiesTable rows={propertyRows}/>)
    }

    return (
        <ProgressContainer loadData={getData} getChildren={getChildrenContent}/>
    );
}

export default Home;