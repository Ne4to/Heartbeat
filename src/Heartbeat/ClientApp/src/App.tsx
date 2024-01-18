import { Admin, Resource, localStorageStore, useStore, StoreContextProvider } from 'react-admin'
import { Layout } from './layout';
import { Home } from './home'
import headDump from './heapDumpStat'
import segments from './segments'
import objectInstances from './objectInstances'
import clrObject from './clrObject'
import modules from './modules'
import { themes, ThemeName } from './themes/themes';

const store = localStorageStore(undefined, 'Heartbeat');

const App = () => {
  const [themeName] = useStore<ThemeName>('themeName', 'soft');
  const lightTheme = themes.find(theme => theme.name === themeName)?.light;
  const darkTheme = themes.find(theme => theme.name === themeName)?.dark;

  return (
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
  );
}

const AppWrapper = () => (
  <StoreContextProvider value={store}>
    <App />
  </StoreContextProvider>
);

export default AppWrapper;