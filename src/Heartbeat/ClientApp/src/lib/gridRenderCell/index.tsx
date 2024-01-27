import {GridRenderCellParams, GridValueFormatterParams} from "@mui/x-data-grid";
import toHexAddress from "../toHexAddress";
import React from "react";
import {Button, Link} from "react-admin";
import {NavLink} from "react-router-dom";

export function renderAddress(params: GridRenderCellParams): React.ReactNode {
    const address = toHexAddress(params.value)
    return (
        <div className="monoFont">{address}</div>
    )
}

export function renderClrObjectAddress(params: GridRenderCellParams): React.ReactNode {
    return renderClrObjectLink(params.value);
}

export function renderClrObjectLink(address: number, label: string|undefined=undefined):React.ReactNode {
    const hexAddress = toHexAddress(address)
    return (
        <Button component={NavLink} to={`/clr-object/${hexAddress}/show`} label={label ?? hexAddress} />
    )
}

export function renderMethodTable(params: GridRenderCellParams): React.ReactNode {
    return renderMethodTableLink(params.value)
}

export function renderMethodTableLink(mt?: number):React.ReactNode {
    const hexMt = toHexAddress(mt)
    return (
        <Button component={NavLink} to={`/object-instances/${hexMt}/show`} label={hexMt} />
    )
}