import {GridRenderCellParams, GridValueFormatterParams} from "@mui/x-data-grid";
import toHexAddress from "../toHexAddress";
import React from "react";

export function renderClrObjectAddress(params: GridRenderCellParams): React.ReactNode {
    const address = toHexAddress(params.value)
    return (
        <a href={'#/clr-object?address=' + address}>{address}</a>
    )
}