import {
    RaThemeOptions,
    defaultLightTheme,
    defaultDarkTheme,
    nanoDarkTheme,
    nanoLightTheme,
    radiantDarkTheme,
    radiantLightTheme,
    houseDarkTheme,
    houseLightTheme,
} from 'react-admin';

import { softDarkTheme, softLightTheme } from './softTheme';

export type ThemeName =
    | 'soft'
    | 'default'
    | 'nano'
    | 'radiant'
    | 'house';

export interface Theme {
    name: ThemeName;
    light: RaThemeOptions;
    dark?: RaThemeOptions;
}

const lightTheme = {
    ...softLightTheme,
    components: {
        ...softLightTheme.components,
        RaButton: {
            styleOverrides: {
                root: {
                    fontFamily: 'monospace',
                }
            }
        }
    }
}

export const themes: Theme[] = [
    { name: 'soft', light: lightTheme, dark: softDarkTheme },
    { name: 'default', light: defaultLightTheme, dark: defaultDarkTheme },
    { name: 'nano', light: nanoLightTheme, dark: nanoDarkTheme },
    { name: 'radiant', light: radiantLightTheme, dark: radiantDarkTheme },
    { name: 'house', light: houseLightTheme, dark: houseDarkTheme },
];
