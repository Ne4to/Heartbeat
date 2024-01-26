import * as React from "react";
import { List, Datagrid, TextField, ReferenceField, EditButton } from 'react-admin';

export const ModuleList = () => (
    <List>
        <Datagrid>
            <TextField source="address" />
            <TextField source="size" />
            <TextField source="name" />
        </Datagrid>
    </List>
);