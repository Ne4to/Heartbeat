import {Admin, Resource, localStorageStore, useStore, StoreContextProvider} from 'react-admin'
import {themes, ThemeName} from './themes/themes';
import React from "react";

import {Layout} from './layout';
import {Home} from './pages/home'
import headDump from './pages/heapDumpStat'
import segments from './pages/segments'
import objectInstances from './pages/objectInstances'
import clrObject from './clrObject'
import roots from './pages/roots'
import modules from './pages/modules'
import arraysGrid from './pages/arraysGrid'
import sparseArraysStat from './pages/sparseArraysStat'
import stringsGrid from './pages/stringsGrid'
import stringDuplicates from './pages/stringDuplicates'
import {dataProvider} from "./lib/dataProvider";
import './App.css'

const store = localStorageStore(undefined, 'Heartbeat');

const App = () => {
    const [themeName] = useStore<ThemeName>('themeName', 'soft');
    const lightTheme = themes.find(theme => theme.name === themeName)?.light;
    const darkTheme = themes.find(theme => theme.name === themeName)?.dark;

    return (
        <Admin
            title=''
            // dataProvider={dataProvider}
            store={store}
            layout={Layout}
            dashboard={Home}
            disableTelemetry
            lightTheme={lightTheme}
            darkTheme={darkTheme}
            defaultTheme='light'>

            <Resource name='heap-dump' {...headDump} />
            <Resource name='segments' {...segments} />
            <Resource name='object-instances' {...objectInstances} />
            <Resource name='roots' {...roots} />
            <Resource name='modules' {...modules} />
            <Resource name='clr-object' {...clrObject} />
            <Resource name='sparse-arrays-stat' {...sparseArraysStat} />
            <Resource name='arrays' {...arraysGrid} />
            <Resource name='strings' {...stringsGrid} />
            <Resource name='string-duplicates' {...stringDuplicates} />
        </Admin>
    );
}

const AppWrapper = () => (
    <StoreContextProvider value={store}>
        <App/>
    </StoreContextProvider>
);

export default AppWrapper;