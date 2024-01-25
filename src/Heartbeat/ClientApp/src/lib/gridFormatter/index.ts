import { GridValueFormatterParams } from '@mui/x-data-grid';
import toHexAddress from '../toHexAddress'
import toSizeString from "../toSizeString";

export function formatAddress(params: GridValueFormatterParams<number>): string {
    if (params.value == null) {
        return '';
    }
    return toHexAddress(params.value);
}

export function formatSize(params: GridValueFormatterParams<number>): string {
    return toSizeString(params.value);
}

export function formatPercent(params: GridValueFormatterParams<number>): string {
    if (params.value == null) {
        return '';
    }
    return `${(params.value * 100).toFixed(2)}%`
}
