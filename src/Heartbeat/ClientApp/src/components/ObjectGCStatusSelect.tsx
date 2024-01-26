import React from 'react';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import {ObjectGCStatus, ObjectGCStatusObject} from '../client/models';
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

export type ObjectGCStatusSelectProps = {
  gcStatus?: ObjectGCStatus,
  onChange?: (gcStatus?: ObjectGCStatus) => void
}

export const ObjectGCStatusSelect = (props: ObjectGCStatusSelectProps) => {
  const handleChange = (event: SelectChangeEvent) => {
    const isAny = event.target.value === ANY_ITEM_KEY
    if (isAny) {
      props.onChange?.(undefined)
    } else {
      const gcStatus = event.target.value as ObjectGCStatus
      props.onChange?.(gcStatus)
    }
  };

  return (
    <FormControl sx={{ width: 200 }} size="small">
      <InputLabel id="mode-select-label">Object GC status</InputLabel>
      <Select
        labelId="mode-select-label"
        id="mode-select"
        value={props.gcStatus || ANY_ITEM_KEY}
        label="Object GC status"
        onChange={handleChange}
        fullWidth={true}
        MenuProps={MenuProps}
      >
        <MenuItem value={ObjectGCStatusObject.Live}>Live</MenuItem>
        <MenuItem value={ObjectGCStatusObject.Dead}>Dead</MenuItem>
        <MenuItem value={ANY_ITEM_KEY}>Any</MenuItem>
      </Select>
    </FormControl>
  );
}