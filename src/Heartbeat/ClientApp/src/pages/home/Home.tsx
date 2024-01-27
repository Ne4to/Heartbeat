import React, {useEffect} from 'react';

import getClient from '../../lib/getClient'
import {DumpInfo} from "../../client/models";

import {useNotifyError} from "../../hooks/useNotifyError";
import {useStateWithLoading} from "../../hooks/useStateWithLoading";

import fetchData from "../../lib/handleFetchData";

import {PropertiesTable, PropertyRow} from "../../components/PropertiesTable";
import {ProgressContainer} from "../../components/ProgressContainer";

const Home = () => {
    const {notify, notifyError} = useNotifyError();

    const [dump, setDump, isLoading, setIsLoading] = useStateWithLoading<DumpInfo>()

    useEffect(() => {
        const getData = async () => {
            const client = getClient();
            return await client.api.dump.info.get()
        }

        fetchData(getData, setDump, setIsLoading, notifyError);
    }, [notify]);

    const getChildrenContent = (data?: DumpInfo) => {
        if (!data)
            return undefined;

        const propertyRows: PropertyRow[] = [
            {title: 'Dump', value: data.dumpPath},
            {title: 'Runtime', value: `${data.platform} ${data.architecture} .NET ${data.runtimeVersion}`},
            {title: 'Server heap', value: String(data.isServerHeap)},
            {title: 'Can walk heap', value: String(data.canWalkHeap)},
            {title: 'Process id', value: String(data.processId)},
            {title: 'Clr module', value: data.clrModulePath},
        ]

        return (<PropertiesTable rows={propertyRows}/>)
    }

    return (
        <ProgressContainer isLoading={isLoading}>
            {getChildrenContent(dump)}
        </ProgressContainer>
    );
}

export default Home;