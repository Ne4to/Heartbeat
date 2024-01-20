import React, {useEffect} from 'react';
import LinearProgress from '@mui/material/LinearProgress';

import getClient from '../lib/getClient'
import Box from "@mui/material/Box";
import {DumpInfo} from "../client/models";
import {PropertiesTable, PropertyRow} from "../components/PropertiesTable";
import toHexAddress from "../lib/toHexAddress";
import prettyBytes from "pretty-bytes";

const Home = () => {
    const [loading, setLoading] = React.useState<boolean>(true)
    const [dumpInfo, setDumpInfo] = React.useState<DumpInfo>()

    useEffect(() => {
        loadData().catch(console.error).finally(() => setLoading(false));
    }, []);

    const loadData = async () => {
        setLoading(true)
        const client = getClient();
        const result = await client.api.dump.info.get()
        setDumpInfo(result!)
    }

    const propertyRows: PropertyRow[] = [
        {title: 'Runtime', value: `${dumpInfo?.platform} ${dumpInfo?.architecture} ${dumpInfo?.runtimeVersion}`},
        {title: 'Dump', value: dumpInfo?.dumpPath},
        {title: 'Server heap', value: String(dumpInfo?.isServerHeap)},
        {title: 'Can walk heap', value: String(dumpInfo?.canWalkHeap)},
        {title: 'Process id', value: String(dumpInfo?.processId)},
        {title: 'Clr module', value: dumpInfo?.clrModulePath},
    ]

    const contents = loading
        ?
        <Box sx={{width: '100%'}}>
            <LinearProgress/>
        </Box>
        :
        <PropertiesTable rows={propertyRows}/>;

    return (
        <div style={{display: 'flex', flexFlow: 'column'}}>
            {contents}
        </div>
    );
}

export default Home;