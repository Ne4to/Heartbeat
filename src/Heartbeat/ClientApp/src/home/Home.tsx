import React, {useEffect} from 'react';
import LinearProgress from '@mui/material/LinearProgress';

import getClient from '../lib/getClient'
import Box from "@mui/material/Box";
import {DumpInfo} from "../client/models";

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

    const contents = loading
        ?
        <Box sx={{width: '100%'}}>
            <LinearProgress/>
        </Box>
        :
        <ul>
            <li>Runtime: {dumpInfo?.platform} {dumpInfo?.architecture} {dumpInfo?.runtimeVersion}</li>
            <li>Dump: {dumpInfo?.dumpPath}</li>
            <li>Server heap: {String(dumpInfo?.isServerHeap)}</li>
            <li>Can walk heap: {String(dumpInfo?.canWalkHeap)}</li>
            <li>Process id: {dumpInfo?.processId}</li>
            <li>Clr module: {dumpInfo?.clrModulePath}</li>
        </ul>;

    return (
        <div>
            {contents}
        </div>
    );
}

export default Home;