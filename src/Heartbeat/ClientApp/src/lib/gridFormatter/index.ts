import { GridValueFormatterParams } from '@mui/x-data-grid';
import toHexAddress from '../toHexAddress'
import prettyBytes from 'pretty-bytes';

export function formatAddress(params: GridValueFormatterParams<number>): string {
    if (params.value == null) {
        return '';
    }
    return toHexAddress(params.value);
}

export function formatSize(params: GridValueFormatterParams<number>): string {
    if (params.value == null) {
        return '';
    }
    return prettyBytes(params.value);
}