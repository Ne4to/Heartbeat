import * as React from 'react';
import Box from '@mui/material/Box';

import {
    DashboardMenuItem,
    MenuItemLink,
    MenuProps,
    useSidebarState,
} from 'react-admin';

import heapDump from '../pages/heapDumpStat';
import segments from '../pages/segments';
import roots from '../pages/roots';
import modules from '../pages/modules';
import arraysGrid from '../pages/arraysGrid';
import sparseArraysStat from '../sparseArraysStat';
import stringsGrid from '../pages/stringsGrid';
import stringDuplicates from '../pages/stringDuplicates';

const Menu = ({ dense = false }: MenuProps) => {
    const [open] = useSidebarState();

    return (
        <Box
            sx={{
                width: open ? 200 : 50,
                marginTop: 1,
                marginBottom: 1,
                transition: theme =>
                    theme.transitions.create('width', {
                        easing: theme.transitions.easing.sharp,
                        duration: theme.transitions.duration.leavingScreen,
                    }),
            }}
        >
            <DashboardMenuItem />
            <MenuItemLink
                to="/heap-dump"
                state={{ _scrollToTop: true }}
                primaryText='Heap dump'
                leftIcon={<heapDump.icon />}
                dense={dense}
            />
            <MenuItemLink
                to="/segments"
                state={{ _scrollToTop: true }}
                primaryText='Segments'
                leftIcon={<segments.icon />}
                dense={dense}
            />
            <MenuItemLink
                to="/roots"
                state={{ _scrollToTop: true }}
                primaryText='Roots'
                leftIcon={<roots.icon />}
                dense={dense}
            />
            <MenuItemLink
                to="/modules"
                state={{ _scrollToTop: true }}
                primaryText='Modules'
                leftIcon={<modules.icon />}
                dense={dense}
            />
            <MenuItemLink
                to="/sparse-arrays-stat"
                state={{ _scrollToTop: true }}
                primaryText='Sparse arrays'
                leftIcon={<sparseArraysStat.icon />}
                dense={dense}
            />
            <MenuItemLink
                to="/arrays"
                state={{ _scrollToTop: true }}
                primaryText='Arrays'
                leftIcon={<arraysGrid.icon />}
                dense={dense}
            />
            <MenuItemLink
                to="/strings"
                state={{ _scrollToTop: true }}
                primaryText='Strings'
                leftIcon={<stringsGrid.icon />}
                dense={dense}
            />
            <MenuItemLink
                to="/string-duplicates"
                state={{ _scrollToTop: true }}
                primaryText='String duplicates'
                leftIcon={<stringDuplicates.icon />}
                dense={dense}
            />
        </Box>
    );
};

export default Menu;
