import React from 'react';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select, {SelectChangeEvent} from '@mui/material/Select';
import {Generation, GenerationObject, HttpRequestStatus, HttpRequestStatusObject} from '../client/models';
import {FormControl} from '@mui/material';

const ANY_ITEM_KEY = 'any'
const ITEM_HEIGHT = 48;
const ITEM_PADDING_TOP = 8;
const MenuProps = {
    PaperProps: {
        style: {
            maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP,
            width: 250,
        },
    },
};

export type HttpRequestStatusSelectProps = {
    status?: HttpRequestStatus,
    onChange?: (status?: HttpRequestStatus) => void
}

export const HttpRequestStatusSelect = (props: HttpRequestStatusSelectProps) => {
    const handleChange = (event: SelectChangeEvent) => {
        const isAny = event.target.value === ANY_ITEM_KEY
        if (isAny) {
            props.onChange?.(undefined)
        } else {
            const status = event.target.value as HttpRequestStatus
            props.onChange?.(status)
        }
    };

    return (
        <FormControl sx={{width: 200}} size="small">
            <InputLabel id="mode-select-label">Request status</InputLabel>
            <Select
                labelId="mode-select-label"
                id="mode-select"
                value={props.status || ANY_ITEM_KEY}
                label="Request status"
                onChange={handleChange}
                fullWidth={true}
                MenuProps={MenuProps}
            >
                <MenuItem value={ANY_ITEM_KEY}>Any</MenuItem>
                <MenuItem value={HttpRequestStatusObject.Pending}>Pending</MenuItem>
                <MenuItem value={HttpRequestStatusObject.Completed}>Completed</MenuItem>
            </Select>
        </FormControl>
    );
}