import React from 'react';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select, {SelectChangeEvent} from '@mui/material/Select';
import {Generation, GenerationObject} from '../client/models';
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

export type GenerationSelectProps = {
    generation?: Generation,
    onChange?: (generation?: Generation) => void
}

export const GenerationSelect = (props: GenerationSelectProps) => {
    const handleChange = (event: SelectChangeEvent) => {
        const isAny = event.target.value === ANY_ITEM_KEY
        if (isAny) {
            props.onChange?.(undefined)
        } else {
            const generation = event.target.value as Generation
            props.onChange?.(generation)
        }
    };

    return (
        <FormControl sx={{width: 200}} size="small">
            <InputLabel id="mode-select-label">Generation</InputLabel>
            <Select
                labelId="mode-select-label"
                id="mode-select"
                value={props.generation || ANY_ITEM_KEY}
                label="Generation"
                onChange={handleChange}
                fullWidth={true}
                MenuProps={MenuProps}
            >
                <MenuItem value={ANY_ITEM_KEY}>Any</MenuItem>
                <MenuItem value={GenerationObject.Generation0}>Generation0</MenuItem>
                <MenuItem value={GenerationObject.Generation1}>Generation1</MenuItem>
                <MenuItem value={GenerationObject.Generation2}>Generation2</MenuItem>
                <MenuItem value={GenerationObject.Large}>Large</MenuItem>
                <MenuItem value={GenerationObject.Pinned}>Pinned</MenuItem>
                <MenuItem value={GenerationObject.Frozen}>Frozen</MenuItem>
                <MenuItem value={GenerationObject.Unknown}>Unknown</MenuItem>
            </Select>
        </FormControl>
    );
}