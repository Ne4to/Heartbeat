import React, { Component } from 'react';
import Box from '@mui/material/Box';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select, { SelectChangeEvent } from '@mui/material/Select';

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

export class TraversingHeapModeSelect extends Component<{}, {}> {
    constructor(props: {}) {
        super(props);
    }

    render() {
        return (
            <Box sx={{ width: 200 }}>
                <InputLabel id="mode-select-label">Traversing heap mode</InputLabel>
                <Select
                    labelId="mode-select-label"
                    id="mode-select"
                    // value={age}
                    label="Age"
                    // onChange={handleChange}
                    fullWidth={true}
                    // MenuProps={MenuProps}
                    >
                    <MenuItem value={10}>Live</MenuItem>
                    <MenuItem value={20}>Dead</MenuItem>
                    <MenuItem value={30}>All</MenuItem>
                </Select>
            </Box>
        );
    }
}