import {Admin, Resource, localStorageStore, useStore, StoreContextProvider} from 'react-admin'
import {Layout} from './layout';
import {Home} from './home'
import headDump from './heapDumpStat'
import segments from './segments'
import objectInstances from './objectInstances'
import clrObject from './clrObject'
import modules from './modules'
import {AlertContext} from './contexts/alertContext';
import {themes, ThemeName} from './themes/themes';
import React, {useState} from "react";
import Snackbar from "@mui/material/Snackbar";
import MuiAlert from "@mui/material/Alert";

const store = localStorageStore(undefined, 'Heartbeat');

const App = () => {
    const [themeName] = useStore<ThemeName>('themeName', 'soft');
    const lightTheme = themes.find(theme => theme.name === themeName)?.light;
    const darkTheme = themes.find(theme => theme.name === themeName)?.dark;

    const [showErrorMessage, setShowErrorMessage] = useState(false)
    const [errorMessage, setErrorMessage] = useState('')

    const handleCloseErrorMessage = (event?: React.SyntheticEvent | Event, reason?: string) => {
        if (reason === 'clickaway') {
            return;
        }

        setShowErrorMessage(false);
    };

    const onErrorMessage = (message: string) => {
        setErrorMessage(message)
        setShowErrorMessage(true)
    }

    return (
        <AlertContext.Provider value={onErrorMessage}>
            <>
                <Admin
                    title=''
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
                    <Resource name='modules' {...modules} />
                    <Resource name='clr-object' {...clrObject} />
                </Admin>

                <Snackbar open={showErrorMessage} autoHideDuration={6000}
                          anchorOrigin={{vertical: 'top', horizontal: 'right'}} onClose={handleCloseErrorMessage}>
                    <MuiAlert elevation={6} variant="filled" severity="error" sx={{width: '100%'}}
                              onClose={handleCloseErrorMessage}>
                        {errorMessage}
                    </MuiAlert>
                </Snackbar>
            </>
        </AlertContext.Provider>
    );
}

const AppWrapper = () => (
    <StoreContextProvider value={store}>
        <App/>
    </StoreContextProvider>
);

export default AppWrapper;