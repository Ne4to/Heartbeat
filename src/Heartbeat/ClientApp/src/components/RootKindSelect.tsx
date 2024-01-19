import React from 'react';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import {ClrRootKind, ClrRootKindObject} from '../client/models';
import { FormControl } from '@mui/material';

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

export type RootKindSelectProps = {
  kind?: ClrRootKind,
  onChange?: (kind?: ClrRootKind) => void
}

export const RootKindSelect = (props: RootKindSelectProps) => {
  const handleChange = (event: SelectChangeEvent) => {
    const isAny = event.target.value === ANY_ITEM_KEY
    if (isAny) {
      props.onChange?.(undefined)
    } else {
      const kind = event.target.value as ClrRootKind
      props.onChange?.(kind)
    }
  };

  return (
    <FormControl sx={{ width: 200 }} size="small">
      <InputLabel id="mode-select-label">Root kind</InputLabel>
      <Select
        labelId="mode-select-label"
        id="mode-select"
        value={props.kind || ANY_ITEM_KEY}
        label="Root kind"
        onChange={handleChange}
        fullWidth={true}
        MenuProps={MenuProps}
      >
        <MenuItem value={ANY_ITEM_KEY}>Any</MenuItem>
        <MenuItem value={ClrRootKindObject.None}>None</MenuItem>
        <MenuItem value={ClrRootKindObject.FinalizerQueue}>FinalizerQueue</MenuItem>
        <MenuItem value={ClrRootKindObject.StrongHandle}>StrongHandle</MenuItem>
        <MenuItem value={ClrRootKindObject.PinnedHandle}>PinnedHandle</MenuItem>
        <MenuItem value={ClrRootKindObject.Stack}>Stack</MenuItem>
        <MenuItem value={ClrRootKindObject.RefCountedHandle}>RefCountedHandle</MenuItem>
        <MenuItem value={ClrRootKindObject.AsyncPinnedHandle}>AsyncPinnedHandle</MenuItem>
        <MenuItem value={ClrRootKindObject.SizedRefHandle}>SizedRefHandle</MenuItem>
      </Select>
    </FormControl>
  );
}