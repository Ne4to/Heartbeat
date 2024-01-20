import {GridRenderCellParams, GridValueFormatterParams} from "@mui/x-data-grid";
import toHexAddress from "../toHexAddress";
import React from "react";

export function renderAddress(params: GridRenderCellParams): React.ReactNode {
    const address = toHexAddress(params.value)
    return (
        <div className="monoFont">{address}</div>
    )
}

export function renderClrObjectAddress(params: GridRenderCellParams): React.ReactNode {
    const address = toHexAddress(params.value)
    return (
        <a className="monoFont" href={'#/clr-object?address=' + address}>{address}</a>
    )
}

export function renderMethodTable(params: GridRenderCellParams): React.ReactNode {
    return renderMethodTableLink(params.value)
}

export function renderMethodTableLink(mt?: number):React.ReactNode {
    const address = toHexAddress(mt)
    return (
        <a className="monoFont" href={'#/object-instances?mt=' + address}>{address}</a>
    )
}