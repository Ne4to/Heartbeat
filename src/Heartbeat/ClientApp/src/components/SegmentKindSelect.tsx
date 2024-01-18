import React from 'react';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import { GCSegmentKind, GCSegmentKindObject } from '../client/models';
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

export type SegmentKindSelectProps = {
  kind?: GCSegmentKind,
  onChange?: (kind?: GCSegmentKind) => void
}

export const SegmentKindSelect = (props: SegmentKindSelectProps) => {
  const handleChange = (event: SelectChangeEvent) => {
    const isAny = event.target.value === ANY_ITEM_KEY
    if (isAny) {
      props.onChange?.(undefined)
    } else {
      const kind = event.target.value as GCSegmentKind
      props.onChange?.(kind)
    }
  };

  return (
    <FormControl sx={{ width: 200 }} size="small">
      <InputLabel id="mode-select-label">GC Segment kind</InputLabel>
      <Select
        labelId="mode-select-label"
        id="mode-select"
        value={props.kind || ANY_ITEM_KEY}
        label="GC Segment kind"
        onChange={handleChange}
        fullWidth={true}
        MenuProps={MenuProps}
      >
        <MenuItem value={ANY_ITEM_KEY}>Any</MenuItem>
        <MenuItem value={GCSegmentKindObject.Ephemeral}>Ephemeral</MenuItem>
        <MenuItem value={GCSegmentKindObject.Generation0}>Generation0</MenuItem>
        <MenuItem value={GCSegmentKindObject.Generation1}>Generation1</MenuItem>
        <MenuItem value={GCSegmentKindObject.Generation2}>Generation2</MenuItem>
        <MenuItem value={GCSegmentKindObject.Frozen}>Frozen</MenuItem>
        <MenuItem value={GCSegmentKindObject.Large}>Large</MenuItem>
        <MenuItem value={GCSegmentKindObject.Pinned}>Pinned</MenuItem>
      </Select>
    </FormControl>
  );
}