import React from 'react';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import { TraversingHeapModes, TraversingHeapModesObject } from '../client/models';
import { FormControl } from '@mui/material';

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

export type TraversingHeapModeSelectProps = {
  mode: TraversingHeapModes,
  onChange?: (mode: TraversingHeapModes) => void
}

export const TraversingHeapModeSelect = (props: TraversingHeapModeSelectProps) => {
  const [mode, setMode] = React.useState<TraversingHeapModes>(props.mode)

  const handleChange = (event: SelectChangeEvent) => {
    const mode = event.target.value as TraversingHeapModes
    setMode(mode)
    props.onChange?.(mode)
  };

  return (
    <FormControl sx={{ width: 200 }} size="small">
      <InputLabel id="mode-select-label">Traversing heap mode</InputLabel>
      <Select
        labelId="mode-select-label"
        id="mode-select"
        value={mode}
        label="Traversing heap mode"
        onChange={handleChange}
        fullWidth={true}
        MenuProps={MenuProps}
      >
        <MenuItem value={TraversingHeapModesObject.Live}>Live</MenuItem>
        <MenuItem value={TraversingHeapModesObject.Dead}>Dead</MenuItem>
        <MenuItem value={TraversingHeapModesObject.All}>All</MenuItem>
      </Select>
    </FormControl>
  );
}