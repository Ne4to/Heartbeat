import {formatAddress, formatSize} from "../gridFormatter";
import {renderAddress, renderClrObjectAddress, renderMethodTable} from "../gridRenderCell";
import {GridColDef} from "@mui/x-data-grid";

export const methodTableColumn: GridColDef = {
    field: 'methodTable',
    headerName: 'MT',
    width: 160,
    align: 'right',
    type: 'number',
    valueFormatter: formatAddress,
    renderCell: renderMethodTable,
}

export const objectAddressColumn: GridColDef = {
    field: 'address',
    headerName: 'Address',
    width: 160,
    type: 'number',
    align: 'right',
    renderCell: renderClrObjectAddress
}

export const addressColumn: GridColDef = {
    field: 'address',
    headerName: 'Address',
    width: 160,
    type: 'number',
    align: 'right',
    renderCell: renderAddress
}

export const sizeColumn: GridColDef = {
    field: 'size',
    headerName: 'Size',
    width: 90,
    align: 'right',
    valueFormatter: formatSize
}